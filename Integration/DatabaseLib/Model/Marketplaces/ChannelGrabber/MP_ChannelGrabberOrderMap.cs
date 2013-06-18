using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Database {
	public class MP_ChannelGrabberOrderMap : ClassMap<MP_ChannelGrabberOrder> {
		public MP_ChannelGrabberOrderMap() {
			Table( "MP_ChannelGrabberOrder" );
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
	} // class MP_ChannelGrabberOrderMap
} // namespace