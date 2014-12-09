namespace EZBob.DatabaseLib.Model.Database
{
	public class CampaignType
	{
		public virtual int Id { get; set; }
		public virtual string Type { get; set; }
		public virtual string Description { get; set; }
	}
}

namespace EZBob.DatabaseLib.Model.DataMapping
{
	using Database;
	using FluentNHibernate.Mapping;

	public class CampaignTypeMap : ClassMap<CampaignType>
	{
		public CampaignTypeMap()
		{
			Table("CampaignType");
			Id(x => x.Id);
			Map(x => x.Type).Length(300);
			Map(x => x.Description).Length(300);
		}
	}
}

namespace EZBob.DatabaseLib.Model.Database.Repository
{
	using ApplicationMng.Repository;
	using Database;
	using NHibernate;

	public interface ICampaignTypeRepository : IRepository<CampaignType>
	{

	}

	public class CampaignTypeRepository : NHibernateRepositoryBase<CampaignType>, ICampaignTypeRepository
	{
		public CampaignTypeRepository(ISession session)
			: base(session)
		{
		}
	}
}
