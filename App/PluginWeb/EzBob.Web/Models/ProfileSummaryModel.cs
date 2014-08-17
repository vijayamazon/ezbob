namespace EzBob.Web.Models
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Areas.Underwriter.Models;
	using EZBob.DatabaseLib.Model.Database;
	using System.ComponentModel;
	using EzBob.Models;

	public class BaseProfileSummaryModel
	{
		public Lighter Lighter { get; set; }
	}

	public class MarketPlaces : BaseProfileSummaryModel
	{
		public string NumberOfStores { get; set; }
		public double? AnualTurnOver { get; set; }
		public string Inventory { get; set; }
		public string TotalPositiveReviews { get; set; }
		public string Seniority { get; set; }
		public bool IsNew { get; set; }
	}

	public class PaymentAccounts : BaseProfileSummaryModel
	{
		public string NumberOfPayPalAccounts { get; set; }
		public double NetIncome { get; set; }
		public string NetExpences { get; set; }
		public string Balance { get; set; }
		public string Seniority { get; set; }
		public bool IsNew { get; set; }
	}

	public class AmlBwa : BaseProfileSummaryModel
	{
		public string Bwa { get; set; }
		public string Aml { get; set; }
	}

	public class LoanActivity : BaseProfileSummaryModel
	{
		public string Collection { get; set; }
		public string CurrentBalance { get; set; }
		public string LatePaymentsSum { get; set; }
		public string PreviousLoans { get; set; }
		public string LateInterest { get; set; }
		public string PaymentDemeanor { get; set; }
		public string CurrentLateDays { get; set; }
		public string TotalFees { get; set; }
		public string FeesCount { get; set; }

		//Dashboard
		public decimal TotalIssuesSum { get; set; }
		public int TotalIssuesCount { get; set; }
		public decimal RepaidSum { get; set; }
		public int RepaidCount { get; set; }
		public decimal ActiveSum { get; set; }
		public int ActiveCount { get; set; }
		public decimal EarnedInterest { get; set; }

		public IOrderedEnumerable<ActiveLoan> ActiveLoans { get; set; }
	}

	public class ActiveLoan
	{
		public DateTime LoanDate { get; set; }
		public int LoanNumber { get; set; }
		public int Term { get; set; }
		public int TermApproved { get; set; }
		public double? Approved { get; set; }
		public decimal Balance { get; set; }
		public decimal BalancePercent { get; set; }
		public decimal BalanceWidthPercent { get; set; }
		public decimal LoanAmount { get; set; }
		public decimal LoanAmountPercent { get; set; }
		public decimal LoanAmountWidthPercent { get; set; }
		public double WidthPercent { get; set; }
		public bool IsEU { get; set; }
		public decimal InterestRate { get; set; }
		public decimal TotalFee { get; set; }
		public bool IsLate { get; set; }
		public string Comment { get; set; }
	}

	public class AffordabilityAnalysis
	{
		public string EzBobMonthlyRepayment { get; set; }
		public string CashAvailabilityOrDeficits { get; set; }
	}

	public class CreditBureau : BaseProfileSummaryModel
	{
		public int? CreditBureauScore { get; set; }
		public int? TotalMonthlyRepayments { get; set; }
		public int? TotalDebt { get; set; }
		public int? CreditCardBalances { get; set; }
		public string BorrowerType { get; set; }
		public int? FinancialAccounts { get; set; }
		public string ThinFile { get; set; }
		public List<DateTime?> ApplicantDOBs { get; set; }
	}

	public class CustomerRequestedLoanModel
	{
		public DateTime Created { get; set; }
		public string CustomerReason { get; set; }
		public string CustomerSourceOfRepayment { get; set; }
		public double? Amount { get; set; }
		public string OtherReason { get; set; }
		public string OtherSourceOfRepayment { get; set; }
	}

	public class DecisionsModel
	{
		public int TotalDecisionsCount { get; set; }
		public decimal TotalApprovedAmount { get; set; }
		public int RejectsCount { get; set; }
		public decimal LastInterestRate { get; set; }
		public DateTime? LastDecisionDate { get; set; }
	}

	public class ProfileSummaryModel
	{
		public MarketPlaces MarketPlaces { get; set; }
		public PaymentAccounts PaymentAccounts { get; set; }
		public AmlBwa AmlBwa { get; set; }
		public LoanActivity LoanActivity { get; set; }
		public DecisionsModel Decisions { get; set; }
		public AffordabilityAnalysis AffordabilityAnalysis { get; set; }
		public CreditBureau CreditBureau { get; set; }
		public FraudCheck FraudCheck { get; set; }

		public string Comment { get; set; }

		public int Id { get; set; }

		public List<DecisionHistoryModel> DecisionHistory { get; set; }

		public decimal? OverallTurnOver { get; set; }
		public decimal? WebSiteTurnOver { get; set; }

		public bool? IsOffline { get; set; }
		public CustomerRequestedLoanModel RequestedLoan { get; set; }
		public CompanyEmployeeCountInfo CompanyEmployeeCountInfo { get; set; }
		public CompanyInfoMap CompanyInfo { get; set; }
		public AlertsModel Alerts { get; set; }
	}

	public class AlertsModel
	{
		public List<AlertModel> Errors { get; set; }
		public List<AlertModel> Warnings { get; set; }
		public List<AlertModel> Infos { get; set; } 
	}

	public class AlertModel
	{
		public string AlertType { get; set; }
		public string Alert { get; set; }
		public string Tooltip { get; set; }
		public string Abbreviation { get; set; }
	}

	public enum AlertType
	{
		[Description("danger")]
		Error,
		[Description("warning")]
		Warning,
		[Description("info")]
		Info,
		[Description("success")]
		Success
	}

	public class FraudCheck
	{
		public string Status { get; set; }
		public string NumOfInternalDetection { get; set; }
		public string NumOfExternalDetection { get; set; }
		public string StatusComment { get; set; }
	}

	public enum LightsState
	{
		Passed,
		Warning,
		Reject,
		Error,
		InProgress,
		NotPerformed
	}

	public class Lighter
	{
		public Lighter(LightsState state)
		{
			switch (state)
			{
				case LightsState.Passed:
					Icon = "icon-white icon-ok-sign";
					ButtonStyle = "btn-success";
					Caption = "Passed";
					break;
				case LightsState.Warning:
					Icon = "icon-white icon-question-sign";
					ButtonStyle = "btn-warning";
					Caption = "Warning";
					break;
				case LightsState.Reject:
					Icon = "icon-white icon-remove-sign";
					ButtonStyle = "btn-danger";
					Caption = "Reject";
					break;
				case LightsState.InProgress:
					Icon = "icon-white icon-remove-sign";
					ButtonStyle = "btn-danger btn-more-danger";
					Caption = "In progress";
					break;
				case LightsState.Error:
					Icon = "icon-white icon-remove-sign";
					ButtonStyle = "btn-danger btn-more-danger";
					Caption = "Error";
					break;
				case LightsState.NotPerformed:
					Icon = "icon-white icon-ban-circle";
					ButtonStyle = "btn";
					Caption = "Not Performed ";
					break;
				default:
					throw new ArgumentOutOfRangeException("state");
			}
		}

		public string Icon { get; set; }
		public string Caption { get; set; }
		public string ButtonStyle { get; set; }
	}
}