using System;
using System.Collections.Generic;
using System.Linq;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using EzBob.Models;
using EzBob.Web.Areas.Underwriter.Models;
using StructureMap;

namespace EzBob.Web.Areas.Underwriter {
	public class MarketPlacesFacade {
	    private readonly ICustomerRepository _customers;

	    public MarketPlacesFacade(ICustomerRepository customers)
	    {
	        _customers = customers;
	    }

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


        public DateTime MarketplacesSeniority(int id, bool onlyForEluminationPassed = false)
        {
            var customer = _customers.Get(id);
            var marketplaces = customer.CustomerMarketPlaces.ToList();

            if (onlyForEluminationPassed)
            {
                marketplaces = marketplaces.Where(m => m.EliminationPassed).ToList();
            }

            var dates = marketplaces.Select(mp =>
            {
                var builder = GetBuilder(mp);
                return builder.GetSeniority(mp);
            }).Where(d => d != null).Select(d => d.Value).ToList();

            return dates.Any() ? dates.Min() : DateTime.UtcNow;
        }

	    private static IMarketplaceModelBuilder GetBuilder(MP_CustomerMarketPlace mp) {
			var builder = ObjectFactory.TryGetInstance<IMarketplaceModelBuilder>(mp.Marketplace.GetType().ToString());
			return builder ?? ObjectFactory.GetNamedInstance<IMarketplaceModelBuilder>("DEFAULT");
		} // GetBuilder
	} // MarketPlacesFacade
} // namespace
