namespace EZBob.DatabaseLib.Repository.Turnover {
	using ApplicationMng.Repository;
	using EZBob.DatabaseLib.Model.Database;
	using FluentNHibernate.Mapping;
	using NHibernate;
	using NHibernate.Type;

	public class FreeAgentAggregation {
		public virtual long FreeAgentAggregationID { get; set; }
		
		public virtual System.DateTime TheMonth { get; set; }
		public virtual bool IsActive { get; set; }
		public virtual decimal Turnover { get; set; }
		public virtual int NumOfExpenses { get; set; }
		public virtual int NumOfOrders { get; set; }
		public virtual decimal SumOfAdminExpensesCategory { get; set; }
		public virtual decimal SumOfCostOfSalesExpensesCategory { get; set; }
		public virtual decimal SumOfDraftInvoices { get; set; }
		public virtual decimal SumOfGeneralExpensesCategory { get; set; }
		public virtual decimal SumOfOpenInvoices { get; set; }
		public virtual decimal SumOfOverdueInvoices { get; set; }
		public virtual decimal SumOfPaidInvoices { get; set; }
		public virtual decimal TotalSumOfExpenses { get; set; }
		public virtual decimal TotalSumOfOrders { get; set; }

		public virtual MP_CustomerMarketplaceUpdatingHistory CustomerMarketPlaceUpdatingHistory { get; set; }

	}//FreeAgentAggregation


	public class FreeAgentAggregationRepository : NHibernateRepositoryBase<FreeAgentAggregation> {
		public FreeAgentAggregationRepository(ISession session) : base(session) { } // constructor
	}//FreeAgentAggregationRepository


	public class FreeAgentAggregationMap : ClassMap<FreeAgentAggregation> {
		public FreeAgentAggregationMap() {

			Table("FreeAgentAggregation");

			Id(x => x.FreeAgentAggregationID).Column("FreeAgentAggregationID").GeneratedBy.Native();
		
			Map(x => x.Turnover).Precision(18).Scale(2);
			Map(x => x.TheMonth).CustomType<UtcDateTimeType>();
			Map(x => x.IsActive);

			Map(x => x.NumOfExpenses);
			Map(x => x.NumOfOrders);
			Map(x => x.SumOfAdminExpensesCategory);
			Map(x => x.SumOfCostOfSalesExpensesCategory);
			Map(x => x.SumOfDraftInvoices);
			Map(x => x.SumOfGeneralExpensesCategory);
			Map(x => x.SumOfOpenInvoices);
			Map(x => x.SumOfOverdueInvoices);
			Map(x => x.SumOfPaidInvoices);
			Map(x => x.TotalSumOfExpenses);
			Map(x => x.TotalSumOfOrders);

			References(x => x.CustomerMarketPlaceUpdatingHistory, "CustomerMarketPlaceUpdatingHistoryID").Cascade.None();

		} // constructor
	}//FreeAgentAggregationMap

} // namespace