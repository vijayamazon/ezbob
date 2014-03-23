using FluentNHibernate.Mapping;
using NHibernateWrapper.NHibernate.Model;
using System;
namespace ApplicationMng.Model
{
	public class SingleRunApplicationMap : ClassMap<SingleRunApplication>
	{
		public SingleRunApplicationMap()
		{
			base.Table("SingleRunApplication");
			this.Id((SingleRunApplication x) => (object)x.Id).GeneratedBy.Native("SEQ_SingleRunApplication");
			base.References<ApplicationExecutionTypeItem>((SingleRunApplication x) => x.ExecutionTypeItem).Column("ApplicationExecutionTypeId").LazyLoad();
		}
	}
}
