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

		public MP_FreeAgentExpenseCategory GetSimilarCategory(FreeAgentExpenseCategory category) {
			return Session
				.QueryOver<MP_FreeAgentExpenseCategory>()
				.Where(c =>
					c.allowable_for_tax == category.allowable_for_tax &&
					c.auto_sales_tax_rate == category.auto_sales_tax_rate &&
					c.category_group == category.category_group &&
					c.description == category.description &&
					c.nominal_code == category.nominal_code &&
					c.tax_reporting_name == category.tax_reporting_name &&
					c.url == category.url
				)
				.SingleOrDefault();
		} // GetSimilarCategory
	} // class MP_FreeAgentExpenseCategoryRepository
} // namespace
