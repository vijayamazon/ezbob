using System;
using EzBob.CommonLib.TimePeriodLogic;

namespace EZBob.DatabaseLib.DatabaseWrapper.Order {
	public class PlayOrderItem : TimeDependentRangedDataBase {
		public virtual string NativeOrderId { get; set; }
		public virtual double? TotalCost { get; set; }
		public virtual string CurrencyCode { get; set; }
		public virtual DateTime PaymentDate { get; set; }
		public virtual DateTime PurchaseDate { get; set; }
		public virtual string OrderStatus { get; set; }

		public override DateTime RecordTime { get { return PurchaseDate; }} // RecordTime
	} // class PlayOrderItem
} // namespace