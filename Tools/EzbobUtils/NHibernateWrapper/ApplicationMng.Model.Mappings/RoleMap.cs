using FluentNHibernate.Mapping;
using NHibernateWrapper.NHibernate.Model;
using System;
namespace ApplicationMng.Model.Mappings
{
	public class RoleMap : ClassMap<Role>
	{
		public RoleMap()
		{
			base.Cache.Region("LongTerm").ReadWrite();
			base.Table("Security_Role");
			this.Id((Role x) => (object)x.Id).Column("RoleId").GeneratedBy.Native("SEQ_INSERT_SECURITY_ROLE");
			base.Map((Role x) => x.Name);
			base.Map((Role x) => x.Description);
			base.HasManyToMany<User>((Role x) => x.Users).AsSet().Table("Security_UserRoleRelation").ParentKeyColumn("RoleId").ChildKeyColumn("UserId").Cascade.SaveUpdate().Inverse().Cache.Region("LongTerm").ReadWrite();
			base.HasManyToMany<Permission>((Role x) => x.Permissions).AsSet().Table("Security_RolePermissionRel").ParentKeyColumn("RoleId").ChildKeyColumn("PermissionId").Cascade.All().Cache.Region("LongTerm").ReadWrite();
		}
	}
}
