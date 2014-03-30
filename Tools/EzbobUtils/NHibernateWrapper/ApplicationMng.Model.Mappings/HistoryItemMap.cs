using FluentNHibernate.Mapping;
using System;
namespace ApplicationMng.Model.Mappings
{
	public class HistoryItemMap : ClassMap<HistoryItem>
	{
		public HistoryItemMap()
		{
			base.Table("Control_History");
			this.Id((HistoryItem x) => (object)x.Id, "HISROTYID").GeneratedBy.Native("SEQ_CONTROL_HYISTORY");
			base.References<Application>((HistoryItem x) => x.App, "APPLICATIONID");
			base.References<SecurityApplication>((HistoryItem x) => x.SecApp, "SECURITYAPPID");
			base.References<User>((HistoryItem x) => x.User, "USERID");
			base.References<Node>((HistoryItem x) => x.Node, "NODEID");
			base.Map((HistoryItem x) => (object)x.ChangeTime, "CHANGESTIME");
			base.Map((HistoryItem x) => x.ControlName, "CONTROLNAME");
			base.Map((HistoryItem x) => x.ControlValue, "CONTROLVALUE");
		}
	}
}
