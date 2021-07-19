using EnterprisePortal.Models;
using EnterprisePortal.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EnterprisePortal.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ValidateUserBeforeDownload(string password)
        {
            int currentUserId = int.Parse(System.Web.HttpContext.Current.User.Identity.Name);
            using (var context = new PortalModel())
            {
                UserAccount currentUser = context.UserAccounts.FirstOrDefault(f => f.UserId == currentUserId);
                string passwordSalted = Salt.GenerateHashWithSalt(password, currentUser.PasswordSalt);
                if (!passwordSalted.Equals(currentUser.Password))
                {
                    return Json(new { success = false, message = "密碼錯誤" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = true, message = "good" }, JsonRequestBehavior.AllowGet);
                }
            };
        }
    }
}