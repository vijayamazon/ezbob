namespace EzBob.Web.Areas.Underwriter.Controllers
{
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using System.Web.Mvc;
	using Backend.Models;
	using ConfigManager;
	using EZBob.DatabaseLib.Model;
	using EZBob.DatabaseLib.Model.Database;
	using Infrastructure.Attributes;
	using Infrastructure.csrf;
	using Models;
	using Newtonsoft.Json;
	using System;
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;
	using log4net;

	public class StrategySettingsController : Controller
	{
		private readonly ServiceClient serviceClient;
		private readonly IConfigurationVariablesRepository _configurationVariablesRepository;
		private readonly CampaignRepository _campaignRepository;
		private readonly CampaignTypeRepository _campaignTypeRepository;
		private readonly CustomerRepository _customerRepository;
		private static readonly ILog Log = LogManager.GetLogger(typeof(StrategySettingsController));

		public StrategySettingsController(IConfigurationVariablesRepository configurationVariablesRepository, CampaignRepository campaignRepository, CampaignTypeRepository campaignTypeRepository, CustomerRepository customerRepository)
		{
			_configurationVariablesRepository = configurationVariablesRepository;
			_campaignRepository = campaignRepository;
			_campaignTypeRepository = campaignTypeRepository;
			_customerRepository = customerRepository;
			serviceClient = new ServiceClient();
		}

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpGet]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public JsonResult Index()
		{
			return Json(string.Empty, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpGet]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public JsonResult SettingsGeneral()
		{
			var bwaBusinessCheck = _configurationVariablesRepository.GetByName("BWABusinessCheck");
			//var displayEarnedPoints = _configurationVariablesRepository.GetByName("DisplayEarnedPoints");
			var hmrcSalariesMultiplier = _configurationVariablesRepository.GetByName(Variables.HmrcSalariesMultiplier.ToString());
			var fcfFactor = _configurationVariablesRepository.GetByName(Variables.FCFFactor.ToString());
			var st = new
				{
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
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public JsonResult SettingsPricingModel()
		{
			var pricingModelTenurePercents = _configurationVariablesRepository.GetByName(Variables.PricingModelTenurePercents.ToString());
			var pricingModelDefaultRateCompanyShare = _configurationVariablesRepository.GetByName(Variables.PricingModelDefaultRateCompanyShare.ToString());
			var pricingModelInterestOnlyPeriod = _configurationVariablesRepository.GetByName(Variables.PricingModelInterestOnlyPeriod.ToString());
			var pricingModelCollectionRate = _configurationVariablesRepository.GetByName(Variables.PricingModelCollectionRate.ToString());
			var pricingModelCogs = _configurationVariablesRepository.GetByName(Variables.PricingModelCogs.ToString());
			var pricingModelDebtOutOfTotalCapital = _configurationVariablesRepository.GetByName(Variables.PricingModelDebtOutOfTotalCapital.ToString());
			var pricingModelCostOfDebtPA = _configurationVariablesRepository.GetByName(Variables.PricingModelCostOfDebtPA.ToString());
			var pricingModelOpexAndCapex = _configurationVariablesRepository.GetByName(Variables.PricingModelOpexAndCapex.ToString());
			var pricingModelProfitMarkupPercentsOfRevenue = _configurationVariablesRepository.GetByName(Variables.PricingModelProfitMarkupPercentsOfRevenue.ToString());
			
			var pricingModelSettings = new
			{
				PricingModelTenurePercents = pricingModelTenurePercents.Value,
				PricingModelTenurePercentsDesc = pricingModelTenurePercents.Description,
				PricingModelDefaultRateCompanyShare = pricingModelDefaultRateCompanyShare.Value,
				PricingModelDefaultRateCompanyShareDesc = pricingModelDefaultRateCompanyShare.Description,
				PricingModelInterestOnlyPeriod = pricingModelInterestOnlyPeriod.Value,
				PricingModelInterestOnlyPeriodDesc = pricingModelInterestOnlyPeriod.Description,
				PricingModelCollectionRate = pricingModelCollectionRate.Value,
				PricingModelCollectionRateDesc = pricingModelCollectionRate.Description,
				PricingModelCogs = pricingModelCogs.Value,
				PricingModelCogsDesc = pricingModelCogs.Description,
				PricingModelDebtOutOfTotalCapital = pricingModelDebtOutOfTotalCapital.Value,
				PricingModelDebtOutOfTotalCapitalDesc = pricingModelDebtOutOfTotalCapital.Description,
				PricingModelCostOfDebtPA = pricingModelCostOfDebtPA.Value,
				PricingModelCostOfDebtPADesc = pricingModelCostOfDebtPA.Description,
				PricingModelOpexAndCapex = pricingModelOpexAndCapex.Value,
				PricingModelOpexAndCapexDesc = pricingModelOpexAndCapex.Description,
				PricingModelProfitMarkupPercentsOfRevenue = pricingModelProfitMarkupPercentsOfRevenue.Value,
				PricingModelProfitMarkupPercentsOfRevenueDesc = pricingModelProfitMarkupPercentsOfRevenue.Description,
			};
			return Json(pricingModelSettings, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpPost]
		public JsonResult SettingsPricingModel(decimal pricingModelTenurePercents, decimal pricingModelDefaultRateCompanyShare,
		                                       int pricingModelInterestOnlyPeriod, decimal pricingModelCollectionRate,
		                                       decimal pricingModelCogs, decimal pricingModelDebtOutOfTotalCapital,
		                                       decimal pricingModelCostOfDebtPA, decimal pricingModelOpexAndCapex,
		                                       decimal pricingModelProfitMarkupPercentsOfRevenue)
		{
			UpdateSettingsPricingModel(pricingModelTenurePercents, pricingModelDefaultRateCompanyShare,
			                           pricingModelInterestOnlyPeriod, pricingModelCollectionRate, pricingModelCogs,
			                           pricingModelDebtOutOfTotalCapital, pricingModelCostOfDebtPA, pricingModelOpexAndCapex,
			                           pricingModelProfitMarkupPercentsOfRevenue);

			UpdateConfigVars();
			return SettingsPricingModel();
		}

		[Transactional]
		private void UpdateSettingsPricingModel(decimal pricingModelTenurePercents, decimal pricingModelDefaultRateCompanyShare,
		                                        int pricingModelInterestOnlyPeriod, decimal pricingModelCollectionRate,
		                                        decimal pricingModelCogs, decimal pricingModelDebtOutOfTotalCapital,
		                                        decimal pricingModelCostOfDebtPA, decimal pricingModelOpexAndCapex,
		                                        decimal pricingModelProfitMarkupPercentsOfRevenue)
		{
			_configurationVariablesRepository.SetByName("PricingModelTenurePercents", pricingModelTenurePercents.ToString(CultureInfo.InvariantCulture));
			_configurationVariablesRepository.SetByName("PricingModelDefaultRateCompanyShare", pricingModelDefaultRateCompanyShare.ToString(CultureInfo.InvariantCulture));
			_configurationVariablesRepository.SetByName("PricingModelInterestOnlyPeriod", pricingModelInterestOnlyPeriod.ToString(CultureInfo.InvariantCulture));
			_configurationVariablesRepository.SetByName("PricingModelCollectionRate", pricingModelCollectionRate.ToString(CultureInfo.InvariantCulture));
			_configurationVariablesRepository.SetByName("PricingModelCogs", pricingModelCogs.ToString(CultureInfo.InvariantCulture));
			_configurationVariablesRepository.SetByName("PricingModelDebtOutOfTotalCapital", pricingModelDebtOutOfTotalCapital.ToString(CultureInfo.InvariantCulture));
			_configurationVariablesRepository.SetByName("PricingModelCostOfDebtPA", pricingModelCostOfDebtPA.ToString(CultureInfo.InvariantCulture));
			_configurationVariablesRepository.SetByName("PricingModelOpexAndCapex", pricingModelOpexAndCapex.ToString(CultureInfo.InvariantCulture));
			_configurationVariablesRepository.SetByName("PricingModelProfitMarkupPercentsOfRevenue", pricingModelProfitMarkupPercentsOfRevenue.ToString(CultureInfo.InvariantCulture));
		}

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpPost]
		public JsonResult SettingsGeneral(string BWABusinessCheck, decimal HmrcSalariesMultiplier, decimal fcfFactor/*, string DisplayEarnedPoints*/)
		{
			UpdateSettingsGeneral(BWABusinessCheck, HmrcSalariesMultiplier, fcfFactor);

			UpdateConfigVars();
			return SettingsGeneral();
		}

		[Transactional]
		private void UpdateSettingsGeneral(string BWABusinessCheck, decimal HmrcSalariesMultiplier, decimal fcfFactor)
		{
			_configurationVariablesRepository.SetByName("BWABusinessCheck", BWABusinessCheck);
			if (HmrcSalariesMultiplier >= 0 && HmrcSalariesMultiplier <= 1)
			{
				_configurationVariablesRepository.SetByName(Variables.HmrcSalariesMultiplier.ToString(),
															HmrcSalariesMultiplier.ToString(CultureInfo.InvariantCulture));
			}
			_configurationVariablesRepository.SetByName(Variables.FCFFactor.ToString(), fcfFactor.ToString(CultureInfo.InvariantCulture));
			//_configurationVariablesRepository.SetByName("DisplayEarnedPoints", DisplayEarnedPoints);
		}

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpGet]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public JsonResult SettingsCharges()
		{
			var latePaymentCharge = _configurationVariablesRepository.GetByName("LatePaymentCharge");
			var rolloverCharge = _configurationVariablesRepository.GetByName("RolloverCharge");
			var partialPaymentCharge = _configurationVariablesRepository.GetByName("PartialPaymentCharge");
			var administrationCharge = _configurationVariablesRepository.GetByName("AdministrationCharge");
			var otherCharge = _configurationVariablesRepository.GetByName("OtherCharge");
			var amountToChargeFrom = _configurationVariablesRepository.GetByName("AmountToChargeFrom");

			var sc = new
				{
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
			)
		{
			UpdateSettingsCharges(administrationCharge, latePaymentCharge, otherCharge, partialPaymentCharge, rolloverCharge, amountToChargeFrom);

			UpdateConfigVars();
			return SettingsCharges();
		}

		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		private void UpdateSettingsCharges(string administrationCharge, string latePaymentCharge, string otherCharge,
										   string partialPaymentCharge, string rolloverCharge, string amountToChargeFrom)
		{
			_configurationVariablesRepository.SetByName("AdministrationCharge", administrationCharge);
			_configurationVariablesRepository.SetByName("LatePaymentCharge", latePaymentCharge);
			_configurationVariablesRepository.SetByName("OtherCharge", otherCharge);
			_configurationVariablesRepository.SetByName("PartialPaymentCharge", partialPaymentCharge);
			_configurationVariablesRepository.SetByName("RolloverCharge", rolloverCharge);
			_configurationVariablesRepository.SetByName("AmountToChargeFrom", amountToChargeFrom);
		}

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpGet]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public JsonResult AutomationGeneral()
		{
			return Json(string.Empty, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpPost]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public JsonResult AutomationGeneral(string[] newSettings)
		{
			return Json(string.Empty, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpGet]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public JsonResult AutomationApproval()
		{
			var enableAutomaticApproval = _configurationVariablesRepository.GetByName("EnableAutomaticApproval");
			var enableAutomaticReApproval = _configurationVariablesRepository.GetByName("EnableAutomaticReApproval");
			var maxCapHomeOwner = _configurationVariablesRepository.GetByName("MaxCapHomeOwner");
			var maxCapNotHomeOwner = _configurationVariablesRepository.GetByName("MaxCapNotHomeOwner");

			var sa = new
				{
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
			)
		{
			UpdateAutomationApproval(EnableAutomaticApproval, EnableAutomaticReApproval, MaxCapHomeOwner, MaxCapNotHomeOwner);

			UpdateConfigVars();
			return AutomationApproval();
		}

		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		private void UpdateAutomationApproval(string EnableAutomaticApproval, string EnableAutomaticReApproval,
											  string MaxCapHomeOwner, string MaxCapNotHomeOwner)
		{
			_configurationVariablesRepository.SetByName("EnableAutomaticApproval", EnableAutomaticApproval);
			_configurationVariablesRepository.SetByName("EnableAutomaticReApproval", EnableAutomaticReApproval);
			_configurationVariablesRepository.SetByName("MaxCapHomeOwner", MaxCapHomeOwner);
			_configurationVariablesRepository.SetByName("MaxCapNotHomeOwner", MaxCapNotHomeOwner);
		}

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpGet]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public JsonResult AutomationRejection()
		{
			var enableAutomaticRejection = _configurationVariablesRepository.GetByName("EnableAutomaticRejection");
			var lowCreditScore = _configurationVariablesRepository.GetByName("LowCreditScore");
			var totalAnnualTurnover = _configurationVariablesRepository.GetByName("TotalAnnualTurnover");
			var totalThreeMonthTurnover = _configurationVariablesRepository.GetByName("TotalThreeMonthTurnover");

			var reject_Defaults_CreditScore = _configurationVariablesRepository.GetByName("Reject_Defaults_CreditScore");
			var reject_Defaults_AccountsNum = _configurationVariablesRepository.GetByName("Reject_Defaults_AccountsNum");
			var reject_Defaults_Amount = _configurationVariablesRepository.GetByName("Reject_Defaults_Amount");
			var reject_Defaults_MonthsNum = _configurationVariablesRepository.GetByName("Reject_Defaults_MonthsNum");
			var reject_Minimal_Seniority = _configurationVariablesRepository.GetByName("Reject_Minimal_Seniority");

			var enableAutomaticReRejection = _configurationVariablesRepository.GetByName("EnableAutomaticReRejection");
			var autoRejectionExceptionCreditScore = _configurationVariablesRepository.GetByName("AutoRejectionException_CreditScore");
			var autoRejectionExceptionAnualTurnover = _configurationVariablesRepository.GetByName("AutoRejectionException_AnualTurnover");


			var sr = new
				{
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
					AutoRejectionException_AnualTurnoverDesc = autoRejectionExceptionAnualTurnover.Description
				};
			return Json(sr, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpPost]

		public JsonResult AutomationRejection(string EnableAutomaticRejection,
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
												 string AutoRejectionException_AnualTurnover)
		{
			UpdateAutomationRejection(EnableAutomaticRejection, LowCreditScore, Reject_Defaults_AccountsNum, Reject_Defaults_Amount, Reject_Defaults_CreditScore, Reject_Defaults_MonthsNum, Reject_Minimal_Seniority, TotalAnnualTurnover, TotalThreeMonthTurnover, EnableAutomaticReRejection, AutoRejectionException_CreditScore, AutoRejectionException_AnualTurnover);
			UpdateConfigVars();
			return AutomationRejection();
		}

		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		private void UpdateAutomationRejection(string EnableAutomaticRejection, string LowCreditScore,
											   string Reject_Defaults_AccountsNum, string Reject_Defaults_Amount,
											   string Reject_Defaults_CreditScore, string Reject_Defaults_MonthsNum,
											   string Reject_Minimal_Seniority, string TotalAnnualTurnover,
											   string TotalThreeMonthTurnover, string EnableAutomaticReRejection,
											   string AutoRejectionException_CreditScore,
											   string AutoRejectionException_AnualTurnover)
		{
			_configurationVariablesRepository.SetByName("EnableAutomaticRejection", EnableAutomaticRejection);
			_configurationVariablesRepository.SetByName("LowCreditScore", LowCreditScore);
			_configurationVariablesRepository.SetByName("Reject_Defaults_AccountsNum", Reject_Defaults_AccountsNum);
			_configurationVariablesRepository.SetByName("Reject_Defaults_Amount", Reject_Defaults_Amount);
			_configurationVariablesRepository.SetByName("Reject_Defaults_CreditScore", Reject_Defaults_CreditScore);
			_configurationVariablesRepository.SetByName("Reject_Defaults_MonthsNum", Reject_Defaults_MonthsNum);
			_configurationVariablesRepository.SetByName("Reject_Minimal_Seniority", Reject_Minimal_Seniority);
			_configurationVariablesRepository.SetByName("TotalAnnualTurnover", TotalAnnualTurnover);
			_configurationVariablesRepository.SetByName("TotalThreeMonthTurnover", TotalThreeMonthTurnover);
			_configurationVariablesRepository.SetByName("EnableAutomaticReRejection", EnableAutomaticReRejection);
			_configurationVariablesRepository.SetByName("AutoRejectionException_CreditScore", AutoRejectionException_CreditScore);
			_configurationVariablesRepository.SetByName("AutoRejectionException_AnualTurnover",
														AutoRejectionException_AnualTurnover);
		}

		[Ajax]
		[HttpGet]
		public JsonResult SettingsExperian()
		{
			var mainApplicant = _configurationVariablesRepository.GetByName("FinancialAccounts_MainApplicant").Value;
			var aliasOfMainApplicant = _configurationVariablesRepository.GetByName("FinancialAccounts_AliasOfMainApplicant").Value;
			var associationOfMainApplicant = _configurationVariablesRepository.GetByName("FinancialAccounts_AssociationOfMainApplicant").Value;
			var jointApplicant = _configurationVariablesRepository.GetByName("FinancialAccounts_JointApplicant").Value;
			var aliasOfJointApplicant = _configurationVariablesRepository.GetByName("FinancialAccounts_AliasOfJointApplicant").Value;
			var associationOfJointApplicant = _configurationVariablesRepository.GetByName("FinancialAccounts_AssociationOfJointApplicant").Value;
			var noMatch = _configurationVariablesRepository.GetByName("FinancialAccounts_No_Match").Value;

			var model = new
			{
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
			string FinancialAccounts_No_Match)
		{
			UpdateSettingsExperian(FinancialAccounts_MainApplicant, FinancialAccounts_AliasOfMainApplicant, FinancialAccounts_AssociationOfMainApplicant, FinancialAccounts_JointApplicant, FinancialAccounts_AliasOfJointApplicant, FinancialAccounts_AssociationOfJointApplicant, FinancialAccounts_No_Match);
			UpdateConfigVars();
			return SettingsGeneral();
		}

		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		private void UpdateSettingsExperian(string FinancialAccounts_MainApplicant,
											string FinancialAccounts_AliasOfMainApplicant,
											string FinancialAccounts_AssociationOfMainApplicant,
											string FinancialAccounts_JointApplicant,
											string FinancialAccounts_AliasOfJointApplicant,
											string FinancialAccounts_AssociationOfJointApplicant,
											string FinancialAccounts_No_Match)
		{
			_configurationVariablesRepository.SetByName("FinancialAccounts_MainApplicant", FinancialAccounts_MainApplicant);
			_configurationVariablesRepository.SetByName("FinancialAccounts_AliasOfMainApplicant",
														FinancialAccounts_AliasOfMainApplicant);
			_configurationVariablesRepository.SetByName("FinancialAccounts_AssociationOfMainApplicant",
														FinancialAccounts_AssociationOfMainApplicant);
			_configurationVariablesRepository.SetByName("FinancialAccounts_JointApplicant", FinancialAccounts_JointApplicant);
			_configurationVariablesRepository.SetByName("FinancialAccounts_AliasOfJointApplicant",
														FinancialAccounts_AliasOfJointApplicant);
			_configurationVariablesRepository.SetByName("FinancialAccounts_AssociationOfJointApplicant",
														FinancialAccounts_AssociationOfJointApplicant);
			_configurationVariablesRepository.SetByName("FinancialAccounts_No_Match", FinancialAccounts_No_Match);
		}

		[Ajax]
		[HttpGet]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public JsonResult SettingsCampaign()
		{
			var campaignsList = _campaignRepository
				.GetAll().ToList();

			var campaigns = campaignsList
				.Select(c => new CampaignModel
					{
						Name = c.Name,
						Type = c.CampaignType.Type,
						StartDate = c.StartDate,
						EndDate = c.EndDate,
						Description = c.Description,
						Id = c.Id,
						Customers = c.Clients
						.OrderBy(cc => cc.Customer.Id)
						.Select(cc => new CampaignCustomerModel
							{
								Id = cc.Customer.Id,
								Email = cc.Customer.Name,
								Name = cc.Customer.PersonalInfo == null ? "" : cc.Customer.PersonalInfo.Fullname
							}).ToList()
					})
				.ToList();

			var campaignTypes = _campaignTypeRepository
				.GetAll()
				.Select(ct => new
					{
						Type = ct.Type,
						Id = ct.Id,
						Description = ct.Description
					})
				.ToList();

			return Json(new { campaigns, campaignTypes }, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpGet]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public JsonResult SettingsBasicInterestRate()
		{
			var basicInterestRatesList = serviceClient.Instance.GetSpResultTable("GetBasicInterestRates", null);
			var deserializedArray = JsonConvert.DeserializeObject<BasicInterestRate[]>(basicInterestRatesList.SerializedDataTable);
			var basicInterestRates = deserializedArray == null ? null : deserializedArray.ToList();
			if (basicInterestRates != null)
			{
				foreach (BasicInterestRate basicInterestRate in basicInterestRates)
				{
					basicInterestRate.LoanInterestBase *= 100; // Convert to percent
				}
			}

			return Json(new { basicInterestRates }, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpPost]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public JsonResult SaveBasicInterestRate(string serializedModels)
		{
			var deserializedModels = JsonConvert.DeserializeObject<List<BasicInterestRate>>(serializedModels);
			var sortedModels = new SortedDictionary<int, BasicInterestRate>();
			var sortedList = new List<BasicInterestRate>();
			foreach (BasicInterestRate model in deserializedModels)
			{
				if (sortedModels.ContainsKey(model.FromScore))
				{
					string errorMessage = string.Format("FromScore must be unique:{0}", model.FromScore);
					Log.WarnFormat(errorMessage);
					return Json(new { error = errorMessage }, JsonRequestBehavior.AllowGet);
				}
				sortedModels.Add(model.FromScore, model);
			}

			bool isFirst = true;
			int highestSoFar = 0;
			foreach (int key in sortedModels.Keys)
			{
				BasicInterestRate model = sortedModels[key];
				sortedList.Add(model);
				model.LoanInterestBase /= 100; // Convert to decimal number
				if (isFirst)
				{
					if (model.FromScore != 0)
					{
						const string errorMessage = "FromScore must start at 0";
						Log.WarnFormat(errorMessage);
						return Json(new { error = errorMessage }, JsonRequestBehavior.AllowGet);
					}
					isFirst = false;
				}
				else
				{
					if (highestSoFar + 1 < model.FromScore)
					{
						string errorMessage = string.Format("No range covers the numbers {0}-{1}", highestSoFar + 1, model.FromScore - 1);
						Log.WarnFormat(errorMessage);
						return Json(new { error = errorMessage }, JsonRequestBehavior.AllowGet);
					}
					if (highestSoFar + 1 > model.FromScore)
					{
						string errorMessage = string.Format("The numbers {0}-{1} are coverered by more than one range", model.FromScore, highestSoFar);
						Log.WarnFormat(errorMessage);
						return Json(new { error = errorMessage }, JsonRequestBehavior.AllowGet);
					}
				}
				highestSoFar = model.ToScore;
			}

			if (highestSoFar < 100000000)
			{
				string errorMessage = string.Format("No range covers the numbers {0}-100000000", highestSoFar);
				Log.WarnFormat(errorMessage);
				return Json(new { error = errorMessage }, JsonRequestBehavior.AllowGet);
			}

			if (highestSoFar > 100000000)
			{
				const string errorMessage = "Maximum allowed number is 100000000";
				Log.WarnFormat(errorMessage);
				return Json(new { error = errorMessage }, JsonRequestBehavior.AllowGet);
			}

			BoolActionResult result = serviceClient.Instance.SaveBasicInterestRate(sortedList.ToArray());
			return Json(new { error = result.Value ? "Error occurred during save" : null }, JsonRequestBehavior.AllowGet);
		}
		[Ajax]
		[HttpGet]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public JsonResult SettingsLoanOfferMultiplier()
		{
			var loanOfferMultipliersList = serviceClient.Instance.GetSpResultTable("GetLoanOfferMultipliers", null);
			var deserializedArray = JsonConvert.DeserializeObject<LoanOfferMultiplier[]>(loanOfferMultipliersList.SerializedDataTable);
			var loanOfferMultipliers = deserializedArray == null ? null : deserializedArray.ToList();
			if (loanOfferMultipliers != null)
			{
				foreach (LoanOfferMultiplier loanOfferMultiplier in loanOfferMultipliers)
				{
					loanOfferMultiplier.Multiplier *= 100; // Convert to percent
				}
			}

			return Json(new { loanOfferMultipliers }, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpPost]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public JsonResult SaveLoanOfferMultiplier(string serializedModels)
		{
			var deserializedModels = JsonConvert.DeserializeObject<List<LoanOfferMultiplier>>(serializedModels);
			var sortedModels = new SortedDictionary<int, LoanOfferMultiplier>();
			var sortedList = new List<LoanOfferMultiplier>();
			foreach (LoanOfferMultiplier model in deserializedModels)
			{
				if (sortedModels.ContainsKey(model.StartScore))
				{
					string errorMessage = string.Format("StartScore must be unique:{0}", model.StartScore);
					Log.WarnFormat(errorMessage);
					return Json(new { error = errorMessage }, JsonRequestBehavior.AllowGet);
				}
				sortedModels.Add(model.StartScore, model);
			}

			bool isFirst = true;
			int highestSoFar = 0;
			foreach (int key in sortedModels.Keys)
			{
				LoanOfferMultiplier model = sortedModels[key];
				sortedList.Add(model);
				model.Multiplier /= 100; // Convert to decimal number
				if (isFirst)
				{
					if (model.StartScore != 0)
					{
						const string errorMessage = "StartScore must start at 0";
						Log.WarnFormat(errorMessage);
						return Json(new { error = errorMessage }, JsonRequestBehavior.AllowGet);
					}
					isFirst = false;
				}
				else
				{
					if (highestSoFar + 1 < model.StartScore)
					{
						string errorMessage = string.Format("No range covers the numbers {0}-{1}", highestSoFar + 1, model.StartScore - 1);
						Log.WarnFormat(errorMessage);
						return Json(new { error = errorMessage }, JsonRequestBehavior.AllowGet);
					}
					if (highestSoFar + 1 > model.StartScore)
					{
						string errorMessage = string.Format("The numbers {0}-{1} are coverered by more than one range", model.StartScore, highestSoFar);
						Log.WarnFormat(errorMessage);
						return Json(new { error = errorMessage }, JsonRequestBehavior.AllowGet);
					}
				}
				highestSoFar = model.EndScore;
			}

			if (highestSoFar < 100000000)
			{
				string errorMessage = string.Format("No range covers the numbers {0}-100000000", highestSoFar);
				Log.WarnFormat(errorMessage);
				return Json(new { error = errorMessage }, JsonRequestBehavior.AllowGet);
			}

			if (highestSoFar > 100000000)
			{
				const string errorMessage = "Maximum allowed number is 100000000";
				Log.WarnFormat(errorMessage);
				return Json(new { error = errorMessage }, JsonRequestBehavior.AllowGet);
			}

			BoolActionResult result = serviceClient.Instance.SaveLoanOfferMultiplier(sortedList.ToArray());
			return Json(new { error = result.Value ? "Error occurred during save" : null }, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpPost]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public JsonResult AddCampaign(

			string campaignName,
			string campaignDescription,
			int? campaignType,
			string campaignStartDate,
			string campaignEndDate,
			string campaignCustomers,
			int? campaignId
			)
		{
			if (string.IsNullOrEmpty(campaignName) || string.IsNullOrEmpty(campaignStartDate) ||
				string.IsNullOrEmpty(campaignEndDate) || !campaignType.HasValue)
			{
				return Json(new { success = false, errorText = "One or more parameters missing" }, JsonRequestBehavior.AllowGet);
			}


			DateTime startDate = DateTime.ParseExact(campaignStartDate, "dd/MM/yyyy", null);
			DateTime endDate = DateTime.ParseExact(campaignEndDate, "dd/MM/yyyy", null);

			if (endDate < startDate)
			{
				return Json(new { success = false, errorText = "End date prior to start date" }, JsonRequestBehavior.AllowGet);
			}

			Campaign campaign = campaignId.HasValue ? _campaignRepository.Get(campaignId) : new Campaign();

			campaign.Name = campaignName;
			campaign.CampaignType = _campaignTypeRepository.Get(campaignType.Value);
			campaign.StartDate = startDate;
			campaign.EndDate = endDate;
			campaign.Description = campaignDescription;


			var campClients = campaign.Clients.ToArray();
			foreach (var client in campClients)
			{
				campaign.Clients.Remove(client);
			}

			_campaignRepository.SaveOrUpdate(campaign);

			if (string.IsNullOrEmpty(campaignCustomers))
			{
				return Json(new { success = true, errorText = "" }, JsonRequestBehavior.AllowGet);
			}
			string error = "";
			var clients = campaignCustomers.Trim().Split(' ');
			foreach (string client in clients)
			{
				if (string.IsNullOrWhiteSpace(client)) continue;

				int customerId;
				if (int.TryParse(client, out customerId))
				{
					try
					{
						var customer = _customerRepository.TryGet(customerId);
						if (customer != null && campaign.Clients.All(cc => cc.Customer != customer))
						{
							campaign.Clients.Add(new CampaignClients { Campaign = campaign, Customer = customer });
						}
						else
						{
							error += customerId + " not a valid customer id.";
						}
					}
					catch (Exception)
					{
						error += customerId + " not a valid customer id.";
					}
				}
				else
				{
					error += client + " not a valid customer id.";
				}
			}
			Log.DebugFormat("{0}, {1}, {2}, {3}, {4}, {5}. ", campaignName, campaignDescription, campaignType, startDate, endDate, campaignCustomers);
			return Json(new { success = true, errorText = error }, JsonRequestBehavior.AllowGet);
		}

		private void UpdateConfigVars()
		{
			var c = new ServiceClient();
			c.Instance.UpdateConfigurationVariables();
			CurrentValues.ReInit();
		}
	}
}