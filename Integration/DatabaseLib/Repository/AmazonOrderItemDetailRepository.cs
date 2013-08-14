namespace EZBob.DatabaseLib.Model.Database.Repository
{
	using System.Linq;
	using ApplicationMng.Repository;
	using NHibernate;

	using Marketplaces.Amazon;

	public class AmazonOrderItemDetailRepository : NHibernateRepositoryBase<MP_AmazonOrderItemDetail>
	{
		public AmazonOrderItemDetailRepository(ISession session)
			: base(session)
		{
		}

		public MP_EbayAmazonCategory[] FindCategoriesByAsin( string asin )
		{
			MP_AmazonOrderItemDetail product = GetAll().FirstOrDefault( p => p.ASIN == asin );
			if ( product == null )
			{
				return null;
			}

			return product.OrderItemCategories.Select( oi => oi.Category).ToArray();
		}

		public MP_EbayAmazonCategory[] FindCategoriesBySellectSku( string sellerSKU )
		{
			MP_AmazonOrderItemDetail product = GetAll().FirstOrDefault( p => p.SellerSKU == sellerSKU );
			if ( product == null )
			{
				return null;
			}

			return product.OrderItemCategories.Select( oi => oi.Category ).ToArray();
		}
	}
}