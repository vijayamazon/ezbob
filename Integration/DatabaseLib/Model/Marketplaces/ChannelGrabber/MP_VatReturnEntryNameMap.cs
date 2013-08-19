using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database {
	#region class BusinessMap

	class MP_VatReturnEntryNameMap : ClassMap<MP_VatReturnEntryName> {
		#region public

		#region constructor

		public MP_VatReturnEntryNameMap() {
			Table("MP_VatReturnEntryNames");
			Id(x => x.Id);
			Map(x => x.Name).Length(512);
		} // constructor

		#endregion constructor

		#endregion public
	} // class MP_VatReturnEntryNameMap

	#endregion class MP_VatReturnEntryNameMap
} // namespace
