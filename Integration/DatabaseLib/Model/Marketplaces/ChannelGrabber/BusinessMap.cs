using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Database {
	#region class BusinessMap

	class BusinessMap : ClassMap<Business> {
		#region public

		#region constructor

		public BusinessMap() {
			Table("Business");
			Id(x => x.Id);
			Map(x => x.Name).Length(256);
			Map(x => x.Address).Length(4000);
		} // constructor

		#endregion constructor

		#endregion public
	} // class BusinessMap

	#endregion class BusinessMap
} // namespace
