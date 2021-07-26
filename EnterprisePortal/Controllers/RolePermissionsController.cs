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
    public class RolePermissionsController : Controller
    {
        private readonly PortalModel db = new PortalModel();

        // GET: RolePermissions
        public ActionResult Index()
        {
            var rolePermissions = db.RolePermissions.Include(r => r.Permission).OrderByDescending(o => o.PermissionId);
            return View(rolePermissions.ToList());
        }

        public ActionResult Icons()
        {
            return View();
        }

        // GET: RolePermissions/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RolePermission rolePermission = db.RolePermissions.Find(id);
            if (rolePermission == null)
            {
                return HttpNotFound();
            }
            return View(rolePermission);
        }

        // GET: RolePermissions/Create
        public ActionResult Create()
        {
            ViewBag.Pid = new SelectList(db.RolePermissions, "PermissionId", "PermissionName");
            return View();
        }

        // POST: RolePermissions/Create
        // 若要避免過量張貼攻擊，請啟用您要繫結的特定屬性。
        // 如需詳細資料，請參閱 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(RolePermission rolePermission)
        {
            if (ModelState.IsValid)
            {
                if (String.IsNullOrWhiteSpace(rolePermission.Icon))
                {
                    rolePermission.Icon = "ti-layout-grid2-alt";
                }
                db.RolePermissions.Add(rolePermission);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            ViewBag.Pid = new SelectList(db.RolePermissions, "PermissionId", "PermissionName", rolePermission.Pid);
            return View(rolePermission);
        }

        // GET: RolePermissions/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RolePermission rolePermission = db.RolePermissions.Find(id);
            if (rolePermission == null)
            {
                return HttpNotFound();
            }
            ViewBag.Pid = new SelectList(db.RolePermissions, "PermissionId", "PermissionName", rolePermission.Pid);
            return View(rolePermission);
        }

        // POST: RolePermissions/Edit/5
        // 若要避免過量張貼攻擊，請啟用您要繫結的特定屬性。
        // 如需詳細資料，請參閱 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(RolePermission rolePermission)
        {
            if (ModelState.IsValid)
            {
                if (String.IsNullOrWhiteSpace(rolePermission.Icon))
                {
                    rolePermission.Icon = "ti-layout-grid2-alt";
                }
                db.Entry(rolePermission).State = EntityState.Modified;
                var log = CurrentUser.RecordActivity(LogAction.修改, LogArea.權限, $"{rolePermission.PermissionName}(id= {rolePermission.PermissionId})");
                db.UserLogs.Add(log);

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Pid = new SelectList(db.RolePermissions, "PermissionId", "PermissionName", rolePermission.Pid);
            return View(rolePermission);
        }

        // GET: RolePermissions/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RolePermission rolePermission = db.RolePermissions.Find(id);
            if (rolePermission == null)
            {
                return HttpNotFound();
            }
            return View(rolePermission);
        }

        // POST: RolePermissions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var permissionList = db.RolePermissions.Select(s => s.Pid).ToList();
            RolePermission rolePermission = db.RolePermissions.Find(id);
            if (permissionList.IndexOf(rolePermission.PermissionId) > -1)
            {
                TempData["message"] = $"無法刪除，此權限[{rolePermission.PermissionName}]底下有其他權限，請先刪除他底下的權限再回來刪除此權限。";
                return Json(Url.Action("Index"));
            }
            db.RolePermissions.Remove(rolePermission);

            db.SaveChanges();
            return Json(Url.Action("Index"));
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