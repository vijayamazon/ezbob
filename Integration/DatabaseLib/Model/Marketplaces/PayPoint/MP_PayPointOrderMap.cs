using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_PayPointOrderMap : ClassMap<MP_PayPointOrder>
	{
		public MP_PayPointOrderMap()
		{
			Table( "MP_PayPointOrder" );
			Id( x => x.Id );
			Map( x => x.Created ).CustomType<UtcDateTimeType>().Not.Nullable();
			References( x => x.CustomerMarketPlace, "CustomerMarketPlaceId" );
			HasMany( x => x.OrderItems ).KeyColumn( "OrderId" ).Cascade.All();
			References( x => x.HistoryRecord )
				.Column( "CustomerMarketPlaceUpdatingHistoryRecordId" )
				.Unique()
				.Cascade.None();
		}
	}
}
