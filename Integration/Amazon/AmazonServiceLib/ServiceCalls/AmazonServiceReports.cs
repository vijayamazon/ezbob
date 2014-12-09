namespace EzBob.AmazonServiceLib.ServiceCalls {
	using System;
	using System.Collections;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;
	using Common;
	using MarketWebService.Configurator;
	using Orders.Model;
	using CommonLib;
	using CommonLib.TrapForThrottlingLogic;
	using MarketplaceWebService;
	using MarketplaceWebService.Model;

	public class AmazonServiceReports {
		private readonly IMarketplaceWebService _Service;
		private static readonly ITrapForThrottling RequestGetReportTrapForThrottling;
		private static readonly ITrapForThrottling ReportGetRequestListTrapForThrottling;
		private static readonly ITrapForThrottling ReportGetReportListTrapForThrottling;
		private static readonly ITrapForThrottling ReportGetReportTrapForThrottling;

		private static readonly ITrapForThrottling ReportGetRequestListNextTokenTrapForThrottling;

		static AmazonServiceReports() {
			RequestGetReportTrapForThrottling = TrapForThrottlingController.Create("Request Report", 15, 120);
			//ReportGetRequestListTrapForThrottling = TrapForThrottlingController.CreateSimpleWait( "GetReportRequestList", 10 , RequestQuoteTimePeriodType.PerMinute);
			ReportGetRequestListTrapForThrottling = TrapForThrottlingController.Create("GetReportRequestList", 10, 45);
			ReportGetRequestListNextTokenTrapForThrottling = TrapForThrottlingController.Create("GetReportRequestListByNextToken", 30, 2);
			ReportGetReportListTrapForThrottling = TrapForThrottlingController.Create("GetReportList", 10);
			ReportGetReportTrapForThrottling = TrapForThrottlingController.Create("GetReport", 15);
		}

		private AmazonServiceReports(IMarketplaceWebService service) {
			_Service = service;

		}

		public static AmazonOrdersList GetUserOrders(IAmazonServiceReportsConfigurator configurator, AmazonOrdersRequestInfo requestInfo, ActionAccessType access) {
			var service = configurator.AmazonService;

			var data = new AmazonServiceReports(service);

			return data.GetUserOrders(requestInfo, access);
		}

		private AmazonOrdersList GetUserOrders(AmazonOrdersRequestInfo amazonRequestInfo, ActionAccessType access) {
			const string getFlatFileOrdersDataRequestStr = "_GET_FLAT_FILE_ORDERS_DATA_";

			var reportRequestList = new RequestsListInfo(amazonRequestInfo, "GetUserOrders", access /*timeout: 3 hours*/, amazonRequestInfo.ErrorRetryingInfo);
			reportRequestList.AddRequest(getFlatFileOrdersDataRequestStr);

			RequestAndWait(reportRequestList);

			return ParseOrdersResult(getFlatFileOrdersDataRequestStr, reportRequestList);
		}

		private void RequestAndWait(RequestsListInfo reportRequestList) {
			RequestReports(reportRequestList);
			WaitRequestsDone(reportRequestList);
		}

		private AmazonOrdersList ParseOrdersResult(string getFlatFileOrdersDataRequestStr, RequestsListInfo reportRequestList) {
			var requestInfo = reportRequestList.GetRequestByName(getFlatFileOrdersDataRequestStr);

			AmazonOrdersList data = new AmazonOrdersList(reportRequestList.StartDate.Value);
			if (requestInfo != null && requestInfo.IsDone) {
				var reportRequestInfo = requestInfo.ReportData;
				if (reportRequestInfo != null) {
					data = new AmazonOrdersList(reportRequestInfo.SubmittedDate);

					using (var stream = new MemoryStream()) {
						GetReportData(requestInfo, stream);
						ParseOrdersStream(data, stream);
					}
				}
			}
			return data;
		}

		private void WaitRequestsDone(RequestsListInfo requestsListInfo) {
			var actionName = requestsListInfo.ActionName;
			var timeOutInMinutes = requestsListInfo.TimeOutInMinutes;
			var access = requestsListInfo.Access;

			var amazonIsdList = requestsListInfo.AmazonIds;

			var getReportRequestListRequest = new GetReportRequestListRequest {
				Merchant = requestsListInfo.UserId,
				ReportRequestIdList = amazonIsdList
			};

			var endDate = DateTime.Now.AddMinutes(timeOutInMinutes);

			while (DateTime.Now <= endDate) {

				var reportRequestListResponse = AmazonWaitBeforeRetryHelper.DoServiceAction(
									requestsListInfo.ErrorRetryingInfo,
									ReportGetRequestListTrapForThrottling,
									actionName,
									access,
									requestsListInfo.RequestsCounter,
									() => _Service.GetReportRequestList(getReportRequestListRequest),
									"GetReportRequestList");

				if (reportRequestListResponse != null && reportRequestListResponse.IsSetGetReportRequestListResult()) {
					var getReportRequestListResult = reportRequestListResponse.GetReportRequestListResult;

					requestsListInfo.UpdateAmazonReportRequestInfo(getReportRequestListResult == null ? null : getReportRequestListResult.ReportRequestInfo);
				}

				if (!requestsListInfo.HasAmazonResult) {
					continue;
				}

				if (requestsListInfo.IsDoneAll) {
					break;
				}

			}

		}

		private void RequestReports(RequestsListInfo reportRequestList) {
			var marketplaceIdList = new IdList { Id = reportRequestList.MarketPlaceId };
			var access = reportRequestList.Access;

			foreach (var requestInfo in reportRequestList) {
				var reportType = requestInfo.Name;

				var requestReportRequest = new RequestReportRequest {
					MarketplaceIdList = marketplaceIdList,
					Merchant = reportRequestList.UserId,
					ReportOptions = "ShowSalesChannel=true",
					ReportType = reportType
				};

				if (reportRequestList.StartDate.HasValue) {
					requestReportRequest.StartDate = reportRequestList.StartDate.Value.ToUniversalTime();
				}

				if (reportRequestList.EndDate.HasValue) {
					requestReportRequest.EndDate = reportRequestList.EndDate.Value.ToUniversalTime();
				}

				RequestInfo info = requestInfo;

				var resp = AmazonWaitBeforeRetryHelper.DoServiceAction(
									reportRequestList.ErrorRetryingInfo,
									RequestGetReportTrapForThrottling,
									reportType,
									access,
									reportRequestList.RequestsCounter,
									() => _Service.RequestReport(requestReportRequest),
									"RequestReport");

				if (resp != null && resp.IsSetRequestReportResult()) {
					info.SetId(resp.RequestReportResult.ReportRequestInfo.ReportRequestId);
				}
			}

		}

		private void GetReportData(RequestInfo requestInfo, Stream stream) {
			var requestReport = new GetReportRequest();
			var requestsListInfo = requestInfo.Owner;
			var merchant = requestsListInfo.UserId;
			var access = requestsListInfo.Access;
			var requestName = requestInfo.Name;

			var requestReportList = new GetReportListRequest { Merchant = merchant };

			ReportInfo reportInfo = null;

			var respGetReportList = AmazonWaitBeforeRetryHelper.DoServiceAction(
									requestsListInfo.ErrorRetryingInfo,
									ReportGetReportListTrapForThrottling,
									requestName,
									access,
									requestsListInfo.RequestsCounter,
									() => _Service.GetReportList(requestReportList),
									"GetReportList");

			if (respGetReportList != null && respGetReportList.IsSetGetReportListResult()) {
				reportInfo = respGetReportList.GetReportListResult.ReportInfo.FirstOrDefault(r => r.ReportRequestId.Equals(requestInfo.Id));
			}

			if (reportInfo != null) {
				requestReport.Merchant = merchant;
				requestReport.ReportId = reportInfo.ReportId;

				requestReport.Report = stream;

				AmazonWaitBeforeRetryHelper.DoServiceAction(
									requestsListInfo.ErrorRetryingInfo,
									ReportGetReportTrapForThrottling,
									requestName,
									access,
									requestsListInfo.RequestsCounter,
									() => _Service.GetReport(requestReport),
									"GetReport");
			}
		}

		private void ParseOrdersStream(AmazonOrdersList orders, MemoryStream stream) {
			using (var sr = new StreamReader(stream)) {
				// заголовок
				var h = sr.ReadLine();

				while (sr.Peek() >= 0) {
					var item = new AmazonOrderItem();
					var str = sr.ReadLine();
					var list = str.Split(new[] { '\t' });

					item.OrderId = GetData(list, AmazonOrderDataEnum.OrderId);
					item.LastUpdateDate = ConvertToDate(GetData(list, AmazonOrderDataEnum.PaymentsDate));
					item.PurchaseDate = ConvertToDate(GetData(list, AmazonOrderDataEnum.PurchaseDate));
					item.NumberOfItemsShipped = ConvertToInt(GetData(list, AmazonOrderDataEnum.QuantityPurchased));

					item.OrderTotal = new AmountInfo {
						Value = ConvertToDouble(GetData(list, AmazonOrderDataEnum.ItemPrice)),
						CurrencyCode = GetData(list, AmazonOrderDataEnum.Currency)
					};

					item.OrderStatus =AmazonOrdersList2ItemStatusType.Shipped;

					orders.Add(item);
				}
			}
		}

		private DateTime? ConvertToDate(string data) {
			if (string.IsNullOrWhiteSpace(data)) {
				return null;
			}

			DateTime val;
			if (DateTime.TryParse(data, out val)) {
				return val.ToUniversalTime();
			}

			return null;
		}

		private int ConvertToInt(string data) {
			if (string.IsNullOrWhiteSpace(data)) {
				return 0;
			}

			int val;

			int.TryParse(data, out val);

			return val;
		}

		private double ConvertToDouble(string data) {
			if (string.IsNullOrWhiteSpace(data)) {
				return 0d;
			}

			double val;

			double.TryParse(data, out val);

			return val;
		}

		private string GetData(string[] list, AmazonOrderDataEnum itemIndex) {
			var indx = (int)itemIndex;
			if (indx >= list.Length)
				return "";
			return list[indx];
		}

	}

	internal class RequestsListInfo : IEnumerable<RequestInfo> {
		private readonly ConcurrentBag<RequestInfo> _List = new ConcurrentBag<RequestInfo>();
		private ICollection<ReportRequestInfo> _LastReportResults;

		public string UserId { get; private set; }
		public string ActionName { get; private set; }
		public ActionAccessType Access { get; private set; }
		public int TimeOutInMinutes { get; private set; }
		public ErrorRetryingInfo ErrorRetryingInfo { get; private set; }
		public RequestsCounterData RequestsCounter { get; private set; }
		public DateTime? EndDate { get; set; }
		public DateTime? StartDate { get; set; }
		public List<string> MarketPlaceId { get; private set; }

		public RequestsListInfo(AmazonRequestInfoBase amazonRequestInfo, string actionName, ActionAccessType access, ErrorRetryingInfo errorRetryingInfo, int timeOutInMinutes = 10) {
			UserId = amazonRequestInfo.MerchantId;
			MarketPlaceId = amazonRequestInfo.MarketplaceId;
			StartDate = amazonRequestInfo.StartDate;
			EndDate = amazonRequestInfo.EndDate;
			ActionName = actionName;
			Access = access;
			TimeOutInMinutes = timeOutInMinutes;
			ErrorRetryingInfo = errorRetryingInfo;

			RequestsCounter = new RequestsCounterData();
		}

		public void AddRequest(string requestName) {
			_List.Add(new RequestInfo(this, requestName));
		}

		public bool HasAmazonResult {
			get { return _LastReportResults != null && _LastReportResults.Count > 0; }
		}

		public bool IsDoneAll {
			get { return _List.All(i => i.IsEnded); }
		}
		public IdList AmazonIds {
			get {
				return new IdList {
					Id = _List.Select(i => i.Id).ToList()
				};
			}
		}

		public void UpdateAmazonReportRequestInfo(ICollection<ReportRequestInfo> reportRequestInfos) {
			_LastReportResults = reportRequestInfos;

			UpdateRequestsItems();
		}

		private void UpdateRequestsItems() {
			if (!HasAmazonResult) {
				_List.AsParallel().ForAll(i => i.ResetData());
				return;
			}

			_LastReportResults.AsParallel().ForAll(r => {
				var req = _List.FirstOrDefault(i => i.Id == r.ReportRequestId);
				if (req != null) {
					req.UpdateData(r);
				}
			});
		}

		public RequestInfo GetRequestByName(string requestName) {
			return _List.FirstOrDefault(i => string.Equals(i.Name, requestName, StringComparison.InvariantCultureIgnoreCase));
		}

		public IEnumerator<RequestInfo> GetEnumerator() {
			return _List.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}

	internal class RequestInfo {
		public string Id { get; private set; }
		public RequestsListInfo Owner { get; private set; }
		public string Name { get; private set; }
		public ReportRequestInfo ReportData { get; private set; }
		public RequestStatusType Status { get; private set; }

		public RequestInfo(RequestsListInfo owner, string requestName) {
			Owner = owner;
			Name = requestName;
			ResetData();
		}

		public void SetId(string requestId) {
			Id = requestId;
		}

		private void UpdateStatus(string status) {
			switch (status) {
				case "_SUBMITTED_":
					Status = RequestStatusType.Submited;
					break;

				case "_IN_PROGRESS_":
					Status = RequestStatusType.InProgress;
					break;

				case "_CANCELLED_":
					Status = RequestStatusType.Canceled;
					break;

				case "_DONE_":
					Status = RequestStatusType.Done;
					break;

				case "_DONE_NO_DATA_":
					Status = RequestStatusType.NoData;
					break;

				default:
					Status = RequestStatusType.Unknown;
					break;
			}
		}

		public bool IsEnded {
			get { return IsDone || Status == RequestStatusType.Canceled || Status == RequestStatusType.NoData; }
		}

		public bool IsDone {
			get { return Status == RequestStatusType.Done; }

		}

		public void ResetData() {
			Status = RequestStatusType.Unknown;
		}

		public void UpdateData(ReportRequestInfo reportRequestData) {
			ReportData = reportRequestData;

			UpdateStatus(ReportData.ReportProcessingStatus);
		}

	}

	internal enum RequestStatusType {
		// Unknown
		Unknown,
		//_SUBMITTED_,
		Submited,
		//_IN_PROGRESS_,
		InProgress,
		//_CANCELLED_,
		Canceled,
		//_DONE_,
		Done,
		//_DONE_NO_DATA_
		NoData
	}

	internal enum AmazonOrderDataEnum {
		OrderId = 0,
		OrderItemId,
		PurchaseDate,
		PaymentsDate,
		BayerEmail,
		BuyerName,
		BuyerPhone,
		Sku,
		ProductName,
		QuantityPurchased,
		Currency,
		ItemPrice,
		ItemTax,
		ShipingPrice,
		ShipingTax,
		ShipServiceLevel,
		RecipientName,
		ShipAddress1,
		ShipAddress2,
		ShipAddress3,
		ShipCity,
		ShipState,
		ShipPostalCode,
		ShipCountry,
		ShipPhoneNumber,
		DeliveryStartDate,
		DeliveryEndDate,
		DeliveryTimeZone,
		DeliveryInstructions,
		SalesChannel,
		OrderChannel,
		OrderChannelInstance,
		ExternalOrderId
	}
}
