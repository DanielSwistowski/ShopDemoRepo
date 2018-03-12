using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ShopDemo.CustomValidationAttributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class CannotContainsEmptyElementsAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var list = value as IEnumerable<int>;
            if (list == null)
            {
                return false;
            }
            else
            {
                bool itemValueIsZero = false;
                foreach (var item in list)
                {
                    if (item == 0)
                    {
                        itemValueIsZero = true;
                        break;
                    }
                }
                if (itemValueIsZero)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
    }
}