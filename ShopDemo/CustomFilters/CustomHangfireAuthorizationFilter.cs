using Hangfire.Dashboard;
using System.Web;
using Hangfire.Annotations;

namespace ShopDemo.CustomFilters
{
    public class CustomHangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize([NotNull] DashboardContext context)
        {
            return HttpContext.Current.User.IsInRole("Admin") ? true : false;
        }
    }
}