namespace EZBob.DatabaseLib.Model.Alibaba {
    using System.Linq;
    using ApplicationMng.Repository;
    using EZBob.DatabaseLib.Model.Database;
    using FluentNHibernate.Mapping;
    using NHibernate;

    public class AlibabaBuyer {

        public virtual int Id { get; set; }
        public virtual long AliId { get; set; }
        public virtual decimal? Freeze { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual int? ContractId { get; set; }
        public virtual string BussinessName { get; set; }
        public virtual string street1 { get; set; }
        public virtual string street2 { get; set; }
        public virtual string City { get; set; }
        public virtual string State { get; set; }
        public virtual string Zip { get; set; }
        public virtual string Country { get; set; }
        public virtual string AuthRepFname { get; set; }
        public virtual string AuthRepLname { get; set; }
        public virtual string Phone { get; set; }
        public virtual string Fax { get; set; }
        public virtual string Email { get; set; }
        public virtual int? OrderRequestCountLastYear { get; set; }
        public virtual bool? ConfirmShippingDocAndAmount { get; set; }
        public virtual string FinancingType { get; set; }
        public virtual bool? ConfirmReleaseFunds { get; set; }
	}

	public class AlibabaBuyerRepository : NHibernateRepositoryBase<AlibabaBuyer> {
		public AlibabaBuyerRepository(ISession session) : base(session) { }

		public AlibabaBuyer ByCustomer(int customerId) {
			return GetAll().FirstOrDefault(l => l.Customer.Id == customerId);
		}

        public AlibabaBuyer ByCustomerRefNum(string customerRefNum)
        {
            return GetAll().FirstOrDefault(l => l.Customer.RefNumber.Equals(customerRefNum));
        }
	}


	public class AlibabaBuyerMap : ClassMap<AlibabaBuyer> {
		public AlibabaBuyerMap() {

			Table("AlibabaBuyer");
           
            Id(x => x.Id).GeneratedBy.Identity();

            Map(x => x.AliId);
            Map(x => x.Freeze);
            Map(x => x.BussinessName);
            Map(x => x.street1);
            Map(x => x.street2);
            Map(x => x.City);
            Map(x => x.State);
            Map(x => x.Zip);
            Map(x => x.Country);
            Map(x => x.AuthRepFname);
            Map(x => x.AuthRepLname);
            Map(x => x.Phone);
            Map(x => x.Fax);
            Map(x => x.Email);
            Map(x => x.OrderRequestCountLastYear);
            Map(x => x.ConfirmShippingDocAndAmount);
            Map(x => x.FinancingType);
            Map(x => x.ConfirmReleaseFunds);
		    Map(x => x.ContractId);

            References(x => x.Customer, "CustomerId").LazyLoad().Cascade.None();
            
		} // constructor
	}
}
