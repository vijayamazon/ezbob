using ZohoCRM;

namespace EzBob.Web.Areas.Customer.Controllers
{
    using System;
    using System.Linq;
    using System.Web.Mvc;
    using ApplicationMng.Repository;
    using EZBob.DatabaseLib.Model.Database;
    using Infrastructure;
    using Scorto.Web;
    using PayPoint;
    using Code.MpUniq;
    using Web.Models.Strings;
    using log4net;
    using ApplicationCreator;
    using EZBob.DatabaseLib;
    using CommonLib;

    public class PayPointMarketPlacesController: Controller
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PayPointMarketPlacesController));
        private readonly IEzbobWorkplaceContext _context;
        private readonly IRepository<MP_MarketplaceType> _mpTypes;
        private readonly Customer _customer;
        private readonly IMPUniqChecker _mpChecker;
        private readonly IAppCreator _appCreator;
        private readonly DatabaseDataHelper _helper;
        private readonly int _payPointMarketTypeId;
        private readonly IZohoFacade _crm;

        public PayPointMarketPlacesController(
            IEzbobWorkplaceContext context,
            DatabaseDataHelper helper, 
            IRepository<MP_MarketplaceType> mpTypes, 
            IMPUniqChecker mpChecker,
            IAppCreator appCreator, IZohoFacade crm)
        {
            _context = context;
            _helper = helper;
            _mpTypes = mpTypes;
            _customer = context.Customer;
            _mpChecker = mpChecker;
            _appCreator = appCreator;
            _crm = crm;

            var payPointServiceInfo = new PayPointServiceInfo();
            _payPointMarketTypeId = _mpTypes.GetAll().First(a => a.InternalId == payPointServiceInfo.InternalId).Id;
        }

        [Transactional]
        public JsonNetResult Accounts()
        {
            var payPoints = _customer.CustomerMarketPlaces.Where(mp => mp.Marketplace.Id == _payPointMarketTypeId).Select(PayPointAccountModel.ToModel).ToList();
            return this.JsonNet(payPoints);
        }

        [Transactional]
        [Ajax]
        [HttpPost]
        public JsonNetResult Accounts(PayPointAccountModel model)
        {
            string errorMsg;
            if (!PayPointConnector.Validate(model.mid, model.vpnPassword, model.remotePassword, out errorMsg))
            {
				var errorObject = new { error = errorMsg };
				Log.ErrorFormat("PayPoint validation failed: {0}", errorObject);
                return this.JsonNet(errorObject);
            }
            try
            {
                var customer = _context.Customer;
                var username = model.mid;
                var payPointDatabaseMarketPlace = new PayPointDatabaseMarketPlace();

                _mpChecker.Check(payPointDatabaseMarketPlace.InternalId, customer, username);

                var payPointSecurityInfo = new PayPointSecurityInfo(model.id, model.remotePassword, model.vpnPassword, model.mid);

				if (customer.WizardStep != WizardStepType.PaymentAccounts && customer.WizardStep != WizardStepType.AllStep)
					customer.WizardStep = WizardStepType.Marketplace;
                var payPoint = _helper.SaveOrUpdateCustomerMarketplace(username, payPointDatabaseMarketPlace, payPointSecurityInfo, customer);
                _crm.ConvertLead(customer);
                _appCreator.CustomerMarketPlaceAdded(customer, payPoint.Id);
                return this.JsonNet(PayPointAccountModel.ToModel(_helper.GetExistsCustomerMarketPlace(username, payPointDatabaseMarketPlace, customer)));
            }
            catch (MarketPlaceAddedByThisCustomerException e)
			{
				Log.Error(e);
				return this.JsonNet(new { error = DbStrings.AccountAddedByYou });
            }
            catch (MarketPlaceIsAlreadyAddedException e)
			{
				Log.Error(e);
                return this.JsonNet(new { error = DbStrings.StoreAlreadyExistsInDb });
            }
            catch (Exception e)
            {
                Log.Error(e);
                return this.JsonNet(new { error = e.Message });
            }
        }
    }

    public class PayPointAccountModel
    {
        public int id { get; set; }
        public string mid { get; set; }
        public string vpnPassword { get; set; }
        public string remotePassword { get; set; }
        public string displayName { get { return mid; } }

        public static PayPointAccountModel ToModel(MP_CustomerMarketPlace account)
        {
            var payPointSecurityInfo = SerializeDataHelper.DeserializeType<PayPointSecurityInfo>(account.SecurityData);

            return new PayPointAccountModel
                {
                    id = payPointSecurityInfo.MarketplaceId,
                    mid = payPointSecurityInfo.Mid,
                    vpnPassword = payPointSecurityInfo.VpnPassword,
                    remotePassword = payPointSecurityInfo.RemotePassword
                };
        }
    }
}