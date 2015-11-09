namespace EZBob.DatabaseLib.Model.Marketplaces.FreeAgent {
	using ApplicationMng.Repository;
	using DatabaseWrapper.Order;
	using NHibernate;

	public class MP_FreeAgentExpenseCategory {
		public virtual int Id { get; set; }

		public virtual string category_group { get; set; }
		public virtual string url { get; set; }
		public virtual string description { get; set; }
		public virtual string nominal_code { get; set; }
		public virtual bool? allowable_for_tax { get; set; }
		public virtual string tax_reporting_name { get; set; }
		public virtual string auto_sales_tax_rate { get; set; }
	} // class MP_FreeAgentExpenseCategory

	public interface IMP_FreeAgentExpenseCategoryRepository : IRepository<MP_FreeAgentExpenseCategory> {
	} // interface IMP_FreeAgentExpenseCategoryRepository

	public class MP_FreeAgentExpenseCategoryRepository :
		NHibernateRepositoryBase<MP_FreeAgentExpenseCategory>,
		IMP_FreeAgentExpenseCategoryRepository
	{
		public MP_FreeAgentExpenseCategoryRepository(ISession session) : base(session) {
		} // constructor

		public MP_FreeAgentExpenseCategory Find(FreeAgentExpenseCategory category) {
			return Session
				.QueryOver<MP_FreeAgentExpenseCategory>()
				.Where(c => c.url == category.url)
				.SingleOrDefault();
		} // Find
	} // class MP_FreeAgentExpenseCategoryRepository
} // namespace
