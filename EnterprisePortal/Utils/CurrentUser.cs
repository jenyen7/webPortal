using EnterprisePortal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using static EnterprisePortal.Utils.Enum;

namespace EnterprisePortal.Utils
{
    public class CurrentUser
    {
        public string Account { get; set; }
        public string Avatar { get; set; }
        public string Permission { get; set; }
        public string PermissionSection { get; set; }
        public int Department { get; set; }

        /// <summary>
        /// 將使用者資料寫入cookie,產生AuthenTicket
        /// </summary>
        /// <param name="userData">使用者資料</param>
        /// <param name="userId">UserAccount</param>
        public static void SetAuthenTicket(string userId, string userData)
        {
            //宣告一個驗證票
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1, userId, DateTime.Now, DateTime.Now.AddDays(1), false, userData);
            //加密驗證票
            string encryptedTicket = FormsAuthentication.Encrypt(ticket);
            //建立Cookie
            HttpCookie authenticationCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
            {
                Expires = DateTime.Now.AddDays(1)
            };
            var context = HttpContext.Current;
            //將Cookie寫入回應
            if (context.Request.Cookies["myCookie"] != null)
            {
                context.Response.Cookies["myCookie"].Expires = DateTime.Now.AddDays(-1);
                context.Response.Cookies.Add(new HttpCookie("ASP.NET_SessionId", ""));
            }
            context.Response.Cookies.Add(authenticationCookie);
        }

        public static void RecordActivityWithSaveChanges(LogAction action, LogArea area, string nameId)
        {
            int currentUserId = int.Parse(HttpContext.Current.User.Identity.Name);
            using (var context = new PortalModel())
            {
                UserLog userLog = new UserLog()
                {
                    TheUser = currentUserId,
                    Action = action,
                    Area = area,
                    TheId = nameId,
                    Time = DateTime.Now
                };
                context.UserLogs.Add(userLog);
                context.SaveChanges();
            };
        }

        public static UserLog RecordActivity(LogAction action, LogArea area, string nameId)
        {
            int currentUserId = int.Parse(HttpContext.Current.User.Identity.Name);
            UserLog userLog = new UserLog()
            {
                TheUser = currentUserId,
                Action = action,
                Area = area,
                TheId = nameId,
                Time = DateTime.Now
            };
            return userLog;
        }
    }
}