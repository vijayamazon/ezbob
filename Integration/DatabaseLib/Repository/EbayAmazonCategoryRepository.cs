namespace EZBob.DatabaseLib.Repository {
	using System.Collections.Generic;
	using System.Linq;
	using ApplicationMng.Repository;
	using EZBob.DatabaseLib.Model.Marketplaces.Amazon;
	using Model.Database;
	using NHibernate;
	using NHibernate.Linq;

	public class EbayAmazonCategoryRepository : NHibernateRepositoryBase<MP_EbayAmazonCategory> {
		public EbayAmazonCategoryRepository(ISession session) : base(session) { }

		public MP_EbayAmazonCategory FindItem(string catId) {
			return GetAll().FirstOrDefault(i => i.CategoryId == catId);
		} // FindItem

		public List<string> GetAmazonCategories(MP_CustomerMarketPlace marketplace) {
				return Session.Query<MP_AmazonOrderItemDetailCatgory>()
                .Where(x => x.OrderItemDetail.OrderItem.Order.CustomerMarketPlace.Id == marketplace.Id)
                .Select(x => x.Category.Name)
                .Distinct()
                .ToList();
		}
	} // class EbayAmazonCategoryRepository
} // namespace
