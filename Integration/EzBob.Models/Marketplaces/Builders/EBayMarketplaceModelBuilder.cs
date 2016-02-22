namespace EzBob.Models.Marketplaces.Builders {
	using System;
	using System.Globalization;
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database;
	using EzBob.Web.Areas.Underwriter.Models;
	using NHibernate;
	using NHibernate.Linq;
	using System.Collections.Generic;
	using Ezbob.Backend.Models;
	using EzBob.CommonLib.TimePeriodLogic;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Marketplaces;

	class EBayMarketplaceModelBuilder : MarketplaceModelBuilder {
		public EBayMarketplaceModelBuilder(ISession session) : base(session) { }

		protected override void InitializeSpecificData(
			MP_CustomerMarketPlace mp,
			MarketPlaceModel model,
			DateTime? history
		) {
			var ebayFeedBack = mp.EbayFeedback.LastOrDefault();

			model.Categories = GetEbayCategories(mp);

			model.EBay = BuildEBay(mp.EbayUserData.LastOrDefault(), mp.EbayUserAccountData.LastOrDefault(), ebayFeedBack);
		} // InitializeSpecificData

		protected override MarketPlaceFeedbackModel GetFeedbackData(List<IAnalysisDataParameterInfo> aggregations) {
			var raitingPercent = aggregations.FirstOrDefault(
				x => x.ParameterName == AggregationFunction.PositiveFeedbackPercentRate.ToString()
			);

			decimal? raitingPercentValue = raitingPercent == null ? 0 : (decimal?)raitingPercent.Value;
			var model = new MarketPlaceFeedbackModel();
			model.RaitingPercent = raitingPercentValue ?? 0;

			var yearAggregations = aggregations.Where(x => x.TimePeriod.TimePeriodType == TimePeriodEnum.Year).ToList();

			if (!yearAggregations.Any())
				return model;

			var positive = yearAggregations.FirstOrDefault(
				x => x.ParameterName == AggregationFunction.PositiveFeedbackRate.ToString()
			);
			var negative = yearAggregations.FirstOrDefault(
				x => x.ParameterName == AggregationFunction.NegativeFeedbackRate.ToString()
			);
			var neutral = yearAggregations.FirstOrDefault(
				x => x.ParameterName == AggregationFunction.NeutralFeedbackRate.ToString()
			);

			model.PositiveFeedbacks = positive == null ? null : (int?)positive.Value;
			model.NegativeFeedbacks = negative == null ? null : (int?)negative.Value;
			model.NeutralFeedbacks = neutral == null ? null : (int?)neutral.Value;

			return model;
		} // GetFeedbackData

		private static EBayModel BuildEBay(
			MP_EbayUserData ebayUserData,
			MP_EbayUserAccountData ebayAccount,
			MP_EbayFeedback ebayFeedBack
		) {
			var ebayFeedBackScore = ebayUserData != null ? ebayUserData.FeedbackScore : 0;

			var ebayAdditionalAccount = ebayAccount != null
				? ebayAccount.EbayUserAdditionalAccountData.LastOrDefault()
				: null;

			var model = new EBayModel {
				EBayFeedBackScore = ebayFeedBackScore.ToString(),
			};

			BuildEbayUserData(ebayUserData, model);
			BuildEbayAccountData(ebayAccount, model);
			BuildEbayAdditionalAccountData(model, ebayAdditionalAccount);
			BuildEbayFeedbackData(ebayFeedBack, model);

			return model;
		} // BuildEBay

		private static void BuildEbayFeedbackData(MP_EbayFeedback ebayFeedBack, EBayModel model) {
			if (ebayFeedBack == null)
				return;

			model.NegativeFeedbackCount = Convert.ToString(ebayFeedBack.FeedbackByPeriodItems.Sum(x => x.Negative));
			model.NeutralFeedbackCount = Convert.ToString(ebayFeedBack.FeedbackByPeriodItems.Sum(x => x.Neutral));
			model.PositiveFeedbackCount = Convert.ToString(ebayFeedBack.FeedbackByPeriodItems.Sum(x => x.Positive));
			model.UniqueNegativeFeedbackCount = Convert.ToString(ebayFeedBack.UniqueNegativeCount);
			model.UniqueNeutralFeedbackCount = Convert.ToString(ebayFeedBack.UniqueNeutralCount);
			model.UniqueBuyerCount = Convert.ToString(ebayFeedBack.UniqueBuyerCount);
			model.UniquePositiveFeedbackCount = Convert.ToString(ebayFeedBack.UniquePositiveCount);
			model.TransactionPercent = Convert.ToString(ebayFeedBack.TransactionPercent, CultureInfo.InvariantCulture);
			model.RepeatBuyerCount = Convert.ToString(ebayFeedBack.RepeatBuyerCount);
			model.RepeatBuyerPercent = Convert.ToString(ebayFeedBack.RepeatBuyerPercent, CultureInfo.InvariantCulture);
		} // BuildEbayFeedbackData

		private static void BuildEbayAdditionalAccountData(
			EBayModel model,
			MP_EbayUserAdditionalAccountData ebayAdditionalAccount
		) {
			if (ebayAdditionalAccount == null)
				return;

			model.AdditionalAccountAccountCode = ebayAdditionalAccount.AccountCode;
			model.AdditionalAccountBalance = ebayAdditionalAccount.Balance.ToString();
			model.AdditionalAccountCurrency = ebayAdditionalAccount.Currency;
		} // BuildEbayAdditionalAccountData

		private static void BuildEbayAccountData(MP_EbayUserAccountData ebayAccount, EBayModel model) {
			if (ebayAccount == null)
				return;

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
		} // BuildEbayAccountData

		private static void BuildEbayUserData(MP_EbayUserData ebayUserData, EBayModel model) {
			if (ebayUserData == null)
				return;

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
		} // BuildEbayUserData

		private static void BuildSellerInfo(MP_EbayUserData ebayUserData, EBayModel model) {
			if (ebayUserData == null || ebayUserData.SellerInfo == null)
				return;

			model.SellerInfoStoreOwner = ebayUserData.SellerInfo.SellerInfoStoreOwner.ToString();
			model.SellerInfoStoreSite = ebayUserData.SellerInfo.SellerInfoStoreSite;
			model.SellerInfoTopRatedProgram = ebayUserData.SellerInfo.SellerInfoTopRatedProgram ?? "-";
			model.SellerInfoTopRatedSeller = ebayUserData.SellerInfo.SellerInfoTopRatedSeller.ToString();
		} // BuildSellerInfo

		public override DateTime? GetSeniority(MP_CustomerMarketPlace mp) {
			var mpEbayUserData = mp.EbayUserData.FirstOrDefault(e => e.CustomerMarketPlaceId == mp.Id);
			return mpEbayUserData != null ? mpEbayUserData.RegistrationDate : null;
		} // GetSeniority

		public override DateTime? GetLastTransaction(MP_CustomerMarketPlace mp) {
			var lst = new List<DateTime>();

			var tp = this._session.Query<MP_TeraPeakOrderItem>().Where(oi => oi.Order.CustomerMarketPlace.Id == mp.Id);

			if (tp.Count() != 0)
				lst.Add(tp.Max(oi => oi.StartDate));

			var native = this._session.Query<MP_EbayOrderItem>().Where(
				oi => (oi.CreatedTime != null) && (oi.Order.CustomerMarketPlace.Id == mp.Id)
			);

			if (native.Count() != 0)
				lst.Add(native.Max(oi => oi.CreatedTime.Value));

			return lst.Count > 0 ? lst.Max() : (DateTime?)null;
		} // GetLastTransaction

		private List<string> GetEbayCategories(MP_CustomerMarketPlace marketplace) {
			IEnumerable<string> terapeak;
			IEnumerable<string> native;

			if (marketplace.TeraPeakOrders != null) {
				List<string> cats = this._session.Query <MP_TeraPeakCategoryStatistics>()
					.Where(x => x.OrderItem.Order.CustomerMarketPlace.Id == marketplace.Id)
					.Select(c => c.Category.FullName)
					.ToList();

				terapeak = cats.Count > 0 ? cats.Distinct().ToList() : cats;
			} else
				terapeak = new List<string>();

			if (marketplace.EbayOrders != null) {
				native = this._session.Query<MP_EbayTransaction>()
					.Where(x => x.OrderItem.Order.CustomerMarketPlace.Id == marketplace.Id)
					.Where(x => x.OrderItemDetail.PrimaryCategory != null)
					.Select(x => x.OrderItemDetail.PrimaryCategory.Name)
					.Distinct()
					.ToList();
			} else
				native = new List<string>();
			return terapeak.Union(native).Distinct().ToList();
		} // GetEbayCategories
	} // class EBayMarketplaceModelBuilder
} // namespace