using FluentNHibernate.Mapping;
using NHibernateWrapper.NHibernate.Model;
using System;
namespace ApplicationMng.Model.Mappings
{
	public class ApplicationMap : ClassMap<Application>
	{
		public ApplicationMap()
		{
			base.Table("Application_Application");
			this.Id((Application x) => (object)x.Id).Column("ApplicationId");
			base.Map((Application x) => (object)x.Version);
			base.References<User>((Application x) => x.Locker).Column("LockedByUserId").Cascade.All();
			base.References<User>((Application x) => x.Creator).Column("CreatorUserId").Cascade.All();
			base.Map((Application x) => (object)x.CreationDate);
			base.Map((Application x) => (object)x.IsTimeLimitExceeded);
			base.Map((Application x) => (object)x.AppCounter);
			base.Map((Application x) => (object)x.ParentAppID);
			base.Map((Application x) => x.ExecutionPathBin);
		}
	}
}
