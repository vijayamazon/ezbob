namespace EZBob.DatabaseLib.Model.Marketplaces {
	using FluentNHibernate.Mapping;

	public class MP_MarketplaceGroup 
	{
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
	}

    public class MP_MarketplaceGroupMap : ClassMap<MP_MarketplaceGroup>
    {

        public MP_MarketplaceGroupMap()
        {
            Table("MP_MarketplaceGroup");
            Not.LazyLoad();
            Cache.ReadWrite().Region("Longest").ReadWrite();
            Id(x => x.Id).GeneratedBy.Identity().Column("Id");
            Map(x => x.Name).Column("Name").Not.Nullable().Length(50);
        }
    }
}
