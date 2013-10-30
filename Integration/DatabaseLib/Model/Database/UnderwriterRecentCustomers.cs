namespace EZBob.DatabaseLib.Model.Database
{
	using System.Linq;
	using ApplicationMng.Repository;
	using NHibernate;

	public class UnderwriterRecentCustomers
	{
		public virtual int Id { get; set; }

		public virtual string UserName { get; set; }
		public virtual int CustomerId { get; set; }
	}

	public interface IUnderwriterRecentCustomersRepository : IRepository<UnderwriterRecentCustomers>
	{
		void Add(int customerId, string userName);
	}

	public class UnderwriterRecentCustomersRepository : NHibernateRepositoryBase<UnderwriterRecentCustomers>, IUnderwriterRecentCustomersRepository
	{
		public UnderwriterRecentCustomersRepository(ISession session)
			: base(session)
		{
		}

		public void Add(int customerId, string userName)
		{
			UnderwriterRecentCustomers underwriterRecentCustomers = GetAll().FirstOrDefault(r => r.CustomerId == customerId && r.UserName == userName);
			if (underwriterRecentCustomers != null)
			{
				Delete(underwriterRecentCustomers);
			}

			var currentRecent = GetAll().Where(r => r.UserName == userName).OrderBy(r => r.Id);
			if (currentRecent.Count() == 5) // TODO: move to config
			{
				int counter = 0;
				foreach (var customer in currentRecent)
				{
					counter++;
					if (counter == 5)
					{
						Delete(customer);
					}
				}
			}

			var newUnderwriterRecentCustomers = new UnderwriterRecentCustomers {CustomerId = customerId, UserName = userName};
			SaveOrUpdate(newUnderwriterRecentCustomers);
		}
	}
}