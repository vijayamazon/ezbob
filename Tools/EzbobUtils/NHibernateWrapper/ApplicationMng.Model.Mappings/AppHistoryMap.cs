using FluentNHibernate.Mapping;
using System;
namespace ApplicationMng.Model.Mappings
{
	public class AppHistoryMap : ClassMap<AppHistory>
	{
		public AppHistoryMap()
		{
			base.Table("Application_History");
			this.Id((AppHistory x) => (object)x.Id);
			base.Map((AppHistory x) => (object)x.UserId);
			base.Map((AppHistory x) => (object)x.SecurityApplicationId);
			base.Map((AppHistory x) => (object)x.ActionType);
			base.References<Application>((AppHistory x) => x.App).LazyLoad().Column("ApplicationId");
			base.Map((AppHistory x) => (object)x.CurrentNodeID);
		}
	}
}
