namespace EZBob.DatabaseLib.Model.Database.UserManagement
{
	using FluentNHibernate.Mapping;

	public class PermissionMap : ClassMap<Permission>
	{
		public PermissionMap()
		{
			Table("Security_Permission");
			Id(x => (object)x.Id).GeneratedBy.Assigned();
			Map(x => x.Name);
			Map(x => x.Description);
			HasManyToMany(x => x.Roles).AsSet().Table("Security_RolePermissionRel").ParentKeyColumn("RoleId").ChildKeyColumn("PermissionId").Cascade.SaveUpdate().Inverse().Cache.Region("LongTerm").ReadWrite();
		}
	}
}
