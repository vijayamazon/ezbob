using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Database {
	public class MP_PlayOrderItemMap : ClassMap<MP_PlayOrderItem> {
		public MP_PlayOrderItemMap() {
			Table("MP_PlayOrderItem");
			Id(x => x.Id);
			References(x => x.Order, "OrderId");
			Map(x => x.TotalCost, "TotalCost");
			Map(x => x.PaymentDate, "PaymentDate").CustomType<UtcDateTimeType>();
			Map(x => x.PurchaseDate, "PurchaseDate").CustomType<UtcDateTimeType>();
			Map(x => x.OrderStatus, "OrderStatus").Length(300);
			Map(x => x.CurrencyCode, "CurrencyCode").Length(3);
			Map(x => x.NativeOrderId, "NativeOrderId").Length(300);
		} // constructor
	} // class MP_PlayOrderItemMap
} // namespace