using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(EnterprisePortal.Startup))]

namespace EnterprisePortal
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
            ConfigureAuth(app);
        }
    }
}