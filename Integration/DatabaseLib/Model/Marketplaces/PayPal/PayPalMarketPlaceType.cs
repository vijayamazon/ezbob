namespace EZBob.DatabaseLib.Model.Marketplaces.PayPal {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EzBob.CommonLib.TimePeriodLogic;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Repository.Turnover;
	using FluentNHibernate.Mapping;

	public class PayPalMarketPlaceType : MP_MarketplaceType {
		public override int UWPriority { get { return 1; } }

		public override IEnumerable<IAnalysisDataParameterInfo> GetAggregations(MP_CustomerMarketPlace mp, DateTime? history) {
			DateTime relevantDate = GetRelevantDate(history);
			var aggregations = mp.UpdatingHistory
				.SelectMany(x => x.PayPalAggregations)
				.Where(ag => ag.IsActive && ag.TheMonth < relevantDate)
				.OrderByDescending(ag => ag.TheMonth).ToList();

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

		}

		private IEnumerable<IAnalysisDataParameterInfo> GetAnalysisDataParameters(List<PayPalAggregation> ag, ITimePeriod time, DateTime firstOfMonth) {
			if (time.TimePeriodType != TimePeriodEnum.Lifetime) {
				ag = ag.Where(x => x.TheMonth > firstOfMonth.AddMonths(-time.MonthsInPeriod)).ToList();
			}
			var parameterInfos = new List<IAnalysisDataParameterInfo>();
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.Turnover), AggregationFunction.Turnover));
			var amountPerTransferInDenominator = ag.Sum(x => x.AmountPerTransferInDenominator);
			parameterInfos.Add(new AnalysisDataParameterInfo(time, amountPerTransferInDenominator == 0 ? 0 : ag.Sum(x => x.AmountPerTransferInNumerator) / amountPerTransferInDenominator, AggregationFunction.AmountPerTransferIn));
			var amountPerTransferOutDenominator = ag.Sum(x => x.AmountPerTransferOutDenominator);
			parameterInfos.Add(new AnalysisDataParameterInfo(time, amountPerTransferOutDenominator == 0 ? 0 : ag.Sum(x => x.AmountPerTransferOutNumerator) / amountPerTransferOutDenominator, AggregationFunction.AmountPerTransferOut));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.GrossIncome), AggregationFunction.GrossIncome));
			var grossProfitMarginDenominator = ag.Sum(x => x.GrossProfitMarginDenominator);
			parameterInfos.Add(new AnalysisDataParameterInfo(time, grossProfitMarginDenominator == 0 ? 0 : ag.Sum(x => x.GrossProfitMarginNumerator) / grossProfitMarginDenominator, AggregationFunction.GrossProfitMargin));
			var ratioNetSumOfRefundsAndReturnsToNetRevenuesDenominator = ag.Sum(x => x.RatioNetSumOfRefundsAndReturnsToNetRevenuesDenominator);
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ratioNetSumOfRefundsAndReturnsToNetRevenuesDenominator == 0 ? 0 : ag.Sum(x => x.RatioNetSumOfRefundsAndReturnsToNetRevenuesNumerator) / ratioNetSumOfRefundsAndReturnsToNetRevenuesDenominator, AggregationFunction.RatioNetSumOfRefundsAndReturnsToNetRevenues));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.NetNumOfRefundsAndReturns), AggregationFunction.NetNumOfRefundsAndReturns));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.NetSumOfRefundsAndReturns), AggregationFunction.NetSumOfRefundsAndReturns));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.NetTransfersAmount), AggregationFunction.NetTransfersAmount));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.NumOfTotalTransactions), AggregationFunction.NumOfTotalTransactions));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.NumTransfersIn), AggregationFunction.NumTransfersIn));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.NumTransfersOut), AggregationFunction.NumTransfersOut));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.OutstandingBalance), AggregationFunction.OutstandingBalance));
			var revenuePerTrasnactionDenominator = ag.Sum(x => x.RevenuePerTrasnactionDenominator);
			parameterInfos.Add(new AnalysisDataParameterInfo(time, revenuePerTrasnactionDenominator == 0 ? 0 : ag.Sum(x => x.RevenuePerTrasnactionNumerator) / revenuePerTrasnactionDenominator, AggregationFunction.RevenuePerTrasnaction));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.RevenuesForTransactions), AggregationFunction.RevenuesForTransactions));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.TotalNetExpenses), AggregationFunction.TotalNetExpenses));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.TotalNetInPayments), AggregationFunction.TotalNetInPayments));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.TotalNetOutPayments), AggregationFunction.TotalNetOutPayments));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.TotalNetRevenues), AggregationFunction.TotalNetRevenues));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.TransactionsNumber), AggregationFunction.TransactionsNumber));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.TransferAndWireIn), AggregationFunction.TransferAndWireIn));
			parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.TransferAndWireOut), AggregationFunction.TransferAndWireOut));

			if (time.MonthsInPeriod <= MonthsInYear) {
				parameterInfos.Add(new AnalysisDataParameterInfo(time, ag.Sum(x => x.TotalNetInPayments) * (MonthsInYear / (decimal)time.MonthsInPeriod), AggregationFunction.TotalNetInPaymentsAnnualized));
			}
			return parameterInfos;
		}
	}

	public class PayPalMarketPlaceTypeMap : SubclassMap<PayPalMarketPlaceType> {
		public PayPalMarketPlaceTypeMap() {
			DiscriminatorValue("Pay Pal");
		}
	}
}
