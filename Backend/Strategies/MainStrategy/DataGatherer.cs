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
		public bool EnableAutomaticReRejection { get; private set; }
		public bool EnableAutomaticReApproval { get; private set; }
		public bool EnableAutomaticApproval { get; private set; }
		public bool EnableAutomaticRejection { get; private set; }
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
		public string AppEmail { get; private set; }
		public string CompanyType { get; private set; }
		public string AppFirstName { get; private set; }
		public string AppSurname { get; private set; }
		public string AppGender { get; private set; }
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
		public bool IsAlibaba { get; private set; }
		public int? BrokerId { get; private set; }
		public DateTime? LastStartedMainStrategyEndTime { get; private set; }
		public DateTime? CompanyIncorporationDate { get; private set; }

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
			AppEmail = results["CustomerEmail"];
			CompanyType = results["CompanyType"];
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
			IsAlibaba = results["IsAlibaba"];
			BrokerId = results["BrokerId"];
			LastStartedMainStrategyEndTime = results["LastStartedMainStrategyEndTime"];
		}

		private void GetCompanySeniorityDays()
		{
			// TODO: create IsLimited method elsewhere
			var getCompanySeniority = new GetCompanySeniority(customerId, Utils.IsLimitedCompany(TypeOfBusiness), db, log);
			getCompanySeniority.Execute();
			CompanyIncorporationDate = getCompanySeniority.CompanyIncorporationDate;
		}
	}
}
