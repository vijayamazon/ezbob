namespace EZBob.DatabaseLib.Model.Marketplaces.Yodlee
{
	using System.Linq;
	using ApplicationMng.Repository;
	using NHibernate;

	public class YodleeBanks
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual long ContentServiceId { get; set; }
		public virtual string ParentBank { get; set; }
		public virtual bool Active { get; set; }
	}

	public interface IYodleeBanksRepository : IRepository<YodleeBanks>
	{
		YodleeBanks Search(long csId);
	}

	public class YodleeBanksRepository : NHibernateRepositoryBase<YodleeBanks>, IYodleeBanksRepository
	{
		public YodleeBanksRepository(ISession session)
			: base(session)
		{
		}

		public YodleeBanks Search(long csId)
		{
			return GetAll().FirstOrDefault(b => b.ContentServiceId == csId);
		}
	}
}