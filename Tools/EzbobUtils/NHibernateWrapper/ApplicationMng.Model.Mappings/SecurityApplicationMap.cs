using FluentNHibernate.Mapping;
using System;
namespace ApplicationMng.Model.Mappings
{
	public class SecurityApplicationMap : ClassMap<SecurityApplication>
	{
		public SecurityApplicationMap()
		{
			base.Cache.Region("LongTerm").ReadWrite();
			base.Table("Security_Application");
			base.LazyLoad();
			this.Id((SecurityApplication x) => (object)x.Id, "ApplicationId").GeneratedBy.Native("SEQ_SEC_APP");
			base.Map((SecurityApplication x) => x.Name).Length(255);
			base.Map((SecurityApplication x) => (object)x.ApplicationType).CustomType<SecurityApplicationType>();
			base.Map((SecurityApplication x) => x.Description);
			base.HasMany<MenuItem>((SecurityApplication x) => x.MenuItems).AsList(delegate(ListIndexPart i)
			{
				i.Column("Position");
			}).KeyColumn("SecAppId").Cascade.AllDeleteOrphan().Cache.ReadWrite().Region("LongTerm");
			base.HasManyToMany<Role>((SecurityApplication x) => x.Roles).AsSet().Table("Security_RoleAppRel").ParentKeyColumn("AppId").ChildKeyColumn("RoleId").Cascade.None().Cache.Region("LongTerm").ReadWrite();
		}
	}
}
