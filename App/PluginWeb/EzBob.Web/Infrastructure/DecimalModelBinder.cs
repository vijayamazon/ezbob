using System;
using System.Globalization;
using System.Web.Mvc;
using log4net;

namespace EzBob.Web.Infrastructure
{
    public class DecimalModelBinder : DefaultModelBinder
    {

        private static readonly ILog log = LogManager.GetLogger("EzBob.Web.Infrastructure.DecimalModelBinder");

        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            if (valueProviderResult == null) return base.BindModel(controllerContext, bindingContext);

            if (valueProviderResult.AttemptedValue.Equals("N.aN") ||
                valueProviderResult.AttemptedValue.Equals("NaN") ||
                valueProviderResult.AttemptedValue.Equals("Infini.ty") ||
                valueProviderResult.AttemptedValue.Equals("Infinity") ||
                string.IsNullOrEmpty(valueProviderResult.AttemptedValue))
                return 0m;

            decimal val = 0;

            if ( !Decimal.TryParse(valueProviderResult.AttemptedValue, NumberStyles.Number, CultureInfo.InvariantCulture, out val))
            {
                var msg = string.Format("Cannot convert value '{0}' to decimal. ModelName is {1}", valueProviderResult.AttemptedValue, bindingContext.ModelName);
                log.Error(msg);
            }

            return val;
        }
    }
}