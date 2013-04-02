using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_EbayAmazonInventoryItemMap : ClassMap<MP_EbayAmazonInventoryItem>
	{
		public MP_EbayAmazonInventoryItemMap()
		{
			Table( "MP_EbayAmazonInventoryItem" );
            Id(x => x.Id).GeneratedBy.HiLo("hibernate_unique_key", "InventoryItemIdSeed", "1000");
			Map( x => x.ItemId );
			Map( x => x.Quantity );
			Map( x => x.Sku );
			Map( x => x.BidCount );
			References( x => x.Inventory, "InventoryId" );

			Component( x => x.Amount, m =>
			{
				m.Map( x => x.CurrencyCode, "Currency" ).Length( 50 );
				m.Map( x => x.Value, "Price" );
			} );
		}
	}
}