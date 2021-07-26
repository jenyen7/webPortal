using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using EnterprisePortal.Models;
using EnterprisePortal.Utils;
using static EnterprisePortal.Utils.Enum;

namespace EnterprisePortal.Controllers
{
    public class RolesController : Controller
    {
        private readonly PortalModel db = new PortalModel();

        // GET: Roles
        public ActionResult Index()
        {
            return View(db.Roles.OrderByDescending(o => o.RoleId).ToList());
        }

        // GET: Roles/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Role role = db.Roles.Find(id);
            if (role == null)
            {
                return HttpNotFound();
            }
            return View(role);
        }

        // GET: Roles/Create
        public ActionResult Create()
        {
            ViewBag.tree = GetPermissionTree();
            return View();
        }

        // POST: Roles/Create
        // 若要避免過量張貼攻擊，請啟用您要繫結的特定屬性。
        // 如需詳細資料，請參閱 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Role role)
        {
            if (ModelState.IsValid)
            {
                if (String.IsNullOrWhiteSpace(role.RoleValue))
                {
                    role.RoleValue = "guest_only";
                }
                var roles = db.Roles.FirstOrDefault(f => f.RoleTitle.Equals(role.RoleTitle));
                if (roles != null)
                {
                    TempData["message"] = "此角色名稱已使用過，請更改名稱。";
                    ViewBag.tree = GetPermissionTree();
                    return View();
                }
                db.Roles.Add(role);
                db.SaveChanges();

                CurrentUser.RecordActivityWithSaveChanges(LogAction.新增, LogArea.角色, $"{role.RoleTitle}(id= {role.RoleId})");
                return RedirectToAction("Index");
            }
            ViewBag.tree = GetPermissionTree();
            return View(role);
        }

        // GET: Roles/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Role role = db.Roles.Find(id);
            if (role == null)
            {
                return HttpNotFound();
            }
            ViewBag.tree = GetPermissionTree();
            return View(role);
        }

        // POST: Roles/Edit/5
        // 若要避免過量張貼攻擊，請啟用您要繫結的特定屬性。
        // 如需詳細資料，請參閱 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Role role)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrWhiteSpace(role.RoleValue))
                {
                    role.RoleValue = "guest_only";
                }
                var roles = db.Roles.FirstOrDefault(f => f.RoleTitle.Equals(role.RoleTitle) && f.RoleId != role.RoleId);
                if (roles != null)
                {
                    TempData["message"] = "此角色名稱已使用過，請更改名稱。";
                    ViewBag.tree = GetPermissionTree();
                    return View();
                }
                db.Entry(role).State = EntityState.Modified;
                var log = CurrentUser.RecordActivity(LogAction.修改, LogArea.角色, $"{role.RoleTitle}(id= {role.RoleId})");
                db.UserLogs.Add(log);

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            //ViewBag.PermissionId = new SelectList(db.RolePermissions, "PermissionId", "PermissionName", role.PermissionId);
            ViewBag.tree = GetPermissionTree();
            return View(role);
        }

        [HttpPost]
        public JsonResult IsRoleAvailable(string roleTitle, string initialRole)
        {
            if (roleTitle == initialRole)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            var roles = db.Roles.FirstOrDefault(f => f.RoleTitle.Equals(roleTitle));
            return Json(roles == null, JsonRequestBehavior.AllowGet);
        }

        public string GetPermissions(ICollection<RolePermission> list)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var permission in list)
            {
                sb.Append("{\"id\":\"" + permission.PValue + "\",\"text\":\"" + permission.PermissionName + "\"");
                if (permission.PermissionList.Count > 0)
                {
                    sb.Append(",\"children\":[");
                    sb.Append(GetPermissions(permission.PermissionList));
                    sb.Append("]");
                }
                sb.Append("},");
                sb.ToString().TrimEnd(',');
            }
            return sb.ToString();
        }

        private string GetPermissionTree()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            sb.Append(GetPermissions(db.RolePermissions.Where(x => x.Pid == null).ToList()));
            sb.Append("]");
            return sb.ToString();
        }

        // GET: Roles/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Role role = db.Roles.Find(id);
            if (role == null)
            {
                return HttpNotFound();
            }
            return View(role);
        }

        // POST: Roles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Role role = db.Roles.Find(id);
            db.Roles.Remove(role);
            var log = CurrentUser.RecordActivity(LogAction.刪除, LogArea.角色, $"{role.RoleTitle}(id= {role.RoleId})");
            db.UserLogs.Add(log);

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