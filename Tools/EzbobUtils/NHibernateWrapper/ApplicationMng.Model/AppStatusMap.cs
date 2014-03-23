using FluentNHibernate.Mapping;
using System;
namespace ApplicationMng.Model
{
	public class AppStatusMap : ClassMap<AppStatus>
	{
		public AppStatusMap()
		{
			this.Id((AppStatus x) => (object)x.Id).Column("Id").GeneratedBy.Native("SEQ_APPSTATUS");
			base.Map((AppStatus x) => x.Name).Length(128);
			base.Map((AppStatus x) => x.Description).Length(1024);
			base.Table("AppStatus");
		}
	}
}
