using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebMVCRazor.App_Start;
using WebMVCRazor.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

namespace WebMVCRazor.Controllers
{
    public class PendingController : Controller
    {
         private ApplicationUserManager _userManager;

        public PendingController()
        {
        }

        public PendingController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ApplicationUserManager UserManager
        {
            get { return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
            private set { _userManager = value; }
        }

        //PENDING
        public ActionResult PendingNotes(string Facilities, string Providers, string Patients)
        {
            using (var db = new ApplicationDbContext())
            {
                var visits = db.Visits.Where(visit => visit.IsNoteComplete == false).OrderBy(item => item.VisitDate).ToList();
                var overdueVisitList = new List<PendingWrapper>();

                //======================Search area========================================================================================//

                var facilityList = db.Facilities.Where(fsc => fsc.IsActive).OrderBy(item => item.Name).Select(fsc => fsc.Name).ToList();
                var listofFacilities = new SelectList(facilityList);


                var providerList = db.Providers.Where(prov => prov.User.IsActive).OrderBy(item => item.User.UserName).Select(prov => prov.User.UserName).ToList();
                var listofProviders = new SelectList(providerList);

                var patientList = db.Patients.Where(patient => patient.IsActive).OrderBy(item => item.LastName).Select(patient => patient.LastName).ToList();
                var listofPatients = new SelectList(patientList);


                if (!String.IsNullOrEmpty(Facilities))
                {
                    visits = visits.Where(c => c.Patient.Facility.Name.Contains(Facilities)).ToList();
                }
                if (!String.IsNullOrEmpty(Providers))
                {
                    visits = visits.Where(c => c.Provider.User.UserName.Contains(Providers)).ToList();
                }
                if (!String.IsNullOrEmpty(Patients))
                {
                    visits = visits.Where(c => c.Patient.LastName.Contains(Patients)).ToList();
                }

                //============================================================================================================================//

                foreach (var visit in visits)
                {
                    overdueVisitList.Add(new PendingWrapper
                    {
                        Patient = visit.Patient.FirstName + " " + visit.Patient.MiddleName + " " + visit.Patient.LastName,
                        Provider = visit.Provider.User.FirstName + " " + visit.Provider.User.MiddleName + " " + visit.Provider.User.LastName,
                        Location = visit.Patient.Facility.Name,
                        OverdueDays = (DateTime.Now - visit.VisitDate).Days,
                        VisitType = visit.VisitType.ToString(),
                        VisitDate = visit.VisitDate.ToString("d"),
                        DueDate = visit.VisitDate.AddDays(7)
                    });
                }

                var model = new OverdueVisitWrapperViewModel { Visits = overdueVisitList, Facilities = listofFacilities, Providers = listofProviders, Patients = listofPatients };

                return View(model);
            }
        }


	}
}