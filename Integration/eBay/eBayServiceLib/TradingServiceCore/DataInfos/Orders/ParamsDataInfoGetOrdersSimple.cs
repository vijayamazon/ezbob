using System.Diagnostics;

namespace EzBob.eBayServiceLib.TradingServiceCore.DataInfos.Orders
{
	public class ParamsDataInfoGetOrdersSimple : ParamsDataInfoGetOrdersBase
	{
		public ParamsDataInfoGetOrdersSimple(byte countDays = 30)
		{
			Debug.Assert( countDays <= 30 && countDays > 0, @"Long Message: You have exceeded the 30 day maximum time window allowed" );
			if ( countDays > 30 )
			{
				countDays = 30;
			}
			CountDays = countDays;
		}

		public override bool HasData
		{
			get { return true; }
		}

		public override ParamsDataInfoGetOrdersParamsType Type
		{
			get { return ParamsDataInfoGetOrdersParamsType.Simple; }
		}

		public int CountDays { get; private set; }
	}
}