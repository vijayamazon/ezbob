namespace EZBob.DatabaseLib.Model.Database {
	using System.Linq;
	using ApplicationMng.Repository;
	using ConfigManager;
	using NHibernate;

	public class UnderwriterRecentCustomers {
		public virtual int Id { get; set; }
		public virtual string UserName { get; set; }
		public virtual int CustomerId { get; set; }
	} // class UnderwriterRecentCustomers

	public interface IUnderwriterRecentCustomersRepository : IRepository<UnderwriterRecentCustomers> {
		void Add(int customerId, string userName);
	} // interface IUnderwriterRecentCustomersRepository

	public class UnderwriterRecentCustomersRepository
		: NHibernateRepositoryBase<UnderwriterRecentCustomers>, IUnderwriterRecentCustomersRepository
	{
		public UnderwriterRecentCustomersRepository(ISession session) : base(session) {
			this.numOfCustomersToKeep = CurrentValues.Instance.RecentCustomersToKeep;
		}

		public void Add(int customerId, string userName) {
			UnderwriterRecentCustomers underwriterRecentCustomers = GetAll()
				.FirstOrDefault(r => r.CustomerId == customerId && r.UserName == userName);

			if (underwriterRecentCustomers != null)
				Delete(underwriterRecentCustomers);

			var currentRecent = GetAll().Where(r => r.UserName == userName).OrderBy(r => r.Id);

			while (currentRecent.Count() >= this.numOfCustomersToKeep)
				Delete(currentRecent.First());

			var newUnderwriterRecentCustomers = new UnderwriterRecentCustomers {
				CustomerId = customerId,
				UserName = userName,
			};

			SaveOrUpdate(newUnderwriterRecentCustomers);
		} // Add

		private readonly int numOfCustomersToKeep;
	} // class UnderwriterRecentCustomersRepository
} // namespace