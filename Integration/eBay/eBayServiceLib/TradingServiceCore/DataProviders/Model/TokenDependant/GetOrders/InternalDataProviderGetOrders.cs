using EzBob.CommonLib;
using EzBob.eBayServiceLib.Common;
using EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Data;
using EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Model.Base;
using EzBob.eBayServiceLib.TradingServiceCore.TokenProvider;
using EzBob.eBayServiceLib.com.ebay.developer.soap;

namespace EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Model.TokenDependant.GetOrders
{
	class InternalDataProviderGetOrders : DataProviderTokenDependentBase
	{
		public InternalDataProviderGetOrders(DataProviderCreationInfo info) 
			: base(info)
		{
		}

		public override CallProcedureType CallProcedureType
		{
			get { return CallProcedureTypeTokenDependent.GetOrders; }
		}

		public GetOrdersResponseType GetOrders( GetOrdersRequestType request, RequestsCounterData requestsCounter )
		{
			var details = string.Empty;
			if ( request.NumberOfDaysSpecified )
			{
				details = string.Format( "Num od Days: {0}", request.NumberOfDays );
			}
			else if ( request.ModTimeFromSpecified )
			{
				details = string.Format( "[ {0} - {1} ]", request.ModTimeFrom, request.ModTimeTo );
			}
			else if ( request.CreateTimeFromSpecified )
			{
				details = string.Format( "[ {0} - {1} ]", request.CreateTimeFrom, request.CreateTimeTo );
			}

			var response = base.GetServiceData( Service.GetOrders, request );
			requestsCounter.IncrementRequests( "GetOrders", details );
			return response;
		}

	}
}