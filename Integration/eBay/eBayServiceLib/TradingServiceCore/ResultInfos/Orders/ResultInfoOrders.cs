using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EzBob.eBayServiceLib.TradingServiceCore.DataInfos;
using EzBob.eBayServiceLib.com.ebay.developer.soap;

namespace EzBob.eBayServiceLib.TradingServiceCore.ResultInfos.Orders
{
	public class ResultInfoOrders : EbayResultInfoBase, IEnumerable<OrderType>
	{
		private readonly List<OrderType> _List = new List<OrderType>();

		public ResultInfoOrders( DateTime submittedDate )
			:base(submittedDate)
		{
		}

		public ResultInfoOrders( ResultDataByResponseOrders data )
			:base(data.SubmittedDate)
		{
			AddData( data );
		}

		public ResultInfoOrders( ResultInfoOrders data )
			: base( data.SubmittedDate )
		{
			AddData( data );
		}

		public ResultInfoOrders( GetOrdersResponseType data )
			: this( new ResultDataByResponseOrders( data ) )
		{			
		}

		public void AddData( ResultInfoOrders data )
		{			
			if ( data == null )
			{
				return;
			}

			if ( data.Orders != null && data.Orders.Any() )
			{
				AddOrders( data.Orders );
			}

			if ( data.Errors != null && data.Errors.Length > 0 )
			{
				AddErrors( data.Errors );
			}
		}

		public void AddData( ResultDataByResponseOrders data )
		{
			if ( data == null )
			{
				return;
			}

			if ( data.Orders != null && data.Orders.Any() )
			{
				AddOrders(data.Orders);
			}

			if ( data.Errors != null && data.Errors.Length > 0 )
			{
				AddErrors(data.Errors);
			}
		}

		private void AddOrders( IEnumerable<OrderType> orders )
		{
			_List.AddRange(orders);
		}

		public override DataInfoTypeEnum DataInfoType
		{
			get { return DataInfoTypeEnum.OrdersInfoList; }
		}

		public List<OrderType> Orders
		{
			get { return _List; }
		}

		public void AddData( GetOrdersResponseType data )
		{
			AddData( new ResultDataByResponseOrders( data ) );
		}

		public IEnumerator<OrderType> GetEnumerator()
		{
			return _List.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}