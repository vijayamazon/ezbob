using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_EbayOrderMap : ClassMap<MP_EbayOrder>
	{
		public MP_EbayOrderMap()
		{
			Table( "MP_EbayOrder" );
			Id( x => x.Id );
			Map( x => x.Created ).CustomType<UtcDateTimeType>().Not.Nullable();
			References( x => x.CustomerMarketPlace, "CustomerMarketPlaceId" );
			HasMany( x => x.OrderItems ).KeyColumn( "OrderId" ).Cascade.All();
			References( x => x.HistoryRecord )
				.Column( "CustomerMarketPlaceUpdatingHistoryRecordId" )
				.Unique()
				.LazyLoad()
				.Cascade.None();
		}
	}
}