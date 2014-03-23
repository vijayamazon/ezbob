using FluentNHibernate.Mapping;
using System;
namespace ApplicationMng.Model.Mappings
{
	public class AppDetailNameMap : ClassMap<AppDetailName>
	{
		public AppDetailNameMap()
		{
			base.Table("Application_DetailName");
			this.Id((AppDetailName x) => (object)x.Id, "DetailNameId").GeneratedBy.Native("SEQ_APP_DETAILNAME");
			base.Map((AppDetailName x) => x.Name, "Name");
		}
	}
}
