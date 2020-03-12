using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace calisthenics
{
    public class Application
    {
        private readonly Dictionary<string, List<List<string>>> jobs = new Dictionary<string, List<List<string>>>();
        private readonly Dictionary<string, List<List<string>>> applied = new Dictionary<string, List<List<string>>>();
        private readonly List<List<string>> failedApplications = new List<List<string>>();

        public void Execute(string command, string employerName, string jobName, string jobType, string jobSeekerName,
            string resumeApplicantName, DateTime? applicationTime)
        {
            if (command == "publish")
            {
                if (!jobType.Equals("JReq") && !jobType.Equals("ATS"))
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
            else if (command == "save")
            {
                List<List<string>> saved = jobs.GetValueOrDefault(employerName, new List<List<string>>());

                saved.Add(new List<string>() { jobName, jobType });
                jobs.Add(employerName, saved);
            }
            else if (command == "apply")
            {
                if (jobType.Equals("JReq") && resumeApplicantName == null)
                {
                    List<string> failedApplication = new List<string>()
                        {jobName, jobType, applicationTime?.ToString("yyyy-MM-dd"), employerName};
                    failedApplications.Add(failedApplication);
                    throw new RequiresResumeForJReqJobException();
                }

                if (jobType.Equals("JReq") && !resumeApplicantName.Equals(jobSeekerName))
                {
                    throw new InvalidResumeException();
                }

                List<List<string>> saved = this.applied.GetValueOrDefault(jobSeekerName, new List<List<string>>());
                saved.Add(new List<string>() { jobName, jobType, applicationTime?.ToString("yyyy-MM-dd"), employerName });
                if (!applied.TryAdd(jobSeekerName, saved))
                {
                    applied[jobSeekerName] = saved;
                }
            }
        }

        public List<List<string>> getJobs(string employerName, string type)
        {
            if (type.Equals("applied"))
            {
                return applied[employerName];
            }

            return jobs[employerName];
        }

        public List<string> findApplicants(string jobName, string employerName)
        {
            return findApplicants(jobName, employerName, null);
        }

        public List<string> findApplicants(string jobName, string employerName, DateTime? from)
        {
            return findApplicants(jobName, employerName, from, null);
        }

        public List<string> findApplicants(string jobName, string employerName, DateTime? from, DateTime? to)
        {
            if (from == null && to == null)
            {
                List<string> result = new List<string>() { };
                foreach (var set in applied)
                {
                    string applicant = set.Key;
                    List<List<string>> jobs = set.Value;
                    bool hasAppliedToThisJob = jobs.Any(x=>x[0].Equals(jobName));
                    if (hasAppliedToThisJob)
                    {
                        result.Add(applicant);
                    }
                }
                return result;
            }
            else if (jobName == null && to == null)
            {
                List<string> result = new List<string>() { };
                foreach (var set in applied)
                {
                    string applicant = set.Key;
                    List<List<string>> jobs = set.Value;
                    bool isAppliedThisDate = jobs.Any(x=>Convert.ToDateTime(x[2]) >= from );
                    if (isAppliedThisDate)
                    {
                        result.Add(applicant);
                    }
                }
                return result;
            }
            else if (jobName == null && from == null)
            {
                List<string> result = new List<string>() { };
                foreach (var set in applied)
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
            else if (jobName == null)
            {
                List<string> result = new List<string>() { };
                foreach (var set in applied)
                {
                    string applicant = set.Key;
                    List<List<string>> jobs = set.Value;
                    bool isAppliedThisDate = jobs.Any(x =>from <= Convert.ToDateTime(x[2]) && Convert.ToDateTime(x[2]) <= to);
                    if (isAppliedThisDate)
                    {
                        result.Add(applicant);
                    }
                }
                return result;
            }
            else if (to != null)
            {
                List<string> result = new List<string>() { };
                foreach (var set in applied)
                {
                    string applicant = set.Key;
                    List<List<string>> jobs = set.Value;
                    bool isAppliedThisDate = jobs.Any(x => x[0].Equals(jobName) && Convert.ToDateTime(x[2]) < to);
                    if (isAppliedThisDate)
                    {
                        result.Add(applicant);
                    }
                }
                return result;
            }
            else
            {

                List<string> result = new List<string>() { };
                foreach (var set in applied)
                {
                    string applicant = set.Key;
                    List<List<string>> jobs = set.Value;
                    bool isAppliedThisDate = jobs.Any(x => x[0].Equals(jobName) && Convert.ToDateTime(x[2]) >= from);
                    if (isAppliedThisDate)
                    {
                        result.Add(applicant);
                    }
                }
                return result;
            }
        }

        public string Export(string type, DateTime date)
        {
            if (type == "csv")
            {
                string result = "Employer,Job,Job Type,Applicants,Date" + "\n";

                foreach (var set in applied)
                {
                    string applicant = set.Key;
                    List<List<string>> jobs = set.Value;
                    List<List<string>> appliedOnDate = jobs.Where(x=>x[2].Equals(date.ToString("yyyy-MM-dd"))).ToList();
                    foreach (List<string> job in appliedOnDate)
                    {
                        result += (job[3] + "," + job[0] + "," + job[1] + "," + applicant + "," + job[2] + "\n");
                    }
                }

                return result;
            }
            else
            {
                string content = "";
                foreach (var set in applied)
                {
                    string applicant = set.Key;
                    List<List<string>> jobs1 = set.Value;
                    List<List<string>> appliedOnDate = jobs1.Where(x=>x[2].Equals(date.ToString("yyyy-MM-dd"))).ToList();

                    foreach (List<string> job in appliedOnDate)
                    {
                        content = content + ("<tr>" + "<td>" + job[3] + "</td>" + "<td>" + job[0] + "</td>" + "<td>" + job[1] + "</td>" + "<td>" + applicant + "</td>" + "<td>" + job[2] + "</td>" + "</tr>");
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
        }

        public int GetSuccessfulApplications(string employerName, string jobName)
        {
            int result = 0;
            foreach (var set in applied)
            {
                List<List<string>> jobs = set.Value;

                result += jobs.Any(x=>x[3].Equals(employerName) && x[0].Equals(jobName))? 1:0;
            }
            return result;
        }

        public int GetUnsuccessfulApplications(string employerName, string jobName)
        {
            return (int)failedApplications.Count(x => x[0].Equals(jobName) && x[3].Equals(employerName));
        }
    }
}
