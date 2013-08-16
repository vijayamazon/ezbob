using EZBob.DatabaseLib;
using EzBob.Configuration;
using EZBob.DatabaseLib.Model.Database;
using EzBob.Models;
using EzBob.Signals.ZohoCRM;
using EzBob.Web.Areas.Underwriter;
using EzBob.Web.Code.Agreements;
using ZohoCRM;

namespace EzBob.Web.Code
{
    public class ZohoFacadeSignaled : ZohoFacade
    {
        public ZohoFacadeSignaled(IZohoConfig config, DatabaseDataHelper helper, MarketPlacesFacade marketPlacesFacade,
                                  ProfileSummaryModelBuilder profileSummaryModelBuilder,
                                  AgreementRenderer agreementRenderer)
            : base(config, helper, marketPlacesFacade, profileSummaryModelBuilder, agreementRenderer)
        {
        }

        public override void ConvertLead(Customer customer)
        {
            new ZohoSignalClient(customer.Id, ZohoMethodType.ConvertLead).Execute();
        }

        public override void RegisterLead(Customer customer)
        {
            new ZohoSignalClient(customer.Id, ZohoMethodType.RegisterLead).Execute();
        }

        public override void UpdateOrCreate(Customer customer)
        {
            new ZohoSignalClient(customer.Id, ZohoMethodType.UpdateOrCreate).Execute();
        }
    }
}