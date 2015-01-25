namespace EZBob.DatabaseLib.Repository.Turnover {
	using ApplicationMng.Repository;
	using EZBob.DatabaseLib.Model.Database;
	using FluentNHibernate.Mapping;
	using NHibernate;

	public class YodleeAggregation {

		public virtual long YodleeAggregationID {get;set;}		
		public virtual System.DateTime TheMonth {get;set;}
		public virtual bool IsActive {get;set;}
		public virtual decimal Turnover {get;set;}
		public virtual int NumberOfTransactions {get;set;}
		public virtual decimal TotalExpense {get;set;}
		public virtual decimal TotalIncome {get;set;}
		public virtual decimal NetCashFlow {get;set;}

		public virtual MP_CustomerMarketplaceUpdatingHistory CustomerMarketPlaceUpdatingHistory { get; set; }
	}

	public class YodleeAggregationRepository : NHibernateRepositoryBase<YodleeAggregation> {
		public YodleeAggregationRepository(ISession session) : base(session) { } // constructor

	}


	public class YodleeAggregationMap : ClassMap<YodleeAggregation> {
		public YodleeAggregationMap() {

			Table("YodleeAggregation");

			Id(x => x.YodleeAggregationID).Column("YodleeAggregationID").GeneratedBy.Native();
			
			Map(x => x.TheMonth);
			Map(x => x.IsActive);
			Map(x => x.Turnover).Precision(18).Scale(2);
			Map(x => x.NumberOfTransactions);
			Map(x => x.TotalExpense);
			Map(x => x.TotalIncome);
			Map(x => x.NetCashFlow);

			References(x => x.CustomerMarketPlaceUpdatingHistory, "CustomerMarketPlaceUpdatingHistoryID").Cascade.None();

		} // constructor
	} 
}
