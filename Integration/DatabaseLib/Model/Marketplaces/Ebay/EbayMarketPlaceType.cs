namespace EZBob.DatabaseLib.Model.Marketplaces.Ebay {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EzBob.CommonLib.TimePeriodLogic;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Repository.Turnover;
	using FluentNHibernate.Mapping;
	using NHibernate.Linq;

	public class eBayMarketPlaceType : MP_MarketplaceType {
		public override int UWPriority { get { return 1; } }

		public override IEnumerable<IAnalysisDataParameterInfo> GetAggregations(MP_CustomerMarketPlace mp, DateTime? history) {
			var aggregations = mp.UpdatingHistory.SelectMany(x => x.EbayAggregations).Where(ag => ag.IsActive).OrderByDescending(ag => ag.TheMonth).ToList();
			var calculatedAggregations = new List<IAnalysisDataParameterInfo>();

			if (!aggregations.Any()) {
				return calculatedAggregations;
			}//if

			if (history.HasValue) {
				aggregations = aggregations.Where(x => x.TheMonth < history).ToList();
			}//if

			var month = TimePeriodFactory.Create(TimePeriodEnum.Month);
			var month3 = TimePeriodFactory.Create(TimePeriodEnum.Month3);
			var month6 = TimePeriodFactory.Create(TimePeriodEnum.Month6);
			var year = TimePeriodFactory.Create(TimePeriodEnum.Year);
			var month15 = TimePeriodFactory.Create(TimePeriodEnum.Month15);
			var month18 = TimePeriodFactory.Create(TimePeriodEnum.Month18);
			var year2 = TimePeriodFactory.Create(TimePeriodEnum.Year2);
			var all = TimePeriodFactory.Create(TimePeriodEnum.Lifetime);

			DateTime today = history ?? DateTime.UtcNow;
			DateTime firstOfMonth = new DateTime(today.Year, today.Month, 1);

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
		}//GetAggregations

		private IEnumerable<IAnalysisDataParameterInfo> GetAnalysisDataParameters(List<EbayAggregation> ag, ITimePeriod time, DateTime firstOfMonth) {
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
		}//GetAnalysisDataParameters

		protected List<IAnalysisDataParameterInfo> GetFeedbacks(MP_CustomerMarketPlace mp, DateTime? history) {
			MP_EbayFeedback feedbacks;
			if (history == null) {
				feedbacks = mp.EbayFeedback
					.OrderByDescending(x => x.Created)
					.FirstOrDefault();
			} else {
				feedbacks = mp.EbayFeedback
					.Where(x => x.Created <= history)
					.OrderByDescending(x => x.Created)
					.FirstOrDefault();
			}//if

			var feedBackParams = new List<IAnalysisDataParameterInfo>();

			if (feedbacks == null) {
				return feedBackParams;
			}//if

			feedbacks.FeedbackByPeriodItems.ForEach(afp => {
				var timePeriod = TimePeriodFactory.CreateById(afp.TimePeriod.InternalId);

				var g = new AnalysisDataParameterInfo(timePeriod, afp.Negative, AggregationFunction.NegativeFeedbackRate);
				var n = new AnalysisDataParameterInfo(timePeriod, afp.Neutral, AggregationFunction.NeutralFeedbackRate);
				var p = new AnalysisDataParameterInfo(timePeriod, afp.Positive, AggregationFunction.PositiveFeedbackRate);

				var sum = afp.Positive + afp.Neutral + afp.Negative;
				var c = new AnalysisDataParameterInfo(timePeriod, sum, AggregationFunction.NumberOfReviews);

				decimal? positivePercent = sum != 0 ? ((afp.Positive)) / (decimal)sum.Value : null;
				decimal? neutralPercent = sum != 0 ? ((afp.Neutral)) / (decimal)sum.Value : null;
				decimal? negativePercent = sum != 0 ? ((afp.Negative)) / (decimal)sum.Value : null;
				var pp = new AnalysisDataParameterInfo(timePeriod, positivePercent, AggregationFunction.PositiveFeedbackPercentRate);
				var np = new AnalysisDataParameterInfo(timePeriod, neutralPercent, AggregationFunction.NeutralFeedbackPercentRate);
				var gp = new AnalysisDataParameterInfo(timePeriod, negativePercent, AggregationFunction.NegativeFeedbackPercentRate);

				feedBackParams.AddRange(new[] { c, n, g, p, pp, np, gp });
			});

			return feedBackParams;
		}//GetFeedbacks
	}//class eBayMarketPlaceType

	public class eBayMarketPlaceTypeMap : SubclassMap<eBayMarketPlaceType> {
		public eBayMarketPlaceTypeMap() {
			DiscriminatorValue("eBay");
		}//constructor
	}//eBayMarketPlaceTypeMap
}//ns
