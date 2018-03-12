using System;
using System.ComponentModel.DataAnnotations;

namespace ShopDemo.CustomValidationAttributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class MustBeTrueAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            return value is bool && (bool)value;
        }
    }
}