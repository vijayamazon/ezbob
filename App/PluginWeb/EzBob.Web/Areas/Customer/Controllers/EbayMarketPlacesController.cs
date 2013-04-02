using System;
using System.Linq;
using System.Web.Mvc;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using EzBob.Web.ApplicationCreator;
using EzBob.Web.Areas.Customer.Models;
using EzBob.Web.Code.MpUniq;
using EzBob.Web.Infrastructure;
using EzBob.Web.Infrastructure.csrf;
using EzBob.Web.Models.Strings;
using EzBob.eBayLib;
using EzBob.eBayServiceLib;
using NHibernate;
using Scorto.Web;
using log4net;

namespace EzBob.Web.Areas.Customer.Controllers
{
    public class EbayMarketPlacesController : Controller
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(EbayMarketPlacesController));
        private readonly IEzbobWorkplaceContext _context;
        private readonly DatabaseDataHelper _helper;
        private readonly CustomerRepository _customers;
        private readonly ISession _session;
        private readonly eBayServiceHelper _eBayServiceHelper;
        private readonly IAppCreator _creator;
        private readonly IMPUniqChecker _mpChecker;

        public EbayMarketPlacesController(
            IEzbobWorkplaceContext context, 
            DatabaseDataHelper helper, 
            CustomerRepository customers, 
            ISession session, 
            eBayServiceHelper eBayServiceHelper, 
            IAppCreator creator,
            IMPUniqChecker mpChecker)
        {
            _context = context;
            _helper = helper;
            _customers = customers;
            _session = session;
            _eBayServiceHelper = eBayServiceHelper;
            _creator = creator;
            _mpChecker = mpChecker;
        }

        [Transactional]
        [Ajax]
        [HttpGet]
        [ValidateJsonAntiForgeryToken]
        public JsonNetResult Index()
        {
            var customer = _customers.Get(_context.User.Id);
            var ebay = new eBayDatabaseMarketPlace();
            var marketplaces = customer.CustomerMarketPlaces
                                            .Where(m => m.Marketplace.InternalId == ebay.InternalId)
                                            .Select(m => new SimpleMarketPlaceModel { displayName = m.DisplayName });
            return this.JsonNet(marketplaces);
        }

        public JsonResult CreateSessionId()
        {
            string sid = "";
            try
            {
                sid = _eBayServiceHelper.CreateSessionId();
                _log.InfoFormat("SID: '{0}' was generated", sid);
            }
            catch (Exception e)
            {
                _log.Error(e);
            }
            return Json(new { sid = sid }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CreateUrl(string sid, bool isUpdate = false)
        {
            string urlValue = "";
            try
            {
                var url = _eBayServiceHelper.CreateUrl(sid);
                urlValue = url.Value;
                _log.InfoFormat("Url: '{0}' was generated", urlValue);
            }
            catch (Exception e)
            {
                _log.Error(e);
            }
            return Json(new { url = urlValue }, JsonRequestBehavior.AllowGet);
        }

        public RedirectResult AttachEbay(bool isUpdate = false)
        {
            var sid = _eBayServiceHelper.CreateSessionId();
            var url = _eBayServiceHelper.CreateUrl(sid);
            TempData["SID"] = sid;
            TempData["isUpdate"] = isUpdate;
            return Redirect(url.Value);
        }

        [NoCache]
        [ValidateJsonAntiForgeryToken]
        public JsonResult CreateSidAndUrl()
        {
            string urlValue = "";
            string sid = "";
            try
            {
                sid = _eBayServiceHelper.CreateSessionId();
                _log.InfoFormat("SID: '{0}' was generated", sid);
                var url = _eBayServiceHelper.CreateUrl(sid);
                urlValue = url.Value;
                _log.InfoFormat("Url: '{0}' was generated", urlValue);
            }
            catch (Exception e)
            {
                _log.Error(e);
            }
            return Json(new { url = urlValue, sid = sid }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [JsonpFilter]
        public JsonResult FetchToken(string username)
        {
            try
            {
                var customer = _context.Customer;
                if (customer == null)
                {
                    _log.ErrorFormat("Customer is not authorized in system");
                    return Json(new { error = "Customer is not authorized in system" }, JsonRequestBehavior.AllowGet);
                }

                var ebay = new eBayDatabaseMarketPlace();

                _mpChecker.Check(ebay.InternalId, customer, username);

                var sid = TempData["SID"] as string;
                var isUpdate = TempData["isUpdate"] is bool && (bool) TempData["isUpdate"];

                if (string.IsNullOrEmpty(sid))
                {
                    _log.Error("Sid is empty");
                    return Json(new { error = "Username is empty" }, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(username))
                {
                    _log.Error("Username is empty");
                    return Json(new { error = "Username is empty" }, JsonRequestBehavior.AllowGet);
                }

                _log.InfoFormat("Saving sid {0} for username {1}", sid, username);
                var token = _eBayServiceHelper.FetchToken(sid);
                _log.InfoFormat("Token {0} was generated.", token);

                var eBaySecurityInfo = new eBaySecurityInfo {Token = token};

				var mp = _helper.SaveOrUpdateCustomerMarketplace( username, ebay, eBaySecurityInfo, customer );
                
                _session.Flush();

                if (! isUpdate)
                {
                    _creator.EbayAdded(_context.Customer, mp.Id);
                }

                if (customer.WizardStep != WizardStepType.PaymentAccounts || customer.WizardStep != WizardStepType.AllStep)
                    customer.WizardStep = WizardStepType.Marketplace;

                _customers.SaveOrUpdate(customer); 

                return Json(new { msg = string.Format("Congratulations. Your shop was {0} successfully.", isUpdate ? "updated" : "added") }, JsonRequestBehavior.AllowGet);
            }
            catch (MarketPlaceAddedByThisCustomerException e)
            {
                return Json(new { error = DbStrings.StoreAddedByYou }, JsonRequestBehavior.AllowGet);
            }
            catch (MarketPlaceIsAlreadyAddedException e)
            {
                return Json(new { error = DbStrings.StoreAlreadyExistsInDb }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                _log.Error(e);
                return Json(new { error = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ViewResult EbayConnected()
        {
            return View();
        }

    }
}
