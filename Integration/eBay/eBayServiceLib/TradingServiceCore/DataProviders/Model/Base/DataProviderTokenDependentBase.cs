using System;
using System.Diagnostics;
using System.Linq;
using System.Web.Services.Protocols;
using EZBob.DatabaseLib;
using EzBob.CommonLib;
using EzBob.eBayServiceLib.Common;
using EzBob.eBayServiceLib.TradingServiceCore.DataInfos;
using EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Data;
using EzBob.eBayServiceLib.TradingServiceCore.TokenProvider;
using EzBob.eBayServiceLib.com.ebay.developer.soap;

namespace EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Model.Base
{
	public abstract class DataProviderTokenDependentBase : IDataProvider
	{
		private static readonly string[] _CommonInternalErrors;

		protected DataProviderCreationInfo Info { get; private set; }
		private readonly string _ApiVersion = "757";

		private CallProcedureInfo _CallProcedureInfo;

		static DataProviderTokenDependentBase()
		{
			// http://developer.ebay.com/DevZone/XML/docs/WebHelp/wwhelp/wwhimpl/common/html/wwhelp.htm?context=eBay_XML_API&file=InvokingWebServices-Error_Handling.html
			// Also, sometimes errors may occur due to problems on eBay's side. For example, error 10007 ("Internal error to the application") indicates an error on the eBay server side, not an error in your application.

			// http://developer.ebay.com/devzone/xml/docs/Reference/eBay/Errors/ErrorMessages.htm
			_CommonInternalErrors = new[]
			{
				//  "10007" - Internal error to the application.
				"10007",
				// "16100" -The requested data is currently not available due to an eBay system error. Please try again later.
				"16100",
				// "16101"- The data returned from eBay may be incomplete due to an eBay system error. Please try again later to get the complete data.
				"16101",
				// "17104" - eBay is experiencing system issues. Please try again later.
				"17104",
				// "17108" - eBay is experiencing system issues. Please try again later.
				"17108",
				// "14005" - Web Service framework internal error. replaceable_value.
				"14005",
				// "946" -  eBay is experiencing system issues. Please try again later.
				"946",
				// "21428" - An internal error occurred.
				"21428",
				// "21425" -  Internal error.
				"21425"				
			};
		}

		protected DataProviderTokenDependentBase( DataProviderCreationInfo info )			
		{
			Info = info;

			if ( info.ServiceTokenProvider == null )
			{
				info.ServiceTokenProvider = new ServiceTokenProviderEmpty();
			}

			Debug.Assert( info.ServiceProvider.ServiceType == EbayServiceType.Trading );
			var sp = info.ServiceProvider as IEbayServiceProvider<eBayAPIInterfaceService>;
			Service = sp.GetService( CallProcedureName, ApiVersion, info.ServiceTokenProvider );
		}

		public abstract CallProcedureType CallProcedureType { get; }

		public string CallProcedureName
		{
			get { return CallProcedureInfo.ServiceName; }
		}

		public string CallProcedureDisplayName
		{
			get { return CallProcedureInfo.DisplayName; }
		}

		public string CallProcedureDescription
		{
			get { return CallProcedureInfo.Description; }
		}

		public bool IsTokenDependent
		{
			get { return CallProcedureType.IsTokenDependent; }
		}

		public ServiceVersion ApiVersion
		{
			get { return new ServiceVersion(_ApiVersion); }
		}

		protected CallProcedureInfo CallProcedureInfo
		{
			get { return _CallProcedureInfo ?? ( _CallProcedureInfo = CallProcedureHelper.CreateInfo( CallProcedureType ) ); }
		}

		protected eBayAPIInterfaceService Service { get; private set; }

		protected R GetServiceData<T, R>( Func<T, R> func, T request )
			where T : AbstractRequestType
			where R : AbstractResponseType
		{
			request.Version = ApiVersion.Value;

			var errorRetryingInfo = Info.Settings == null ? null : Info.Settings.ErrorRetryingInfo;
			var retryingController = new WaitBeforeRetryController( new ErrorRetryingWaiter(), errorRetryingInfo );

			return retryingController.Do( () => 
				{
					WriteLog( string.Format( "Request to eBay: {0} started", CallProcedureName ) );
					R response;
					try
					{
						response = func( request );
					}
					catch ( SoapException ex)
					{
						ServiceFaultDetail fault;

						if ( ServiceRequestException.TryExtractErrorMessage( ex, out fault ) )
						{
							throw ServiceRequestExceptionFactory.Create( fault, ex );
						}

						throw;
					}

					if (response.Ack == AckCodeType.Failure)
					{
						throw ServiceRequestExceptionFactory.Create( new AmazonServiceResponceExceptionWrapper( response ) );
					}

					WriteLog( string.Format( "Request to eBay: {0} ended successfuly", CallProcedureName ) );
					return response;
				},
				ex =>
				{
					WriteLog( string.Format( "Request to eBay: {0} ended with error", CallProcedureName ), ex );

					if ( ex is IServiceRequestException )
					{
						var exFail = ex as IServiceRequestException;						
						return _CommonInternalErrors.Any( exFail.HasErrorWithCode );
					}					

					return true;

				} );						
		}

		private void WriteLog( string message, Exception ex = null)
		{
			WriteLoggerHelper.Write( message, WriteLogType.Info, null, ex );
			Debug.WriteLine( message );
		}
	}

	internal class AmazonServiceResponceExceptionWrapper : ServiceResponceExceptionWrapperBase
	{
		public AmazonServiceResponceExceptionWrapper(AbstractResponseType response)
		{
			BaseException = new FailServiceRequestException( response );

			if (response.Errors != null && response.Errors.Length != 0)
			{
				Errors = response.Errors.Select( err => new ServiceErrorType
															{
																ErrorCode = err.ErrorCode,
																LongMessage = err.LongMessage,
																SeverityCode = err.SeverityCode.ToString()
															} ).ToArray();
			}
		}
	}

}
