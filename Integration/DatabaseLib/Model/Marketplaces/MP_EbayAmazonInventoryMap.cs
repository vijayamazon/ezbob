using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_EbayAmazonInventoryMap: ClassMap<MP_EbayAmazonInventory>
	{
		public MP_EbayAmazonInventoryMap()
		{
			Table( "MP_EbayAmazonInventory" );
			Id(x => x.Id);
			Map( x => x.Created ).CustomType<UtcDateTimeType>().Not.Nullable();			
			References( x => x.CustomerMarketPlace, "CustomerMarketPlaceId" );
			HasMany( x => x.InventoryItems ).KeyColumn( "InventoryId" ).Cascade.All();
			Map( x => x.AmazonUseAFN );

			References( x => x.HistoryRecord )
				.Column( "CustomerMarketPlaceUpdatingHistoryRecordId" )
				.Unique()
				.Cascade.None();
		}
	}
}