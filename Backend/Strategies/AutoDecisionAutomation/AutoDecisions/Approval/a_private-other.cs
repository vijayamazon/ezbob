namespace Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Approval {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using AutomationCalculator;
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

	public partial class Approval {
		private DateTime Now { get; set; }

		private void SaveTrailInputData(GetAvailableFunds availFunds) {
			this.trail.MyInputData.SetDataAsOf(Now);

			var cfg = new Configuration {
				AllowedCaisStatusesWithLoan = CurrentValues.Instance.AutoApproveAllowedCaisStatusesWithLoan,
				AllowedCaisStatusesWithoutLoan = CurrentValues.Instance.AutoApproveAllowedCaisStatusesWithoutLoan,
				AvailableFundsOverdraft = CurrentValues.Instance.AutoApproveAvailableFundsOverdraft,
				BusinessScoreThreshold = CurrentValues.Instance.AutoApproveBusinessScoreThreshold,
				CustomerMaxAge = CurrentValues.Instance.AutoApproveCustomerMaxAge,
				CustomerMinAge = CurrentValues.Instance.AutoApproveCustomerMinAge,
				ExperianScoreThreshold = CurrentValues.Instance.AutoApproveExperianScoreThreshold,
				HmrcTurnoverAge = CurrentValues.Instance.AutoApproveHmrcTurnoverAge,
				HmrcTurnoverDropHalfYearRatio = CurrentValues.Instance.AutoApproveHmrcTurnoverDropHalfYearRatio,
				HmrcTurnoverDropQuarterRatio = CurrentValues.Instance.AutoApproveHmrcTurnoverDropQuarterRatio,
				IsSilent = CurrentValues.Instance.AutoApproveIsSilent,
				MaxAllowedDaysLate = CurrentValues.Instance.AutoApproveMaxAllowedDaysLate,
				MaxAmount = CurrentValues.Instance.AutoApproveMaxAmount,
				MaxDailyApprovals = CurrentValues.Instance.AutoApproveMaxDailyApprovals,
				MaxHourlyApprovals = CurrentValues.Instance.AutoApproveMaxHourlyApprovals,
				MaxLastHourApprovals = CurrentValues.Instance.AutoApproveMaxLastHourApprovals,
				MaxNumOfOutstandingLoans = CurrentValues.Instance.AutoApproveMaxNumOfOutstandingLoans,
				MaxOutstandingOffers = CurrentValues.Instance.AutoApproveMaxOutstandingOffers,
				MaxTodayLoans = CurrentValues.Instance.AutoApproveMaxTodayLoans,
				MinLoan = CurrentValues.Instance.MinLoan,
				MinMPSeniorityDays = CurrentValues.Instance.AutoApproveMinMPSeniorityDays,
				MinRepaidPortion = CurrentValues.Instance.AutoApproveMinRepaidPortion,
				MinTurnover1M = CurrentValues.Instance.AutoApproveMinTurnover1M,
				MinTurnover1Y = CurrentValues.Instance.AutoApproveMinTurnover1Y,
				MinTurnover3M = CurrentValues.Instance.AutoApproveMinTurnover3M,
				OffHoursMaxDailyApprovals = CurrentValues.Instance.AutoApproveOffHoursMaxDailyApprovals,
				OffHoursMaxHourlyApprovals = CurrentValues.Instance.AutoApproveOffHoursMaxHourlyApprovals,
				OffHoursMaxLastHourApprovals = CurrentValues.Instance.AutoApproveOffHoursMaxLastHourApprovals,
				OffHoursMaxTodayLoans = CurrentValues.Instance.AutoApproveOffHoursMaxTodayLoans,
				OffHoursMaxOutstandingOffers = CurrentValues.Instance.AutoApproveOffHoursMaxOutstandingOffers,
				OfficeTimeEnd = CurrentValues.Instance.AutoApproveOfficeTimeEnd,
				OfficeTimeStart = CurrentValues.Instance.AutoApproveOfficeTimeStart,
				OnlineTurnoverAge = CurrentValues.Instance.AutoApproveOnlineTurnoverAge,
				OnlineTurnoverDropMonthRatio = CurrentValues.Instance.AutoApproveOnlineTurnoverDropMonthRatio,
				OnlineTurnoverDropQuarterRatio = CurrentValues.Instance.AutoApproveOnlineTurnoverDropQuarterRatio,
				Reject_Defaults_Amount = CurrentValues.Instance.Reject_Defaults_Amount,
				Reject_Defaults_MonthsNum = CurrentValues.Instance.Reject_Defaults_MonthsNum,
				SilentTemplateName = CurrentValues.Instance.AutoApproveSilentTemplateName,
				SilentToAddress = CurrentValues.Instance.AutoApproveSilentToAddress,
				TurnoverDropQuarterRatio = CurrentValues.Instance.AutoApproveTurnoverDropQuarterRatio,
				Weekend = CurrentValues.Instance.AutoApproveWeekend,
			};
			
			this.trail.MyInputData.SetConfiguration(cfg);

			this.db.ForEachRowSafe(
				srName => this.trail.MyInputData.Configuration.EnabledTraces.Add(srName["Name"]),
				"LoadEnabledTraces",
				CommandSpecies.StoredProcedure
			);

			this.trail.MyInputData.SetArgs(
				this.trail.CustomerID,
				this.trail.CashRequestID,
				this.trail.NLCashRequestID,
				this.trail.SafeAmount,
				(AutomationCalculator.Common.Medal)this.medalClassification,
				this.medalType,
				this.turnoverType
			);

			this.trail.MyInputData.SetMetaData(new MetaData {
				RowType = "MetaData",
				FirstName = (this.customer != null) && (this.customer.PersonalInfo != null)
					? this.customer.PersonalInfo.FirstName
					: null,
				LastName = (this.customer != null) && (this.customer.PersonalInfo != null)
					? this.customer.PersonalInfo.Surname
					: null,
				IsBrokerCustomer = this.isBrokerCustomer,
				NumOfTodayAutoApproval = CalculateTodaysApprovals(Now),
				NumOfYesterdayAutoApproval = CalculateTodaysApprovals(Now.AddDays(-1)),
				NumOfHourlyAutoApprovals = CalculateHourlyApprovals(),
				NumOfLastHourAutoApprovals = CalculateLastHourApprovals(),
				TodayLoanSum = CalculateTodaysLoans(Now),
				YesterdayLoanSum = CalculateTodaysLoans(Now.AddDays(-1)),
				FraudStatusValue = DetectFraudStatusValue(),
				AmlResult = (this.customer == null) ? "failed because customer not found" : this.customer.AMLResult,
				CustomerStatusName = this.customer == null ? "unknown" : this.customer.CollectionStatus.Name,
				CustomerStatusEnabled = this.customer != null && this.customer.CollectionStatus.IsEnabled,
				CompanyScore = this.minCompanyScore,
				ConsumerScore = this.minExperianScore,
				IncorporationDate = GetCustomerIncorporationDate(),
				DateOfBirth = (
					(this.customer != null) &&
					(this.customer.PersonalInfo != null) &&
					this.customer.PersonalInfo.DateOfBirth.HasValue
				) ? this.customer.PersonalInfo.DateOfBirth.Value : Now,
				NumOfDefaultAccounts = this.experianConsumerData.FindNumOfPersonalDefaults(
					cfg.Reject_Defaults_Amount,
					Now.AddMonths(-1 * cfg.Reject_Defaults_MonthsNum)
				),
				NumOfRollovers = CalculateRollovers(),
				TotalLoanCount = this.loanRepository.ByCustomer(this.trail.CustomerID).Count(),
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
						cr.AutoDecisionID == null &&
						cr.UnderwriterDecisionDate.Value < Now
					)
					: 0,
				CompanyDissolutionDate = this.companyDissolutionDate,
			});

			FindOutstandingLoans();

			this.trail.MyInputData.MetaData.EmailSendingBanned = this.db.ExecuteScalar<bool>(
				"GetLastOfferDataForApproval",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", this.trail.CustomerID)
			);

			this.trail.MyInputData.MetaData.OfferStart = Now;
			this.trail.MyInputData.MetaData.OfferValidUntil = Now.AddHours((int)CurrentValues.Instance.OfferValidForHours);

			this.trail.MyInputData.SetDirectorNames(this.directors);
			this.trail.MyInputData.SetHmrcBusinessNames(this.hmrcNames);

			FindLatePayments();
			this.trail.MyInputData.SetSeniority(CalculateSeniority());
			this.trail.MyInputData.SetAvailableFunds(availFunds.AvailableFunds, availFunds.ReservedAmount);

			this.db.ForEachResult<TurnoverDbRow>(
				r => this.turnover.Add(r),
				"GetCustomerTurnoverForAutoDecision",
				new QueryParameter("IsForApprove", true),
				new QueryParameter("CustomerID", this.trail.CustomerID),
				new QueryParameter("Now", Now)
			);

			this.trail.MyInputData.SetTurnoverData(this.turnover);

			this.trail.MyInputData.MetaData.Validate();
		} // SaveTrailInputData

		private T StepDone<T>() where T : ATrace {
			return this.trail.Affirmative<T>(false);
		} // StepDone

		private T StepFailed<T>() where T : ATrace {
			bool isStepEnabled = this.trail.MyInputData.Configuration.IsTraceEnabled<T>();

			if (!isStepEnabled) {
				this.trail.AddNote(
					"Step '" + typeof(T).FullName + "' has failed but is disabled hence marked as passed."
				);
			} // if

			return isStepEnabled ? StepForceFailed<T>() : StepDone<T>();
		} // StepFailed

		private T StepForceFailed<T>() where T : ATrace {
			this.trail.Amount = 0;
			return this.trail.Negative<T>(false);
		} // StepForceFailed

		private readonly CashRequestsRepository cashRequestsRepository;
		private readonly Customer customer;
		private readonly AConnection db;
		private readonly LoanRepository loanRepository;
		private readonly LoanScheduleTransactionRepository loanScheduleTransactionRepository;
		private readonly ASafeLog log;
		private readonly AutomationCalculator.AutoDecision.AutoApproval.Agent m_oSecondaryImplementation;
		private readonly ApprovalTrail trail;
		private readonly AutoApprovalTurnover turnover;
		private readonly Medal medalClassification;
		private readonly AutomationCalculator.Common.TurnoverType? turnoverType;
		private readonly AutomationCalculator.Common.MedalType medalType;

		private DateTime? incorporationDate;

		private DateTime? companyDissolutionDate;
		private List<Name> directors;
		private List<NameForComparison> hmrcNames;
		private bool isBrokerCustomer;
		private ExperianConsumerData experianConsumerData;
		private int minCompanyScore;
		private int minExperianScore;
		private OfficeHoursHandler officeHoursHandler;
	} // class Approval
} // namespace
