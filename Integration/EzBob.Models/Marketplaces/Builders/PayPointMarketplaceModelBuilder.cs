using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using EzBob.Web.Areas.Customer.Models;
using EzBob.Web.Areas.Underwriter.Models;

namespace EzBob.Models
{
    class PayPointMarketplaceModelBuilder : MarketplaceModelBuilder
    {
        public PayPointMarketplaceModelBuilder(CustomerMarketPlaceRepository customerMarketplaces) : base(customerMarketplaces)
        {
        }

        public override PaymentAccountsModel GetPaymentAccountModel(MP_CustomerMarketPlace mp, MarketPlaceModel model)
        {
            return new PaymentAccountsModel();
        }
    }
}