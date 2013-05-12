using System;
using System.Collections.Generic;
using EzBob.CommonLib.ReceivedDataListLogic;

namespace EZBob.DatabaseLib.DatabaseWrapper.Order {

	public class PlayOrdersList
		: ReceivedDataListTimeMarketTimeDependentBase<PlayOrderItem>
	{
		public PlayOrdersList(
			DateTime submittedDate,
			IEnumerable<PlayOrderItem> collection = null
		) : base(submittedDate, collection)
		{} // constructor

		public override ReceivedDataListTimeDependentBase<PlayOrderItem> Create(
			DateTime submittedDate,
			IEnumerable<PlayOrderItem> collection
		) {
			return new PlayOrdersList(submittedDate, collection);
		} // Create
	} // class PlayOrdersList
} // namespace