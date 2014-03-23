using FluentNHibernate.Mapping;
using System;
namespace ApplicationMng.Model.Filters
{
	public class FilterBaseMap : ClassMap<FilterBase>
	{
		public FilterBaseMap()
		{
			base.Cache.Region("LongTerm").ReadWrite();
			base.Table("Filter");
			this.Id((FilterBase x) => (object)x.Id);
			base.Map((FilterBase x) => x.Name).Length(256);
			this.DiscriminateSubClassesOnColumn<string>("FilterType").Length(256);
		}
	}
}
