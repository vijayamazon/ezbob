using System;
using System.Collections.Generic;
using EzBob.CommonLib.ReceivedDataListLogic;

namespace EZBob.DatabaseLib.DatabaseWrapper.Order {
	public class InternalDataList
		: ReceivedDataListTimeMarketTimeDependentBase<InternalOrderItem>
	{
		public InternalDataList(
			DateTime submittedDate,
			IEnumerable<InternalOrderItem> collection = null
		) : base(submittedDate, collection)
		{} // constructor

		public override ReceivedDataListTimeDependentBase<InternalOrderItem> Create(
			DateTime submittedDate,
			IEnumerable<InternalOrderItem> collection
		) {
			return new InternalDataList(submittedDate, collection);
		} // Create
	} // class InternalDataList
} // namespace