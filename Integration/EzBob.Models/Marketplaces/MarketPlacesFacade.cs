namespace EzBob.Models.Marketplaces {
	using System;
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database;
	using Builders;
	using StructureMap;

	public class MarketPlacesFacade {
		public DateTime MarketplacesSeniority(Customer customer, bool? isPaymentAccount = null) {
			var marketplaces = customer.CustomerMarketPlaces.Where(m => m.Disabled == false).ToList();
			
			if (isPaymentAccount != null)
				marketplaces = marketplaces.Where(m => m.Marketplace.IsPaymentAccount == isPaymentAccount.Value).ToList();

			var dates = marketplaces.Where(mp => !mp.Marketplace.IsPaymentAccount || mp.Marketplace.Name == "Pay Pal").Select(mp => {
				var builder = GetBuilder(mp);
				builder.UpdateOriginationDate(mp);
				return mp.OriginationDate;
			}).Where(d => d != null).Select(d => d.Value).ToList();

			return dates.Any() ? dates.Min() : DateTime.UtcNow;
		} // MarketplacesSeniority

		private static IMarketplaceModelBuilder GetBuilder(MP_CustomerMarketPlace mp) {
			var builder = ObjectFactory.TryGetInstance<IMarketplaceModelBuilder>(mp.Marketplace.GetType().ToString());
			return builder ?? ObjectFactory.GetNamedInstance<IMarketplaceModelBuilder>("DEFAULT");
		} // GetBuilder
	} // MarketPlacesFacade
} // namespace
