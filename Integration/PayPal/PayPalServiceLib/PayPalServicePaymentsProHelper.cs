using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper.Transactions;
using EzBob.CommonLib;
using EzBob.PayPalDbLib.Models;
using EzBob.PayPalServiceLib.Common;
using EzBob.PayPalServiceLib.com.paypal.service;
using StructureMap;

namespace EzBob.PayPalServiceLib
{
	internal class PayPalServicePaymentsProHelper
	{
		private readonly string Version = "94";

		private readonly IPayPalConfig _Config;
		private readonly ServiceUrlsInfo _ConnectionInfo;
		private static string[] _CommonInternalErrors;

		static PayPalServicePaymentsProHelper()
		{
			// https://cms.paypal.com/es/cgi-bin/?cmd=_render-content&content_ID=developer/e_howto_api_soap_errorcodes
			// GetTransactionDetails API Errors
			_CommonInternalErrors = new[]
			{
				// 10001 Internal Error
				"10001",
			};
		}

		public PayPalServicePaymentsProHelper(IPayPalConfig config)
		{
			_Config = config;
			var factory = ObjectFactory.GetInstance<IServiceEndPointFactory>();
			_ConnectionInfo = factory.Create( PayPalServiceType.WebServiceThreeToken, config.ServiceType );
		}

		private PayPalAPIInterfaceClient CreateService( PayPalRequestInfo reqInfo )
		{
			int openTimeOutInMinutes = reqInfo.OpenTimeOutInMinutes;
			int sendTimeout = reqInfo.SendTimeoutInMinutes;
			
			

			var binding = new BasicHttpBinding( BasicHttpSecurityMode.Transport );
			binding.MaxReceivedMessageSize = 2147483647;
			binding.MaxBufferSize = 2147483647;
			binding.OpenTimeout = new TimeSpan( 0, openTimeOutInMinutes, 0 );
			binding.ReceiveTimeout = new TimeSpan( 0, 20, 0 );
			
			binding.SendTimeout = new TimeSpan( 0, sendTimeout, 0 );
			var addr = new EndpointAddress( _ConnectionInfo.ServiceEndPoint );


			return new PayPalAPIInterfaceClient( binding, addr );			
		}

		private List<Tuple<DateTime, DateTime>> GetDailyRanges(DateTime startDate, DateTime endDate)
		{
			var rez = new List<Tuple<DateTime, DateTime>>();

			DateTime fromDate = startDate;
			DateTime toDate = fromDate.AddDays(1);
			while (true)
			{
				if (toDate >= endDate)
				{
					rez.Add(new Tuple<DateTime, DateTime>(fromDate, endDate));
					break;
				}

				rez.Add(new Tuple<DateTime, DateTime>(fromDate, toDate));
				fromDate = toDate;
				toDate = toDate.AddDays(1);
				fromDate = fromDate.AddSeconds(1);
			}

			return rez;
		}

		public PayPalTransactionsList GetTransactionData( PayPalRequestInfo reqInfo)
		{
			DateTime startDate = reqInfo.StartDate;
			DateTime endDate = reqInfo.EndDate;
			PayPalTransactionsList data = null;
			var requestsCounter = new RequestsCounterData();

			var ranges = GetDailyRanges(startDate, endDate);
			
			int counter = 0;

			foreach (var range in ranges)
			{
				++counter;
				var fromDate = range.Item1;
				var toDate = range.Item2;
				bool hasMoreItems;

				do
				{
					WriteLog( string.Format( "Request Transactions {0} of {1} ({2}): [{3} - {4}]", counter, ranges.Count, reqInfo.SecurityInfo.UserId, fromDate, toDate ) );
					TransactionSearchResponseType resp = GetTransactions(fromDate, toDate, reqInfo, requestsCounter);
					WriteLog( string.Format( "Result Request Transactions {0} of {1} ({2}): [{3} - {4}]: {5}", counter, ranges.Count, reqInfo.SecurityInfo.UserId, fromDate, toDate, resp == null || resp.PaymentTransactions == null ? 0 : resp.PaymentTransactions.Length ) );

					if (resp == null)
					{
						break;
					}

					if (data == null)
					{
						data = new PayPalTransactionsList(resp.Timestamp);
					}

					List<PayPalTransactionItem> list;
					if (!TryParseData(resp, out list))
					{						
						break;
					}					

					if (!data.TryAddNewData(list))
					{
						break;
					}

					toDate = list.AsParallel().Min(x => x.Created);

					hasMoreItems = resp.Ack == AckCodeType.SuccessWithWarning && resp.Errors.Any(e => e.ErrorCode == "11002");

				}
				while ( hasMoreItems );	
			}

			if ( data != null )
			{
				data.RequestsCounter = requestsCounter;
			}
			return data;
		}

