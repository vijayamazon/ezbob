using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EZBob.DatabaseLib.Model.Database.Repository;
using EzBob.Web.ApplicationCreator;
using EzBob.Web.Areas.Underwriter.Models;
using Scorto.Web;

namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
    public class MarketPlacesController : Controller
    {
        private readonly CustomerRepository _customers;
        private readonly AnalyisisFunctionValueRepository _functions;
        private readonly MarketPlacesFacade _marketPlaces;
        private readonly CustomerMarketPlaceRepository _customerMarketplaces;
        private readonly IAppCreator _appCreator;

        public MarketPlacesController(CustomerRepository customers, AnalyisisFunctionValueRepository functions, CustomerMarketPlaceRepository customerMarketplaces, MarketPlacesFacade marketPlaces, IAppCreator appCreator)
        {
            _customerMarketplaces = customerMarketplaces;
            _marketPlaces = marketPlaces;
            _appCreator = appCreator;
            _functions = functions;
            _customers = customers;
        }

        [Ajax]
        [HttpGet]
        [Transactional]
        public JsonNetResult Index(int id)
        {
            var customer = _customers.Get(id);
            var models = GetCustomerMarketplaces(customer);
            return this.JsonNet(models);
        }

        public IEnumerable<MarketPlaceModel> GetCustomerMarketplaces(EZBob.DatabaseLib.Model.Database.Customer customer)
        {
            return _marketPlaces.GetMarketPlaceModels(customer);
        }


        [Ajax]
        [HttpGet]
        [Transactional]
        public JsonNetResult Details(int id)
        {
            var cm = _customerMarketplaces.Get(id);
            var values = _functions.GetAllValuesFor(cm);
            return this.JsonNet(values.Select(v => new FunctionValueModel(v)));
        }

        [Ajax]
        public void ReCheckMarketplaces(int umi)
        {
            var mp = _customerMarketplaces.Get(umi);
            if (mp.UpdatingEnd == null && mp.UpdatingStart != null)
            {
                throw new Exception("Strategy already started");
            }

            var customer = mp.Customer;
            _customerMarketplaces.ClearUpdatingEnd(umi);
            switch(mp.Marketplace.Name)
            {
                case "Amazon":
                    _appCreator.AmazonAdded(customer, umi);
                    break;
                case "eBay": 
                case "EKM": 
                case "Volusion": 
                case "PayPoint": 
                    _appCreator.EbayAdded(customer, umi);
                    break;
                case "Pay Pal": 
                    _appCreator.PayPalAdded(customer, umi);
                    break;
            }
        }

        [Ajax]
        [HttpGet]
        public JsonNetResult CheckForUpdatedStatus(int mpId)
        {
            return this.JsonNet(new { status = _customerMarketplaces.GetUpdatedStatus(mpId) });
        }

        [Ajax]
        [HttpPost]
        public void RenewEbayToken(int umi)
        {
            var mp = _customerMarketplaces.Get(umi);
            var url = string.Format("https://app.ezbob.com/Customer/Profile/RenewEbayToken/");
            _appCreator.RenewEbayToken(mp.Customer, mp.DisplayName, url);
        }
    }
}
