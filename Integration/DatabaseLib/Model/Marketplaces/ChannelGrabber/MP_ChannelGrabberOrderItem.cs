using System;

namespace EZBob.DatabaseLib.Model.Database {
	public class MP_ChannelGrabberOrderItem {
		public virtual int Id { get; set; }
		public virtual MP_ChannelGrabberOrder Order { get; set; }
		public virtual string NativeOrderId { get; set; }
		public virtual double? TotalCost { get; set; }
		public virtual string CurrencyCode { get; set; }
		public virtual DateTime PaymentDate { get; set; }
		public virtual DateTime PurchaseDate { get; set; }
		public virtual string OrderStatus { get; set; }
	} // class MP_ChannelGrabberOrderItem
} // namespace