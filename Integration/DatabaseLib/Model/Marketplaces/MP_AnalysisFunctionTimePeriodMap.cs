namespace EZBob.DatabaseLib.Model.Database {
	using FluentNHibernate.Mapping;

	public class MP_AnalysisFunctionTimePeriodMap : ClassMap<MP_AnalysisFunctionTimePeriod> {
		public MP_AnalysisFunctionTimePeriodMap() {
			Table("MP_AnalysisFunctionTimePeriod");
			Id(x => x.Id).GeneratedBy.Identity().Column("Id");
			Map(x => x.Name).Column("Name").Not.Nullable().Length(50);
			Map(x => x.InternalId).Column("InternalId").Not.Nullable();
			Map(x => x.Description).Column("Description").Length(1073741823);
			Cache.ReadWrite().Region("LongTerm");
		}
	}
}
