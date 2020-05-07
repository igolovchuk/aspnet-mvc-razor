using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using WebMVCRazor.App_Start;
using WebMVCRazor.Models;
using Microsoft.Owin.Security;
using System.Security.Claims;

namespace WebMVCRazor.Controllers
{
    [Authorize(Roles = "Clerk, Administrator")]   
    public class ProviderController : Controller
    {
        private ApplicationUserManager _userManager;

        public ProviderController()
        {
        }

        public ProviderController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ApplicationUserManager UserManager
        {
            get { return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
            private set { _userManager = value; }
        }

        #region Providers

        //ALL PROVIDERS
        public ActionResult Providers()
        {
            using (var db = new ApplicationDbContext())
            {
                List<ApplicationUser> providers = db.Users.Where(c => c.IsActive == true).ToList();
                var usersPat = new List<ProviderWrapper>();
                foreach (ApplicationUser item in providers)
                {
                    IList<string> rolesForUser = UserManager.GetRoles(item.Id);
                    usersPat.AddRange(from role in rolesForUser
                        where UserManager.IsInRole(item.Id, "MD") || UserManager.IsInRole(item.Id, "NP")
                        select new ProviderWrapper
                        {
                            UserId = item.Id,
                            UserName = item.UserName,
                            MiddleName = item.MiddleName,
                            FirstName = item.FirstName,
                            LastName = item.LastName,
                            Email = item.Email,
                            RoleName = role,
                            IsActive = item.IsActive
                        });
                }

                ViewBag.Error = "List is Empty!";
                return View(usersPat);
            }
        }

        // DELETE PROVIDER
        public ActionResult DeleteProvider(string id)
        {
            using (var db = new ApplicationDbContext())
            {
                if (ModelState.IsValid)
                {
                    if (id == null)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }

                    IEnumerable<Provider> data = from d in db.Providers.ToList()
                        where d.ProviderId == id
                        select d;
                    foreach (Provider item in data)
                    {
                        item.User.IsActive = false;
                    }

                    db.SaveChanges();
                }
                return RedirectToAction("Providers", "Provider");
            }
        }

        //EDIT PROVIDER
        public ActionResult EditProvider(string userName, AccountController.ManageMessageId? message = null)
        {
            using (var db = new ApplicationDbContext())
            {
                ApplicationUser user = db.Users.First(u => u.UserName == userName);
                Provider provider = db.Providers.First(u => u.ProviderId == user.Id);
                var model = new EditUserViewModel();

                // TODO: Complete member initialization
                model.UserId = user.Id;
                model.UserName = user.UserName;
                model.FirstName = user.FirstName;
                model.MiddleName = user.MiddleName;
                model.LastName = user.LastName;
                model.Email = user.Email;
                model.IsActive = user.IsActive;
                model.DateOfBirth = provider.DateOfBirth;


                ViewBag.MessageId = message;
                TempData["nema"] = user.UserName;

                var roleForUser = UserManager.GetRoles(user.Id).First();
                IEnumerable<string> roles = from role in db.Roles.ToList()
                    where role.Name != "Clerk" && role.Name != "Administrator"
                    select role.Name;

                var list = new SelectList(roles.ToList(),roleForUser);
                ViewBag.Roles = list;

                return View(model);
            }
        }


