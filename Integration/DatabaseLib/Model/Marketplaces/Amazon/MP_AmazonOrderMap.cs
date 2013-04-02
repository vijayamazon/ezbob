using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_AmazonOrderMap : ClassMap<MP_AmazonOrder>
	{
		public MP_AmazonOrderMap()
		{
			Table( "MP_AmazonOrder" );
			Id( x => x.Id );
			Map( x => x.Created ).CustomType<UtcDateTimeType>().Not.Nullable();
			References( x => x.CustomerMarketPlace, "CustomerMarketPlaceId" );
			HasMany( x => x.OrderItems )
				.KeyColumn( "AmazonOrderId" )
				.Cascade.All();

			HasMany( x => x.OrderItems2 )
				.KeyColumn( "AmazonOrderId" )
				.Cascade.All();

			References( x => x.HistoryRecord )
				.Column( "CustomerMarketPlaceUpdatingHistoryRecordId" )
				.Unique()
				.Cascade.None();
		}
	}
}