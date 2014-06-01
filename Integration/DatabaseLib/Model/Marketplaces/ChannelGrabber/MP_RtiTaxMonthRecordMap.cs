namespace EZBob.DatabaseLib.Model.Database {
	using FluentNHibernate.Mapping;
	using NHibernate.Type;

	class MP_RtiTaxMonthRecordMap : ClassMap<MP_RtiTaxMonthRecord> {
		public MP_RtiTaxMonthRecordMap() {
			Table("MP_RtiTaxMonthRecords");
			Id(x => x.Id);
			References(x => x.CustomerMarketPlace, "CustomerMarketPlaceId");
			Map(x => x.Created).CustomType<UtcDateTimeType>().Not.Nullable();
			Map(x => x.SourceID);

			HasMany(x => x.Entries).KeyColumn( "RecordId" ).Cascade.All();

			References(x => x.HistoryRecord)
				.Column("CustomerMarketPlaceUpdatingHistoryRecordId")
				.Unique()
				.Cascade
				.None();
		} // constructor
	} // class MP_VatReturnRecordMap
} // namespace
