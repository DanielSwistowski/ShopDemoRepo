using Ninject;
using NLog;
using ShopDemo.Controllers;
using ShopDemo.CustomModelBinders;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace ShopDemo
{
    public class MvcApplication : HttpApplication
    {
        [Inject]
        public ILogger Logger { get; set; }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //GlobalFilters.Filters.Add(new RequireHttpsAttribute());
            //AntiForgeryConfig.RequireSsl = true;

            ModelBinders.Binders.Add(typeof(decimal), new DecimalModelBinder());
            ModelBinders.Binders.Add(typeof(decimal?), new DecimalModelBinder());

            ModelBinders.Binders.Add(typeof(DateTime), new DateTimeModelBinder());
            ModelBinders.Binders.Add(typeof(DateTime?), new DateTimeModelBinder());
        }

        //protected void Application_BeginRequest()
        //{
        //    if (!Context.Request.IsSecureConnection)
        //        Response.Redirect(Context.Request.Url.ToString().Replace("http:", "https:"));
        //}

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception exception = Server.GetLastError();
            HttpException httpException = exception as HttpException;

            var area = Request.RequestContext.RouteData.DataTokens["area"];

            if (httpException != null)
            {
                int statusCode = httpException.GetHttpCode();

                Response.Clear();
                //Server.ClearError();
                Response.TrySkipIisCustomErrors = true;

                IController controller = new ErrorController();
                var routeData = new RouteData();

                if (area != null)
                    routeData.Values.Add("area", area.ToString());
                else
                    routeData.Values.Add("area", "");

                if (statusCode == 404)
                {
                    routeData.Values.Add("controller", "Error");
                    routeData.Values.Add("action", "NotFound");
                }
                else if (statusCode == 400)
                {
                    routeData.Values.Add("controller", "Error");
                    routeData.Values.Add("action", "BadRequest");
                }
                else if (statusCode == 403)
                {
                    routeData.Values.Add("controller", "Error");
                    routeData.Values.Add("action", "Forbidden");
                }
                else
                {
                    routeData.Values.Add("controller", "Error");
                    routeData.Values.Add("action", "Index");

                    Logger.Error(exception);
                }

                var context = new HttpContextWrapper(Context);
                context.Response.ContentType = "text/html";
                var requestContext = new RequestContext(context, routeData);
                controller.Execute(requestContext);
            }
            else
            {
                Logger.Error(exception.ToString());

                if (area!= null)
                {
                    if (area.ToString() == "Admin")
                    {
                        Response.Redirect("~/Admin/Error/Index");
                    }
                    else
                    {
                        Response.Redirect("~/Error/Index");
                    }
                }
                else
                {
                    Response.Redirect("~/Error/Index");
                }
            }
        }
    }
}