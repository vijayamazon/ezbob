namespace EzBob.Backend.Strategies.MainStrategy
{
	using System;
	using ConfigManager;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Misc;

	public class DataGatherer
	{
		private readonly int customerId;
		private readonly AConnection db;
		private readonly ASafeLog log;

		// Config values
		public int RejectDefaultsCreditScore { get; private set; }
		public int RejectDefaultsAccountsNum { get; private set; }
		public int RejectMinimalSeniority { get; private set; }
		public string BwaBusinessCheck { get; private set; }
		public bool EnableAutomaticReRejection { get; set; } // TODO: make the set private
		public bool EnableAutomaticReApproval { get; set; } // TODO: make the set private
		public bool EnableAutomaticApproval { get; set; } // TODO: make the set private
		public bool EnableAutomaticRejection { get; set; } // TODO: make the set private
		public int MaxCapHomeOwner { get; private set; }
		public int MaxCapNotHomeOwner { get; private set; }
		public int LowCreditScore { get; private set; }
		public int LowTotalAnnualTurnover { get; private set; }
		public int LowTotalThreeMonthTurnover { get; private set; }
		public int DefaultFeedbackValue { get; private set; }
		public int LimitedMedalMinOffer { get; private set; }

		// Customer info
		public bool CustomerStatusIsEnabled { get; private set; }
		public bool CustomerStatusIsWarning { get; private set; }
		public string CustomerStatusName { get; private set; }
		public bool IsOffline { get; private set; }
		public bool IsBrokerCustomer { get; private set; } // TODO: fetch from DB BrokerId and apply logic within main
		public string AppEmail { get; private set; }
		public string CompanyType { get; private set; }
		public string AppFirstName { get; private set; }
		public string AppSurname { get; private set; }
		public string AppGender { get; private set; }
		public bool WasMainStrategyExecutedBefore { get; private set; } // TODO: fetch from DB LastStartedMainStrategyEndTime and apply logic within main
		public bool IsOwnerOfMainAddress { get; private set; }
		public bool IsOwnerOfOtherProperties { get; private set; }
		public string PropertyStatusDescription { get; private set; }
		public int AllMPsNum { get; private set; }
		public string AppAccountNumber { get; private set; }
		public string AppSortCode { get; private set; }
		public DateTime AppRegistrationDate { get; private set; }
		public string AppBankAccountType { get; private set; }
		public string TypeOfBusiness;
		public int NumOfLoans { get; private set; }
		public int NumOfHmrcMps { get; private set; }
		public int CompanySeniorityDays { get; private set; } // TODO: Get incorporation date and calc the days in main

		public DataGatherer(int customerId, AConnection db, ASafeLog log)
		{
			this.customerId = customerId;
			this.db = db;
			this.log = log;
		}

		public void Gather()
		{
			ReadConfigurations();
			GetPersonalInfo();
			GetCompanySeniorityDays();
		}

		private void ReadConfigurations()
		{
			RejectDefaultsCreditScore = CurrentValues.Instance.Reject_Defaults_CreditScore;
			RejectDefaultsAccountsNum = CurrentValues.Instance.Reject_Defaults_AccountsNum;
			RejectMinimalSeniority = CurrentValues.Instance.Reject_Minimal_Seniority;
			BwaBusinessCheck = CurrentValues.Instance.BWABusinessCheck;
			EnableAutomaticApproval = CurrentValues.Instance.EnableAutomaticApproval;
			EnableAutomaticReApproval = CurrentValues.Instance.EnableAutomaticReApproval;
			EnableAutomaticRejection = CurrentValues.Instance.EnableAutomaticRejection;
			EnableAutomaticReRejection = CurrentValues.Instance.EnableAutomaticReRejection;
			MaxCapHomeOwner = CurrentValues.Instance.MaxCapHomeOwner;
			MaxCapNotHomeOwner = CurrentValues.Instance.MaxCapNotHomeOwner;
			LowCreditScore = CurrentValues.Instance.LowCreditScore;
			LowTotalAnnualTurnover = CurrentValues.Instance.TotalAnnualTurnover;
			LowTotalThreeMonthTurnover = CurrentValues.Instance.TotalThreeMonthTurnover;
			DefaultFeedbackValue = CurrentValues.Instance.DefaultFeedbackValue;
			LimitedMedalMinOffer = CurrentValues.Instance.LimitedMedalMinOffer;
		}

		private void GetPersonalInfo()
		{
			log.Info("Getting personal info for customer:{0}", customerId);
			SafeReader results = db.GetFirst("GetPersonalInfo", CommandSpecies.StoredProcedure, new QueryParameter("CustomerId", customerId));

			CustomerStatusIsEnabled = results["CustomerStatusIsEnabled"];
			CustomerStatusIsWarning = results["CustomerStatusIsWarning"];
			CustomerStatusName = results["CustomerStatusName"];
			IsOffline = results["IsOffline"];
			IsBrokerCustomer = results["IsBrokerCustomer"];
			AppEmail = results["CustomerEmail"];
			CompanyType = results["CompanyType"];
			WasMainStrategyExecutedBefore = results["MainStrategyExecutedBefore"];
			AppFirstName = results["FirstName"];
			AppSurname = results["Surname"];
			AppGender = results["Gender"];
			IsOwnerOfMainAddress = results["IsOwnerOfMainAddress"];
			IsOwnerOfOtherProperties = results["IsOwnerOfOtherProperties"];
			PropertyStatusDescription = results["PropertyStatusDescription"];
			AllMPsNum = results["NumOfMps"];
			AppAccountNumber = results["AccountNumber"];
			AppSortCode = results["SortCode"];
			AppRegistrationDate = results["RegistrationDate"];
			AppBankAccountType = results["BankAccountType"];
			NumOfLoans = results["NumOfLoans"];
			TypeOfBusiness = results["TypeOfBusiness"];
			NumOfHmrcMps = results["NumOfHmrcMps"];
		}

		private void GetCompanySeniorityDays()
		{
			// TODO: create IsLimited method elsewhere
			var seniority = new GetCompanySeniority(customerId, TypeOfBusiness == "Limited" || TypeOfBusiness == "LLP", db, log);
			seniority.Execute();
			CompanySeniorityDays = seniority.CompanyIncorporationDate.HasValue
								   ? (DateTime.UtcNow - seniority.CompanyIncorporationDate.Value).Days
								   : 0;
		}
	}
}
