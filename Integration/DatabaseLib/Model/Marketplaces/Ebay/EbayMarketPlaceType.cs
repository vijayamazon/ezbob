namespace EZBob.DatabaseLib.Model.Marketplaces.Ebay {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Database;
	using EzBob.CommonLib.TimePeriodLogic;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Repository.Turnover;
	using FluentNHibernate.Mapping;
	using NHibernate.Linq;

	public class eBayMarketPlaceType : MP_MarketplaceType {
		public override int UWPriority { get { return 1; } }

		public override IEnumerable<IAnalysisDataParameterInfo> GetAggregations(
			MP_CustomerMarketPlace mp,
			DateTime? history
		) {
			DateTime relevantDate = GetRelevantDate(history);

			List<EbayAggregation> aggregations = Library.Instance.DB.Fill<EbayAggregation>(
				"LoadActiveEbayAggregations",
				CommandSpecies.StoredProcedure,
				new QueryParameter("MpID", mp.Id),
				new QueryParameter("RelevantDate", relevantDate)
			);

			var calculatedAggregations = new List<IAnalysisDataParameterInfo>();

			if (!aggregations.Any())
				return calculatedAggregations;

			DateTime firstOfMonth = new DateTime(relevantDate.Year, relevantDate.Month, 1);

			AppendAggregations(calculatedAggregations, aggregations, TimePeriodEnum.Month, firstOfMonth);
			AppendAggregations(calculatedAggregations, aggregations, TimePeriodEnum.Month3, firstOfMonth);
			AppendAggregations(calculatedAggregations, aggregations, TimePeriodEnum.Month6, firstOfMonth);
			AppendAggregations(calculatedAggregations, aggregations, TimePeriodEnum.Year, firstOfMonth);
			AppendAggregations(calculatedAggregations, aggregations, TimePeriodEnum.Month15, firstOfMonth);
			AppendAggregations(calculatedAggregations, aggregations, TimePeriodEnum.Month18, firstOfMonth);
			AppendAggregations(calculatedAggregations, aggregations, TimePeriodEnum.Year2, firstOfMonth);
			AppendAggregations(calculatedAggregations, aggregations, TimePeriodEnum.Lifetime, firstOfMonth);

			AppendFeedbacks(calculatedAggregations, mp, history);

			return calculatedAggregations;
		} // GetAggregations

		private void AppendAggregations(
			List<IAnalysisDataParameterInfo> target,
			List<EbayAggregation> ag,
			TimePeriodEnum timePeriod,
			DateTime firstOfMonth
		) {
			ITimePeriod time = TimePeriodFactory.Create(timePeriod);

			if (time.TimePeriodType != TimePeriodEnum.Lifetime)
				ag = ag.Where(x => x.TheMonth > firstOfMonth.AddMonths(-time.MonthsInPeriod)).ToList();

			var parameterInfos = new List<IAnalysisDataParameterInfo>();

			AddAnalysisItem(parameterInfos, time, AggregationFunction.Turnover, ag.Sum(x => x.Turnover));

			AddAnalysisItem(
				parameterInfos,
				time,
				AggregationFunction.AverageSumOfOrder,
				ag.Sum(x => x.AverageSumOfOrderNumerator),
				ag.Sum(x => x.AverageSumOfOrderDenominator)
			);

			AddAnalysisItem(
				parameterInfos,
				time,
				AggregationFunction.AverageItemsPerOrder,
				ag.Sum(x => x.AverageItemsPerOrderNumerator),
				ag.Sum(x => x.AverageItemsPerOrderDenominator)
			);

			AddAnalysisItem(
				parameterInfos,
				time,
				AggregationFunction.CancelledOrdersCount,
				ag.Sum(x => x.CancelledOrdersCount)
			);

			AddAnalysisItem(parameterInfos, time, AggregationFunction.NumOfOrders, ag.Sum(x => x.NumOfOrders));

			AddAnalysisItem(
				parameterInfos,
				time,
				AggregationFunction.OrdersCancellationRate,
				ag.Sum(x => x.OrdersCancellationRateNumerator),
				ag.Sum(x => x.OrdersCancellationRateDenominator)
			);

			AddAnalysisItem(parameterInfos, time, AggregationFunction.TotalItemsOrdered, ag.Sum(x => x.TotalItemsOrdered));

			AddAnalysisItem(parameterInfos, time, AggregationFunction.TotalSumOfOrders, ag.Sum(x => x.TotalSumOfOrders));

			if (time.MonthsInPeriod <= MonthsInYear) {
				AddAnalysisItem(
					parameterInfos,
					time,
					AggregationFunction.TotalSumOfOrdersAnnualized,
					ag.Sum(x => x.TotalSumOfOrders) * (MonthsInYear / (decimal)time.MonthsInPeriod)
				);
			} // if

			target.AddRange(parameterInfos);
		} // AppendAggregations

		private void AppendFeedbacks(
			List<IAnalysisDataParameterInfo> target,
			MP_CustomerMarketPlace mp,
			DateTime? history
		) {
			MP_EbayFeedback feedbacks;

			if (history == null)
				feedbacks = mp.EbayFeedback.OrderByDescending(x => x.Created).FirstOrDefault();
			else {
				feedbacks = mp.EbayFeedback
					.Where(x => x.Created <= history)
					.OrderByDescending(x => x.Created)
					.FirstOrDefault();
			} // if

			if (feedbacks == null)
				return;

			feedbacks.FeedbackByPeriodItems.ForEach(afp => {
				ITimePeriod timePeriod = TimePeriodFactory.CreateById(afp.TimePeriod.InternalId);

				int sum = (afp.Positive ?? 0) + (afp.Neutral ?? 0) + (afp.Negative ?? 0);

				AddAnalysisItem(target, timePeriod, AggregationFunction.NumberOfReviews, sum);

				AddAnalysisItem(target, timePeriod, AggregationFunction.NegativeFeedbackRate, afp.Negative ?? 0);
				AddAnalysisItem(target, timePeriod, AggregationFunction.NeutralFeedbackRate, afp.Neutral ?? 0);
				AddAnalysisItem(target, timePeriod, AggregationFunction.PositiveFeedbackRate, afp.Positive ?? 0);

				AddAnalysisItem(target, timePeriod, AggregationFunction.PositiveFeedbackPercentRate, afp.Positive ?? 0, sum);
				AddAnalysisItem(target, timePeriod, AggregationFunction.NeutralFeedbackPercentRate, afp.Neutral ?? 0, sum);
				AddAnalysisItem(target, timePeriod, AggregationFunction.NegativeFeedbackPercentRate, afp.Negative ?? 0, sum);
			});
		} // AppendFeedbacks

		private void AddAnalysisItem(
			List<IAnalysisDataParameterInfo> target,
			ITimePeriod time,
			AggregationFunction function,
			decimal numerator,
			decimal denominator
		) {
			AddAnalysisItem(target, time, function, denominator == 0 ? 0 : numerator / denominator);
		} // AddAnalysisItem

		private void AddAnalysisItem<T>(
			List<IAnalysisDataParameterInfo> target,
			ITimePeriod time,
			AggregationFunction function,
			T val
		) {
			target.Add(new AnalysisDataParameterInfo(time, val, function));
		} // AddAnalysisItem
	} // class eBayMarketPlaceType

	public class eBayMarketPlaceTypeMap : SubclassMap<eBayMarketPlaceType> {
		public eBayMarketPlaceTypeMap() {
			DiscriminatorValue("eBay");
		} // constructor
	} // eBayMarketPlaceTypeMap
} // ns
