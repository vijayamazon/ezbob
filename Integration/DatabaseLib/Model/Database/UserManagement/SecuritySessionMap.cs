namespace EZBob.DatabaseLib.Model.Database.UserManagement
{
	using FluentNHibernate.Mapping;

	public class SecuritySessionMap : ClassMap<SecuritySession>
	{
		public SecuritySessionMap()
		{
			Id(ss => ss.Id).GeneratedBy.UuidHex("").Column("SessionId");
			Table("Security_Session");
			References(ss => ss.User).Column("UserId").Not.Nullable();
			Map(ss => (object)ss.CreationDate);
			Map(ss  => (object)ss.LastAccessTime);
			Map(ss => ss.HostAddress);
			Map(ss => (object)ss.State, "State");
		}
	}
}
