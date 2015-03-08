namespace EZBob.DatabaseLib.Model.Alibaba {
	using System.Collections.Generic;
	using System.Linq;
	using ApplicationMng.Repository;
	using EZBob.DatabaseLib.Model.Database;
	using FluentNHibernate.Mapping;
	using NHibernate;

	public class AlibabaBuyer {

		public virtual int Id { get; set; }
		public virtual int AliId { get; set; }
		public virtual decimal? Freeze { get; set; }
		public virtual Customer Customer { get; set; }
	}

	public class AlibabaBuyerRepository : NHibernateRepositoryBase<AlibabaBuyer> {
		public AlibabaBuyerRepository(ISession session) : base(session) { }

		public IEnumerable<AlibabaBuyer> ByCustomer(int customerId) {
			return GetAll().Where(l => l.Customer.Id == customerId);
		}
	}


	public class AlibabaBuyerMap : ClassMap<AlibabaBuyer> {
		public AlibabaBuyerMap() {

			Table("AlibabaBuyer");

			Id(x => x.Id).GeneratedBy.Identity();

			Map(x => x.AliId);
			Map(x => x.Freeze);

			References(x => x.Customer, "CustomerId").LazyLoad().Cascade.None();

		} // constructor
	}
}
