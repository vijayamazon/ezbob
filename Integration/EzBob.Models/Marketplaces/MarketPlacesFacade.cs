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
			var marketplaces = history.HasValue
								   ? customer.CustomerMarketPlaces.Where(mp => mp.Created.HasValue && mp.Created.Value.Date <= history.Value.Date).ToList()
								   : customer.CustomerMarketPlaces.ToList();


			var models = marketplaces.Select(mp =>
			{
				var builder = GetBuilder(mp);

				var model = builder.Create(mp, history);

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
			var modelsMpUpdates = customer
				.CustomerMarketPlaces
				.Where(mp => mp.Customer == customer)
				.SelectMany(mp => mp.UpdatingHistory)
				.Where(x => x.UpdatingEnd.HasValue)
				.ToList()
				.Select(h => h.UpdatingEnd.Value.Date)
				.Distinct()
				.Select(x => new MarketPlaceHistoryModel
					{
						HistoryDate = x,
						HistoryType = "MP Update"
					});

			var modelsCashRequests =
				customer.CashRequests.Where(cr => cr.CreationDate.HasValue)
						.Select(cr => cr.CreationDate.Value.Date)
						.Distinct()
						.Select(x => new MarketPlaceHistoryModel
							{
								HistoryDate = x,
								HistoryType = "Cash Request"
							});

			var models = new List<MarketPlaceHistoryModel>();
			models.AddRange(modelsMpUpdates);
			models.AddRange(modelsCashRequests);
			
			return models.OrderByDescending(m => m.HistoryDate);
		}

		private static IMarketplaceModelBuilder GetBuilder(MP_CustomerMarketPlace mp)
		{
			var builder = ObjectFactory.TryGetInstance<IMarketplaceModelBuilder>(mp.Marketplace.GetType().ToString());
			return builder ?? ObjectFactory.GetNamedInstance<IMarketplaceModelBuilder>("DEFAULT");
		} // GetBuilder
	} // MarketPlacesFacade
} // namespace