        //EDIT PROVIDER: POST

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditProvider(EditUserViewModel model, String RoleName)
        {
            using (var db = new ApplicationDbContext())
            {
                ApplicationUser user = db.Users.First(u => u.UserName == model.UserName);
                if (ModelState.IsValid)
                {
                    
                    Provider provider = db.Providers.First(u => u.ProviderId == user.Id);

                    user.FirstName = model.FirstName;
                    user.MiddleName = model.MiddleName;
                    user.LastName = model.LastName;
                    user.Email = model.Email;
                    user.IsActive = model.IsActive;
                    provider.DateOfBirth = model.DateOfBirth;


                    if (!UserManager.IsInRole(user.Id, RoleName))
                    {
                        IList<string> rolesForUser = await UserManager.GetRolesAsync(user.Id);

                        if (rolesForUser.Any())
                        {
                            foreach (string item in rolesForUser)
                            {
                                // item should be the name of the role
                                IdentityResult result = await UserManager.RemoveFromRoleAsync(user.Id, item);
                            }
                            IdentityResult roleAdd = await UserManager.AddToRoleAsync(user.Id, RoleName);
                        }
                    }

                    db.Entry(user).State = EntityState.Modified;
                    await db.SaveChangesAsync();

                    IEnumerable<string> roles = from role in db.Roles.ToList()
                        where role.Name != "Clerk" && role.Name != "Administrator"
                        select role.Name;
                    var roleForUser = UserManager.GetRoles(user.Id).First();
                    var list = new SelectList(roles.ToList(), roleForUser);
                    ViewBag.Roles = list;
                    return RedirectToAction("Providers", "Provider");
                }
                else
                {
                    IEnumerable<string> roles = from role in db.Roles.ToList()
                        where role.Name != "Clerk" && role.Name != "Administrator"
                        select role.Name;
                    var roleForUser = UserManager.GetRoles(user.Id).First();
                    var list = new SelectList(roles.ToList(), roleForUser);
                    ViewBag.Roles = list;
                    // If we got this far, something failed, redisplay form
                    return View(model);
                }
            }
        }

        //
        // GET: ADD PROVIDER
        public ActionResult AddProvider()
        {
            using (var db = new ApplicationDbContext())
            {
                IEnumerable<string> roles = from role in db.Roles.ToList()
                    where role.Name != "Clerk" && role.Name != "Administrator"
                    select role.Name;

                var list = new SelectList(roles.ToList());
                ViewBag.Roles = list;
                return View();
            }
        }

        //
        // POST: ADD PROVIDER
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddProvider(AddProviderViewModel model, String roleName)
        {
            if (ModelState.IsValid)
            {
                if (model.MiddleName == null)
                {
                    model.MiddleName = string.Empty;
                }
                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    FirstName = model.FirstName,
                    MiddleName = model.MiddleName,
                    LastName = model.LastName,
                    Email = model.Email,
                    EmailConfirmed = true,
                    IsActive = true
                };
                var ctx = new ApplicationDbContext();
                var oldUser = ctx.Providers.FirstOrDefault(u => u.User.FirstName == model.FirstName && u.User.LastName == model.LastName && u.User.MiddleName == model.MiddleName && u.User.UserName == model.UserName && u.DateOfBirth == model.DateOfBirth);
                // bool exist =  oldUser!= null ? true : false;
                if (oldUser != null)
                {
                    oldUser.User.IsActive = true;
                    ctx.SaveChanges();
                    return RedirectToAction("Providers", "Provider");
                }
                else
                {
                    var result = UserManager.Create(user, model.Password);
                    if (result.Succeeded)
                    {
                        IdentityResult roleAdd = UserManager.AddToRole(user.Id, roleName);
                        using (var db = new ApplicationDbContext())
                        {

                            db.Providers.Add(new Provider { ProviderId = user.Id, DateOfBirth = model.DateOfBirth });
                            db.SaveChanges();

                            return RedirectToAction("Providers", "Provider");

                        }
                    }
                    else
                    {
                        AddErrors(result);
                    }

                   
                }
            }
            var ctxt = new ApplicationDbContext();
            IEnumerable<string> rolels = from role in ctxt.Roles.ToList()
                                         where role.Name != "Clerk" && role.Name != "Administrator"
                                         select role.Name;

            var listlt = new SelectList(rolels.ToList());
            ViewBag.Roles = listlt;
            // If we got this far, something failed, redisplay form
            return View(model);

        }


        #endregion


        #region Helpers

        // Used for XSRF protection when adding external logins
        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            Error
        }

        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get { return HttpContext.GetOwinContext().Authentication; }
        }

        private async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            ClaimsIdentity identity =
                await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, identity);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (string error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        private class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties() { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }

        #endregion
    }
}