namespace EzBob.Backend.Strategies.MainStrategy
{
	using System;
	using ConfigManager;
	using Experian;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Misc;

	public class DataGatherer
	{
		private readonly int customerId;
		private readonly AConnection db;
		private readonly ASafeLog log;

		// Preliminary
		public string BwaBusinessCheck { get; private set; }
		public string AppBankAccountType { get; private set; }
		public string AppAccountNumber { get; private set; }
		public string AppSortCode { get; private set; }
		public DateTime? LastStartedMainStrategyEndTime { get; private set; }

		// Config values
		public int RejectDefaultsCreditScore { get; private set; }
		public int RejectDefaultsAccountsNum { get; private set; }
		public int RejectMinimalSeniority { get; private set; }
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
		public string AppFirstName { get; private set; }
		public string AppSurname { get; private set; }
		public string AppGender { get; private set; }
		public bool IsOwnerOfMainAddress { get; private set; }
		public bool IsOwnerOfOtherProperties { get; private set; }
		public string PropertyStatusDescription { get; private set; }
		public int AllMPsNum { get; private set; }
		public DateTime AppRegistrationDate { get; private set; }
		public string TypeOfBusiness;
		public int NumOfLoans { get; private set; }
		public int NumOfHmrcMps { get; private set; }
		public bool IsAlibaba { get; private set; }
		public int? BrokerId { get; private set; }
		public DateTime? CompanyIncorporationDate { get; private set; }
		public int MaxCompanyScore { get; private set; }
		public int ExperianConsumerScore { get; private set; }
		public int MinExperianConsumerScore { get; private set; }
		public int MaxExperianConsumerScore { get; private set; }

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
			GetMaxCompanyScore();
			GetCurrentExperianScore();
			GetMinMaxExperianScore();
		}

		public void GatherPreliminaryData()
		{
			BwaBusinessCheck = CurrentValues.Instance.BWABusinessCheck;

			SafeReader results = db.GetFirst("GetPersonalInfo", CommandSpecies.StoredProcedure, new QueryParameter("CustomerId", customerId));
			AppBankAccountType = results["BankAccountType"];
			LastStartedMainStrategyEndTime = results["LastStartedMainStrategyEndTime"];
			AppAccountNumber = results["AccountNumber"];
			AppSortCode = results["SortCode"];
			TypeOfBusiness = results["TypeOfBusiness"];
		}

		private void ReadConfigurations()
		{
			RejectDefaultsCreditScore = CurrentValues.Instance.Reject_Defaults_CreditScore;
			RejectDefaultsAccountsNum = CurrentValues.Instance.Reject_Defaults_AccountsNum;
			RejectMinimalSeniority = CurrentValues.Instance.Reject_Minimal_Seniority;
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
			AppFirstName = results["FirstName"];
			AppSurname = results["Surname"];
			AppGender = results["Gender"];
			IsOwnerOfMainAddress = results["IsOwnerOfMainAddress"];
			IsOwnerOfOtherProperties = results["IsOwnerOfOtherProperties"];
			PropertyStatusDescription = results["PropertyStatusDescription"];
			AllMPsNum = results["NumOfMps"];
			AppRegistrationDate = results["RegistrationDate"];
			NumOfLoans = results["NumOfLoans"];
			NumOfHmrcMps = results["NumOfHmrcMps"];
			IsAlibaba = results["IsAlibaba"];
			BrokerId = results["BrokerId"];
		}

		private void GetCompanySeniorityDays()
		{
			var getCompanySeniority = new GetCompanySeniority(customerId, Utils.IsLimitedCompany(TypeOfBusiness), db, log);
			getCompanySeniority.Execute();
			CompanyIncorporationDate = getCompanySeniority.CompanyIncorporationDate;
		}

		private void GetMaxCompanyScore()
		{
			MaxCompanyScore = db.ExecuteScalar<int>(
				"GetCompanyScore",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId)
			);
		}

		private void GetCurrentExperianScore()
		{
			var scoreStrat = new GetExperianConsumerScore(customerId, db, log);
			scoreStrat.Execute();
			ExperianConsumerScore = scoreStrat.Score;
		}

		private void GetMinMaxExperianScore()
		{
			SafeReader sr = db.GetFirst(
				"GetExperianMinMaxConsumerDirectorsScore",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId)
				);

			if (!sr.IsEmpty)
			{
				MinExperianConsumerScore = sr["MinExperianScore"];
				MaxExperianConsumerScore = sr["MaxExperianScore"];
			}
		}
	}
}
