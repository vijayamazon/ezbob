using System;

namespace EzBob.eBayServiceLib.TradingServiceCore.DataInfos.Orders
{
	public class ParamsDataInfoGetOrdersFromDateToDateCreated : ParamsDataInfoGetOrdersFromDateToDateBase
	{
		public ParamsDataInfoGetOrdersFromDateToDateCreated(DateTime fromDate, DateTime toDate) 
			: base(fromDate, toDate)
		{
		}

		public override ParamsDataInfoGetOrdersParamsType Type
		{
			get { return ParamsDataInfoGetOrdersParamsType.FromDateToDateCreated; }
		}
	}
}