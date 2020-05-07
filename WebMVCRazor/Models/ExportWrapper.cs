using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebMVCRazor.Models
{
    public class ExportWrapper
    {
        public int PatientId { get; set; }
        public int LocationId { get; set; }
        public string Location { get; set; }
        public string MiddleName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DateofBirth { get; set; }
        public string AdmissionDate { get; set; }
        public string DischargeDate { get; set; }
        public string EligibileDate { get; set; }
        public string DeadlineDate { get; set; }
        public string isActive { get; set; }
        public string isNoteComplete { get; set; }
        public string MRRegVis { get; set; }
        public string PrAtRegVis { get; set; }
    }
}