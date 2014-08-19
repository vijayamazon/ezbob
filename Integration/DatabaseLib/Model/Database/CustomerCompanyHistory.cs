namespace EZBob.DatabaseLib.Model.Database {
	using System;
	using ApplicationMng.Repository;
	using FluentNHibernate.Mapping;
	using NHibernate;
	using NHibernate.Type;

	public class CustomerCompanyHistory {
		public virtual int Id { get; set; }
		public virtual DateTime InsertDate { get; set; }
		public virtual int CustomerId { get; set; }
		public virtual int CompanyId { get; set; }
	}

	public class CustomerCompanyHistoryMap : ClassMap<CustomerCompanyHistory> {
		public CustomerCompanyHistoryMap() {
			Table("CustomerCompanyHistory");
			
			Id(x => x.Id);
			Map(x => x.CustomerId);
			Map(x => x.CompanyId);
			Map(x => x.InsertDate).CustomType<UtcDateTimeType>();
		}
	} // class CompanyMap

	public interface ICustomerCompanyHistoryRepository : IRepository<CustomerCompanyHistory> {
		
	}
	public class CustomerCompanyHistoryRepository : NHibernateRepositoryBase<CustomerCompanyHistory>, ICustomerCompanyHistoryRepository {
		public CustomerCompanyHistoryRepository(ISession session)
			: base(session) {
		}
	}

} // namespace
