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
using static EnterprisePortal.Utils.Enum;

namespace EnterprisePortal.Controllers
{
    public class QuickLinksController : Controller
    {
        private readonly PortalModel db = new PortalModel();

        // GET: QuickLinks
        public ActionResult Index()
        {
            QuickLinksViewModel quickLinksView = new QuickLinksViewModel
            {
                QuickLinks = db.QuickLinks.Include(q => q.QuickLinksCategory).OrderByDescending(o => o.LinkId).ToList(),
                QuickLinksCategories = db.QuickLinksCategories.ToList()
            };
            return View(quickLinksView);
        }

        // GET: QuickLinks/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            QuickLink quickLink = db.QuickLinks.Find(id);
            if (quickLink == null)
            {
                return HttpNotFound();
            }
            return View(quickLink);
        }

        // GET: QuickLinks/Create
        public ActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(db.QuickLinksCategories, "LinkCatId", "LinkCatName");
            return View();
        }

        // POST: QuickLinks/Create
        // 若要避免過量張貼攻擊，請啟用您要繫結的特定屬性。
        // 如需詳細資料，請參閱 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(QuickLink quickLink)
        {
            if (ModelState.IsValid)
            {
                db.QuickLinks.Add(quickLink);
                db.SaveChanges();

                CurrentUser.RecordActivityWithSaveChanges(LogAction.新增, LogArea.連結, $"{quickLink.LinkName}(id= {quickLink.LinkId})");
                return RedirectToAction("Index");
            }

            ViewBag.CategoryId = new SelectList(db.QuickLinksCategories, "LinkCatId", "LinkCatName", quickLink.CategoryId);
            return View(quickLink);
        }

        // GET: QuickLinks/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            QuickLink quickLink = db.QuickLinks.Find(id);
            if (quickLink == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryId = new SelectList(db.QuickLinksCategories, "LinkCatId", "LinkCatName", quickLink.CategoryId);
            return View(quickLink);
        }

        // POST: QuickLinks/Edit/5
        // 若要避免過量張貼攻擊，請啟用您要繫結的特定屬性。
        // 如需詳細資料，請參閱 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(QuickLink quickLink)
        {
            if (ModelState.IsValid)
            {
                db.Entry(quickLink).State = EntityState.Modified;
                var log = CurrentUser.RecordActivity(LogAction.修改, LogArea.連結, $"{quickLink.LinkName}(id= {quickLink.LinkId})");
                db.UserLogs.Add(log);

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryId = new SelectList(db.QuickLinksCategories, "LinkCatId", "LinkCatName", quickLink.CategoryId);
            return View(quickLink);
        }

        // GET: QuickLinks/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            QuickLink quickLink = db.QuickLinks.Find(id);
            if (quickLink == null)
            {
                return HttpNotFound();
            }
            return View(quickLink);
        }

        // POST: QuickLinks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            QuickLink quickLink = db.QuickLinks.Find(id);
            db.QuickLinks.Remove(quickLink);
            var log = CurrentUser.RecordActivity(LogAction.刪除, LogArea.連結, $"{quickLink.LinkName}(id= {quickLink.LinkId})");
            db.UserLogs.Add(log);

            db.SaveChanges();
            return Json(Url.Action("Index", "QuickLinks"));
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