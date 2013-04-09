using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Database {
	public class MP_VolusionOrderItemMap : ClassMap<MP_VolusionOrderItem> {
		public MP_VolusionOrderItemMap() {
			Table("MP_VolusionOrderItem");
			Id(x => x.Id);
			References( x => x.Order, "OrderId" );
            Map(x => x.TotalCost, "TotalCost");
            Map(x => x.PaymentDate, "PaymentDate").CustomType<UtcDateTimeType>();
            Map(x => x.PurchaseDate, "PurchaseDate").CustomType<UtcDateTimeType>();
            Map(x => x.OrderStatus, "OrderStatus").Length(300);
		} // constructor
	} // class MP_VolusionOrderItemMap
} // namespace