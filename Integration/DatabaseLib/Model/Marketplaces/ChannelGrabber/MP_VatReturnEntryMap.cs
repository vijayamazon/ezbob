using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database {

	public class MP_VatReturnEntryMap : ClassMap<MP_VatReturnEntry> {

		public MP_VatReturnEntryMap() {
			Table("MP_VatReturnEntries");
			Id(x => x.Id);
			References(x => x.Record, "RecordId");
			References(x => x.Name, "NameId");
			Map(x => x.Amount);
			Map(x => x.CurrencyCode).Length(3);
		} // constructor

	} // class MP_VatReturnEntryMap

} // namespace
