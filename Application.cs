using System;
using System.Collections.Generic;
using System.Linq;

namespace calisthenics
{
    public class Application
    {
        private const string JReq = "JReq";
        private const string ATS = "ATS";
        private const string _Save = "save";
        private string publish = "publish";
        private const string Apply = "apply";
        private readonly Dictionary<string, List<List<string>>> jobs = new Dictionary<string, List<List<string>>>();
        private readonly Dictionary<Employer, List<Job>> jobs1 = new Dictionary<Employer, List<Job>>();
        private readonly Dictionary<JobSeeker, List<Job>> saveJobs = new Dictionary<JobSeeker, List<Job>>();

        private readonly Dictionary<string, List<List<string>>> appliedRecord = new Dictionary<string, List<List<string>>>();
        private readonly Dictionary<JobSeeker, List<Job>> appliedRecord1 = new Dictionary<JobSeeker, List<Job>>();

        private readonly List<List<string>> failedApplications = new List<List<string>>();
        private readonly List<Job> failedApplications1 = new List<Job>();

        public Application()
        {
        }

        public void Execute(string command, string employerName, string jobName, string jobType, string jobSeekerName,
            string resumeApplicantName, DateTime? applicationTime)
        {
            if (command == publish)
            {
                Publish(employerName, jobName, jobType);
            }

            if (command == _Save)
            {
                Save(employerName, jobName, jobType);
            }

            if (command == Apply)
            {
                ApplyJob(employerName, jobName, jobType, jobSeekerName, resumeApplicantName, applicationTime);
            }
        }

        private void Save(string employerName, string jobName, string jobType)
        {
            List<List<string>> saved = jobs.GetValueOrDefault(employerName, new List<List<string>>());
            saved.Add(new List<string>() {jobName, jobType});
            jobs.Add(employerName, saved);
        }
        public void Save(JobSeeker jobSeeker, Job job)
        {
            var saved = saveJobs.GetValueOrDefault(jobSeeker, new List<Job>());
            saved.Add(new Job(job.JobType, job.JobName));
            saveJobs.Add(jobSeeker, saved);
        }

        public void Execute(string command, Employer employerName, Job job, JobSeeker jobSeekerName,
            JobSeeker resumeApplicantName, DateTime? applicationTime)
        {
            if (command == publish)
            {
                Publish(employerName, job);
            }

            if (command == _Save)
            {
                var saved = jobs1.GetValueOrDefault(employerName, new List<Job>());
                saved.Add(job);
                jobs1.Add(employerName, saved);
            }

            if (command == Apply)
            {
                ApplyJob(employerName, job, jobSeekerName, resumeApplicantName, applicationTime);
            }
        }


        private void ApplyJob(string employerName, string jobName, string jobType, string jobSeekerName,
            string resumeApplicantName, DateTime? applicationTime)
        {
            if (jobType.Equals(JReq) && resumeApplicantName == null)
            {
                List<string> failedApplication = new List<string>()
                    {jobName, jobType, applicationTime?.ToString("yyyy-MM-dd"), employerName};
                failedApplications.Add(failedApplication);
                throw new RequiresResumeForJReqJobException();
            }

            if (jobType.Equals(JReq) && !resumeApplicantName.Equals(jobSeekerName))
            {
                throw new InvalidResumeException();
            }

            List<List<string>> saved = appliedRecord.GetValueOrDefault(jobSeekerName, new List<List<string>>());
            saved.Add(new List<string>() { jobName, jobType, applicationTime?.ToString("yyyy-MM-dd"), employerName });
            if (!appliedRecord.TryAdd(jobSeekerName, saved))
            {
                appliedRecord[jobSeekerName] = saved;
            }
        }


