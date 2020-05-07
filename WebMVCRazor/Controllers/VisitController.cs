using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using WebMVCRazor.Models;

namespace WebMVCRazor.Controllers
{
   [Authorize(Roles = "Clerk, Administrator")]   
    public class VisitController : Controller
    {
        #region Visits

        //GET ALL VISITS
        public ActionResult Visits(int id, string Facilities, string Providers, bool _flag = false)
        {
            using (var db = new ApplicationDbContext())
            {
                IEnumerable<Visit> visits = from vis in db.Visits.ToList()
                    where vis.Patient.PatientId == id
                    select vis;

                var facilityList = db.Facilities.Where(fsc => fsc.IsActive).OrderBy(item => item.Name).Select(fsc => fsc.Name).ToList();
                var listofFacilities = new SelectList(facilityList);


                var providerList = db.Providers.Where(prov => prov.User.IsActive).OrderBy(item => item.User.UserName).Select(prov => prov.User.UserName).ToList();
                var listofProviders = new SelectList(providerList);

                if (!String.IsNullOrEmpty(Facilities))
                {
                    visits = visits.Where(c => c.Patient.Facility.Name.Contains(Facilities)).ToList();
                }
                if (!String.IsNullOrEmpty(Providers))
                {
                    visits = visits.Where(c => c.Provider.User.UserName.Contains(Providers)).ToList();
                }

                List<VisitWrapper> visitPat = visits.Select(item => new VisitWrapper
                {
                    PatientId = id,
                    VisitId = item.VisitId,
                    VisitDate = item.VisitDate.ToString("d"),
                    VisitType = item.VisitType.ToString(),
                    ProviderName = item.Provider.User.FirstName + " " + item.Provider.User.MiddleName + " " + item.Provider.User.LastName,                   
                    IsNoteComplete = item.IsNoteComplete
                }).ToList();

                TempData["SProv"] = Providers;
                TempData["VisFac"] = Facilities;
                TempData["Visits"] = visitPat;
                

                var initials = from init in db.Patients.ToList()
                               where init.PatientId == id
                               select new { firstName = init.FirstName, lastName = init.LastName };
                foreach (var item in initials)
                {
                    string title = string.Format("{0} {1} Visits", item.firstName, item.lastName);
                    ViewBag.Title = title;
                }
               
                
                var model = new VisitWrapperViewModel {PatientId = id, Visits = visitPat, Facilities = listofFacilities, Providers = listofProviders };
                ViewBag.Error = "List is Empty!";

                if (_flag) ViewBag.ResultMessage = "Document Saved.";

                return View(model);
            }
        }

        //GET ALL VISITS FOR PROVIDERS
        public ActionResult ProvidersVisits(string id, string Facilities, bool searchUnActive = false, bool _flag = false)
        {
            using (var db = new ApplicationDbContext())
            {
                var visits = from vis in db.Visits.ToList()
                                            where vis.ProviderId == id && vis.Patient.IsActive == true
                                            select vis;

                var facilityList = db.Facilities.Where(fsc => fsc.IsActive).OrderBy(item => item.Name).Select(fsc => fsc.Name).ToList();
                var listofFacilities = new SelectList(facilityList);
                
                var providerList = db.Providers.Where(prov => prov.User.IsActive).OrderBy(item => item.User.UserName).Select(prov => prov.User.UserName).ToList();
                var listofProviders = new SelectList(providerList);

                if (!String.IsNullOrEmpty(Facilities))
                {
                    visits = visits.Where(c => c.Patient.Facility.Name.Contains(Facilities)).ToList();
                }
                if (searchUnActive)
                {
                    visits = visits.Where(c => c.IsNoteComplete == false).ToList();
                }
               

                List<VisitWrapper> visitProviders = visits.Select(item => new VisitWrapper
                {
                    PatientName = item.Patient.FirstName + " " + item.Patient.MiddleName + " " + item.Patient.LastName,
                    VisitId = item.VisitId,
                    VisitDate = item.VisitDate.ToString().Substring(0, 10),
                    VisitType = item.VisitType.ToString(),
                    ProviderId = item.Provider.User.UserName,
                    IsNoteComplete = item.IsNoteComplete
                }).ToList();

                TempData["VisFacProv"] = Facilities;
                TempData["ProvVisits"] = visitProviders;
                

                var initials = from init in db.Providers.ToList()
                               where init.ProviderId == id
                               select new { firstName = init.User.FirstName, lastName = init.User.LastName };
                foreach (var item in initials)
                {
                    string title = string.Format("{0} {1} Visits", item.firstName, item.lastName);
                    ViewBag.Title = title;
                }


                var model = new VisitWrapperViewModel { ProviderId = id, Visits = visitProviders, Facilities = listofFacilities, Providers = listofProviders };
                ViewBag.Error = "List is Empty!";

                if (_flag) ViewBag.ResultMessage = "Document Saved.";

                return View(model);
            }
        }

