using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_TeraPeakOrderItemMap : ClassMap<MP_TeraPeakOrderItem>
	{
		public MP_TeraPeakOrderItemMap()
		{
			Table( "MP_TeraPeakOrderItem" );
			Id(x => x.Id);
			References( x => x.Order, "TeraPeakOrderId" );
			Map( x => x.StartDate ).CustomType<UtcDateTimeType>();
			Map( x => x.EndDate ).CustomType<UtcDateTimeType>();
			Map( x => x.Revenue );
			Map( x => x.Listings );
			Map( x => x.Transactions );
			Map( x => x.Successful );
			Map( x => x.Bids );
			Map( x => x.ItemsOffered );
			Map( x => x.ItemsSold );
			Map( x => x.AverageSellersPerDay );
			Map( x => x.SuccessRate );
			Map(x => x.RangeMarker).CustomType<int>();
		    HasMany(x => x.CategoryStatistics).AsBag().OrderBy("Listings").Inverse().KeyColumn("OrderItemId").Cascade.All();
		}
	}
}