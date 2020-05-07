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
    public class PatientController : Controller
    {
        #region Patients

        //PATIENTS
        //ALL PATIENTS
        public ActionResult Patients(string searchFirstName, string searchLastName, string Facilities,
            bool searchUnActive = false)
        {
            using (var db = new ApplicationDbContext())
            {
                List<Patient> patients = db.Patients.ToList();

                var facilityList = db.Facilities.Where(fsc => fsc.IsActive).OrderBy(item => item.Name).Select(fsc => fsc.Name).ToList();
                
                if (!String.IsNullOrEmpty(searchFirstName))
                {
                    patients = patients.Where(c => c.FirstName.ToLower().Contains(searchFirstName.ToLower())).ToList();
                }
                if (!String.IsNullOrEmpty(searchLastName))
                {
                    patients = patients.Where(c => c.LastName.ToLower().Contains(searchLastName.ToLower())).ToList();
                }
                if (!String.IsNullOrEmpty(Facilities))
                {
                    patients = patients.Where(c => c.Facility.Name.Contains(Facilities)).ToList();
                }
                if (searchUnActive)
                {
                    patients = patients.Where(c => c.IsActive == false).ToList();
                }
                else
                {
                    patients = patients.Where(c => c.IsActive).ToList();
                }
                var usersPat = new List<PatientWrapper>();
                foreach (Patient item in patients)
                {
                    usersPat.Add(new PatientWrapper
                    {
                        PatientId = item.PatientId,
                        LocationId = item.LocationId,
                        FirstName = item.FirstName,
                        MiddleName = item.MiddleName,
                        LastName = item.LastName,
                        Location = item.Facility.Name,
                        AdmissionDate =
                            (item.AdmissionDate != null) ? Convert.ToDateTime(item.AdmissionDate).ToString("d") : null,
                        DischargeDate =
                            (item.DischargeDate != null) ? Convert.ToDateTime(item.DischargeDate).ToString("d") : null,
                        EligibileDate =
                            (item.EligibileDate != null) ? Convert.ToDateTime(item.EligibileDate).ToString("d") : null,
                        DeadlineDate =
                            (item.DeadlineDate != null) ? Convert.ToDateTime(item.DeadlineDate).ToString("d") : null,
                        IsActive = item.IsActive,
                    });
                }
                var lis = new SelectList(facilityList);
                TempData["Facilit"] = Facilities;
                TempData["Patients"] = usersPat;
                TempData["searchFirst"] = searchFirstName;
                TempData["searchLast"] = searchLastName;
                TempData["unActive"] = searchUnActive;
                var model = new PatientWrapperViewModel
                {
                    Facilities = lis,
                    Patients = usersPat.OrderBy(item => item.LastName)
                };
                ViewBag.Error = "List is Empty!";
                return View(model);
            }
        }

        public ActionResult AddPatient()
        {
            using (var db = new ApplicationDbContext())
            {
                IEnumerable<string> facilities = from fsc in db.Facilities.ToList()
                    where fsc.IsActive
                    select fsc.Name;
                var list = new SelectList(facilities.ToList());
                ViewBag.Facil = list;
                //---------------------ADD VISIT----------------------------------
                IEnumerable<string> provider = from prov in db.Providers.ToList()
                    where prov.User.IsActive
                    select prov.User.UserName;

                var model = new AddPatientViewModel();
                model.VisitDate = DateTime.Now;
                var listProv = new SelectList(provider.ToList());
                ViewBag.Prov = listProv;
                //----------------------------------------------------------------

                //PARAMETERS OF SEARCH-------------------------------
                ViewBag.first = TempData["searchFirst"] as string;
                ViewBag.last = TempData["searchLast"] as string;
                ViewBag.Active = !string.IsNullOrEmpty(TempData["unActive"] as string) &&
                                 Boolean.Parse(TempData["unActive"].ToString());
                ViewBag.FacilityArea = TempData["Facilit"] as string;
                //---------------------------------------------------

                return View();
            }
        }

        //
        // POST: ADD PATIENT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddPatient(AddPatientViewModel model, String location, String providerName)
        {
            using (var db = new ApplicationDbContext())
            {
                if (ModelState.IsValid)
                {
                    IEnumerable<int> selectId = from f in db.Facilities.ToList()
                        where f.Name == location
                        select f.Id;
                    if (model.MiddleName == null)
                    {
                        model.MiddleName = "    ";
                    }

                    Patient firstOrDefaultPatient =
                        db.Patients.FirstOrDefault(
                            u =>
                                u.FirstName == model.FirstName && u.LastName == model.LastName &&
                                u.MiddleName == model.MiddleName && u.DateOfBirth == model.DateOfBirth);
                    if (firstOrDefaultPatient != null)
                        firstOrDefaultPatient.IsActive = true;

                    else
                    {
                        foreach (int id in selectId)
                        {
                            if (model.AdmissionDate != null && model.VisitDate != null)
                            {
                                db.Patients.Add(new Patient
                                {
                                    LocationId = id,
                                    FirstName = model.FirstName,
                                    LastName = model.LastName,
                                    MiddleName = model.MiddleName,
                                    DateOfBirth = model.DateOfBirth,
                                    AdmissionDate = model.AdmissionDate,
                                    DischargeDate = null,
                                    EligibileDate = Convert.ToDateTime(model.VisitDate).AddDays(1),
                                    DeadlineDate = Convert.ToDateTime(model.VisitDate).AddDays(20),
                                    IsActive = true
                                });

                                db.SaveChanges();

                                //-------------------------------------------------------------------------
                                model.ProviderId = providerName;
                                IEnumerable<string> selectedIds = from p in db.Providers.ToList()
                                    where p.User.UserName == model.ProviderId
                                    select p.ProviderId;
                                int idPat =
                                    db.Patients.First(
                                        u =>
                                            u.FirstName == model.FirstName && u.LastName == model.LastName &&
                                            u.DateOfBirth == model.DateOfBirth).PatientId;
                                foreach (string iD in selectedIds)
                                {
                                    db.Visits.Add(new Visit
                                    {
                                        PatientId = idPat,
                                        VisitType = model.VisitType,
                                        VisitDate = model.VisitDate == null ? DateTime.Now : Convert.ToDateTime(model.VisitDate),
                                        ProviderId = iD,
                                        IsNoteComplete = false
                                    });
                                }
                                //-------------------------------------------------------------------------
                            }

                            if (model.AdmissionDate == null && model.VisitDate != null)
                            {
                                db.Patients.Add(new Patient
                                {
                                    LocationId = id,
                                    FirstName = model.FirstName,
                                    LastName = model.LastName,
                                    MiddleName = model.MiddleName,
                                    DateOfBirth = model.DateOfBirth,
                                    AdmissionDate = model.AdmissionDate,
                                    DischargeDate = null,
                                    EligibileDate = Convert.ToDateTime(model.VisitDate).AddDays(1),
                                    DeadlineDate = Convert.ToDateTime(model.VisitDate).AddDays(20),
                                    IsActive = true
                                });
                                db.SaveChanges();
                                //-------------------------------------------------------------------------
                                model.ProviderId = providerName;
                                IEnumerable<string> selectedIds = from p in db.Providers.ToList()
                                    where p.User.UserName == model.ProviderId
                                    select p.ProviderId;
                                int idPat =
                                    db.Patients.First(
                                        u =>
                                            u.FirstName == model.FirstName && u.LastName == model.LastName &&
                                            u.DateOfBirth == model.DateOfBirth).PatientId;
                                foreach (string iD in selectedIds)
                                {
                                    db.Visits.Add(new Visit
                                    {
                                        PatientId = idPat,
                                        VisitType = model.VisitType,
                                        VisitDate = model.VisitDate == null ? DateTime.Now : Convert.ToDateTime(model.VisitDate),
                                        ProviderId = iD,
                                        IsNoteComplete = false
                                    });
                                }

                            }
                            if (model.AdmissionDate != null && model.VisitDate == null)
                            {
                                db.Patients.Add(new Patient
                                {
                                    LocationId = id,
                                    FirstName = model.FirstName,
                                    LastName = model.LastName,
                                    MiddleName = model.MiddleName,
                                    DateOfBirth = model.DateOfBirth,
                                    AdmissionDate = model.AdmissionDate,
                                    DischargeDate = null,
                                    EligibileDate = Convert.ToDateTime(model.AdmissionDate).AddDays(1),
                                    DeadlineDate = Convert.ToDateTime(model.AdmissionDate).AddDays(20),
                                    IsActive = true
                                });
                            }
                            if (model.AdmissionDate == null && model.VisitDate == null)
                            {
                                db.Patients.Add(new Patient
                                {
                                    LocationId = id,
                                    FirstName = model.FirstName,
                                    LastName = model.LastName,
                                    MiddleName = model.MiddleName,
                                    DateOfBirth = model.DateOfBirth,
                                    AdmissionDate = model.AdmissionDate,
                                    DischargeDate = null,
                                    EligibileDate = null,
                                    DeadlineDate = null,
                                    IsActive = true
                                });
                            }
                        }
                    }


                    db.SaveChanges();


                    IEnumerable<string> facilities = from fsc in db.Facilities.ToList()
                        where fsc.IsActive
                        select fsc.Name;
                    var list = new SelectList(facilities);
                    ViewBag.Facil = list;
                    //---------------------ADD VISIT----------------------------------
                    IEnumerable<string> provider = from prov in db.Providers.ToList()
                        where prov.User.IsActive
                        select prov.User.UserName;
                    var listProv = new SelectList(provider.ToList());
                    ViewBag.Prov = listProv;
                    //----------------------------------------------------------------
                    return RedirectToAction("Patients", "Patient");
                }
                else
                {
                    IEnumerable<string> facilities = from fsc in db.Facilities.ToList()
                        where fsc.IsActive
                        select fsc.Name;
                    var list = new SelectList(facilities.ToList());
                    ViewBag.Facil = list;
                    //---------------------ADD VISIT----------------------------------
                    IEnumerable<string> provider = from prov in db.Providers.ToList()
                        where prov.User.IsActive
                        select prov.User.UserName;
                    var listProv = new SelectList(provider.ToList());
                    ViewBag.Prov = listProv;
                    //----------------------------------------------------------------
                    return View(model);
                }
            }
        }


        //EDIT PATIENT
        public ActionResult EditPatient(int id, AccountController.ManageMessageId? Message = null)
        {
            using (var db = new ApplicationDbContext())
            {
                Patient patient = db.Patients.First(u => u.PatientId == id);

                var model = new EditPatientViewModel();

                // TODO: Complete member initialization
                model.Id = patient.PatientId;
                model.FirstName = patient.FirstName;
                model.LastName = patient.LastName;
                model.MiddleName = patient.MiddleName;
                model.IsActive = patient.IsActive;
                model.DateOfBirth = patient.DateOfBirth;
                model.AdmissionDate = patient.AdmissionDate;
                model.DischargeDate = patient.DischargeDate;

                // model.AdmissionDate = patient.AdmissionDate;
                // model.DischargeDate = patient.DischargeDate;
                //  model.EligibileDate = patient.EligibileDate;
                //  model.DeadlineDate = patient.DeadlineDate;

                IEnumerable<string> facilities = from fsc in db.Facilities.ToList()
                    where fsc.IsActive
                    select fsc.Name;
                string facName = db.Patients.First(u => u.PatientId == id).Facility.Name;
                var list = new SelectList(facilities.ToList(), facName);

                ViewBag.Facil = list;
                //PARAMETERS OF SEARCH-------------------------------
                ViewBag.first = TempData["searchFirst"] as string;
                ViewBag.last = TempData["searchLast"] as string;
                ViewBag.Active = (bool) TempData["unActive"];
                ViewBag.FacilityArea = TempData["Facilit"] as string;
                //---------------------------------------------------
                ViewBag.MessageId = Message;

                return View(model);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        //EDIT PATIENT: POST
        public async Task<ActionResult> EditPatient(EditPatientViewModel model, String location)
        {
            using (var db = new ApplicationDbContext())
            {
                if (ModelState.IsValid)
                {
                    //var Db = new ApplicationDbContext();

                    Patient patient = db.Patients.First(u => u.PatientId == model.Id);

                    IEnumerable<int> selectedIds = from f in db.Facilities.ToList()
                        where f.Name == location
                        select f.Id;

                    patient.FirstName = model.FirstName;
                    patient.LastName = model.LastName;
                    patient.MiddleName = model.MiddleName;
                    patient.IsActive = model.IsActive;
                    patient.DateOfBirth = model.DateOfBirth;
                    patient.AdmissionDate = model.AdmissionDate;

                    if (model.DischargeDate != null)
                    {
                        patient.DischargeDate = model.DischargeDate;
                        patient.IsActive = false;
                    }

                    bool isRegVisit = db.Visits.Any(u => u.PatientId == model.Id && u.VisitType == 0);

                    if (isRegVisit && model.AdmissionDate != null)
                    {
                        patient.EligibileDate = Convert.ToDateTime(patient.AdmissionDate).AddDays(1);
                        patient.DeadlineDate = Convert.ToDateTime(patient.AdmissionDate).AddDays(20);
                    }
                   
                    foreach (int id in selectedIds)
                    {
                        patient.LocationId = id;
                    }

                    db.Entry(patient).State = EntityState.Modified;
                    await db.SaveChangesAsync();

                    string facName = db.Patients.First(u => u.PatientId == model.Id).Facility.Name;
                    IEnumerable<string> facilities = from fsc in db.Facilities.ToList()
                        where fsc.IsActive
                        select fsc.Name;
                    var list = new SelectList(facilities.ToList(), facName);
                    ViewBag.Facil = list;

                    return RedirectToAction("Patients", "Patient");
                }
                else
                {
                    //var db = new ApplicationDbContext();
                    string facName = db.Patients.First(u => u.PatientId == model.Id).Facility.Name;
                    IEnumerable<string> facilities = from fsc in db.Facilities.ToList()
                        where fsc.IsActive
                        select fsc.Name;
                    var list = new SelectList(facilities.ToList(), facName);
                    ViewBag.Facil = list;
                    // If we got this far, something failed, redisplay form
                    return View(model);
                }
            }
        }


        // DELETE PATIENT
        public ActionResult DeletePatient(int id)
        {
            using (var db = new ApplicationDbContext())
            {
                if (ModelState.IsValid)
                {
                    IEnumerable<Patient> data = from d in db.Patients.ToList()
                        where d.PatientId == id
                        select d;
                    foreach (Patient item in data)
                    {
                        item.IsActive = false;
                        //db.Patients.Remove(item);
                    }

                    db.SaveChanges();
                }

                return RedirectToAction("Patients", "Patient");
            }
        }

        #endregion

    }
}