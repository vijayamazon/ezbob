using System;
using System.Linq;
using System.Web.Mvc;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using EzBob.Web.Code;
using EzBob.Web.Infrastructure;
using Scorto.Web;

namespace EzBob.Web.Areas.Customer.Controllers
{
    public class EkmAccountsController: Controller
    {
        private readonly IEzbobWorkplaceContext _context;
        private readonly ICustomerRepository _customers;
        private readonly IRepository<MP_MarketplaceType> _mpTypes;
        private readonly IRepository<MP_CustomerMarketPlace> _marketplaces;
        private EZBob.DatabaseLib.Model.Database.Customer _customer;

        public EkmAccountsController(IEzbobWorkplaceContext context, ICustomerRepository customers, IRepository<MP_MarketplaceType> mpTypes, IRepository<MP_CustomerMarketPlace> marketplaces)
        {
            _context = context;
            _customers = customers;
            _mpTypes = mpTypes;
            _marketplaces = marketplaces;
            _customer = context.Customer;
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

            return this.JsonNet(EKMAccountModel.ToModel(mp));
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