        //
        // GET: ADD PROVIDERS VISIT
        public ActionResult AddProvidersVisit(string providerId)
        {
            using (var db = new ApplicationDbContext())
            {
                IEnumerable<string> patient = from pat in db.Patients.ToList()
                                               where pat.IsActive
                                               select pat.LastName;

                var model = new VisitViewModel();

                model.ProviderId = providerId;
                model.VisitDate = DateTime.Now;

                var list = new SelectList(patient.ToList());

                ViewBag.Id = model.ProviderId;
                ViewBag.Pat = list;
                return View(model);
            }
        }

        // POST: ADD PROVIDERS VISIT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddProvidersVisit(VisitViewModel model, String patientName)
        {
            using (var db = new ApplicationDbContext())
            {
                if (ModelState.IsValid)
                {
                   // model.ProviderId = patientName;//just a sting field
                    
                    var selectedId = (from p in db.Patients.ToList()
                                                      where p.LastName == patientName
                                                      select p.PatientId).First();

                   
                        db.Visits.Add(new Visit
                        {
                            PatientId = selectedId,
                            VisitType = model.VisitType,
                            VisitDate = model.VisitDate,
                            ProviderId = model.ProviderId,
                            IsNoteComplete = false
                        });
                    

                    if (model.VisitType == 0)
                    {

                        Patient patient = db.Patients.FirstOrDefault(u => u.PatientId == selectedId);
                        if (patient.AdmissionDate != null)
                        {
                            var regVisit = db.Visits.FirstOrDefault(u => u.PatientId == model.PatientId && u.VisitType == 0);

                            if (regVisit != null)
                            {
                                patient.EligibileDate = patient.AdmissionDate;
                                patient.DeadlineDate = Convert.ToDateTime(patient.AdmissionDate).AddDays(12);
                            }
                            else
                            {
                                patient.EligibileDate = model.VisitDate.AddDays(20);
                                patient.DeadlineDate = model.VisitDate.AddDays(40);
                            }
                        }
                    }


                    db.SaveChanges();

                    IEnumerable<string> patients = from pat in db.Patients.ToList()
                                                  where pat.IsActive
                                                  select pat.LastName;

                    var list = new SelectList(patients.ToList());

                    ViewBag.Pat = list;
                    return RedirectToAction("ProvidersVisits", "Visit", new { id = model.ProviderId });
                }

                IEnumerable<string> _patients = from pat in db.Patients.ToList()
                                               where pat.IsActive
                                               select pat.LastName;

                var lists = new SelectList(_patients.ToList());
                ViewBag.Pat = lists;
                // If we got this far, something failed, redisplay form
                return View(model);
            }
        }


        //
        // GET: ADD VISIT
        public ActionResult AddVisit(int patientId)
        {
            using (var db = new ApplicationDbContext())
            {
                IEnumerable<string> provider = from prov in db.Providers.ToList()
                    where prov.User.IsActive
                    select prov.User.UserName;
              
                var model = new VisitViewModel();

                model.PatientId = patientId;
                model.VisitDate = DateTime.Now;
             
                var list = new SelectList(provider.ToList());

                ViewBag.Id = model.PatientId;
                ViewBag.Prov = list;
                return View(model);
            }
        }