		private void WriteLog(string message, Exception ex = null)
		{
			WriteLoggerHelper.Write( message, WriteLogType.Info, null, ex );
			Debug.WriteLine( message );
		}

		private TransactionSearchResponseType GetTransactions(DateTime startDate, DateTime endDate, PayPalRequestInfo reqInfo, RequestsCounterData requestsCounter)
		{
			var request = new TransactionSearchReq
			{
				TransactionSearchRequest = new TransactionSearchRequestType
				{
					Version = Version,
					StartDate = startDate.ToUniversalTime(),
					StatusSpecified = true,
					EndDate = endDate.ToUniversalTime(),
					EndDateSpecified = true,	
					DetailLevel = new[] { DetailLevelCodeType.ReturnAll },					
					Status = PaymentTransactionStatusCodeType.Success
				}
			};
			
			var userId = reqInfo.SecurityInfo.UserId;

			bool needToRetry = true;
			Exception lastEx = null;
			int counter = 0;

			while (needToRetry && counter <= _Config.NumberOfRetries)
			{
				counter++;
				try
				{
					TransactionSearchResponseType resp = null;
					var cred = CreateCredentials(reqInfo.SecurityInfo);
					WriteLog(string.Format("PayPalService TransactionSearch Starting ({0})", userId));
					var service = CreateService(reqInfo);
					resp = service.TransactionSearch(ref cred, request);

					WriteLog(string.Format("PayPalService TransactionSearch Request:\n{0}\nResponse:\n{1}", GetLogFor(request.TransactionSearchRequest), GetLogFor(resp)));

					requestsCounter.IncrementRequests("TransactionSearch");
					if (resp.Ack == AckCodeType.Failure)
					{
						throw ServiceRequestExceptionFactory.Create(new PayPalServiceResponceExceptionWrapper(resp));
					}
					WriteLog(string.Format("PayPalService TransactionSearch Ended ({0})", userId));

					return resp;
				}
				catch (Exception ex)
				{
					lastEx = ex;
					if (ex is IServiceRequestException)
					{
						var exFail = ex as IServiceRequestException;
						needToRetry = _CommonInternalErrors.Any(exFail.HasErrorWithCode);
					}

					WriteLog(string.Format("PayPalService TransactionSearch Error ({0}): {1} ", userId, ex.Message), ex);
				}
			}
			throw lastEx;
		}

		private bool TryParseData(TransactionSearchResponseType resp, out List<PayPalTransactionItem> result )
		{
			result = new List<PayPalTransactionItem>();

			if (resp == null || resp.PaymentTransactions == null || resp.PaymentTransactions.Length <= 0)
			{
				return false;
			}

			result.AddRange(resp.PaymentTransactions.Where(p => p != null).Select(p => new PayPalTransactionItem
				{
					Created = p.Timestamp.ToUniversalTime(),
					Timezone = p.Timezone,
					Type = p.Type,
					Status = p.Status,
					FeeAmount = ConvertToAmountInfo(p.FeeAmount),
					GrossAmount = ConvertToAmountInfo(p.GrossAmount),
					NetAmount = ConvertToAmountInfo(p.NetAmount),
					Payer = p.Payer,
					PayerDisplayName = p.PayerDisplayName,
					TransactionId = p.TransactionID
				}));

			return true;
		}			

		private static AmountInfo ConvertToAmountInfo(BasicAmountType data)
		{
            if (data == null) return null;
			double val;
			double.TryParse( data.Value, out val );
			return new AmountInfo
			       	{
			       		CurrencyCode = data.currencyID.ToString(),
			       		Value = val
			       	};
		}

		private CustomSecurityHeaderType CreateCredentials( PayPalSecurityData securityInfo )
		{
			return new CustomSecurityHeaderType
			       	{
			       		Credentials = new UserIdPasswordType
			       		              	{
			       		              		Username = _Config.ApiUsername,
			       		              		Password = _Config.ApiPassword,
			       		              		Signature = _Config.ApiSignature,
			       		              		Subject = securityInfo.UserId,
											AppId = _Config.PPApplicationId
											
			       		              	}
			       	};
		}

        public static string GetLogFor(object target)
        {
            var properties =
                from property in target.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                select new
                {
                    Name = property.Name,
                    Value = property.GetValue(target, null)
                };

            var builder = new StringBuilder();

            foreach (var property in properties)
            {
                builder
                    .Append(property.Name)
                    .Append(" = ")
                    .Append(property.Value)
                    .AppendLine();
            }

            return builder.ToString();
        }
	}

	internal class PayPalServiceResponceExceptionWrapper : ServiceResponceExceptionWrapperBase
	{
		public PayPalServiceResponceExceptionWrapper( AbstractResponseType response )
		{
			BaseException = new ServiceResponceException( response );

			if ( response.Errors != null && response.Errors.Length != 0 )
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