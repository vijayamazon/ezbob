namespace EZBob.DatabaseLib.Model.CustomerRelations
{
	
	using ApplicationMng.Repository;
	using FluentNHibernate.Mapping;
	using Iesi.Collections.Generic;
	using NHibernate;

	public class CRMStatusGroup
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual int Priority { get; set; }
		public virtual ISet<CRMStatuses> Statuseses { get; set; }
	}

	public class CRMStatusGroupMap : ClassMap<CRMStatusGroup>
	{
		public CRMStatusGroupMap()
		{
			Table("CRMStatusGroup");
			Id(x => x.Id);

			Map(x => x.Priority);
			Map(x => x.Name).Length(20);

			HasMany(x => x.Statuseses).KeyColumn("GroupId");
		}
	}

	public interface ICRMStatusGroupRepository : IRepository<CRMStatusGroup>
	{
	}

	public class CRMStatusGroupRepository : NHibernateRepositoryBase<CRMStatusGroup>, ICRMStatusGroupRepository
	{
		public CRMStatusGroupRepository(ISession session)
			: base(session)
		{
		}
	}
}