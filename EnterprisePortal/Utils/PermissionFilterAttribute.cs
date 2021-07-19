using EnterprisePortal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace EnterprisePortal.Utils
{
    public class PermissionFilterAttribute : ActionFilterAttribute
    {
        private readonly PortalModel db = new PortalModel();

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //var test = HttpContext.Current.Request.IsSecureConnection;
            //var test2 = HttpContext.Current.Request.Url.Scheme;

            string url = filterContext.HttpContext.Request.Url.ToString();
            if (!WithoutVerifyToken(url))
            {
                if (!HttpContext.Current.User.Identity.IsAuthenticated)
                {
                    filterContext.Result = new RedirectResult("~/Login/Login");
                    return;
                }
                string data = ((FormsIdentity)HttpContext.Current.User.Identity).Ticket.UserData;
                CurrentUser user = Newtonsoft.Json.JsonConvert.DeserializeObject<CurrentUser>(data);

                if (!String.IsNullOrEmpty(user.Permission))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("<div class='pcoded-navigatio-lavel'>其他管理</div>");
                    sb.Append(GetPermission(false, user.Permission, db.RolePermissions.Where(x => x.Pid == null).ToList()));
                    filterContext.Controller.ViewBag.Menu = sb.ToString();
                }
                filterContext.Controller.ViewBag.Account = user.Account;
                filterContext.Controller.ViewBag.Avatar = user.Avatar.Substring(1);
                filterContext.Controller.ViewBag.DepartmentId = user.Department;

                if (!WithoutVerifyPermission(url))
                {
                    string urlSection = url.Split('/')[3];
                    if (urlSection.IndexOf('?') > -1)
                    {
                        urlSection = urlSection.Split('?')[0];
                    }
                    if (!user.PermissionSection.Contains(urlSection))
                    {
                        filterContext.Result = new RedirectResult("~/Login/HomePage");
                        return;
                    }
                }
            }
            base.OnActionExecuting(filterContext);
        }

        private string GetPermission(bool flag, string currentUserPermission, ICollection<RolePermission> list)
        {
            StringBuilder sb = new StringBuilder();
            foreach (RolePermission permission in list)
            {
                if (currentUserPermission.IndexOf(permission.PValue) > -1)
                {
                    if (permission.PermissionList.Count > 0)
                    {
                        flag = true;
                        sb.Append("<ul class='pcoded-item pcoded-left-item'>");
                        sb.Append("<li class='pcoded-hasmenu'>");
                        sb.Append("<a href='javascript:void(0)'>");
                        sb.Append($"<span class='pcoded-micon'><i class='{permission.Icon}'></i></span>");
                        sb.Append($"<span class='pcoded-mtext'>{permission.PermissionName}</span>");
                        sb.Append("<span class='pcoded-mcaret'></span>");
                        sb.Append("</a>");
                        sb.Append(GetPermission(flag, currentUserPermission, permission.PermissionList));
                        sb.Append("</ul></ul>");
                    }
                    else
                    {
                        if (permission.Pid == null)
                        {
                            sb.Append("<ul class='pcoded-item pcoded-left-item'>");
                            sb.Append("<li>");
                            sb.Append($"<a href='{permission.Url}'>");
                            sb.Append($"<span class='pcoded-micon'><i class='{permission.Icon}'></i></span>");
                            sb.Append($"<span class='pcoded-mtext'>{permission.PermissionName}</span>");
                            sb.Append("<span class='pcoded-mcaret'></span>");
                            sb.Append("</a>");
                            sb.Append("</li>");
                            sb.Append("</ul>");
                        }
                        else
                        {
                            if (flag)
                            {
                                flag = false;
                                sb.Append("<ul class='pcoded-submenu'>");
                                sb.Append("<li>");
                                sb.Append($"<a href='{permission.Url}'>");
                                sb.Append($"<span class='pcoded-micon'><i class='{permission.Icon}'></i></span>");
                                sb.Append($"<span class='pcoded-mtext'>{permission.PermissionName}</span>");
                                sb.Append("<span class='pcoded-mcaret'></span>");
                                sb.Append("</li>");
                            }
                            else
                            {
                                sb.Append("<li>");
                                sb.Append($"<a href='{permission.Url}'>");
                                sb.Append($"<span class='pcoded-micon'><i class='{permission.Icon}'></i></span>");
                                sb.Append($"<span class='pcoded-mtext'>{permission.PermissionName}</span>");
                                sb.Append("<span class='pcoded-mcaret'></span>");
                                sb.Append("</li>");
                            }
                        }
                    }
                }
            }
            return sb.ToString();
        }

        public bool WithoutVerifyToken(string requestUri)
        {
            if (requestUri.EndsWith("/Login") ||
                requestUri.EndsWith("/Logout") ||
                requestUri.EndsWith("/ForgotPassword") ||
                requestUri.EndsWith("/ForgotPasswordConfirmation"))
                return true;
            return false;
        }

        public bool WithoutVerifyPermission(string requestUri)
        {
            if (requestUri.EndsWith("/HomePage") ||
                requestUri.EndsWith("/Settings") ||
                requestUri.Contains("/Announcements/Details") ||
                requestUri.Contains("/ApplicationForm") ||
                requestUri.Contains("/VotingForm") ||
                requestUri.Contains("/MessageLists"))
                return true;
            return false;
        }
    }
}