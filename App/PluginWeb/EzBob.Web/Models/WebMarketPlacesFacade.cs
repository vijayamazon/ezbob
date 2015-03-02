namespace EzBob.Web.Models {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database;
	using EzBob.Models.Marketplaces;
	using EzBob.Models.Marketplaces.Builders;
	using Ezbob.Logger;
	using StructureMap;

	public class WebMarketPlacesFacade {
		public IEnumerable<MarketPlaceModel> GetMarketPlaceModels(Customer customer, DateTime? history) {
			List<MP_CustomerMarketPlace> marketplaces = history.HasValue
				? customer.CustomerMarketPlaces.Where(mp => mp.Created.HasValue && mp.Created.Value.Date <= history.Value.Date).ToList()
				: customer.CustomerMarketPlaces.ToList();

			List<MarketPlaceModel> models = marketplaces.Select(mp => {
				try {
					var builder = GetBuilder(mp);

					var model = builder.Create(mp, history);

					return model;
				}
				catch (Exception e) {
					new SafeILog(this).Warn(e, "Something went wrong while building marketplace model for marketplace id {0} of type {1}.", mp.Id, mp.Marketplace.Name);

					return new MarketPlaceModel {
						Id = mp.Id,
						Type = mp.DisplayName,
						Name = mp.Marketplace.Name,
						IsPaymentAccount = mp.Marketplace.IsPaymentAccount,
					};
				} // try
			}).ToList();

			return models;
		} // GetMarketPlaceModels

		public IEnumerable<MarketPlaceHistoryModel> GetMarketPlaceHistoryModel(Customer customer) {
			var modelsMpUpdates = customer
				.CustomerMarketPlaces
				.Where(mp => mp.Customer == customer)
				.SelectMany(mp => mp.UpdatingHistory)
				.Where(x => x.UpdatingEnd.HasValue)
				.ToList()
				.Select(h => h.UpdatingEnd.Value.Date)
				.Distinct()
				.Select(x => new MarketPlaceHistoryModel {
					HistoryDate = x,
					HistoryType = "MP Update"
				});

			var modelsCashRequests =
				customer.CashRequests.Where(cr => cr.CreationDate.HasValue)
						.Select(cr => cr.CreationDate.Value.Date)
						.Distinct()
						.Select(x => new MarketPlaceHistoryModel {
							HistoryDate = x,
							HistoryType = "Cash Request"
						});

			var models = new List<MarketPlaceHistoryModel>();
			models.AddRange(modelsMpUpdates);
			models.AddRange(modelsCashRequests);

			return models.OrderByDescending(m => m.HistoryDate);
		} // GetMarketPlaceHistoryModel

		private static IMarketplaceModelBuilder GetBuilder(MP_CustomerMarketPlace mp) {
			var builder = ObjectFactory.TryGetInstance<IMarketplaceModelBuilder>(mp.Marketplace.GetType().ToString());
			return builder ?? ObjectFactory.GetNamedInstance<IMarketplaceModelBuilder>("DEFAULT");
		} // GetBuilderr
	} // WebMarketPlacesFacade
} // namespace
