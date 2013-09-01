using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Database {
	#region class MP_VatReturnRecordMap

	class MP_RtiTaxMonthRecordMap : ClassMap<MP_RtiTaxMonthRecord> {
		#region public

		#region constructor

		public MP_RtiTaxMonthRecordMap() {
			Table("MP_RtiTaxMonthRecords");
			Id(x => x.Id);
			References(x => x.CustomerMarketPlace, "CustomerMarketPlaceId");
			Map(x => x.Created).CustomType<UtcDateTimeType>().Not.Nullable();

			HasMany(x => x.Entries).KeyColumn( "RecordId" ).Cascade.All();

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
