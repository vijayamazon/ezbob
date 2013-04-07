using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database {
    
    
    public class MP_MarketplaceTypeMap : ClassMap<MP_MarketplaceType> {
        
        public MP_MarketplaceTypeMap() {
			Table("MP_MarketplaceType");
            Cache.ReadWrite().Region("Longest").ReadWrite();
			Id(x => x.Id).GeneratedBy.Identity().Column("Id");
			Map(x => x.Name).Column("Name").Not.Nullable().Length(255);
			Map(x => x.InternalId).Not.Nullable();
			Map(x => x.Description);
			Map(x => x.Active); // alexbo Apr 4 2013
        }
    }
}
