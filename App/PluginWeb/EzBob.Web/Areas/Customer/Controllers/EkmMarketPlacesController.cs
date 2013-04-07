using System;
using System.Linq;
using System.Web.Mvc;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using EzBob.Web.Code;
using EzBob.Web.Infrastructure;
using Scorto.Web;
using EKM;
using EzBob.Web.Code.MpUniq;
using EzBob.Web.Models.Strings;
using log4net;
using EzBob.CommonLib.Security;
using EzBob.Web.ApplicationCreator;

namespace EzBob.Web.Areas.Customer.Controllers
{
    public class EkmMarketPlacesController: Controller
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(EkmMarketPlacesController));
        private readonly IEzbobWorkplaceContext _context;
        private readonly ICustomerRepository _customers;
        private readonly IRepository<MP_MarketplaceType> _mpTypes;
        private readonly IRepository<MP_CustomerMarketPlace> _marketplaces;
        private EZBob.DatabaseLib.Model.Database.Customer _customer;
        private readonly IMPUniqChecker _mpChecker;
        private readonly IAppCreator _appCreator;
        private readonly EkmConnector _validator = new EkmConnector();

        public EkmMarketPlacesController(
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
            var ekms = _customer.CustomerMarketPlaces.Where(mp => mp.Marketplace.Id == 4).Select(a => EKMAccountModel.ToModel(a)).ToList();
            return this.JsonNet(ekms);
        }

        [Transactional]
        [Ajax]
        [HttpPost]
        public JsonNetResult Accounts(EKMAccountModel model)
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
                var ekm = new EkmDatabaseMarketPlace();
                _mpChecker.Check(ekm.InternalId, customer, username);
                var mp = new MP_CustomerMarketPlace
                             {
                                 Marketplace = _mpTypes.Get(4),
                                 DisplayName = model.login,
                                 SecurityData = Encryptor.EncryptBytes(model.password),
                                 Customer = _customer,
                                 Created = DateTime.UtcNow,
                                 UpdatingStart = DateTime.UtcNow,
                                 Updated = DateTime.UtcNow,
                                 UpdatingEnd = DateTime.UtcNow
                             };

                _customer.CustomerMarketPlaces.Add(mp);
                _appCreator.EbayAdded(customer, mp.Id);
                return this.JsonNet(EKMAccountModel.ToModel(mp));
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

    public class EKMAccountModel
    {
        public int id { get; set; }
        public string login { get; set; }
        public string password { get; set; }
        public string displayName { get { return login; } }

        public static EKMAccountModel ToModel(MP_CustomerMarketPlace account)
        {
            return new EKMAccountModel()
                       {
                           id = account.Id,
                           login = account.DisplayName,
                           password = Encryptor.Decrypt(account.SecurityData)
                       };
        }
    }
}