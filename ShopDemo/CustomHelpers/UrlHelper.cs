using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace ShopDemo.CustomHelpers
{
    public static class UrlHelper
    {
        public static string ToSeoUrl(this string url)
        {
            if (url != null)
            {
                var sb = new StringBuilder(Regex.Replace(HttpUtility.HtmlDecode(url).RemoveAccent(), @"[^0-9a-zA-Z:/]+", "-").Trim());
                return sb.ToString().ToLower();
            }
            else
            {
                return url;
            }
        }

        public static string RemoveAccent(this string s)
        {
            return Encoding.ASCII.GetString(Encoding.GetEncoding("Cyrillic").GetBytes(s));
        }
    }
}