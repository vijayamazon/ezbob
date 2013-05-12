using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Database {
	public class MP_PlayOrderMap : ClassMap<MP_PlayOrder> {
		public MP_PlayOrderMap() {
			Table( "MP_PlayOrder" );
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
	} // class MP_PlayOrderMap
} // namespace