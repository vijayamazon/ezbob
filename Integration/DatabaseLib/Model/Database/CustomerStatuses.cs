namespace EZBob.DatabaseLib.Model.Database
{
	using ApplicationMng.Repository;
	using NHibernate;

	public class CustomerStatuses
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
    }

	public interface ICustomerStatusesRepository : IRepository<CustomerStatuses>
    {
    }

    public class CustomerStatusesRepository : NHibernateRepositoryBase<CustomerStatuses>, ICustomerStatusesRepository
    {
		public CustomerStatusesRepository(ISession session)
			: base(session)
        {
        }
    }
}
