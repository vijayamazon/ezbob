using System;
using System.Collections.Generic;
using EzBob.CommonLib.ReceivedDataListLogic;

namespace EZBob.DatabaseLib.DatabaseWrapper.Order {
	public class InternalDataList
		: ReceivedDataListTimeMarketTimeDependentBase<AInternalOrderItem>
	{
		public InternalDataList(
			DateTime submittedDate,
			IEnumerable<AInternalOrderItem> collection = null
		) : base(submittedDate, collection)
		{} // constructor

		public override ReceivedDataListTimeDependentBase<AInternalOrderItem> Create(
			DateTime submittedDate,
			IEnumerable<AInternalOrderItem> collection
		) {
			return new InternalDataList(submittedDate, collection);
		} // Create
	} // class InternalDataList
} // namespace