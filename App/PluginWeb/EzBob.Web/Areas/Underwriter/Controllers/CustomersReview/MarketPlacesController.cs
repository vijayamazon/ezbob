namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Repository;
	using EzBob.Models.Marketplaces.Builders;
	using Ezbob.Utils.Serialization;
	using Infrastructure;
	using Infrastructure.Attributes;
	using Models;
	using EzBob.Models.Marketplaces;
	using NHibernate;
	using Ezbob.Utils.Security;
	using EZBob.DatabaseLib.Model.Marketplaces.Yodlee;
	using Newtonsoft.Json;
	using ServiceClientProxy;
	using Web.Models;
	using YodleeLib;
	using YodleeLib.connector;
	using log4net;
	using ActionResult = System.Web.Mvc.ActionResult;

	public class MarketPlacesController : Controller
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(MarketPlacesController));
		private readonly CustomerRepository _customers;
		private readonly AnalyisisFunctionValueRepository _functions;
		private readonly WebMarketPlacesFacade _marketPlaces;
		private readonly CustomerMarketPlaceRepository _customerMarketplaces;
		private readonly MP_TeraPeakOrderItemRepository _teraPeakOrderItems;
		private readonly YodleeAccountsRepository _yodleeAccountsRepository;
		private readonly YodleeSearchWordsRepository _yodleeSearchWordsRepository;
		private readonly YodleeGroupRepository _yodleeGroupRepository;
		private readonly YodleeRuleRepository _yodleeRuleRepository;
		private readonly YodleeGroupRuleMapRepository _yodleeGroupRuleMapRepository;
		private readonly ISession _session;
		private readonly ServiceClient m_oServiceClient;
		private readonly CompanyFilesMetaDataRepository _companyFiles;
		private readonly IWorkplaceContext _context;

		public MarketPlacesController(CustomerRepository customers,
			AnalyisisFunctionValueRepository functions,
			CustomerMarketPlaceRepository customerMarketplaces,
			WebMarketPlacesFacade marketPlaces,
			MP_TeraPeakOrderItemRepository teraPeakOrderItems,
			YodleeAccountsRepository yodleeAccountsRepository,
			YodleeSearchWordsRepository yodleeSearchWordsRepository,
			YodleeGroupRepository yodleeGroupRepository,
			YodleeRuleRepository yodleeRuleRepository,
			YodleeGroupRuleMapRepository yodleeGroupRuleMapRepository,
			ISession session, 
			CompanyFilesMetaDataRepository companyFiles, 
			IWorkplaceContext context)
		{
			_customerMarketplaces = customerMarketplaces;
			_marketPlaces = marketPlaces;
			m_oServiceClient = new ServiceClient();
			_functions = functions;
			_customers = customers;
			_teraPeakOrderItems = teraPeakOrderItems;
			_yodleeAccountsRepository = yodleeAccountsRepository;
			_yodleeSearchWordsRepository = yodleeSearchWordsRepository;
			_yodleeGroupRepository = yodleeGroupRepository;
			_yodleeRuleRepository = yodleeRuleRepository;
			_yodleeGroupRuleMapRepository = yodleeGroupRuleMapRepository;
			_session = session;
			_companyFiles = companyFiles;
			_context = context;
		}

		[Ajax]
		[HttpGet]
		public JsonResult Index(int id, DateTime? history = null)
		{
			try
			{
				var ar = m_oServiceClient.Instance.CalculateModelsAndAffordability(_context.UserId, id, history);
				var mps = JsonConvert.DeserializeObject<MarketPlaceModel[]>(ar.Models);
				return Json(mps.ToList(), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				Log.Error("failed to retrieve mp data from service", ex);
				return Json(new List<MarketPlaceModel>(), JsonRequestBehavior.AllowGet); 
			}
		}
		
		[Ajax]
		[HttpGet]
		public JsonResult GetAffordabilityData(int id) {
			var ar = m_oServiceClient.Instance.CalculateModelsAndAffordability(_context.UserId, id, null);
			return Json(ar.Affordability, JsonRequestBehavior.AllowGet);
		} // GetAffordabilityData

		[Ajax]
		[HttpGet]
		public JsonResult GetTeraPeakOrderItems(int customerMarketPlaceId)
		{
			var data = _teraPeakOrderItems.GetTeraPeakOrderItems(customerMarketPlaceId);
			return Json(data.Select(item => new Double?[2] { (ToUnixTimestamp(item.StartDate) + ToUnixTimestamp(item.EndDate)) / 2, item.Revenue }).Cast<object>().ToArray(), JsonRequestBehavior.AllowGet);

		}

		public static long ToUnixTimestamp(DateTime d)
		{
			var duration = d - new DateTime(1970, 1, 1, 0, 0, 0);
			return (long)duration.TotalSeconds * 1000;
		}


		[HttpGet]
		public JsonResult GetCustomerMarketplacesHistory(int customerId)
		{
			var customer = _customers.Get(customerId);
			var models = _marketPlaces.GetMarketPlaceHistoryModel(customer);
			return Json(models, JsonRequestBehavior.AllowGet);
		}


		[Ajax]
		[HttpGet]
		public JsonResult Details(int id)
		{
			var cm = _customerMarketplaces.Get(id);
			var values = _functions.GetAllValuesFor(cm);
			return Json(values.Select(v => new FunctionValueModel(v)), JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpGet]
		public JsonResult YodleeDetails(int id)
		{
			var mp = _customerMarketplaces.Get(id);
			var b = new YodleeMarketplaceModelBuilder(_session);
			return Json(b.BuildYodlee(mp, null), JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[Transactional]
		public void ReCheckMarketplaces(int umi)
		{
			var mp = _customerMarketplaces.Get(umi);

			string currentState = (string)_session.CreateSQLQuery(string.Format("EXEC GetLastMarketplaceStatus {0}, {1}", mp.Customer.Id, mp.Id)).UniqueResult();
			if (currentState == "In progress" || currentState == "BG launch")
				throw new Exception("Strategy already started");

			var customer = mp.Customer;

			switch (mp.Marketplace.Name)
			{
				case "Amazon":
				case "eBay":
				case "EKM":
				case "FreeAgent":
				case "Sage":
				case "PayPoint":
				case "Pay Pal":
					m_oServiceClient.Instance.UpdateMarketplace(customer.Id, umi, true, _context.UserId);
					break;

				default:
					if (null != Integration.ChannelGrabberConfig.Configuration.Instance.GetVendorInfo(mp.Marketplace.Name))
						m_oServiceClient.Instance.UpdateMarketplace(customer.Id, umi, true, _context.UserId);
					break;
			} // switch
		} // ReCheckMarketplaces

		[Transactional]
		public ActionResult TryRecheckYodlee(int umi)
		{
			var mp = _customerMarketplaces.Get(umi);
			var yodleeMain = new YodleeMain();
			var yodleeAccount = _yodleeAccountsRepository.Search(mp.Customer.Id);

			if (yodleeAccount == null)
			{
				return View(new { error = "Yodlee Account was not found" });
			}

			var securityInfo = Serialized.Deserialize<YodleeSecurityInfo>(mp.SecurityData);
			long itemId = securityInfo.ItemId;
			var lu = yodleeMain.LoginUser(yodleeAccount.Username, Encrypted.Decrypt(yodleeAccount.Password));
			if (lu == null)
			{
				return View(new { error = "Error Loging to Yodlee Account" });
			}

			if (!yodleeMain.IsMFA(itemId))
			{
				bool isRefreshed;
				try
				{
					isRefreshed = yodleeMain.RefreshNotMFAItem(itemId);
				}
				catch (RefreshYodleeException ex)
				{
					Log.WarnFormat("TryRecheckYodlee exception {0}", ex);
					return View(new { error = ex.ToString() });
				}
				if (isRefreshed)
				{
					var customer = mp.Customer;
					m_oServiceClient.Instance.UpdateMarketplace(customer.Id, umi, true, _context.UserId);
					return View(new { success = true });
				}

				return View(new { error = "Account wasn't refreshed successfully" });
			}
			else //MFA Account for testing redirecting to Yodlee LAW
			{
				var callback = Url.Action("YodleeCallback", "YodleeRecheck", new { Area = "Underwriter" }, "https") + "/" + umi;
				string finalUrl = yodleeMain.GetEditAccountUrl(securityInfo.ItemId, callback, yodleeAccount.Username, Encrypted.Decrypt(yodleeAccount.Password));
				return Redirect(finalUrl);
			}
		} // TryRecheckYodlee

		[Ajax]
		[HttpGet]
		public JsonResult CheckForUpdatedStatus(int mpId)
		{
			return Json(new { status = _customerMarketplaces.Get(mpId).GetUpdatingStatus() }, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpPost]
		public void RenewEbayToken(int umi)
		{
			var mp = _customerMarketplaces.Get(umi);

			m_oServiceClient.Instance.RenewEbayToken(
				_context.UserId,
				mp.Customer.Id,
				mp.DisplayName,
				"https://app.ezbob.com/Customer/Profile/RenewEbayToken/"
			);
		}

		[Ajax]
		[HttpPost]
		[Transactional]
		public JsonResult Disable(int umi)
		{
			var mp = _customerMarketplaces.Get(umi);
			mp.Disabled = true;
			return Json(new { });
		}

		[Ajax]
		[HttpPost]
		[Transactional]
		public JsonResult Enable(int umi)
		{
			var mp = _customerMarketplaces.Get(umi);
			mp.Disabled = false;
			return Json(new { });
		}

		[Transactional]
		[Ajax]
		public void AddSearchWord(string word)
		{
			_yodleeSearchWordsRepository.AddWord(word);
		}

		[Transactional]
		[Ajax]
		public void AddYodleeRule(int group, int rule, string literal)
		{
			Log.DebugFormat("{0} {1} {2}", group, rule, literal);
			var oGroup = _yodleeGroupRepository.Get(group);
			var oRule = _yodleeRuleRepository.Get(rule);

			if (oGroup != null && oRule != null)
			{
				if (
					(!_yodleeGroupRuleMapRepository.GetAll()
												   .Any(
													   x =>
													   x.Group == oGroup && x.Rule == oRule &&
													   x.Rule.Id != (int)YodleeRule.IncludeLiteralWord &&
													   x.Rule.Id != (int)YodleeRule.DontIncludeLiteralWord)) ||
					(!_yodleeGroupRuleMapRepository.GetAll()
												   .Any(
													   x =>
													   x.Group == oGroup && x.Rule == oRule &&
													   (x.Rule.Id == (int)YodleeRule.IncludeLiteralWord ||
														x.Rule.Id == (int)YodleeRule.DontIncludeLiteralWord) &&
													   x.Literal == literal.Trim().ToLowerInvariant())))
				{
					_yodleeGroupRuleMapRepository.Save(new MP_YodleeGroupRuleMap
						{
							Group = oGroup,
							Rule = oRule,
							Literal = literal.Trim().ToLowerInvariant()
						});

					var t = new YodleeTransactionRepository(_session);
					t.RemoveEzbobCategorization();
				}
			}
		}

		[Transactional]
		[Ajax]
		public void DeleteSearchWord(string word)
		{
			_yodleeSearchWordsRepository.DeleteWord(word);
		}

		public FileResult DownloadCompanyFile(int fileId)
		{
			var file = m_oServiceClient.Instance.GetCompanyFile(_context.UserId, fileId);
			var fileMetaData = _companyFiles.Get(fileId);
			if (file != null && fileMetaData != null)
			{
				FileResult fs = new FileContentResult(file, fileMetaData.FileContentType);
				fs.FileDownloadName = fileMetaData.FileName;
				return fs;
			}
			return null;
		}
	}
}
