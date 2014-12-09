using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database {

    public class MP_AnalyisisFunctionMap : ClassMap<MP_AnalyisisFunction> {

        public MP_AnalyisisFunctionMap() {
			Table("MP_AnalyisisFunction");
			Id(x => x.Id).GeneratedBy.Identity().Column("Id");
			References(x => x.Marketplace).Column("MarketPlaceId");
			References(x => x.ValueType).Column("ValueTypeId");
			Map(x => x.Name).Column("Name").Not.Nullable().Length(50);
			Map(x => x.InternalId).Column("InternalId").Not.Nullable();
			Map(x => x.Description).Column("Description").Length(1073741823);
			HasMany(x => x.AnalyisisFunctionValues).KeyColumn("AnalyisisFunctionId");
        }
    }
}
