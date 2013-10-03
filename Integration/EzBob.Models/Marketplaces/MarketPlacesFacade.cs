namespace EzBob.Models.Marketplaces
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database;
	using Builders;
	using StructureMap;

	public class MarketPlacesFacade
	{
		public IEnumerable<MarketPlaceModel> GetMarketPlaceModels(Customer customer, DateTime? history)
		{
			var marketplaces = customer.CustomerMarketPlaces.ToList();

			var models = marketplaces.Select(mp =>
			{
				var builder = GetBuilder(mp);

				var model = builder.Create(mp);

				model.PaymentAccountBasic = builder.GetPaymentAccountModel(mp, model, history);

				return model;
			});

			return models;
		} // GetMarketPlaceModels


		public DateTime MarketplacesSeniority(Customer customer, bool onlyForEluminationPassed = false, bool? isPaymentAccount = null)
		{
			var marketplaces = customer.CustomerMarketPlaces.Where(m => m.Disabled == false).ToList();

			if (onlyForEluminationPassed)
			{
				marketplaces = marketplaces.Where(m => m.EliminationPassed).ToList();
			}

			if (isPaymentAccount != null)
			{
				marketplaces = marketplaces.Where(m => m.Marketplace.IsPaymentAccount == isPaymentAccount.Value).ToList();
			}

			var dates = marketplaces.Where(mp => !mp.Marketplace.IsPaymentAccount || mp.Marketplace.Name == "Pay Pal").Select(mp =>
			{
				var builder = GetBuilder(mp);
				builder.UpdateOriginationDate(mp);
				return mp.OriginationDate;
			}).Where(d => d != null).Select(d => d.Value).ToList();

			return dates.Any() ? dates.Min() : DateTime.UtcNow;
		}

		public IEnumerable<MarketPlaceHistoryModel> GetMarketPlaceHistoryModel(Customer customer)
		{
			var models = customer
				.CustomerMarketPlaces
				.ToList()
				.Where(mp => mp.Customer == customer)
				.SelectMany(mp => mp.UpdatingHistory)
				.ToList()
				.Where(x => x.UpdatingEnd.HasValue)
				.Select(x => new MarketPlaceHistoryModel
					{
						HistoryDate = x.UpdatingEnd.Value,
						HistoryId = x.Id
					});


			return models;
		}

		private static IMarketplaceModelBuilder GetBuilder(MP_CustomerMarketPlace mp)
		{
			var builder = ObjectFactory.TryGetInstance<IMarketplaceModelBuilder>(mp.Marketplace.GetType().ToString());
			return builder ?? ObjectFactory.GetNamedInstance<IMarketplaceModelBuilder>("DEFAULT");
		} // GetBuilder
	} // MarketPlacesFacade
} // namespace
