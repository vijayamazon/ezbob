using FluentNHibernate.Mapping;
using System;
namespace ApplicationMng.Model.Filters
{
	public class MultiStatusFilterMap : SubclassMap<MultiStatusFilter>
	{
		public MultiStatusFilterMap()
		{
			base.Map((MultiStatusFilter x) => x.Statuses).Length(2048);
			base.Map((MultiStatusFilter x) => x.States).Length(2048);
		}
	}
}
