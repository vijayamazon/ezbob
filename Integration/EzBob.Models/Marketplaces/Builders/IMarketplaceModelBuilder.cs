using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.Model.Database;
using EzBob.Web.Areas.Customer.Models;

namespace EzBob.Models.Marketplaces.Builders
{
    public interface IMarketplaceModelBuilder
    {
        PaymentAccountsModel GetPaymentAccountModel(MP_CustomerMarketPlace mp, MarketPlaceModel model);
        string GetUrl(MP_CustomerMarketPlace mp, IMarketPlaceSecurityInfo securityInfo);
        MarketPlaceModel Create(MP_CustomerMarketPlace mp);
        void UpdateOriginationDate(MP_CustomerMarketPlace mp);
    }
}