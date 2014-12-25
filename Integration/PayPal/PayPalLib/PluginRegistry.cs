namespace EzBob.PayPal {
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.DatabaseWrapper;
	using EzBob.PayPalServiceLib;
	using StructureMap.Configuration.DSL;

	using EZBob.DatabaseLib.PayPal;

	public class PluginRegistry : Registry {
		public PluginRegistry() {
			var paypal = new PayPalServiceInfo();
			For<IMarketplaceType>().Use<PayPalDatabaseMarketPlace>().Named(paypal.DisplayName);
			For<IMarketplaceRetrieveDataHelper>().Use<PayPalRetriveDataHelper>().Named(paypal.DisplayName);
			For<IPayPalAggregationFormulaRepository>().Use<PayPalAggregationFormulaRepository>();
		}
	}
}