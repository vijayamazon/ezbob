namespace EZBob.DatabaseLib.Model.Marketplaces.Yodlee
{
	using FluentNHibernate.Mapping;
	public enum YodleeRule
	{
		IncludeLiteralWord = 1,
		IncludeDirector = 2,
		TransactionRoundFigure = 3,
		NotCategorized = 4,
		DontIncludeLiteralWord = 5,
		DontIncludeDirector = 6
	}

	public class MP_YodleeRule
	{
		public virtual int Id { get; set; }
		public virtual string Rule { get; set; }
	}

	public class MP_YodleeRuleMap : ClassMap<MP_YodleeRule>
	{
		public MP_YodleeRuleMap()
		{
			Table("MP_YodleeRule");
			Id(x => x.Id);
			Map(x => x.Rule, "RuleDescription").Length(100);
		}
	}
}

namespace EZBob.DatabaseLib.Model.Database.Repository
{
	using System.Linq;
	using ApplicationMng.Repository;
	using Marketplaces.Yodlee;
	using NHibernate;

	public class YodleeRuleRepository : NHibernateRepositoryBase<MP_YodleeRule>
	{
		public YodleeRuleRepository(ISession session)
			: base(session)
		{
		}

	}
}