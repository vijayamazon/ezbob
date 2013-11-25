namespace EZBob.DatabaseLib.Model.Database
{
	using System;
	using Iesi.Collections.Generic;

	public class Campaign
	{
		public virtual int Id { get; set; }
		public virtual CampaignType CampaignType { get; set; }
		public virtual string Name { get; set; }
		public virtual DateTime StartDate { get; set; }
		public virtual DateTime EndDate { get; set; }
		public virtual string Description { get; set; }

		private ISet<CampaignClients> _clients = new HashedSet<CampaignClients>();
		public virtual ISet<CampaignClients> Clients
		{
			get { return _clients; }
			set { _clients = value; }
		}
	}
}


namespace EZBob.DatabaseLib.Model.DataMapping
{
	using Database;
	using FluentNHibernate.Mapping;
	using NHibernate.Type;

	public class CampaignMap : ClassMap<Campaign>
	{
		public CampaignMap()
		{
			Table("Campaign");
			Id(x => x.Id);
			References(x => x.CampaignType, "TypeId");
			Map(x => x.Name).Length(300);
			Map(x => x.StartDate).CustomType<UtcDateTimeType>();
			Map(x => x.EndDate).CustomType<UtcDateTimeType>();
			Map(x => x.Description).Length(300);
			HasMany(x => x.Clients)
				.AsSet()
				.KeyColumn("CampaignId")
				.Cascade.All()
				.Inverse();
		}
	}
}

namespace EZBob.DatabaseLib.Model.Database.Repository
{
	using ApplicationMng.Repository;
	using Database;
	using NHibernate;

	public interface ICampaignRepository : IRepository<Campaign>
	{
		
	}

	public class CampaignRepository : NHibernateRepositoryBase<Campaign>, ICampaignRepository
	{
		public CampaignRepository(ISession session)
			: base(session)
		{
		}
	}
}
