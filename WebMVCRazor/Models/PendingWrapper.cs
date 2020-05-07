using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebMVCRazor.Models
{
    public class PendingWrapper
    {
       
        public string Patient { get; set; }
        public string Provider { get; set; }
        public string Location { get; set; }
        public int OverdueDays { get; set; }
        public string VisitType { get; set; }
        public string VisitDate { get; set; }
        public DateTime DueDate { get; set; }
    }
}