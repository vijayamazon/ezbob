namespace EZBob.DatabaseLib.Model.Database.UserManagement
{
	using FluentNHibernate.Mapping;

	public class SecurityApplicationMap : ClassMap<SecurityApplication>
	{
		public SecurityApplicationMap()
		{
			Cache.Region("LongTerm").ReadWrite();
			Table("Security_Application");
			LazyLoad();
			Id(sa => (object)sa.Id, "ApplicationId").GeneratedBy.Native("SEQ_SEC_APP");
			Map(sa => sa.Name).Length(255);
			Map(sa => sa.Description);
			HasManyToMany(sa => sa.Roles).AsSet().Table("Security_RoleAppRel").ParentKeyColumn("AppId").ChildKeyColumn("RoleId").Cascade.None().Cache.Region("LongTerm").ReadWrite();
		}
	}
}
