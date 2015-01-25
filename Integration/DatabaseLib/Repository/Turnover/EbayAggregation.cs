namespace EZBob.DatabaseLib.Repository.Turnover {
	using ApplicationMng.Repository;
	using EZBob.DatabaseLib.Model.Database;
	using FluentNHibernate.Mapping;
	using NHibernate;

	public class EbayAggregation {

		public EbayAggregation() { }

		public virtual long EbayAggregationID { get; set; }
		
		public virtual System.DateTime TheMonth { get; set; }
		public virtual bool IsActive { get; set; }
		public virtual decimal Turnover { get; set; }
		public virtual int AverageItemsPerOrderDenominator { get; set; }
		public virtual int AverageItemsPerOrderNumerator { get; set; }
		public virtual decimal AverageSumOfOrderDenominator { get; set; }
		public virtual decimal AverageSumOfOrderNumerator { get; set; }
		public virtual int CancelledOrdersCount { get; set; }
		public virtual int NumOfOrders { get; set; }
		public virtual decimal OrdersCancellationRateDenominator { get; set; }
		public virtual decimal OrdersCancellationRateNumerator { get; set; }
		public virtual int TotalItemsOrdered { get; set; }
		public virtual decimal TotalSumOfOrders { get; set; }

		public virtual MP_CustomerMarketplaceUpdatingHistory CustomerMarketPlaceUpdatingHistory { get; set; }

	}


	public class EbayAggregationRepository : NHibernateRepositoryBase<EbayAggregation> {
		public EbayAggregationRepository(ISession session) : base(session) { } // constructor

	}

	public class EbayAggregationMap : ClassMap<EbayAggregation> {
		public EbayAggregationMap() {

			Table("EbayAggregation");

			Map(x => x.EbayAggregationID);
			Id(x => x.EbayAggregationID).Column("EbayAggregationID").GeneratedBy.Native();

			Map(x => x.Turnover).Precision(18).Scale(2);
			Map(x => x.TheMonth);
			Map(x => x.IsActive);

			Map(x => x.AverageItemsPerOrderDenominator);
			Map(x => x.AverageItemsPerOrderNumerator);
			Map(x => x.AverageSumOfOrderDenominator);
			Map(x => x.AverageSumOfOrderNumerator);
			Map(x => x.CancelledOrdersCount);
			Map(x => x.NumOfOrders);
			Map(x => x.OrdersCancellationRateDenominator);
			Map(x => x.OrdersCancellationRateNumerator);
			Map(x => x.TotalItemsOrdered);
			Map(x => x.TotalSumOfOrders);

			References(x => x.CustomerMarketPlaceUpdatingHistory, "CustomerMarketPlaceUpdatingHistoryID").Cascade.None();

		} // constructor
	}
}
