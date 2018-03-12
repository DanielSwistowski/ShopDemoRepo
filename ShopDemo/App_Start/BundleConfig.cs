using System.Web;
using System.Web.Optimization;

namespace ShopDemo
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/jquerydecimal").Include(
                "~/Scripts/CustomScripts/DecimalValidation.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryalertmanager").Include(
                "~/Scripts/CustomScripts/alertsManager.js"));

            bundles.Add(new ScriptBundle("~/bundles/modal").Include(
                 "~/Scripts/jquery-3.1.1.min.js",
                "~/Scripts/bootstrap.min.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/fancyboxjs").Include(
                "~/Scripts/jquery.fancybox.pack.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/moment").Include(
                "~/Scripts/moment.min.js",
                "~/Scripts/moment-with-locales.min.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/datetimepicker").Include(
                "~/Scripts/bootstrap-datetimepicker.min.js"
                ));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/productIndex").Include(
               "~/Areas/Admin/Scripts/Product/productIndex.js",
               "~/Areas/Base/Scripts/categoriesMenu.js",
               "~/Areas/Base/Scripts/searchFilters.js"));

            bundles.Add(new ScriptBundle("~/bundles/productsDeletedFromOffer").Include(
                "~/Areas/Admin/Scripts/Product/productDeletedFromOffer.js",
                "~/Areas/Base/Scripts/categoriesMenu.js"));

            bundles.Add(new ScriptBundle("~/bundles/productsOnPromotion").Include(
                "~/Areas/Admin/Scripts/Product/productsOnPromotion.js",
                "~/Areas/Base/Scripts/categoriesMenu.js"));

            bundles.Add(new ScriptBundle("~/bundles/addProduct").Include(
                "~/Areas/Admin/Scripts/Product/addProduct.js",
                "~/Areas/Admin/Scripts/Product/addOrEditProduct.js"));

            bundles.Add(new ScriptBundle("~/bundles/editProduct").Include(
                "~/Areas/Admin/Scripts/Product/editProduct.js",
                "~/Areas/Admin/Scripts/Product/addOrEditProduct.js"));

            bundles.Add(new ScriptBundle("~/bundles/adminProductDetails").Include(
                "~/Areas/Admin/Scripts/Product/productDetails.js"
               ));

            bundles.Add(new ScriptBundle("~/bundles/customerProductIndex").Include(
                "~/Scripts/CustomScripts/Cart/cartShared.js",
               "~/Scripts/CustomScripts/Product/productIndex.js",
               "~/Areas/Base/Scripts/categoriesMenu.js",
               "~/Areas/Base/Scripts/searchFilters.js"
               ));

            bundles.Add(new ScriptBundle("~/bundles/customerProductDetails").Include(
                "~/Scripts/CustomScripts/Cart/cartShared.js",
               "~/Scripts/CustomScripts/Product/productDetails.js"
               ));

            bundles.Add(new ScriptBundle("~/bundles/cart").Include(
                "~/Scripts/CustomScripts/Cart/cartShared.js",
               "~/Scripts/CustomScripts/Cart/cart.js"
               ));

            bundles.Add(new ScriptBundle("~/bundles/categoryIndex").Include(
                "~/Areas/Admin/Scripts/Category/categoryIndex.js"
                ));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/Site.css"
                      ));

            bundles.Add(new StyleBundle("~/Content/pagecss").Include(
                "~/Content/PageList.css"
                ));

            bundles.Add(new StyleBundle("~/Content/fancyboxcss").Include(
                "~/Content/jquery.fancybox.css"
                ));

            bundles.Add(new StyleBundle("~/Content/datetimepickercss").Include(
                "~/Content/bootstrap-datetimepicker.min.css"
                ));
        }
    }
}
