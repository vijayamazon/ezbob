using ApplicationMng.Model;
using FluentNHibernate.Mapping;
using System;
namespace NHibernateWrapper.NHibernate.Model.ApplicationMng.Model
{
	public class ExecutionStateMap : ClassMap<Signal>
	{
		public ExecutionStateMap()
		{
			base.LazyLoad();
			base.Table("Signal");
			this.Id((Signal x) => (object)x.Id).Column("Id");
			base.Map((Signal x) => (object)x.Priority);
			base.Map((Signal x) => (object)x.OwnerApplicationId);
			base.Map((Signal x) => x.Message).Length(8000).LazyLoad();
			base.References<Application>((Signal x) => x.App).Unique().Column("ApplicationId").Cascade.None().LazyLoad();
		}
	}
}
