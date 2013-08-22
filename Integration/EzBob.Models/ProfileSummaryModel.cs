using System;
using System.Collections.Generic;

namespace EzBob.Web.Areas.Underwriter.Models
{
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
        public string AverageLateDays { get; set; }
        public string TotalFees { get; set; }
        public string FeesCount { get; set; }
    }

    public class AffordabilityAnalysis
    {
        public string EzBobMonthlyRepayment { get; set; }
        public string CashAvailabilityOrDeficits { get; set; }
    }

    public class CreditBureau : BaseProfileSummaryModel
    {
        public double CreditBureauScore { get; set; }
        public double TotalMonthlyRepayments { get; set; }
        public double TotalDebt { get; set; }
        public double CreditCardBalances { get; set; }
        public string BorrowerType { get; set; }
        public int FinancialAccounts { get; set; }
        public string ThinFile { get; set; }
    }

    public class ProfileSummaryModel
    {
        public MarketPlaces MarketPlaces { get; set; }
        public PaymentAccounts PaymentAccounts { get; set; }
        public AmlBwa AmlBwa { get; set; }
        public LoanActivity LoanActivity { get; set; }
        public AffordabilityAnalysis AffordabilityAnalysis { get; set; }
        public CreditBureau CreditBureau { get; set; }
        public FraudCheck FraudCheck { get; set; }

        public string Comment { get; set; }

        public int Id { get; set; }

        public List<DecisionHistoryModel> DecisionHistory { get; set; }

        public decimal? OverallTurnOver { get; set; }
        public decimal? WebSiteTurnOver { get; set; }
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
                    Icon = "icon-ok-sign";
                    ButtonStyle = "btn-success";
                    Caption = "Passed";
                    break;
                case LightsState.Warning:
                    Icon = "icon-question-sign";
                    ButtonStyle = "btn-warning";
                    Caption = "Warning";
                    break;
                case LightsState.Reject:
                    Icon = "icon-remove-sign";
                    ButtonStyle = "btn-danger";
                    Caption = "Reject";
                    break;
                case LightsState.InProgress:
                    Icon = "icon-remove-sign";
                    ButtonStyle = "btn-danger btn-more-danger";
                    Caption = "In progress";
                    break;
                case LightsState.Error:
                    Icon = "icon-remove-sign";
                    ButtonStyle = "btn-danger btn-more-danger";
                    Caption = "Error";
                    break;
                case LightsState.NotPerformed:
                    Icon = "icon-ban-circle";
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