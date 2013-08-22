using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Database {
	#region class MP_VatReturnRecordMap

	class MP_VatReturnRecordMap : ClassMap<MP_VatReturnRecord> {
		#region public

		#region constructor

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

			References(x => x.HistoryRecord)
				.Column("CustomerMarketPlaceUpdatingHistoryRecordId")
				.Unique()
				.Cascade
				.None();
		} // constructor

		#endregion constructor

		#endregion public
	} // class MP_VatReturnRecordMap

	#endregion class MP_VatReturnRecordMap
} // namespace
