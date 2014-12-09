using System;
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
	internal class GetOrdersExecutorFromDateToDateModified : GetOrdersExecutorFromDateToDateCreated
	{
		private int MAX_RETURN_DAYS = 30;

		public GetOrdersExecutorFromDateToDateModified(ParamsDataInfoGetOrdersFromDateToDateBase param, DataProviderCreationInfo info) 
			: base(param, info)
		{
		}

		public override ResultInfoOrders GetOrders(RequestsCounterData requestsCounter)
		{
			var now = DateTime.Now;

			var diffDays = (int)now.Subtract( FromDate ).TotalDays;

			WriteToLog( "---------------------------------" );
			WriteToLog( "{0:g} - {1:g}", FromDate, ToDate );
			WriteToLog( "Count Days: {0}", diffDays );
			if ( diffDays <= MAX_RETURN_DAYS )
			{
				WriteToLog( "all." );
				var orders = GetOrders( FromDate, ToDate, requestsCounter );
				WriteToLog( "---------------------------------" );
				return orders;
			}

			int rem;
			var iter = Math.DivRem( diffDays, MAX_RETURN_DAYS, out rem );

			WriteToLog( "Count Iterations: {0}", iter );
			ResultInfoOrders rez = null;

			for ( int i = 0; i < iter; i++ )
			{
				DateTime fromDate = i == 0 ? FromDate : FromDate.AddDays( i * MAX_RETURN_DAYS );

				DateTime toDate = FromDate.AddDays( ( i + 1 ) * MAX_RETURN_DAYS );

				WriteToLog( "iter: {0,3}. {1:g} - {2:g}", i + 1, fromDate, toDate );
				var orders = GetOrders( fromDate, toDate, requestsCounter );

				if ( rez == null )
				{
					rez = new ResultInfoOrders( orders );
				}
				else
				{
					rez.AddData(orders);
				}
			}

			if ( rem > 0 )
			{
				DateTime fromDate = FromDate.AddDays( iter * MAX_RETURN_DAYS );
				DateTime toDate = ToDate;
				WriteToLog( "remainder. {0:g} - {1:g}", fromDate, toDate );
				var orders = GetOrders( fromDate, toDate, requestsCounter );

				if ( rez == null )
				{
					rez = new ResultInfoOrders( orders );
				}
				else
				{
					rez.AddData( orders );
				}
			}
			WriteToLog( "---------------------------------" );

			return rez;
		}

		private ResultInfoOrders GetOrders( DateTime fromDate, DateTime toDate, RequestsCounterData requestsCounter )
		{
			var response = GetOrders( fromDate, toDate, null, requestsCounter );

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
					var data = new ResultDataByResponseOrders( GetOrders( fromDate, toDate, i + 1, requestsCounter ) );
					rez.AddData( data );
					WriteToLog( "page {0,3} of {1,3}: {2}", i + 1, response.PaginationResult.TotalNumberOfPages, data.CountOrders );
				}
			}

			return rez;
		}

		private GetOrdersResponseType GetOrders( DateTime fromDate, DateTime toDate, int? pageNumber, RequestsCounterData requestsCounter )
		{
			var req = new GetOrdersRequestType
			{
				ModTimeFrom = fromDate.ToUniversalTime(),
				ModTimeFromSpecified = true,
				ModTimeTo = toDate.ToUniversalTime(),
				ModTimeToSpecified = true,
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
