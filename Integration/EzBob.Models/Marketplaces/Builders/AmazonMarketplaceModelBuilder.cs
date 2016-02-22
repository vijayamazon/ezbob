namespace EzBob.Models.Marketplaces.Builders {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Repository;
	using AmazonServiceLib;
	using Ezbob.Utils;
	using EzBob.CommonLib.TimePeriodLogic;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Marketplaces;
	using NHibernate;
	using NHibernate.Linq;
	using StructureMap;
	using EZBob.DatabaseLib.Model.Marketplaces.Amazon;

	class AmazonMarketplaceModelBuilder : MarketplaceModelBuilder {
		public AmazonMarketplaceModelBuilder(ISession session) : base(session) { }

		public override string GetUrl(MP_CustomerMarketPlace mp, IMarketPlaceSecurityInfo securityInfo) {
			return string.Format(
				"http://www.amazon.co.uk/gp/aag/main?ie=UTF8&seller={0}",
				((AmazonSecurityInfo)securityInfo).MerchantId
			);
		} // GetUrl

		public override DateTime? GetSeniority(MP_CustomerMarketPlace mp) {
			var s = _session
				.Query<MP_AmazonOrderItem>()
				.Where(oi => oi.Order.CustomerMarketPlace.Id == mp.Id)
				.Where(oi => oi.PurchaseDate != null)
				.Select(oi => oi.PurchaseDate);
			return !s.Any() ? null : s.Min();
		} // GetSeniority

		public override DateTime? GetLastTransaction(MP_CustomerMarketPlace mp) {
			var s = _session
				.Query<MP_AmazonOrderItem>()
				.Where(oi => oi.Order.CustomerMarketPlace.Id == mp.Id)
				.Where(oi => oi.PurchaseDate != null);

			if (s.Count() != 0)
				return s.Max(oi => oi.PurchaseDate);

			return null;
		} // GetLastTransaction

		protected override void InitializeSpecificData(
			MP_CustomerMarketPlace mp,
			MarketPlaceModel model,
			DateTime? history
		) {
			TimeCounter tc = new TimeCounter("MarketplaceModelBuilder Amazon InitializeSpecificData building time for mp " + mp.Id);
			using (tc.AddStep("AskvilleStatus Time taken")) {
				var askville = ObjectFactory.GetInstance<AskvilleRepository>();
				var askvilleTmp = askville.GetAskvilleByMarketplace(mp);

				model.AskvilleStatus = askvilleTmp != null
					? askvilleTmp.Status.ToString()
					: AskvilleStatus.NotPerformed.ToString();

				model.AskvilleGuid = askvilleTmp != null ? askvilleTmp.Guid : "";
			}
			using (tc.AddStep("GetAmazonCategories Time taken")) {
				model.Categories = GetAmazonCategories(mp);
			}

			Log.Info(tc.ToString());
		} // InitializeSpecificData

		protected override MarketPlaceFeedbackModel GetFeedbackData(List<IAnalysisDataParameterInfo> aggregations) {
			var sellerRating = aggregations.FirstOrDefault(
				x => x.ParameterName == AggregationFunction.UserRating.ToString()
			);

			var model = new MarketPlaceFeedbackModel();
			model.AmazonSelerRating = sellerRating == null ? 0 : (double)sellerRating.Value;

			var raitingPercent = aggregations.FirstOrDefault(x =>
				x.ParameterName == AggregationFunction.PositiveFeedbackPercentRate.ToString() &&
				x.TimePeriod.TimePeriodType == TimePeriodEnum.Year
			);

			decimal? raitingPercentValue = raitingPercent == null ? 0 : (decimal?)raitingPercent.Value;

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
		} // GetFeedbackDAta

		private List<string> GetAmazonCategories(MP_CustomerMarketPlace marketplace) {
			return _session.Query<MP_AmazonOrderItemDetailCatgory>()
				.Where(x => x.OrderItemDetail.OrderItem.Order.CustomerMarketPlace.Id == marketplace.Id)
				.Select(x => x.Category.Name)
				.Distinct()
				.ToList();
		} // GetAmazonCategories
	} // class AmazonMarketplaceModelBuilder
} // namespace
