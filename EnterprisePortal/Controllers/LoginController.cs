using EnterprisePortal.Models;
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

namespace EnterprisePortal.Controllers
{
    public class LoginController : Controller
    {
        private readonly PortalModel db = new PortalModel();

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
                        Avatar = user.Avatar,
                        Permission = permissions.Key,
                        PermissionSection = permissions.Value,
                        Department = user.DepId
                    };
                    string data = Newtonsoft.Json.JsonConvert.SerializeObject(userInfo);
                    CurrentUser.SetAuthenTicket(user.UserId.ToString(), data);
                    using (var context = new PortalModel())
                    {
                        UserLog userLog = new UserLog()
                        {
                            TheUser = user.UserId,
                            Action = LogAction.登入,
                            Area = LogArea.首頁登入,
                            TheId = "",
                            Time = DateTime.Now
                        };
                        context.UserLogs.Add(userLog);
                        context.SaveChanges();
                    };
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
                        permissionSections += permission.Url.Split('/')[3] + ",";
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

            var toDoLists = db.ToDoLists.Where(w => w.UserId == currentUserId).OrderByDescending(o => o.PublishedDate).Take(50).ToList();
            var activitiesLists = db.ActivitiesApplications.Where(w => w.UserId == currentUserId).OrderByDescending(o => o.Activity.PublishedDate).Take(25).ToList();
            var votingLists = db.VotingForms.Where(w => w.UserId == currentUserId).OrderByDescending(o => o.Voting.PublishedDate).Take(25).ToList();

            var allLists = new List<AllList>();
            toDoLists.ForEach(item => allLists.Add(new AllList()
            {
                Id = item.ToDoListId,
                ListType = ListType.待辦,
                Title = item.Title,
                ListStatus = item.ListStatus,
                EndTime = item.EndTime,
                PublishedDate = item.PublishedDate
            }));
            activitiesLists.ForEach(item => allLists.Add(new AllList()
            {
                Id = item.ActivityId,
                ListType = ListType.活動,
                Title = item.Activity.Title,
                ListStatus = item.ListStatus,
                EndTime = item.Activity.EndTime,
                PublishedDate = item.Activity.PublishedDate
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
            var announcements = db.Announcements.Where(w => w.DepId == user.DepId || w.IsPublic).OrderByDescending(o => o.PublishedDate).Take(50).ToList();

            SettingsViewModel settings = new SettingsViewModel
            {
                Account = user.Account,
                Email = user.Email,
                Avatar = user.Avatar.Substring(1),
                Department = user.Department.DepartmentName,
                JoinedDate = user.JoinedDate.ToString("yyyy-MM-dd")
            };

            Activity activity = db.Activities.OrderByDescending(o => o.ActivityId).FirstOrDefault(f => f.UserId == currentUserId);
            Voting voting = db.Votings.OrderByDescending(o => o.VotingId).FirstOrDefault(f => f.UserId == currentUserId);

            HomePageViewModel homePage = new HomePageViewModel
            {
                AllLists = allLists.OrderByDescending(o => o.PublishedDate),
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
            SettingsViewModel settings = new SettingsViewModel
            {
                UserId = thisUser.UserId,
                Account = thisUser.Account,
                PhoneNumber = thisUser.PhoneNumber,
                Email = thisUser.Email,
                Avatar = thisUser.Avatar
            };
            return View(settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Settings(SettingsViewModel model, HttpPostedFileBase file)
        {
            UserAccount thisUser = db.UserAccounts.FirstOrDefault(f => f.UserId == model.UserId);
            model.Avatar = thisUser.Avatar;
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
                    message = Utility.UploadImage(file, "Avatars");
                    if (!message.StartsWith("~/Upload"))
                    {
                        TempData["messageForAvatar"] = message;
                    }
                }
                if (TempData["messageForPhone"] != null || TempData["messageForEmail"] != null || TempData["messageForAvatar"] != null)
                {
                    return View(model);
                }

                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    thisUser.PasswordSalt = Salt.CreateSalt();
                    thisUser.Password = Salt.GenerateHashWithSalt(model.Password, thisUser.PasswordSalt);
                }
                thisUser.PhoneNumber = model.PhoneNumber;
                thisUser.Email = model.Email;
                thisUser.Avatar = string.IsNullOrEmpty(message) ? thisUser.Avatar : message;
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