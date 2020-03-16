using System;
using System.Collections.Generic;
using System.Text;

namespace calisthenics
{
    public class Job
    {
        public Job()
        {
        }
        public Job(string jobType, string jobName)
        {
            JobType = jobType;
            JobName = jobName;
            Employer = new Employer("");
            ApplicationTime = DateTime.Now.ToString("yyy-MM-dd");
        }

        public Job(string jobType, string jobName, Employer employer)
        {
            JobType = jobType;
            JobName = jobName;
            Employer = employer;
            ApplicationTime = DateTime.Now.ToString("yyy-MM-dd");
        }

        public Job(string jobType, string jobName, Employer employer, string applicationTime)
        {
            JobType = jobType;
            JobName = jobName;
            Employer = employer;
            ApplicationTime = applicationTime;
        }

        public string JobType { get; set; }

        public string JobName { get; set; }

        public Employer Employer { get; set; }

        public string ApplicationTime { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is Job jobSeeker && jobSeeker.Equals(this);
        }

        public bool Equals(Job obj)
        {
            if (obj == null)
            {
                return false;
            }

            return obj.JobName.Equals(JobName) && obj.JobType.Equals(JobType)
                                               && obj.Employer.Equals(Employer);
        }

        public override int GetHashCode()
        {
            return (JobName + JobType + Employer).GetHashCode();
        }
    }
}
