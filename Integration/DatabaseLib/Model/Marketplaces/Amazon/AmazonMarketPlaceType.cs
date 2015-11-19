namespace EZBob.DatabaseLib.Model.Marketplaces.Amazon {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Database;
	using EzBob.CommonLib.TimePeriodLogic;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Repository.Turnover;
	using FluentNHibernate.Mapping;

	public class AmazonMarketPlaceType : MP_MarketplaceType {
		public override int UWPriority { get { return 2; } }

		public override IEnumerable<IAnalysisDataParameterInfo> GetAggregations(MP_CustomerMarketPlace mp, DateTime? history) {
			DateTime relevantDate = GetRelevantDate(history);

			List<AmazonAggregation> aggregations = Library.Instance.DB.Fill<AmazonAggregation>(
				"LoadActiveAmazonAggregations",
				CommandSpecies.StoredProcedure,
				new QueryParameter("MpID", mp.Id),
				new QueryParameter("RelevantDate", relevantDate)
			);

			var calculatedAggregations = new List<IAnalysisDataParameterInfo>();

			if (!aggregations.Any())
				return calculatedAggregations;
			
			DateTime firstOfMonth = new DateTime(relevantDate.Year, relevantDate.Month, 1);

			var month = TimePeriodFactory.Create(TimePeriodEnum.Month);
			var month3 = TimePeriodFactory.Create(TimePeriodEnum.Month3);
			var month6 = TimePeriodFactory.Create(TimePeriodEnum.Month6);
			var year = TimePeriodFactory.Create(TimePeriodEnum.Year);
			var month15 = TimePeriodFactory.Create(TimePeriodEnum.Month15);
			var month18 = TimePeriodFactory.Create(TimePeriodEnum.Month18);
			var year2 = TimePeriodFactory.Create(TimePeriodEnum.Year2);
			var all = TimePeriodFactory.Create(TimePeriodEnum.Lifetime);

			calculatedAggregations.AddRange(GetAnalysisDataParameters(aggregations, month, firstOfMonth));
			calculatedAggregations.AddRange(GetAnalysisDataParameters(aggregations, month3, firstOfMonth));
			calculatedAggregations.AddRange(GetAnalysisDataParameters(aggregations, month6, firstOfMonth));
			calculatedAggregations.AddRange(GetAnalysisDataParameters(aggregations, year, firstOfMonth));
			calculatedAggregations.AddRange(GetAnalysisDataParameters(aggregations, month15, firstOfMonth));
			calculatedAggregations.AddRange(GetAnalysisDataParameters(aggregations, month18, firstOfMonth));
			calculatedAggregations.AddRange(GetAnalysisDataParameters(aggregations, year2, firstOfMonth));
			calculatedAggregations.AddRange(GetAnalysisDataParameters(aggregations, all, firstOfMonth));

			calculatedAggregations.AddRange(GetFeedbacks(mp, history));
			return calculatedAggregations;
		}

		private IEnumerable<IAnalysisDataParameterInfo> GetAnalysisDataParameters(List<AmazonAggregation> ag, ITimePeriod time, DateTime firstOfMonth) {
			if (time.TimePeriodType != TimePeriodEnum.Lifetime) {
				ag = ag.Where(x => x.TheMonth > firstOfMonth.AddMonths(-time.MonthsInPeriod)).ToList();
			}
			var parameterInfos = new List<IAnalysisDataParameterInfo>();
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.Turnover), AggregationFunction.Turnover));
			var averageSumOfOrderDenominator = ag.Sum(x => x.AverageSumOfOrderDenominator);
			parameterInfos.Add(new AnalysisDataParameterInfo(time, averageSumOfOrderDenominator == 0 ? 0 : ag.Sum(x => x.AverageSumOfOrderNumerator) / averageSumOfOrderDenominator, AggregationFunction.AverageSumOfOrder));
			var averageItemsPerOrderDenominator = ag.Sum(x => x.AverageItemsPerOrderDenominator);
			parameterInfos.Add(new AnalysisDataParameterInfo(time, averageItemsPerOrderDenominator == 0 ? 0 : ag.Sum(x => x.AverageItemsPerOrderNumerator) / averageItemsPerOrderDenominator, AggregationFunction.AverageItemsPerOrder));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.CancelledOrdersCount), AggregationFunction.CancelledOrdersCount));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.NumOfOrders), AggregationFunction.NumOfOrders));
			var ordersCancellationRateDenominator = ag.Sum(x => x.OrdersCancellationRateDenominator);
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ordersCancellationRateDenominator == 0 ? 0 : ag.Sum(x => x.OrdersCancellationRateNumerator) / ordersCancellationRateDenominator, AggregationFunction.OrdersCancellationRate));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.TotalItemsOrdered), AggregationFunction.TotalItemsOrdered));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.TotalSumOfOrders), AggregationFunction.TotalSumOfOrders));

			if (time.MonthsInPeriod <= MonthsInYear) {
				parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.TotalSumOfOrders) * (MonthsInYear / (decimal)time.MonthsInPeriod), AggregationFunction.TotalSumOfOrdersAnnualized));
			}

			return parameterInfos;
		}

		protected IEnumerable<IAnalysisDataParameterInfo> GetFeedbacks(MP_CustomerMarketPlace mp, DateTime? history) {
			MP_AmazonFeedback feedbacks;
			if (history == null) {
				feedbacks = mp.AmazonFeedback
					.OrderByDescending(x => x.Created)
					.FirstOrDefault();
			} else {
				feedbacks = mp.AmazonFeedback
					.Where(x => x.Created <= history)
					.OrderByDescending(x => x.Created)
					.FirstOrDefault();
			}//if

			var feedBackParams = new List<IAnalysisDataParameterInfo>();

			if (feedbacks == null)
				return feedBackParams;

			feedBackParams.Add(new AnalysisDataParameterInfo(TimePeriodFactory.Create(TimePeriodEnum.Zero), feedbacks.UserRaining, AggregationFunction.UserRating));

			foreach (MP_AmazonFeedbackItem afp in feedbacks.FeedbackByPeriodItems) {
				var timePeriod = TimePeriodFactory.CreateById(afp.TimePeriod.InternalId);

				var c = new AnalysisDataParameterInfo(timePeriod, afp.Count, AggregationFunction.NumberOfReviews);
				var g = new AnalysisDataParameterInfo(timePeriod, afp.Negative, AggregationFunction.NegativeFeedbackRate);
				var n = new AnalysisDataParameterInfo(timePeriod, afp.Neutral, AggregationFunction.NeutralFeedbackRate);
				var p = new AnalysisDataParameterInfo(timePeriod, afp.Positive, AggregationFunction.PositiveFeedbackRate);

				var sum = afp.Positive + afp.Neutral + afp.Negative;
				decimal positivePercent = sum != 0 ? ((afp.Positive ?? 0)) / (decimal)sum.Value : 0M;
				decimal neutralPercent = sum != 0 ? ((afp.Neutral ?? 0)) / (decimal)sum.Value : 0M;
				decimal negativePercent = sum != 0 ? ((afp.Negative ?? 0)) / (decimal)sum.Value : 0M;
				var pp = new AnalysisDataParameterInfo(timePeriod, positivePercent, AggregationFunction.PositiveFeedbackPercentRate);
				var np = new AnalysisDataParameterInfo(timePeriod, neutralPercent, AggregationFunction.NeutralFeedbackPercentRate);
				var gp = new AnalysisDataParameterInfo(timePeriod, negativePercent, AggregationFunction.NegativeFeedbackPercentRate);

				feedBackParams.AddRange(new[] { c, n, g, p, pp, np, gp });
			} // for each

			return feedBackParams;
		} // GetFeedbacks
	} // class AmazonMarketPlaceType

	public class AmazonMarketPlaceTypeMap : SubclassMap<AmazonMarketPlaceType> {
		public AmazonMarketPlaceTypeMap() {
			DiscriminatorValue("Amazon");
		} // constructor
	} // AmazonMarketPlaceTypeMap
} // namespace
