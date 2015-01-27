namespace EZBob.DatabaseLib.Repository.Turnover {
	using ApplicationMng.Repository;
	using EZBob.DatabaseLib.Model.Database;
	using FluentNHibernate.Mapping;
	using NHibernate;
	using NHibernate.Type;

	public class PayPointAggregation {
		public virtual long PayPointAggregationID { get; set; }
		
		public virtual System.DateTime TheMonth { get; set; }
		public virtual bool IsActive { get; set; }
		public virtual decimal Turnover { get; set; }
		public virtual decimal CancellationRateDenominator { get; set; }
		public virtual decimal CancellationRateNumerator { get; set; }
		public virtual decimal CancellationValue { get; set; }
		public virtual int NumOfFailures { get; set; }
		public virtual int NumOfOrders { get; set; }
		public virtual decimal OrdersAverageDenominator { get; set; }
		public virtual decimal OrdersAverageNumerator { get; set; }
		public virtual decimal SumOfAuthorisedOrders { get; set; }
		
		public virtual MP_CustomerMarketplaceUpdatingHistory CustomerMarketPlaceUpdatingHistory { get; set; }

	}//PayPointAggregation


	public class PayPointAggregationRepository : NHibernateRepositoryBase<PayPointAggregation> {
		public PayPointAggregationRepository(ISession session) : base(session) { } // constructor
	}//PayPointAggregationRepository


	public class PayPointAggregationMap : ClassMap<PayPointAggregation> {
		public PayPointAggregationMap() {

			Table("PayPointAggregation");

			Id(x => x.PayPointAggregationID).Column("PayPointAggregationID").GeneratedBy.Native();
		
			Map(x => x.Turnover).Precision(18).Scale(2);
			Map(x => x.TheMonth).CustomType<UtcDateTimeType>();
			Map(x => x.IsActive);

			Map(x => x.CancellationRateDenominator);
			Map(x => x.CancellationRateNumerator);
			Map(x => x.CancellationValue);
			Map(x => x.NumOfFailures);
			Map(x => x.NumOfOrders);
			Map(x => x.OrdersAverageDenominator);
			Map(x => x.OrdersAverageNumerator);
			Map(x => x.SumOfAuthorisedOrders);

			References(x => x.CustomerMarketPlaceUpdatingHistory, "CustomerMarketPlaceUpdatingHistoryID").Cascade.None();
		} // constructor
	}//PayPointAggregationMap

} // namespace