namespace EZBob.DatabaseLib.Model.Marketplaces.Yodlee
{
	using FluentNHibernate.Mapping;
	public enum YodleeGroup
	{
		RevenuesGroupOnline = 1,
		RevenuesTransfers = 2,
		Opex = 3,
		SalariesAndTaxSalaries = 4,
		SalariesAndTaxTaxes = 5,
		SalariesAndTaxDirectorsWithdrawal = 6,
		CorporateTax = 7,
		LoanRepayments = 8,
		Exception = 9,
	}

	public class MP_YodleeGroup
	{
		public virtual int Id { get; set; }
		public virtual string Group { get; set; }
		public virtual string SubGroup { get; set; }
	}

	public class MP_YodleeGroupMap : ClassMap<MP_YodleeGroup>
	{
		public MP_YodleeGroupMap()
		{
			Table("MP_YodleeGroup");
			Id(x => x.Id);
			Map(x => x.Group, "MainGroup").Length(100);
			Map(x => x.SubGroup).Length(100);
		}
	}
}

namespace EZBob.DatabaseLib.Model.Database.Repository
{
	using System.Linq;
	using ApplicationMng.Repository;
	using Marketplaces.Yodlee;
	using NHibernate;

	public class YodleeGroupRepository : NHibernateRepositoryBase<MP_YodleeGroup>
	{
		public YodleeGroupRepository(ISession session)
			: base(session)
		{
		}

	}
}