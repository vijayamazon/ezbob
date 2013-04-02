using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_CustomerMarketplaceUpdatingCounterMap : ClassMap<MP_CustomerMarketplaceUpdatingCounter>
	{
		public MP_CustomerMarketplaceUpdatingCounterMap()
		{
			Table("MP_CustomerMarketplaceUpdatingCounter");
			Id(x => x.Id);
			References( x => x.Action, "CustomerMarketplaceUpdatingActionLogId" );
			Map( x => x.Created ).CustomType<UtcDateTimeType>();
			Map( x => x.Method );
			Map( x => x.Details );
		}
	}
}