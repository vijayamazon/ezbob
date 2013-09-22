namespace EZBob.DatabaseLib.Model.Database
{
	using System;
	using System.Linq;
	using ApplicationMng.Repository;
	using NHibernate;

	[Serializable]
	public class CustomerStatuses
    {
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual bool IsEnabled { get; set; }
		public virtual bool IsWarning { get; set; }
    }

	public interface ICustomerStatusesRepository : IRepository<CustomerStatuses>
	{
		bool GetIsEnabled(int id);
		CustomerStatuses GetByName(string name);
	}

    public class CustomerStatusesRepository : NHibernateRepositoryBase<CustomerStatuses>, ICustomerStatusesRepository
    {
		public CustomerStatusesRepository(ISession session)
			: base(session)
        {
        }

		public bool GetIsEnabled(int id)
	    {
			var customerStatus = GetAll().FirstOrDefault(s => s.Id == id);
			if (customerStatus == null)
			{
				return false;
			}

		    return customerStatus.IsEnabled;
	    }

		public CustomerStatuses GetByName(string name)
		{
			return GetAll().FirstOrDefault(s => s.Name == name);
		}
	}
}
