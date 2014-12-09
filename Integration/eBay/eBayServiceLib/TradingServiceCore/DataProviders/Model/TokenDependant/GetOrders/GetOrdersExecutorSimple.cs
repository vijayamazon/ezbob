using System.Diagnostics;
using EzBob.CommonLib;
using EzBob.eBayServiceLib.Common;
using EzBob.eBayServiceLib.TradingServiceCore.DataInfos.Orders;
using EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Model.Base;
using EzBob.eBayServiceLib.TradingServiceCore.ResultInfos.Orders;
using EzBob.eBayServiceLib.TradingServiceCore.TokenProvider;
using EzBob.eBayServiceLib.com.ebay.developer.soap;

namespace EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Model.TokenDependant.GetOrders
{
	class GetOrdersExecutorSimple : GetOrdersExecutorBase
	{
		private readonly ParamsDataInfoGetOrdersSimple _ParamsDataInfoGetOrdersSimple;

		public GetOrdersExecutorSimple(ParamsDataInfoGetOrdersSimple paramsDataInfoGetOrdersSimple, DataProviderCreationInfo info) 
			: base(info)
		{
			_ParamsDataInfoGetOrdersSimple = paramsDataInfoGetOrdersSimple;		
		}

		public override ResultInfoOrders GetOrders(RequestsCounterData requestsCounter)
		{
			var response = GetOrders( null, requestsCounter );

			var orders = new ResultDataByResponseOrders( response );
			var rez = new ResultInfoOrders(orders);

			var countOrders = orders.CountOrders;
			if ( countOrders == 0 )
			{
				WriteToLog( "no data" );
				return rez;
			}
			else
			{
				WriteToLog( "page {0,3} of {1,3}: {2}", 1, response.PaginationResult.TotalNumberOfPages, countOrders );
			}

			if ( response.HasMoreOrdersSpecified && response.HasMoreOrders )
			{
				var pages = response.PaginationResult.TotalNumberOfPages;
				for ( int i = 1; i < pages; i++ )
				{
					var data = new ResultDataByResponseOrders( GetOrders( i + 1, requestsCounter ) );
					rez.AddData( data );
					WriteToLog( "page {0,3} of {1,3}: {2}", i + 1, response.PaginationResult.TotalNumberOfPages, data.CountOrders );
				}
			}			
			return rez;
		}

		private GetOrdersResponseType GetOrders(int? pageNumber, RequestsCounterData requestsCounter)
		{
			var req = new GetOrdersRequestType
						{
							NumberOfDays = _ParamsDataInfoGetOrdersSimple.CountDays,
							NumberOfDaysSpecified = true,
							Pagination = new PaginationType
							{
								EntriesPerPage = 200,
								EntriesPerPageSpecified = true,
								PageNumber = pageNumber.HasValue ? pageNumber.Value : 0,
								PageNumberSpecified = pageNumber.HasValue
							}
						};

			return GetOrders(req, requestsCounter);
		}

	}
}
