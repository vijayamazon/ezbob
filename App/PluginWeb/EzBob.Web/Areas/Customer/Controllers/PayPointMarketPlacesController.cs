using System;
using System.Linq;
using System.Web.Mvc;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using EzBob.Web.Code;
using EzBob.Web.Infrastructure;
using Scorto.Web;
using PayPoint;
using EzBob.Web.Code.MpUniq;
using EzBob.Web.Models.Strings;
using log4net;
using EzBob.CommonLib.Security;
using EzBob.Web.ApplicationCreator;
using EZBob.DatabaseLib;
using EzBob.CommonLib;
using EzBob.CommonLib.Security;

namespace EzBob.Web.Areas.Customer.Controllers
{
    public class PayPointMarketPlacesController: Controller
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(EkmMarketPlacesController));
        private readonly IEzbobWorkplaceContext _context;
        private readonly ICustomerRepository _customers;
        private readonly IRepository<MP_MarketplaceType> _mpTypes;
        private readonly IRepository<MP_CustomerMarketPlace> _marketplaces;
        private EZBob.DatabaseLib.Model.Database.Customer _customer;
        private readonly IMPUniqChecker _mpChecker;
        private readonly IAppCreator _appCreator;
        private readonly DatabaseDataHelper _helper;
        private int payPointMarketTypeId;

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
            payPointMarketTypeId = 5; // qqq - SELECT Id FROM MP_MarketPlaceType WHERE Name = 'PayPoint'
        }

        [Transactional]
        public JsonNetResult Accounts()
        {
            var payPoints = _customer.CustomerMarketPlaces.Where(mp => mp.Marketplace.Id == payPointMarketTypeId).Select(a => PayPointAccountModel.ToModel(a)).ToList();
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
                var payPoint1 = new PayPointDatabaseMarketPlace();

                _mpChecker.Check(payPoint1.InternalId, customer, username);

                var payPointSecurityInfo = new PayPointSecurityInfo(model.id, model.remotePassword, model.vpnPassword, model.mid);

                var payPoint = _helper.SaveOrUpdateCustomerMarketplace(username, payPoint1, payPointSecurityInfo, customer);


                /* all of this code happens in SaveOrUpdateCustomerMarketplace (qqq - i should make sure)
                var mp = new MP_CustomerMarketPlace
                             {
                                 Marketplace = _mpTypes.Get(payPointMarketTypeId),
                                 DisplayName = model.mid,
                                 SecurityData = Encryptor.EncryptBytes(model.vpnPassword), // qqq what does it mean?
                                 // probably should add column to MP_CustomerMarketPlace to hold the second password...
                                 Customer = _customer,
                                 Created = DateTime.UtcNow,
                                 UpdatingStart = DateTime.UtcNow,
                                 Updated = DateTime.UtcNow,
                                 UpdatingEnd = DateTime.UtcNow
                             };

                _customer.CustomerMarketPlaces.Add(mp);*/
                _appCreator.EbayAdded(customer, payPoint.Id); // qqq - should be different strategy
                //return this.JsonNet(PayPointAccountModel.ToModel(payPoint.Marketplace));

                return this.JsonNet(new { msg = "Congratulations. Your PayPoint was added successfully." });
            }
            catch (MarketPlaceAddedByThisCustomerException e)
            {
                return this.JsonNet(new { error = DbStrings.StoreAddedByYou });
            }
            catch (MarketPlaceIsAlreadyAddedException e)
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
            var i = SerializeDataHelper.DeserializeType<PayPointSecurityInfo>(account.SecurityData);
             
            return new PayPointAccountModel()
                       {
                           id = i.MarketplaceId,
                           mid = i.Mid,
                           vpnPassword = i.VpnPassword,
                           remotePassword = i.RemotePassword
                       };
        }
    }
}