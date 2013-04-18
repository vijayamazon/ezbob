namespace EzBob.Web.Areas.Customer.Controllers
{
    using System;
    using System.Linq;
    using System.Web.Mvc;
    using ApplicationMng.Repository;
    using EZBob.DatabaseLib.Model.Database;
    using EZBob.DatabaseLib.Model.Database.Repository;
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
        private static readonly ILog _log = LogManager.GetLogger(typeof(EkmMarketPlacesController));
        private readonly IEzbobWorkplaceContext _context;
        private readonly ICustomerRepository _customers;
        private readonly IRepository<MP_MarketplaceType> _mpTypes;
        private readonly IRepository<MP_CustomerMarketPlace> _marketplaces;
        private Customer _customer;
        private readonly IMPUniqChecker _mpChecker;
        private readonly IAppCreator _appCreator;
        private readonly DatabaseDataHelper _helper;
        private readonly int payPointMarketTypeId;

        public PayPointMarketPlacesController(
            IEzbobWorkplaceContext context,
            DatabaseDataHelper helper, 
            ICustomerRepository customers, 
            IRepository<MP_MarketplaceType> mpTypes, 
            IRepository<MP_CustomerMarketPlace> marketplaces, 
            IMPUniqChecker mpChecker,
            IAppCreator appCreator)
        {
            _context = context;
            _helper = helper;
            _customers = customers;
            _mpTypes = mpTypes;
            _marketplaces = marketplaces;
            _customer = context.Customer;
            _mpChecker = mpChecker;
            _appCreator = appCreator;

            var oPsi = new PayPointServiceInfo();
            payPointMarketTypeId = _mpTypes
                .GetAll()
                .First(a => a.InternalId == oPsi.InternalId)
                .Id;
        }

        [Transactional]
        public JsonNetResult Accounts()
        {
            var payPoints = _customer.CustomerMarketPlaces.Where(mp => mp.Marketplace.Id == payPointMarketTypeId).Select(PayPointAccountModel.ToModel).ToList();
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
                return this.JsonNet(errorObject);
            }
            try
            {
                var customer = _context.Customer;
                var username = model.mid;
                var payPointDatabaseMarketPlace = new PayPointDatabaseMarketPlace();

                _mpChecker.Check(payPointDatabaseMarketPlace.InternalId, customer, username);

                var payPointSecurityInfo = new PayPointSecurityInfo(model.id, model.remotePassword, model.vpnPassword, model.mid);

                var payPoint = _helper.SaveOrUpdateCustomerMarketplace(username, payPointDatabaseMarketPlace, payPointSecurityInfo, customer);

                _appCreator.EbayAdded(customer, payPoint.Id); // qqq - should be different strategy
                return this.JsonNet(PayPointAccountModel.ToModel(_helper.GetExistsCustomerMarketPlace(username, payPointDatabaseMarketPlace, customer)));

                //return this.JsonNet(new { msg = "Congratulations. Your PayPoint was added successfully." });
            }
            catch (MarketPlaceAddedByThisCustomerException)
            {
                return this.JsonNet(new { error = DbStrings.StoreAddedByYou });
            }
            catch (MarketPlaceIsAlreadyAddedException)
            {
                return this.JsonNet(new { error = DbStrings.StoreAlreadyExistsInDb });
            }
            catch (Exception e)
            {
                _log.Error(e);
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