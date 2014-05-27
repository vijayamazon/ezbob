namespace EZBob.DatabaseLib.Model.CustomerRelations
{
	using System;
	using System.Linq;
	using ApplicationMng.Repository;
	using NHibernate;

	public class CustomerRelations
	{
		public virtual int Id { get; set; }

		public virtual int CustomerId { get; set; }
		public virtual string UserName { get; set; }
		public virtual bool Incoming { get; set; }
		public virtual CRMActions Action { get; set; }
		public virtual CRMStatuses Status { get; set; }
		public virtual CRMRanks Rank { get; set; }
		public virtual string Comment { get; set; }
		public virtual DateTime Timestamp { get; set; }
	}

	public interface ICustomerRelationsRepository : IRepository<CustomerRelations>
	{
	}

	public class CustomerRelationsRepository : NHibernateRepositoryBase<CustomerRelations>, ICustomerRelationsRepository
	{
		public CustomerRelationsRepository(ISession session)
			: base(session)
		{
		}

		public virtual IQueryable<CustomerRelations> ByCustomer(int customerId)
		{
			return GetAll().Where(r => r.CustomerId == customerId);
		}
	}
}