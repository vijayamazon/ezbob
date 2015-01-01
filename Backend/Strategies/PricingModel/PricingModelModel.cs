﻿namespace Ezbob.Backend.Strategies.PricingModel
{
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using System.Text;

	[DataContract]
	public class PricingModelModel
	{
		// Main input fields
		[DataMember]
		public decimal LoanAmount { get; set; }
		[DataMember]
		public decimal DefaultRate { get; set; }
		[DataMember]
		public decimal DefaultRateCompanyShare { get; set; }
		[DataMember]
		public decimal DefaultRateCustomerShare { get; set; }
		[DataMember]
		public decimal SetupFeePounds { get; set; }
		[DataMember]
		public decimal SetupFeePercents { get; set; }
		[DataMember]
		public decimal BrokerSetupFeePounds { get; set; }
		[DataMember]
		public decimal BrokerSetupFeePercents { get; set; }
		[DataMember]
		public int LoanTerm { get; set; }

		// Other input fields
		[DataMember]
		public int InterestOnlyPeriod { get; set; }
		[DataMember]
		public decimal TenurePercents { get; set; }
		[DataMember]
		public decimal TenureMonths { get; set; }
		[DataMember]
		public decimal CollectionRate { get; set; }
		[DataMember]
		public decimal EuCollectionRate { get; set; }
		[DataMember]
		public decimal CosmeCollectionRate { get; set; }
		[DataMember]
		public decimal Cogs { get; set; }
		[DataMember]
		public decimal DebtPercentOfCapital { get; set; }
		[DataMember]
		public decimal CostOfDebt { get; set; }
		[DataMember]
		public decimal OpexAndCapex { get; set; }
		[DataMember]
		public decimal ProfitMarkup { get; set; }

		// Main output fields
		[DataMember]
		public decimal MonthlyInterestRate { get; set; }
		
		// Other output fields
		[DataMember]
		public decimal Revenue { get; set; }
		[DataMember]
		public decimal InterestRevenue { get; set; }
		[DataMember]
		public decimal FeesRevenue { get; set; }
		[DataMember]
		public decimal CogsOutput { get; set; }
		[DataMember]
		public decimal GrossProfit { get; set; }
		[DataMember]
		public decimal OpexAndCapexOutput { get; set; }
		[DataMember]
		public decimal Ebitda { get; set; }
		[DataMember]
		public decimal NetLossFromDefaults { get; set; }
		[DataMember]
		public decimal CostOfDebtOutput { get; set; }
		[DataMember]
		public decimal TotalCost { get; set; }
		[DataMember]
		public decimal ProfitMarkupOutput { get; set; }
		[DataMember]
		public List<PricingSourceModel> PricingSourceModels { get; set; }

		public PricingModelModel Clone() {
			List<PricingSourceModel> pricingSourceModels = new List<PricingSourceModel>();
			foreach (var pricingSourceModel in PricingSourceModels) {
				pricingSourceModels.Add(new PricingSourceModel {
					AIR = pricingSourceModel.AIR,
					APR = pricingSourceModel.APR,
					InterestRate = pricingSourceModel.InterestRate,
					IsPreferable = pricingSourceModel.IsPreferable,
					SetupFee = pricingSourceModel.SetupFee,
					Source = pricingSourceModel.Source
				});
			}
			return new PricingModelModel {
				PricingSourceModels = pricingSourceModels,
				LoanAmount = LoanAmount,
				DefaultRate = DefaultRate,
				DefaultRateCompanyShare = DefaultRateCompanyShare,
				DefaultRateCustomerShare = DefaultRateCustomerShare,
				SetupFeePounds = SetupFeePounds,
				SetupFeePercents = SetupFeePercents,
				BrokerSetupFeePounds = BrokerSetupFeePounds,
				BrokerSetupFeePercents = BrokerSetupFeePercents,
				LoanTerm = LoanTerm,
				InterestOnlyPeriod = InterestOnlyPeriod,
				TenurePercents = TenurePercents,
				TenureMonths = TenureMonths,
				CollectionRate = CollectionRate,
				EuCollectionRate = EuCollectionRate,
				CosmeCollectionRate = CosmeCollectionRate,
				Cogs = Cogs,
				DebtPercentOfCapital = DebtPercentOfCapital,
				CostOfDebt = CostOfDebt,
				OpexAndCapex = OpexAndCapex,
				ProfitMarkup = ProfitMarkup,
				Revenue = Revenue,
				InterestRevenue = InterestRevenue,
				FeesRevenue = FeesRevenue,
				CogsOutput = CogsOutput,
				GrossProfit = GrossProfit,
				OpexAndCapexOutput = OpexAndCapexOutput,
				Ebitda = Ebitda,
				NetLossFromDefaults = NetLossFromDefaults,
				CostOfDebtOutput = CostOfDebtOutput,
				TotalCost = TotalCost,
				ProfitMarkupOutput = ProfitMarkupOutput,
			};
		}
	}
	[DataContract]
	public class PricingSourceModel {
		[DataMember]
		public string Source { get; set; }
		[DataMember]
		public decimal InterestRate { get; set; }
		[DataMember]
		public decimal SetupFee { get; set; }
		[DataMember]
		public decimal AIR { get; set; }
		[DataMember]
		public decimal APR { get; set; }
		[DataMember]
		public bool IsPreferable { get; set; }
	}
}