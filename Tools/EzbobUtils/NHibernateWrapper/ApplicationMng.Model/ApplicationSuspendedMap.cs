using FluentNHibernate.Mapping;
using NHibernateWrapper.NHibernate.Model;
using System;
namespace ApplicationMng.Model
{
	public class ApplicationSuspendedMap : ClassMap<ApplicationSuspended>
	{
		public ApplicationSuspendedMap()
		{
			base.LazyLoad();
			base.Table("Application_Suspended");
			this.Id((ApplicationSuspended x) => (object)x.ApplicationId).GeneratedBy.Assigned();
			base.Map((ApplicationSuspended x) => x.ExecutionState).CustomType("StringClob").Nullable().LazyLoad();
			base.Map((ApplicationSuspended x) => x.Postfix);
			base.Map((ApplicationSuspended x) => x.Target);
			base.Map((ApplicationSuspended x) => x.Label);
			base.Map((ApplicationSuspended x) => x.Message).Length(2147483647).LazyLoad();
			base.Map((ApplicationSuspended x) => (object)x.AppSpecific).Nullable();
			base.Map((ApplicationSuspended x) => (object)x.Date).Column("\"Date\"");
			base.Map((ApplicationSuspended x) => (object)x.ExecutionType).Nullable();
			base.Map((ApplicationSuspended x) => (object)x.ExecutionPathCurrentItemId);
		}
	}
}
