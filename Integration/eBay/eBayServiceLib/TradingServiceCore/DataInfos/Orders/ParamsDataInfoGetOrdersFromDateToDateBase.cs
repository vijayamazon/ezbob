using System;
using System.Diagnostics;

namespace EzBob.eBayServiceLib.TradingServiceCore.DataInfos.Orders
{
	public abstract class ParamsDataInfoGetOrdersFromDateToDateBase : ParamsDataInfoGetOrdersBase
	{
		protected ParamsDataInfoGetOrdersFromDateToDateBase(DateTime fromDate, DateTime toDate)
		{
			FromDate = fromDate;
			ToDate = toDate;
			Debug.Assert( fromDate < toDate );
		}

		public DateTime FromDate { get; private set; }
		public DateTime ToDate { get; private set; }

		public override bool HasData
		{
			get { return true; }
		}

	}
}
