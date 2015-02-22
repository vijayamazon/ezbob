namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Web;
	using System.Web.Mvc;
	using BankTransactionsParser;
	using Code;
	using CompanyFiles;
	using ConfigManager;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EzBob.Models.Marketplaces.Builders;
	using Ezbob.Logger;
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
	using ServiceClientProxy.EzServiceReference;
	using Web.Models;
	using YodleeLib;
	using YodleeLib.connector;
	using ActionResult = System.Web.Mvc.ActionResult;

	public class MarketPlacesController : Controller {
		public MarketPlacesController(CustomerRepository customers,
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
			IWorkplaceContext context,
			DatabaseDataHelper helper,
			MarketPlaceRepository mpTypes) {
			_customerMarketplaces = customerMarketplaces;
			_marketPlaces = marketPlaces;
			m_oServiceClient = new ServiceClient();
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
			_helper = helper;
			_mpTypes = mpTypes;
		}

		public static long ToUnixTimestamp(DateTime d) {
			var duration = d - new DateTime(1970, 1, 1, 0, 0, 0);
			return (long)duration.TotalSeconds * 1000;
		}

		[Transactional]
		[Ajax]
		public void AddSearchWord(string word) {
			_yodleeSearchWordsRepository.AddWord(word);
		}

		[Transactional]
		[Ajax]
		public void AddYodleeRule(int group, int rule, string literal) {
			Log.Debug("{0} {1} {2}", group, rule, literal);
			var oGroup = _yodleeGroupRepository.Get(group);
			var oRule = _yodleeRuleRepository.Get(rule);

			if (oGroup != null && oRule != null) {
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
													   x.Literal == literal.Trim().ToLowerInvariant()))) {
					_yodleeGroupRuleMapRepository.Save(new MP_YodleeGroupRuleMap {
						Group = oGroup,
						Rule = oRule,
						Literal = literal.Trim().ToLowerInvariant()
					});

					var t = new YodleeTransactionRepository(_session);
					t.RemoveEzbobCategorization();
				}
			}
		}

		[Ajax]
		[HttpGet]
		public JsonResult CheckForUpdatedStatus(int mpId) {
			return Json(new { status = _customerMarketplaces.Get(mpId).GetUpdatingStatus() }, JsonRequestBehavior.AllowGet);
		}

		[Transactional]
		[Ajax]
		public void DeleteSearchWord(string word) {
			_yodleeSearchWordsRepository.DeleteWord(word);
		}

		[Ajax]
		[HttpGet]
		public JsonResult Details(int id) {
			// var cm = _customerMarketplaces.Get(id);
			// var values = _functions.GetAllValuesFor(cm);
			// return Json(values.Select(v => new FunctionValueModel(v)), JsonRequestBehavior.AllowGet);

			// TODO: insert actual totals here, the following is just a placeholder

			var lst = new List<FunctionValueModel> {
				new FunctionValueModel { Id = 1, Name = "Total 1", Value = "25", },
				new FunctionValueModel { Id = 2, Name = "Total 2", Value = "50", },
			};
			return Json(lst, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpPost]
		[Transactional]
		public JsonResult Disable(int umi) {
			var mp = _customerMarketplaces.Get(umi);
			mp.Disabled = true;
			return Json(new { });
		}

		public ActionResult DownloadCompanyFile(int fileId) {
			var file = m_oServiceClient.Instance.GetCompanyFile(_context.UserId, fileId);
			var fileMetaData = _companyFiles.Get(fileId);
			if (file != null && fileMetaData != null) {
				var document = file;
				var cd = new System.Net.Mime.ContentDisposition {
					FileName = fileMetaData.FileName,
					Inline = true,
				};

				Response.AppendHeader("Content-Disposition", cd.ToString());

				if (fileMetaData.FileContentType.Contains("image") ||
					fileMetaData.FileContentType.Contains("pdf") ||
					fileMetaData.FileContentType.Contains("html") ||
					fileMetaData.FileContentType.Contains("text")) {
					return File(document, fileMetaData.FileContentType);
				}

				var pdfDocument = AgreementRenderer.ConvertToPdf(document);

				return pdfDocument == null ? File(document, fileMetaData.FileContentType) : File(pdfDocument, "application/pdf");
			}

			return null;
		}

		[Ajax]
		[HttpPost]
		[Transactional]
		public JsonResult Enable(int umi) {
			var mp = _customerMarketplaces.Get(umi);
			mp.Disabled = false;
			return Json(new { });
		}

		[Ajax]
		[HttpGet]
		public JsonResult GetAffordabilityData(int id) {
			var ar = m_oServiceClient.Instance.CalculateModelsAndAffordability(_context.UserId, id, null);
			return Json(ar.Affordability, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpGet]
		public JsonResult GetCustomerManualAnnualRevenue(int nCustomerID) {
			CustomerManualAnnualizedRevenueActionResult cmarar = null;
			Customer customer = _customers.ReallyTryGet(nCustomerID);

			if (customer == null) {
				Log.Alert("Failed to load manual annual revenue for customer {0}: no customer found.", nCustomerID);
				return Json(new { success = false, }, JsonRequestBehavior.AllowGet);
			} // if

			Log.Debug("Loading customer manual annual revenue for customer {0}...", nCustomerID);

			try {
				cmarar = m_oServiceClient.Instance.GetCustomerManualAnnualizedRevenue(nCustomerID);
			} catch (Exception e) {
				Log.Warn(e, "Failed to load manual annual revenue for customer {0}.", nCustomerID);
				return Json(new { success = false, }, JsonRequestBehavior.AllowGet);
			} // try

			Log.Debug(
				"Loading customer manual annual revenue for customer {0} complete, result: has value: {1}, value: {2}, set time: {3}, comment: {4}.",
				nCustomerID,
				cmarar.Value.Revenue.HasValue ? "yes" : "no",
				cmarar.Value.Revenue,
				cmarar.Value.EntryTime,
				cmarar.Value.Comment
			);

			return Json(new {
				success = true,
				has_value = cmarar.Value.Revenue.HasValue,
				is_alibaba = customer.IsAlibaba,
				value = cmarar.Value,
			}, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public JsonResult GetCustomerMarketplacesHistory(int customerId) {
			var customer = _customers.Get(customerId);
			var models = _marketPlaces.GetMarketPlaceHistoryModel(customer);
			return Json(models, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpGet]
		public JsonResult GetTeraPeakOrderItems(int customerMarketPlaceId) {
			var data = _teraPeakOrderItems.GetTeraPeakOrderItems(customerMarketPlaceId);
			return Json(data.Select(item => new double?[] {
				(ToUnixTimestamp(item.StartDate) + ToUnixTimestamp(item.EndDate)) / 2,
				item.Revenue
			}).Cast<object>().ToArray(), JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpGet]
		public JsonResult Index(int id, DateTime? history = null) {
			try {
				var ar = m_oServiceClient.Instance.CalculateModelsAndAffordability(_context.UserId, id, history);
				var mps = JsonConvert.DeserializeObject<MarketPlaceModel[]>(ar.Models);
				return Json(mps.ToList(), JsonRequestBehavior.AllowGet);
			} catch (Exception ex) {
				Log.Error(ex, "failed to retrieve mp data from service");
				return Json(new List<MarketPlaceModel>(), JsonRequestBehavior.AllowGet);
			}
		}

		[Ajax]
		public JsonResult ParseYodlee(int customerId, int fileId) {
			var file = m_oServiceClient.Instance.GetCompanyFile(_context.UserId, fileId);
			var fileMetaData = _companyFiles.Get(fileId);
			var parser = new TransactionsParser();
			var parsed = parser.ParseFile(fileMetaData.FileName, file);

			if (!string.IsNullOrEmpty(parsed.Error)) {
				return Json(new { error = parsed.Error });
			}

			if (parsed.NumOfTransactions == 0) {
				return Json(new { error = "File contains 0 transactions" });
			}

			var customer = _customers.Get(customerId);
			var yodlee = new YodleeServiceInfo();
			var yodleeMp = customer.CustomerMarketPlaces.FirstOrDefault(mp => mp.Marketplace.InternalId == yodlee.InternalId && mp.DisplayName == "ParsedBank");

			if (yodleeMp != null) {
				var data = Serialized.Deserialize<YodleeSecurityInfo>(yodleeMp.SecurityData);
				data.ItemId = fileId;
				yodleeMp.SecurityData = new Serialized(data);
				_session.Flush();
				m_oServiceClient.Instance.UpdateMarketplace(customer.Id, yodleeMp.Id, false, _context.UserId);
			} else {
				int marketPlaceId = _mpTypes.GetAll().First(a => a.InternalId == yodlee.InternalId).Id;

				var securityData = new YodleeSecurityInfo {
					ItemId = fileId,
					Name = "",
					Password = "",
					MarketplaceId = marketPlaceId,
					CsId = 0
				};

				var yodleeDatabaseMarketPlace = new YodleeDatabaseMarketPlace();
				var mp = _helper.SaveOrUpdateCustomerMarketplace("ParsedBank", yodleeDatabaseMarketPlace, securityData, customer);

				m_oServiceClient.Instance.UpdateMarketplace(customer.Id, mp.Id, false, _context.UserId);
			}
			return Json(new { });
		}

		[Ajax]
		[Transactional]
		public void ReCheckMarketplaces(int umi) {
			var mp = _customerMarketplaces.Get(umi);

			string currentState = (string)_session.CreateSQLQuery(string.Format("EXEC GetLastMarketplaceStatus {0}", mp.Id)).UniqueResult();
			if (currentState == "In progress" || currentState == "BG launch")
				throw new Exception("Strategy already started");

			var customer = mp.Customer;

			switch (mp.Marketplace.Name) {
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
		}

		[Ajax]
		[HttpPost]
		public void RenewEbayToken(int umi) {
			var mp = _customerMarketplaces.Get(umi);

			m_oServiceClient.Instance.RenewEbayToken(
				_context.UserId,
				mp.Customer.Id,
				mp.DisplayName,
				"Customer/Profile/RenewEbayToken/"
			);
		}

		[Ajax]
		[HttpPost]
		public JsonResult SetCustomerManualAnnualRevenue(int nCustomerID, decimal nRevenue, string sComment) {
			CustomerManualAnnualizedRevenueActionResult cmarar = null;
			Customer customer = _customers.ReallyTryGet(nCustomerID);

			if (customer == null) {
				Log.Alert("Failed to set manual annual revenue for customer {0}: no customer found.", nCustomerID);
				return Json(new { success = false, });
			} // if

			Log.Debug("Setting customer manual annual revenue for customer {0} to be {1} ({2})...", nCustomerID, nRevenue, sComment);

			if (string.IsNullOrWhiteSpace(sComment)) {
				Log.Alert("Failed to set manual annual revenue for customer {0}: no comment provided.", nCustomerID);
				return Json(new { success = false, });
			} // if

			try {
				cmarar = m_oServiceClient.Instance.SetCustomerManualAnnualizedRevenue(nCustomerID, nRevenue, sComment);
			} catch (Exception e) {
				Log.Warn(e, "Failed to set customer manual annual revenue for customer {0} to be {1} ({2})...", nCustomerID, nRevenue, sComment);
				return Json(new { success = false, });
			} // try

			if (!cmarar.Value.Revenue.HasValue) {
				Log.Warn("Failed to set customer manual annual revenue for customer {0} to be {1} ({2})...", nCustomerID, nRevenue, sComment);
				return Json(new { success = false, });
			} // try

			Log.Debug("Setting customer manual annual revenue for customer {0} to be {1} ({2}) complete.", nCustomerID, nRevenue, sComment);

			return Json(new {
				success = true,
				has_value = true,
				is_alibaba = customer.IsAlibaba,
				value = cmarar.Value,
			});
		}

		[Transactional]
		public ActionResult TryRecheckYodlee(int umi) {
			var mp = _customerMarketplaces.Get(umi);
			var yodleeMain = new YodleeMain();
			var yodleeAccount = _yodleeAccountsRepository.Search(mp.Customer.Id);

			if (yodleeAccount == null) {
				return View(new { error = "Yodlee Account was not found" });
			}

			var securityInfo = Serialized.Deserialize<YodleeSecurityInfo>(mp.SecurityData);
			long itemId = securityInfo.ItemId;
			var lu = yodleeMain.LoginUser(yodleeAccount.Username, Encrypted.Decrypt(yodleeAccount.Password));
			if (lu == null) {
				return View(new { error = "Error Loging to Yodlee Account" });
			}

			if (!yodleeMain.IsMFA(itemId)) {
				bool isRefreshed;
				try {
					isRefreshed = yodleeMain.RefreshNotMFAItem(itemId);
				} catch (RefreshYodleeException ex) {
					Log.Warn(ex, "TryRecheckYodlee exception");
					return View(new { error = ex.ToString() });
				}
				if (isRefreshed) {
					var customer = mp.Customer;
					m_oServiceClient.Instance.UpdateMarketplace(customer.Id, umi, true, _context.UserId);
					return View(new { success = true });
				}

				return View(new { error = "Account wasn't refreshed successfully" });
			}

			//MFA Account for testing redirecting to Yodlee LAW
			var callback = Url.Action("YodleeCallback", "YodleeRecheck", new { Area = "Underwriter" }, "https") + "/" + umi;
			string finalUrl = yodleeMain.GetEditAccountUrl(securityInfo.ItemId, callback, yodleeAccount.Username, Encrypted.Decrypt(yodleeAccount.Password));
			return Redirect(finalUrl);
		}

		// TryRecheckYodlee
		[HttpPost]
		public ActionResult UploadFile() {
			Response.AddHeader("x-frame-options", "SAMEORIGIN");

			int customerId;

			if (!int.TryParse(Request.Headers["ezbob-underwriter-customer-id"], out customerId)) {
				return Json(new { error = "Failed to upload files: customer id header is missing." });
			}

			OneUploadLimitation oLimitations = CurrentValues.Instance.GetUploadLimitations("CompanyFilesMarketPlaces", "UploadedFiles");
			var customer = _customers.Get(customerId);

			try {
				new Transactional(() => {
					var serviceInfo = new CompanyFilesServiceInfo();
					var name = serviceInfo.DisplayName;
					var cf = new CompanyFilesDatabaseMarketPlace();
					var mp = _helper.SaveOrUpdateCustomerMarketplace(customer.Name + "_" + name, cf, null, customer);
					var rdh = mp.Marketplace.GetRetrieveDataHelper(_helper);
					rdh.UpdateCustomerMarketplaceFirst(mp.Id);
					//nResult = mp.Id;
				}).Execute();
			} catch (Exception ex) {
				Log.Error(ex, "Failed to create a company files marketplace for customer {0}.", customer.Stringify());
				return Json(new { error = ex.Message });
			}

			var error = new StringBuilder();
			for (int i = 0; i < Request.Files.Count; ++i) {
				HttpPostedFileBase file = Request.Files[i];

				if (file != null) {
					var content = new byte[file.ContentLength];

					int nRead = file.InputStream.Read(content, 0, file.ContentLength);

					if (nRead != file.ContentLength) {
						Log.Warn("File {0}: failed to read entire file contents, ignoring.", i);
						error.AppendLine("File ").Append(file.FileName).Append(" failed to read entire file contents, ignoring.");
						continue;
					} // if

					string sMimeType = oLimitations.DetectFileMimeType(content, file.FileName, oLog: new SafeILog(Log));

					if (string.IsNullOrWhiteSpace(sMimeType)) {
						Log.Warn("Not saving file #" + (i + 1) + ": " + file.FileName + " because it has unsupported MIME type.");
						error.AppendLine("Not saving file ").Append(file.FileName).Append(" because it has unsupported MIME type.");
						continue;
					} // if

					m_oServiceClient.Instance.CompanyFilesUpload(customerId, file.FileName, content, file.ContentType);
				}
			}

			return error.Length > 0 ? Json(new { error = error.ToString() }) : Json(new { success = true });
		}

		// GetAffordabilityData
		[Ajax]
		[HttpGet]
		public JsonResult YodleeDetails(int id) {
			var mp = _customerMarketplaces.Get(id);
			var b = new YodleeMarketplaceModelBuilder(_session);
			return Json(b.BuildYodlee(mp, null), JsonRequestBehavior.AllowGet);
		}

		private static readonly ASafeLog Log = new SafeILog(typeof(MarketPlacesController));
		private readonly CompanyFilesMetaDataRepository _companyFiles;
		private readonly IWorkplaceContext _context;
		private readonly CustomerMarketPlaceRepository _customerMarketplaces;
		private readonly CustomerRepository _customers;
		private readonly DatabaseDataHelper _helper;
		private readonly WebMarketPlacesFacade _marketPlaces;
		private readonly MarketPlaceRepository _mpTypes;
		private readonly ISession _session;
		private readonly MP_TeraPeakOrderItemRepository _teraPeakOrderItems;
		private readonly YodleeAccountsRepository _yodleeAccountsRepository;
		private readonly YodleeGroupRepository _yodleeGroupRepository;
		private readonly YodleeGroupRuleMapRepository _yodleeGroupRuleMapRepository;
		private readonly YodleeRuleRepository _yodleeRuleRepository;
		private readonly YodleeSearchWordsRepository _yodleeSearchWordsRepository;
		private readonly ServiceClient m_oServiceClient;
		// ReCheckMarketplaces

		// UploadedFiles

		// GetCustomerManualAnnualRevenue

		// SetCustomerManualAnnualRevenue

	} // class MarketPlacesController
} // namespace
