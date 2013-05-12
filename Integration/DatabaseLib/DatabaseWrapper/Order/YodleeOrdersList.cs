using System;
using System.Collections.Generic;
using EZBob.DatabaseLib.Common;
using EzBob.CommonLib;
using EzBob.CommonLib.ReceivedDataListLogic;
using EzBob.CommonLib.TimePeriodLogic;

namespace EZBob.DatabaseLib.DatabaseWrapper.Order
{
	public class YodleeOrdersList : ReceivedDataListTimeMarketTimeDependentBase<YodleeOrderItem>
	{
		public YodleeOrdersList(DateTime submittedDate, IEnumerable<YodleeOrderItem> collection = null) 
			: base(submittedDate, collection)
		{
		}

		public override ReceivedDataListTimeDependentBase<YodleeOrderItem> Create(DateTime submittedDate, IEnumerable<YodleeOrderItem> collection)
		{
			return new YodleeOrdersList(submittedDate, collection);
		}
	}

	public class YodleeOrderItem : TimeDependentRangedDataBase
	{
        //public int? YodleeOrderId { get; set; }
        //public string OrderNumber { get; set; }
        //public int? CustomerID { get; set; }
        //public string CompanyName { get; set; }
        //public string FirstName { get; set; }
        //public string LastName { get; set; }
        //public string EmailAddress { get; set; }
        //public double? TotalCost { get; set; }
        //public string OrderStatus { get; set; }
        //public DateTime OrderDateIso { get; set; }
        //public DateTime OrderDate { get; set; }
        //public string OrderStatusColour { get; set; }
        //TODO: add real fields
		public override DateTime RecordTime
		{
			get { return new DateTime(); }
		}
	}
}