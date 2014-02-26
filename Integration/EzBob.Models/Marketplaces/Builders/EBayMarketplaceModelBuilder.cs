using System;
using System.Globalization;
using System.Linq;
using EZBob.DatabaseLib.Model.Database;
using EzBob.Web.Areas.Underwriter.Models;
using EzBob.Web.Code;
using NHibernate;
using NHibernate.Linq;

namespace EzBob.Models.Marketplaces.Builders
{
	using EZBob.DatabaseLib.Repository;

	class EBayMarketplaceModelBuilder : MarketplaceModelBuilder
    {
        private readonly EbayAmazonCategoryRepository _ebayAmazonCategoryRepository;

        public EBayMarketplaceModelBuilder(EbayAmazonCategoryRepository ebayAmazonCategoryRepository, ISession session) : base(session)
        {
            _ebayAmazonCategoryRepository = ebayAmazonCategoryRepository;
        }

        protected override void InitializeSpecificData(MP_CustomerMarketPlace mp, MarketPlaceModel model, DateTime? history)
        {
            var ebayFeedBack = mp.EbayFeedback.LastOrDefault();

            var feedbackByPeriodEbay = ebayFeedBack != null
                                           ? ebayFeedBack.FeedbackByPeriodItems.FirstOrDefault(
                                               x => x.TimePeriod.Id == 4)
                                           : null;

            var negative = feedbackByPeriodEbay != null ? feedbackByPeriodEbay.Negative : null;
            var positive = feedbackByPeriodEbay != null ? feedbackByPeriodEbay.Positive : null;
            var neutral = feedbackByPeriodEbay != null ? feedbackByPeriodEbay.Neutral : null;

            var raiting = (positive + negative + neutral) != 0
                              ? (positive * 100) / (positive + negative + neutral)
                              : 0;

            model.PositiveFeedbacks = positive ?? 0;
            model.NegativeFeedbacks = negative ?? 0;
            model.NeutralFeedbacks = neutral ?? 0;

            model.RaitingPercent = raiting != null ? raiting.ToString() : "-";

            model.Categories = _ebayAmazonCategoryRepository.GetEbayCategories(mp);

            model.EBay = BuildEBay(mp.EbayUserData.LastOrDefault(), mp.EbayUserAccountData.LastOrDefault(), ebayFeedBack);
        }

        private static EBayModel BuildEBay(MP_EbayUserData ebayUserData, MP_EbayUserAccountData ebayAccount, MP_EbayFeedback ebayFeedBack)
        {
            var ebayFeedBackScore = ebayUserData != null ? ebayUserData.FeedbackScore : 0;
            var ebayAdditionalAccount = ebayAccount != null
                                            ? ebayAccount.EbayUserAdditionalAccountData.LastOrDefault()
                                            : null;

            var model = new EBayModel();

            model.EBayFeedBackScore = ebayFeedBackScore.ToString();

            BuildEbayUserData(ebayUserData, model);
            BuildEbayAccountData(ebayAccount, model);
            BuildEbayAdditionalAccountData(model, ebayAdditionalAccount);
            BuildEbayFeedbackData(ebayFeedBack, model);

            return model;
        }

        private static void BuildEbayFeedbackData(MP_EbayFeedback ebayFeedBack, EBayModel model)
        {
            if (ebayFeedBack == null) return;

            model.NegativeFeedbackCount = Convert.ToString(ebayFeedBack.FeedbackByPeriodItems.Sum(x => x.Negative));
            model.NeutralFeedbackCount = Convert.ToString(ebayFeedBack.FeedbackByPeriodItems.Sum(x => x.Neutral));
            model.PositiveFeedbackCount = Convert.ToString(ebayFeedBack.FeedbackByPeriodItems.Sum(x => x.Positive));
            model.UniqueNegativeFeedbackCount = Convert.ToString(ebayFeedBack.UniqueNegativeCount);
            model.UniqueNeutralFeedbackCount = Convert.ToString(ebayFeedBack.UniqueNeutralCount);
            model.UniqueBuyerCount = Convert.ToString(ebayFeedBack.UniqueBuyerCount);
            model.UniquePositiveFeedbackCount = Convert.ToString(ebayFeedBack.UniquePositiveCount);
            model.TransactionPercent = Convert.ToString(ebayFeedBack.TransactionPercent);
            model.RepeatBuyerCount = Convert.ToString(ebayFeedBack.RepeatBuyerCount);
            model.RepeatBuyerPercent = Convert.ToString(ebayFeedBack.RepeatBuyerPercent);
        }

