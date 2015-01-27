namespace EZBob.DatabaseLib.Model.Marketplaces.Yodlee {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EzBob.CommonLib.TimePeriodLogic;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Repository.Turnover;
	using FluentNHibernate.Mapping;

	public class YodleeMarketPlaceType : MP_MarketplaceType {
		public override int UWPriority { get { return 3; } }

		public override IEnumerable<IAnalysisDataParameterInfo> GetAggregations(MP_CustomerMarketPlace mp, DateTime? history) {
			var aggregations = mp.UpdatingHistory.SelectMany(x => x.YodleeAggregations).Where(ag => ag.IsActive).OrderByDescending(ag => ag.TheMonth).ToList();

			var calculatedAggregations = new List<IAnalysisDataParameterInfo>();

			if (!aggregations.Any()) {
				return calculatedAggregations;
			}
			if (history.HasValue) {
				aggregations = aggregations.Where(x => x.TheMonth < history).ToList();
			}

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
			return calculatedAggregations;

		}//GetAggregations

		private IEnumerable<IAnalysisDataParameterInfo> GetAnalysisDataParameters(List<YodleeAggregation> ag, ITimePeriod time, DateTime firstOfMonth) {
			if (time.TimePeriodType != TimePeriodEnum.Lifetime) {
				ag = ag.Where(x => x.TheMonth >= firstOfMonth.AddMonths(-time.MonthsInPeriod)).ToList();
			}
			var parameterInfos = new List<IAnalysisDataParameterInfo>();
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.Turnover), AggregationFunction.Turnover));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.NetCashFlow), AggregationFunction.NetCashFlow));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.NumberOfTransactions), AggregationFunction.NumberOfTransactions));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.TotalExpense), AggregationFunction.TotalExpense));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.TotalIncome), AggregationFunction.TotalIncome));

			if (time.MonthsInPeriod <= MonthsInYear) {
				parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.TotalIncome) * (MonthsInYear / (decimal)time.MonthsInPeriod), AggregationFunction.TotalIncomeAnnualized));
			}

			return parameterInfos;
		}//GetAnalysisDataParameters
	}//class YodleeMarketPlaceType

	public class YodleeMarketPlaceTypeMap : SubclassMap<YodleeMarketPlaceType> {
		public YodleeMarketPlaceTypeMap() {
			DiscriminatorValue("Yodlee");
		}
	}//YodleeMarketPlaceTypeMap
}//ns
