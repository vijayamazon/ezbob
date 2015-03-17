﻿namespace Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Approval {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using AutomationCalculator.AutoDecision.AutoApproval;
	using AutomationCalculator.ProcessHistory;
	using AutomationCalculator.ProcessHistory.Trails;
	using AutomationCalculator.Turnover;
	using ConfigManager;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using Ezbob.Backend.Strategies.Misc;
	using Ezbob.Database;
	using Ezbob.Logger;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using MailApi;

	public partial class Approval {
		private DateTime Now { get; set; }

		private void NotifyAutoApproveSilentMode(
			decimal autoApprovedAmount,
			int repaymentPeriod,
			decimal interestRate,
			decimal setupFee
		) {
			try {
				var message = string.Format(
					@"<h1><u>Silent auto approve for customer <b style='color:red'>{0}</b></u></h1><br>
					<h2><b>Offer:</b></h2>
					<pre><h3>Amount: {1}</h3></pre><br>
					<pre><h3>Period: {2}</h3></pre><br>
					<pre><h3>Interest rate: {3}</h3></pre><br>
					<pre><h3>Setup fee: {4}</h3></pre><br>
					<h2><b>Decision flow:</b></h2>
					<pre><h3>{5}</h3></pre><br>
					<h2><b>Decision data:</b></h2>
					<pre><h3>{6}</h3></pre>", this.customerId,
					autoApprovedAmount.ToString("C0", Library.Instance.Culture),
					repaymentPeriod,
					interestRate.ToString("P2", Library.Instance.Culture),
					setupFee.ToString("P2", Library.Instance.Culture),
					HttpUtility.HtmlEncode(this.m_oTrail.ToString()),
					HttpUtility.HtmlEncode(this.m_oTrail.InputData.Serialize())
				);

				new Mail().Send(
					CurrentValues.Instance.AutoApproveSilentToAddress,
					null,
					message,
					CurrentValues.Instance.MailSenderEmail,
					CurrentValues.Instance.MailSenderName,
					"#SilentApprove for customer " + this.customerId
				);
			} catch (Exception e) {
				this.log.Error(e, "Failed sending alert mail - silent auto approval.");
			} // try
		} // NotifyAutoApproveSilentMode

		private void SaveTrailInputData(GetAvailableFunds availFunds) {
			this.m_oTrail.MyInputData.SetDataAsOf(Now);

			var cfg = new Configuration {
				ExperianScoreThreshold = CurrentValues.Instance.AutoApproveExperianScoreThreshold,
				CustomerMinAge = CurrentValues.Instance.AutoApproveCustomerMinAge,
				CustomerMaxAge = CurrentValues.Instance.AutoApproveCustomerMaxAge,
				MinTurnover1M = CurrentValues.Instance.AutoApproveMinTurnover1M,
				MinTurnover3M = CurrentValues.Instance.AutoApproveMinTurnover3M,
				MinTurnover1Y = CurrentValues.Instance.AutoApproveMinTurnover1Y,
				MinMPSeniorityDays = CurrentValues.Instance.AutoApproveMinMPSeniorityDays,
				MaxOutstandingOffers = CurrentValues.Instance.AutoApproveMaxOutstandingOffers,
				MaxTodayLoans = CurrentValues.Instance.AutoApproveMaxTodayLoans,
				MaxDailyApprovals = CurrentValues.Instance.AutoApproveMaxDailyApprovals,
				MaxAllowedDaysLate = CurrentValues.Instance.AutoApproveMaxAllowedDaysLate,
				MaxNumOfOutstandingLoans = CurrentValues.Instance.AutoApproveMaxNumOfOutstandingLoans,
				MinRepaidPortion = CurrentValues.Instance.AutoApproveMinRepaidPortion,
				MinLoan = CurrentValues.Instance.MinLoan,
				MaxAmount = CurrentValues.Instance.AutoApproveMaxAmount,
				IsSilent = CurrentValues.Instance.AutoApproveIsSilent,
				SilentTemplateName = CurrentValues.Instance.AutoApproveSilentTemplateName,
				SilentToAddress = CurrentValues.Instance.AutoApproveSilentToAddress,
				BusinessScoreThreshold = CurrentValues.Instance.AutoApproveBusinessScoreThreshold,
				AllowedCaisStatusesWithLoan = CurrentValues.Instance.AutoApproveAllowedCaisStatusesWithLoan,
				AllowedCaisStatusesWithoutLoan = CurrentValues.Instance.AutoApproveAllowedCaisStatusesWithoutLoan,
				OnlineTurnoverAge = CurrentValues.Instance.AutoApproveOnlineTurnoverAge,
				OnlineTurnoverDropQuarterRatio = CurrentValues.Instance.AutoApproveOnlineTurnoverDropQuarterRatio,
				OnlineTurnoverDropMonthRatio = CurrentValues.Instance.AutoApproveOnlineTurnoverDropMonthRatio,
				HmrcTurnoverAge = CurrentValues.Instance.AutoApproveHmrcTurnoverAge,
				HmrcTurnoverDropQuarterRatio = CurrentValues.Instance.AutoApproveHmrcTurnoverDropQuarterRatio,
				HmrcTurnoverDropHalfYearRatio = CurrentValues.Instance.AutoApproveHmrcTurnoverDropHalfYearRatio,
				TurnoverDropQuarterRatio = CurrentValues.Instance.AutoApproveTurnoverDropQuarterRatio,
				Reject_Defaults_Amount = CurrentValues.Instance.Reject_Defaults_Amount,
				Reject_Defaults_MonthsNum = CurrentValues.Instance.Reject_Defaults_MonthsNum,
			};
			
			this.m_oTrail.MyInputData.SetConfiguration(cfg);

			this.db.ForEachRowSafe(
				srName => this.m_oTrail.MyInputData.Configuration.EnabledTraces.Add(srName["Name"]),
				"LoadEnabledTraces",
				CommandSpecies.StoredProcedure
			);

			this.m_oTrail.MyInputData.SetArgs(
				this.customerId,
				this.m_oTrail.SafeAmount,
				(AutomationCalculator.Common.Medal)this.medalClassification,
				this.medalType,
				this.turnoverType
			);

			this.m_oTrail.MyInputData.SetMetaData(new MetaData {
				RowType = "MetaData",
				FirstName = (this.customer != null) && (this.customer.PersonalInfo != null)
					? this.customer.PersonalInfo.FirstName
					: null,
				LastName = (this.customer != null) && (this.customer.PersonalInfo != null)
					? this.customer.PersonalInfo.Surname
					: null,
				IsBrokerCustomer = this.isBrokerCustomer,
				NumOfTodayAutoApproval = CalculateTodaysApprovals(),
				TodayLoanSum = CalculateTodaysLoans(),
				FraudStatusValue = (int)(
					(this.customer == null) ? FraudStatus.UnderInvestigation : this.customer.FraudStatus
				),
				AmlResult = (this.customer == null) ? "failed because customer not found" : this.customer.AMLResult,
				CustomerStatusName = this.customer == null ? "unknown" : this.customer.CollectionStatus.CurrentStatus.Name,
				CustomerStatusEnabled = this.customer != null && this.customer.CollectionStatus.CurrentStatus.IsEnabled,
				CompanyScore = this.minCompanyScore,
				ConsumerScore = this.minExperianScore,
				IncorporationDate = GetCustomerIncorporationDate(),
				DateOfBirth = (
					(this.customer != null) &&
					(this.customer.PersonalInfo != null) &&
					this.customer.PersonalInfo.DateOfBirth.HasValue
				) ? this.customer.PersonalInfo.DateOfBirth.Value : Now,
				NumOfDefaultAccounts = this.m_oConsumerData.FindNumOfPersonalDefaults(
					cfg.Reject_Defaults_Amount,
					Now.AddMonths(-1 * cfg.Reject_Defaults_MonthsNum)
				),
				NumOfRollovers = CalculateRollovers(),
				TotalLoanCount = this.loanRepository.ByCustomer(this.customerId).Count(),
				ExperianCompanyName = (this.customer != null) && (this.customer.Company != null)
					? this.customer.Company.ExperianCompanyName
					: null,
				EnteredCompanyName = (this.customer != null) && (this.customer.Company != null)
					? this.customer.Company.CompanyName
					: null,
				IsLimitedCompanyType =
					(this.customer != null) &&
					(this.customer.PersonalInfo != null) &&
					(this.customer.PersonalInfo.TypeOfBusiness.Reduce() == TypeOfBusinessReduced.Limited),
				PreviousManualApproveCount = (this.customer != null) && (this.customer.CashRequests != null)
					? this.customer.CashRequests.Count(cr =>
						cr.UnderwriterDecision == CreditResultStatus.Approved &&
						cr.UnderwriterDecisionDate.HasValue &&
						cr.UnderwriterDecisionDate.Value < Now
					)
					: 0,
				CompanyDissolutionDate = this.companyDissolutionDate,
			});

			FindOutstandingLoans();

			SafeReader sr = this.db.GetFirst(
				"GetLastOfferDataForApproval",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", this.customerId),
				new QueryParameter("Now", Now)
			);

			this.m_oTrail.MyInputData.MetaData.EmailSendingBanned = sr["EmailSendingBanned"];
			this.m_oTrail.MyInputData.MetaData.OfferStart = sr["OfferStart"];
			this.m_oTrail.MyInputData.MetaData.OfferValidUntil = sr["OfferValidUntil"];

			this.m_oTrail.MyInputData.SetDirectorNames(this.directors);
			this.m_oTrail.MyInputData.SetHmrcBusinessNames(this.hmrcNames);

			this.m_oTrail.MyInputData.SetWorstStatuses(this.consumerCaisDetailWorstStatuses);
			FindLatePayments();
			this.m_oTrail.MyInputData.SetSeniority(CalculateSeniority());
			this.m_oTrail.MyInputData.SetAvailableFunds(availFunds.AvailableFunds, availFunds.ReservedAmount);

			this.db.ForEachResult<TurnoverDbRow>(
				r => this.m_oTurnover.Add(r),
				"GetCustomerTurnoverForAutoDecision",
				new QueryParameter("IsForApprove", true),
				new QueryParameter("CustomerID", this.customerId),
				new QueryParameter("Now", Now)
			);

			this.m_oTrail.MyInputData.SetTurnoverData(this.m_oTurnover);

			this.m_oTrail.MyInputData.MetaData.Validate();
		} // SaveTrailInputData

		private T StepDone<T>() where T : ATrace {
			return this.m_oTrail.Affirmative<T>(false);
		} // StepDone

		private T StepFailed<T>() where T : ATrace {
			bool isStepEnabled = this.m_oTrail.MyInputData.Configuration.IsTraceEnabled<T>();

			if (!isStepEnabled) {
				this.m_oTrail.AddNote(
					"Step '" + typeof(T).FullName + "' has failed but is disabled hence marked as passed."
				);
			} // if

			return isStepEnabled ? StepForceFailed<T>() : StepDone<T>();
		} // StepFailed

		private T StepForceFailed<T>() where T : ATrace {
			this.m_oTrail.Amount = 0;
			return this.m_oTrail.Negative<T>(false);
		} // StepForceFailed

		private readonly CashRequestsRepository cashRequestsRepository;
		private readonly List<string> consumerCaisDetailWorstStatuses;
		private readonly Customer customer;
		private readonly int customerId;
		private readonly AConnection db;
		private readonly LoanRepository loanRepository;
		private readonly LoanScheduleTransactionRepository loanScheduleTransactionRepository;
		private readonly ASafeLog log;
		private readonly AutomationCalculator.AutoDecision.AutoApproval.Agent m_oSecondaryImplementation;
		private readonly ApprovalTrail m_oTrail;
		private readonly AutoApprovalTurnover m_oTurnover;
		private readonly Medal medalClassification;
		private readonly AutomationCalculator.Common.TurnoverType? turnoverType;
		private readonly AutomationCalculator.Common.MedalType medalType;
		private readonly CustomerAnalyticsRepository customerAnalytics;

		private DateTime? companyDissolutionDate;
		private List<Name> directors;
		private bool hasLoans;
		private List<String> hmrcNames;
		private bool isBrokerCustomer;
		private ExperianConsumerData m_oConsumerData;
		private int minCompanyScore;
		private int minExperianScore;
	} // class Approval
} // namespace