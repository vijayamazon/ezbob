namespace EZBob.DatabaseLib.Model.Alibaba {
	using System;
	using System.Linq;
	using ApplicationMng.Repository;
	using EZBob.DatabaseLib.Model.Database;
	using FluentNHibernate.Mapping;
	using NHibernate;

	public class AlibabaContractItem {

        public virtual long? ItemId { get; set; }
        public virtual AlibabaContract Contract { get; set; }
        public virtual long OrderProdNumber { get; set; }
        public virtual string ProductName { get; set; }
        public virtual string ProductSpecs { get; set; }
        public virtual int? ProductQuantity { get; set; }
        public virtual int? ProductUnit { get; set; }
        public virtual int? ProductUnitPrice { get; set; }
        public virtual int? ProductTotalAmount { get; set; }
	}

	public class AlibabaContractItemRepository : NHibernateRepositoryBase<AlibabaContractItem> {
        public AlibabaContractItemRepository(ISession session) : base(session) { }

	}


	public class AlibabaContractItemMap : ClassMap<AlibabaContractItem> {
        public AlibabaContractItemMap()
        {

			Table("AlibabaBuyer");

            Id(x => x.ItemId).GeneratedBy.Identity();

            Map(x => x.OrderProdNumber);
            Map(x => x.ProductName);
            Map(x => x.ProductSpecs);
            Map(x => x.ProductQuantity);
            Map(x => x.ProductUnit);
            Map(x => x.ProductUnitPrice);
            Map(x => x.ProductTotalAmount);

            References(x => x.Contract, "ContractId").LazyLoad().Cascade.None();
		} // constructor
	}
}