        public void ApplyJob(Employer employer, Job job, JobSeeker jobSeeker,
            JobSeeker resumeApplicantName, DateTime? applicationTime)
        {
            if (job.JobType.Equals(JReq) && resumeApplicantName == null)
            {
                var failJobApplicatin = new Job()
                {
                    JobName = job.JobName,
                    JobType = job.JobType,
                    ApplicationTime = applicationTime?.ToString("yyyy-MM-dd"),
                    Employer = employer
                };
                failedApplications1.Add(failJobApplicatin);
                throw new RequiresResumeForJReqJobException();
            }

            if (job.JobType.Equals(JReq) && !resumeApplicantName.Equals(jobSeeker))
            {
                throw new InvalidResumeException();
            }
            var jobApplicatin = new Job()
            {
                JobName = job.JobName,
                JobType = job.JobType,
                ApplicationTime = applicationTime?.ToString("yyyy-MM-dd"),
                Employer = employer
            };
            var saved = appliedRecord1.GetValueOrDefault(jobSeeker, new List<Job>());
            saved.Add(jobApplicatin);
            if (!appliedRecord1.TryAdd(jobSeeker, saved))
            {
                appliedRecord1[jobSeeker] = saved;
            }
        }

        private void Publish(string employerName, string jobName, string jobType)
        {
            bool notExistType = !jobType.Equals(JReq) && !jobType.Equals(ATS);
            if (notExistType)
            {
                throw new NotSupportedJobTypeException();
            }

            List<List<string>> alreadyPublished = jobs.GetValueOrDefault(employerName, new List<List<string>>());
            alreadyPublished.Add(new List<string>() { jobName, jobType });
            if (!jobs.TryAdd(employerName, alreadyPublished))
            {
                jobs[employerName] = alreadyPublished;
            }
        }

        public void Publish(Employer jobSeeker, Job job)
        {
            bool notExistType = !job.JobType.Equals(JReq) && !job.JobType.Equals(ATS);
            if (notExistType)
            {
                throw new NotSupportedJobTypeException();
            }

            List<Job> alreadyPublished = jobs1.GetValueOrDefault(jobSeeker, new List<Job>());
            alreadyPublished.Add(job);
            if (!jobs1.TryAdd(jobSeeker, alreadyPublished))
            {
                jobs1[jobSeeker] = alreadyPublished;
            }
        }

        public List<List<string>> GetJobs(string employerName, string type)
        {
            if (type.Equals("applied"))
            {
                return appliedRecord[employerName];
            }

            return jobs[employerName];
        }

        public List<Job> GetJobs(Employer employerName)
        {
            return jobs1[employerName];
        }
        public List<Job> GetJobs(JobSeeker employerName, string type)
        {
            if (type== "published")
            {
                return saveJobs[employerName];
            }
            return appliedRecord1[employerName];
        }

        public List<string> FindApplicants(string jobName, string employerName)
        {
            return FindApplicants(jobName, employerName, null);
        }

        public List<string> FindApplicants(string jobName, string employerName, DateTime? beginDate)
        {
            return FindApplicants(jobName, employerName, beginDate, null);
        }

        public List<string> FindApplicants(string jobName, string employerName, DateTime? beginDate, DateTime? endDate)
        {
            bool appliedJobName = beginDate == null && endDate == null;
            if (appliedJobName)
            {
                return AppliedJobName(jobName);
            }

            if (jobName == null && endDate == null)
            {
                return AppliedAfterBeginDate1(beginDate);
            }

            if (jobName == null && beginDate == null)
            {
                return AppliedBeforeEndDate(endDate);
            }

            if (jobName == null)
            {
                return AppliedDateTimeRange(beginDate, endDate);
            }

            if (endDate != null)
            {
                return AppliedJobNameAndBeforeEndDate(jobName, endDate);
            }

            return AppliedJobNameAfterBeinDate(jobName, beginDate);
        }

        private List<string> AppliedJobNameAfterBeinDate(string jobName, DateTime? beginDate)
        {
            List<string> result = new List<string>();
            foreach (var set in appliedRecord)
            {
                string applicant = set.Key;
                List<List<string>> value = set.Value;
                bool isAppliedThisDate = value.Any(x => x[0].Equals(jobName) && Convert.ToDateTime(x[2]) >= beginDate);
                if (isAppliedThisDate)
                {
                    result.Add(applicant);
                }
            }

            return result;
        }

