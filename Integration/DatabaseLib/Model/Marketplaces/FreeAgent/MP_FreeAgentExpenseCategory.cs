namespace EZBob.DatabaseLib.Model.Marketplaces.FreeAgent
{
	using ApplicationMng.Repository;
	using NHibernate;

	public class MP_FreeAgentExpenseCategory
	{
		public virtual int Id { get; set; }

		public virtual string url { get; set; }
		public virtual string description { get; set; }
		public virtual string nominal_code { get; set; }
		public virtual bool? allowable_for_tax { get; set; }
		public virtual string tax_reporting_name { get; set; }
		public virtual string auto_sales_tax_rate { get; set; }
	}

	public interface IMP_FreeAgentExpenseCategoryRepository : IRepository<MP_FreeAgentExpenseCategory>
	{
	}

	public class MP_FreeAgentExpenseCategoryRepository : NHibernateRepositoryBase<MP_FreeAgentExpenseCategory>, IMP_FreeAgentExpenseCategoryRepository
	{
		public MP_FreeAgentExpenseCategoryRepository(ISession session)
			: base(session)
		{
		}
	}
}