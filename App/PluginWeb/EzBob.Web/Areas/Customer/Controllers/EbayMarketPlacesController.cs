namespace EzBob.Web.Areas.Customer.Controllers {
    using System;
    using System.Linq;
    using System.Web.Mvc;
    using EZBob.DatabaseLib;
    using EZBob.DatabaseLib.Model.Database.Repository;
    using EzBob.Models;
    using Infrastructure.Attributes;
    using Code.MpUniq;
    using Infrastructure;
    using Infrastructure.csrf;
    using ServiceClientProxy;
    using Web.Models.Strings;
    using eBayLib;
    using eBayServiceLib;
    using EZBob.DatabaseLib.Model.Database;
    using NHibernate;
    using log4net;

    public class EbayMarketPlacesController : Controller {
        
        public EbayMarketPlacesController(
            IEzbobWorkplaceContext context,
            DatabaseDataHelper helper,
            CustomerRepository customers,
            ISession session,
            eBayServiceHelper eBayServiceHelper,
            IMPUniqChecker mpChecker) {
            this.context = context;
            this.helper = helper;
            this.customers = customers;
            this.session = session;
            this.eBayServiceHelper = eBayServiceHelper;
            this.serviceClient = new ServiceClient();
            this.mpChecker = mpChecker;
        }

        [Ajax]
        [HttpGet]
        [ValidateJsonAntiForgeryToken]
        public JsonResult Index() {
            var ebay = new eBayDatabaseMarketPlace();
            var marketplaces = this.context.Customer.CustomerMarketPlaces
                                            .Where(m => m.Marketplace.InternalId == ebay.InternalId)
                                            .Select(m => new SimpleMarketPlaceModel { displayName = m.DisplayName, MpId = m.Marketplace.Id, MpName = m.Marketplace.Name });
            return Json(marketplaces, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CreateSessionId() {
            string sid = "";
            try {
                sid = this.eBayServiceHelper.CreateSessionId(GetRuName());
                Log.InfoFormat("SID: '{0}' was generated", sid);
            } catch (Exception e) {
                Log.Error(e);
            }
            return Json(new { sid = sid }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CreateUrl(string sid, bool isUpdate = false) {
            string urlValue = "";
            try {
                string ruName = GetRuName();
                var url = this.eBayServiceHelper.CreateUrl(sid, ruName);
                urlValue = url.Value;
                Log.InfoFormat("Url: '{0}' was generated", urlValue);
            } catch (Exception e) {
                Log.Error(e);
            }
            return Json(new { url = urlValue }, JsonRequestBehavior.AllowGet);
        }

        public RedirectResult AttachEbay(bool isUpdate = false) {

            string ruName = GetRuName();
            var sid = this.eBayServiceHelper.CreateSessionId(ruName);
            var url = this.eBayServiceHelper.CreateUrl(sid, ruName);
            Log.InfoFormat("Url: '{0}' was generated", url.Value);
            TempData["SID"] = sid;
            TempData["isUpdate"] = isUpdate;
            return Redirect(url.Value);
        }

        [NoCache]
        [ValidateJsonAntiForgeryToken]
        public JsonResult CreateSidAndUrl() {
            string urlValue = "";
            string sid = "";
            
            try {
                string ruName = GetRuName();
                sid = this.eBayServiceHelper.CreateSessionId(ruName);
                Log.InfoFormat("SID: '{0}' was generated", sid);
                var url = this.eBayServiceHelper.CreateUrl(sid, ruName);
                urlValue = url.Value;
                Log.InfoFormat("Url: '{0}' was generated", urlValue);
            } catch (Exception e) {
                Log.Error(e);
            }
            return Json(new { url = urlValue, sid = sid }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [JsonpFilter]
        public JsonResult FetchToken(string username) {
            try {
                var customer = this.context.Customer;
                if (customer == null) {
                    Log.ErrorFormat("Customer is not authorized in system");
                    return Json(new { error = "Customer is not authorized in system" }, JsonRequestBehavior.AllowGet);
                }

                var ebay = new eBayDatabaseMarketPlace();

                var sid = TempData["SID"] as string;
                var isUpdate = TempData["isUpdate"] is bool && (bool)TempData["isUpdate"];

                if (string.IsNullOrEmpty(sid)) {
                    Log.Error("Sid is empty");
                    return Json(new { error = "Username is empty" }, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(username)) {
                    Log.Error("Username is empty");
                    return Json(new { error = "Username is empty" }, JsonRequestBehavior.AllowGet);
                }

                Log.InfoFormat("Saving sid {0} for username {1}", sid, username);
                var token = this.eBayServiceHelper.FetchToken(sid);
                Log.InfoFormat("Token {0} was generated.", token);

                var eBaySecurityInfo = new eBaySecurityInfo { Token = token };
                
                bool isValid = this.eBayServiceHelper.ValidateAccount(eBaySecurityInfo);
                if (!isValid) {
                    Log.WarnFormat("eBay account has not been activated yet");
                    return Json(new { error = "Your account has not been activated yet. Accounts are not accessible until an actual debit or credit has first been posted to the account, even though you may have already filled out our account creation form." }, JsonRequestBehavior.AllowGet);
                }
                
                this.mpChecker.Check(ebay.InternalId, customer, username);

                var mp = this.helper.SaveOrUpdateCustomerMarketplace(username, ebay, eBaySecurityInfo, customer);

                this.session.Flush();

                if (!isUpdate)
                    this.serviceClient.Instance.UpdateMarketplace(this.context.Customer.Id, mp.Id, true, this.context.UserId);

                this.customers.SaveOrUpdate(customer);

                return Json(new { msg = string.Format("Congratulations. Your shop was {0} successfully.", isUpdate ? "updated" : "added") }, JsonRequestBehavior.AllowGet);
            } catch (MarketPlaceAddedByThisCustomerException) {
                return Json(new { error = DbStrings.StoreAddedByYou }, JsonRequestBehavior.AllowGet);
            } catch (MarketPlaceIsAlreadyAddedException) {
                return Json(new { error = DbStrings.StoreAlreadyExistsInDb }, JsonRequestBehavior.AllowGet);
            } catch (Exception e) {
                Log.Error(e);
                return Json(new { error = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ViewResult EbayConnected() {
            return View();
        }

        [NonAction]
        private string GetRuName() {
            if (this.context.Customer.CustomerOrigin.IsEverline()) {
                return ConfigManager.CurrentValues.Instance.EbayRuNameEverline.Value;
            }

            if (this.context.Customer.CustomerOrigin.IsAlibaba()) {
                return ConfigManager.CurrentValues.Instance.EbayRuNameAlibaba.Value;
            }

            return ConfigManager.CurrentValues.Instance.EbayRuName.Value;
        }

        private static readonly ILog Log = LogManager.GetLogger(typeof(EbayMarketPlacesController));
        private readonly IEzbobWorkplaceContext context;
        private readonly DatabaseDataHelper helper;
        private readonly CustomerRepository customers;
        private readonly ISession session;
        private readonly eBayServiceHelper eBayServiceHelper;
        private readonly ServiceClient serviceClient;
        private readonly IMPUniqChecker mpChecker;

    }
}
