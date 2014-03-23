using FluentNHibernate.Mapping;
using System;
namespace ApplicationMng.Model
{
	public class ExclusiveApplicationMap : ClassMap<ExclusiveApplication>
	{
		public ExclusiveApplicationMap()
		{
			base.Table("ExclusiveApplication");
			this.Id((ExclusiveApplication x) => (object)x.Id).GeneratedBy.Native("SEQ_ExclusiveApplication");
			base.Map((ExclusiveApplication x) => (object)x.ApplicationId);
		}
	}
}
