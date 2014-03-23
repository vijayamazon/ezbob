using FluentNHibernate.Mapping;
using System;
namespace ApplicationMng.Model
{
	public class ExecutionStateMap : ClassMap<ExecutionState>
	{
		public ExecutionStateMap()
		{
			base.LazyLoad();
			base.Table("StrategyEngine_ExecutionState");
			this.Id((ExecutionState x) => (object)x.Id).Column("Id");
			base.References<Application>((ExecutionState x) => x.App).Unique().Column("ApplicationId").Cascade.None().LazyLoad();
			base.References<Node>((ExecutionState x) => x.CurrentNode).Column("CurrentNodeId").Cascade.None().LazyLoad();
			base.Map((ExecutionState x) => x.CurrentNodePostfix);
			base.Map((ExecutionState x) => x.Data).Nullable().LazyLoad();
		}
	}
}
