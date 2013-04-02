using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_EbayAmazonCategoryMap : ClassMap<MP_EbayAmazonCategory>
	{
		public MP_EbayAmazonCategoryMap()
		{
			Table( "MP_EbayAmazonCategory" );
			Id( x => x.Id );
			Map( x => x.CategoryId, "ServiceCategoryId" );
			Map( x => x.Name );
			Map( x => x.IsVirtual );
			References( x => x.Parent, "ParentId" );
			References( x => x.Marketplace, "MarketplaceTypeId" );
		}
	}
}