        // POST: ADD VISIT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddVisit(VisitViewModel model, String providerName)
        {
            using (var db = new ApplicationDbContext())
            {
                if (ModelState.IsValid)
                {
                    model.ProviderId = providerName;
                    // var db = new ApplicationDbContext();

                    IEnumerable<string> selectedIds = from p in db.Providers.ToList()
                        where p.User.UserName == model.ProviderId
                        select p.ProviderId;                  


                    var visits = db.Visits.Where(v => v.PatientId == model.PatientId).ToList();
                    foreach (var visit in visits)
                    {
                        if (visit.Provider.User.UserName == model.ProviderId && visit.VisitDate == model.VisitDate)
                        {

                            IEnumerable<string> _provider = from prov in db.Providers.ToList()
                                                            where prov.User.IsActive
                                                            select prov.User.UserName;
                            var lists = new SelectList(_provider.ToList());
                            ViewBag.Prov = lists;
                            ViewBag.ErrorFer = "There is visit assigned to this provider. Please change provider or visit date!";
                            return View(model);
                        }
                    }
                        
                            if (model.VisitType == 0)
                            {

                                Patient patient = db.Patients.FirstOrDefault(u => u.PatientId == model.PatientId);
                                if (patient.AdmissionDate != null)
                                {
                                    var regVisit = db.Visits.FirstOrDefault(u => u.PatientId == model.PatientId && u.VisitType == 0);

                                    if (regVisit != null)
                                    {
                                        db.Visits.Remove(regVisit);
                                        patient.EligibileDate = model.VisitDate;
                                        patient.DeadlineDate = model.VisitDate.AddDays(12);
                                    }
                                    else
                                    {
                                        patient.EligibileDate = model.VisitDate.AddDays(20);
                                        patient.DeadlineDate = model.VisitDate.AddDays(40);
                                    }
                                }
                            }

                            foreach (string id in selectedIds)
                            {
                                db.Visits.Add(new Visit
                                {
                                    PatientId = model.PatientId,
                                    VisitType = model.VisitType,
                                    VisitDate = model.VisitDate,
                                    ProviderId = id,
                                    IsNoteComplete = false
                                });
                            }
                        
            
                    
                    db.SaveChanges();
                    
                    IEnumerable<string> provider = from prov in db.Providers.ToList()
                        where prov.User.IsActive
                        select prov.User.UserName;
                    var list = new SelectList(provider.ToList());
                    
                    ViewBag.Prov = list;
                    return RedirectToAction("Visits", "Visit", new {id = model.PatientId});
                }

                IEnumerable<string> _providerr = from prov in db.Providers.ToList()
                    where prov.User.IsActive
                    select prov.User.UserName;
                var listss = new SelectList(_providerr.ToList());
                ViewBag.Prov = listss;
                // If we got this far, something failed, redisplay form
                return View(model);
            }
        }

        //EDIT VISIT
        public ActionResult EditVisit(int id, AccountController.ManageMessageId? message = null)
        {
            using (var db = new ApplicationDbContext())
            {
                Visit visit = db.Visits.First(u => u.VisitId == id);
                var model = new EditVisitViewModel();

                model.VisitId = visit.VisitId;
                model.VisitDate = visit.VisitDate;
                model.PatientId = visit.PatientId;
                model.IsNoteComplete = visit.IsNoteComplete;

                ViewBag.MessageId = message;
                ViewBag.Id = visit.PatientId;
                return View(model);
            }
        }

        //EDIT VISIT: POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditVisit(EditVisitViewModel model)
        {
            using (var db = new ApplicationDbContext())
            {
                if (ModelState.IsValid)
                {
                    Visit visit = db.Visits.First(u => u.VisitId == model.VisitId);

                    visit.VisitDate = model.VisitDate;
                    visit.IsNoteComplete = model.IsNoteComplete;

                    db.Entry(visit).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    return RedirectToAction("Visits", "Visit", new {id = model.PatientId});
                }
                // If we got this far, something failed, redisplay form
                return View(model);
            }
        }
         //COMPLETE NOTE
        public async Task<ActionResult> CompleteNote(int id, int PatId)
        {
            using (var db = new ApplicationDbContext())
            {
                
                    Visit visit = db.Visits.First(u => u.VisitId == id);

                    
                    visit.IsNoteComplete = true;

                    db.Entry(visit).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    return RedirectToAction("Visits", "Visit", new { id = PatId });
                
            }
        }

        //DELETE VISIT
        public ActionResult DeleteVisit(int id, int PatId)
        {
            using (var db = new ApplicationDbContext())
            {
                if (ModelState.IsValid)
                {
                    IEnumerable<Visit> data = from v in db.Visits.ToList()
                        where v.VisitId == id
                        select v;
                    foreach (Visit item in data)
                    {
                        db.Visits.Remove(item);
                    }

                    db.SaveChanges();
                }
                return RedirectToAction("Visits", "Visit", new {id = PatId});
            }
        }

       

        #endregion
    }
}