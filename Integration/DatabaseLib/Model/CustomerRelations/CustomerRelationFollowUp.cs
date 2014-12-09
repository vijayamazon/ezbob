namespace EZBob.DatabaseLib.Model.CustomerRelations {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ApplicationMng.Repository;
	using FluentNHibernate.Mapping;
	using NHibernate;
	using NHibernate.Type;

	public class CustomerRelationFollowUp {
		public virtual int Id { get; set; }
		public virtual int CustomerId { get; set; }
		public virtual DateTime DateAdded { get; set; }
		public virtual DateTime FollowUpDate { get; set; }
		public virtual string Comment { get; set; }
		public virtual bool IsClosed { get; set; }
		public virtual DateTime? CloseDate { get; set; }
		public virtual bool? IsBroker { get; set; }
	} // class CustomerRelationFollowUp

	public class CustomerRelationFollowUpMap : ClassMap<CustomerRelationFollowUp> {
		public CustomerRelationFollowUpMap() {
			Table("CustomerRelationFollowUp");
			Id(x => x.Id);
			Map(x => x.DateAdded).CustomType<UtcDateTimeType>();
			Map(x => x.FollowUpDate).CustomType<UtcDateTimeType>();
			Map(x => x.Comment).Length(1000);
			Map(x => x.CustomerId);
			Map(x => x.IsClosed);
			Map(x => x.CloseDate).CustomType<UtcDateTimeType>();
			Map(x => x.IsBroker);
		} // constructor
	} // class CustomerRelationFollowUpMap

	public interface ICustomerRelationFollowUpRepository : IRepository<CustomerRelationFollowUp> {
		IEnumerable<CustomerRelationFollowUp> GetByCustomer(int customerId);
	} // interface ICustomerRelationFollowUpRepository

	public class CustomerRelationFollowUpRepository : NHibernateRepositoryBase<CustomerRelationFollowUp>, ICustomerRelationFollowUpRepository {
		public CustomerRelationFollowUpRepository(ISession session) : base(session) {} // constructor

		public CustomerRelationFollowUp GetLastFollowUp(int customerId) {
			return GetAll().Where(x => x.CustomerId == customerId).OrderByDescending(x => x.DateAdded).FirstOrDefault(x => x.IsClosed == false);
		} // GetLastFollowUp

		public IEnumerable<CustomerRelationFollowUp> GetByCustomer(int customerId) {
			return GetAll().Where(x => x.CustomerId == customerId);
		} // GetByCustomer
	} // class CustomerRelationFollowUpRepository

} // namespace
