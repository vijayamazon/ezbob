namespace EZBob.DatabaseLib.Model.Marketplaces.FreeAgent {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EzBob.CommonLib.TimePeriodLogic;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Repository.Turnover;
	using FluentNHibernate.Mapping;

	public class FreeAgentMarketPlaceType : MP_MarketplaceType {
		public override int UWPriority { get { return 4; } }

		public override IEnumerable<IAnalysisDataParameterInfo> GetAggregations(MP_CustomerMarketPlace mp, DateTime? history) {
			var aggregations = mp.UpdatingHistory.SelectMany(x => x.FreeAgentAggregations).Where(ag => ag.IsActive).OrderByDescending(ag => ag.TheMonth).ToList();

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

		private IEnumerable<IAnalysisDataParameterInfo> GetAnalysisDataParameters(List<FreeAgentAggregation> ag, ITimePeriod time, DateTime firstOfMonth) {
			if (time.TimePeriodType != TimePeriodEnum.Lifetime) {
				ag = ag.Where(x => x.TheMonth >= firstOfMonth.AddMonths(-time.MonthsInPeriod)).ToList();
			}
			var parameterInfos = new List<IAnalysisDataParameterInfo>();
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.Turnover), AggregationFunction.Turnover));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.NumOfExpenses), AggregationFunction.NumOfExpenses));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.NumOfOrders), AggregationFunction.NumOfOrders));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.SumOfAdminExpensesCategory), AggregationFunction.SumOfAdminExpensesCategory));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.SumOfCostOfSalesExpensesCategory), AggregationFunction.SumOfCostOfSalesExpensesCategory));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.SumOfDraftInvoices), AggregationFunction.SumOfDraftInvoices));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.SumOfGeneralExpensesCategory), AggregationFunction.SumOfGeneralExpensesCategory));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.SumOfOpenInvoices), AggregationFunction.SumOfOpenInvoices));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.SumOfOverdueInvoices), AggregationFunction.SumOfOverdueInvoices));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.SumOfPaidInvoices), AggregationFunction.SumOfPaidInvoices));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.TotalSumOfExpenses), AggregationFunction.TotalSumOfExpenses));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.TotalSumOfOrders), AggregationFunction.TotalSumOfOrders));

			return parameterInfos;
		}//GetAnalysisDataParameters
	}//class FreeAgentMarketPlaceType

	public class FreeAgentMarketPlaceTypeMap : SubclassMap<FreeAgentMarketPlaceType> {
		public FreeAgentMarketPlaceTypeMap() {
			DiscriminatorValue("FreeAgent");
		}
	}//FreeAgentMarketPlaceTypeMap
}//ns
