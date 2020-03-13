using System;
using System.Collections.Generic;
using System.Text;

namespace calisthenics
{
    public class JobSeeker
    {
        public JobSeeker(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public override bool Equals(object? obj)
        {
            return obj is JobSeeker jobSeeker && jobSeeker.Equals(this);
        }

        public bool Equals(JobSeeker obj)
        {
            if (obj == null)
            {
                return false;
            }

            return obj.Name.Equals(Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
