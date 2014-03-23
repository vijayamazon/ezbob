using FluentNHibernate.Mapping;
using System;
namespace ApplicationMng.Model.Mappings
{
	public class AppDetailMap : ClassMap<AppDetail>
	{
		public AppDetailMap()
		{
			base.Table("Application_Detail");
			this.Id((AppDetail x) => (object)x.Id, "DetailId").GeneratedBy.Native("SEQ_APP_DETAIL");
			base.Map((AppDetail x) => x.ValueStr, "ValueStr").CustomType("StringClob");
			base.References<Application>((AppDetail x) => x.App, "ApplicationId");
			base.References<AppDetailName>((AppDetail x) => x.Name, "DetailNameId").Cascade.All().Fetch.Join();
			base.References<AppDetail>((AppDetail x) => x.Parent, "ParentDetailId");
			base.HasMany<AppDetail>((AppDetail x) => x.ChildDetails).AsSet().Inverse().Fetch.Join().KeyColumn("ParentDetailId").Cascade.All();
		}
	}
}
