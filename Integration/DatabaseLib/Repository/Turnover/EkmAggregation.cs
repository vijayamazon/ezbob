namespace EZBob.DatabaseLib.Repository.Turnover {
	using ApplicationMng.Repository;
	using EZBob.DatabaseLib.Model.Database;
	using FluentNHibernate.Mapping;
	using NHibernate;
	using NHibernate.Type;

	public class EkmAggregation {
		public virtual long EkmAggregationID { get; set; }
		
		public virtual System.DateTime TheMonth { get; set; }
		public virtual bool IsActive { get; set; }
		public virtual decimal Turnover { get; set; }
		public virtual decimal AverageSumOfCancelledOrderDenominator { get; set; }
		public virtual decimal AverageSumOfCancelledOrderNumerator { get; set; }
		public virtual decimal AverageSumOfOrderDenominator { get; set; }
		public virtual decimal AverageSumOfOrderNumerator { get; set; }
		public virtual decimal AverageSumOfOtherOrderDenominator { get; set; }
		public virtual decimal AverageSumOfOtherOrderNumerator { get; set; }
		public virtual decimal CancellationRateDenominator { get; set; }
		public virtual decimal CancellationRateNumerator { get; set; }
		public virtual int NumOfCancelledOrders { get; set; }
		public virtual int NumOfOrders { get; set; }
		public virtual int NumOfOtherOrders { get; set; }
		public virtual decimal TotalSumOfCancelledOrders { get; set; }
		public virtual decimal TotalSumOfOrders { get; set; }
		public virtual decimal TotalSumOfOtherOrders { get; set; }

		public virtual MP_CustomerMarketplaceUpdatingHistory CustomerMarketPlaceUpdatingHistory { get; set; }

	}//EkmAggregation


	public class EkmAggregationRepository : NHibernateRepositoryBase<EkmAggregation> {
		public EkmAggregationRepository(ISession session) : base(session) { } // constructor
	}//EkmAggregationRepository


	public class EkmAggregationMap : ClassMap<EkmAggregation> {
		public EkmAggregationMap() {

			Table("EkmAggregation");

			Id(x => x.EkmAggregationID).Column("EkmAggregationID").GeneratedBy.Native();
		
			Map(x => x.Turnover).Precision(18).Scale(2);
			Map(x => x.TheMonth).CustomType<UtcDateTimeType>();
			Map(x => x.IsActive);

			Map(x => x.AverageSumOfCancelledOrderDenominator);
			Map(x => x.AverageSumOfCancelledOrderNumerator);
			Map(x => x.AverageSumOfOrderDenominator);
			Map(x => x.AverageSumOfOrderNumerator);
			Map(x => x.AverageSumOfOtherOrderDenominator);
			Map(x => x.AverageSumOfOtherOrderNumerator);
			Map(x => x.CancellationRateDenominator);
			Map(x => x.CancellationRateNumerator);
			Map(x => x.NumOfCancelledOrders);
			Map(x => x.NumOfOrders);
			Map(x => x.NumOfOtherOrders);
			Map(x => x.TotalSumOfCancelledOrders);
			Map(x => x.TotalSumOfOrders);
			Map(x => x.TotalSumOfOtherOrders);

			References(x => x.CustomerMarketPlaceUpdatingHistory, "CustomerMarketPlaceUpdatingHistoryID").Cascade.None();
		} // constructor
	}//EkmAggregationMap

} // namespace