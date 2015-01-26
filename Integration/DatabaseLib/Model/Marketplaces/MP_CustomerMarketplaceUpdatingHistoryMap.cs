namespace EZBob.DatabaseLib.Model.Database {
	using FluentNHibernate.Mapping;
	using NHibernate.Type;

	public class MP_CustomerMarketplaceUpdatingHistoryMap : ClassMap<MP_CustomerMarketplaceUpdatingHistory> {
		public MP_CustomerMarketplaceUpdatingHistoryMap() {
			Table("MP_CustomerMarketplaceUpdatingHistory");

			Id(x => x.Id);

			References(x => x.CustomerMarketPlace, "CustomerMarketPlaceId");
			Map(x => x.UpdatingStart).CustomType<UtcDateTimeType>();
			Map(x => x.UpdatingEnd).CustomType<UtcDateTimeType>();
			Map(x => x.Error).Nullable();

			HasMany(x => x.ActionLog)
				.KeyColumn("CustomerMarketplaceUpdatingHistoryRecordId")
				.Cascade.All();

			HasOne(x => x.AmazonOrder)
				.PropertyRef(p => p.HistoryRecord)
				.Cascade.All();

			HasOne(x => x.AmazonFeedback)
				.PropertyRef(p => p.HistoryRecord)
				.Cascade.All();

			HasOne(x => x.EbayFeedback)
				.PropertyRef(p => p.HistoryRecord)
				.Cascade.All();

			HasOne(x => x.EbayOrder)
				.PropertyRef(p => p.HistoryRecord)
				.Cascade.All();

			HasOne(x => x.TeraPeakOrder)
				.PropertyRef(p => p.HistoryRecord)
				.Cascade.All();

			HasOne(x => x.EbayUserAccountData)
				.PropertyRef(p => p.HistoryRecord)
				.Cascade.All();

			HasOne(x => x.EbayUserData)
				.PropertyRef(p => p.HistoryRecord)
				.Cascade.All();

			HasOne(x => x.PayPalTransaction)
				.PropertyRef(p => p.HistoryRecord)
				.Cascade.All();

			HasOne(x => x.EkmOrder)
				.PropertyRef(p => p.HistoryRecord)
				.Cascade.All();

			HasOne(x => x.FreeAgentRequest)
				.PropertyRef(p => p.HistoryRecord)
				.Cascade.All();

			HasOne(x => x.SageRequest)
				.PropertyRef(p => p.HistoryRecord)
				.Cascade.All();

			HasOne(x => x.PayPointOrder)
				.PropertyRef(p => p.HistoryRecord)
				.Cascade.All();

			HasOne(x => x.ChannelGrabberOrder)
				.PropertyRef(p => p.HistoryRecord)
				.Cascade.All();

			HasOne(x => x.YodleeOrder)
				.PropertyRef(p => p.HistoryRecord)
				.Cascade.All();
			
			HasMany(x => x.HmrcAggregations).KeyColumn("CustomerMarketPlaceUpdatingHistoryID").LazyLoad().Inverse().Cascade.All();
			HasMany(x => x.YodleeAggregations).KeyColumn("CustomerMarketPlaceUpdatingHistoryID").LazyLoad().Inverse().Cascade.All();
			HasMany(x => x.PayPalAggregations).KeyColumn("CustomerMarketPlaceUpdatingHistoryID").LazyLoad().Inverse().Cascade.All();
			HasMany(x => x.EbayAggregations).KeyColumn("CustomerMarketPlaceUpdatingHistoryID").LazyLoad().Inverse().Cascade.All();
			HasMany(x => x.AmazonAggregations).KeyColumn("CustomerMarketPlaceUpdatingHistoryID").LazyLoad().Inverse().Cascade.All();

		}
	}
}