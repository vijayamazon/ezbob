using ApplicationMng.Model.Commands;
using FluentNHibernate.Mapping;
using System;
namespace ApplicationMng.Model
{
	public class AppAdditionalDataMap<T> : ClassMap<AppAdditionalData>
	{
		public AppAdditionalDataMap()
		{
			this.Id((AppAdditionalData x) => (object)x.Id, "AppId").GeneratedBy.Foreign("App");
			base.HasOne<Application>((AppAdditionalData x) => x.App).Constrained().Cascade.None();
			base.References<AppStatus>((AppAdditionalData x) => x.Status).Column("StatusId").LazyLoad().Cascade.All();
			base.Table("AppAdditionalData");
			base.References<CommandsList>((AppAdditionalData x) => x.CommandsList, "CommandsList").Cascade.All();
		}
	}
}
