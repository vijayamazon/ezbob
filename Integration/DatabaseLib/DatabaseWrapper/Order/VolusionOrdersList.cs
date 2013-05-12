using System;
using System.Collections.Generic;
using EzBob.CommonLib.ReceivedDataListLogic;

namespace EZBob.DatabaseLib.DatabaseWrapper.Order {
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