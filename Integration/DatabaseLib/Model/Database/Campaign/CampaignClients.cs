namespace EZBob.DatabaseLib.Model.Database
{
	public class CampaignClients
	{
		public virtual int Id { get; set; }
		public virtual Customer Customer { get; set; }
		public virtual Campaign Campaign { get; set; }
	}
}

namespace EZBob.DatabaseLib.Model.DataMapping
{
	using Database;
	using FluentNHibernate.Mapping;

	public class CampaignClientsMap : ClassMap<CampaignClients>
	{
		public CampaignClientsMap()
		{
			Table("CampaignClients");
			Id(x => x.Id);
			References(x => x.Campaign, "CampaignId");
			References(x => x.Customer, "CustomerId");
		}
	}
}

namespace EZBob.DatabaseLib.Model.Database.Repository
{
	using ApplicationMng.Repository;
	using Database;
	using NHibernate;

	public interface ICampaignClientsRepository : IRepository<CampaignClients>
	{

	}

	public class CampaignClientsRepository : NHibernateRepositoryBase<CampaignClients>, ICampaignClientsRepository
	{
		public CampaignClientsRepository(ISession session)
			: base(session)
		{
		}
	}
}
