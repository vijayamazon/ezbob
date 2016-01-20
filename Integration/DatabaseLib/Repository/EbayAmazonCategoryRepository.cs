namespace EZBob.DatabaseLib.Repository {
	using System.Linq;
	using ApplicationMng.Repository;
	using Model.Database;
	using NHibernate;

	public class EbayAmazonCategoryRepository : NHibernateRepositoryBase<MP_EbayAmazonCategory> {
		public EbayAmazonCategoryRepository(ISession session) : base(session) { }

		public MP_EbayAmazonCategory FindItem(string catId) {
			return GetAll().FirstOrDefault(i => i.CategoryId == catId);
		} // FindItem
	} // class EbayAmazonCategoryRepository
} // namespace