        public List<JobSeeker> AppliedJobNameAfterBeinDate(Job job, DateTime? beginDate)
        {
            List<JobSeeker> result = new List<JobSeeker>();
            foreach (var set in appliedRecord1)
            {
                var applicant = set.Key;
                List<Job> value = set.Value;
                bool isAppliedThisDate = value.Any(x => x.Equals(job) && Convert.ToDateTime(x.ApplicationTime) >= beginDate);
                if (isAppliedThisDate)
                {
                    result.Add(applicant);
                }
            }

            return result;
        }

        public List<string> AppliedJobNameAndBeforeEndDate(string jobName, DateTime? endDate)
        {
            List<string> result = new List<string>();
            foreach (var set in appliedRecord)
            {
                string applicant = set.Key;
                List<List<string>> value = set.Value;
                bool isAppliedThisDate = value.Any(x => x[0].Equals(jobName) && Convert.ToDateTime(x[2]) < endDate);
                if (isAppliedThisDate)
                {
                    result.Add(applicant);
                }
            }

            return result;
        }


        public List<JobSeeker> AppliedJobNameAndBeforeEndDate(Job job, DateTime? endDate)
        {
            List<JobSeeker> result = new List<JobSeeker>();
            foreach (var set in appliedRecord1)
            {
                var applicant = set.Key;
                List<Job> value = set.Value;
                bool isAppliedThisDate = value.Any(x => x.Equals(job) && Convert.ToDateTime(x.ApplicationTime) < endDate);
                if (isAppliedThisDate)
                {
                    result.Add(applicant);
                }
            }

            return result;
        }


        private List<string> AppliedDateTimeRange(DateTime? beginDate, DateTime? endDate)
        {
            List<string> result = new List<string>();
            foreach (var set in appliedRecord)
            {
                string applicant = set.Key;
                List<List<string>> jobs = set.Value;
                bool isAppliedThisDate =
                    jobs.Any(x => beginDate <= Convert.ToDateTime(x[2]) && Convert.ToDateTime(x[2]) <= endDate);
                if (isAppliedThisDate)
                {
                    result.Add(applicant);
                }
            }

            return result;
        }

        public List<JobSeeker> AppliedDateTimeRangeNew(DateTime? beginDate, DateTime? endDate)
        {
            List<JobSeeker> result = new List<JobSeeker>();
            foreach (var set in appliedRecord1)
            {
                var applicant = set.Key;
                List<Job> jobs = set.Value;
                bool isAppliedThisDate =
                    jobs.Any(x => beginDate <= Convert.ToDateTime(x.ApplicationTime) && Convert.ToDateTime(x.ApplicationTime) <= endDate);
                if (isAppliedThisDate)
                {
                    result.Add(applicant);
                }
            }

            return result;
        }

        private List<string> AppliedBeforeEndDate(DateTime? to)
        {
            List<string> result = new List<string>();
            foreach (var set in appliedRecord)
            {
                string applicant = set.Key;
                List<List<string>> jobs = set.Value;
                bool isAppliedThisDate = jobs.Any(x => Convert.ToDateTime(x[2]) < to);
                if (isAppliedThisDate)
                {
                    result.Add(applicant);
                }
            }

            return result;
        }

        public List<JobSeeker> AppliedBeforeEndDateNew(DateTime? to)
        {
            List<JobSeeker> result = new List<JobSeeker>();
            foreach (var set in appliedRecord1)
            {
                var applicant = set.Key;
                List<Job> jobs = set.Value;
                bool isAppliedThisDate = jobs.Any(x => Convert.ToDateTime(x.ApplicationTime) < to);
                if (isAppliedThisDate)
                {
                    result.Add(applicant);
                }
            }

            return result;
        }

        private List<string> AppliedAfterBeginDate1(DateTime? @from)
        {
            List<string> result = new List<string>() { };
            foreach (var set in appliedRecord)
            {
                string applicant = set.Key;
                List<List<string>> jobs = set.Value;
                bool isAppliedThisDate = jobs.Any(x => Convert.ToDateTime(x[2]) >= @from);
                if (isAppliedThisDate)
                {
                    result.Add(applicant);
                }
            }

            return result;
        }

