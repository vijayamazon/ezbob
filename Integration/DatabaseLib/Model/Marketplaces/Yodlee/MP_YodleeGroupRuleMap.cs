namespace EZBob.DatabaseLib.Model.Marketplaces.Yodlee
{
	using FluentNHibernate.Mapping;

	public class MP_YodleeGroupRuleMap
	{
		public virtual int Id { get; set; }
		public virtual MP_YodleeGroup Group { get; set; }
		public virtual MP_YodleeRule Rule { get; set; }
		public virtual string Literal { get; set; }
		public virtual bool IsRegex { get; set; }
	}

	public class MP_YodleeGroupRuleMapMap : ClassMap<MP_YodleeGroupRuleMap>
	{
		public MP_YodleeGroupRuleMapMap()
		{
			Table("MP_YodleeGroupRuleMap");
			Id(x => x.Id);
			References(x => x.Group, "GroupId");
			References(x => x.Rule, "RuleId");
			Map(x => x.Literal).Length(100);
			Map(x => x.IsRegex).Default("0");
		}
	}
}

namespace EZBob.DatabaseLib.Model.Database.Repository
{
	using System.Linq;
	using ApplicationMng.Repository;
	using Marketplaces.Yodlee;
	using NHibernate;

	public class YodleeGroupRuleMapRepository : NHibernateRepositoryBase<MP_YodleeGroupRuleMap>
	{
		public YodleeGroupRuleMapRepository(ISession session)
			: base(session)
		{
		}

	}
}