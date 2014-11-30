namespace EzBob.Backend.Strategies.MainStrategy.AutoDecisions.Reject {
	using AutomationCalculator.ProcessHistory;
	using AutomationCalculator.ProcessHistory.AutoRejection;
	using AutomationCalculator.ProcessHistory.Trails;

	internal class Checker {
		#region public

		public RejectionTrail Trail { get; private set; }

		#region constructor

		public Checker(RejectionTrail oTrail) {
			Trail = oTrail;
		} // constructor

		#endregion constructor

		#region exceptions

		public void WasApproved() {
			if (Trail.MyInputData.WasApproved)
				StepNoReject<WasApproved>().Init(Trail.MyInputData.WasApproved);
			else
				StepNoDecision<WasApproved>().Init(Trail.MyInputData.WasApproved);
		} // WasApproved

		public void HighAnnualTurnover() {
			if (Trail.MyInputData.AnnualTurnover > Trail.MyInputData.AutoRejectionException_AnualTurnover)
				StepNoReject<AnnualTurnoverException>().Init(Trail.MyInputData.AnnualTurnover, Trail.MyInputData.AutoRejectionException_AnualTurnover);
			else
				StepNoDecision<AnnualTurnoverException>().Init(Trail.MyInputData.AnnualTurnover, Trail.MyInputData.AutoRejectionException_AnualTurnover);
		} // HighAnnualTurnover

		public void IsBroker() {
			if (Trail.MyInputData.IsBrokerClient)
				StepNoReject<BrokerClientException>().Init(Trail.MyInputData.IsBrokerClient);
			else
				StepNoDecision<BrokerClientException>().Init(Trail.MyInputData.IsBrokerClient);
		} // IsBroker

		public void HighConsumerScore() {
			if (Trail.MyInputData.ConsumerScore > Trail.MyInputData.AutoRejectionException_CreditScore)
				StepNoReject<ConsumerScoreException>().Init(Trail.MyInputData.ConsumerScore, Trail.MyInputData.AutoRejectionException_CreditScore);
			else
				StepNoDecision<ConsumerScoreException>().Init(Trail.MyInputData.ConsumerScore, Trail.MyInputData.AutoRejectionException_CreditScore);
		} // HighConsumerScore

		public void HighBusinessScore() {
			if (Trail.MyInputData.BusinessScore > Trail.MyInputData.RejectionExceptionMaxCompanyScore)
				StepNoReject<BusinessScoreException>().Init(Trail.MyInputData.BusinessScore, Trail.MyInputData.RejectionExceptionMaxCompanyScore);
			else
				StepNoDecision<BusinessScoreException>().Init(Trail.MyInputData.BusinessScore, Trail.MyInputData.RejectionExceptionMaxCompanyScore);
		} // HighBusinessScore

		public void MpErrors() {
			var data = new MarketPlaceWithErrorException.DataModel {
				HasMpError = Trail.MyInputData.HasMpError,
				MaxBusinessScore = Trail.MyInputData.BusinessScore,
				MaxBusinessScoreThreshhold = Trail.MyInputData.RejectionExceptionMaxCompanyScoreForMpError,
				MaxConsumerScore = Trail.MyInputData.ConsumerScore,
				MaxConsumerScoreThreshhold = Trail.MyInputData.RejectionExceptionMaxConsumerScoreForMpError,
			};

			bool bNoReject = Trail.MyInputData.HasMpError && (
				(Trail.MyInputData.ConsumerScore > Trail.MyInputData.RejectionExceptionMaxConsumerScoreForMpError) ||
				(Trail.MyInputData.BusinessScore > Trail.MyInputData.RejectionExceptionMaxCompanyScoreForMpError)
			);

			if (bNoReject)
				StepNoReject<MarketPlaceWithErrorException>().Init(data);
			else
				StepNoDecision<MarketPlaceWithErrorException>().Init(data);
		} // MpErrors

		#endregion exceptions

		#region regular checks

		public void LowConsumerScore() {
			if ((0 < Trail.MyInputData.ConsumerScore) && (Trail.MyInputData.ConsumerScore < Trail.MyInputData.LowCreditScore))
				StepReject<ConsumerScore>().Init(Trail.MyInputData.ConsumerScore, 0, Trail.MyInputData.LowCreditScore, false);
			else
				StepNoDecision<ConsumerScore>().Init(Trail.MyInputData.ConsumerScore, 0, Trail.MyInputData.LowCreditScore, false);
		} // LowConsumerScore

		public void LowBusinessScore() {
			if ((0 < Trail.MyInputData.BusinessScore) && (Trail.MyInputData.BusinessScore < Trail.MyInputData.RejectionCompanyScore))
				StepReject<BusinessScore>().Init(Trail.MyInputData.BusinessScore, 0, Trail.MyInputData.RejectionCompanyScore, false);
			else
				StepNoDecision<BusinessScore>().Init(Trail.MyInputData.BusinessScore, 0, Trail.MyInputData.RejectionCompanyScore, false);
		} // LowBusinessScore

		public void PersonalDefauls() {
			var data = new ConsumerDefaults.DataModel {
				MaxConsumerScore = Trail.MyInputData.ConsumerScore,
				MaxConsumerScoreThreshhold = Trail.MyInputData.Reject_Defaults_CreditScore,
				AmountOfDefaults = Trail.MyInputData.DefaultAmountInConsumerAccounts,
				AmountDefaultAccountsThreshhold = Trail.MyInputData.Reject_Defaults_Amount,
				NumOfDefaults = Trail.MyInputData.NumOfDefaultConsumerAccounts,
				NumDefaultAccountsThreshhold = Trail.MyInputData.Reject_Defaults_AccountsNum,
			};

			bool bReject =
				(Trail.MyInputData.ConsumerScore < Trail.MyInputData.Reject_Defaults_CreditScore) &&
				(Trail.MyInputData.NumOfDefaultConsumerAccounts >= Trail.MyInputData.Reject_Defaults_AccountsNum);

			if (bReject)
				StepReject<ConsumerDefaults>().Init(data);
			else
				StepNoDecision<ConsumerDefaults>().Init(data);
		} // PersonalDefaults

		public void BusinessDefaults() {
			var data = new BusinessDefaults.DataModel {
				MaxBusinessScore = Trail.MyInputData.BusinessScore,
				MaxBusinessScoreThreshhold = Trail.MyInputData.Reject_Defaults_CompanyScore,
				AmountOfDefaults = Trail.MyInputData.DefaultAmountInBusinessAccounts,
				AmountDefaultAccountsThreshhold = Trail.MyInputData.Reject_Defaults_CompanyAmount,
				NumOfDefaults = Trail.MyInputData.NumOfDefaultBusinessAccounts,
				NumDefaultAccountsThreshhold = Trail.MyInputData.Reject_Defaults_CompanyAccountsNum,
			};

			bool bReject =
				(Trail.MyInputData.BusinessScore < Trail.MyInputData.Reject_Defaults_CompanyScore) &&
				(Trail.MyInputData.NumOfDefaultBusinessAccounts >= Trail.MyInputData.Reject_Defaults_CompanyAccountsNum);

			if (bReject)
				StepReject<BusinessDefaults>().Init(data);
			else
				StepNoDecision<BusinessDefaults>().Init(data);
		} // BusinessDefaults

		public void CompanyAge() {
			if ((0 < Trail.MyInputData.BusinessSeniorityDays) && (Trail.MyInputData.BusinessSeniorityDays < Trail.MyInputData.Reject_Minimal_Seniority))
				StepReject<Seniority>().Init(Trail.MyInputData.BusinessSeniorityDays, 0, Trail.MyInputData.Reject_Minimal_Seniority, false);
			else
				StepNoDecision<BusinessScore>().Init(Trail.MyInputData.BusinessSeniorityDays, 0, Trail.MyInputData.Reject_Minimal_Seniority, false);
		} // CompanyAge

		public void CustomerStatus() {
			if ((Trail.MyInputData.CustomerStatus == "Enabled") || (Trail.MyInputData.CustomerStatus == "Fraud Suspect"))
				StepNoDecision<CustomerStatus>().Init(Trail.MyInputData.CustomerStatus);
			else
				StepReject<CustomerStatus>().Init(Trail.MyInputData.CustomerStatus);
		} // CustomerStatus

		public void CompanyFiles() {
			var data = new AutomationCalculator.ProcessHistory.AutoRejection.Turnover.DataModel {
				AnnualTurnover = (int)Trail.MyInputData.AnnualTurnover,
				AnnualTurnoverThreshhold = Trail.MyInputData.TotalAnnualTurnover,
				QuarterTurnover = (int)Trail.MyInputData.QuarterTurnover,
				QuarterTurnoverThreshhold = Trail.MyInputData.TotalThreeMonthTurnover,
				HasCompanyFiles = Trail.MyInputData.HasCompanyFiles,
			};

			bool bReject = (
				(data.AnnualTurnover < data.AnnualTurnoverThreshhold) ||
				(data.QuarterTurnover < data.QuarterTurnoverThreshhold)
			) && !data.HasCompanyFiles;

			if (bReject)
				StepReject<AutomationCalculator.ProcessHistory.AutoRejection.Turnover>().Init(data);
			else
				StepNoDecision<AutomationCalculator.ProcessHistory.AutoRejection.Turnover>().Init(data);
		} // CompanyFiles

		public void LateAccounts() {
			var data = new ConsumerLates.DataModel {
				LateDays = Trail.MyInputData.ConsumerLateDays,
				LateDaysThreshhold = Trail.MyInputData.RejectionLastValidLate,
				NumOfLates = Trail.MyInputData.NumOfLateConsumerAccounts,
				NumOfLatesThreshhold = Trail.MyInputData.Reject_NumOfLateAccounts,
			};

			if ((data.LateDays > data.LateDaysThreshhold) && (data.NumOfLates >= data.NumOfLatesThreshhold))
				StepReject<ConsumerLates>().Init(data);
			else
				StepNoDecision<ConsumerLates>().Init(data);
		} // LateAccounts

		#endregion regular checks

		#endregion public

		#region private

		#region method StepNoReject

		private T StepNoReject<T>() where T : ATrace {
			return Trail.Negative<T>(true);
		} // StepNoReject

		#endregion method StepNoReject

		#region method StepReject

		private T StepReject<T>() where T : ATrace {
			return Trail.Affirmative<T>(true);
		} // StepReject

		#endregion method StepReject

		#region method StepNoDecision

		private T StepNoDecision<T>() where T : ATrace {
			return Trail.Dunno<T>();
		} // StepNoDecision

		#endregion method StepReject

		#endregion private
	} // class Checker
} // namespace
