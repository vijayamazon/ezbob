using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Marketplaces.Amazon
{
	public class MP_AmazonMarketplaceTypeMap : ClassMap<MP_AmazonMarketplaceType>
	{
		public MP_AmazonMarketplaceTypeMap()
		{
			Table("MP_AmazonMarketplaceType");
			Id(x => x.Id);
			Map(x => x.MarketplaceId);
			Map(x => x.Country);
			Map(x => x.Domain);
		}
	}
}