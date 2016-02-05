namespace Ezbob.Backend.ModelsWithDB {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.Serialization;
	using AutomationCalculator.Common;
	using Ezbob.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;

	[DataContract(IsReference = true)]
	public class PricingModelModel {
		//*********************************************************************
		//
		// Input fields
		//
		//*********************************************************************
		[DataMember]
		public AutoDecisionFlowTypes FlowType { get; set; }
		[DataMember]
		public decimal LoanAmount { get; set; }
		[DataMember]
		public decimal DefaultRateCompanyShare { get; set; }
		[DataMember]
		[FieldName("SetupFee")]
		public decimal SetupFeePercents { get; set; }
		[DataMember]
		[FieldName("BrokerSetupFee")]
		public decimal BrokerSetupFeePercents { get; set; }
		[DataMember]
		public int LoanTerm { get; set; }
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
		[FieldName("COSMECollectionRate")]
		public decimal CosmeCollectionRate { get; set; }
		[DataMember]
		public decimal Cogs { get; set; }
		[DataMember]
		public decimal DebtPercentOfCapital { get; set; }
		[DataMember]
		[FieldName("CostOfDebtPA")]
		public decimal CostOfDebt { get; set; }
		[DataMember]
		public decimal OpexAndCapex { get; set; }
		[DataMember]
		[FieldName("ProfitMarkupPercentsOfRevenue")]
		public decimal ProfitMarkup { get; set; }

		public decimal SetupFeePounds { get { return SetupFeePercents * LoanAmount; } }
		public decimal BrokerSetupFeePounds { get { return BrokerSetupFeePercents * LoanAmount; } }
		public decimal DefaultRateCustomerShare { get { return 1 - DefaultRateCompanyShare; } }

		//*********************************************************************
		//
		// Output fields
		//
		//*********************************************************************
		[DataMember]
		public decimal MonthlyInterestRate { get; set; }
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

		[DataMember]
		public int ConsumerScore { get; set; }
		[DataMember]
		public decimal ConsumerDefaultRate { get; set; }
		[DataMember]
		[FieldName("BusinessScore")]
		public int CompanyScore { get; set; }
		[DataMember]
		[FieldName("BusinessDefaultRate")]
		public decimal CompanyDefaultRate { get; set; }

		[DataMember]
		public int? GradeID { get; set; }
		[DataMember]
		public decimal? GradeScore { get; set; }
		[DataMember]
		public decimal? ProbabilityOfDefault { get; set; }

		[DataMember]
		public int OriginID { get; set; }

		public Bucket? Grade {
			get {
				if (GradeID == null)
					return null;

				return buckets.Contains(GradeID.Value) ? (Bucket)GradeID.Value : (Bucket?)null;
			} // get
		} // Grade

		public decimal DefaultRate {
			get {
				return (FlowType == AutoDecisionFlowTypes.LogicalGlue)
					? (ProbabilityOfDefault ?? 1)
					: ConsumerDefaultRate * DefaultRateCustomerShare + CompanyDefaultRate * DefaultRateCompanyShare;
			} // get
		} // DefaultRate

		//*********************************************************************
		//
		// Helpers.
		//
		//*********************************************************************

		public PricingModelModel ClearOutput() {
			PricingSourceModels = null;
			// TODO
			return this;
		} // ClearOutput

		public PricingModelModel Clone() {
			List<PricingSourceModel> pricingSourceModels = (PricingSourceModels == null)
				? null
				: PricingSourceModels.Select(pricingSourceModel => pricingSourceModel.Clone()).ToList();

			return new PricingModelModel {
				PricingSourceModels = pricingSourceModels,
				FlowType = FlowType,
				LoanAmount = LoanAmount,
				DefaultRateCompanyShare = DefaultRateCompanyShare,
				SetupFeePercents = SetupFeePercents,
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
				ConsumerScore = ConsumerScore,
				ConsumerDefaultRate = ConsumerDefaultRate,
				CompanyScore = CompanyScore,
				CompanyDefaultRate = CompanyDefaultRate,
				GradeID = GradeID,
				GradeScore = GradeScore,
				ProbabilityOfDefault = ProbabilityOfDefault,
				OriginID = OriginID,
			};
		} // Clone

		private static readonly int[] buckets = (int[])Enum.GetValues(typeof(Bucket));
	} // class PricingModelModel

	[DataContract]
	public class PricingSourceModel {
		[DataMember]
		public LoanSourceName LoanSource { get; set; }

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

		public PricingSourceModel Clone() {
			return new PricingSourceModel {
				LoanSource = LoanSource,
				Source = Source,
				InterestRate = InterestRate,
				SetupFee = SetupFee,
				AIR = AIR,
				APR = APR,
				IsPreferable = IsPreferable,
			};
		} // Clone
	} // class PricingSourceModel
} // namespace