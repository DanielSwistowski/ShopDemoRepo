using System.Web.Mvc;

namespace ShopDemo.Areas
{
    internal static class RouteConfig
    {
        internal static void RegisterRoutes(AreaRegistrationContext context)
        {
            #region Product
            context.MapRoute(
                "Admin_Produkty",
                "admin/produkty/aktualna-oferta/{categoryId}/{categoryName}",
                defaults: new { controller = "Product", action = "Index", categoryId = UrlParameter.Optional, categoryName = UrlParameter.Optional },
                constraints: new { categoryId = @"\d+" });

            context.MapRoute(
                "Admin_Product_Index",
                "admin/produkty/aktualna-oferta",
                defaults: new { controller = "Product", action = "Index" });

            context.MapRoute(
                "Admin_ProduktyPromocyjne",
                "admin/produkty/promocje/{categoryId}/{categoryName}",
                defaults: new { controller = "Product", action = "ProductsOnPromotion", categoryId = UrlParameter.Optional, categoryName = UrlParameter.Optional },
                constraints: new { categoryId = @"\d+" });

            context.MapRoute(
                "Admin_ProductsOnPromotion",
                "admin/produkty/promocje",
                defaults: new { controller = "Product", action = "ProductsOnPromotion" });

            context.MapRoute(
                "Admin_ProductsDeletedFromOffer",
                "admin/produkty/wycofane-ze-sprzedazy/{categoryId}/{categoryName}",
                defaults: new { controller = "Product", action = "ProductsDeletedFromOffer", categoryId = UrlParameter.Optional, categoryName = UrlParameter.Optional },
                constraints: new { categoryId = @"\d+" });

            context.MapRoute(
                "Admin_ProductsDeletedFromOffer_Menu",
                "admin/produkty/wycofane-ze-sprzedazy",
                defaults: new { controller = "Product", action = "ProductsDeletedFromOffer" });

            context.MapRoute(
                "Admin_Details",
                "admin/produkty/szczegoly-produktu/{productId}/{productName}",
                defaults: new { controller = "Product", action = "ProductDetails", productId = UrlParameter.Optional, productName = UrlParameter.Optional });

            context.MapRoute(
                "Admin_AddProdukt",
                "admin/produkty/nowy-produkt",
                defaults: new { controller = "Product", action = "AddProduct" });

            context.MapRoute(
                "Admin_EditProdukt",
                "admin/produkty/edytuj-produkt/{productId}",
                defaults: new { controller = "Product", action = "EditProduct", productId = UrlParameter.Optional });

            context.MapRoute(
                "Admin_Delete_Produkt",
                "admin/produkty/usun-produkt/{productId}",
                defaults: new { controller = "Product", action = "DeleteProduct", productId = UrlParameter.Optional });

            context.MapRoute(
                "Admin_Delete_Produkt_From_Offer",
                "admin/produkty/wycofaj-ze-sprzedazy/{productId}",
                defaults: new { controller = "Product", action = "DeleteProductFromOffer", productId = UrlParameter.Optional });

            context.MapRoute(
                "Admin_Add_Produkt_From_Offer",
                "admin/produkty/przywroc-do-sprzedazy/{productId}",
                defaults: new { controller = "Product", action = "AddProductToOffer", productId = UrlParameter.Optional });
            #endregion

            context.MapRoute(
                "Admin_Categories",
                "admin/kategorie",
                defaults: new { controller = "Category", action = "Index" });

            #region ProductDiscount
            context.MapRoute(
                "Admin_Discount_Details",
                "admin/promocje/szczegoly-promocji/{productId}",
                defaults: new { controller = "ProductDiscount", action = "Details", productId = UrlParameter.Optional },
                constraints: new { productId = @"\d+" });

            context.MapRoute(
                "Admin_Add_Discount",
                "admin/promocje/nowa-promocja/{productId}",
                defaults: new { controller = "ProductDiscount", action = "Add", productId = UrlParameter.Optional },
                constraints: new { productId = @"\d+" });

            context.MapRoute(
                "Admin_Edit_Discount",
                "admin/promocje/edytuj-promocje/{productId}",
                defaults: new { controller = "ProductDiscount", action = "EditProductDiscount", productId = UrlParameter.Optional },
                constraints: new { productId = @"\d+" });

            context.MapRoute(
                "Admin_Delete_Discount",
                "admin/promocje/usun-promocje/{productId}",
                defaults: new { controller = "ProductDiscount", action = "DeleteProductDiscount", productId = UrlParameter.Optional },
                constraints: new { productId = @"\d+" });
            #endregion

            #region User
            context.MapRoute(
                "Admin_Users",
                "admin/uzykownicy",
                defaults: new { controller = "User", action = "Index" });

            context.MapRoute(
                "Admin_Users_Details",
                "admin/uzykownicy/informacje-o-uzytkowniku/{id}",
                defaults: new { controller = "User", action = "Details", id = UrlParameter.Optional });

            context.MapRoute(
                "Admin_Users_LockUser",
                "admin/uzykownicy/zablokuj-konto-uzytkownika/{id}",
                defaults: new { controller = "User", action = "LockUserAccount", id = UrlParameter.Optional });

            context.MapRoute(
                "Admin_Users_UnlockUser",
                "admin/uzykownicy/odblokuj-konto-uzytkownika/{id}",
                defaults: new { controller = "User", action = "UnlockUserAccount", id = UrlParameter.Optional });

            context.MapRoute(
                "Admin_Users_Send_Message",
                "admin/uzykownicy/wiadomosc-do-uzytkownika",
                defaults: new { controller = "User", action = "SendMessage" });
            #endregion


            #region Delivery
            context.MapRoute(
                "Admin_Add_New_Delivery",
                "admin/opcje-dostawy/nowy-sposob-dostawy",
                defaults: new { controller = "Delivery", action = "AddNewDelivery" });

            context.MapRoute(
                "Admin_Delete_Delivery",
                "admin/opcje-dostawy/usun-sposob-dostawy/{deliveryId}",
                defaults: new { controller = "Delivery", action = "DeleteDelivery", deliveryId = UrlParameter.Optional },
                constraints: new { deliveryId = @"\d+" });

            context.MapRoute(
                "Admin_Deactivate_Delivery_Option",
                "admin/opcje-dostawy/deaktywuj-sposob-dostawy/{deliveryId}",
                defaults: new { controller = "Delivery", action = "DeactivateDeliveryOption", deliveryId = UrlParameter.Optional },
                constraints: new { deliveryId = @"\d+" });

            context.MapRoute(
                "Admin_Activate_Delivery_Option",
                "admin/opcje-dostawy/aktywuj-sposob-dostawy/{deliveryId}",
                defaults: new { controller = "Delivery", action = "ActivateDeliveryOption", deliveryId = UrlParameter.Optional },
                constraints: new { deliveryId = @"\d+" });

            context.MapRoute(
                "Admin_Delivery_Index",
                "admin/opcje-dostawy",
                defaults: new { controller = "Delivery", action = "Index" });
            #endregion

            context.MapRoute(
                "Admin_Log",
                "admin/log",
                defaults: new { controller = "Log", action = "Index" });

            #region Order
            context.MapRoute(
                "Admin_Order_Details",
                "admin/zamowienia/szczegoly-zamowienia/{orderId}",
                defaults: new { controller = "Order", action = "Details", orderId = UrlParameter.Optional },
                constraints: new { orderId = @"\d+" });

            context.MapRoute(
                "Admin_Realize_Order",
                "admin/zamowienia/realizuj-zamowienie/{orderId}",
                defaults: new { controller = "Order", action = "RealizeOrder", orderId = UrlParameter.Optional },
                constraints: new { orderId = @"\d+" });

            context.MapRoute(
                "Admin_Realize_Order_Success",
                "admin/zamowienia/realizacja-zamowienia-zakonczona/{orderId}",
                defaults: new { controller = "Order", action = "RealizeOrderSuccess", orderId = UrlParameter.Optional },
                constraints: new { orderId = @"\d+" });

            context.MapRoute(
                "Admin_Bill",
                "admin/zamowienia/rachunek/{orderId}",
                defaults: new { controller = "Order", action = "PrintBill", orderId = UrlParameter.Optional },
                constraints: new { orderId = @"\d+" });

            context.MapRoute(
                "Admin_Cancel_Order",
                "admin/zamowienia/anuluj-zamowienie/{orderId}",
                defaults: new { controller = "Order", action = "CancelOrder", orderId = UrlParameter.Optional },
                constraints: new { orderId = @"\d+" });            

            context.MapRoute(
                "Admin_User_Orders",
                "admin/zamowienia/zamowienia-uzytkownika/{userId}/{userEmail}",
                defaults: new { controller = "Order", action = "UserOrders", userId = UrlParameter.Optional, userEmail = UrlParameter.Optional },
                constraints: new { userId = @"\d+" });

            context.MapRoute(
                "Admin_Month_Orders",
                "admin/zamowienia/zamowienia-dla-miesiaca/{monthName}/{year}",
                defaults: new { controller = "Order", action = "GetOrdersForSelectedMonth", monthName = UrlParameter.Optional, year = UrlParameter.Optional });

            context.MapRoute(
                "Admin_Orders",
                "admin/zamowienia",
                defaults: new { controller = "Order", action = "Index" });
            #endregion

            context.MapRoute(
                "Admin_Accept_Payment",
                "admin/platnosci/zaakceptuj-platnosc/{orderId}",
                defaults: new { controller = "Payu", action = "AcceptPayment", orderId = UrlParameter.Optional },
                constraints: new { orderId = @"\d+" });

            #region Statistics
            context.MapRoute(
                "Admin_Most_Sold_Products",
                "admin/statystyki/najczesciej-sprzedawane-produkty/{take}/{monthName}/{year}",
                defaults: new { controller = "Statistics", action = "MostSoldProducts", take = UrlParameter.Optional, monthName = UrlParameter.Optional, year = UrlParameter.Optional });

            context.MapRoute(
                "Admin_Top_Rated_Products",
                "admin/statystyki/najlepiej-oceniane-produkty/{take}",
                defaults: new { controller = "Statistics", action = "TopRatedProducts", take = UrlParameter.Optional });

            context.MapRoute(
                "Admin_Best_Customers",
                "admin/statystyki/najlepsi-kupujacy/{take}",
                defaults: new { controller = "Statistics", action = "BestCustomers", take = UrlParameter.Optional });

            context.MapRoute(
                "Admin_Best_Orders",
                "admin/statystyki/najwieksze-zrealizowane-zamowienia/{take}",
                defaults: new { controller = "Statistics", action = "BestOrders", take = UrlParameter.Optional });

            context.MapRoute(
                "Admin_Product_Order_History",
                "admin/statystyki/historia-zamowien-produktu/{productName}/{productId}",
                defaults: new { controller = "Statistics", action = "ProductOrderHistory", productName = UrlParameter.Optional, productId = UrlParameter.Optional });

            context.MapRoute(
                "Admin_Best_Delivery_Options",
                "admin/statystyki/najczesciej-wybierane-sposoby-dostawy",
                defaults: new { controller = "Statistics", action = "BestDeliveryOptions" });

            context.MapRoute(
                "Admin_Sales_Summary",
                "admin/statystyki/podsumowanie-sprzedazy",
                defaults: new { controller = "Statistics", action = "SalesSummary" });

            context.MapRoute(
                "Admin_Statistics",
                "admin/statystyki",
                defaults: new { controller = "Statistics", action = "Index" });
            #endregion

            context.MapRoute(
                "Admin_default",
                "admin/{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}