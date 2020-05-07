using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using WebMVCRazor.Models;

namespace WebMVCRazor.Controllers
{
    [Authorize(Roles = "Clerk, Administrator")]   
    public class FacilityController : Controller
    {
        //ALL FACILITIES
        public ActionResult Facilities()
        {
            using (var db = new ApplicationDbContext())
            {
                List<Facility> facilities = db.Facilities.Where(c => c.IsActive == true).ToList();

                ViewBag.Error = "List is Empty!";
                return View(facilities);
            }
        }

        //
        // GET: ADD FACILITY
        public ActionResult AddFacility()
        {
            return View();
        }

        //
        // POST: ADD FACILITY
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddFacility(FacilityViewModel model)
        {
            using (var db = new ApplicationDbContext())
            {
                if (ModelState.IsValid)
                {
                    db.Facilities.Add(new Facility {Name = model.Name, Address = model.Address, PhoneNumber = model.PhoneNumber, IsActive = true});
                    db.SaveChanges();

                    return RedirectToAction("Facilities", "Facility");
                }

                // If we got this far, something failed, redisplay form
                return View(model);
            }
        }


        //EDIT FACILITY
        public ActionResult EditFacility(int id, AccountController.ManageMessageId? message = null)
        {
            using (var db = new ApplicationDbContext())
            {
                Facility facility = db.Facilities.First(u => u.Id == id);
                var model = new EditFacilityViewModel();

                // TODO: Complete member initialization
                model.Id = facility.Id;
                model.Name = facility.Name;
                model.Address = facility.Address;
                model.PhoneNumber = facility.PhoneNumber;
                model.IsActive = facility.IsActive;              

                ViewBag.MessageId = message;
                return View(model);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        //EDIT FACILITY: POST
        public async Task<ActionResult> EditFacility(EditFacilityViewModel model)
        {
            using (var db = new ApplicationDbContext())
            {
                if (ModelState.IsValid)
                {
                    Facility facility = db.Facilities.First(u => u.Id == model.Id);

                    facility.Name = model.Name;
                    facility.Address = model.Address;
                    facility.PhoneNumber = model.PhoneNumber;
                    facility.IsActive = model.IsActive;

                    db.Entry(facility).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    return RedirectToAction("Facilities", "Facility");
                }
                // If we got this far, something failed, redisplay form
                return View(model);
            }
        }

        // DELETE FACILITY
        public ActionResult DeleteFacility(int id)
        {
            using (var db = new ApplicationDbContext())
            {
                if (ModelState.IsValid)
                {
                    IEnumerable<Facility> data = from d in db.Facilities.ToList()
                        where d.Id == id
                        select d;
                    foreach (Facility item in data)
                    {
                        // db.Facilities.Remove(item);
                        item.IsActive = false;
                    }

                    db.SaveChanges();
                }
                return RedirectToAction("Facilities", "Facility");
            }
        }
    }
}