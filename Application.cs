using System;
using System.Collections.Generic;
using System.Linq;

namespace calisthenics
{
    public class Application
    {
        private const string JReq = "JReq";
        private const string ATS = "ATS";
        private readonly Dictionary<Employer, List<Job>> jobs = new Dictionary<Employer, List<Job>>();
        private readonly Dictionary<JobSeeker, List<Job>> saveJobs = new Dictionary<JobSeeker, List<Job>>();
        private readonly Dictionary<JobSeeker, List<Job>> appliedRecord = new Dictionary<JobSeeker, List<Job>>();
        private readonly List<Job> failedApplications = new List<Job>();

        public Application()
        {
        }

        public void Save(JobSeeker jobSeeker, Job job)
        {
            var saved = saveJobs.GetValueOrDefault(jobSeeker, new List<Job>());
            saved.Add(new Job(job.JobType, job.JobName));
            saveJobs.Add(jobSeeker, saved);
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
                failedApplications.Add(failJobApplicatin);
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
            var saved = appliedRecord.GetValueOrDefault(jobSeeker, new List<Job>());
            saved.Add(jobApplicatin);
            if (!appliedRecord.TryAdd(jobSeeker, saved))
            {
                appliedRecord[jobSeeker] = saved;
            }
        }

        public void Publish(Employer jobSeeker, Job job)
        {
            bool notExistType = !job.JobType.Equals(JReq) && !job.JobType.Equals(ATS);
            if (notExistType)
            {
                throw new NotSupportedJobTypeException();
            }

            List<Job> alreadyPublished = jobs.GetValueOrDefault(jobSeeker, new List<Job>());
            alreadyPublished.Add(job);
            if (!jobs.TryAdd(jobSeeker, alreadyPublished))
            {
                jobs[jobSeeker] = alreadyPublished;
            }
        }

        public List<Job> GetJobs(Employer employerName)
        {
            return jobs[employerName];
        }
        public List<Job> GetJobs(JobSeeker employerName, string type)
        {
            if (type== "published")
            {
                return saveJobs[employerName];
            }
            return appliedRecord[employerName];
        }

        public List<JobSeeker> AppliedJobNameAfterBeinDate(Job job, DateTime? beginDate)
        {
            List<JobSeeker> result = new List<JobSeeker>();
            foreach (var set in appliedRecord)
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


        public List<JobSeeker> AppliedJobNameAndBeforeEndDate(Job job, DateTime? endDate)
        {
            List<JobSeeker> result = new List<JobSeeker>();
            foreach (var set in appliedRecord)
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


        public List<JobSeeker> AppliedDateTimeRangeNew(DateTime? beginDate, DateTime? endDate)
        {
            List<JobSeeker> result = new List<JobSeeker>();
            foreach (var set in appliedRecord)
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

        public List<JobSeeker> AppliedBeforeEndDateNew(DateTime? to)
        {
            List<JobSeeker> result = new List<JobSeeker>();
            foreach (var set in appliedRecord)
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

        public List<JobSeeker> AppliedAfterBeginDate(DateTime? @from)
        {
            List<JobSeeker> result = new List<JobSeeker>() { };
            foreach (var set in appliedRecord)
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

        public List<JobSeeker> AppliedJobName(Job job)
        {
            List<JobSeeker> result = new List<JobSeeker>();
            foreach (var set in appliedRecord)
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

        public string ExportHtml(DateTime date)
        {
            string content = "";
            foreach (var set in appliedRecord)
            {
                var applicant = set.Key;
                var jobs1 = set.Value;
                var appliedOnDate = jobs1.Where(x => x.ApplicationTime.Equals(date.ToString("yyyy-MM-dd"))).ToList();

                foreach (var job in appliedOnDate)
                {
                    content += ("<tr>" + "<td>" + job.Employer.Name + "</td>" + "<td>" + job.JobName + "</td>" + "<td>" + job.JobType +
                                "</td>" + "<td>" + applicant.Name + "</td>" + "<td>" + job.ApplicationTime + "</td>" + "</tr>");
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


        public string ExportCsv(DateTime date)
        {
            string result = "Employer,Job,Job Type,Applicants,Date" + "\n";

            foreach (var set in appliedRecord)
            {
                var applicant = set.Key;
                var jobs1 = set.Value;
                var appliedOnDate = jobs1.Where(x => x.ApplicationTime.Equals(date.ToString("yyyy-MM-dd"))).ToList();
                foreach (var job in appliedOnDate)
                {
                    result += (job.Employer.Name + "," + job.JobName + "," + job.JobType + "," + applicant.Name + "," + job.ApplicationTime + "\n");
                }
            }

            return result;
        }


        public int GetSuccessfulApplications(Employer employer, Job job)
        {
            int result = 0;
            foreach (var set in appliedRecord)
            {
                var jobs = set.Value;
                result += jobs.Any(x => x.Employer.Equals(employer) && x.Equals(job)) ? 1 : 0;
            }
            return result;
        }

        public int GetUnsuccessfulApplications(Employer employer, Job job)
        {
            return failedApplications.Count(x => x.Equals(job) && x.Employer.Equals(employer));
        }

        public List<JobSeeker> AppliedJobNameAndDateRange(Job job, DateTime beginTime, DateTime endDate)
        {
            List<JobSeeker> result = new List<JobSeeker>();
            foreach (var set in appliedRecord)
            {
                var applicant = set.Key;
                List<Job> value = set.Value;
                bool isAppliedThisDate = value.Any(x => x.Equals(job) && Convert.ToDateTime(x.ApplicationTime) < endDate 
                                                                      && Convert.ToDateTime(x.ApplicationTime) > beginTime);
                if (isAppliedThisDate)
                {
                    result.Add(applicant);
                }
            }

            return result;
        }
    }
}
