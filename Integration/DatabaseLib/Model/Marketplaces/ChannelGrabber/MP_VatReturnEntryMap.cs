using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Database {
	#region class MP_VatReturnEntryMap

	class MP_VatReturnEntryMap : ClassMap<MP_VatReturnEntry> {
		#region public

		#region constructor

		public MP_VatReturnEntryMap() {
			Table("MP_VatReturnEntries");
			Id(x => x.Id);
			References(x => x.Record, "RecordId");
			References(x => x.Name, "NameId");
			Map(x => x.Amount);
			Map(x => x.Currency).Length(3);
		} // constructor

		#endregion constructor

		#endregion public
	} // class MP_VatReturnEntryMap

	#endregion class MP_VatReturnEntryMap
} // namespace
