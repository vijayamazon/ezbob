using System;

namespace EZBob.DatabaseLib.DatabaseWrapper.Order {
	public interface IChannelGrabberOrderItem {
		string NativeOrderId { get; set; }
		double? TotalCost { get; set; }
		string CurrencyCode { get; set; }
		DateTime PaymentDate { get; set; }
		DateTime PurchaseDate { get; set; }
		string OrderStatus { get; set; }
	} // class IChannelGrabberOrderItem
} // namespace