namespace EZBob.DatabaseLib.Model.Marketplaces.Sage {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EzBob.CommonLib.TimePeriodLogic;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Repository.Turnover;
	using FluentNHibernate.Mapping;

	public class SageMarketPlaceType : MP_MarketplaceType {
		public override int UWPriority { get { return 14; } }

		public override IEnumerable<IAnalysisDataParameterInfo> GetAggregations(MP_CustomerMarketPlace mp, DateTime? history) {
			var aggregations = mp.UpdatingHistory.SelectMany(x => x.SageAggregations).Where(ag => ag.IsActive).OrderByDescending(ag => ag.TheMonth).ToList();

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

		private IEnumerable<IAnalysisDataParameterInfo> GetAnalysisDataParameters(List<SageAggregation> ag, ITimePeriod time, DateTime firstOfMonth) {
			if (time.TimePeriodType != TimePeriodEnum.Lifetime) {
				ag = ag.Where(x => x.TheMonth >= firstOfMonth.AddMonths(-time.MonthsInPeriod)).ToList();
			}
			var parameterInfos = new List<IAnalysisDataParameterInfo>();
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.Turnover), AggregationFunction.Turnover));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.NumOfExpenditures), AggregationFunction.NumOfExpenditures));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.NumOfIncomes), AggregationFunction.NumOfIncomes));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.NumOfOrders), AggregationFunction.NumOfOrders));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.NumOfPurchaseInvoices), AggregationFunction.NumOfPurchaseInvoices));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.TotalSumOfExpenditures), AggregationFunction.TotalSumOfExpenditures));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.TotalSumOfIncomes), AggregationFunction.TotalSumOfIncomes));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.TotalSumOfOrders), AggregationFunction.TotalSumOfOrders));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.TotalSumOfPaidPurchaseInvoices), AggregationFunction.TotalSumOfPaidPurchaseInvoices));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.TotalSumOfPaidSalesInvoices), AggregationFunction.TotalSumOfPaidSalesInvoices));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.TotalSumOfPartiallyPaidPurchaseInvoices), AggregationFunction.TotalSumOfPartiallyPaidPurchaseInvoices));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.TotalSumOfPartiallyPaidSalesInvoices), AggregationFunction.TotalSumOfPartiallyPaidSalesInvoices));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.TotalSumOfPurchaseInvoices), AggregationFunction.TotalSumOfPurchaseInvoices));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.TotalSumOfUnpaidPurchaseInvoices), AggregationFunction.TotalSumOfUnpaidPurchaseInvoices));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.TotalSumOfUnpaidSalesInvoices), AggregationFunction.TotalSumOfUnpaidSalesInvoices));

			return parameterInfos;
		}//GetAnalysisDataParameters
	}//class SageMarketPlaceType

	public class SageMarketPlaceTypeMap : SubclassMap<SageMarketPlaceType> {
		public SageMarketPlaceTypeMap() {
			DiscriminatorValue("Sage");
		}
	}//SageMarketPlaceTypeMap
}//ns
