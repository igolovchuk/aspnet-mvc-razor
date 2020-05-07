using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebMVCRazor.Models
{
    public class VisitWrapper
    {
        public int VisitId { get; set; }
        public int PatientId { get; set; }
        public string VisitType { get; set; }
        public string ProviderId { get; set; }
        public string ProviderName { get; set; }
        public string PatientName { get; set; }
        public string VisitDate { get; set; }
        public bool IsNoteComplete { get; set; }
    }
}