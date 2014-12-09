using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database {

	class MP_VatReturnEntryNameMap : ClassMap<MP_VatReturnEntryName> {

		public MP_VatReturnEntryNameMap() {
			Table("MP_VatReturnEntryNames");
			Id(x => x.Id);
			Map(x => x.Name).Length(512);
		} // constructor

	} // class MP_VatReturnEntryNameMap

} // namespace
