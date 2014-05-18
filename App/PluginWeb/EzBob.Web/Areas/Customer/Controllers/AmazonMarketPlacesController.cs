﻿namespace EzBob.Web.Areas.Customer.Controllers
{
	using System;
	using System.Linq;
	using System.Web.Mvc;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Repository;
	using ExperianLib;
	using AmazonLib;
	using AmazonServiceLib;
	using AmazonServiceLib.ServiceCalls;
	using AmazonServiceLib.UserInfo;
	using Code.MpUniq;
	using Infrastructure;
	using Infrastructure.Attributes;
	using Infrastructure.csrf;
	using ServiceClientProxy;
	using Web.Models.Strings;
	using NHibernate;
	using log4net;
	using System.Data;
	using ConfigManager;

	public class AmazonMarketPlacesController : Controller
    {
        private readonly IEzbobWorkplaceContext _context;
        private readonly DatabaseDataHelper _helper;
		private readonly ServiceClient m_oServiceClient;
        private readonly ISession _session;
        private readonly CustomerMarketPlaceRepository _customerMarketPlaceRepository;
        private readonly AskvilleRepository _askvilleRepository;
        private readonly IMPUniqChecker _mpChecker;

        private static readonly ILog Log = LogManager.GetLogger(typeof(AmazonMarketPlacesController));
        private readonly AmazonServiceAskville _askvilleService;
        private readonly CustomerRepository _customerRepository;

        public AmazonMarketPlacesController(
            IEzbobWorkplaceContext context, 
            DatabaseDataHelper helper, 
            ISession session, 
            CustomerMarketPlaceRepository customerMarketPlaceRepository, 
            AskvilleRepository askvilleRepository,
            IMPUniqChecker mpChecker,
            CustomerRepository customerRepository)
        {
            _context = context;
            _helper = helper;
	        m_oServiceClient = new ServiceClient();
            _session = session;
            _customerMarketPlaceRepository = customerMarketPlaceRepository;
            _askvilleRepository = askvilleRepository;
            _mpChecker = mpChecker;
	        _askvilleService = new AmazonServiceAskville(CurrentValues.Instance.AmazonAskvilleLogin,
	                                                     CurrentValues.Instance.AmazonAskvillePassword);
	        _customerRepository = customerRepository;
        }
        //--------------------------------------------------------
        [Ajax]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
        [Authorize]
        public AskvilleStatus Askville(int? customerMarketPlaceId, string merchantId, string marketplaceId, string askvilleGuid = "")
        {
            var marketplace = _customerMarketPlaceRepository.GetAll().FirstOrDefault(x => x.Id == customerMarketPlaceId);
            if (marketplace == null)
            {
                throw new Exception(string.Format("Marketplace {0} does not found", customerMarketPlaceId));
            }

            var securityInfo = (AmazonSecurityInfo) RetrieveDataHelper.RetrieveCustomerSecurityInfo(marketplace.Id);

            merchantId = merchantId ?? securityInfo.MerchantId;
            marketplaceId = marketplaceId ?? securityInfo.MarketplaceId[0];

            var guid = askvilleGuid == "" ? Guid.NewGuid().ToString() : askvilleGuid;

            var acceptUrl = Url.Action("ActivateStore", "Home", new { Area = "", id = guid, approve = true.ToString().ToLower() }, "https");
            var disAcceptUrl = Url.Action("ActivateStore", "Home", new { Area = "", id = guid, approve = false.ToString().ToLower() }, "https");

            var message = @"
            Confirm your store on Amazon.

            Your Amazon shop has been added on EZBOB (www.ezbob.com).
            To confirm store please follow the link:
            " + acceptUrl + @"

            If you have not added the store on EZBOB please click the following link:
            " + disAcceptUrl + @"

            Thank you!

            Kindest regards,
            EZBOB team,
            www.ezbob.com
            contacts@ezbob.com
            +44.800.011.4787";
            
            var sendingStatus = _askvilleService.AskQuestion(merchantId, marketplaceId, 31, message);
            var askville = askvilleGuid == ""
                               ? (new Askville
                               {
                                   Guid = guid,
                                   IsPassed = false,
                                   MarketPlace = marketplace,
                                   SendStatus = sendingStatus,
                                   MessageBody = message,
                                   CreationDate = DateTime.UtcNow
                               })
                               : _askvilleRepository.GetAskvilleByGuid(askvilleGuid);

            askville.Status = askvilleGuid == "" ? AskvilleStatus.NotPerformed : AskvilleStatus.ReCheck;
            _askvilleRepository.SaveOrUpdate(askville);
			Utils.WriteLog("Send askville message", sendingStatus.ToString(), ExperianServiceType.Askville, marketplace.Customer.Id);
            return askville.Status;
        }
        
        //--------------------------------------------------------
        [HttpGet]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
        [Ajax]
        [ValidateJsonAntiForgeryToken]
        [Authorize]
        public string IsAmazonUserCorrect(string amazonMerchantId)
        {
			return RetrieveDataHelper.IsAmazonUserCorrect( new AmazonUserInfo { MerchantId = amazonMerchantId } ) ? "true" : "false";
        }

        //-------------------------------------------------------
        [HttpPost]
        [Authorize]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
        [Ajax]
        [ValidateJsonAntiForgeryToken]
        public JsonResult ConnectAmazon(string marketplaceId, string merchantId)
        {
            try
            {
                Log.InfoFormat("Adding Marketplace '{0}' to customer {1} with MerchantId '{2}'", marketplaceId, _context.User.Id, merchantId);

                if (string.IsNullOrEmpty(marketplaceId) || string.IsNullOrEmpty(merchantId))
                {
                    return Json(new{});
                }

                var customer = _context.Customer;

                var amazon = new AmazonDatabaseMarketPlace();
                
                var sellerInfo = AmazonRateInfo.GetUserRatingInfo(merchantId);

                _mpChecker.Check(amazon.InternalId, customer, sellerInfo.Name);

                var amazonSecurityInfo = new AmazonSecurityInfo(merchantId);
                amazonSecurityInfo.AddMarketplace(marketplaceId);

				var marketplace = _helper.SaveOrUpdateCustomerMarketplace(sellerInfo.Name, amazon, amazonSecurityInfo, customer, marketplaceId);

                _session.Flush();
                m_oServiceClient.Instance.UpdateMarketplace(_context.Customer.Id, marketplace.Id, true);

				if (CurrentValues.Instance.AskvilleEnabled)
                {
                    Askville(marketplace.Id, merchantId, marketplaceId);
                }

                _customerRepository.SaveOrUpdate(customer);

                return Json(new { msg = "Congratulations. Amazon account was linked successfully." });
            }
            catch (MarketPlaceAddedByThisCustomerException)
            {
                return Json(new { error = DbStrings.StoreAddedByYou }, JsonRequestBehavior.AllowGet);
            }
            catch (MarketPlaceIsAlreadyAddedException)
            {
                return Json(new { error = DbStrings.StoreAlreadyExistsInDb }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }
    }
}
