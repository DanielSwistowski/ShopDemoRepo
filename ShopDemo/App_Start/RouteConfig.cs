using System.Web.Mvc;
using System.Web.Routing;

namespace ShopDemo
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "ProductDetails",
                url: "produkty/szczegoly-produktu/{productId}/{productName}",
                defaults: new { controller = "Product", action = "ProductDetails", productId = UrlParameter.Optional, productName = UrlParameter.Optional },
                namespaces: new[] { "ShopDemo.Controllers" }
            );

            routes.MapRoute(
                name: "ProductIndexCategory",
                url: "produkty/{categoryId}/{categoryName}",
                defaults: new { controller = "Product", action = "Index", categoryId = UrlParameter.Optional, categoryName = UrlParameter.Optional },
                constraints: new { categoryId = @"\d+" },
                namespaces: new[] { "ShopDemo.Controllers" }
            );

            routes.MapRoute(
                name: "ProductIndex",
                url: "produkty",
                defaults: new { controller = "Product", action = "Index" },
                namespaces: new[] { "ShopDemo.Controllers" }
            );

            routes.MapRoute(
                name: "ShoppingCart",
                url: "koszyk",
                defaults: new { controller = "Cart", action = "GetCart" },
                namespaces: new[] { "ShopDemo.Controllers" }
            );

            routes.MapRoute(
                name: "PayuPayment",
                url: "platnosci/platnosc-zakonczona/{orderId}",
                defaults: new { controller = "Payu", action = "PaymentComplete", orderId = UrlParameter.Optional },
                constraints: new { orderId = @"\d+" },
                namespaces: new[] { "ShopDemo.Controllers" }
            );

            routes.MapRoute(
                name: "PayuCreateOrder",
                url: "platnosci/nowa-platnosc/{orderId}",
                defaults: new { controller = "Payu", action = "CreateNewOrder", orderId = UrlParameter.Optional },
                constraints: new { orderId = @"\d+" },
                namespaces: new[] { "ShopDemo.Controllers" }
            );

            routes.MapRoute(
                name: "PayuMakePayment",
                url: "platnosci/zalegla-platnosc/{orderId}",
                defaults: new { controller = "Payu", action = "MakePaymentForAnExistingOrder", orderId = UrlParameter.Optional },
                constraints: new { orderId = @"\d+" },
                namespaces: new[] { "ShopDemo.Controllers" }
            );

            routes.MapRoute(
                name: "Delivery",
                url: "dostawa/wybierz-sposob-dostawy",
                defaults: new { controller = "Delivery", action = "SelectDeliveryOption" },
                namespaces: new[] { "ShopDemo.Controllers" }
            );

            #region Order
            routes.MapRoute(
                name: "OrderIndex",
                url: "zamowienia",
                defaults: new { controller = "Order", action = "Index" },
                namespaces: new[] { "ShopDemo.Controllers" }
            );

            routes.MapRoute(
                name: "OrderDetails",
                url: "zamowienia/szczegoly-zamowienia/{orderId}",
                defaults: new { controller = "Order", action = "Details", orderId = UrlParameter.Optional },
                constraints: new { orderId = @"\d+" },
                namespaces: new[] { "ShopDemo.Controllers" }
            );

            routes.MapRoute(
                name: "OrderSummary",
                url: "zamowienia/podsumowanie/{deliveryId}",
                defaults: new { controller = "Order", action = "OrderSummaryPreview", deliveryId = UrlParameter.Optional },
                constraints: new { deliveryId = @"\d+" },
                namespaces: new[] { "ShopDemo.Controllers" }
            );

            routes.MapRoute(
                name: "OrderCreate",
                url: "zamowienia/utworz-zamowienie",
                defaults: new { controller = "Order", action = "CreateOrder" },
                namespaces: new[] { "ShopDemo.Controllers" }
            );

            routes.MapRoute(
                name: "UpdateOrderSummary",
                url: "zamowienia/podsumowanie/{orderId}/{deliveryId}",
                defaults: new { controller = "Order", action = "UpdateOrderSummaryPreview", orderId = UrlParameter.Optional, deliveryId = UrlParameter.Optional },
                constraints: new { orderId = @"\d+", deliveryId = @"\d+" },
                namespaces: new[] { "ShopDemo.Controllers" }
            );

            routes.MapRoute(
                name: "OrderUpdate",
                url: "zamowienia/aktualizuj-zamowienie/{orderId}",
                defaults: new { controller = "Order", action = "UpdateOrder", orderId = UrlParameter.Optional },
                constraints: new { orderId = @"\d+" },
                namespaces: new[] { "ShopDemo.Controllers" }
            );

            routes.MapRoute(
                name: "OrderFailure",
                url: "zamowienia/blad-zamowienia",
                defaults: new { controller = "Order", action = "OrderFailure" },
                namespaces: new[] { "ShopDemo.Controllers" }
            );

            routes.MapRoute(
                name: "Order_Success",
                url: "zamowienia/zamowienie-utworzone/{orderId}",
                defaults: new { controller = "Order", action = "OrderSuccess", orderId = UrlParameter.Optional },
                constraints: new { orderId = @"\d+" },
                namespaces: new[] { "ShopDemo.Controllers" }
            );

            routes.MapRoute(
                name: "CancelOrder",
                url: "zamowienia/anuluj-zamowienie/{orderId}",
                defaults: new { controller = "Order", action = "CancelOrder", orderId = UrlParameter.Optional },
                constraints: new { orderId = @"\d+" },
                namespaces: new[] { "ShopDemo.Controllers" }
            );

            routes.MapRoute(
                name: "RemoveORderFromList",
                url: "zamowienia/usun-z-listy/{orderId}",
                defaults: new { controller = "Order", action = "RemoveOrderFromList", orderId = UrlParameter.Optional },
                constraints: new { orderId = @"\d+" },
                namespaces: new[] { "ShopDemo.Controllers" }
            );
            #endregion

            #region Manage
            routes.MapRoute(
                name: "ManageIndex",
                url: "ustawienia-konta",
                defaults: new { controller = "Manage", action = "Index" },
                namespaces: new[] { "ShopDemo.Controllers" }
            );

            routes.MapRoute(
                name: "ManegeChangePassword",
                url: "ustawienia-konta/zmien-haslo",
                defaults: new { controller = "Manage", action = "ChangePassword" },
                namespaces: new[] { "ShopDemo.Controllers" }
            );

            routes.MapRoute(
                name: "ManegeEditPersonalData",
                url: "ustawienia-konta/edytuj-dane-osobowe",
                defaults: new { controller = "Manage", action = "EditPersonalData" },
                namespaces: new[] { "ShopDemo.Controllers" }
            );

            routes.MapRoute(
                name: "ManegeEditAddress",
                url: "ustawienia-konta/edytuj-adres",
                defaults: new { controller = "Manage", action = "EditAddress" },
                namespaces: new[] { "ShopDemo.Controllers" }
            );

            routes.MapRoute(
                name: "ManegeAddAddress",
                url: "ustawienia-konta/dodaj-adres",
                defaults: new { controller = "Manage", action = "AddAddress" },
                namespaces: new[] { "ShopDemo.Controllers" }
            );
            #endregion

            #region Account
            routes.MapRoute(
                name: "AccountLogin",
                url: "konto-uzytkownika/logowanie",
                defaults: new { controller = "Account", action = "Login" },
                namespaces: new[] { "ShopDemo.Controllers" }
            );

            routes.MapRoute(
                name: "AccountLocked",
                url: "konto-uzytkownika/konto-zablokowane",
                defaults: new { controller = "Account", action = "AccountIsLocked" },
                namespaces: new[] { "ShopDemo.Controllers" }
            );

            routes.MapRoute(
                name: "AccountMessageSent",
                url: "konto-uzytkownika/wiadomosc-wyslana",
                defaults: new { controller = "Account", action = "MessageWasSent" },
                namespaces: new[] { "ShopDemo.Controllers" }
            );

            routes.MapRoute(
                name: "AccountEmailNotConfirm",
                url: "konto-uzytkownika/email-niepotwierdzony",
                defaults: new { controller = "Account", action = "EmailIsNotConfirm" },
                namespaces: new[] { "ShopDemo.Controllers" }
            );

            routes.MapRoute(
                name: "AccountConfirmEmailInfo",
                url: "konto-uzytkownika/link-wyslany",
                defaults: new { controller = "Account", action = "ConfirmEmailInfo" },
                namespaces: new[] { "ShopDemo.Controllers" }
            );

            routes.MapRoute(
                name: "AccountRegister",
                url: "konto-uzytkownika/rejestracja",
                defaults: new { controller = "Account", action = "Register" },
                namespaces: new[] { "ShopDemo.Controllers" }
            );

            routes.MapRoute(
                name: "AccountRegisterInfo",
                url: "konto-uzytkownika/rejestracja-zakonczona",
                defaults: new { controller = "Account", action = "ConfirmAccountInfo" },
                namespaces: new[] { "ShopDemo.Controllers" }
            );

            routes.MapRoute(
                name: "AccountConfirmEmail",
                url: "konto-uzytkownika/potwierdzenie-adresu-email",
                defaults: new { controller = "Account", action = "ConfirmEmail" },
                namespaces: new[] { "ShopDemo.Controllers" }
            );

            routes.MapRoute(
                name: "AccountForgotPassword",
                url: "konto-uzytkownika/odzyskiwanie-hasla",
                defaults: new { controller = "Account", action = "ForgotPassword" },
                namespaces: new[] { "ShopDemo.Controllers" }
            );

            routes.MapRoute(
                name: "AccountForgotPasswordConfirmation",
                url: "konto-uzytkownika/potwierdzenie-odzyskiwania-hasla",
                defaults: new { controller = "Account", action = "ForgotPasswordConfirmation" },
                namespaces: new[] { "ShopDemo.Controllers" }
            );

            routes.MapRoute(
                name: "AccountResetPassword",
                url: "konto-uzytkownika/ustaw-nowe-haslo",
                defaults: new { controller = "Account", action = "ResetPassword" },
                namespaces: new[] { "ShopDemo.Controllers" }
            );

            routes.MapRoute(
                name: "AccountResetPasswordConfirm",
                url: "konto-uzytkownika/haslo-zresetowane",
                defaults: new { controller = "Account", action = "ResetPasswordConfirmation" },
                namespaces: new[] { "ShopDemo.Controllers" }
            );

            routes.MapRoute(
                name: "AccountLogOff",
                url: "konto-uzytkownika/wyloguj-sie",
                defaults: new { controller = "Account", action = "LogOff" },
                namespaces: new[] { "ShopDemo.Controllers" }
            );
            #endregion

            routes.MapRoute(
                name: "HomeContact",
                url: "kontakt",
                defaults: new { controller = "Home", action = "Contact" },
                namespaces: new[] { "ShopDemo.Controllers" }
            );

            routes.MapRoute(
                name: "HomeMessageSent",
                url: "kontakt/wiadomosc-wyslana",
                defaults: new { controller = "Home", action = "MessageSent" },
                namespaces: new[] { "ShopDemo.Controllers" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ShopDemo.Controllers" }
            );
        }
    }
}