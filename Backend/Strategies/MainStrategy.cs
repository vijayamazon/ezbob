namespace Strategies
{
	using System;
	using System.Globalization;
	using System.Threading;
	using ExperianLib;
	using ExperianLib.Ebusiness;
	using EzBob.Models;
	using EzBobIntegration.Web_References.Consumer;
	using log4net;
	using System.Collections.Generic;

	public enum NewCreditLineOption
	{
		SkipEverything = 1,
		UpdateEverythingExceptMp = 2,
		UpdateEverythingAndApplyAutoRules = 3,
		UpdateEverythingAndGoToManualDecision = 4,
	}

	public class MainStrategy
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(MainStrategy));
		private readonly StrategiesMailer mailer = new StrategiesMailer();


		// main strategy - 1
		public void Evaluate(int customerId, NewCreditLineOption newCreditLineOption, int avoidAutomaticDescison, bool isUnderwriterForced = false)
		{
			StrategyHelper strategyHelper = new StrategyHelper();
			// TODO: remove column Customer.LastStartedMainStrategy

			/*new StrategyParameter("userId", user.Id),
            new StrategyParameter("Underwriter_Check", isUnderwriterForced ? 1 : 0),
            new StrategyParameter("NewCreditLineOption", (int)newCreditLineOption),
            new StrategyParameter("AvoidAutomaticDescison", avoidAutomaticDescison)*/

			// TODO: Read from ConfigurationVariables (ConfigurationVariables_MainStrat)
			string BWABusinessCheck = string.Empty;
			bool EnableAutomaticRejection = false;
			bool EnableAutomaticReApproval = false;
			bool EnableAutomaticApproval = false;
			int LowCreditScore = 1;
			int LowTotalAnnualTurnover = 1;
			int LowTotalThreeMonthTurnover = 1;
			int MaxCapHomeOwner = 1;
			int MaxCapNotHomeOwner = 1;
			int Reject_Defaults_CreditScore = 1;
			int Reject_Defaults_AccountsNum = 1;
			int Reject_Defaults_Amount = 1;
			int Reject_Defaults_MonthsNum = 1;
			bool EnableAutomaticReRejection = false;
			int AutoRejectionException_CreditScore = 1;
			int AutoRejectionException_AnualTurnover = 1;
			int Reject_Minimal_Seniority = 1;
			int AutoReApproveMaxNumOfOutstandingLoans = 1;
			bool AutoApproveIsSilent = false;
			string AutoApproveSilentTemplateName = string.Empty;
			string AutoApproveSilentToAddress = string.Empty;
			//string ScortoInternalErrorMessage = string.Empty;

			strategyHelper.GetZooplaData(customerId);




			// TODO: get personal info (GetPersonalInfo)
			string App_email = string.Empty;
			string App_FirstName = string.Empty;
			string App_Surname = string.Empty;
			DateTime App_DateOfBirth = DateTime.Now;
			string App_HomeOwner = string.Empty;
			string App_MaritalStatus = string.Empty;
			int App_TimeAtAddress = 1;
			string App_Gender = string.Empty;
			string CompanyType = string.Empty;
			string App_AccountNumber = string.Empty;
			string App_SortCode = string.Empty;
			string App_LimitedRefNum = string.Empty;
			string App_NonLimitedRefNum = string.Empty;
			decimal App_OverallTurnOver = 1;
			decimal App_WebSiteTurnOver = 1;
			DateTime App_ApplyForLoan = DateTime.Now;
			DateTime App_ValidFor = DateTime.Now;
			DateTime App_RegistrationDate = DateTime.Now;
			string App_RefNumber = string.Empty;
			string App_BankAccountType = string.Empty;
			int Prev_ExperianConsumerScore = 1;
			bool CustomerStatusIsEnabled = false;
			bool CustomerStatusIsWarning = false;
			bool IsOffline = false;
			bool HasAccountingAccounts = false;
			string ScortoInternalErrorMessage = string.Empty;



			string ExperianLimitedError = null;
			string ExperianNonLimitedError = null;
			decimal ExperianBureauScoreLimited = 0;
			decimal ExperianExistingBusinessLoans = 0;
			decimal ExperianBureauScoreNonLimited = 0;
			bool ExperianCompanyNotFoundOnBureau = false;
			string ExperianConsumerError = string.Empty;
			string ExperianConsumerErrorPrev = string.Empty;
			double ExperianConsumerScore = 0;
			double MinExperianScore = 0;
			double Inintial_ExperianConsumerScore = 0;
			double ExperianScoreConsumer = 0;
			DateTime ExperianBirthDate = DateTime.Now;



			if (!CustomerStatusIsEnabled || CustomerStatusIsWarning)
			{
				EnableAutomaticReApproval = false;
				EnableAutomaticApproval = false;
			}


			if (IsOffline)
			{
				EnableAutomaticReApproval = false;
				EnableAutomaticApproval = false;
				EnableAutomaticReRejection = false;
				EnableAutomaticRejection = false;
			}

			if (newCreditLineOption != NewCreditLineOption.SkipEverything && newCreditLineOption != NewCreditLineOption.UpdateEverythingExceptMp)
			{
				if (!WaitForMarketplacesToFinishUpdates(customerId))
				{
					string customerEmail = null;

					// TODO: get customerEmail from customerId

					var variables = new Dictionary<string, string>
						{
							{"UserEmail", customerEmail},
							{"CustomerID", customerId.ToString(CultureInfo.InvariantCulture)},
							{"ApplicationID", customerEmail}
						};
					mailer.SendToEzbob(variables, "Mandrill - No Information about shops", "No information about customer marketplace");
					return;
				}
			}

			if (newCreditLineOption != NewCreditLineOption.SkipEverything)
			{
				if (CompanyType == "Limited" || CompanyType == "LLP")
				{
					if (string.IsNullOrEmpty(App_LimitedRefNum))
					{
						ExperianLimitedError = "RefNumber is empty";
					}
					else
					{
						var service = new EBusinessService();
						LimitedResults limitedData = service.GetLimitedBusinessData(App_LimitedRefNum, customerId);

						if (!limitedData.IsError)
						{
							ExperianBureauScoreLimited = limitedData.BureauScore;
							ExperianExistingBusinessLoans = limitedData.ExistingBusinessLoans;
						}
						else
						{
							ExperianLimitedError = limitedData.Error;
						}
					}

					// TODO: call UpdateExperianBusiness (App_LimitedRefNum,ExperianLimitedError,ExperianBureauScoreLimited)
				}
				else if (CompanyType == "PShip3P" || CompanyType == "PShip" || CompanyType == "SoleTrader")
				{
					if (string.IsNullOrEmpty(App_NonLimitedRefNum))
					{
						ExperianNonLimitedError = "RefNumber is empty";
					}
					else
					{
						var service = new EBusinessService();
						var nonlimitedData = service.GetNotLimitedBusinessData(App_NonLimitedRefNum, customerId);
						if (!nonlimitedData.IsError)
						{
							ExperianBureauScoreNonLimited = nonlimitedData.BureauScore;
							ExperianCompanyNotFoundOnBureau = nonlimitedData.CompanyNotFoundOnBureau;
						}
						else
						{
							ExperianNonLimitedError = nonlimitedData.Error;
						}
					}

					// TODO: call UpdateExperianBusiness (App_NonLimitedRefNum,ExperianNonLimitedError,ExperianBureauScoreNonLimited)
				}

				// TODO: call Get_Customer_Address_and_Previous_Address
				string App_Line1 = null;
				string App_Line2 = null;
				string App_Line3 = null;
				string App_Line4 = null;
				string App_Line5 = null;
				string App_Line6 = null;
				string App_Line1Prev = null;
				string App_Line2Prev = null;
				string App_Line3Prev = null;
				string App_Line4Prev = null;
				string App_Line5Prev = null;
				string App_Line6Prev = null;



				var consumerService = new ConsumerService();
				var location = new InputLocationDetailsMultiLineLocation();
				location.LocationLine1 = App_Line1;
				location.LocationLine2 = App_Line2;
				location.LocationLine3 = App_Line3;
				location.LocationLine4 = App_Line4;
				location.LocationLine5 = App_Line5;
				location.LocationLine6 = App_Line6;
				var result = consumerService.GetConsumerInfo(App_FirstName, App_Surname, App_Gender, App_DateOfBirth, null, location,"PL" , customerId, 0); // TODO: verify the null in 5th parameter is ok

				if (result.IsError)
				{
					ExperianConsumerError = result.Error;
				}
				else
				{
					ExperianConsumerScore = result.BureauScore;
					ExperianBirthDate = result.BirthDate;
				}

				if (!string.IsNullOrEmpty(ExperianConsumerError) && App_TimeAtAddress == 1 && !string.IsNullOrEmpty(App_Line6Prev))
				{
					var consumerServiceForPrev = new ConsumerService();
					var prevLocation = new InputLocationDetailsMultiLineLocation();
					prevLocation.LocationLine1 = App_Line1Prev;
					prevLocation.LocationLine2 = App_Line2Prev;
					prevLocation.LocationLine3 = App_Line3Prev;
					prevLocation.LocationLine4 = App_Line4Prev;
					prevLocation.LocationLine5 = App_Line5Prev;
					prevLocation.LocationLine6 = App_Line6Prev;

					result = consumerServiceForPrev.GetConsumerInfo(App_FirstName, App_Surname, App_Gender, App_DateOfBirth, null, location, "PL", customerId, 0); // TODO: verify the null in 5th parameter is ok

					if (result.IsError)
					{
						ExperianConsumerErrorPrev = result.Error;
					}
					else
					{
						ExperianConsumerScore = result.BureauScore;
						ExperianBirthDate = result.BirthDate;
					}
				}

				if (ExperianBirthDate.Year == 1900 && ExperianBirthDate.Month == 1 && ExperianBirthDate.Day == 1)
				{
					ExperianBirthDate = App_DateOfBirth;
				}

				MinExperianScore = ExperianConsumerScore;
				Inintial_ExperianConsumerScore = ExperianConsumerScore;
				ExperianScoreConsumer = ExperianConsumerScore;

				// Continue with 2 calls to UpdateExperianConsumer
			}

			// Continue with MP_GetScoreCardData...
		}

		private bool WaitForMarketplacesToFinishUpdates(int customerId)
		{
			bool isUpdated = false; // TODO: get from MP_IsCustomerMarketPlacesUpdated
			int totalTimeToWaitInSeconds = 43200; //TODO put in ConfigurationVariables
			int intervalInMilliseconds = 300000; //TODO put in ConfigurationVariables
			DateTime startWaitingTime = DateTime.UtcNow;

			while (!isUpdated)
			{
				if ((DateTime.UtcNow - startWaitingTime).TotalSeconds > totalTimeToWaitInSeconds)
				{
					return false;
				}

				Thread.Sleep(intervalInMilliseconds);
				isUpdated = false; // TODO: get from MP_IsCustomerMarketPlacesUpdated
			}

			return true;
		}
	}
}
