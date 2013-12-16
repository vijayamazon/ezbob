namespace EZBob.DatabaseLib.Model.Marketplaces.Yodlee
{
	using System;
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
		LoanRepaymentsReceipt = 8,
		LoanRepaymentsRepayment = 9,
		Exception = 10,
		VAT = 11
	}

	[Serializable]
	public class MP_YodleeGroup
	{
		public virtual int Id { get; set; }
		public virtual string Group { get; set; }
		public virtual string SubGroup { get; set; }
		public virtual string BaseType { get; set; }
	}

	public class MP_YodleeGroupMap : ClassMap<MP_YodleeGroup>
	{
		public MP_YodleeGroupMap()
		{
			Table("MP_YodleeGroup");
			ReadOnly();
			Id(x => x.Id);
			Map(x => x.Group, "MainGroup").Length(100);
			Map(x => x.SubGroup).Length(100);
			Map(x => x.BaseType).Length(100);
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