namespace EZBob.DatabaseLib.Model.Database {
	using FluentNHibernate.Mapping;
	using NHibernate.Type;

	class MP_VatReturnRecordMap : ClassMap<MP_VatReturnRecord> {
		public MP_VatReturnRecordMap() {
			Table("MP_VatReturnRecords");
			Id(x => x.Id);
			References(x => x.CustomerMarketPlace, "CustomerMarketPlaceId");
			Map(x => x.Created).CustomType<UtcDateTimeType>().Not.Nullable();

			HasMany(x => x.Entries).KeyColumn( "RecordId" ).Cascade.All();

			Map(x => x.Period).Length(256);
			Map(x => x.DateFrom).CustomType<UtcDateTimeType>().Not.Nullable();
			Map(x => x.DateTo).CustomType<UtcDateTimeType>().Not.Nullable();
			Map(x => x.DateDue).CustomType<UtcDateTimeType>().Not.Nullable();

			Map(x => x.RegistrationNo);

			References(x => x.Business, "BusinessId");

			Map(x => x.IsDeleted);

			Map(x => x.SourceID);

			References(x => x.HistoryRecord)
				.Column("CustomerMarketPlaceUpdatingHistoryRecordId")
				.Unique()
				.Cascade
				.None();
		} // constructor
	} // class MP_VatReturnRecordMap
} // namespace
