using System.Collections.Generic;
using System.Linq;
using ApplicationMng.Repository;
using NHibernate;
using System.Linq;
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

        public  List<MP_EbayAmazonCategory> CategoryForMarketplace(MP_CustomerMarketPlace marketplace)
        {
            return marketplace.Marketplace.Name == "Amazon" ? GetAmazonCategories(marketplace) : GetEbayCategories(marketplace);
        }

	    public List<MP_EbayAmazonCategory> GetEbayCategories(MP_CustomerMarketPlace marketplace)
	    {
            return _session.Query<MP_EbayTransaction>()
                .Where(x => x.OrderItem.Order.CustomerMarketPlace.Id == marketplace.Id)
                .Where(x => x.OrderItemDetail.PrimaryCategory != null)
                .Select(x => x.OrderItemDetail.PrimaryCategory)
                .Distinct().ToList();
	    }

	    public List<MP_EbayAmazonCategory> GetAmazonCategories(MP_CustomerMarketPlace marketplace)
	    {
            return _session.Query<MP_AmazonOrderItemDetailCatgory>()
                .Where(x => x.OrderItemDetail.OrderItem.Order.CustomerMarketPlace.Id == marketplace.Id)
                .Select(x => x.Category)
                .Distinct().ToList();
	    }

	    public string GetTopCategories(MP_CustomerMarketPlace marketplace)
        {
            var categories = CategoryForMarketplace(marketplace);

            if (!categories.Any()) return "";

            string topCategories;
            if (marketplace.Marketplace.Name == "Amazon")
            {
                var min = categories.Min(y => y.Parent.Id);
                topCategories = categories.Where(x => x.Parent.Id == min).Select(x => x.Name).FirstOrDefault();
            }
            else
            {
                topCategories = categories.Select(x => x.Name.Split(':')[0]).FirstOrDefault();
            }
            return topCategories;
        }
	}
}