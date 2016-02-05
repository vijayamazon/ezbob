namespace EzBob.Web.Areas.Underwriter.Controllers {
	using System.Collections.Generic;
	using System.Globalization;
	using System.Web.Mvc;
	using Backend.Models;
	using ConfigManager;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Backend.Models;
	using Infrastructure;
	using Infrastructure.Attributes;
	using Infrastructure.csrf;
	using Models;
	using Newtonsoft.Json;
	using System;
	using System.Linq;
	using Ezbob.Backend.ModelsWithDB;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;
	using log4net;

	public class StrategySettingsController : Controller {
		private readonly ServiceClient serviceClient;
		private readonly CampaignRepository _campaignRepository;
		private readonly CampaignTypeRepository _campaignTypeRepository;
		private readonly CustomerRepository _customerRepository;
		private static readonly ILog Log = LogManager.GetLogger(typeof(StrategySettingsController));
		private readonly IWorkplaceContext _context;

		public StrategySettingsController(
			CampaignRepository campaignRepository,
			CampaignTypeRepository campaignTypeRepository,
			CustomerRepository customerRepository,
			IWorkplaceContext context
		) {
			this._campaignRepository = campaignRepository;
			this._campaignTypeRepository = campaignTypeRepository;
			this._customerRepository = customerRepository;
			this._context = context;
			this.serviceClient = new ServiceClient();
		}

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpGet]
		public JsonResult Index() {
			return Json(string.Empty, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpGet]
		public JsonResult SettingsGeneral() {
			var bwaBusinessCheck = CurrentValues.Instance.BWABusinessCheck;
			//var displayEarnedPoints = CurrentValues.Instance.DisplayEarnedPoints;
			var hmrcSalariesMultiplier = CurrentValues.Instance.HmrcSalariesMultiplier;
			var fcfFactor = CurrentValues.Instance.FCFFactor;
			var st = new {
				BWABusinessCheck = bwaBusinessCheck.Value,
				BWABusinessCheckDesc = bwaBusinessCheck.Description,
				HmrcSalariesMultiplier = hmrcSalariesMultiplier.Value,
				HmrcSalariesMultiplierDesc = hmrcSalariesMultiplier.Description,
				FCFFactor = fcfFactor.Value,
				FCFFactorDesc = fcfFactor.Description
				//DisplayEarnedPoints = displayEarnedPoints.Value,
				//DisplayEarnedPointsDesc = displayEarnedPoints.Description
			};
			return Json(st, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpGet]
		public JsonResult SettingsPricingModel() {
			PricingModelModelActionResult getPricingModelModelResponse =
				this.serviceClient.Instance.GetPricingModelModel(0, this._context.UserId, "Basic New");
			return Json(getPricingModelModelResponse.Value, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpPost]
		public JsonResult SettingsPricingModelForScenario(long scenarioID) {
			PricingModelModelActionResult getPricingModelModelResponse =
				this.serviceClient.Instance.GetPricingScenarioDetails(this._context.UserId, scenarioID);

			return Json(getPricingModelModelResponse.Value, JsonRequestBehavior.AllowGet);
		} // SettingsPricingModelForScenario

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpPost]
		public JsonResult SettingsSavePricingModelScenario(long scenarioID, string model) {
			PricingModelModel inputModel = JsonConvert.DeserializeObject<PricingModelModel>(model);
			this.serviceClient.Instance.SavePricingModelSettings(this._context.UserId, scenarioID, inputModel);
			return SettingsPricingModelForScenario(scenarioID);
		}

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpPost]
		public JsonResult SettingsGeneral(string BWABusinessCheck, decimal HmrcSalariesMultiplier, decimal fcfFactor) {
			UpdateSettingsGeneral(BWABusinessCheck, HmrcSalariesMultiplier, fcfFactor);

			UpdateConfigVars();
			return SettingsGeneral();
		}

		private void UpdateSettingsGeneral(string BWABusinessCheck, decimal HmrcSalariesMultiplier, decimal fcfFactor) {
			var upd = new SortedDictionary<Variables, string>();

			upd[Variables.BWABusinessCheck] = BWABusinessCheck;

			if (HmrcSalariesMultiplier >= 0 && HmrcSalariesMultiplier <= 1)
				upd[Variables.HmrcSalariesMultiplier] = HmrcSalariesMultiplier.ToString(CultureInfo.InvariantCulture);

			upd[Variables.FCFFactor] = fcfFactor.ToString(CultureInfo.InvariantCulture);
			//upd[Variables.DisplayEarnedPoints] = DisplayEarnedPoints;

			CurrentValues.Instance.Update(upd);
		}

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpGet]
		public JsonResult SettingsCharges() {
			var latePaymentCharge = CurrentValues.Instance.LatePaymentCharge;
			var rolloverCharge = CurrentValues.Instance.RolloverCharge;
			var partialPaymentCharge = CurrentValues.Instance.PartialPaymentCharge;
			var administrationCharge = CurrentValues.Instance.AdministrationCharge;
			var otherCharge = CurrentValues.Instance.OtherCharge;
			var amountToChargeFrom = CurrentValues.Instance.AmountToChargeFrom;

			var sc = new {
				LatePaymentCharge = latePaymentCharge.Value,
				LatePaymentChargeDesc = latePaymentCharge.Description,
				RolloverCharge = rolloverCharge.Value,
				RolloverChargeDesc = rolloverCharge.Description,
				PartialPaymentCharge = partialPaymentCharge.Value,
				PartialPaymentChargeDesc = partialPaymentCharge.Description,
				AdministrationCharge = administrationCharge.Value,
				AdministrationChargeDesc = administrationCharge.Description,
				OtherCharge = otherCharge.Value,
				OtherChargeDesc = otherCharge.Description,
				AmountToChargeFrom = amountToChargeFrom.Value,
				AmountToChargeFromDesc = amountToChargeFrom.Description
			};
			return Json(sc, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpPost]
		public JsonResult SettingsCharges(string administrationCharge,
			string latePaymentCharge,
			string otherCharge,
			string partialPaymentCharge,
			string rolloverCharge,
			string amountToChargeFrom
		) {
			UpdateSettingsCharges(
				administrationCharge,
				latePaymentCharge,
				otherCharge,
				partialPaymentCharge,
				rolloverCharge,
				amountToChargeFrom
			);

			UpdateConfigVars();
			return SettingsCharges();
		}

		private void UpdateSettingsCharges(
			string administrationCharge,
			string latePaymentCharge,
			string otherCharge,
			string partialPaymentCharge,
			string rolloverCharge,
			string amountToChargeFrom
		) {
			var upd = new SortedDictionary<Variables, string>();

			upd[Variables.AdministrationCharge] = administrationCharge;
			upd[Variables.LatePaymentCharge] = latePaymentCharge;
			upd[Variables.OtherCharge] = otherCharge;
			upd[Variables.PartialPaymentCharge] = partialPaymentCharge;
			upd[Variables.RolloverCharge] = rolloverCharge;
			upd[Variables.AmountToChargeFrom] = amountToChargeFrom;

			CurrentValues.Instance.Update(upd);
		}

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpGet]
		public JsonResult SettingsProduct() {
			var sc = new { firstProperty = "" };
			return Json(sc, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpGet]
		public JsonResult SettingsOpenPlatform() {
			var sc = new { firstProperty = "" };
			return Json(sc, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpGet]
		public JsonResult AutomationGeneral() {
			return Json(string.Empty, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpPost]
		public JsonResult AutomationGeneral(string[] newSettings) {
			return Json(string.Empty, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpGet]
		public JsonResult AutomationApproval() {
			var enableAutomaticApproval = CurrentValues.Instance.EnableAutomaticApproval;
			var enableAutomaticReApproval = CurrentValues.Instance.EnableAutomaticReApproval;
			var maxCapHomeOwner = CurrentValues.Instance.MaxCapHomeOwner;
			var maxCapNotHomeOwner = CurrentValues.Instance.MaxCapNotHomeOwner;

			var sa = new {
				EnableAutomaticApproval = enableAutomaticApproval.Value,
				EnableAutomaticApprovalDesc = enableAutomaticApproval.Description,
				EnableAutomaticReApproval = enableAutomaticReApproval.Value,
				EnableAutomaticReApprovalDesc = enableAutomaticReApproval.Description,
				MaxCapHomeOwner = maxCapHomeOwner.Value,
				MaxCapHomeOwnerDesc = maxCapHomeOwner.Description,
				MaxCapNotHomeOwner = maxCapNotHomeOwner.Value,
				MaxCapNotHomeOwnerDesc = maxCapNotHomeOwner.Description
			};
			return Json(sa, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpPost]
		public JsonResult AutomationApproval(
			string EnableAutomaticApproval,
			string EnableAutomaticReApproval,
			string MaxCapHomeOwner,
			string MaxCapNotHomeOwner
		) {
			UpdateAutomationApproval(
				EnableAutomaticApproval,
				EnableAutomaticReApproval,
				MaxCapHomeOwner,
				MaxCapNotHomeOwner
			);

			UpdateConfigVars();
			return AutomationApproval();
		}

		private void UpdateAutomationApproval(
			string EnableAutomaticApproval,
			string EnableAutomaticReApproval,
			string MaxCapHomeOwner,
			string MaxCapNotHomeOwner
		) {
			var upd = new SortedDictionary<Variables, string>();

			upd[Variables.EnableAutomaticApproval] = EnableAutomaticApproval;
			upd[Variables.EnableAutomaticReApproval] = EnableAutomaticReApproval;
			upd[Variables.MaxCapHomeOwner] = MaxCapHomeOwner;
			upd[Variables.MaxCapNotHomeOwner] = MaxCapNotHomeOwner;

			CurrentValues.Instance.Update(upd);
		}

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpGet]
		public JsonResult AutomationRejection() {
			var enableAutomaticRejection = CurrentValues.Instance.EnableAutomaticRejection;
			var lowCreditScore = CurrentValues.Instance.LowCreditScore;
			var totalAnnualTurnover = CurrentValues.Instance.TotalAnnualTurnover;
			var totalThreeMonthTurnover = CurrentValues.Instance.TotalThreeMonthTurnover;

			var reject_Defaults_CreditScore = CurrentValues.Instance.Reject_Defaults_CreditScore;
			var reject_Defaults_AccountsNum = CurrentValues.Instance.Reject_Defaults_AccountsNum;
			var reject_Defaults_Amount = CurrentValues.Instance.Reject_Defaults_Amount;
			var reject_Defaults_MonthsNum = CurrentValues.Instance.Reject_Defaults_MonthsNum;
			var reject_Minimal_Seniority = CurrentValues.Instance.Reject_Minimal_Seniority;

			var enableAutomaticReRejection = CurrentValues.Instance.EnableAutomaticReRejection;
			var autoRejectionExceptionCreditScore = CurrentValues.Instance.AutoRejectionException_CreditScore;
			var autoRejectionExceptionAnualTurnover = CurrentValues.Instance.AutoRejectionException_AnualTurnover;

			var reject_LowOfflineAnnualRevenue = CurrentValues.Instance.Reject_LowOfflineAnnualRevenue;
			var reject_LowOfflineQuarterRevenue = CurrentValues.Instance.Reject_LowOfflineQuarterRevenue;
			var reject_LateLastMonthsNum = CurrentValues.Instance.Reject_LateLastMonthsNum;
			var reject_NumOfLateAccounts = CurrentValues.Instance.Reject_NumOfLateAccounts;

			var rejectionLastValidLate = CurrentValues.Instance.RejectionLastValidLate;
			var rejectionCompanyScore = CurrentValues.Instance.RejectionCompanyScore;
			var rejectionExceptionMaxConsumerScoreForMpError =
				CurrentValues.Instance.RejectionExceptionMaxConsumerScoreForMpError;
			var rejectionExceptionMaxCompanyScoreForMpError =
				CurrentValues.Instance.RejectionExceptionMaxCompanyScoreForMpError;
			var rejectionExceptionMaxCompanyScore = CurrentValues.Instance.RejectionExceptionMaxCompanyScore;

			var reject_Defaults_CompanyScore = CurrentValues.Instance.Reject_Defaults_CompanyScore;
			var reject_Defaults_CompanyAccountsNum = CurrentValues.Instance.Reject_Defaults_CompanyAccountsNum;
			var reject_Defaults_CompanyMonthsNum = CurrentValues.Instance.Reject_Defaults_CompanyMonthsNum;
			var reject_Defaults_CompanyAmount = CurrentValues.Instance.Reject_Defaults_CompanyAmount;

			var sr = new {
				EnableAutomaticRejection = enableAutomaticRejection.Value,
				EnableAutomaticRejectionDesc = enableAutomaticRejection.Description,
				LowCreditScore = lowCreditScore.Value,
				LowCreditScoreDesc = lowCreditScore.Description,
				TotalAnnualTurnover = totalAnnualTurnover.Value,
				TotalAnnualTurnoverDesc = totalAnnualTurnover.Description,
				TotalThreeMonthTurnover = totalThreeMonthTurnover.Value,
				TotalThreeMonthTurnoverDesc = totalThreeMonthTurnover.Description,
				Reject_Defaults_CreditScore = reject_Defaults_CreditScore.Value,
				Reject_Defaults_CreditScoreDesc = reject_Defaults_CreditScore.Description,
				Reject_Defaults_AccountsNum = reject_Defaults_AccountsNum.Value,
				Reject_Defaults_AccountsNumDesc = reject_Defaults_AccountsNum.Description,
				Reject_Defaults_Amount = reject_Defaults_Amount.Value,
				Reject_Defaults_AmountDesc = reject_Defaults_Amount.Description,
				Reject_Defaults_MonthsNum = reject_Defaults_MonthsNum.Value,
				Reject_Defaults_MonthsNumDesc = reject_Defaults_MonthsNum.Description,
				Reject_Minimal_Seniority = reject_Minimal_Seniority.Value,
				Reject_Minimal_SeniorityDesc = reject_Minimal_Seniority.Description,
				EnableAutomaticReRejection = enableAutomaticReRejection.Value,
				EnableAutomaticReRejectionDesc = enableAutomaticReRejection.Description,
				AutoRejectionException_CreditScore = autoRejectionExceptionCreditScore.Value,
				AutoRejectionException_CreditScoreDesc = autoRejectionExceptionCreditScore.Description,
				AutoRejectionException_AnualTurnover = autoRejectionExceptionAnualTurnover.Value,
				AutoRejectionException_AnualTurnoverDesc = autoRejectionExceptionAnualTurnover.Description,
				Reject_LowOfflineAnnualRevenue = reject_LowOfflineAnnualRevenue.Value,
				Reject_LowOfflineAnnualRevenueDesc = reject_LowOfflineAnnualRevenue.Description,
				Reject_LowOfflineQuarterRevenue = reject_LowOfflineQuarterRevenue.Value,
				Reject_LowOfflineQuarterRevenueDesc = reject_LowOfflineQuarterRevenue.Description,
				Reject_LateLastMonthsNum = reject_LateLastMonthsNum.Value,
				Reject_LateLastMonthsNumDesc = reject_LateLastMonthsNum.Description,
				Reject_NumOfLateAccounts = reject_NumOfLateAccounts.Value,
				Reject_NumOfLateAccountsDesc = reject_NumOfLateAccounts.Description,
				RejectionLastValidLate = rejectionLastValidLate.Value,
				RejectionLastValidLateDesc = rejectionLastValidLate.Description,
				RejectionCompanyScore = rejectionCompanyScore.Value,
				RejectionCompanyScoreDesc = rejectionCompanyScore.Description,
				RejectionExceptionMaxConsumerScoreForMpError = rejectionExceptionMaxConsumerScoreForMpError.Value,
				RejectionExceptionMaxConsumerScoreForMpErrorDesc =
					rejectionExceptionMaxConsumerScoreForMpError.Description,
				RejectionExceptionMaxCompanyScoreForMpError =
					rejectionExceptionMaxCompanyScoreForMpError.Value,
				RejectionExceptionMaxCompanyScoreForMpErrorDesc =
					rejectionExceptionMaxCompanyScoreForMpError.Description,
				RejectionExceptionMaxCompanyScore = rejectionExceptionMaxCompanyScore.Value,
				RejectionExceptionMaxCompanyScoreDesc = rejectionExceptionMaxCompanyScore.Description,
				Reject_Defaults_CompanyScore = reject_Defaults_CompanyScore.Value,
				Reject_Defaults_CompanyScoreDesc = reject_Defaults_CompanyScore.Description,
				Reject_Defaults_CompanyAccountsNum = reject_Defaults_CompanyAccountsNum.Value,
				Reject_Defaults_CompanyAccountsNumDesc = reject_Defaults_CompanyAccountsNum.Description,
				Reject_Defaults_CompanyMonthsNum = reject_Defaults_CompanyMonthsNum.Value,
				Reject_Defaults_CompanyMonthsNumDesc = reject_Defaults_CompanyMonthsNum.Description,
				Reject_Defaults_CompanyAmount = reject_Defaults_CompanyAmount.Value,
				Reject_Defaults_CompanyAmountDesc = reject_Defaults_CompanyAmount.Description,
			};
			return Json(sr, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpPost]
		public JsonResult AutomationRejection(
			string EnableAutomaticRejection,
			string LowCreditScore,
			string Reject_Defaults_AccountsNum,
			string Reject_Defaults_Amount,
			string Reject_Defaults_CreditScore,
			string Reject_Defaults_MonthsNum,
			string Reject_Minimal_Seniority,
			string TotalAnnualTurnover,
			string TotalThreeMonthTurnover,
			string EnableAutomaticReRejection,
			string AutoRejectionException_CreditScore,
			string AutoRejectionException_AnualTurnover,
			string Reject_LowOfflineAnnualRevenue,
			string Reject_LowOfflineQuarterRevenue,
			string Reject_LateLastMonthsNum,
			string Reject_NumOfLateAccounts,
			string RejectionLastValidLate,
			string RejectionCompanyScore,
			string RejectionExceptionMaxConsumerScoreForMpError,
			string RejectionExceptionMaxCompanyScoreForMpError,
			string RejectionExceptionMaxCompanyScore,
			string Reject_Defaults_CompanyScore,
			string Reject_Defaults_CompanyAccountsNum,
			string Reject_Defaults_CompanyMonthsNum,
			string Reject_Defaults_CompanyAmount
		) {
			var upd = new SortedDictionary<Variables, string>();

			upd[Variables.EnableAutomaticRejection] = EnableAutomaticRejection;
			upd[Variables.LowCreditScore] = LowCreditScore;
			upd[Variables.Reject_Defaults_AccountsNum] = Reject_Defaults_AccountsNum;
			upd[Variables.Reject_Defaults_Amount] = Reject_Defaults_Amount;
			upd[Variables.Reject_Defaults_CreditScore] = Reject_Defaults_CreditScore;
			upd[Variables.Reject_Defaults_MonthsNum] = Reject_Defaults_MonthsNum;
			upd[Variables.Reject_Minimal_Seniority] = Reject_Minimal_Seniority;
			upd[Variables.TotalAnnualTurnover] = TotalAnnualTurnover;
			upd[Variables.TotalThreeMonthTurnover] = TotalThreeMonthTurnover;
			upd[Variables.EnableAutomaticReRejection] = EnableAutomaticReRejection;
			upd[Variables.AutoRejectionException_CreditScore] = AutoRejectionException_CreditScore;
			upd[Variables.AutoRejectionException_AnualTurnover] = AutoRejectionException_AnualTurnover;
			upd[Variables.Reject_LowOfflineAnnualRevenue] = Reject_LowOfflineAnnualRevenue;
			upd[Variables.Reject_LowOfflineQuarterRevenue] = Reject_LowOfflineQuarterRevenue;
			upd[Variables.Reject_LateLastMonthsNum] = Reject_LateLastMonthsNum;
			upd[Variables.Reject_NumOfLateAccounts] = Reject_NumOfLateAccounts;
			upd[Variables.RejectionLastValidLate] = RejectionLastValidLate;
			upd[Variables.RejectionCompanyScore] = RejectionCompanyScore;
			upd[Variables.RejectionExceptionMaxConsumerScoreForMpError] = RejectionExceptionMaxConsumerScoreForMpError;
			upd[Variables.RejectionExceptionMaxCompanyScoreForMpError] = RejectionExceptionMaxCompanyScoreForMpError;
			upd[Variables.RejectionExceptionMaxCompanyScore] = RejectionExceptionMaxCompanyScore;
			upd[Variables.Reject_Defaults_CompanyScore] = Reject_Defaults_CompanyScore;
			upd[Variables.Reject_Defaults_CompanyAccountsNum] = Reject_Defaults_CompanyAccountsNum;
			upd[Variables.Reject_Defaults_CompanyMonthsNum] = Reject_Defaults_CompanyMonthsNum;
			upd[Variables.Reject_Defaults_CompanyAmount] = Reject_Defaults_CompanyAmount;

			CurrentValues.Instance.Update(upd);

			UpdateConfigVars();
			return AutomationRejection();
		}

		[Ajax]
		[HttpGet]
		public JsonResult SettingsExperian() {
			var mainApplicant = CurrentValues.Instance.FinancialAccounts_MainApplicant.Value;
			var aliasOfMainApplicant = CurrentValues.Instance.FinancialAccounts_AliasOfMainApplicant.Value;
			var associationOfMainApplicant = CurrentValues.Instance.FinancialAccounts_AssociationOfMainApplicant.Value;
			var jointApplicant = CurrentValues.Instance.FinancialAccounts_JointApplicant.Value;
			var aliasOfJointApplicant = CurrentValues.Instance.FinancialAccounts_AliasOfJointApplicant.Value;
			var associationOfJointApplicant = CurrentValues.Instance.FinancialAccounts_AssociationOfJointApplicant.Value;
			var noMatch = CurrentValues.Instance.FinancialAccounts_No_Match.Value;

			var model = new {
				FinancialAccounts_MainApplicant = mainApplicant,
				FinancialAccounts_AliasOfMainApplicant = aliasOfMainApplicant,
				FinancialAccounts_AssociationOfMainApplicant = associationOfMainApplicant,
				FinancialAccounts_JointApplicant = jointApplicant,
				FinancialAccounts_AliasOfJointApplicant = aliasOfJointApplicant,
				FinancialAccounts_AssociationOfJointApplicant = associationOfJointApplicant,
				FinancialAccounts_No_Match = noMatch
			};
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		// ReSharper disable  InconsistentNaming
		[Ajax]
		[HttpPost]
		public JsonResult SettingsExperian(

			string FinancialAccounts_MainApplicant,
			string FinancialAccounts_AliasOfMainApplicant,
			string FinancialAccounts_AssociationOfMainApplicant,
			string FinancialAccounts_JointApplicant,
			string FinancialAccounts_AliasOfJointApplicant,
			string FinancialAccounts_AssociationOfJointApplicant,
			string FinancialAccounts_No_Match) {
			UpdateSettingsExperian(
				FinancialAccounts_MainApplicant,
				FinancialAccounts_AliasOfMainApplicant,
				FinancialAccounts_AssociationOfMainApplicant,
				FinancialAccounts_JointApplicant,
				FinancialAccounts_AliasOfJointApplicant,
				FinancialAccounts_AssociationOfJointApplicant,
				FinancialAccounts_No_Match
			);
			UpdateConfigVars();
			return SettingsGeneral();
		}

		private void UpdateSettingsExperian(
			string FinancialAccounts_MainApplicant,
			string FinancialAccounts_AliasOfMainApplicant,
			string FinancialAccounts_AssociationOfMainApplicant,
			string FinancialAccounts_JointApplicant,
			string FinancialAccounts_AliasOfJointApplicant,
			string FinancialAccounts_AssociationOfJointApplicant,
			string FinancialAccounts_No_Match
		) {
			var upd = new SortedDictionary<Variables, string>();

			upd[Variables.FinancialAccounts_MainApplicant] = FinancialAccounts_MainApplicant;
			upd[Variables.FinancialAccounts_AliasOfMainApplicant] = FinancialAccounts_AliasOfMainApplicant;
			upd[Variables.FinancialAccounts_AssociationOfMainApplicant] = FinancialAccounts_AssociationOfMainApplicant;
			upd[Variables.FinancialAccounts_JointApplicant] = FinancialAccounts_JointApplicant;
			upd[Variables.FinancialAccounts_AliasOfJointApplicant] = FinancialAccounts_AliasOfJointApplicant;
			upd[Variables.FinancialAccounts_AssociationOfJointApplicant] = FinancialAccounts_AssociationOfJointApplicant;
			upd[Variables.FinancialAccounts_No_Match] = FinancialAccounts_No_Match;

			CurrentValues.Instance.Update(upd);
		}

		[Ajax]
		[HttpGet]
		public JsonResult SettingsCampaign() {
			var campaignsList = this._campaignRepository
				.GetAll().ToList();

			var campaigns = campaignsList
				.Select(c => new CampaignModel {
					Name = c.Name,
					Type = c.CampaignType.Type,
					StartDate = c.StartDate,
					EndDate = c.EndDate,
					Description = c.Description,
					Id = c.Id,
					Customers = c.Clients
					.OrderBy(cc => cc.Customer.Id)
					.Select(cc => new CampaignCustomerModel {
						Id = cc.Customer.Id,
						Email = cc.Customer.Name,
						Name = cc.Customer.PersonalInfo == null ? "" : cc.Customer.PersonalInfo.Fullname
					}).ToList()
				})
				.ToList();

			var campaignTypes = this._campaignTypeRepository
				.GetAll()
				.Select(ct => new {
					Type = ct.Type,
					Id = ct.Id,
					Description = ct.Description
				})
				.ToList();

			return Json(new { campaigns, campaignTypes }, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpGet]
		[Transactional]
		public JsonResult SettingsConfigTable(string tableName) {
			ConfigTable[] deserializedArray =
				this.serviceClient.Instance.GetConfigTable(this._context.UserId, tableName).Table;

			if (deserializedArray != null)
				foreach (ConfigTable entry in deserializedArray)
					entry.Value *= 100; // Convert to percent

			return Json(new { configTableEntries = deserializedArray }, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpPost]
		[Transactional]
		public JsonResult SaveConfigTable(string serializedModels, string configTableType) {
			ConfigTableType c;
			switch (configTableType) {
			case "LoanOfferMultiplier":
				c = ConfigTableType.LoanOfferMultiplier;
				break;
			case "EuLoanMonthlyInterest":
				c = ConfigTableType.EuLoanMonthlyInterest;
				break;
			case "DefaultRateCompany":
				c = ConfigTableType.DefaultRateCompany;
				break;
			case "DefaultRateCustomer":
				c = ConfigTableType.DefaultRateCustomer;
				break;
			default:
				c = ConfigTableType.BasicInterestRate;
				break;
			}
			var deserializedModels = JsonConvert.DeserializeObject<List<ConfigTable>>(serializedModels);

			BoolActionResult result = this.serviceClient.Instance.SaveConfigTable(deserializedModels.ToArray(), c);
			return Json(new { error = result.Value ? "Error occurred during save" : null }, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpPost]
		[Transactional]
		public JsonResult AddCampaign(

			string campaignName,
			string campaignDescription,
			int? campaignType,
			string campaignStartDate,
			string campaignEndDate,
			string campaignCustomers,
			int? campaignId
			) {
			if (string.IsNullOrEmpty(campaignName) || string.IsNullOrEmpty(campaignStartDate) ||
				string.IsNullOrEmpty(campaignEndDate) || !campaignType.HasValue) {
				return Json(
					new { success = false, errorText = "One or more parameters missing" },
					JsonRequestBehavior.AllowGet
				);
			}

			DateTime startDate = DateTime.ParseExact(campaignStartDate, "dd/MM/yyyy", null);
			DateTime endDate = DateTime.ParseExact(campaignEndDate, "dd/MM/yyyy", null);

			if (endDate < startDate) {
				return Json(
					new { success = false, errorText = "End date prior to start date" },
					JsonRequestBehavior.AllowGet
				);
			}

			Campaign campaign = campaignId.HasValue ? this._campaignRepository.Get(campaignId) : new Campaign();

			campaign.Name = campaignName;
			campaign.CampaignType = this._campaignTypeRepository.Get(campaignType.Value);
			campaign.StartDate = startDate;
			campaign.EndDate = endDate;
			campaign.Description = campaignDescription;

			var campClients = campaign.Clients.ToArray();
			foreach (var client in campClients) {
				campaign.Clients.Remove(client);
			}

			this._campaignRepository.SaveOrUpdate(campaign);

			if (string.IsNullOrEmpty(campaignCustomers)) {
				return Json(new { success = true, errorText = "" }, JsonRequestBehavior.AllowGet);
			}
			string error = "";
			var clients = campaignCustomers.Trim().Split(' ');
			foreach (string client in clients) {
				if (string.IsNullOrWhiteSpace(client))
					continue;

				int customerId;
				if (int.TryParse(client, out customerId)) {
					try {
						var customer = this._customerRepository.ReallyTryGet(customerId);
						if (customer != null && campaign.Clients.All(cc => cc.Customer != customer)) {
							campaign.Clients.Add(new CampaignClients { Campaign = campaign, Customer = customer });
						} else {
							error += customerId + " not a valid customer id.";
						}
					} catch (Exception) {
						error += customerId + " not a valid customer id.";
					}
				} else {
					error += client + " not a valid customer id.";
				}
			}
			Log.DebugFormat(
				"{0}, {1}, {2}, {3}, {4}, {5}. ",
				campaignName,
				campaignDescription,
				campaignType,
				startDate,
				endDate,
				campaignCustomers
			);
			return Json(new { success = true, errorText = error }, JsonRequestBehavior.AllowGet);
		} // AddCampaign

		private void UpdateConfigVars() {
			new ServiceClient().Instance.UpdateConfigurationVariables(this._context.UserId);
		} // UpdateConfigVars
	} // class StrategySettingsController
} // namespace
