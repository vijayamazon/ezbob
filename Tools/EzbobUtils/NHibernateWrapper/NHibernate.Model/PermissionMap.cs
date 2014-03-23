using ApplicationMng.Model;
using FluentNHibernate.Mapping;
using System;
namespace NHibernateWrapper.NHibernate.Model
{
	public class PermissionMap : ClassMap<Permission>
	{
		public PermissionMap()
		{
			base.Table("Security_Permission");
			this.Id((Permission x) => (object)x.Id).GeneratedBy.Assigned();
			base.Map((Permission x) => x.Name);
			base.Map((Permission x) => x.Description);
			base.HasManyToMany<Role>((Permission x) => x.Roles).AsSet().Table("Security_RolePermissionRel").ParentKeyColumn("RoleId").ChildKeyColumn("PermissionId").Cascade.SaveUpdate().Inverse().Cache.Region("LongTerm").ReadWrite();
		}
	}
}
