namespace EZBob.DatabaseLib.Model.Database
{
	using System;
	using ApplicationMng.Repository;
	using NHibernate;

	[Serializable]
	public class CustomerStatusHistory
    {
		public virtual int Id { get; set; }
		public virtual string Username { get; set; }
		public virtual DateTime Timestamp { get; set; }
		public virtual int CustomerId { get; set; }
		public virtual int PreviousStatus { get; set; }
		public virtual int NewStatus { get; set; }
    }

	public interface ICustomerStatusHistoryRepository : IRepository<CustomerStatusHistory>
	{
	}

    public class CustomerStatusHistoryRepository : NHibernateRepositoryBase<CustomerStatusHistory>, ICustomerStatusHistoryRepository
    {
		public CustomerStatusHistoryRepository(ISession session)
			: base(session)
        {
        }
	}
}
