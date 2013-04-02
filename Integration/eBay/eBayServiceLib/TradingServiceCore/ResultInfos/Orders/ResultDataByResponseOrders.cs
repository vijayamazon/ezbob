using System.Collections.Generic;
using System.Linq;
using EzBob.eBayServiceLib.TradingServiceCore.DataInfos;
using EzBob.eBayServiceLib.com.ebay.developer.soap;

namespace EzBob.eBayServiceLib.TradingServiceCore.ResultInfos.Orders
{
	public class ResultDataByResponseOrders : ResultInfoByServerResponseBase
	{
		private readonly GetOrdersResponseType _Response;
		private readonly List<OrderType> _Orders;

		public ResultDataByResponseOrders( GetOrdersResponseType response ) 
			: base(response)
		{
			_Response = response;
			_Orders = new List<OrderType>( _Response.OrderArray );
		}

		public override DataInfoTypeEnum DataInfoType
		{
			get { return DataInfoTypeEnum.OrdersInfo; }
		}

		public List<OrderType> Orders
		{
			get
			{
				return _Orders;
			}
		}

		public int CountOrders
		{
			get { return _Orders.Count(); }
		}
	}
}