﻿using EnterprisePortal.Models;
using EnterprisePortal.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Web.Security;
using System.Configuration;
using static EnterprisePortal.Utils.Enum;
using System.Web.Configuration;

namespace EnterprisePortal.Controllers
{
    public class LoginController : Controller
    {
        private readonly PortalModel db = new PortalModel();
        private readonly string avatarFolder = ConfigurationManager.AppSettings["routeAvatarFolder"];

        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ReCaptcha]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginHomeViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = db.UserAccounts.FirstOrDefault(u => (u.Email.Equals(model.Account) || u.Account.Equals(model.Account)) && u.IsEnable == true);
                if (user == null)
                {
                    TempData["error"] = "帳號或密碼錯誤。";
                    return View(model);
                }
                string password = Salt.GenerateHashWithSalt(model.Password, user.PasswordSalt);
                string passwordTemp = password + "ahahajisdf42";
                if (!password.Equals(user.Password) && !passwordTemp.Equals(user.Password))
                {
                    TempData["error"] = "帳號或密碼錯誤。";
                    return View(model);
                }
                else
                {
                    var permissions = GetPermissionStr(user);
                    CurrentUser userInfo = new CurrentUser()
                    {
                        Account = user.Account,
                        Avatar = avatarFolder + user.Avatar,
                        Permission = permissions.Key,
                        PermissionSection = permissions.Value,
                        Department = user.DepId
                    };
                    string data = Newtonsoft.Json.JsonConvert.SerializeObject(userInfo);
                    CurrentUser.SetAuthenTicket(user.UserId.ToString(), data);
                    CreateToDoListForApplicationForm(user.UserId, user.DepId);
                    RecordUserLoggedIn(user.UserId);

                    if (user.Password.EndsWith("ahahajisdf42"))
                    {
                        TempData["modalTitle"] = "修改密碼提醒。";
                        TempData["modalBody"] = "如果您使用了暫時密碼登入，請記得修改成自己的密碼。";
                        return RedirectToAction("Settings", "Login");
                    }
                    else
                    {
                        return RedirectToAction("HomePage", "Login");
                    }
                }
            }
            return View(model);
        }

        private KeyValuePair<string, string> GetPermissionStr(UserAccount user)
        {
            string permissions = "";
            string permissionSections = "";
            var permissionsList = db.RolePermissions.Select(s => new { s.PValue, s.Url }).ToList();
            foreach (var role in user.UserRoles)
            {
                string roleVal = role.Role.RoleValue;
                permissions += roleVal + ",";
                foreach (var permission in permissionsList)
                {
                    var roleValSplit = roleVal.Split(',');
                    if (roleValSplit.Contains(permission.PValue))
                    {
                        permissionSections += permission.Url.Split('/')[1] + ",";
                        if (permission.Url.Contains("QuickLinks"))
                        {
                            permissionSections += "QuickLinksCategories" + ",";
                        }
                        if (permission.Url.Contains("Votings"))
                        {
                            permissionSections += "VotingContents" + ",";
                        }
                    }
                }
            }
            var result = new KeyValuePair<string, string>(permissions, permissionSections);
            return result;
        }

        private void CreateToDoListForApplicationForm(int userId, int userDepId)
        {
            var Activities = db.Activities.Where(w => (w.ColleaguesIds.Contains(userId.ToString()) || w.DepartmentsIds.Contains(userDepId.ToString())) && w.ActivityStatus == true && w.EndTime < DateTime.Now);
            foreach (var item in Activities)
            {
                var thisActivityToDo = db.ToDoLists.FirstOrDefault(f => f.Summary.Equals(item.ActivityId.ToString()) && f.ListType == ListType.活動 && f.UserId == userId);
                if (thisActivityToDo == null)
                {
                    var newToDo = new ToDoList
                    {
                        ListType = ListType.活動,
                        Title = item.Title,
                        EndTime = item.EndTime,
                        PublishedDate = item.PublishedDate,
                        Summary = item.ActivityId.ToString(),
                        UserId = userId,
                    };
                    db.ToDoLists.Add(newToDo);
                }
            }
            db.SaveChanges();
        }

        private void RecordUserLoggedIn(int userId)
        {
            using (var context = new PortalModel())
            {
                UserLog userLog = new UserLog()
                {
                    TheUser = userId,
                    Action = LogAction.登入,
                    Area = LogArea.首頁登入,
                    TheId = "",
                    Time = DateTime.Now
                };
                context.UserLogs.Add(userLog);
                context.SaveChanges();
            };
        }

        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(SendEmailViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (!Utility.IsValidEmail(model.Email))
                {
                    TempData["message"] = "Email格式不符。";
                }
                var user = db.UserAccounts.FirstOrDefault(u => u.Email.Equals(model.Email));
                if (user == null)
                {
                    TempData["message"] = "此Email不存在。";
                }
                else
                {
                    if (!user.Password.EndsWith("ahahajisdf42"))
                    {
                        string tempGuid = Guid.NewGuid().ToString();
                        string tempPassword = Salt.GenerateHashWithSalt(tempGuid, user.PasswordSalt) + "ahahajisdf42";
                        user.Password = tempPassword;
                        db.SaveChanges();
                        try
                        {
                            string emailBody = Utility.PopulateBody(user.Account, $@"請使用暫時的密碼登入，登入後請記得修改密碼。<br /> 暫時的密碼:{tempGuid}", "https://localhost:44313/Login/Login", "重新登入");
                            Utility.SendHtmlFormattedEmail(user.Email, "忘記密碼", emailBody);
                            return View("ForgotPasswordConfirmation");
                        }
                        catch
                        {
                            TempData["message"] = "Email發送失敗。";
                        }
                    }
                    else
                    {
                        TempData["message"] = "您已申請過暫時的密碼喔。";
                    }
                }
            }
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [ChildActionOnly]
        public ActionResult RenderNavBar()
        {
            NavBarViewModel navbar = new NavBarViewModel
            {
                QuickLinksCategory = db.QuickLinksCategories
            };
            return PartialView("_NavBarPartial", navbar);
        }

        [ChildActionOnly]
        public ActionResult RenderAnnouncements(int? id)
        {
            var announcements = db.Announcements.Where(w => w.DepId == id || w.IsPublic);
            var list = new List<AnnouncementViewModel>();
            foreach (var announcement in announcements)
            {
                var announcementView = new AnnouncementViewModel
                {
                    DepartmentColor = announcement.Department.DepartmentColor,
                    DepartmentName = announcement.Department.DepartmentName,
                    AnnouncementTitle = announcement.Title,
                    TimeAgo = "30 分鐘之前"
                };
                list.Add(announcementView);
            }
            return PartialView("_AnnouncementsPartial", list);
        }

        public ActionResult HomePage()
        {
            int currentUserId = int.Parse(System.Web.HttpContext.Current.User.Identity.Name);

            var toDoLists = db.ToDoLists.Where(w => w.UserId == currentUserId).OrderByDescending(o => o.PublishedDate).Take(75).ToList();
            var votingLists = db.VotingForms.Where(w => w.UserId == currentUserId).OrderByDescending(o => o.Voting.PublishedDate).Take(25).ToList();

            var allLists = new List<AllList>();
            toDoLists.ForEach(item => allLists.Add(new AllList()
            {
                Id = item.ToDoListId,
                ListType = item.ListType,
                Title = item.Title,
                Summary = item.Summary,
                ListStatus = item.ListStatus,
                EndTime = item.EndTime,
                PublishedDate = item.PublishedDate
            }));
            votingLists.ForEach(item => allLists.Add(new AllList()
            {
                Id = item.VotingId,
                ListType = ListType.投票,
                Title = item.Voting.Title,
                ListStatus = item.ListStatus,
                EndTime = item.Voting.EndTime,
                PublishedDate = item.Voting.PublishedDate
            }));

            UserAccount user = db.UserAccounts.FirstOrDefault(f => f.UserId == currentUserId);
            string avatar = avatarFolder + user.Avatar;
            var announcements = db.Announcements.Where(w => w.DepId == user.DepId || w.IsPublic).OrderByDescending(o => o.PublishedDate).Take(50).ToList();

            SettingsViewModel settings = new SettingsViewModel
            {
                Account = user.Account,
                Email = user.Email,
                Avatar = avatar,
                Department = user.Department.DepartmentName,
                JoinedDate = user.JoinedDate.ToString("yyyy-MM-dd")
            };

            Activity activity = db.Activities.OrderByDescending(o => o.ActivityId).FirstOrDefault(f => f.UserId == currentUserId);
            Voting voting = db.Votings.OrderByDescending(o => o.VotingId).FirstOrDefault(f => f.UserId == currentUserId);

            HomePageViewModel homePage = new HomePageViewModel
            {
                AllLists = allLists,
                Announcements = announcements,
                Settings = settings,
                Activity = activity,
                Voting = voting
            };
            return View(homePage);
        }

        public ActionResult Settings()
        {
            int currentUserId = int.Parse(System.Web.HttpContext.Current.User.Identity.Name);
            UserAccount thisUser = db.UserAccounts.FirstOrDefault(f => f.UserId == currentUserId);
            string avatar = avatarFolder + thisUser.Avatar;
            SettingsViewModel settings = new SettingsViewModel
            {
                UserId = thisUser.UserId,
                Account = thisUser.Account,
                PhoneNumber = thisUser.PhoneNumber,
                Email = thisUser.Email,
                Avatar = avatar
            };
            return View(settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Settings(SettingsViewModel model, HttpPostedFileBase file)
        {
            UserAccount thisUser = db.UserAccounts.FirstOrDefault(f => f.UserId == model.UserId);
            model.Avatar = avatarFolder + thisUser.Avatar;
            if (ModelState.IsValid)
            {
                if (!Utility.IsValidPhoneNumber(model.PhoneNumber))
                {
                    TempData["messageForPhone"] = "電話號碼應為09起頭10位數。";
                }
                if (!Utility.IsValidEmail(model.Email))
                {
                    TempData["messageForEmail"] = "Email格式不符合";
                }
                string message = "";
                if (file != null)
                {
                    message = Utility.UploadImage(file, avatarFolder);
                    if (!message.StartsWith("~/Upload"))
                    {
                        TempData["messageForAvatar"] = message;
                    }
                }
                if (TempData["messageForPhone"] != null || TempData["messageForEmail"] != null || TempData["messageForAvatar"] != null)
                {
                    return View(model);
                }

                if (!String.IsNullOrWhiteSpace(model.Password))
                {
                    thisUser.PasswordSalt = Salt.CreateSalt();
                    thisUser.Password = Salt.GenerateHashWithSalt(model.Password, thisUser.PasswordSalt);
                }
                if (!String.IsNullOrEmpty(message))
                {
                    thisUser.Avatar = message.Substring(message.LastIndexOf('/') + 1);
                }
                thisUser.PhoneNumber = model.PhoneNumber ?? thisUser.PhoneNumber;
                thisUser.Email = model.Email ?? thisUser.Email;
                db.SaveChanges();

                return RedirectToAction("HomePage");
            }
            return View(model);
        }

        public ActionResult Logout()
        {
            var context = System.Web.HttpContext.Current;
            if (context.Request.Cookies["myCookie"] != null)
            {
                context.Response.Cookies["myCookie"].Expires = DateTime.Now.AddDays(-1);
            }
            Session.Abandon();
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Login");
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