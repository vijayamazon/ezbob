using System.Linq;
using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using EzBob.Web.Areas.Customer.Models;
using EzBob.Web.Areas.Underwriter.Models;

namespace EzBob.Models
{
    class PayPalMarketplaceModelBuilder : MarketplaceModelBuilder
    {
        public PayPalMarketplaceModelBuilder(CustomerMarketPlaceRepository customerMarketplaces) : base(customerMarketplaces)
        {
        }

        public override PaymentAccountsModel GetPaymentAccountModel(MP_CustomerMarketPlace mp, MarketPlaceModel model)
        {
            return model.PayPal.GeneralInfo;
        }

        public override string GetUrl(MP_CustomerMarketPlace mp, IMarketPlaceSecurityInfo securityInfo)
        {
            var url = base.GetUrl(mp, securityInfo);
            if (mp.EbayUserData == null) return url;
            var mpEbayUserData = mp.EbayUserData.LastOrDefault();
            if (mpEbayUserData == null || mpEbayUserData.SellerInfo == null) return url;
            if (string.IsNullOrEmpty(mpEbayUserData.SellerInfo.SellerInfoStoreURL)) return url;
            return mpEbayUserData.SellerInfo.SellerInfoStoreURL;
        }

        protected override void InitializeSpecificData(MP_CustomerMarketPlace mp, MarketPlaceModel model)
        {
            model.PayPal = PayPalModelBuilder.Create(mp);
        }
    }
}