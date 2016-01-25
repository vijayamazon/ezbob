namespace AutomationCalculator.AutoDecision.AutoRejection {
	using System;
	using System.Collections.Generic;
	using AutomationCalculator.AutoDecision.AutoRejection.Models;
	using AutomationCalculator.Common;
	using AutomationCalculator.ProcessHistory;
	using AutomationCalculator.ProcessHistory.AutoRejection;
	using AutomationCalculator.ProcessHistory.Common;
	using AutomationCalculator.ProcessHistory.Trails;
	using AutomationCalculator.Turnover;
	using Ezbob.Database;
	using Ezbob.Logger;

	/// <summary>
	/// Rejection Agent is will determine weather client should be auto rejected or not
	/// </summary>
	public class RejectionAgent {
		/// <summary>
		/// Constructor get db, log customer id and rejection configuration variables
		/// </summary>
		public RejectionAgent(
			AConnection oDB,
			ASafeLog oLog,
			int nCustomerID,
			long? cashRequestID,
			long? nlCashRequestID,
			RejectionConfigs configs = null
		) {
			this.customerId = nCustomerID;

			this.log = oLog;
			this.db = oDB;

			this.dbHelper = new DbHelper(oDB, oLog);

			this.configs = configs ?? this.dbHelper.GetRejectionConfigs();

			Trail = new RejectionTrail(nCustomerID, cashRequestID, nlCashRequestID, oLog);
		} // constructor

		/// <summary>
		/// Retrieves customer's rejection input data
		/// </summary>
		/// <param name="dataAsOf">optional parameter to retrieve historical data for rejection</param>
		/// <returns></returns>
		public RejectionInputData GetRejectionInputData(DateTime? dataAsOf) {
			DateTime now = dataAsOf ?? DateTime.UtcNow;

			var model = new RejectionInputData();

			AutoRejectionInputDataModelDb dbData = this.db.FillFirst<AutoRejectionInputDataModelDb>(
				"AV_GetRejectionData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@CustomerId", this.customerId)
			);

			var originationTime = new OriginationTime(this.log);

			this.db.ForEachRowSafe(
				originationTime.Process,
				"LoadCustomerMarketplaceOriginationTimes",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", this.customerId)
			);

			originationTime.FromExperian(dbData.IncorporationDate);

			double days = originationTime.Since.HasValue ? (now - originationTime.Since.Value).TotalDays : 0;

			CaisStatusesCalculation consumerCaisStatusesCalculation = new CaisStatusesCalculation(this.db, this.log);

			List<CaisStatus> consumerCais = consumerCaisStatusesCalculation.GetConsumerCaisStatuses(this.customerId);

			ConsumerLatesModel lates = consumerCaisStatusesCalculation.GetLates(
				this.customerId,
				now,
				this.configs.RejectionLastValidLate,
				this.configs.Reject_LateLastMonthsNum,
				consumerCais
			);

			var consumerDefaults = consumerCaisStatusesCalculation.GetDefaults(
				this.customerId,
				now,
				this.configs.Reject_Defaults_Amount,
				this.configs.Reject_Defaults_MonthsNum,
				consumerCais
			);

			var businessCais = this.dbHelper.GetBusinessCaisStatuses(this.customerId);

			var businessDefaults = consumerCaisStatusesCalculation.GetDefaults(
				this.customerId, 
				now,
				this.configs.Reject_Defaults_CompanyAmount,
				this.configs.Reject_Defaults_CompanyMonthsNum,
				businessCais
			);

			RejectionTurnover turnover = GetTurnoverForRejection(now);

			var data = new RejectionInputData {
				IsBrokerClient = dbData.IsBrokerClient,
				CustomerStatus = dbData.CustomerStatus,
				ConsumerScore = dbData.ExperianScore,
				BusinessScore = dbData.CompanyScore,
				WasApproved = dbData.WasApproved,
				NumOfDefaultConsumerAccounts = consumerDefaults.NumOfDefaults,
				NumOfDefaultBusinessAccounts = businessDefaults.NumOfDefaults,
				DefaultAmountInConsumerAccounts = consumerDefaults.DefaultsAmount,
				DefaultAmountInBusinessAccounts = businessDefaults.DefaultsAmount,
				HasMpError = dbData.HasErrorMp,
				HasCompanyFiles = dbData.HasCompanyFiles,
				BusinessSeniorityDays = (int)days,
				AnnualTurnover = turnover.Annual,
				QuarterTurnover = turnover.Quarter,
				NumOfLateConsumerAccounts = lates.NumOfLates,
				ConsumerLateDays = lates.LateDays,
				ConsumerDataTime = dbData.ConsumerDataTime,
			};

			model.Init(now, data, this.configs);
			return model;
		} // GetRejectionInputData

		/// <summary>
		/// Main logic flow function to determine weather to auto reject the customer or not 
		/// </summary>
		/// <param name="data">rejection input data</param>
		public void MakeDecision(RejectionInputData data) {
			Trail.Init(data);

			this.log.Debug("Secondary: checking if auto reject should take place for customer {0}...", this.customerId);

			try {
				CheckRejectionExceptions();
				Trail.LockDecision();
				CheckRejections();
			} catch (Exception e) {
				this.log.Error(e, "Exception during auto rejection.");
				StepNoDecision<ExceptionThrown>().Init(e);
			} // try

			Trail.HasApprovalChance = 
				(!this.lowPersonalScore || !this.lowBusinessScore) &&
				!this.unresolvedPersonalDefaults &&
				!this.companyTooYoung;

			Trail.DecideIfNotDecided();

			this.log.Debug(
				"Secondary: checking if auto reject should take place for customer {0} complete; {1}",
				this.customerId,
				Trail
			);
		} // MakeDecision

		public RejectionTrail Trail { get; private set; }

		/// <summary>
		/// Rejection exception steps - if one of them determins no reject - the client won't be auto rejected
		/// </summary>
		private void CheckRejectionExceptions() {
			CheckWasApproved();
			CheckHighAnnualTurnover();
			CheckBrokerClient();
			CheckHighConsumerScore();
			CheckHighBusinessScore();
			CheckMpError();
			CheckConsumerDataTime();
		} // CheckRejectionExceptions

		private void CheckWasApproved() {
			if (Trail.MyInputData.WasApproved)
				StepNoReject<WasApprovedPreventer>(true).Init(Trail.MyInputData.WasApproved);
			else
				StepNoDecision<WasApprovedPreventer>().Init(Trail.MyInputData.WasApproved);
		} // CheckWasApproved

		private void CheckHighAnnualTurnover() {
			if (Trail.MyInputData.AnnualTurnover > Trail.MyInputData.AutoRejectionException_AnualTurnover) {
				StepNoReject<AnnualTurnoverPreventer>(true).Init(
					Trail.MyInputData.AnnualTurnover,
					Trail.MyInputData.AutoRejectionException_AnualTurnover,
					units: "£"
				);
			} else {
				StepNoDecision<AnnualTurnoverPreventer>().Init(
					Trail.MyInputData.AnnualTurnover,
					Trail.MyInputData.AutoRejectionException_AnualTurnover,
					units: "£"
				);
			}
		} // CheckHighAnnualTurnover

		private void CheckBrokerClient() {
			// Pay attention, the trace being checked is Auto Approve trace - this is not an error.
			bool isBrokerTraceEnabled =
				Trail.MyInputData.IsTraceEnabled<AutomationCalculator.ProcessHistory.AutoApproval.IsBrokerCustomer>();

			if (!isBrokerTraceEnabled) {
				StepNoDecision<BrokerClientPreventer>().Init(Trail.MyInputData.IsBrokerClient);
				return;
			} // if

			if (Trail.MyInputData.IsBrokerClient)
				StepNoReject<BrokerClientPreventer>(true).Init(Trail.MyInputData.IsBrokerClient);
			else
				StepNoDecision<BrokerClientPreventer>().Init(Trail.MyInputData.IsBrokerClient);
		} // CheckBrokerClient

		private void CheckHighConsumerScore() {
			if (Trail.MyInputData.ConsumerScore > Trail.MyInputData.AutoRejectionException_CreditScore) {
				StepNoReject<ConsumerScorePreventer>(true).Init(
					Trail.MyInputData.ConsumerScore,
					Trail.MyInputData.AutoRejectionException_CreditScore
				);
			} else {
				StepNoDecision<ConsumerScorePreventer>().Init(
					Trail.MyInputData.ConsumerScore,
					Trail.MyInputData.AutoRejectionException_CreditScore
				);
			} // if
		} // CheckHighConsumerScore

		private void CheckHighBusinessScore() {
			if (Trail.MyInputData.BusinessScore > Trail.MyInputData.RejectionExceptionMaxCompanyScore) {
				StepNoReject<BusinessScorePreventer>(true).Init(
					Trail.MyInputData.BusinessScore,
					Trail.MyInputData.RejectionExceptionMaxCompanyScore
				);
			} else {
				StepNoDecision<BusinessScorePreventer>().Init(
					Trail.MyInputData.BusinessScore,
					Trail.MyInputData.RejectionExceptionMaxCompanyScore
				);
			} // if
		} // CheckHighBusinessScore

		private void CheckMpError() {
			var data = new MarketPlaceWithErrorPreventer.DataModel {
				HasMpError = Trail.MyInputData.HasMpError,
				MaxBusinessScore = Trail.MyInputData.BusinessScore,
				MaxBusinessScoreThreshhold = Trail.MyInputData.RejectionExceptionMaxCompanyScoreForMpError,
				MaxConsumerScore = Trail.MyInputData.ConsumerScore,
				MaxConsumerScoreThreshhold = Trail.MyInputData.RejectionExceptionMaxConsumerScoreForMpError
			};

			if (data.NotRejectStep)
				StepNoReject<MarketPlaceWithErrorPreventer>(true).Init(data);
			else
				StepNoDecision<MarketPlaceWithErrorPreventer>().Init(data);
		} // CheckMpError

		private void CheckConsumerDataTime() {
			if (Trail.MyInputData.ConsumerDataIsTooOld) {
				StepNoReject<ConsumerDataTooOldPreventer>(true).Init(
					Trail.MyInputData.ConsumerDataTime,
					Trail.InputData.DataAsOf
				);
			} else {
				StepNoDecision<ConsumerDataTooOldPreventer>().Init(
					Trail.MyInputData.ConsumerDataTime,
					Trail.InputData.DataAsOf
				);
			} // if
		} // CheckConsumerDataTime

		/// <summary>
		/// Rejection steps - if one of the steps determine reject -
		/// the client will be rejected (if none of the rejection preventer rules where true).
		/// </summary>
		private void CheckRejections() {
			CheckLowConsumerScore();
			CheckLowBusinessScore();
			CheckConsumerDefaults();
			CheckCompanyDefaults();
			CheckSeniority();
			CheckCustomerStatus();
			CheckLowTurnover();
			CheckConsumerLates();
		} // CheckRejections

		private void CheckLowConsumerScore() {
			if (Trail.MyInputData.ConsumerScore > 0 && Trail.MyInputData.ConsumerScore < Trail.MyInputData.LowCreditScore) {
				this.lowPersonalScore = true;

				StepReject<ConsumerScore>(true).Init(
					Trail.MyInputData.ConsumerScore,
					0,
					Trail.MyInputData.LowCreditScore,
					false
				);
			} else {
				this.lowPersonalScore = false;

				StepNoDecision<ConsumerScore>().Init(
					Trail.MyInputData.ConsumerScore,
					0,
					Trail.MyInputData.LowCreditScore,
					false
				);
			} // if
		} // CheckLowConsumerScore

		private void CheckLowBusinessScore() {
			this.lowBusinessScore =
				(Trail.MyInputData.BusinessScore > 0) &&
				(Trail.MyInputData.BusinessScore < Trail.MyInputData.RejectionCompanyScore);

			if (this.lowBusinessScore) {
				StepReject<BusinessScore>(true).Init(
					Trail.MyInputData.BusinessScore,
					0,
					Trail.MyInputData.RejectionCompanyScore,
					false
				);
			} else {
				StepNoDecision<BusinessScore>().Init(
					Trail.MyInputData.BusinessScore,
					0,
					Trail.MyInputData.RejectionCompanyScore,
					false
				);
			} // if
		} // CheckLowBusinessScore

		private void CheckConsumerDefaults() {
			var data = new ConsumerDefaults.DataModel {
				MaxConsumerScore = Trail.MyInputData.ConsumerScore,
				MaxConsumerScoreThreshhold = Trail.MyInputData.Reject_Defaults_CreditScore,
				AmountOfDefaults = Trail.MyInputData.DefaultAmountInConsumerAccounts,
				AmountDefaultAccountsThreshhold = Trail.MyInputData.Reject_Defaults_Amount,
				NumOfDefaults = Trail.MyInputData.NumOfDefaultConsumerAccounts,
				NumDefaultAccountsThreshhold = Trail.MyInputData.Reject_Defaults_AccountsNum
			};

			this.unresolvedPersonalDefaults = data.TooManyDefaults;

			if (data.RejectStep)
				StepReject<ConsumerDefaults>(true).Init(data);
			else
				StepNoDecision<ConsumerDefaults>().Init(data);
		} // CheckConsumerDefaults

		private void CheckCompanyDefaults() {
			var data = new BusinessDefaults.DataModel {
				MaxBusinessScore = Trail.MyInputData.BusinessScore,
				MaxBusinessScoreThreshhold = Trail.MyInputData.Reject_Defaults_CompanyScore,
				AmountOfDefaults = Trail.MyInputData.DefaultAmountInBusinessAccounts,
				AmountDefaultAccountsThreshhold = Trail.MyInputData.Reject_Defaults_CompanyAmount,
				NumOfDefaults = Trail.MyInputData.NumOfDefaultBusinessAccounts,
				NumDefaultAccountsThreshhold = Trail.MyInputData.Reject_Defaults_CompanyAccountsNum
			};

			if (data.RejectStep)
				StepReject<BusinessDefaults>(true).Init(data);
			else
				StepNoDecision<BusinessDefaults>().Init(data);
		} // CheckCompanyDefaults

		private void CheckSeniority() {
			this.companyTooYoung =
				0 < Trail.MyInputData.BusinessSeniorityDays &&
				Trail.MyInputData.BusinessSeniorityDays < Trail.MyInputData.Reject_Minimal_Seniority;

			if (this.companyTooYoung) {
				StepReject<Seniority>(true).Init(
					Trail.MyInputData.BusinessSeniorityDays, 
					0,
					Trail.MyInputData.Reject_Minimal_Seniority,
					false
				);
			} else {
				StepNoDecision<Seniority>().Init(
					Trail.MyInputData.BusinessSeniorityDays,
					0,
					Trail.MyInputData.Reject_Minimal_Seniority,
					false
				);
			} // if
		} // CheckSeniority

		private void CheckCustomerStatus() {
			if (Trail.MyInputData.CustomerStatus == "Enabled" || Trail.MyInputData.CustomerStatus == "Fraud Suspect")
				StepNoDecision<CustomerStatus>().Init(Trail.MyInputData.CustomerStatus);
			else
				StepReject<CustomerStatus>(true).Init(Trail.MyInputData.CustomerStatus);
		} // CheckCustomerStatus

		private void CheckLowTurnover() {
			var data = new Turnover.DataModel {
				AnnualTurnover = Trail.MyInputData.AnnualTurnover,
				AnnualTurnoverThreshhold = Trail.MyInputData.TotalAnnualTurnover,
				QuarterTurnover = Trail.MyInputData.QuarterTurnover,
				QuarterTurnoverThreshhold = Trail.MyInputData.TotalThreeMonthTurnover,
				HasCompanyFiles = Trail.MyInputData.HasCompanyFiles
			};

			bool rejectStep = (
					data.AnnualTurnover < data.AnnualTurnoverThreshhold ||
					data.QuarterTurnover < data.QuarterTurnoverThreshhold
				) &&
				!data.HasCompanyFiles;

			if (rejectStep)
				StepReject<Turnover>(true).Init(data);
			else
				StepNoDecision<Turnover>().Init(data);
		} // CheckLowTurnover

		private void CheckConsumerLates() {
			var data = new ConsumerLates.DataModel {
				LateDays = Trail.MyInputData.ConsumerLateDays,
				LateDaysThreshhold = Trail.MyInputData.RejectionLastValidLate,
				NumOfLates = Trail.MyInputData.NumOfLateConsumerAccounts,
				NumOfLatesThreshhold = Trail.MyInputData.Reject_NumOfLateAccounts
			};

			if (data.LateDays > data.LateDaysThreshhold && data.NumOfLates >= data.NumOfLatesThreshhold)
				StepReject<ConsumerLates>(true).Init(data);
			else
				StepNoDecision<ConsumerLates>().Init(data);
		} // CheckConsumerLates

		private T StepReject<T>(bool bLockDecisionAfterAddingAStep) where T : ATrace {
			return Trail.Affirmative<T>(bLockDecisionAfterAddingAStep);
		} // StepReject

		private T StepNoReject<T>(bool bLockDecisionAfterAddingAStep) where T : ATrace {
			return Trail.Negative<T>(bLockDecisionAfterAddingAStep);
		} // StepNoReject

		private T StepNoDecision<T>() where T : ATrace {
			return Trail.Dunno<T>();
		} // StepReject

		public class RejectionTurnover : Tuple<decimal, decimal> {
			public RejectionTurnover(decimal annual, decimal quarter) : base(annual, quarter) { } // constructor

			public decimal Annual {
				get { return Item1; }
			} // Annual

			public decimal Quarter {
				get { return Item2; }
			} // Annual
		} // RejectionTurnover

		/// <summary>
		/// Calculates figures for 4 categories annual (max of annualized 1m 3m 6m and 1y),
		/// and quarter (max of 3 month not annualized): hmrc, yodlee, online, payment.
		/// Returns max of 4 categories for annual turnover and quarter turnover.
		/// </summary>
		/// <returns>Rejection turnover, annual and quarter.</returns>
		private RejectionTurnover GetTurnoverForRejection(DateTime now) {
			var turnover = new AutoRejectTurnover();
			turnover.Init();

			this.db.ForEachResult<TurnoverDbRow>(
				row => turnover.Add(row),
				"GetCustomerTurnoverForAutoDecision",
				new QueryParameter("@IsForApprove", false),
				new QueryParameter("@CustomerID", customerId),
				new QueryParameter("@Now", now)
			);

			return new RejectionTurnover(turnover[12], turnover[3]);
		} // GetTurnoverForRejection

		private readonly AConnection db;
		private readonly ASafeLog log;

		private readonly int customerId;
		private readonly RejectionConfigs configs;

		private readonly DbHelper dbHelper;

		private bool lowPersonalScore;
		private bool lowBusinessScore;
		private bool companyTooYoung;
		private bool unresolvedPersonalDefaults;
	} // class RejectionAgent
} // namespace
