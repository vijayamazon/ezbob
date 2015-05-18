namespace EzBob.AmazonServiceLib.Common
{
	using System;
	using System.Diagnostics;
	using System.Net;
	using CommonLib;
	using MarketplaceWebService;
	using MarketplaceWebServiceOrders;
	using MarketplaceWebServiceProducts;

	class AmazonWaitBeforeRetryHelper
	{
		private readonly WaitBeforeRetryController _WaitBeforeRetryController;
		private static readonly string _RequestThrottledString = "RequestThrottled";

		private AmazonWaitBeforeRetryHelper(ErrorRetryingInfo errorRetryingInfo)
		{		
			_WaitBeforeRetryController = new WaitBeforeRetryController( new ErrorRetryingWaiter(), errorRetryingInfo );
		}

		public static T DoServiceAction<T>( ErrorRetryingInfo errorRetryingInfo, ITrapForThrottling trapForThrottling, string actionName, ActionAccessType access, RequestsCounterData requestCounter, Func<T> func, string details = null )
		{
			return new AmazonWaitBeforeRetryHelper( errorRetryingInfo ).DoServiceAction( trapForThrottling, actionName, access, requestCounter, func, details );
		}

		private T DoServiceAction<T>(ITrapForThrottling trapForThrottling, string actionName, ActionAccessType access, RequestsCounterData requestCounter, Func<T> func, string details)
		{
			return _WaitBeforeRetryController.Do( () =>
				{
					T response = default( T );

					trapForThrottling.Execute( new ActionInfo( actionName )
					{
						Action = () =>
						{
							response = func();
							requestCounter.IncrementRequests( actionName, details );
						},
						Access = access
					} );

					return response;
				},
				ex =>
				{
					if (ex != null)
					{
						WriteLoggerHelper.Write(ex.Message, WriteLogType.Warning, null, ex);
						Debug.WriteLine(ex);
					}

					if ( ex is MarketplaceWebServiceProductsException )
					{
						var prodEx = ex as MarketplaceWebServiceProductsException;
						if ( prodEx.StatusCode == HttpStatusCode.InternalServerError ||
							prodEx.StatusCode == HttpStatusCode.ServiceUnavailable ||
							string.Equals( prodEx.ErrorCode, _RequestThrottledString, StringComparison.InvariantCultureIgnoreCase ) )
						{
							return true;
						}
						else if (prodEx.StatusCode == HttpStatusCode.Unauthorized || prodEx.StatusCode == HttpStatusCode.BadRequest)
						{
							return false;
						}
					} else if (ex is MarketplaceWebServiceOrdersException)
					{
						var ordersEx = ex as MarketplaceWebServiceOrdersException;
						if ( ordersEx.StatusCode == HttpStatusCode.InternalServerError ||
							ordersEx.StatusCode == HttpStatusCode.ServiceUnavailable ||
							string.Equals( ordersEx.ErrorCode, _RequestThrottledString, StringComparison.InvariantCultureIgnoreCase ) )
						{
							return true;
						}
						else if (ordersEx.StatusCode == HttpStatusCode.Unauthorized)
						{
							return false;
						}
					}
					else if ( ex is MarketplaceWebServiceException )
					{
						var repEx = ex as MarketplaceWebServiceException;

						if ( repEx.StatusCode == HttpStatusCode.InternalServerError ||
							repEx.StatusCode == HttpStatusCode.ServiceUnavailable ||
							string.Equals( repEx.ErrorCode, _RequestThrottledString, StringComparison.InvariantCultureIgnoreCase ) )
						{
							return true;
						}
						else if (repEx.StatusCode == HttpStatusCode.Unauthorized)
						{
							return false;
						}
					}

					return true;
				} 
			);			
		}
	}
}
