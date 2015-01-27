namespace EZBob.DatabaseLib.Repository.Turnover {
	using System;
	using ApplicationMng.Repository;
	using EZBob.DatabaseLib.Model.Database;
	using FluentNHibernate.Mapping;
	using NHibernate;
	using NHibernate.Type;

	public class ChannelGrabberAggregation {

		public virtual long ChannelGrabberAggregationID { get; set; }
		//public virtual byte[] TimestampCounter { get; set; }
		public virtual DateTime TheMonth { get; set; }
		public virtual bool IsActive { get; set; }
		public virtual decimal Turnover { get; set; }
		public virtual decimal AverageSumOfExpensesDenominator { get; set; }
		public virtual decimal AverageSumOfExpensesNumerator { get; set; }
		public virtual decimal AverageSumOfOrdersDenominator { get; set; }
		public virtual decimal AverageSumOfOrdersNumerator { get; set; }
		public virtual int NumOfExpenses { get; set; }
		public virtual int NumOfOrders { get; set; }
		public virtual decimal TotalSumOfExpenses { get; set; }
		public virtual decimal TotalSumOfOrders { get; set; }

		public virtual MP_CustomerMarketplaceUpdatingHistory CustomerMarketPlaceUpdatingHistory { get; set; }
	}//ChannelGrabberAggregation

	public class ChannelGrabberAggregationRepository : NHibernateRepositoryBase<ChannelGrabberAggregation> {
		public ChannelGrabberAggregationRepository(ISession session) : base(session) { } // constructor

	}//ChannelGrabberAggregationRepository


	public class ChannelGrabberAggregationMap : ClassMap<ChannelGrabberAggregation> {
		public ChannelGrabberAggregationMap() {

			Table("ChannelGrabberAggregation");
			Id(x => x.ChannelGrabberAggregationID).Column("ChannelGrabberAggregationID").GeneratedBy.Native();

			Map(x => x.TheMonth).CustomType<UtcDateTimeType>();
			Map(x => x.IsActive);

			Map(x => x.Turnover).Precision(18).Scale(2);
			Map(x => x.AverageSumOfExpensesDenominator);
			Map(x => x.AverageSumOfExpensesNumerator);
			Map(x => x.AverageSumOfOrdersDenominator);
			Map(x => x.AverageSumOfOrdersNumerator);
			Map(x => x.NumOfExpenses);
			Map(x => x.NumOfOrders);
			Map(x => x.TotalSumOfExpenses);
			Map(x => x.TotalSumOfOrders);

			References(x => x.CustomerMarketPlaceUpdatingHistory, "CustomerMarketPlaceUpdatingHistoryID").Cascade.None();
		} // ChannelGrabberAggregationMap
	}
}
