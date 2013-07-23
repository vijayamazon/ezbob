using System;
using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.Model.Database;
using EzBob.Web.Areas.Customer.Models;
using EzBob.Web.Areas.Underwriter.Models;

namespace EzBob.Models
{
    public interface IMarketplaceModelBuilder
    {
        PaymentAccountsModel GetPaymentAccountModel(MP_CustomerMarketPlace mp, MarketPlaceModel model);
        string GetUrl(MP_CustomerMarketPlace mp, IMarketPlaceSecurityInfo securityInfo);
        MarketPlaceModel Create(MP_CustomerMarketPlace mp);
        DateTime? GetSeniority(MP_CustomerMarketPlace mp);
    }
}