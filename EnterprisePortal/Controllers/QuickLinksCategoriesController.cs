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
    public class QuickLinksCategoriesController : Controller
    {
        private readonly PortalModel db = new PortalModel();

        // GET: QuickLinksCategories
        public ActionResult Index()
        {
            return View(db.QuickLinksCategories.ToList());
        }

        // GET: QuickLinksCategories/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: QuickLinksCategories/Create
        // 若要避免過量張貼攻擊，請啟用您要繫結的特定屬性。
        // 如需詳細資料，請參閱 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(QuickLinksCategory quickLinksCategory)
        {
            if (ModelState.IsValid)
            {
                var checkRepetition = db.QuickLinksCategories.FirstOrDefault(f => f.LinkCatName.Equals(quickLinksCategory.LinkCatName));
                if (checkRepetition != null)
                {
                    return Json("重複名稱。");
                }
                db.QuickLinksCategories.Add(quickLinksCategory);
                db.SaveChanges();

                CurrentUser.RecordActivityWithSaveChanges(LogAction.新增, LogArea.連結分類, $"{quickLinksCategory.LinkCatName}(id= {quickLinksCategory.LinkCatId})");
                return Json(quickLinksCategory.LinkCatId);
            }

            return View(quickLinksCategory);
        }

        // GET: QuickLinksCategories/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            QuickLinksCategory quickLinksCategory = db.QuickLinksCategories.Find(id);
            if (quickLinksCategory == null)
            {
                return HttpNotFound();
            }
            return View(quickLinksCategory);
        }

        // POST: QuickLinksCategories/Edit/5
        // 若要避免過量張貼攻擊，請啟用您要繫結的特定屬性。
        // 如需詳細資料，請參閱 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(QuickLinksCategory quickLinksCategory)
        {
            if (ModelState.IsValid)
            {
                db.Entry(quickLinksCategory).State = EntityState.Modified;
                var log = CurrentUser.RecordActivity(LogAction.修改, LogArea.連結分類, $"{quickLinksCategory.LinkCatName}(id= {quickLinksCategory.LinkCatId})");
                db.UserLogs.Add(log);

                db.SaveChanges();
                return Json(quickLinksCategory.LinkCatId);
            }
            return View(quickLinksCategory);
        }

        // GET: QuickLinksCategories/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            QuickLinksCategory quickLinksCategory = db.QuickLinksCategories.Find(id);
            if (quickLinksCategory == null)
            {
                return HttpNotFound();
            }
            return View(quickLinksCategory);
        }

        // POST: QuickLinksCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            QuickLinksCategory quickLinksCategory = db.QuickLinksCategories.Find(id);
            db.QuickLinksCategories.Remove(quickLinksCategory);
            var log = CurrentUser.RecordActivity(LogAction.刪除, LogArea.連結分類, $"{quickLinksCategory.LinkCatName}(id= {quickLinksCategory.LinkCatId})");
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