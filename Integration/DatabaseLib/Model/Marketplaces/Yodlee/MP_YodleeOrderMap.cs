using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Marketplaces.Yodlee
{
    public class MP_YodleeOrderMap : ClassMap<MP_YodleeOrder>
    {
        public MP_YodleeOrderMap()
        {
            Table("MP_YodleeOrder");
            Id(x => x.Id);
            Map(x => x.Created).CustomType<UtcDateTimeType>().Not.Nullable();
            References(x => x.CustomerMarketPlace, "CustomerMarketPlaceId");
			HasMany(x => x.OrderItems).KeyColumn("OrderId").LazyLoad().Cascade.All();
            References(x => x.HistoryRecord)
                .Column("CustomerMarketPlaceUpdatingHistoryRecordId")
                .Unique()
                .Cascade.None();
        }
    }
}