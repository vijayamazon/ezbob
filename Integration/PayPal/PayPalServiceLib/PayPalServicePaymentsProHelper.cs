namespace EzBob.PayPalServiceLib {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Reflection;
	using System.ServiceModel;
	using System.Text;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.DatabaseWrapper.Transactions;
	using CommonLib;
	using PayPalDbLib.Models;
	using Common;
	using com.paypal.service;
	using StructureMap;
	using ConfigManager;

	internal class PayPalServicePaymentsProHelper {
		private readonly string Version = "117";

		private readonly ServiceUrlsInfo _ConnectionInfo;
		private static string[] _CommonInternalErrors;

		static PayPalServicePaymentsProHelper() {
			// https://cms.paypal.com/es/cgi-bin/?cmd=_render-content&content_ID=developer/e_howto_api_soap_errorcodes
			// GetTransactionDetails API Errors
			_CommonInternalErrors = new[]
			{
				"10001", // 10001 Internal Error
			};
		}

		public PayPalServicePaymentsProHelper() {
			var factory = ObjectFactory.GetInstance<IServiceEndPointFactory>();
			_ConnectionInfo = factory.Create(PayPalServiceType.WebServiceThreeToken);
		}

		private PayPalAPIInterfaceClient CreateService(PayPalRequestInfo reqInfo) {
			int openTimeOutInMinutes = reqInfo.OpenTimeOutInMinutes;
			int sendTimeout = reqInfo.SendTimeoutInMinutes;

			var binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);
			binding.MaxReceivedMessageSize = 2147483647;
			binding.MaxBufferSize = 2147483647;
			binding.OpenTimeout = new TimeSpan(0, openTimeOutInMinutes, 0);
			binding.ReceiveTimeout = new TimeSpan(0, 20, 0);

			binding.SendTimeout = new TimeSpan(0, sendTimeout, 0);
			var addr = new EndpointAddress(_ConnectionInfo.ServiceEndPoint);

			return new PayPalAPIInterfaceClient(binding, addr);
		}

		private List<Tuple<DateTime, DateTime>> GetDailyRanges(DateTime startDate, DateTime endDate, int daysPerRequest) {
			var rez = new List<Tuple<DateTime, DateTime>>();

			DateTime fromDate = startDate;
			DateTime toDate = fromDate.AddDays(daysPerRequest);
			while (true) {
				if (toDate >= endDate) {
					rez.Add(new Tuple<DateTime, DateTime>(fromDate, endDate));
					break;
				}

				rez.Add(new Tuple<DateTime, DateTime>(fromDate, toDate));
				fromDate = toDate;
				toDate = toDate.AddDays(daysPerRequest);
				fromDate = fromDate.AddSeconds(1);
			}

			return rez;
		}

		public RequestsCounterData GetTransactionData(PayPalRequestInfo reqInfo, Func<List<PayPalTransactionItem>, bool> func) {
			DateTime startDate = reqInfo.StartDate;
			DateTime endDate = reqInfo.EndDate;
			var requestsCounter = new RequestsCounterData();

			var ranges = GetDailyRanges(startDate, endDate, reqInfo.DaysPerRequest);

			int counter = 0;
			int daysFailedSoFar = 0;

			foreach (var range in ranges) {
				++counter;
				var fromDate = range.Item1;
				var toDate = range.Item2;
				bool hasMoreItems;

				do {
					WriteLog(string.Format("Request Transactions {0} of {1} ({2}): [{3} - {4}]", counter, ranges.Count, reqInfo.SecurityInfo.UserId, fromDate, toDate));
					TransactionSearchResponseType resp = GetTransactions(fromDate, toDate, reqInfo, requestsCounter, ref daysFailedSoFar);
					WriteLog(string.Format("Result Request Transactions {0} of {1} ({2}): [{3} - {4}]: {5}", counter, ranges.Count, reqInfo.SecurityInfo.UserId, fromDate, toDate, resp == null || resp.PaymentTransactions == null ? 0 : resp.PaymentTransactions.Length));

					if (resp == null) {
						break;
					}

					List<PayPalTransactionItem> list;
					if (!TryParseData(resp, out list)) {
						break;
					}

					if (!func(list))
						break;


					toDate = list.AsParallel().Min(x => x.Created);

					hasMoreItems = resp.Ack == AckCodeType.SuccessWithWarning && resp.Errors.Any(e => e.ErrorCode == "11002");

				}
				while (hasMoreItems);
			}

			return requestsCounter;
		}

		private void WriteLog(string message, Exception ex = null) {
			WriteLoggerHelper.Write(message, WriteLogType.Info, null, ex);
			Debug.WriteLine(message);
		}

		private TransactionSearchResponseType GetTransactions(DateTime startDate, DateTime endDate, PayPalRequestInfo reqInfo, RequestsCounterData requestsCounter, ref int daysFailedSoFar) {
			var request = new TransactionSearchReq {
				TransactionSearchRequest = new TransactionSearchRequestType {
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

			while (needToRetry && counter <= CurrentValues.Instance.PayPalNumberOfRetries) {
				counter++;
				try {
					var cred = CreateCredentials(reqInfo.SecurityInfo);
					WriteLog(string.Format("PayPalService TransactionSearch Starting ({0})", userId));
					var service = CreateService(reqInfo);
					TransactionSearchResponseType resp = service.TransactionSearch(ref cred, request);

					WriteLog(string.Format("PayPalService TransactionSearch Request:\n{0}\nResponse:\n{1}", GetLogFor(request.TransactionSearchRequest), GetLogFor(resp)));

					requestsCounter.IncrementRequests("TransactionSearch");
					if (resp.Ack == AckCodeType.Failure) {
						throw ServiceRequestExceptionFactory.Create(new PayPalServiceResponceExceptionWrapper(resp));
					}
					WriteLog(string.Format("PayPalService TransactionSearch Ended ({0})", userId));

					return resp;
				} catch (Exception ex) {
					lastEx = ex;
					if (ex is IServiceRequestException) {
						var exFail = ex as IServiceRequestException;
						needToRetry = _CommonInternalErrors.Any(exFail.HasErrorWithCode);
					}

					WriteLoggerHelper.Write(string.Format("PayPalService TransactionSearch Error ({0}): need to retry: {2} \n {1} ", userId, ex.Message, needToRetry), WriteLogType.Error, null, ex);
				}
			}

			WriteLoggerHelper.Write(string.Format("Failed fetching pay pal data from {0} to {1}", request.TransactionSearchRequest.StartDate, request.TransactionSearchRequest.EndDate), WriteLogType.Info, null, lastEx);

			if (daysFailedSoFar == CurrentValues.Instance.PayPalMaxAllowedFailures) {
				WriteLoggerHelper.Write(string.Format("Max number of failures:{0} exceeded.", daysFailedSoFar), WriteLogType.Error, null, lastEx);
				throw lastEx ?? new Exception();
			}

			daysFailedSoFar++;

			return null;
		}

		private bool TryParseData(TransactionSearchResponseType resp, out List<PayPalTransactionItem> result) {
			result = new List<PayPalTransactionItem>();

			if (resp == null || resp.PaymentTransactions == null || resp.PaymentTransactions.Length <= 0) {
				return false;
			}

			result.AddRange(resp.PaymentTransactions.Where(p => p != null).Select(p => new PayPalTransactionItem {
				Created = p.Timestamp.ToUniversalTime(),
				Timezone = p.Timezone,
				Type = p.Type,
				Status = p.Status,
				FeeAmount = ConvertToAmountInfo(p.FeeAmount),
				GrossAmount = ConvertToAmountInfo(p.GrossAmount),
				NetAmount = ConvertToAmountInfo(p.NetAmount),
				TransactionId = p.TransactionID
			}));

			return true;
		}

		private static AmountInfo ConvertToAmountInfo(BasicAmountType data) {
			if (data == null)
				return null;
			double val;
			double.TryParse(data.Value, out val);
			return new AmountInfo {
				CurrencyCode = data.currencyID.ToString(),
				Value = val
			};
		}

		private CustomSecurityHeaderType CreateCredentials(PayPalSecurityData securityInfo) {
			return new CustomSecurityHeaderType {
				Credentials = new UserIdPasswordType {
					Username = CurrentValues.Instance.PayPalApiUsername,
					Password = CurrentValues.Instance.PayPalApiPassword,
					Signature = CurrentValues.Instance.PayPalApiSignature,
					Subject = securityInfo.UserId,
					AppId = CurrentValues.Instance.PayPalPpApplicationId
				}
			};
		}

		public static string GetLogFor(object target) {
			var properties =
                from property in target.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
				select new {
					Name = property.Name,
					Value = property.GetValue(target, null)
				};

			var builder = new StringBuilder();

			foreach (var property in properties) {
				builder
					.Append(property.Name)
					.Append(" = ")
					.Append(property.Value)
					.AppendLine();
			}

			return builder.ToString();
		}
	}

	internal class PayPalServiceResponceExceptionWrapper : ServiceResponceExceptionWrapperBase {
		public PayPalServiceResponceExceptionWrapper(AbstractResponseType response) {
			BaseException = new ServiceResponceException(response);

			if (response.Errors != null && response.Errors.Length != 0) {
				Errors = response.Errors.Select(err => new ServiceErrorType {
					ErrorCode = err.ErrorCode,
					LongMessage = err.LongMessage,
					SeverityCode = err.SeverityCode.ToString()
				}).ToArray();
			}
		}
	}
}