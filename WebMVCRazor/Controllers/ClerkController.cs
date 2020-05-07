using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using WebMVCRazor.App_Start;
using WebMVCRazor.Models;

namespace WebMVCRazor.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class ClerkController : Controller
    {
        private ApplicationUserManager _userManager;

        public ClerkController()
        {
        }

        public ClerkController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ApplicationUserManager UserManager
        {
            get { return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
            private set { _userManager = value; }
        }

        //
        // GET: /Clerk/All
        public ActionResult Clerks()
        {
            using (var db = new ApplicationDbContext())
            {
                List<ApplicationUser> clerks = db.Users.Where(c => c.IsActive).ToList();
                var usersClerk = new List<ClerkWrapper>();
                foreach (ApplicationUser item in clerks)
                {
                    IList<string> rolesForUser = UserManager.GetRoles(item.Id);
                    usersClerk.AddRange(from role in rolesForUser
                        where UserManager.IsInRole(item.Id, "Clerk")
                        select new ClerkWrapper
                        {
                            UserId = item.Id,
                            UserName = item.UserName,
                            MiddleName = item.MiddleName,
                            FirstName = item.FirstName,
                            LastName = item.LastName,
                            Email = item.Email,
                            RoleName = role,
                            Password = item.PasswordHash,
                            IsActive = item.IsActive
                        });
                }

                ViewBag.Error = "List is Empty!";

                return View(usersClerk);
            }
        }

        //
        // GET: /Clerk/Add     
        public ActionResult AddClerk()
        {
            return View();
        }

        //
        // POST: /Clerk/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddClerk(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.MiddleName == null)
                {
                    model.MiddleName = "    ";
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

                IdentityResult result = UserManager.Create(user, model.Password);

                if (result.Succeeded)
                {
                    IdentityResult roleAdd = UserManager.AddToRole(user.Id, "Clerk");

                    return RedirectToAction("Clerks", "Clerk");
                }
                AddErrors(result);
            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // GET: /Clerk/Delete  
        public ActionResult DeleteClerk(string id)
        {
            using (var db = new ApplicationDbContext())
            {
                if (ModelState.IsValid)
                {
                    if (id == null)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }

                    IEnumerable<ApplicationUser> data = from d in db.Users.ToList()
                        where d.Id == id
                        select d;
                    foreach (ApplicationUser item in data)
                    {
                        item.IsActive = false;
                    }

                    db.SaveChanges();
                }
                return RedirectToAction("Clerks", "Clerk");
            }
        }

        // GET: /Clerk/Edit 
        public ActionResult EditClerk(string userName, AccountController.ManageMessageId? message = null)
        {
            using (var db = new ApplicationDbContext())
            {
                ApplicationUser user = db.Users.First(u => u.UserName == userName);
                var model = new EditClerkViewModel();

                model.UserId = user.Id;
                model.UserName = user.UserName;
                model.FirstName = user.FirstName;
                model.MiddleName = user.MiddleName;
                model.LastName = user.LastName;
                model.Email = user.Email;
                model.IsActive = user.IsActive;

                ViewBag.MessageId = message;

                return View(model);
            }
        }


        // POST: /Clerk/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditClerk(EditClerkViewModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = UserManager.FindById(model.UserId);

                user.UserName = model.UserName;
                user.FirstName = model.FirstName;
                user.MiddleName = model.MiddleName;
                user.LastName = model.LastName;
                user.Email = model.Email;
                user.IsActive = model.IsActive;

                IdentityResult result = await UserManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    if (model.NewPassword != null)
                    {
                        UserManager.RemovePassword(user.Id);
                        UserManager.AddPassword(user.Id, model.NewPassword);
                    }
                    return RedirectToAction("Clerks", "Clerk");
                }
                AddErrors(result);
            }
            return View(model);
        }

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
            AuthenticationManager.SignIn(new AuthenticationProperties {IsPersistent = isPersistent}, identity);
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
            return RedirectToAction("Index", "Home");
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
                var properties = new AuthenticationProperties {RedirectUri = RedirectUri};
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