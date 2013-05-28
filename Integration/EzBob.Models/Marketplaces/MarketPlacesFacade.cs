using System.Collections.Generic;
using System.Linq;
using EZBob.DatabaseLib.Model.Database;
using EzBob.Models;
using EzBob.Web.Areas.Underwriter.Models;
using StructureMap;

namespace EzBob.Web.Areas.Underwriter
{
	public class MarketPlacesFacade
	{
		public IEnumerable<MarketPlaceModel> GetMarketPlaceModels(EZBob.DatabaseLib.Model.Database.Customer customer)
		{
			var marketplaces = customer.CustomerMarketPlaces.ToList();

			var models = marketplaces.Select(
				mp =>
				    {
				        var builder = GetBuilder(mp);
				        var model = builder.Create(mp);
                    model.PaymentAccountBasic = builder.GetPaymentAccountModel(mp, model);
				    return model;
				});
			return models;
		}

	    private static IMarketplaceModelBuilder GetBuilder(MP_CustomerMarketPlace mp)
	    {
	        var builder = ObjectFactory.TryGetInstance<IMarketplaceModelBuilder>(mp.Marketplace.GetType().ToString());
            return builder ?? ObjectFactory.GetNamedInstance<IMarketplaceModelBuilder>("DEFAULT");
	    }
	}
}