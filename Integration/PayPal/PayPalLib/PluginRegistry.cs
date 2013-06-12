using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper;
using EzBob.PayPalDbLib;
using EzBob.PayPalServiceLib;
using StructureMap.Configuration.DSL;

namespace EzBob.PayPal
{
	using EZBob.DatabaseLib.PyaPalDetails;

	public class PluginRegistry : Registry
	{
		public PluginRegistry()
		{
		    var paypal = new PayPalServiceInfo();
            For<IMarketplaceType>().Use<PayPalDatabaseMarketPlace>().Named(paypal.DisplayName);
			For<DatabaseMarketplaceBase<PayPalDatabaseFunctionType>>().Use<PayPalDatabaseMarketPlace>();
            For<IMarketplaceRetrieveDataHelper>().Use<PayPalRetriveDataHelper>().Named(paypal.DisplayName);
			For<IPayPalAggregationFormulaRepository>().Use<PayPalAggregationFormulaRepository>();
		}
	}
}