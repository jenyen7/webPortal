using EnterprisePortal.Utils;
using System.Web;
using System.Web.Mvc;

namespace EnterprisePortal
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new PermissionFilterAttribute());
        }
    }
}