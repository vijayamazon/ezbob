namespace EZBob.DatabaseLib.Repository.Turnover {
	using ApplicationMng.Repository;
	using EZBob.DatabaseLib.Model.Database;
	using FluentNHibernate.Mapping;
	using NHibernate;
	using NHibernate.Type;

	public class SageAggregation {
		public virtual long SageAggregationID { get; set; }
		
		public virtual System.DateTime TheMonth { get; set; }
		public virtual bool IsActive { get; set; }
		public virtual decimal Turnover { get; set; }
		public virtual int NumOfExpenditures { get; set; }
		public virtual int NumOfIncomes { get; set; }
		public virtual int NumOfOrders { get; set; }
		public virtual int NumOfPurchaseInvoices { get; set; }
		public virtual decimal TotalSumOfExpenditures { get; set; }
		public virtual decimal TotalSumOfIncomes { get; set; }
		public virtual decimal TotalSumOfOrders { get; set; }
		public virtual decimal TotalSumOfPaidPurchaseInvoices { get; set; }
		public virtual decimal TotalSumOfPaidSalesInvoices { get; set; }
		public virtual decimal TotalSumOfPartiallyPaidPurchaseInvoices { get; set; }
		public virtual decimal TotalSumOfPartiallyPaidSalesInvoices { get; set; }
		public virtual decimal TotalSumOfPurchaseInvoices { get; set; }
		public virtual decimal TotalSumOfUnpaidPurchaseInvoices { get; set; }
		public virtual decimal TotalSumOfUnpaidSalesInvoices { get; set; }

		public virtual MP_CustomerMarketplaceUpdatingHistory CustomerMarketPlaceUpdatingHistory { get; set; }

	}//SageAggregation


	public class SageAggregationRepository : NHibernateRepositoryBase<SageAggregation> {
		public SageAggregationRepository(ISession session) : base(session) { } // constructor
	}//SageAggregationRepository


	public class SageAggregationMap : ClassMap<SageAggregation> {
		public SageAggregationMap() {

			Table("SageAggregation");

			Id(x => x.SageAggregationID).Column("SageAggregationID").GeneratedBy.Native();
		
			Map(x => x.Turnover).Precision(18).Scale(2);
			Map(x => x.TheMonth).CustomType<UtcDateTimeType>();
			Map(x => x.IsActive);

			Map(x => x.NumOfExpenditures);
			Map(x => x.NumOfIncomes);
			Map(x => x.NumOfOrders);
			Map(x => x.NumOfPurchaseInvoices);
			Map(x => x.TotalSumOfExpenditures);
			Map(x => x.TotalSumOfIncomes);
			Map(x => x.TotalSumOfOrders);
			Map(x => x.TotalSumOfPaidPurchaseInvoices);
			Map(x => x.TotalSumOfPaidSalesInvoices);
			Map(x => x.TotalSumOfPartiallyPaidPurchaseInvoices);
			Map(x => x.TotalSumOfPartiallyPaidSalesInvoices);
			Map(x => x.TotalSumOfPurchaseInvoices);
			Map(x => x.TotalSumOfUnpaidPurchaseInvoices);
			Map(x => x.TotalSumOfUnpaidSalesInvoices);
			
			References(x => x.CustomerMarketPlaceUpdatingHistory, "CustomerMarketPlaceUpdatingHistoryID").Cascade.None();
		} // constructor
	}//SageAggregationMap

} // namespace