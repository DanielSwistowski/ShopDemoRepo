using System;
using System.Globalization;
using System.Web.Mvc;

namespace ShopDemo.CustomModelBinders
{
    public class DateTimeModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            try
            {
                return DateTime.ParseExact(value.AttemptedValue, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
            }
            catch
            {
                return base.BindModel(controllerContext, bindingContext);
            }
        }
    }
}