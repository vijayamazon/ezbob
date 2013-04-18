using System;
using System.Collections.Generic;
using EzBob.CommonLib.ReceivedDataListLogic;
using EzBob.CommonLib.TimePeriodLogic;

namespace EZBob.DatabaseLib.DatabaseWrapper.Order {
	public class VolusionOrderItem : TimeDependentRangedDataBase {
		public virtual string NativeOrderId { get; set; }
		public virtual double? TotalCost { get; set; }
		public virtual string CurrencyCode { get; set; }
		public virtual DateTime PaymentDate { get; set; }
		public virtual DateTime PurchaseDate { get; set; }
		public virtual string OrderStatus { get; set; }

		public override DateTime RecordTime { get { return PurchaseDate; }} // RecordTime
	} // class VolusionOrderItem

	public class VolusionOrdersList
		: ReceivedDataListTimeMarketTimeDependentBase<VolusionOrderItem>
	{
		public VolusionOrdersList(
			DateTime submittedDate,
			IEnumerable<VolusionOrderItem> collection = null
		) : base(submittedDate, collection)
		{} // constructor

		public override ReceivedDataListTimeDependentBase<VolusionOrderItem> Create(
			DateTime submittedDate,
			IEnumerable<VolusionOrderItem> collection
		) {
			return new VolusionOrdersList(submittedDate, collection);
		} // Create
	} // class VolusionOrdersList
} // namespace