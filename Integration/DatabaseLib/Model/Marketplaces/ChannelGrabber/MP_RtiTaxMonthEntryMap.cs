using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Database {
	#region class MP_RtiTaxMonthEntryMap

	public class MP_RtiTaxMonthEntryMap : ClassMap<MP_RtiTaxMonthEntry> {
		#region public

		#region constructor

		public MP_RtiTaxMonthEntryMap() {
			Table("MP_RtiTaxMonthEntries");
			Id(x => x.Id);
			References(x => x.Record, "RecordId");
			Map(x => x.DateStart).CustomType<UtcDateTimeType>().Not.Nullable();
			Map(x => x.DateEnd).CustomType<UtcDateTimeType>().Not.Nullable();
			Map(x => x.AmountPaid);
			Map(x => x.AmountDue);
			Map(x => x.CurrencyCode).Length(3);
		} // constructor

		#endregion constructor

		#endregion public
	} // class MP_RtiTaxMonthEntryMap

	#endregion class MP_RtiTaxMonthEntryMap
} // namespace
