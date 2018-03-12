using Postal;

namespace ShopDemo.ViewModels
{
    public class AccountManagementEmail : Email
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string Link { get; set; }
    }

    public class AdminMessageEmail : Email
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
    }

    public class OrderInfoEmail : Email
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string OrderDetailsUrl { get; set; }
    }

    public class AutoCancelOrderEmail : Email
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
    }

    public class EmailToAdminEmail : Email
    {
        public string To { get; set; }
        public string From { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
    }
}