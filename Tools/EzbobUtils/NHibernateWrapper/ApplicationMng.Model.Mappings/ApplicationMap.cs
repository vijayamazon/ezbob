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
			base.Map((Application x) => (object)x.State).CustomType(typeof(ApplicationStrategyState));
			base.References<Strategy>((Application x) => x.Strategy).Column("StrategyId");
			base.References<User>((Application x) => x.Locker).Column("LockedByUserId").Cascade.All();
			base.References<User>((Application x) => x.Creator).Column("CreatorUserId").Cascade.All();
			base.Map((Application x) => (object)x.CreationDate);
			base.Map((Application x) => (object)x.IsTimeLimitExceeded);
			base.Map((Application x) => (object)x.AppCounter);
			base.HasOne<ExecutionState>((Application x) => x.ExecutionState).PropertyRef((ExecutionState p) => p.App).Cascade.All().Fetch.Join();
			base.Map((Application x) => (object)x.ParentAppID);
			base.Map((Application x) => x.ExecutionPathBin);
			base.HasOne<AppAdditionalData>((Application x) => x.AdditionalData).Fetch.Join().Cascade.All();
			base.HasMany<Signal>((Application application) => application.Signal).AsSet().KeyColumn("ApplicationID").Cascade.All().Inverse();
		}
	}
}
