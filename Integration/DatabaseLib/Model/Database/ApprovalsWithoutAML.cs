namespace EZBob.DatabaseLib.Model.Database
{
	using System;
	using System.Linq;
	using ApplicationMng.Repository;
	using NHibernate;

	public class ApprovalsWithoutAML
	{
		public virtual int Id { get; set; }
		public virtual int CustomerId { get; set; }
		public virtual string Username { get; set; }
		public virtual DateTime Timestamp { get; set; }
		public virtual bool DoNotShowAgain { get; set; }
	}

	public interface IApprovalsWithoutAMLRepository : IRepository<ApprovalsWithoutAML>
	{
		bool ShouldSkipById(int customerId);
	}

	public class ApprovalsWithoutAMLRepository : NHibernateRepositoryBase<ApprovalsWithoutAML>, IApprovalsWithoutAMLRepository
	{
		public ApprovalsWithoutAMLRepository(ISession session)
			: base(session)
		{
		}

		public bool ShouldSkipById(int customerId)
		{
			var latestEntry = GetAll().Where(a => a.CustomerId == customerId).OrderByDescending(a => a.Timestamp).FirstOrDefault();
			if (latestEntry == null)
			{
				return false;
			}

			return latestEntry.DoNotShowAgain;
		}
	}
}