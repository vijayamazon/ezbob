namespace EZBob.DatabaseLib.Model.Marketplaces.EKM {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EzBob.CommonLib.TimePeriodLogic;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Repository.Turnover;
	using FluentNHibernate.Mapping;

	public class EKMMarketPlaceType : MP_MarketplaceType {
		public override int UWPriority { get { return 3; } }

		public override IEnumerable<IAnalysisDataParameterInfo> GetAggregations(MP_CustomerMarketPlace mp, DateTime? history) {
			DateTime relevantDate = GetRelevantDate(history);
			var aggregations = mp.UpdatingHistory
				.SelectMany(x => x.EkmAggregations)
				.Where(ag => ag.IsActive && ag.TheMonth < relevantDate)
				.OrderByDescending(ag => ag.TheMonth)
				.ToList();

			var calculatedAggregations = new List<IAnalysisDataParameterInfo>();

			if (!aggregations.Any()) {
				return calculatedAggregations;
			}

			if (history.HasValue) {
				aggregations = aggregations.Where(x => x.TheMonth < history).ToList();
			}

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
			return calculatedAggregations;

		}//GetAggregations

		private IEnumerable<IAnalysisDataParameterInfo> GetAnalysisDataParameters(List<EkmAggregation> ag, ITimePeriod time, DateTime firstOfMonth) {
			if (time.TimePeriodType != TimePeriodEnum.Lifetime) {
				ag = ag.Where(x => x.TheMonth > firstOfMonth.AddMonths(-time.MonthsInPeriod)).ToList();
			}
			var parameterInfos = new List<IAnalysisDataParameterInfo>();
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.Turnover), AggregationFunction.Turnover));
			var averageSumOfOrderDenominator = ag.Sum(x => x.AverageSumOfOrderDenominator);
			parameterInfos.Add(new AnalysisDataParameterInfo(time, averageSumOfOrderDenominator == 0 ? 0 : ag.Sum(x => x.AverageSumOfOrderNumerator) / averageSumOfOrderDenominator, AggregationFunction.AverageSumOfOrder));
			var averageSumOfCancelledOrderDenominator = ag.Sum(x => x.AverageSumOfCancelledOrderDenominator);
			parameterInfos.Add(new AnalysisDataParameterInfo(time, averageSumOfCancelledOrderDenominator == 0 ? 0 : ag.Sum(x => x.AverageSumOfCancelledOrderNumerator) / averageSumOfCancelledOrderDenominator, AggregationFunction.AverageSumOfCancelledOrder));
			var averageSumOfOtherOrderDenominator = ag.Sum(x => x.AverageSumOfOtherOrderDenominator);
			parameterInfos.Add(new AnalysisDataParameterInfo(time, averageSumOfOtherOrderDenominator == 0 ? 0 : ag.Sum(x => x.AverageSumOfOtherOrderNumerator) / averageSumOfOtherOrderDenominator, AggregationFunction.AverageSumOfOtherOrder));
			var cancellationRateDenominator = ag.Sum(x => x.CancellationRateDenominator);
			parameterInfos.Add(new AnalysisDataParameterInfo(time, cancellationRateDenominator == 0 ? 0 : ag.Sum(x => x.CancellationRateNumerator) / cancellationRateDenominator, AggregationFunction.CancellationRate));

			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.NumOfCancelledOrders), AggregationFunction.NumOfCancelledOrders));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.NumOfOrders), AggregationFunction.NumOfOrders));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.NumOfOtherOrders), AggregationFunction.NumOfOtherOrders));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.TotalSumOfCancelledOrders), AggregationFunction.TotalSumOfCancelledOrders));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.TotalSumOfOrders), AggregationFunction.TotalSumOfOrders));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.TotalSumOfOtherOrders), AggregationFunction.TotalSumOfOtherOrders));

			if (time.MonthsInPeriod <= MonthsInYear) {
				parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.TotalSumOfOrders) * (MonthsInYear / (decimal)time.MonthsInPeriod), AggregationFunction.TotalSumOfOrdersAnnualized));
			}
			return parameterInfos;
		}//GetAnalysisDataParameters
	}//class EKMMarketPlaceType

	public class EKMMarketPlaceTypeMap : SubclassMap<EKMMarketPlaceType> {
		public EKMMarketPlaceTypeMap() {
			DiscriminatorValue("EKM");
		}
	}//EKMMarketPlaceTypeMap
}//ns
