using Hangfire;
using Hangfire.Dashboard;
using Microsoft.Owin;
using Owin;
using ShopDemo.App_Start;
using ShopDemo.CustomFilters;
using System.Web;

[assembly: OwinStartup(typeof(ShopDemo.Startup))]
namespace ShopDemo
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            GlobalConfiguration.Configuration.UseSqlServerStorage("HangfireConnection");

            var options = new DashboardOptions
            {
                Authorization = new[] { new CustomHangfireAuthorizationFilter() },
                AppPath = VirtualPathUtility.ToAbsolute("~/admin")
            };

            app.UseHangfireDashboard("/admin/hangfire-dashboard", options);
            app.UseHangfireServer();

            HangfireJobsInitializer.Initialize();
        }
    }
}
