using System;
using System.Collections.Generic;
using EZBob.DatabaseLib.Common;
using EzBob.CommonLib;
using EzBob.CommonLib.ReceivedDataListLogic;
using EzBob.CommonLib.TimePeriodLogic;

namespace EZBob.DatabaseLib.DatabaseWrapper.Order
{
	public class PayPointOrdersList : ReceivedDataListTimeMarketTimeDependentBase<PayPointOrderItem>
	{
        public PayPointOrdersList(DateTime submittedDate, IEnumerable<PayPointOrderItem> collection = null) 
			: base(submittedDate, collection)
		{
		}

        public override ReceivedDataListTimeDependentBase<PayPointOrderItem> Create(DateTime submittedDate, IEnumerable<PayPointOrderItem> collection)
		{
            return new PayPointOrdersList(submittedDate, collection);
		}
	}
}