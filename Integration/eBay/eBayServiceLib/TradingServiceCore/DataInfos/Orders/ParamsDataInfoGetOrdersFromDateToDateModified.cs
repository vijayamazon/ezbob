using System;

namespace EzBob.eBayServiceLib.TradingServiceCore.DataInfos.Orders
{
	public class ParamsDataInfoGetOrdersFromDateToDateModified : ParamsDataInfoGetOrdersFromDateToDateBase
	{
		public ParamsDataInfoGetOrdersFromDateToDateModified( DateTime fromDate, DateTime toDate )
			: base( fromDate, toDate )
		{
		}

		public override ParamsDataInfoGetOrdersParamsType Type
		{
			get { return ParamsDataInfoGetOrdersParamsType.FromDateToDateModified; }
		}
	}
}