        public List<JobSeeker> AppliedAfterBeginDate(DateTime? @from)
        {
            List<JobSeeker> result = new List<JobSeeker>() { };
            foreach (var set in appliedRecord1)
            {
                var applicant = set.Key;
                List<Job> jobs = set.Value;
                bool isAppliedThisDate = jobs.Any(x => Convert.ToDateTime(x.ApplicationTime) >= @from);
                if (isAppliedThisDate)
                {
                    result.Add(applicant);
                }
            }

            return result;
        }


        private List<string> AppliedJobName(string jobName)
        {
            List<string> result = new List<string>();
            foreach (var set in appliedRecord)
            {
                string applicant = set.Key;
                List<List<string>> jobs = set.Value;
                bool hasAppliedToThisJob = jobs.Any(x => x[0].Equals(jobName));
                if (hasAppliedToThisJob)
                {
                    result.Add(applicant);
                }
            }

            return result;
        }

        public List<JobSeeker> AppliedJobName(Job job)
        {
            List<JobSeeker> result = new List<JobSeeker>();
            foreach (var set in appliedRecord1)
            {
                var applicant = set.Key;
                var jobs = set.Value;
                bool hasAppliedToThisJob = jobs.Any(x => x.Equals(job));
                if (hasAppliedToThisJob)
                {
                    result.Add(applicant);
                }
            }

            return result;
        }

        public string Export(string type, DateTime date)
        {
            if (type == "csv")
            {
                return ExportCsv(date);
            }

            return ExportHtml(date);
        }

        private string ExportHtml(DateTime date)
        {
            string content = "";
            foreach (var set in appliedRecord)
            {
                string applicant = set.Key;
                List<List<string>> jobs1 = set.Value;
                List<List<string>> appliedOnDate = jobs1.Where(x => x[2].Equals(date.ToString("yyyy-MM-dd"))).ToList();

                foreach (List<string> job in appliedOnDate)
                {
                    content += ("<tr>" + "<td>" + job[3] + "</td>" + "<td>" + job[0] + "</td>" + "<td>" + job[1] +
                                "</td>" + "<td>" + applicant + "</td>" + "<td>" + job[2] + "</td>" + "</tr>");
                }
            }

            return "<!DOCTYPE html>"
                   + "<body>"
                   + "<table>"
                   + "<thead>"
                   + "<tr>"
                   + "<th>Employer</th>"
                   + "<th>Job</th>"
                   + "<th>Job Type</th>"
                   + "<th>Applicants</th>"
                   + "<th>Date</th>"
                   + "</tr>"
                   + "</thead>"
                   + "<tbody>"
                   + content
                   + "</tbody>"
                   + "</table>"
                   + "</body>"
                   + "</html>";
        }

        private string ExportCsv(DateTime date)
        {
            string result = "Employer,Job,Job Type,Applicants,Date" + "\n";

            foreach (var set in appliedRecord)
            {
                string applicant = set.Key;
                List<List<string>> jobs = set.Value;
                List<List<string>> appliedOnDate = jobs.Where(x => x[2].Equals(date.ToString("yyyy-MM-dd"))).ToList();
                foreach (List<string> job in appliedOnDate)
                {
                    result += (job[3] + "," + job[0] + "," + job[1] + "," + applicant + "," + job[2] + "\n");
                }
            }

            return result;
        }

        public int GetSuccessfulApplications(string employerName, string jobName)
        {
            int result = 0;
            foreach (var set in appliedRecord)
            {
                List<List<string>> jobs = set.Value;
                result += jobs.Any(x => x[3].Equals(employerName) && x[0].Equals(jobName)) ? 1 : 0;
            }
            return result;
        }

        public int GetUnsuccessfulApplications(string employerName, string jobName)
        {
            return failedApplications.Count(x => x[0].Equals(jobName) && x[3].Equals(employerName));
        }
    }
}
