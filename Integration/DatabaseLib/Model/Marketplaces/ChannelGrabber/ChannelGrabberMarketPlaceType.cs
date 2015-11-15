namespace EZBob.DatabaseLib.Model.Marketplaces.ChannelGrabber {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Database;
	using EzBob.CommonLib.TimePeriodLogic;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Repository.Turnover;
	using FluentNHibernate.Mapping;

	public class ChannelGrabberMarketPlaceType : MP_MarketplaceType {
		public override int UWPriority { get { return 4; } } // UWPriority

		public override IEnumerable<IAnalysisDataParameterInfo> GetAggregations(MP_CustomerMarketPlace mp, DateTime? history) {
			DateTime relevantDate = GetRelevantDate(history);
			DateTime firstOfMonth = new DateTime(relevantDate.Year, relevantDate.Month, 1);

			var month = TimePeriodFactory.Create(TimePeriodEnum.Month);
			var month3 = TimePeriodFactory.Create(TimePeriodEnum.Month3);
			var month6 = TimePeriodFactory.Create(TimePeriodEnum.Month6);
			var year = TimePeriodFactory.Create(TimePeriodEnum.Year);
			var month15 = TimePeriodFactory.Create(TimePeriodEnum.Month15);
			var month18 = TimePeriodFactory.Create(TimePeriodEnum.Month18);
			var year2 = TimePeriodFactory.Create(TimePeriodEnum.Year2);
			var all = TimePeriodFactory.Create(TimePeriodEnum.Lifetime);

			var calculatedAggregations = new List<IAnalysisDataParameterInfo>();
			if (InternalId == new Guid("AE85D6FC-DBDB-4E01-839A-D5BD055CBAEA")) {
				List<HmrcAggregation> aggregations = Library.Instance.DB.Fill<HmrcAggregation>(
					"LoadActiveHmrcAggregations",
					CommandSpecies.StoredProcedure,
					new QueryParameter("MpID", mp.Id),
					new QueryParameter("RelevantDate", relevantDate)
				);

				if (!aggregations.Any())
					return calculatedAggregations;
				
				calculatedAggregations.AddRange(GetAnalysisDataParametersHmrc(aggregations, month, firstOfMonth));
				calculatedAggregations.AddRange(GetAnalysisDataParametersHmrc(aggregations, month3, firstOfMonth));
				calculatedAggregations.AddRange(GetAnalysisDataParametersHmrc(aggregations, month6, firstOfMonth));
				calculatedAggregations.AddRange(GetAnalysisDataParametersHmrc(aggregations, year, firstOfMonth));
				calculatedAggregations.AddRange(GetAnalysisDataParametersHmrc(aggregations, month15, firstOfMonth));
				calculatedAggregations.AddRange(GetAnalysisDataParametersHmrc(aggregations, month18, firstOfMonth));
				calculatedAggregations.AddRange(GetAnalysisDataParametersHmrc(aggregations, year2, firstOfMonth));
				calculatedAggregations.AddRange(GetAnalysisDataParametersHmrc(aggregations, all, firstOfMonth));
			} else {
				List<ChannelGrabberAggregation> aggregations = Library.Instance.DB.Fill<ChannelGrabberAggregation>(
					"LoadActiveChanngelGrabberAggregations",
					CommandSpecies.StoredProcedure,
					new QueryParameter("MpID", mp.Id),
					new QueryParameter("RelevantDate", relevantDate)
				);

				if (!aggregations.Any())
					return calculatedAggregations;

				calculatedAggregations.AddRange(GetAnalysisDataParametersCG(aggregations, month, firstOfMonth));
				calculatedAggregations.AddRange(GetAnalysisDataParametersCG(aggregations, month3, firstOfMonth));
				calculatedAggregations.AddRange(GetAnalysisDataParametersCG(aggregations, month6, firstOfMonth));
				calculatedAggregations.AddRange(GetAnalysisDataParametersCG(aggregations, year, firstOfMonth));
				calculatedAggregations.AddRange(GetAnalysisDataParametersCG(aggregations, month15, firstOfMonth));
				calculatedAggregations.AddRange(GetAnalysisDataParametersCG(aggregations, month18, firstOfMonth));
				calculatedAggregations.AddRange(GetAnalysisDataParametersCG(aggregations, year2, firstOfMonth));
				calculatedAggregations.AddRange(GetAnalysisDataParametersCG(aggregations, all, firstOfMonth));
			}

			return calculatedAggregations;

		}//GetAggregations

		private IEnumerable<IAnalysisDataParameterInfo> GetAnalysisDataParametersHmrc(List<HmrcAggregation> ag, ITimePeriod time, DateTime firstOfMonth) {
			if (time.TimePeriodType != TimePeriodEnum.Lifetime) {
				ag = ag.Where(x => x.TheMonth > firstOfMonth.AddMonths(-time.MonthsInPeriod)).ToList();
			}
			var parameterInfos = new List<IAnalysisDataParameterInfo>();
			
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.Turnover), AggregationFunction.Turnover));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.FreeCashFlow), AggregationFunction.FreeCashFlow));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.ValueAdded), AggregationFunction.ValueAdded));

			return parameterInfos;
		}//GetAnalysisDataParameters

		private IEnumerable<IAnalysisDataParameterInfo> GetAnalysisDataParametersCG(List<ChannelGrabberAggregation> ag, ITimePeriod time, DateTime firstOfMonth) {
			if (time.TimePeriodType != TimePeriodEnum.Lifetime) {
				ag = ag.Where(x => x.TheMonth > firstOfMonth.AddMonths(-time.MonthsInPeriod)).ToList();
			}
			var parameterInfos = new List<IAnalysisDataParameterInfo>();
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.Turnover), AggregationFunction.Turnover));
			var averageSumOfExpensesDenominator = ag.Sum(x => x.AverageSumOfExpensesDenominator);
			parameterInfos.Add(new AnalysisDataParameterInfo(time, averageSumOfExpensesDenominator == 0 ? 0 : ag.Sum(x => x.AverageSumOfExpensesNumerator) / averageSumOfExpensesDenominator, AggregationFunction.AverageSumOfExpenses));
			var averageSumOfOrdersDenominator = ag.Sum(x => x.AverageSumOfOrdersDenominator);
			parameterInfos.Add(new AnalysisDataParameterInfo(time, averageSumOfOrdersDenominator == 0 ? 0 : ag.Sum(x => x.AverageSumOfOrdersNumerator) / averageSumOfOrdersDenominator, AggregationFunction.AverageSumOfOrders));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.NumOfExpenses), AggregationFunction.NumOfExpenses));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.NumOfOrders), AggregationFunction.NumOfOrders));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.TotalSumOfExpenses), AggregationFunction.TotalSumOfExpenses));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.TotalSumOfOrders), AggregationFunction.TotalSumOfOrders));

			if (time.MonthsInPeriod <= MonthsInYear) {
				parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.TotalSumOfOrders) * (MonthsInYear / (decimal)time.MonthsInPeriod), AggregationFunction.TotalSumOfOrdersAnnualized));
			}

			return parameterInfos;
		}//GetAnalysisDataParametersCG
	} // class ChannelGrabberMarketPlaceType

	public class ChannelGrabberMarketPlaceTypeMap : SubclassMap<ChannelGrabberMarketPlaceType> {
		public ChannelGrabberMarketPlaceTypeMap() {
			DiscriminatorValue("ChannelGrabber");
		} // constructor
	} // class ChannelGrabberMarketPlaceTypeMap
}//ns
