using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages.Html;

namespace WebMVCRazor.Models
{
    public class PatientWrapper
    {
        public int PatientId { get; set; }
        public int LocationId { get; set; }
        public string Location { get; set; }
        public string MiddleName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsActive { get; set; }
        public string DateofBirth { get; set; }
        public string AdmissionDate { get; set; }
        public string DischargeDate { get; set; }
        public string EligibileDate { get; set; }
        public string DeadlineDate { get; set; }
       
    }
}