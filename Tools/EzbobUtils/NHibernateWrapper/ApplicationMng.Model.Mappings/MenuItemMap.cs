using FluentNHibernate.Mapping;
using System;
namespace ApplicationMng.Model.Mappings
{
	public class MenuItemMap : ClassMap<MenuItem>
	{
		public MenuItemMap()
		{
			base.LazyLoad();
			base.Cache.ReadWrite().Region("LongTerm");
			this.Id((MenuItem x) => (object)x.Id).GeneratedBy.Native("SEQ_MENUITEM");
			base.References<SecurityApplication>((MenuItem x) => x.SecApp).Column("SecAppId");
			base.Map((MenuItem x) => x.Caption).Length(256);
			base.Map((MenuItem x) => x.Description).Length(256);
			base.Map((MenuItem x) => x.Url).Length(512);
			base.Table("MenuItem");
			base.Map((MenuItem x) => x.FilterData, "Filter");
			base.HasManyToMany<AppStatus>((MenuItem x) => x.Statuses).AsSet().Table("MenuItem_Status_Rel").ParentKeyColumn("MenuItemId").ChildKeyColumn("StatusId").Cascade.None();
			base.References<MenuItem>((MenuItem x) => x.Parent).Column("ParentId");
			base.HasMany<MenuItem>((MenuItem x) => x.Tabs).KeyColumn("ParentId").AsSet().Inverse().Cascade.AllDeleteOrphan();
		}
	}
}
