namespace EzBob.Web.Areas.Underwriter.Controllers
{
	using System.Collections.Generic;
	using System.Data;
	using System.Web.Mvc;
	using Code;
	using EZBob.DatabaseLib.Model;
	using EZBob.DatabaseLib.Model.Database;
	using EzServiceReference;
	using Infrastructure.csrf;
	using Models;
	using Newtonsoft.Json;
	using Scorto.Web;
	using System;
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database.Repository;
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
		public JsonNetResult Index()
		{
			return this.JsonNet(string.Empty);
		}

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpGet]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public JsonNetResult SettingsGeneral()
		{
			var bwaBusinessCheck = _configurationVariablesRepository.GetByName("BWABusinessCheck");
			//var displayEarnedPoints = _configurationVariablesRepository.GetByName("DisplayEarnedPoints");

			var st = new
				{
					BWABusinessCheck = bwaBusinessCheck.Value,
					BWABusinessCheckDesc = bwaBusinessCheck.Description,
					//DisplayEarnedPoints = displayEarnedPoints.Value,
					//DisplayEarnedPointsDesc = displayEarnedPoints.Description
				};
			return this.JsonNet(st);
		}

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpPost]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public JsonNetResult SettingsGeneral(string BWABusinessCheck/*, string DisplayEarnedPoints*/)
		{
			_configurationVariablesRepository.SetByName("BWABusinessCheck", BWABusinessCheck);
			//_configurationVariablesRepository.SetByName("DisplayEarnedPoints", DisplayEarnedPoints);
			return SettingsGeneral();
		}

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpGet]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public JsonNetResult SettingsCharges()
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
			return this.JsonNet(sc);
		}

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpPost]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public JsonNetResult SettingsCharges(string administrationCharge,
			string latePaymentCharge,
			string otherCharge,
			string partialPaymentCharge,
			string rolloverCharge,
			string amountToChargeFrom
			)
		{
			_configurationVariablesRepository.SetByName("AdministrationCharge", administrationCharge);
			_configurationVariablesRepository.SetByName("LatePaymentCharge", latePaymentCharge);
			_configurationVariablesRepository.SetByName("OtherCharge", otherCharge);
			_configurationVariablesRepository.SetByName("PartialPaymentCharge", partialPaymentCharge);
			_configurationVariablesRepository.SetByName("RolloverCharge", rolloverCharge);
			_configurationVariablesRepository.SetByName("AmountToChargeFrom", amountToChargeFrom);
			return SettingsCharges();
		}

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpGet]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public JsonNetResult AutomationGeneral()
		{
			return this.JsonNet(string.Empty);
		}

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpPost]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public JsonNetResult AutomationGeneral(string[] newSettings)
		{
			return this.JsonNet(string.Empty);
		}

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpGet]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public JsonNetResult AutomationApproval()
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
			return this.JsonNet(sa);
		}

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpPost]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public JsonNetResult AutomationApproval(
												string EnableAutomaticApproval,
												string EnableAutomaticReApproval,
												string MaxCapHomeOwner,
												string MaxCapNotHomeOwner
			)
		{
			_configurationVariablesRepository.SetByName("EnableAutomaticApproval", EnableAutomaticApproval);
			_configurationVariablesRepository.SetByName("EnableAutomaticReApproval", EnableAutomaticReApproval);
			_configurationVariablesRepository.SetByName("MaxCapHomeOwner", MaxCapHomeOwner);
			_configurationVariablesRepository.SetByName("MaxCapNotHomeOwner", MaxCapNotHomeOwner);
			return AutomationApproval();
		}

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpGet]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public JsonNetResult AutomationRejection()
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
			return this.JsonNet(sr);
		}

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpPost]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public JsonNetResult AutomationRejection(string EnableAutomaticRejection,
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

			return AutomationRejection();
		}

		[Ajax]
		[HttpGet]
		public JsonNetResult SettingsExperian()
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
			return this.JsonNet(model);
		}

		// ReSharper disable  InconsistentNaming
		[Ajax]
		[HttpPost]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public JsonNetResult SettingsExperian(

			string FinancialAccounts_MainApplicant,
			string FinancialAccounts_AliasOfMainApplicant,
			string FinancialAccounts_AssociationOfMainApplicant,
			string FinancialAccounts_JointApplicant,
			string FinancialAccounts_AliasOfJointApplicant,
			string FinancialAccounts_AssociationOfJointApplicant,
			string FinancialAccounts_No_Match)
		{
			_configurationVariablesRepository.SetByName("FinancialAccounts_MainApplicant", FinancialAccounts_MainApplicant);
			_configurationVariablesRepository.SetByName("FinancialAccounts_AliasOfMainApplicant", FinancialAccounts_AliasOfMainApplicant);
			_configurationVariablesRepository.SetByName("FinancialAccounts_AssociationOfMainApplicant", FinancialAccounts_AssociationOfMainApplicant);
			_configurationVariablesRepository.SetByName("FinancialAccounts_JointApplicant", FinancialAccounts_JointApplicant);
			_configurationVariablesRepository.SetByName("FinancialAccounts_AliasOfJointApplicant", FinancialAccounts_AliasOfJointApplicant);
			_configurationVariablesRepository.SetByName("FinancialAccounts_AssociationOfJointApplicant", FinancialAccounts_AssociationOfJointApplicant);
			_configurationVariablesRepository.SetByName("FinancialAccounts_No_Match", FinancialAccounts_No_Match);
			return SettingsGeneral();
		}

		[Ajax]
		[HttpGet]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public JsonNetResult SettingsCampaign()
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
			
			return this.JsonNet(new { campaigns, campaignTypes });
		}

		[Ajax]
		[HttpGet]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public JsonNetResult SettingsBasicInterestRate()
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

			return this.JsonNet(new { basicInterestRates });
		}

		[Ajax]
		[HttpPost]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public JsonNetResult SaveBasicInterestRate(string serializedModels)
		{
			var deserializedModels = JsonConvert.DeserializeObject<List<BasicInterestRate>>(serializedModels);
			var sortedModels = new SortedDictionary<int, BasicInterestRate>();
			var sortedList = new List<BasicInterestRate>();
			foreach (BasicInterestRate model in deserializedModels)
			{
				sortedList.Add(model);
				if (sortedModels.ContainsKey(model.FromScore))
				{
					string errorMessage = string.Format("FromScore must be unique:{0}", model.FromScore);
					Log.WarnFormat(errorMessage);
					return this.JsonNet(new { error = errorMessage });
				}
				sortedModels.Add(model.FromScore, model);
			}

			bool isFirst = true;
			int highestSoFar = 0;
			foreach (int key in sortedModels.Keys)
			{
				BasicInterestRate model = sortedModels[key];
				model.LoanInterestBase /= 100; // Convert to decimal number
				if (isFirst)
				{
					if (model.FromScore != 0)
					{
						const string errorMessage = "FromScore must start at 0";
						Log.WarnFormat(errorMessage);
						return this.JsonNet(new { error = errorMessage });
					}
					isFirst = false;
				}
				else
				{
					if (highestSoFar + 1 < model.FromScore)
					{
						string errorMessage = string.Format("No range covers the numbers {0}-{1}", highestSoFar + 1, model.FromScore - 1);
						Log.WarnFormat(errorMessage);
						return this.JsonNet(new { error = errorMessage });
					}
					if (highestSoFar + 1 > model.FromScore)
					{
						string errorMessage = string.Format("The numbers {0}-{1} are coverered by more than one range", model.FromScore, highestSoFar);
						Log.WarnFormat(errorMessage);
						return this.JsonNet(new { error = errorMessage });
					}
				}
				highestSoFar = model.ToScore;
			}

			if (highestSoFar < 100000000)
			{
				string errorMessage = string.Format("No range covers the numbers {0}-100000000", highestSoFar);
				Log.WarnFormat(errorMessage);
				return this.JsonNet(new { error = errorMessage });
			}
			
			if (highestSoFar > 100000000)
			{
				const string errorMessage = "Maximum allowed number is 100000000";
				Log.WarnFormat(errorMessage);
				return this.JsonNet(new { error = errorMessage });
			}

			BoolActionResult result = serviceClient.Instance.SaveBasicInterestRate(sortedList.ToArray());
			return this.JsonNet(new { error = result.Value ? "Error occurred during save" : null });
		}

		[Ajax]
		[HttpPost]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public JsonNetResult AddCampaign(

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
				return this.JsonNet(new { success = false, errorText = "One or more parameters missing" });
			}


			DateTime startDate = DateTime.ParseExact(campaignStartDate, "dd/MM/yyyy", null);
			DateTime endDate = DateTime.ParseExact(campaignEndDate, "dd/MM/yyyy", null);

			if (endDate < startDate)
			{
				return this.JsonNet(new { success = false, errorText = "End date prior to start date" });
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
				return this.JsonNet(new { success = true, errorText = "" });
			}
			string error = "";
			var clients = campaignCustomers.Trim().Split(' ');
			foreach (string client in clients)
			{
				if(string.IsNullOrWhiteSpace(client)) continue;

				int customerId;
				if (int.TryParse(client, out customerId))
				{
					try
					{
						var customer = _customerRepository.TryGet(customerId);
						if (customer != null && campaign.Clients.All(cc => cc.Customer != customer))
						{
							campaign.Clients.Add(new CampaignClients {Campaign = campaign, Customer = customer});
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
			return this.JsonNet(new { success = true, errorText = error });
		}
	}
}