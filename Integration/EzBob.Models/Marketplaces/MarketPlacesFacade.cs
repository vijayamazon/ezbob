using System;
using System.Collections.Generic;
using System.Linq;
using EZBob.DatabaseLib.Model.Database;
using EzBob.Web.Areas.Underwriter.Models;
using EzBob.Models.Marketplaces.Builders;
using StructureMap;

namespace EzBob.Web.Areas.Underwriter {
	

	public class MarketPlacesFacade {
	    public IEnumerable<MarketPlaceModel> GetMarketPlaceModels(EZBob.DatabaseLib.Model.Database.Customer customer) {
			var marketplaces = customer.CustomerMarketPlaces.ToList();

			var models = marketplaces.Select(mp => {
				var builder = GetBuilder(mp);

				var model = builder.Create(mp);

				model.PaymentAccountBasic = builder.GetPaymentAccountModel(mp, model);

				return model;
			});

			return models;
		} // GetMarketPlaceModels


        public DateTime MarketplacesSeniority(EZBob.DatabaseLib.Model.Database.Customer customer, bool onlyForEluminationPassed = false, bool? isPaymentAccount = null)
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

	    private static IMarketplaceModelBuilder GetBuilder(MP_CustomerMarketPlace mp) {
			var builder = ObjectFactory.TryGetInstance<IMarketplaceModelBuilder>(mp.Marketplace.GetType().ToString());
			return builder ?? ObjectFactory.GetNamedInstance<IMarketplaceModelBuilder>("DEFAULT");
		} // GetBuilder
	} // MarketPlacesFacade
} // namespace
