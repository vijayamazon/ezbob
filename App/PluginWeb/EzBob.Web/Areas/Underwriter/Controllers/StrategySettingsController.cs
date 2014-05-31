﻿namespace EzBob.Web.Areas.Underwriter.Controllers
{
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using System.Web.Mvc;
	using Backend.Models;
	using ConfigManager;
	using EZBob.DatabaseLib.Model;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Backend.Models;
	using Infrastructure;
	using Infrastructure.Attributes;
	using Infrastructure.csrf;
	using Models;
	using Newtonsoft.Json;
	using System;
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;
	using StructureMap;
	using log4net;

	public class StrategySettingsController : Controller
	{
		private readonly ServiceClient serviceClient;
		private readonly IConfigurationVariablesRepository _configurationVariablesRepository;
		private readonly CampaignRepository _campaignRepository;
		private readonly CampaignTypeRepository _campaignTypeRepository;
		private readonly CustomerRepository _customerRepository;
		private static readonly ILog Log = LogManager.GetLogger(typeof(StrategySettingsController));
		private readonly IWorkplaceContext context;

		public StrategySettingsController(IConfigurationVariablesRepository configurationVariablesRepository, CampaignRepository campaignRepository, CampaignTypeRepository campaignTypeRepository, CustomerRepository customerRepository)
		{
			context = ObjectFactory.GetInstance<IWorkplaceContext>();
			_configurationVariablesRepository = configurationVariablesRepository;
			_campaignRepository = campaignRepository;
			_campaignTypeRepository = campaignTypeRepository;
			_customerRepository = customerRepository;
			serviceClient = new ServiceClient();
		}

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpGet]
		public JsonResult Index()
		{
			return Json(string.Empty, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpGet]
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
		public JsonResult SettingsPricingModel()
		{
			PricingModelModelActionResult getPricingModelModelResponse = serviceClient.Instance.GetPricingModelModel(0, context.UserId, "Basic");
			return Json(getPricingModelModelResponse.Value, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpPost]
		public JsonResult SettingsPricingModelForScenario(string scenarioName)
		{
			PricingModelModelActionResult getPricingModelModelResponse = serviceClient.Instance.GetPricingModelModel(0, context.UserId, scenarioName);
			return Json(getPricingModelModelResponse.Value, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpPost]
		public JsonResult SettingsSavePricingModelScenario(string scenarioName, string model)
		{
			PricingModelModel inputModel = JsonConvert.DeserializeObject<PricingModelModel>(model);
			serviceClient.Instance.SavePricingModelSettings(context.UserId, scenarioName, inputModel);
			return SettingsPricingModelForScenario(scenarioName);
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

		private void UpdateSettingsGeneral(string BWABusinessCheck, decimal HmrcSalariesMultiplier, decimal fcfFactor) {
			Transactional.Execute(() => {
				_configurationVariablesRepository.SetByName("BWABusinessCheck", BWABusinessCheck);

				if (HmrcSalariesMultiplier >= 0 && HmrcSalariesMultiplier <= 1)
					_configurationVariablesRepository.SetByName(Variables.HmrcSalariesMultiplier.ToString(), HmrcSalariesMultiplier.ToString(CultureInfo.InvariantCulture));

				_configurationVariablesRepository.SetByName(Variables.FCFFactor.ToString(), fcfFactor.ToString(CultureInfo.InvariantCulture));
				//_configurationVariablesRepository.SetByName("DisplayEarnedPoints", DisplayEarnedPoints);
			});
		}

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpGet]
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

		private void UpdateSettingsCharges(string administrationCharge, string latePaymentCharge, string otherCharge,
										   string partialPaymentCharge, string rolloverCharge, string amountToChargeFrom)
		{
			Transactional.Execute(() => {
				_configurationVariablesRepository.SetByName("AdministrationCharge", administrationCharge);
				_configurationVariablesRepository.SetByName("LatePaymentCharge", latePaymentCharge);
				_configurationVariablesRepository.SetByName("OtherCharge", otherCharge);
				_configurationVariablesRepository.SetByName("PartialPaymentCharge", partialPaymentCharge);
				_configurationVariablesRepository.SetByName("RolloverCharge", rolloverCharge);
				_configurationVariablesRepository.SetByName("AmountToChargeFrom", amountToChargeFrom);
			}, IsolationLevel.ReadUncommitted);
		}

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpGet]
		public JsonResult AutomationGeneral()
		{
			return Json(string.Empty, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpPost]
		public JsonResult AutomationGeneral(string[] newSettings)
		{
			return Json(string.Empty, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpGet]
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

		private void UpdateAutomationApproval(string EnableAutomaticApproval, string EnableAutomaticReApproval,
											  string MaxCapHomeOwner, string MaxCapNotHomeOwner) {
			Transactional.Execute(() => {
				_configurationVariablesRepository.SetByName("EnableAutomaticApproval", EnableAutomaticApproval);
				_configurationVariablesRepository.SetByName("EnableAutomaticReApproval", EnableAutomaticReApproval);
				_configurationVariablesRepository.SetByName("MaxCapHomeOwner", MaxCapHomeOwner);
				_configurationVariablesRepository.SetByName("MaxCapNotHomeOwner", MaxCapNotHomeOwner);
			});
		}

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpGet]
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

		private void UpdateAutomationRejection(
			string EnableAutomaticRejection, string LowCreditScore,
			string Reject_Defaults_AccountsNum, string Reject_Defaults_Amount,
			string Reject_Defaults_CreditScore, string Reject_Defaults_MonthsNum,
			string Reject_Minimal_Seniority, string TotalAnnualTurnover,
			string TotalThreeMonthTurnover, string EnableAutomaticReRejection,
			string AutoRejectionException_CreditScore,
			string AutoRejectionException_AnualTurnover
		) {
			Transactional.Execute(() => {
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
				_configurationVariablesRepository.SetByName("AutoRejectionException_AnualTurnover", AutoRejectionException_AnualTurnover);
			});
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

		private void UpdateSettingsExperian(
			string FinancialAccounts_MainApplicant,
			string FinancialAccounts_AliasOfMainApplicant,
			string FinancialAccounts_AssociationOfMainApplicant,
			string FinancialAccounts_JointApplicant,
			string FinancialAccounts_AliasOfJointApplicant,
			string FinancialAccounts_AssociationOfJointApplicant,
			string FinancialAccounts_No_Match
		) {
			Transactional.Execute(() => {
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
			});
		}

		[Ajax]
		[HttpGet]
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
		[Transactional]
		public JsonResult SettingsConfigTable(string tableName)
		{
			var entriesList = serviceClient.Instance.GetSpResultTable("GetConfigTable", new []{"TableName", tableName});
			var deserializedArray = JsonConvert.DeserializeObject<ConfigTable[]>(entriesList.SerializedDataTable);
			var configTableEntries = deserializedArray == null ? null : deserializedArray.ToList();
			if (configTableEntries != null)
			{
				foreach (ConfigTable entry in configTableEntries)
				{
					entry.Value *= 100; // Convert to percent
				}
			}

			return Json(new { configTableEntries = configTableEntries }, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpPost]
		[Transactional]
		public JsonResult SaveConfigTable(string serializedModels, string configTableType)
		{
			ConfigTableType c;
			switch (configTableType)
			{
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
				case "BasicInterestRate":
				default:
					c = ConfigTableType.BasicInterestRate;
					break;
			}
			var deserializedModels = JsonConvert.DeserializeObject<List<ConfigTable>>(serializedModels);

			BoolActionResult result = serviceClient.Instance.SaveConfigTable(deserializedModels.ToArray(), c);
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

		private void UpdateConfigVars() {
			var c = new ServiceClient();
			c.Instance.UpdateConfigurationVariables();
			CurrentValues.ReInit();
		}
	}
}