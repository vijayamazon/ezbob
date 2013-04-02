using System.Collections.Generic;
using EZBob.DatabaseLib.Model.Database;

namespace EzBob.Web.Areas.Underwriter.Models
{
    public class MarketPlaceModel
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string LastChecked { get; set; }
        public string EluminatingStatus { get; set; }
        public string UpdatingStatus { get; set; }
        public string UpdateError { get; set; }
        public string RaitingPercent { get; set; }
        public string SellerInfoStoreURL { get; set; }
        public IEnumerable<string> Categories { get; set; }

        //Amazon
        public double AmazonSelerRating { get; set; }
        public string AccountAge { get; set; }
        public string AskvilleStatus { get; set; }
        public string AskvilleGuid { get; set; }

        public EBayModel EBay { get; set; }
        public Dictionary<string, string> AnalysisDataInfo { get; set; }

        public int PositiveFeedbacks { get; set; }
        public int NegativeFeedbacks { get; set; }
        public int NeutralFeedbacks { get; set; }
    }

    public class EBayModel
    {
        public EBayModel()
        {
            EBayFeedBackScore = "-";
            EBayGoodStanding = "-";
            FeedbackPrivate = "-";
            IdVerified = "-";
            NewUser = "-";
            PayPalAccountStatus = "-";
            PayPalAccountType = "-";
            QualifiesForSelling = "-";
            SellerInfoStoreOwner = "-";
            SellerInfoStoreSite = "-";
            SellerInfoTopRatedSeller = "-";
            SellerInfoTopRatedProgram = "-";
            IDChanged = "-";
            IDLastChanged = "-";
            AccountId = "-";
            AccountState = "-";
            AmountPastDueAmount = "-";
            BankAccountInfo = "-";
            BankModifyDate = "-";
            CreditCardExpiration = "-";
            CreditCardModifyDate = "-";
            CurrentBalance = "-";
            PastDue = "-";
            PaymentMethod = "-";
            Currency = "-";
            AdditionalAccountAccountCode = "-";
            AdditionalAccountBalance = "-";
            AdditionalAccountCurrency = "-";
            NegativeFeedbackCount = "-";
            PositiveFeedbackCount = "-";
            NeutralFeedbackCount = "-";
            UniqueNegativeFeedbackCount = "-";
            UniquePositiveFeedbackCount = "-";
            UniqueNeutralFeedbackCount = "-";
            RepeatBuyerCount = "-";
            RepeatBuyerPercent = "-";
            TransactionPercent = "-";
            UniqueBuyerCount = "-";
        }

        public string EBayFeedBackScore { get; set; }
        public string EBayGoodStanding { get; set; }
        public string EIASToken { get; set; }
        public string FeedbackPrivate { get; set; }
        public string IdVerified { get; set; }
        public string NewUser { get; set; }
        public string PayPalAccountStatus { get; set; }
        public string PayPalAccountType { get; set; }
        public string QualifiesForSelling { get; set; }
        public string SellerInfoStoreOwner { get; set; }
        public string SellerInfoStoreSite { get; set; }
        public string SellerInfoTopRatedSeller { get; set; }
        public string SellerInfoTopRatedProgram { get; set; }
        public string IDChanged { get; set; }
        public string IDLastChanged { get; set; }

        public string AccountId { get; set; }
        public string AccountState { get; set; }
        public string AmountPastDueAmount { get; set; }
        public string BankAccountInfo { get; set; }
        public string BankModifyDate { get; set; }
        public string CreditCardExpiration { get; set; }
        public string CreditCardInfo { get; set; }
        public string CreditCardModifyDate { get; set; }
        public string CurrentBalance { get; set; }
        public string PastDue { get; set; }
        public string PaymentMethod { get; set; }
        public string Currency { get; set; }
        public string AdditionalAccountAccountCode { get; set; }
        public string AdditionalAccountBalance { get; set; }
        public string AdditionalAccountCurrency { get; set; }

        public string NegativeFeedbackCount { get; set; }
        public string PositiveFeedbackCount { get; set; }
        public string NeutralFeedbackCount { get; set; }
        public string UniqueNegativeFeedbackCount { get; set; }
        public string UniquePositiveFeedbackCount { get; set; }
        public string UniqueNeutralFeedbackCount { get; set; }
        public string RepeatBuyerCount { get; set; }
        public string RepeatBuyerPercent { get; set; }
        public string TransactionPercent { get; set; }
        public string UniqueBuyerCount { get; set; }
    }
}