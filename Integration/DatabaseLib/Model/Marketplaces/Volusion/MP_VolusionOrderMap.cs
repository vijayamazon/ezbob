using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Database {
	public class MP_VolusionOrderMap : ClassMap<MP_VolusionOrder> {
		public MP_VolusionOrderMap() {
			Table( "MP_VolusionOrder" );
			Id(x => x.Id);
			References(x => x.CustomerMarketPlace, "CustomerMarketPlaceId");
			Map(x => x.Created).CustomType<UtcDateTimeType>().Not.Nullable();
			HasMany(x => x.OrderItems).KeyColumn( "OrderId" ).Cascade.All();
			References(x => x.HistoryRecord)
				.Column("CustomerMarketPlaceUpdatingHistoryRecordId")
				.Unique()
				.Cascade
				.None();
		} // constructor
	} // class MP_VolusionOrderMap
} // namespace