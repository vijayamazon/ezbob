namespace EZBob.DatabaseLib.Model.Database {
	using FluentNHibernate.Mapping;

	class BusinessMap : ClassMap<Business> {
		public BusinessMap() {
			Table("Business");
			Id(x => x.Id);
			Map(x => x.Name).Length(256);
			Map(x => x.Address).Length(4000);
			Map(x => x.RegistrationNo);
		} // constructor
	} // class BusinessMap
} // namespace
