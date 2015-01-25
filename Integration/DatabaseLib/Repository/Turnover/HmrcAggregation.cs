namespace EZBob.DatabaseLib.Repository.Turnover {
	using ApplicationMng.Repository;
	using EZBob.DatabaseLib.Model.Database;
	using FluentNHibernate.Mapping;
	using NHibernate;
	using NHibernate.Type;

	public class HmrcAggregation {
		
		public virtual long HmrcAggregationID { get; set; }
		public virtual System.DateTime TheMonth { get; set; }
		public virtual bool IsActive { get; set; }
		public virtual decimal Turnover { get; set; }
		public virtual decimal ValueAdded { get; set; }
		public virtual decimal FreeCashFlow { get; set; }

		public virtual MP_CustomerMarketplaceUpdatingHistory CustomerMarketPlaceUpdatingHistory { get; set; }
	}

	public class HmrcAggregationRepository : NHibernateRepositoryBase<HmrcAggregation> {
		public HmrcAggregationRepository(ISession session) : base(session) { } // constructor

	}


	public class HmrcAggregationMap : ClassMap<HmrcAggregation> {
		public HmrcAggregationMap() {

			Table("HmrcAggregation");
			Id(x => x.HmrcAggregationID).Column("HmrcAggregationID").GeneratedBy.Native();

			Map(x => x.TheMonth).CustomType<UtcDateTimeType>();
			Map(x => x.IsActive);

			Map(x => x.Turnover).Precision(18).Scale(2);
			Map(x => x.ValueAdded);
			Map(x => x.FreeCashFlow);

			References(x => x.CustomerMarketPlaceUpdatingHistory, "CustomerMarketPlaceUpdatingHistoryID").Cascade.None();
	
		} // constructorCustomerMarketPlaceUpdatingHistoryID
	}
}
