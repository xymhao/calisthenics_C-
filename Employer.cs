#nullable enable
using System;
using System.Collections.Generic;
using System.Text;

namespace calisthenics
{
    public class Employer
    {
        public Employer(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public override bool Equals(object? obj)
        {
            return obj is Employer employer && employer.Equals(this);
        }

        public bool Equals(Employer obj)
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
