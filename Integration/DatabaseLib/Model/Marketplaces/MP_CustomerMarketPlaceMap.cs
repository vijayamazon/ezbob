using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Database
{
    public class MP_CustomerMarketPlaceMap : ClassMap<MP_CustomerMarketPlace>
    {
        public MP_CustomerMarketPlaceMap()
        {
            Table("MP_CustomerMarketPlace");
            Id(x => x.Id);
            Map(x => x.EliminationPassed);
            Map(x => x.SecurityData).Not.Nullable();
            Map(x => x.Created).CustomType<UtcDateTimeType>();
            Map(x => x.Updated).CustomType<UtcDateTimeType>();
            Map(x => x.UpdatingStart).CustomType<UtcDateTimeType>();
            Map(x => x.UpdatingEnd).CustomType<UtcDateTimeType>();
            Map(x => x.UpdateError).CustomType("StringClob").LazyLoad();
            References(x => x.Marketplace, "MarketPlaceId");
            References(x => x.Customer, "CustomerId");
            HasOne(x => x.PersonalInfo)
                .PropertyRef(p => p.CustomerMarketPlace)
                .Cascade.All()
                .Fetch.Join();
            Map(x => x.DisplayName).Length(512);
            HasMany(x => x.PayPalTransactions).
                KeyColumn("CustomerMarketPlaceId")
                .Cascade.All();

            HasMany(x => x.AmazonOrders).
                KeyColumn("CustomerMarketPlaceId")
                .Cascade.All();

            HasMany(x => x.EbayOrders).
                KeyColumn("CustomerMarketPlaceId")
                .Cascade.All();

            HasMany(x => x.Inventory).
                KeyColumn("CustomerMarketPlaceId")
                .Cascade.All();

            HasMany(x => x.UpdatingHistory).
                KeyColumn("CustomerMarketPlaceId")
                .Cascade.All();

            HasMany(x => x.EbayUserData).
                KeyColumn("CustomerMarketPlaceId")
                .Cascade.All();

            HasMany(x => x.EbayUserAccountData).
                KeyColumn("CustomerMarketPlaceId")
                .Cascade.All();

            HasMany(x => x.EbayFeedback).
                KeyColumn("CustomerMarketPlaceId")
                .Cascade.All();

            HasMany(x => x.AmazonFeedback).
                KeyColumn("CustomerMarketPlaceId")
                .Cascade.All();

            HasMany(x => x.TeraPeakOrders).
                KeyColumn("CustomerMarketPlaceId")
                .Cascade.All();

            HasMany(x => x.AnalysysFunctionValues).
                KeyColumn("CustomerMarketPlaceId")
				.Cascade.All();

			HasMany(x => x.EkmOrders).
				KeyColumn("CustomerMarketPlaceId")
				.Cascade.All();

			HasMany(x => x.FreeAgentRequests).
				KeyColumn("CustomerMarketPlaceId")
				.Cascade.All();

            HasMany(x => x.PayPointOrders).
                KeyColumn("CustomerMarketPlaceId")
                .Cascade.All();

            HasMany(x => x.VolusionOrders).
                KeyColumn("CustomerMarketPlaceId")
                .Cascade.All();

            HasMany(x => x.PlayOrders).
                KeyColumn("CustomerMarketPlaceId")
                .Cascade.All();

            HasMany(x => x.YodleeOrders).
                KeyColumn("CustomerMarketPlaceId")
                .Cascade.All();
        }
    }
}
