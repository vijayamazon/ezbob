﻿namespace Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Reject {
	using System.Globalization;
	using AutomationCalculator.ProcessHistory;
	using AutomationCalculator.ProcessHistory.AutoRejection;
	using AutomationCalculator.ProcessHistory.Trails;
	using Ezbob.Logger;

	internal class Checker {
		public Checker(RejectionTrail oTrail, ASafeLog oLog) {
			Trail = oTrail;
			Log = oLog.Safe();
		} // constructor

		public RejectionTrail Trail { get; private set; }

		public ASafeLog Log { get; private set; }

		public Checker Run() {
			Preventers();
			Trail.LockDecision();
			Regulars();

			Trail.HasApprovalChance = 
				(!this.lowPersonalScore || !this.lowBusinessScore) &&
				!this.unresolvedPersonalDefaults &&
				!this.companyTooYoung;

			return this;
		} // Run

		private void Preventers() {
			WasApproved();
			HighAnnualTurnover();
			IsBroker();
			HighConsumerScore();
			HighBusinessScore();
			MpErrors();
			ConsumerDataAge();
		} // Preventers

		private void Regulars() {
			LowConsumerScore();
			LowBusinessScore();
			PersonalDefauls();
			BusinessDefaults();
			CompanyAge();
			CustomerStatus();
			CompanyFiles();
			LateAccounts();
		} // Regulars

		private void WasApproved() {
			if (Trail.MyInputData.WasApproved)
				StepNoReject<WasApprovedPreventer>().Init(Trail.MyInputData.WasApproved);
			else
				StepNoDecision<WasApprovedPreventer>().Init(Trail.MyInputData.WasApproved);
		} // WasApproved

		private void HighAnnualTurnover() {
			if (Trail.MyInputData.AnnualTurnover > Trail.MyInputData.AutoRejectionException_AnualTurnover) {
				StepNoReject<AnnualTurnoverPreventer>().Init(Trail.MyInputData.AnnualTurnover, Trail.MyInputData.AutoRejectionException_AnualTurnover, units: "£");
			} else {
				StepNoDecision<AnnualTurnoverPreventer>().Init(Trail.MyInputData.AnnualTurnover, Trail.MyInputData.AutoRejectionException_AnualTurnover, units: "£");
			} // if
		} // HighAnnualTurnover

		private void IsBroker() {
			// Pay attention, the trace being checked is Auto Approve trace - this is not an error.
			bool isBrokerTraceEnabled =
				Trail.MyInputData.IsTraceEnabled<AutomationCalculator.ProcessHistory.AutoApproval.IsBrokerCustomer>();

			if (!isBrokerTraceEnabled) {
				StepNoDecision<BrokerClientPreventer>().Init(Trail.MyInputData.IsBrokerClient);
				return;
			} // if

			if (Trail.MyInputData.IsBrokerClient)
				StepNoReject<BrokerClientPreventer>().Init(Trail.MyInputData.IsBrokerClient);
			else
				StepNoDecision<BrokerClientPreventer>().Init(Trail.MyInputData.IsBrokerClient);
		} // IsBroker

		private void HighConsumerScore() {
			if (Trail.MyInputData.ConsumerScore > Trail.MyInputData.AutoRejectionException_CreditScore) {
				StepNoReject<ConsumerScorePreventer>()
					.Init(Trail.MyInputData.ConsumerScore, Trail.MyInputData.AutoRejectionException_CreditScore);
			} else {
				StepNoDecision<ConsumerScorePreventer>()
					.Init(Trail.MyInputData.ConsumerScore, Trail.MyInputData.AutoRejectionException_CreditScore);
			} // if
		} // HighConsumerScore

		private void HighBusinessScore() {
			if (Trail.MyInputData.BusinessScore > Trail.MyInputData.RejectionExceptionMaxCompanyScore) {
				StepNoReject<BusinessScorePreventer>()
					.Init(Trail.MyInputData.BusinessScore, Trail.MyInputData.RejectionExceptionMaxCompanyScore);
			} else {
				StepNoDecision<BusinessScorePreventer>()
					.Init(Trail.MyInputData.BusinessScore, Trail.MyInputData.RejectionExceptionMaxCompanyScore);
			} // if
		} // HighBusinessScore

		private void MpErrors() {
			var data = new MarketPlaceWithErrorPreventer.DataModel {
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
				StepNoReject<MarketPlaceWithErrorPreventer>().Init(data);
			else
				StepNoDecision<MarketPlaceWithErrorPreventer>().Init(data);
		} // MpErrors

		private void ConsumerDataAge() {
			Log.Debug(
				"Consumer data age status:\n\tdata time: {0}\n\tnow: {1}\n" +
				"\tmonth count: {2}\n\tnow - month count: {3}\n\ttoo old: {4}",
				Trail.MyInputData.ConsumerDataTime.HasValue
					? Trail.MyInputData.ConsumerDataTime.Value.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture)
					: "-- null --",
				Trail.MyInputData.DataAsOf.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture),
				Trail.MyInputData.AutoRejectConsumerCheckAge,
				Trail.MyInputData.DataAsOf
					.AddMonths(-Trail.MyInputData.AutoRejectConsumerCheckAge)
					.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture),
				Trail.MyInputData.ConsumerDataIsTooOld ? "yes" : "no"
			);

			if (Trail.MyInputData.ConsumerDataIsTooOld) {
				StepNoReject<ConsumerDataTooOldPreventer>()
					.Init(Trail.MyInputData.ConsumerDataTime, Trail.InputData.DataAsOf);
			} else {
				StepNoDecision<ConsumerDataTooOldPreventer>()
					.Init(Trail.MyInputData.ConsumerDataTime, Trail.InputData.DataAsOf);
			} // if
		} // ConsumerDataAge

		private void LowConsumerScore() {
			this.lowPersonalScore =
				(0 < Trail.MyInputData.ConsumerScore) &&
				(Trail.MyInputData.ConsumerScore < Trail.MyInputData.LowCreditScore);

			if (this.lowPersonalScore) {
				StepReject<ConsumerScore>()
					.Init(Trail.MyInputData.ConsumerScore, 0, Trail.MyInputData.LowCreditScore, false);
			} else {
				StepNoDecision<ConsumerScore>()
					.Init(Trail.MyInputData.ConsumerScore, 0, Trail.MyInputData.LowCreditScore, false);
			} // if
		} // LowConsumerScore

		private void LowBusinessScore() {
			this.lowBusinessScore =
				(0 < Trail.MyInputData.BusinessScore) &&
				(Trail.MyInputData.BusinessScore < Trail.MyInputData.RejectionCompanyScore);

			if (this.lowBusinessScore) {
				StepReject<BusinessScore>()
					.Init(Trail.MyInputData.BusinessScore, 0, Trail.MyInputData.RejectionCompanyScore, false);
			} else {
				StepNoDecision<BusinessScore>()
					.Init(Trail.MyInputData.BusinessScore, 0, Trail.MyInputData.RejectionCompanyScore, false);
			} // if
		} // LowBusinessScore

		private void PersonalDefauls() {
			var data = new ConsumerDefaults.DataModel {
				MaxConsumerScore = Trail.MyInputData.ConsumerScore,
				MaxConsumerScoreThreshhold = Trail.MyInputData.Reject_Defaults_CreditScore,
				AmountOfDefaults = Trail.MyInputData.DefaultAmountInConsumerAccounts,
				AmountDefaultAccountsThreshhold = Trail.MyInputData.Reject_Defaults_Amount,
				NumOfDefaults = Trail.MyInputData.NumOfDefaultConsumerAccounts,
				NumDefaultAccountsThreshhold = Trail.MyInputData.Reject_Defaults_AccountsNum,
			};

			this.unresolvedPersonalDefaults =
				(Trail.MyInputData.NumOfDefaultConsumerAccounts >= Trail.MyInputData.Reject_Defaults_AccountsNum);

			bool bReject =
				(Trail.MyInputData.ConsumerScore < Trail.MyInputData.Reject_Defaults_CreditScore) &&
				this.unresolvedPersonalDefaults;

			if (bReject)
				StepReject<ConsumerDefaults>().Init(data);
			else
				StepNoDecision<ConsumerDefaults>().Init(data);
		} // PersonalDefaults

		private void BusinessDefaults() {
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

		private void CompanyAge() {
			this.companyTooYoung =
				(0 < Trail.MyInputData.BusinessSeniorityDays) &&
				(Trail.MyInputData.BusinessSeniorityDays < Trail.MyInputData.Reject_Minimal_Seniority);

			if (this.companyTooYoung) {
				StepReject<Seniority>()
					.Init(Trail.MyInputData.BusinessSeniorityDays, 0, Trail.MyInputData.Reject_Minimal_Seniority, false);
			} else {
				StepNoDecision<Seniority>()
					.Init(Trail.MyInputData.BusinessSeniorityDays, 0, Trail.MyInputData.Reject_Minimal_Seniority, false);
			} // if
		} // CompanyAge

		private void CustomerStatus() {
			if ((Trail.MyInputData.CustomerStatus == "Enabled") || (Trail.MyInputData.CustomerStatus == "Fraud Suspect"))
				StepNoDecision<CustomerStatus>().Init(Trail.MyInputData.CustomerStatus);
			else
				StepReject<CustomerStatus>().Init(Trail.MyInputData.CustomerStatus);
		} // CustomerStatus

		private void CompanyFiles() {
			var data = new AutomationCalculator.ProcessHistory.AutoRejection.Turnover.DataModel {
				AnnualTurnover = Trail.MyInputData.AnnualTurnover,
				AnnualTurnoverThreshhold = Trail.MyInputData.TotalAnnualTurnover,
				QuarterTurnover = Trail.MyInputData.QuarterTurnover,
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

		private void LateAccounts() {
			var data = new ConsumerLates.DataModel {
				LateDays = Trail.MyInputData.ConsumerLateDays,
				LateDaysThreshhold = Trail.MyInputData.RejectionLastValidLate,
				NumOfLates = Trail.MyInputData.NumOfLateConsumerAccounts,
				NumOfLatesThreshhold = Trail.MyInputData.Reject_NumOfLateAccounts,
			};

			if (data.NumOfLates >= data.NumOfLatesThreshhold)
				StepReject<ConsumerLates>().Init(data);
			else
				StepNoDecision<ConsumerLates>().Init(data);
		} // LateAccounts

		private T StepNoReject<T>() where T : ATrace {
			return Trail.Negative<T>(true);
		} // StepNoReject

		private T StepReject<T>() where T : ATrace {
			return Trail.Affirmative<T>(true);
		} // StepReject

		private T StepNoDecision<T>() where T : ATrace {
			return Trail.Dunno<T>();
		} // StepNoDecision

		private bool lowPersonalScore;
		private bool lowBusinessScore;
		private bool companyTooYoung;
		private bool unresolvedPersonalDefaults;
	} // class Checker
} // namespace
