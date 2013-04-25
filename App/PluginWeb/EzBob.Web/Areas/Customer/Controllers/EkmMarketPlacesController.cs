using System;
using System.Linq;
using System.Web.Mvc;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database;
using EzBob.Web.Infrastructure;
using Scorto.Web;
using EKM;
using EzBob.Web.Code.MpUniq;
using EzBob.Web.Models.Strings;
using log4net;
using EzBob.CommonLib.Security;
using EzBob.Web.ApplicationCreator;
using NHibernate;

namespace EzBob.Web.Areas.Customer.Controllers
{
    public class EkmMarketPlacesController : Controller
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(EkmMarketPlacesController));
        private readonly IEzbobWorkplaceContext _context;
        private readonly IRepository<MP_MarketplaceType> _mpTypes;
        private readonly EZBob.DatabaseLib.Model.Database.Customer _customer;
        private readonly IMPUniqChecker _mpChecker;
        private readonly IAppCreator _appCreator;
        private readonly EkmConnector _validator = new EkmConnector();
        private readonly ISession _session;

        public EkmMarketPlacesController(
            IEzbobWorkplaceContext context,
            IRepository<MP_MarketplaceType> mpTypes,
            IMPUniqChecker mpChecker,
            IAppCreator appCreator,
            ISession session)
        {
            _context = context;
            _mpTypes = mpTypes;
            _customer = context.Customer;
            _mpChecker = mpChecker;
            _appCreator = appCreator;
            _session = session;
        }

        [Transactional]
        public JsonNetResult Accounts()
        {
            var oEsi = new EkmServiceInfo();

            var ekms = _customer
                .CustomerMarketPlaces
                .Where(mp => mp.Marketplace.InternalId == oEsi.InternalId)
                .Select(EkmAccountModel.ToModel)
                .ToList();
            return this.JsonNet(ekms);
        }

        [Transactional]
        [Ajax]
        [HttpPost]
        public JsonNetResult Accounts(EkmAccountModel model)
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
                var oEsi = new EkmServiceInfo();
                int marketPlaceId = _mpTypes
                    .GetAll()
                    .First(a => a.InternalId == oEsi.InternalId)
                    .Id;

                var mp = new MP_CustomerMarketPlace
                             {
                                 Marketplace = _mpTypes.Get(marketPlaceId),
                                 DisplayName = model.login,
                                 SecurityData = Encryptor.EncryptBytes(model.password),
                                 Customer = _customer,
                                 Created = DateTime.UtcNow,
                                 UpdatingStart = DateTime.UtcNow,
                                 Updated = DateTime.UtcNow,
                                 UpdatingEnd = DateTime.UtcNow
                             };

                _customer.CustomerMarketPlaces.Add(mp);
                _session.Flush();
                _appCreator.EbayAdded(customer, mp.Id);
                return this.JsonNet(EkmAccountModel.ToModel(mp));
            }
            catch (MarketPlaceAddedByThisCustomerException e)
            {
                Log.Debug(e);
                return this.JsonNet(new { error = DbStrings.StoreAddedByYou });
            }
            catch (MarketPlaceIsAlreadyAddedException e)
            {
                Log.Debug(e);
                return this.JsonNet(new { error = DbStrings.StoreAlreadyExistsInDb });
            }
            catch (Exception e)
            {
                Log.Error(e);
                return this.JsonNet(new { error = e.Message });
            }
        }
    }

    public class EkmAccountModel
    {
        public int id { get; set; }
        public string login { get; set; }
        public string password { get; set; }
        public string displayName { get { return login; } }

        public static EkmAccountModel ToModel(MP_CustomerMarketPlace account)
        {
            return new EkmAccountModel()
                       {
                           id = account.Id,
                           login = account.DisplayName,
                           password = Encryptor.Decrypt(account.SecurityData)
                       };
        }
    }
}