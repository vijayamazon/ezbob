using FluentNHibernate.Mapping;
using System;
namespace ApplicationMng.Model.Mappings
{
	public class PublicNameStrategyMap : ClassMap<PublicNameStrategy>
	{
		public PublicNameStrategyMap()
		{
			base.Table("Strategy_PublicRel");
			this.CompositeId().KeyReference((PublicNameStrategy x) => x.PublicName, new string[]
			{
				"PUBLICID"
			}).KeyReference((PublicNameStrategy x) => x.Strategy, new string[]
			{
				"STRATEGYID"
			});
			base.Map((PublicNameStrategy x) => (object)x.Percent, "`PERCENT`");
		}
	}
}
