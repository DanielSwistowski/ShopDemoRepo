using System.Net;
using System.Web.Helpers;
using System.Web.Mvc;

namespace ShopDemo.CustomValidationAttributes
{
    public class ValidateAntiForgeryTokenForAjaxPostAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var request = filterContext.HttpContext.Request;
            
            if (request.HttpMethod == WebRequestMethods.Http.Post)
            {
                if (request.IsAjaxRequest())
                {
                    var antiForgeryCookie = request.Cookies[AntiForgeryConfig.CookieName];

                    var cookieValue = antiForgeryCookie != null
                        ? antiForgeryCookie.Value
                        : null;

                    AntiForgery.Validate(cookieValue, request.Headers["__RequestVerificationToken"]);
                }
                else
                {
                    base.OnAuthorization(filterContext);
                }
            }
        }
    }
}