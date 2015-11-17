namespace EZBob.DatabaseLib.Model.Marketplaces.PayPoint {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Database;
	using EzBob.CommonLib.TimePeriodLogic;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Repository.Turnover;
	using FluentNHibernate.Mapping;

	public class PayPointMarketPlaceType : MP_MarketplaceType {
		public override int UWPriority { get { return 2; } }

		public override IEnumerable<IAnalysisDataParameterInfo> GetAggregations(MP_CustomerMarketPlace mp, DateTime? history) {
			DateTime relevantDate = GetRelevantDate(history);

			List<PayPointAggregation> aggregations = Library.Instance.DB.Fill<PayPointAggregation>(
				"LoadActivePayPointAggregations",
				CommandSpecies.StoredProcedure,
				new QueryParameter("MpID", mp.Id),
				new QueryParameter("RelevantDate", relevantDate)
			);

			var calculatedAggregations = new List<IAnalysisDataParameterInfo>();

			if (!aggregations.Any()) {
				return calculatedAggregations;
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

		private IEnumerable<IAnalysisDataParameterInfo> GetAnalysisDataParameters(List<PayPointAggregation> ag, ITimePeriod time, DateTime firstOfMonth) {
			if (time.TimePeriodType != TimePeriodEnum.Lifetime) {
				ag = ag.Where(x => x.TheMonth > firstOfMonth.AddMonths(-time.MonthsInPeriod)).ToList();
			}
			var parameterInfos = new List<IAnalysisDataParameterInfo>();
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.Turnover), AggregationFunction.Turnover));
			var cancellationRateDenominator = ag.Sum(x => x.CancellationRateDenominator);
			parameterInfos.Add(new AnalysisDataParameterInfo(time, cancellationRateDenominator == 0 ? 0 : ag.Sum(x => x.CancellationRateNumerator) / cancellationRateDenominator, AggregationFunction.CancellationRate));
			var ordersAverageDenominator = ag.Sum(x => x.OrdersAverageDenominator);
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ordersAverageDenominator == 0 ? 0 : ag.Sum(x => x.OrdersAverageNumerator) / ordersAverageDenominator, AggregationFunction.OrdersAverage));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.CancellationValue), AggregationFunction.CancellationValue));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.NumOfFailures), AggregationFunction.NumOfFailures));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.NumOfOrders), AggregationFunction.NumOfOrders));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.SumOfAuthorisedOrders), AggregationFunction.SumOfAuthorisedOrders));

			return parameterInfos;
		}//GetAnalysisDataParameters
	}//class PayPointMarketPlaceType

	public class PayPointMarketPlaceTypeMap : SubclassMap<PayPointMarketPlaceType> {
		public PayPointMarketPlaceTypeMap() {
			DiscriminatorValue("PayPoint");
		}
	}//PayPointMarketPlaceTypeMap
}//ns
