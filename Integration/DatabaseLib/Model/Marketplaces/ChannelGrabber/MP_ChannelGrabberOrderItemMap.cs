using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Database {
	public class MP_ChannelGrabberOrderItemMap : ClassMap<MP_ChannelGrabberOrderItem> {
		public MP_ChannelGrabberOrderItemMap() {
			Table("MP_ChannelGrabberOrderItem");
			Id(x => x.Id);
			References(x => x.Order, "OrderId");
			Map(x => x.TotalCost, "TotalCost");
			Map(x => x.PaymentDate, "PaymentDate").CustomType<UtcDateTimeType>();
			Map(x => x.PurchaseDate, "PurchaseDate").CustomType<UtcDateTimeType>();
			Map(x => x.OrderStatus, "OrderStatus").Length(300);
			Map(x => x.CurrencyCode, "CurrencyCode").Length(3);
			Map(x => x.NativeOrderId, "NativeOrderId").Length(300);
			Map(x => x.IsExpense, "IsExpense");
		} // constructor
	} // class MP_ChannelGrabberOrderItemMap
} // namespace