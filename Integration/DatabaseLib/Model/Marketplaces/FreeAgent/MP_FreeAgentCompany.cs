namespace EZBob.DatabaseLib.Model.Marketplaces.FreeAgent
{
	using System;
	using ApplicationMng.Repository;
	using NHibernate;

	public class MP_FreeAgentCompany
	{
		public virtual int Id { get; set; }

		public virtual MP_FreeAgentOrder Order { get; set; }

		public virtual string url { get; set; }
		public virtual string name { get; set; }
		public virtual string subdomain { get; set; }
		public virtual string type { get; set; }
		public virtual string currency { get; set; }
		public virtual string mileage_units { get; set; }
		public virtual DateTime company_start_date { get; set; }
		public virtual DateTime freeagent_start_date { get; set; }
		public virtual DateTime first_accounting_year_end { get; set; }
		public virtual int company_registration_number { get; set; }
		public virtual string sales_tax_registration_status { get; set; }
		public virtual int sales_tax_registration_number { get; set; }
	}

	public interface IMP_FreeAgentCompanyRepository : IRepository<MP_FreeAgentCompany>
	{
	}

	public class MP_FreeAgentCompanyRepository : NHibernateRepositoryBase<MP_FreeAgentCompany>, IMP_FreeAgentCompanyRepository
	{
		public MP_FreeAgentCompanyRepository(ISession session)
			: base(session)
		{
		}
	}
}