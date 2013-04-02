using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_TeraPeakOrderMap : ClassMap<MP_TeraPeakOrder>
	{
		public MP_TeraPeakOrderMap()
		{
			Table( "MP_TeraPeakOrder" );
			Id( x => x.Id );
			Map( x => x.Created ).CustomType<UtcDateTimeType>().Not.Nullable();
			Map( x => x.LastOrderItemEndDate ).CustomType<UtcDateTimeType>();
			References( x => x.CustomerMarketPlace, "CustomerMarketPlaceId" );
			HasMany( x => x.OrderItems ).KeyColumn( "TeraPeakOrderId" ).Cascade.All();
			References(x => x.HistoryRecord)
				.Column("CustomerMarketPlaceUpdatingHistoryRecordId")
				.Unique()
				.Cascade.None();
		}
	}
}