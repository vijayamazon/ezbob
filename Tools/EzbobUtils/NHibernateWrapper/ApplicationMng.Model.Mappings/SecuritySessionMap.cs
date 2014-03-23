using FluentNHibernate.Mapping;
using System;
namespace ApplicationMng.Model.Mappings
{
	public class SecuritySessionMap : ClassMap<SecuritySession>
	{
		public SecuritySessionMap()
		{
			this.Id((SecuritySession x) => x.Id).GeneratedBy.UuidHex("").Column("SessionId");
			base.Table("Security_Session");
			base.References<User>((SecuritySession x) => x.User).Column("UserId").Not.Nullable();
			base.References<SecurityApplication>((SecuritySession x) => x.SecApp).Column("AppId").Not.Nullable();
			base.Map((SecuritySession x) => (object)x.CreationDate);
			base.Map((SecuritySession x) => (object)x.LastAccessTime);
			base.Map((SecuritySession x) => x.HostAddress);
			base.Map((SecuritySession x) => (object)x.State, "State");
		}
	}
}
