using System;
using System.Collections.Generic;
using EzBob.CommonLib.ReceivedDataListLogic;

namespace EZBob.DatabaseLib.DatabaseWrapper.Order {
	public class ChannelGrabberOrdersList
		: ReceivedDataListTimeMarketTimeDependentBase<ChannelGrabberOrderItem>
	{
		public ChannelGrabberOrdersList(
			DateTime submittedDate,
			IEnumerable<ChannelGrabberOrderItem> collection = null
		) : base(submittedDate, collection)
		{} // constructor

		public override ReceivedDataListTimeDependentBase<ChannelGrabberOrderItem> Create(
			DateTime submittedDate,
			IEnumerable<ChannelGrabberOrderItem> collection
		) {
			return new ChannelGrabberOrdersList(submittedDate, collection);
		} // Create
	} // class ChannelGrabberOrdersList
} // namespace