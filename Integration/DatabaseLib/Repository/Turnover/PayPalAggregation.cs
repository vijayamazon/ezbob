
namespace EZBob.DatabaseLib.Repository.Turnover {
	using ApplicationMng.Repository;
	using EZBob.DatabaseLib.Model.Database;
	using FluentNHibernate.Mapping;
	using NHibernate;

	public class PayPalAggregation {

		public PayPalAggregation() { }

		public virtual long PayPalAggregationID { get; set; }
		public virtual System.DateTime TheMonth { get; set; }
		public virtual bool IsActive { get; set; }
		public virtual decimal Turnover { get; set; }
		public virtual decimal GrossIncome { get; set; }
		public virtual int NetNumOfRefundsAndReturns { get; set; }
		public virtual decimal NetSumOfRefundsAndReturns { get; set; }
		public virtual decimal NetTransfersAmount { get; set; }
		public virtual int NumOfTotalTransactions { get; set; }
		public virtual int NumTransfersIn { get; set; }
		public virtual int NumTransfersOut { get; set; }
		public virtual decimal OutstandingBalance { get; set; }
		public virtual decimal RatioNetSumOfRefundsAndReturnsToNetRevenuesDenominator { get; set; }
		public virtual decimal RatioNetSumOfRefundsAndReturnsToNetRevenuesNumerator { get; set; }
		public virtual decimal RevenuesForTransactions { get; set; }
		public virtual decimal TotalNetExpenses { get; set; }
		public virtual decimal TotalNetInPayments { get; set; }
		public virtual decimal TotalNetOutPayments { get; set; }
		public virtual decimal TotalNetRevenues { get; set; }
		public virtual int TransactionsNumber { get; set; }
		public virtual decimal TransferAndWireIn { get; set; }
		public virtual decimal TransferAndWireOut { get; set; }
		public virtual decimal AmountPerTransferInNumerator { get; set; }
		public virtual decimal AmountPerTransferInDenominator { get; set; }
		public virtual decimal AmountPerTransferOutNumerator { get; set; }
		public virtual decimal AmountPerTransferOutDenominator { get; set; }
		public virtual decimal GrossProfitMarginNumerator { get; set; }
		public virtual decimal GrossProfitMarginDenominator { get; set; }
		public virtual decimal RevenuePerTrasnactionNumerator { get; set; }
		public virtual decimal RevenuePerTrasnactionDenominator { get; set; }

		public virtual MP_CustomerMarketplaceUpdatingHistory CustomerMarketPlaceUpdatingHistory { get; set; }

	}

	public class PayPalAggregationRepository : NHibernateRepositoryBase<PayPalAggregation> {
		public PayPalAggregationRepository(ISession session) : base(session) { } // constructor

	}

	public class PayPalAggregationMap : ClassMap<PayPalAggregation> {
		public PayPalAggregationMap() {

			Table("PayPalAggregation");

			Id(x => x.PayPalAggregationID).Column("PayPalAggregationID").GeneratedBy.Native();

			Map(x => x.Turnover).Precision(18).Scale(2);
			Map(x => x.TheMonth);
			Map(x => x.IsActive);

			Map(x => x.GrossIncome);
			Map(x => x.NetNumOfRefundsAndReturns);
			Map(x => x.NetSumOfRefundsAndReturns);
			Map(x => x.NetTransfersAmount);
			Map(x => x.NumOfTotalTransactions);
			Map(x => x.NumTransfersIn);
			Map(x => x.NumTransfersOut);
			Map(x => x.OutstandingBalance);
			Map(x => x.RatioNetSumOfRefundsAndReturnsToNetRevenuesDenominator);
			Map(x => x.RatioNetSumOfRefundsAndReturnsToNetRevenuesNumerator);
			Map(x => x.RevenuesForTransactions);
			Map(x => x.TotalNetExpenses);
			Map(x => x.TotalNetInPayments);
			Map(x => x.TotalNetOutPayments);
			Map(x => x.TransactionsNumber);
			Map(x => x.TransferAndWireIn);
			Map(x => x.TransferAndWireOut);
			Map(x => x.AmountPerTransferInNumerator);
			Map(x => x.AmountPerTransferInDenominator);
			Map(x => x.AmountPerTransferOutNumerator);
			Map(x => x.GrossProfitMarginNumerator);
			Map(x => x.GrossProfitMarginDenominator);
			Map(x => x.RevenuePerTrasnactionNumerator);
			Map(x => x.RevenuePerTrasnactionDenominator);

			References(x => x.CustomerMarketPlaceUpdatingHistory, "CustomerMarketPlaceUpdatingHistoryID").Cascade.None();

		} // constructor
	}
}