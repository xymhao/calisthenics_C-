using System;
using System.Collections.Generic;
using System.Text;

namespace calisthenics
{
    public class JobApplication
    {
        public Job Job { get; set; }

        public DateTime ApplicatinDateTime { get; set; }

        public Employer Employer { get; set; }
    }
}
