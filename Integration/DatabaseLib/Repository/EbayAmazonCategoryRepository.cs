using System.Collections.Generic;
using System.Linq;
using ApplicationMng.Repository;
using NHibernate;
using NHibernate.Linq;

namespace EZBob.DatabaseLib.Model.Database.Repository
{
	public class EbayAmazonCategoryRepository : NHibernateRepositoryBase<MP_EbayAmazonCategory>
	{
		public EbayAmazonCategoryRepository( ISession session )
			: base( session )
		{
		}

		public MP_EbayAmazonCategory FindItem( string catId )
		{
			return GetAll().FirstOrDefault( i => i.CategoryId == catId );
		}

        public List<string> CategoryForMarketplace(MP_CustomerMarketPlace marketplace)
        {
            return marketplace.Marketplace.Name == "Amazon" ? GetAmazonCategories(marketplace) : GetEbayCategories(marketplace);
        }

	    public List<string> GetEbayCategories(MP_CustomerMarketPlace marketplace)
	    {
	        var categories = marketplace.TeraPeakOrders.SelectMany(t => t.OrderItems)
	                   .SelectMany(t => t.CategoryStatistics)
	                   .Select(c => c.Category.FullName)
	                   .Distinct()
	                   .ToList();

            if (categories.Any())
            {
                return categories;
            }

            return _session.Query<MP_EbayTransaction>()
                .Where(x => x.OrderItem.Order.CustomerMarketPlace.Id == marketplace.Id)
                .Where(x => x.OrderItemDetail.PrimaryCategory != null)
                .Select(x => x.OrderItemDetail.PrimaryCategory.Name)
                .Distinct()
                .ToList();
	    }

	    public List<string> GetAmazonCategories(MP_CustomerMarketPlace marketplace)
	    {
            return _session.Query<MP_AmazonOrderItemDetailCatgory>()
                .Where(x => x.OrderItemDetail.OrderItem.Order.CustomerMarketPlace.Id == marketplace.Id)
                .Select(x => x.Category.Name)
                .Distinct()
                .ToList();
	    }
	}
}