        private static void BuildEbayAdditionalAccountData(EBayModel model, MP_EbayUserAdditionalAccountData ebayAdditionalAccount)
        {
            if (ebayAdditionalAccount == null) return;

            model.AdditionalAccountAccountCode = ebayAdditionalAccount.AccountCode;
            model.AdditionalAccountBalance = ebayAdditionalAccount.Balance.ToString();
            model.AdditionalAccountCurrency = ebayAdditionalAccount.Currency;
        }

        private static void BuildEbayAccountData(MP_EbayUserAccountData ebayAccount, EBayModel model)
        {
            if (ebayAccount == null) return;

            model.AccountId = ebayAccount.AccountId;
            model.AccountState = ebayAccount.AccountState;
            model.AmountPastDueAmount = ebayAccount.AmountPastDueValue != null
                                            ? ebayAccount.AmountPastDueValue.Value.ToString(CultureInfo.InvariantCulture)
                                            : "-";
            model.BankAccountInfo = ebayAccount.BankAccountInfo ?? "-";
            model.BankModifyDate = FormattingUtils.FormatDateTimeToString(ebayAccount.BankModifyDate, "-");
            model.CreditCardExpiration = FormattingUtils.FormatDateTimeToString(ebayAccount.CreditCardExpiration, "-");
            model.CreditCardInfo = ebayAccount.CreditCardInfo;
            model.CreditCardModifyDate = FormattingUtils.FormatDateTimeToString(ebayAccount.CreditCardModifyDate, "-");
            model.CurrentBalance = ebayAccount.CurrentBalance.ToString();
            model.PastDue = ((bool?)ebayAccount.PastDue).ToString();
            model.PaymentMethod = ebayAccount.PaymentMethod;
            model.Currency = ebayAccount.Currency;
        }

        private static void BuildEbayUserData(MP_EbayUserData ebayUserData, EBayModel model)
        {
            if (ebayUserData == null) return;

            model.EBayGoodStanding = ebayUserData.eBayGoodStanding.ToString();
            model.EIASToken = ebayUserData.EIASToken;
            model.FeedbackPrivate = ebayUserData.FeedbackPrivate.ToString();
            model.IDChanged = ebayUserData.IDChanged.ToString();
            model.IDLastChanged = FormattingUtils.FormatDateTimeToString(ebayUserData.IDLastChanged, "-");
            model.IdVerified = ebayUserData.IDVerified.ToString();
            model.NewUser = ebayUserData.NewUser.ToString();
            model.PayPalAccountStatus = ebayUserData.PayPalAccountStatus;
            model.PayPalAccountType = ebayUserData.PayPalAccountType;
            model.QualifiesForSelling = ebayUserData.QualifiesForSelling.ToString();

            BuildSellerInfo(ebayUserData, model);
        }

        private static void BuildSellerInfo(MP_EbayUserData ebayUserData, EBayModel model)
        {
            if (ebayUserData == null || ebayUserData.SellerInfo == null) return;

            model.SellerInfoStoreOwner = ebayUserData.SellerInfo.SellerInfoStoreOwner.ToString();
            model.SellerInfoStoreSite = ebayUserData.SellerInfo.SellerInfoStoreSite;
            model.SellerInfoTopRatedProgram = ebayUserData.SellerInfo.SellerInfoTopRatedProgram ?? "-";
            model.SellerInfoTopRatedSeller = ebayUserData.SellerInfo.SellerInfoTopRatedSeller.ToString();
        }

        public override DateTime? GetSeniority(MP_CustomerMarketPlace mp)
        {
	        var mpEbayUserData = mp.EbayUserData.FirstOrDefault(e => e.CustomerMarketPlace == mp);
	        return mpEbayUserData != null ? mpEbayUserData.RegistrationDate : null;
        }

		public override DateTime? GetLastTransaction(MP_CustomerMarketPlace mp)
		{
			var s = _session.Query<MP_TeraPeakOrderItem>().Where(tp => tp.Order.CustomerMarketPlace.Id == mp.Id);

			if (s.Count() != 0)
			{
				return s.Max(tp => tp.StartDate);
			}

			return null;
		}
    }
}