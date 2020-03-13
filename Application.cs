using System;
using System.Collections.Generic;
using System.Linq;

namespace calisthenics
{
    public class Application
    {
        private const string Value = "JReq";
        private const string ATS = "ATS";
        private const string Save = "save";
        private string publish = "publish";
        private const string Apply = "apply";
        private readonly Dictionary<string, List<List<string>>> jobs = new Dictionary<string, List<List<string>>>();
        private readonly Dictionary<string, List<List<string>>> appliedRecord = new Dictionary<string, List<List<string>>>();
        private readonly List<List<string>> failedApplications = new List<List<string>>();

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
            
            if (command == Save)
            {
                List<List<string>> saved = jobs.GetValueOrDefault(employerName, new List<List<string>>());
                saved.Add(new List<string>() { jobName, jobType });
                jobs.Add(employerName, saved);
            }

            if (command == Apply)
            {
                ApplyJob(employerName, jobName, jobType, jobSeekerName, resumeApplicantName, applicationTime);
            }
        }

        private void ApplyJob(string employerName, string jobName, string jobType, string jobSeekerName,
            string resumeApplicantName, DateTime? applicationTime)
        {
            if (jobType.Equals(Value) && resumeApplicantName == null)
            {
                List<string> failedApplication = new List<string>()
                    {jobName, jobType, applicationTime?.ToString("yyyy-MM-dd"), employerName};
                failedApplications.Add(failedApplication);
                throw new RequiresResumeForJReqJobException();
            }

            if (jobType.Equals(Value) && !resumeApplicantName.Equals(jobSeekerName))
            {
                throw new InvalidResumeException();
            }

            List<List<string>> saved = appliedRecord.GetValueOrDefault(jobSeekerName, new List<List<string>>());
            saved.Add(new List<string>() {jobName, jobType, applicationTime?.ToString("yyyy-MM-dd"), employerName});
            if (!appliedRecord.TryAdd(jobSeekerName, saved))
            {
                appliedRecord[jobSeekerName] = saved;
            }
        }

        private void Publish(string employerName, string jobName, string jobType)
        {
            bool notExistType = !jobType.Equals(Value) && !jobType.Equals(ATS);
            if (notExistType)
            {
                throw new NotSupportedJobTypeException();
            }

            List<List<string>> alreadyPublished = jobs.GetValueOrDefault(employerName, new List<List<string>>());
            alreadyPublished.Add(new List<string>() {jobName, jobType});
            if (!jobs.TryAdd(employerName, alreadyPublished))
            {
                jobs[employerName] = alreadyPublished;
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
                return AppliedAfterBeginDate(beginDate);
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

        private List<string> AppliedJobNameAndBeforeEndDate(string jobName, DateTime? endDate)
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

        private List<string> AppliedAfterBeginDate(DateTime? @from)
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
                result += jobs.Any(x=>x[3].Equals(employerName) && x[0].Equals(jobName))? 1:0;
            }
            return result;
        }

        public int GetUnsuccessfulApplications(string employerName, string jobName)
        {
            return failedApplications.Count(x => x[0].Equals(jobName) && x[3].Equals(employerName));
        }
    }
}
