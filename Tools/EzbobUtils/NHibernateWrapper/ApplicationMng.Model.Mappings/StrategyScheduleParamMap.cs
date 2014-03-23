using FluentNHibernate.Mapping;
using NHibernateWrapper.NHibernate.Model;
using System;
namespace ApplicationMng.Model.Mappings
{
	public class StrategyScheduleParamMap : ClassMap<StrategyScheduleParam>
	{
		public StrategyScheduleParamMap()
		{
			this.Id((StrategyScheduleParam x) => (object)x.Id).Column("Id");
			base.Table("Strategy_ScheduleParam");
			base.Map((StrategyScheduleParam x) => x.Data).CustomType("StringClob");
		}
	}
}
