using FluentNHibernate.Mapping;
using NHibernateWrapper.NHibernate.Model;
using NHibernateWrapper.StrategySchedule;
using System;
namespace ApplicationMng.Model.Mappings
{
	public class ApplicationExecutionTypeMap : ClassMap<ApplicationExecutionTypeItem>
	{
		public ApplicationExecutionTypeMap()
		{
			base.Cache.Region("LongTerm").ReadWrite();
			base.Table("Application_ExecutionType");
			this.Id((ApplicationExecutionTypeItem map) => (object)map.Id).Column("Id").GeneratedBy.Native("SEQ_Application_ExecutionType");
			base.Map((ApplicationExecutionTypeItem x) => (object)x.ItemId);
			base.Map((ApplicationExecutionTypeItem x) => (object)x.ExecutionType).CustomType(typeof(ExecutionType));
			base.Map((ApplicationExecutionTypeItem x) => (object)x.ApplicationId);
		}
	}
}
