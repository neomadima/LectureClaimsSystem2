using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LecturerClaimsSystem2.Models
{
    public class Claim
    {
        public string Lecturer { get; set; }
        public DateTime Date { get; set; }
        public double Hours { get; set; }
        public double Rate { get; set; }
        public string Notes { get; set; }
        public string DocumentPath { get; set; }
        public string Status { get; set; }
        public string ApprovedBy { get; set; }

        public double Total
        {
            get { return Hours * Rate; }
        }

        public Claim()
        {
            Lecturer = string.Empty;
            Date = DateTime.Now;
            Hours = 0;
            Rate = 0;
            Notes = string.Empty;
            DocumentPath = string.Empty;
            Status = "Pending";
            ApprovedBy = string.Empty;
        }
    }

}
