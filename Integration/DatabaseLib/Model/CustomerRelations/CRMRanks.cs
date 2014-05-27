namespace EZBob.DatabaseLib.Model.CustomerRelations
{
	using ApplicationMng.Repository;
	using FluentNHibernate.Mapping;
	using NHibernate;

	public class CRMRanks
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
	}

	public class CRMRanksMap : ClassMap<CRMRanks>
	{
		public CRMRanksMap()
		{
			Table("CRMRanks");
			Id(x => x.Id);
			Map(x => x.Name).Length(100);
		}
	}

	public interface ICRMRanksRepository : IRepository<CRMRanks>
	{
	}
	
	public class CRMRanksRepository : NHibernateRepositoryBase<CRMRanks>, ICRMRanksRepository
	{
		public CRMRanksRepository(ISession session)
			: base(session)
		{
		}
	}
}