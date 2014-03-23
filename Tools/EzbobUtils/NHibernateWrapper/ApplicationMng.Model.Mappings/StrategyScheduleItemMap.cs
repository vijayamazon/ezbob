using FluentNHibernate.Mapping;
using NHibernateWrapper.NHibernate.Model;
using NHibernateWrapper.StrategySchedule;
using System;
namespace ApplicationMng.Model.Mappings
{
	public class StrategyScheduleItemMap : ClassMap<StrategyScheduleItem>
	{
		public StrategyScheduleItemMap()
		{
			base.Table("Strategy_Schedule");
			this.Id((StrategyScheduleItem map) => (object)map.Id).Column("Id").GeneratedBy.Native("SEQ_Strategy_Schedule");
			base.Map((StrategyScheduleItem x) => x.Name).Length(256);
			base.Map((StrategyScheduleItem x) => (object)x.ScheduleType).CustomType(typeof(ScheduleItemType));
			base.Map((StrategyScheduleItem x) => x.Mask).Column("ScheduleMask").Length(512);
			base.Map((StrategyScheduleItem x) => (object)x.NextRun);
			base.Map((StrategyScheduleItem x) => (object)x.IsPaused);
			base.Map((StrategyScheduleItem x) => (object)x.ExecutionType).CustomType(typeof(ExecutionType));
			base.References<Strategy>((StrategyScheduleItem item) => item.Strategy, "StrategyId");
			base.References<User>((StrategyScheduleItem item) => item.CreatorUser, "CreatorUserId");
			base.HasMany<StrategyScheduleParam>((StrategyScheduleItem item) => item.StrategyScheduledInputs).AsSet().KeyColumn("StrategyScheduleId").Cascade.All().Inverse().Where("Deleted is null");
		}
	}
}
