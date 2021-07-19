using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
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
    public class UserAccountsController : Controller
    {
        private readonly PortalModel db = new PortalModel();
        private readonly string folder = "Avatars";

        // GET: UserAccounts
        public ActionResult Index()
        {
            var userAccounts = db.UserAccounts.Include(u => u.Department).OrderByDescending(o => o.UserId);
            return View(userAccounts.ToList());
        }

        // GET: UserAccounts/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserAccount userAccount = db.UserAccounts.Find(id);
            if (userAccount == null)
            {
                return HttpNotFound();
            }
            return View(userAccount);
        }

        // GET: UserAccounts/Create
        public ActionResult Create()
        {
            ViewBag.DepId = new SelectList(db.Departments, "DepartmentId", "DepartmentName");
            return View();
        }

        // POST: UserAccounts/Create
        // 若要避免過量張貼攻擊，請啟用您要繫結的特定屬性。
        // 如需詳細資料，請參閱 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UserAccount userAccount, HttpPostedFileBase file, string[] roles)
        {
            if (ModelState.IsValid)
            {
                UserAccount account = db.UserAccounts.FirstOrDefault(u => u.Account.Equals(userAccount.Account) || u.Email.Equals(userAccount.Email));
                if (account != null)
                {
                    TempData["duplicateMsg"] = "帳號或Email重複了，請重新填寫。";
                }
                if (!Utility.IsValidPhoneNumber(userAccount.PhoneNumber))
                {
                    TempData["messageForPhone"] = "電話號碼應為09起頭10位數。";
                }
                if (!Utility.IsValidEmail(userAccount.Email))
                {
                    TempData["messageForEmail"] = "Email格式不符合";
                }
                string message = Utility.UploadImage(file, folder);
                if (!message.StartsWith("~/Upload"))
                {
                    TempData["messageForAvatar"] = message;
                }

                if (TempData["duplicateMsg"] != null || TempData["messageForPhone"] != null || TempData["messageForEmail"] != null || TempData["messageForAvatar"] != null)
                {
                    ViewBag.DepId = new SelectList(db.Departments, "DepartmentId", "DepartmentName", userAccount.DepId);
                    return View(userAccount);
                }
                else
                {
                    userAccount.Avatar = message;
                    userAccount.IsEnable = true;
                    userAccount.JoinedDate = DateTime.Now;
                    userAccount.PasswordSalt = Salt.CreateSalt();
                    userAccount.Password = Salt.GenerateHashWithSalt(userAccount.Password, userAccount.PasswordSalt);

                    db.UserAccounts.Add(userAccount);
                    db.SaveChanges();

                    CurrentUser.RecordActivityWithSaveChanges(LogAction.新增, LogArea.帳號, $"{userAccount.Account}(id= {userAccount.UserId})");
                    RecordUserRoles(userAccount, roles);
                    return RedirectToAction("Index");
                }
            }
            ViewBag.DepId = new SelectList(db.Departments, "DepartmentId", "DepartmentName", userAccount.DepId);
            return View(userAccount);
        }

        // GET: UserAccounts/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserAccount userAccount = db.UserAccounts.Find(id);
            if (userAccount == null)
            {
                return HttpNotFound();
            }
            ViewBag.DepId = new SelectList(db.Departments, "DepartmentId", "DepartmentName", userAccount.DepId);
            return View(userAccount);
        }

        // POST: UserAccounts/Edit/5
        // 若要避免過量張貼攻擊，請啟用您要繫結的特定屬性。
        // 如需詳細資料，請參閱 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UserAccount userAccount, HttpPostedFileBase file, string[] roles)
        {
            UserAccount account = db.UserAccounts.FirstOrDefault(u => u.Email.Equals(userAccount.Email) && u.UserId != userAccount.UserId);
            if (account != null)
            {
                TempData["duplicateMsg"] = "Email重複了，請重新填寫。";
                ViewBag.DepId = new SelectList(db.Departments, "DepartmentId", "DepartmentName", userAccount.DepId);
                return View(userAccount);
            }

            if (!Utility.IsValidPhoneNumber(userAccount.PhoneNumber))
            {
                TempData["messageForPhone"] = "電話號碼應為09起頭10位數。";
            }
            if (!Utility.IsValidEmail(userAccount.Email))
            {
                TempData["messageForEmail"] = "Email格式不符合";
            }
            UserAccount thisUser = db.UserAccounts.FirstOrDefault(f => f.UserId == userAccount.UserId);
            string message = "";
            if (file != null)
            {
                message = Utility.UploadImage(file, folder);
                if (!message.StartsWith("~/Upload"))
                {
                    TempData["messageForAvatar"] = message;
                }
            }
            if (TempData["messageForPhone"] != null || TempData["messageForEmail"] != null || TempData["messageForAvatar"] != null)
            {
                ViewBag.DepId = new SelectList(db.Departments, "DepartmentId", "DepartmentName", userAccount.DepId);
                return View(userAccount);
            }

            if (!string.IsNullOrWhiteSpace(userAccount.Password))
            {
                thisUser.PasswordSalt = Salt.CreateSalt();
                thisUser.Password = Salt.GenerateHashWithSalt(userAccount.Password, thisUser.PasswordSalt);
            }

            thisUser.PhoneNumber = userAccount.PhoneNumber ?? thisUser.PhoneNumber;
            thisUser.Email = userAccount.Email ?? thisUser.Email;
            thisUser.IsEnable = userAccount.IsEnable;
            thisUser.DepId = userAccount.DepId;
            thisUser.Avatar = string.IsNullOrEmpty(message) ? thisUser.Avatar : message;
            var log = CurrentUser.RecordActivity(LogAction.修改, LogArea.帳號, $"{userAccount.Account}(id= {userAccount.UserId})");
            db.UserLogs.Add(log);

            db.SaveChanges();

            RecordUserRoles(thisUser, roles);
            return RedirectToAction("Index");
        }

        // GET: UserAccounts/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserAccount userAccount = db.UserAccounts.Find(id);
            if (userAccount == null)
            {
                return HttpNotFound();
            }
            return View(userAccount);
        }

        // POST: UserAccounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            UserAccount userAccount = db.UserAccounts.Find(id);
            db.UserAccounts.Remove(userAccount);
            var checkVotingResults = db.VotingResults.Where(w => w.UserId == id);
            if (checkVotingResults != null)
            {
                db.VotingResults.RemoveRange(checkVotingResults.ToList());
            }
            var log = CurrentUser.RecordActivity(LogAction.刪除, LogArea.帳號, $"{userAccount.Account}(id= {userAccount.UserId})");
            db.UserLogs.Add(log);

            db.SaveChanges();
            return Json(Url.Action("Index"));
        }

        private void RecordUserRoles(UserAccount user, string[] roles)
        {
            var currentRoles = user.UserRoles;
            if (currentRoles == null)
            {
                foreach (string role in roles)
                {
                    int roleId = int.Parse(role);
                    UserRole userRole = new UserRole
                    {
                        UserId = user.UserId,
                        RoleId = roleId
                    };
                    db.UserRoles.Add(userRole);
                }
            }
            else
            {
                if (roles != null)
                {
                    var checkedRoles = roles.Select(s => int.Parse(s));
                    var checkedNotPresent = checkedRoles.Except(currentRoles.Select(s => s.RoleId)).ToList();
                    var presentNotChecked = currentRoles.Select(s => s.RoleId).Except(checkedRoles).ToList();
                    if (checkedNotPresent.Any())
                    {
                        foreach (int role in checkedNotPresent)
                        {
                            UserRole userRole = new UserRole
                            {
                                UserId = user.UserId,
                                RoleId = role
                            };
                            db.UserRoles.Add(userRole);
                        }
                    }
                    if (presentNotChecked.Any())
                    {
                        foreach (int id in presentNotChecked)
                        {
                            UserRole roleToBeRemoved = db.UserRoles.FirstOrDefault(f => f.RoleId == id && f.UserId == user.UserId);
                            if (roleToBeRemoved != null)
                            {
                                db.UserRoles.Remove(roleToBeRemoved);
                            }
                        }
                    }
                }
                else
                {
                    var rolesToBeAllRemoved = db.UserRoles.Where(w => w.UserId == user.UserId);
                    db.UserRoles.RemoveRange(rolesToBeAllRemoved);
                }
            }
            db.SaveChanges();
        }

        [ChildActionOnly]
        public ActionResult RenderRolesCheckboxes(int? id)
        {
            var roles = new List<UserRoleViewModel>();
            if (id != null)
            {
                var permissionList = db.UserAccounts.FirstOrDefault(u => u.UserId == id).UserRoles;
                if (permissionList.Any())
                {
                    var permission = permissionList.Select(s => s.RoleId).ToArray();
                    foreach (var item in db.Roles)
                    {
                        UserRoleViewModel role = new UserRoleViewModel()
                        {
                            RoleTitle = item.RoleTitle,
                            RoleValue = item.RoleId,
                            IsChecked = Array.IndexOf(permission, item.RoleId) > -1
                        };
                        roles.Add(role);
                    }
                    return PartialView("_RoleCheckboxesPartial", roles);
                }
            }

            foreach (var item in db.Roles)
            {
                UserRoleViewModel role = new UserRoleViewModel()
                {
                    RoleTitle = item.RoleTitle,
                    RoleValue = item.RoleId,
                    IsChecked = false
                };
                roles.Add(role);
            }
            return PartialView("_RoleCheckboxesPartial", roles);
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