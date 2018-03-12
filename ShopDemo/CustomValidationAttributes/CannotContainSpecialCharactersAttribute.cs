using ShopDemo.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ShopDemo.CustomValidationAttributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class CannotContainSpecialCharactersAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            string pattern = @"[^A-Za-ząćęłńóśźżĄĘŁŃÓŚŹŻ\d\s\[\]\,\+]";
            bool isValid = true;

            if (value != null)
            {
                IEnumerable<ProductDetailViewModel> productAttributes = (IEnumerable<ProductDetailViewModel>)value;
                foreach (var item in productAttributes)
                {
                    Match result = Regex.Match(item.DetailValue, pattern);

                    if (result.Success)
                    {
                        isValid = false;
                        break;
                    }
                }
            }
            return isValid;
        }
    }
}