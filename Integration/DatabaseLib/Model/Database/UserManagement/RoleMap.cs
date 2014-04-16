namespace EZBob.DatabaseLib.Model.Database.UserManagement
{
	using FluentNHibernate.Mapping;

	public class RoleMap : ClassMap<Role>
	{
		public RoleMap()
		{
			Cache.Region("LongTerm").ReadWrite();
			Table("Security_Role");
			Id(r => (object)r.Id).Column("RoleId").GeneratedBy.Native("SEQ_INSERT_SECURITY_ROLE");
			Map(r => r.Name);
			Map(r => r.Description);
			HasManyToMany(r => r.Users).AsSet().Table("Security_UserRoleRelation").ParentKeyColumn("RoleId").ChildKeyColumn("UserId").Cascade.SaveUpdate().Inverse().Cache.Region("LongTerm").ReadWrite();
			HasManyToMany(r => r.Applications).AsSet().Table("Security_RoleAppRel").ParentKeyColumn("RoleId").ChildKeyColumn("AppId").Cascade.SaveUpdate().Inverse().Cache.Region("LongTerm").ReadWrite();
			HasManyToMany(r => r.Permissions).AsSet().Table("Security_RolePermissionRel").ParentKeyColumn("RoleId").ChildKeyColumn("PermissionId").Cascade.All().Cache.Region("LongTerm").ReadWrite();
		}
	}
}
