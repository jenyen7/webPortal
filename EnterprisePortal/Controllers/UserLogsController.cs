using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using EnterprisePortal.Models;
using EnterprisePortal.Utils;

namespace EnterprisePortal.Controllers
{
    public class UserLogsController : Controller
    {
        private PortalModel db = new PortalModel();

        // GET: UserLogs
        public ActionResult Index()
        {
            var userLogs = db.UserLogs.Include(u => u.UserAccount).OrderByDescending(o => o.LogId).Take(500);
            return View(userLogs.ToList());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}