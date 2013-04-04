using System;
using System.Collections.Generic;
using EZBob.DatabaseLib.Common;
using EzBob.CommonLib;
using EzBob.CommonLib.ReceivedDataListLogic;
using EzBob.CommonLib.TimePeriodLogic;

namespace EZBob.DatabaseLib.DatabaseWrapper.Order
{
	public class EkmOrdersList : ReceivedDataListTimeMarketTimeDependentBase<EkmOrderItem>
	{
		public EkmOrdersList(DateTime submittedDate, IEnumerable<EkmOrderItem> collection = null) 
			: base(submittedDate, collection)
		{
		}

		public override ReceivedDataListTimeDependentBase<EkmOrderItem> Create(DateTime submittedDate, IEnumerable<EkmOrderItem> collection)
		{
			return new EkmOrdersList(submittedDate, collection);
		}
	}

	public class EkmOrderItem : TimeDependentRangedDataBase
	{
        public string EkmOrderId { get; set; }
        public string OrderNumber { get; set; }
        public string CustomerID { get; set; }
        public string CompanyName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public decimal TotalCost { get; set; }
        public string OrderDate { get; set; }
        public DateTime OrderDateIso { get; set; }

        public string OrderStatusColour { get; set; }

		public override DateTime RecordTime
		{
			get { return OrderDateIso; }
		}
	}
}