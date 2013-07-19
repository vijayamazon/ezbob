using EZBob.DatabaseLib.PayPal;
using StructureMap.Configuration.DSL;

namespace EzBob.PayPalDbLib
{
    public class PayPalRegistry : Registry
    {
        public PayPalRegistry()
        {
            For<IPayPalDetailsRepository>().Use<PayPalDetailsRepository>();
        }
    }
}