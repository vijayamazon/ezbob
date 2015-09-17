namespace EZBob.DatabaseLib.Model.Marketplaces {
	using FluentNHibernate.Mapping;

	public class MP_MarketplaceGroup {
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual bool ActiveWizardOnline { get; set; }
		public virtual bool ActiveWizardOffline { get; set; }
		public virtual bool ActiveDashboardOnline { get; set; }
		public virtual bool ActiveDashboardOffline { get; set; }
		public virtual int PriorityOnline { get; set; }
		public virtual int PriorityOffline { get; set; }
		public virtual string DisplayName { get; set; }
		public virtual string Description { get; set; }
	} // class MP_MarketplaceGroup

	public class MP_MarketplaceGroupMap : ClassMap<MP_MarketplaceGroup> {
		public MP_MarketplaceGroupMap() {
			Table("MP_MarketplaceGroup");
			Not.LazyLoad();
			Cache.ReadWrite().Region("Longest").ReadWrite();
			Id(x => x.Id).GeneratedBy.Identity().Column("Id");
			Map(x => x.Name).Column("Name").Not.Nullable().Length(50);
			Map(x => x.ActiveWizardOnline);
			Map(x => x.ActiveWizardOffline);
			Map(x => x.ActiveDashboardOnline);
			Map(x => x.ActiveDashboardOffline);
			Map(x => x.PriorityOnline);
			Map(x => x.PriorityOffline);
			Map(x => x.DisplayName).Not.Nullable().Length(255);
			Map(x => x.Description).Length(2000);
		} // constructor
	} // class MP_MarketplaceGroupMap
} // namespace
