namespace EZBob.DatabaseLib.Model.CustomerRelations {
	using System;
	using System.Linq;
	using ApplicationMng.Repository;
	using NHibernate;

	#region class CustomerRelations

	public class CustomerRelations {
		public virtual int Id { get; set; }

		public virtual int CustomerId { get; set; }
		public virtual string UserName { get; set; }
		public virtual string Type { get; set; }
		public virtual CRMActions Action { get; set; }
		public virtual CRMStatuses Status { get; set; }
		public virtual CRMRanks Rank { get; set; }
		public virtual string Comment { get; set; }
		public virtual DateTime Timestamp { get; set; }
		public virtual bool? IsBroker { get; set; }
		public virtual string PhoneNumber { get; set; }
	} // class CustomerRelations

	#endregion class CustomerRelations

	#region class CustomerRelationsRepository

	public class CustomerRelationsRepository : NHibernateRepositoryBase<CustomerRelations> {
		public CustomerRelationsRepository(ISession session) : base(session) {} // constructor

		public virtual IQueryable<CustomerRelations> ByCustomer(int customerId) {
			return GetAll().Where(r => r.CustomerId == customerId);
		} // ByCustomer

		public CustomerRelations GetLastCrm(int customerId) {
			return GetAll()
				.Where(x => x.CustomerId == customerId)
				.OrderByDescending(x => x.Timestamp)
				.FirstOrDefault();
		} // GetLastCrm
	} // class CustomerRelationsRepository

	#endregion class CustomerRelationsRepository
} // namespace
