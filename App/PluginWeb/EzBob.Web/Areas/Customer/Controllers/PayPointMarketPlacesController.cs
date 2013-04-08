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
        private readonly PayPointConnector _validator = new PayPointConnector();

        public PayPointMarketPlacesController(
            IEzbobWorkplaceContext context, 
            ICustomerRepository customers, 
            IRepository<MP_MarketplaceType> mpTypes, 
            IRepository<MP_CustomerMarketPlace> marketplaces, 
            IMPUniqChecker mpChecker,
            IAppCreator appCreator)
        {
            _context = context;
            _customers = customers;
            _mpTypes = mpTypes;
            _marketplaces = marketplaces;
            _customer = context.Customer;
            _mpChecker = mpChecker;
            _appCreator = appCreator;
        }

        [Transactional]
        public JsonNetResult Accounts()
        {
            // 7 should be marketplace id from db
            var payPoints = _customer.CustomerMarketPlaces.Where(mp => mp.Marketplace.Id == 7).Select(a => PayPointAccountModel.ToModel(a)).ToList();
            return this.JsonNet(payPoints);
        }

        [Transactional]
        [Ajax]
        [HttpPost]
        public JsonNetResult Accounts(PayPointAccountModel model)
        {
            string errorMsg;
            if (!_validator.Validate(model.login, model.password, out errorMsg))
            {
                var errorObject = new { error = errorMsg };
                return this.JsonNet(errorObject);
            }
            try
            {
                var customer = _context.Customer;
                var username = model.login;
                var payPoint = new PayPointDatabaseMarketPlace();
                _mpChecker.Check(ekm.InternalId, customer, username);
                var mp = new MP_CustomerMarketPlace
                             {
                                 Marketplace = _mpTypes.Get(7), // 7 should be marketplace id from db
                                 DisplayName = model.login,
                                 SecurityData = Encryptor.EncryptBytes(model.password),
                                 Customer = _customer,
                                 Created = DateTime.UtcNow,
                                 UpdatingStart = DateTime.UtcNow,
                                 Updated = DateTime.UtcNow,
                                 UpdatingEnd = DateTime.UtcNow
                             };

                _customer.CustomerMarketPlaces.Add(mp);
                _appCreator.EbayAdded(customer, mp.Id); // should be different strategy
                return this.JsonNet(PayPointAccountModel.ToModel(mp));
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


        public static PayPointAccountModel ToModel(MP_CustomerMarketPlace account)
        {
            return new PayPointAccountModel()
                       {
                           id = account.Id,
                           mid = account.DisplayName,
                           password = Encryptor.Decrypt(account.SecurityData),
                           remotePassword = Encryptor.Decrypt(account.SecurityData) // should be another password
                       };
        }
    }
}