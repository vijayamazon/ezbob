namespace EZBob.DatabaseLib.Model.Alibaba {
	using System;
	using System.Linq;
	using ApplicationMng.Repository;
	using EZBob.DatabaseLib.Model.Database;
	using FluentNHibernate.Mapping;
	using NHibernate;

	public class AlibabaContract {

		public virtual long ContractId { get; set; }
		public virtual string RequestId { get; set; }
		public virtual long? LoanId { get; set; }
        public virtual string OrderNumber { get; set; }
        public virtual string ShippingMark { get; set; }
        public virtual int? TotalOrderAmount { get; set; }
        public virtual int? DeviationQuantityAllowed { get; set; }
        public virtual string OrderAddtlDetails { get; set; }
        public virtual string ShippingTerms { get; set; }
        public virtual DateTime? ShippingDate { get; set; }
        public virtual string LoadingPort { get; set; }
        public virtual string DestinationPort { get; set; }
        public virtual int? TACoveredAmount { get; set; }
        public virtual int? OrderDeposit { get; set; }
        public virtual int? OrderBalance { get; set; }
        public virtual string OrderCurrency { get; set; }
        public virtual byte?[] CommercialInvoice { get; set; }
        public virtual byte?[] BillOfLading { get; set; }
        public virtual byte?[] PackingList { get; set; }
        public virtual byte?[] Other { get; set; }
	}

    public class AlibabaContractRepository : NHibernateRepositoryBase<AlibabaContract>
    {
        public AlibabaContractRepository(ISession session) : base(session) { }
	}


	public class AlibabaContractMap : ClassMap<AlibabaContract> {
        public AlibabaContractMap()
        {

            Table("AlibabaContract");

            Id(x => x.ContractId).GeneratedBy.Identity();

            Map(x => x.RequestId);
            Map(x => x.LoanId);
            Map(x => x.OrderNumber);
            Map(x => x.ShippingMark);
            Map(x => x.TotalOrderAmount);
            Map(x => x.DeviationQuantityAllowed);
            Map(x => x.OrderAddtlDetails);
            Map(x => x.ShippingTerms);
            Map(x => x.ShippingDate);
            Map(x => x.LoadingPort);
            Map(x => x.DestinationPort);
            Map(x => x.TACoveredAmount);
            Map(x => x.OrderDeposit);
            Map(x => x.OrderBalance);
            Map(x => x.OrderCurrency);
            Map(x => x.CommercialInvoice);
            Map(x => x.BillOfLading);
            Map(x => x.PackingList);
            Map(x => x.Other);
		} // constructor
	}
}
