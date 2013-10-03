using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.Model.Database;
using EzBob.Web.Areas.Customer.Models;

namespace EzBob.Models.Marketplaces.Builders
{
	using System;

	public interface IMarketplaceModelBuilder
    {
		PaymentAccountsModel GetPaymentAccountModel(MP_CustomerMarketPlace mp, MarketPlaceModel model, DateTime? history = null);
        string GetUrl(MP_CustomerMarketPlace mp, IMarketPlaceSecurityInfo securityInfo);
        MarketPlaceModel Create(MP_CustomerMarketPlace mp, DateTime? history = null);
        void UpdateOriginationDate(MP_CustomerMarketPlace mp);
    }
}