using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(EnterprisePortal.Startup))]

namespace EnterprisePortal
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            app.MapSignalR();
        }
    }
}