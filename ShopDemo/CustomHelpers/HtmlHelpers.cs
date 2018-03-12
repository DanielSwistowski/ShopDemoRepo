using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace ShopDemo.CustomHelpers
{
    public static class HtmlHelpers
    {
        public static MvcHtmlString AccountStatusToStringPL(this HtmlHelper htmlHelper, bool status)
        {
            string text = status ? "Aktywne" : "Zablokowane";
            return new MvcHtmlString(text);
        }

        public static MvcHtmlString YesNoToStringPL(this HtmlHelper htmlHelper, bool value)
        {
            string text = value ? "Tak" : "Nie";
            return new MvcHtmlString(text);
        }

        public static MvcHtmlString NonStyledRolesList(this HtmlHelper htmlHelper, IEnumerable<string> roles)
        {
            string text = "";
            bool isMoreThanOneRole = false;
            foreach (var role in roles)
            {
                if (isMoreThanOneRole)
                    text += "<br/>" + role;
                else
                {
                    text += role;
                    isMoreThanOneRole = true;
                }
            }
            return new MvcHtmlString(text);
        }

        public static MvcHtmlString GetEnumDisplayName(this HtmlHelper htmlHelper, Enum item)
        {
            var type = item.GetType();
            var member = type.GetMember(item.ToString());
            DisplayAttribute displayName = (DisplayAttribute)member[0].GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault();

            if (displayName != null)
            {
                return new MvcHtmlString(displayName.Name);
            }

            return new MvcHtmlString(item.ToString());
        }
    }
}