namespace EzBob.eBayLib {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Utils;
	using EzBob.CommonLib;
	using EzBob.CommonLib.TimePeriodLogic;
	using EzBob.eBayLib.Config;
	using EzBob.eBayServiceLib;
	using EzBob.eBayServiceLib.com.ebay.developer.soap;
	using EzBob.eBayServiceLib.Common;
	using EzBob.eBayServiceLib.TradingServiceCore;
	using EzBob.eBayServiceLib.TradingServiceCore.DataInfos.Orders;
	using EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Model.Base;
	using EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Model.TokenDependant;
	using EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Model.TokenDependant.GetOrders;
	using EzBob.eBayServiceLib.TradingServiceCore.ResultInfos;
	using EzBob.eBayServiceLib.TradingServiceCore.ResultInfos.Orders;
	using EzBob.eBayServiceLib.TradingServiceCore.TokenProvider;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.DatabaseWrapper;
	using EZBob.DatabaseLib.DatabaseWrapper.EbayFeedbackData;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;
	using EZBob.DatabaseLib.Model.Database;
	using StructureMap;

	public class eBayRetriveDataHelper : MarketplaceRetrieveDataHelperBase {
		public eBayRetriveDataHelper(
			DatabaseDataHelper helper,
			DatabaseMarketplaceBaseBase marketplace
		) : base(helper, marketplace) {
			this.settings = ObjectFactory.GetInstance<IEbayMarketplaceSettings>();

			var ebayConnectionInfo = ObjectFactory.GetInstance<IEbayMarketplaceTypeConnection>();

			this.connectionInfo = eBayServiceHelper.CreateConnection(ebayConnectionInfo);
		} // constructor

		public static IEnumerable<string> GetTopSealedProductItems(EbayDatabaseOrdersList orders, int countTopItems = 10) {
			return orders.AsParallel().Where(o => o.TransactionData != null)
				.SelectMany(o => o.TransactionData)
				.GroupBy(tr => tr.ItemID, (key, group) => new { ItemId = key, Sum = group.Sum(t => t.QuantityPurchased) })
				.OrderByDescending(x => x.Sum)
				.Take(countTopItems)
				.Where(a => a.ItemId != null).Select(a => a.ItemId).ToList();
		} // GetTopSealedProductItems

		public ResultInfoEbayUser GetCustomerUserInfo(eBaySecurityInfo data) {
			DataProviderCreationInfo info = CreateProviderCreationInfo(data);
			return GetCustomerUserInfo(info);
		} // GetCustomerUserInfo

		public override IMarketPlaceSecurityInfo RetrieveCustomerSecurityInfo(int customerMarketPlaceId) {
			return RetrieveCustomerSecurityInfo<eBaySecurityInfo>(GetDatabaseCustomerMarketPlace(customerMarketPlaceId));
		} // RetrieveCustomerSecurityInfo

		public override void Update(int nCustomerMarketplaceID) {
			UpdateCustomerMarketplaceFirst(nCustomerMarketplaceID);
		} // Update

		protected override void InternalUpdateInfo(
			IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
			MP_CustomerMarketplaceUpdatingHistory historyRecord
		) {
			var info = CreateProviderCreationInfo(RetrieveCustomerSecurityInfo<eBaySecurityInfo>(
				databaseCustomerMarketPlace
			));

			CheckTokenStatus(databaseCustomerMarketPlace, info, historyRecord);
			UpdateAccountInfo(databaseCustomerMarketPlace, info, historyRecord);
			UpdateUserInfo(databaseCustomerMarketPlace, info, historyRecord);
			UpdateFeedbackInfo(databaseCustomerMarketPlace, info, historyRecord);

			Helper.CustomerMarketplaceUpdateAction(
				CustomerMarketplaceUpdateActionType.EbayGetOrders,
				databaseCustomerMarketPlace,
				historyRecord,
				() => FetchTransactions(databaseCustomerMarketPlace, info, historyRecord)
			);
		} // InternalUpdateInfo

		protected override ElapsedTimeInfo RetrieveAndAggregate(
			IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
			MP_CustomerMarketplaceUpdatingHistory historyRecord
		) {
			// This method is not implemented here because elapsed time is got from over source.
			throw new NotImplementedException();
		} // RetrieveAndAggregate

		private void CheckTokenStatus(
			IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
			DataProviderCreationInfo info,
			MP_CustomerMarketplaceUpdatingHistory historyRecord
		) {
			Helper.CustomerMarketplaceUpdateAction(
				CustomerMarketplaceUpdateActionType.UpdateAccountInfo,
				databaseCustomerMarketPlace,
				historyRecord,
				() => {
					var elapsedTimeInfo = new ElapsedTimeInfo();

					var checker = new DataProviderCheckAuthenticationToken(info);

					var result = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
						elapsedTimeInfo,
						databaseCustomerMarketPlace.Id,
						ElapsedDataMemberType.RetrieveDataFromExternalService,
						() => checker.Check()
					);

					return new UpdateActionResultInfo {
						Name = UpdateActionResultType.GetTokenStatus,
						RequestsCounter = result == null ? null : result.RequestsCounter,
						ElapsedTime = elapsedTimeInfo
					};
				}
			);
		} // CheckTokenStatus

		private DataProviderCreationInfo CreateProviderCreationInfo(eBaySecurityInfo securityInfo) {
			var serviceConnectionInfo = this.connectionInfo;
			IServiceTokenProvider serviceTokenProvider = new ServiceTokenProviderCustom(securityInfo.Token);
			IEbayServiceProvider serviceProvider = new EbayTradingServiceProvider(serviceConnectionInfo);

			return new DataProviderCreationInfo(serviceProvider) {
				ServiceTokenProvider = serviceTokenProvider,
				Settings = this.settings
			};
		} // CreateProviderCreationInfo

		private ResultInfoEbayUser GetCustomerUserInfo(DataProviderCreationInfo info) {
			return DataProviderUserInfo.GetDataAboutMySelf(info);
		} // GetCustomerUserInfo

		private void SaveFeedbackInfo(
			IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
			ResultInfoEbayFeedBack feedbackInfo,
			MP_CustomerMarketplaceUpdatingHistory historyRecord
		) {
			if (feedbackInfo == null)
				return;

			var submittedDate = feedbackInfo.SubmittedDate;

			var data = new DatabaseEbayFeedbackData(submittedDate) {
				RepeatBuyerCount = feedbackInfo.RepeatBuyerCount,
				RepeatBuyerPercent = feedbackInfo.RepeatBuyerPercent,
				TransactionPercent = feedbackInfo.TransactionPercent,
				UniqueBuyerCount = feedbackInfo.UniqueBuyerCount,
				UniqueNegativeCount = feedbackInfo.UniqueNegativeFeedbackCount,
				UniquePositiveCount = feedbackInfo.UniquePositiveFeedbackCount,
				UniqueNeutralCount = feedbackInfo.UniqueNeutralFeedbackCount
			};

			var timePeriodsFeedback = new[] {
				TimePeriodEnum.Zero,
				TimePeriodEnum.Month,
				TimePeriodEnum.Month6,
				TimePeriodEnum.Year
			};

			foreach (TimePeriodEnum timePeriod in timePeriodsFeedback) {
				data.FeedbackByPeriod.Add(timePeriod, new DatabaseEbayFeedbackDataByPeriod(timePeriod) {
					Negative = feedbackInfo.GetNegativeFeedbackByPeriod(timePeriod),
					Neutral = feedbackInfo.GetNeutralFeedbackByPeriod(timePeriod),
					Positive = feedbackInfo.GetPositiveFeedbackByPeriod(timePeriod),
				});
			} // for

			var timePeriodsRaiting = new[] { 
				new Tuple<TimePeriodEnum, FeedbackSummaryPeriodCodeType>(
					TimePeriodEnum.Month,
					FeedbackSummaryPeriodCodeType.ThirtyDays
				),
				new Tuple<TimePeriodEnum, FeedbackSummaryPeriodCodeType>(
					TimePeriodEnum.Year,
					FeedbackSummaryPeriodCodeType.FiftyTwoWeeks
				)
			};

			foreach (var pair in timePeriodsRaiting) {
				var timePeriod = pair.Item1;
				var periodCodeType = pair.Item2;

				data.RaitingByPeriod.Add(timePeriod, new DatabaseEbayRaitingDataByPeriod(timePeriod) {
					Communication = feedbackInfo.GetRaitingData(
						periodCodeType,
						FeedbackRatingDetailCodeType.Communication
					),
					ItemAsDescribed = feedbackInfo.GetRaitingData(
						periodCodeType,
						FeedbackRatingDetailCodeType.ItemAsDescribed
					),
					ShippingAndHandlingCharges = feedbackInfo.GetRaitingData(
						periodCodeType,
						FeedbackRatingDetailCodeType.ShippingAndHandlingCharges
					),
					ShippingTime = feedbackInfo.GetRaitingData(
						periodCodeType,
						FeedbackRatingDetailCodeType.ShippingTime
					)
				});
			} // for

			Helper.StoreEbayFeedbackData(databaseCustomerMarketPlace, data, historyRecord);
		} // SaveFeedbackInfo

		private void UpdateAccountInfo(
			IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
			DataProviderCreationInfo info,
			MP_CustomerMarketplaceUpdatingHistory historyRecord
		) {
			Helper.CustomerMarketplaceUpdateAction(
				CustomerMarketplaceUpdateActionType.UpdateAccountInfo,
				databaseCustomerMarketPlace,
				historyRecord,
				() => {
					var account = new DataProviderGetAccount(info);

					var elapsedTimeInfo = new ElapsedTimeInfo();

					var resultInfo = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
						elapsedTimeInfo,
						databaseCustomerMarketPlace.Id,
						ElapsedDataMemberType.RetrieveDataFromExternalService,
						() => account.GetAccount()
					);

					ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
						elapsedTimeInfo,
						databaseCustomerMarketPlace.Id,
						ElapsedDataMemberType.StoreDataToDatabase,
						() => Helper.StoreEbayUserAccountData(databaseCustomerMarketPlace, resultInfo, historyRecord)
					);

					return new UpdateActionResultInfo {
						Name = UpdateActionResultType.CurrentBalance,
						Value = resultInfo == null ? null : (object)resultInfo.CurrentBalance,
						RequestsCounter = resultInfo == null ? null : resultInfo.RequestsCounter,
						ElapsedTime = elapsedTimeInfo
					};
				}
			);
		} // UpdateAccountInfo

		private void UpdateFeedbackInfo(
			IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
			DataProviderCreationInfo info,
			MP_CustomerMarketplaceUpdatingHistory historyRecord
		) {
			Helper.CustomerMarketplaceUpdateAction(
				CustomerMarketplaceUpdateActionType.UpdateFeedbackInfo,
				databaseCustomerMarketPlace,
				historyRecord,
				() => {
					var elapsedTimeInfo = new ElapsedTimeInfo();

					var resultInfo = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
						elapsedTimeInfo,
						databaseCustomerMarketPlace.Id,
						ElapsedDataMemberType.RetrieveDataFromExternalService,
						() => DataProviderGetFeedback.GetFeedBack(info)
					);

					ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
						elapsedTimeInfo,
						databaseCustomerMarketPlace.Id,
						ElapsedDataMemberType.StoreDataToDatabase,
						() => SaveFeedbackInfo(databaseCustomerMarketPlace, resultInfo, historyRecord)
					);

					var ebayRaitingInfo = resultInfo == null ? null : resultInfo.GetRaitingData(
						FeedbackSummaryPeriodCodeType.FiftyTwoWeeks,
						FeedbackRatingDetailCodeType.ShippingAndHandlingCharges
					);

					return new UpdateActionResultInfo {
						Name = UpdateActionResultType.FeedbackRaiting,
						Value = ebayRaitingInfo == null ? null : (object)ebayRaitingInfo.Value,
						RequestsCounter = resultInfo == null ? null : resultInfo.RequestsCounter,
						ElapsedTime = elapsedTimeInfo
					};
				}
			);
		} // UpdateFeedbackInfo

		private void UpdateUserInfo(
			IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
			DataProviderCreationInfo info,
			MP_CustomerMarketplaceUpdatingHistory historyRecord
		) {
			Helper.CustomerMarketplaceUpdateAction(
				CustomerMarketplaceUpdateActionType.UpdateUserInfo,
				databaseCustomerMarketPlace,
				historyRecord,
				() => {
					var elapsedTimeInfo = new ElapsedTimeInfo();

					var resultInfo = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
						elapsedTimeInfo,
						databaseCustomerMarketPlace.Id,
						ElapsedDataMemberType.RetrieveDataFromExternalService,
						() => GetCustomerUserInfo(info)
					);

					ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
						elapsedTimeInfo,
						databaseCustomerMarketPlace.Id,
						ElapsedDataMemberType.StoreDataToDatabase,
						() => Helper.StoreEbayUserData(databaseCustomerMarketPlace, resultInfo, historyRecord)
					);

					return new UpdateActionResultInfo {
						Name = UpdateActionResultType.FeedbackRatingStar,
						Value = resultInfo == null ? null : (object)resultInfo.FeedbackRatingStar,
						RequestsCounter = resultInfo == null ? null : resultInfo.RequestsCounter,
						ElapsedTime = elapsedTimeInfo
					};
				}
			);
		} // UpdateUserInfo

		private IUpdateActionResultInfo FetchTransactions(
			IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
			DataProviderCreationInfo info,
			MP_CustomerMarketplaceUpdatingHistory historyRecord
		) {
			int mpID = databaseCustomerMarketPlace.Id;

			DateTime toDate = DateTime.UtcNow.Date;

			var elapsedTimeInfo = new ElapsedTimeInfo();

			DateTime fromDate = Helper.FindLastKnownEbayTransactionTime(mpID);

			var periods = new Stack<FetchPeriod>();

			DateTime t = toDate;

			while (t >= fromDate) {
				DateTime f = t.AddDays(-90);

				periods.Push(new FetchPeriod { To = t, From = f < fromDate ? fromDate : f, });

				t = f;
			} // while

			var frc = new FetchResultCounters();

			foreach (var period in periods) {
				frc.Add(FetchOnePeriodTransactions(
					mpID,
					databaseCustomerMarketPlace,
					elapsedTimeInfo,
					info,
					historyRecord,
					period
				));
			} // for each

			return new UpdateActionResultInfo {
				Name = UpdateActionResultType.eBayOrdersCount,
				Value = frc.OrderCount,
				RequestsCounter = frc.RequestCount.IsEmpty ? null : frc.RequestCount,
				ElapsedTime = elapsedTimeInfo
			};
		} // FetchTransactions

		private struct FetchPeriod {
			public DateTime From { get; set; }
			public DateTime To { get; set; }
		} // struct FetchPeriod

		private class FetchResultCounters {
			public FetchResultCounters() : this(null, 0) {} // constructor

			public FetchResultCounters(RequestsCounterData requestCount, int orderCount) {
				RequestCount = requestCount ?? new RequestsCounterData();
				OrderCount = orderCount;
			} // constructor

			public RequestsCounterData RequestCount { get; private set; }
			public int OrderCount { get; private set; }

			public void Add(FetchResultCounters other) {
				if (other == null)
					return;

				if (!other.RequestCount.IsEmpty)
					RequestCount.Add(other.RequestCount);

				OrderCount += other.OrderCount;
			} // Add
		} // class FetchResultCounters

		private FetchResultCounters FetchOnePeriodTransactions(
			int mpID,
			IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
			ElapsedTimeInfo elapsedTimeInfo,
			DataProviderCreationInfo info,
			MP_CustomerMarketplaceUpdatingHistory historyRecord,
			FetchPeriod period
		) {
			ResultInfoOrders orders = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				mpID,
				ElapsedDataMemberType.RetrieveDataFromExternalService,
				() => DataProviderGetOrders.GetOrders(
					info,
					new ParamsDataInfoGetOrdersFromDateToDateCreated(period.From, period.To)
				)
			);

			EbayDatabaseOrdersList databaseOrdersList = ParseOrdersInfo(orders);

			ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				mpID,
				ElapsedDataMemberType.StoreDataToDatabase,
				() => Helper.AddEbayOrdersData(databaseCustomerMarketPlace, databaseOrdersList, historyRecord)
			);

			EbayDatabaseOrdersList allEBayOrders = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				mpID,
				ElapsedDataMemberType.RetrieveDataFromDatabase,
				() => Helper.GetAllEBayOrders(orders.SubmittedDate, databaseCustomerMarketPlace)
			);

			if (this.settings.DownloadCategories) {
				IEnumerable<string> topSealedProductItems = GetTopSealedProductItems(allEBayOrders);

				if (topSealedProductItems != null) {
					List<MP_EBayOrderItemDetail> orderItemDetails = topSealedProductItems.Select(
						item => FindEBayOrderItemInfo(
							databaseCustomerMarketPlace,
							info,
							item,
							databaseOrdersList.RequestsCounter,
							elapsedTimeInfo
						)
					).Where(d => d != null)
					.ToList();

					Helper.UpdateOrderItemsInfo(orderItemDetails, elapsedTimeInfo, mpID);
				} // if
			} // if

			return new FetchResultCounters(
				(databaseOrdersList == null) ? null : databaseOrdersList.RequestsCounter,
				((orders == null) || (orders.Orders == null)) ? 0 : orders.Orders.Count
			);
		} // FetchOnePeriodTransactions

		private EbayDatabaseOrdersList ParseOrdersInfo(ResultInfoOrders data) {
			var rez = new EbayDatabaseOrdersList(data.SubmittedDate) {
				RequestsCounter = data.RequestsCounter,
			};

			foreach (OrderType o in data) {
				if (o == null)
					continue;

				var item = new EbayDatabaseOrderItem {
					CreatedTime = o.CreatedTimeSpecified ? o.CreatedTime.ToUniversalTime() : (DateTime?)null,
					ShippedTime = o.ShippedTimeSpecified ? o.ShippedTime.ToUniversalTime() : (DateTime?)null,
					PaymentTime = o.PaidTimeSpecified ? o.PaidTime.ToUniversalTime() : (DateTime?)null,
					BuyerName = o.BuyerUserID,
					AdjustmentAmount = ConvertToBaseCurrency(o.AdjustmentAmount, o.CreatedTime),
					AmountPaid = ConvertToBaseCurrency(o.AmountPaid, o.CreatedTime),
					SubTotal = ConvertToBaseCurrency(o.Subtotal, o.CreatedTime),
					Total = ConvertToBaseCurrency(o.Total, o.CreatedTime),
					OrderStatus = o.OrderStatusSpecified
						? (EBayOrderStatusCodeType)Enum.Parse(typeof(EBayOrderStatusCodeType), o.OrderStatus.ToString())
						: EBayOrderStatusCodeType.Default,
					PaymentHoldStatus = o.PaymentHoldStatusSpecified ? o.PaymentHoldStatus.ToString() : string.Empty,
					CheckoutStatus = o.CheckoutStatus != null && o.CheckoutStatus.StatusSpecified
						? o.CheckoutStatus.Status.ToString()
						: string.Empty,
					PaymentMethod = o.CheckoutStatus != null && o.CheckoutStatus.PaymentMethodSpecified
						? o.CheckoutStatus.PaymentMethod.ToString()
						: string.Empty,
					PaymentStatus = o.CheckoutStatus != null && o.CheckoutStatus.eBayPaymentStatusSpecified
						? o.CheckoutStatus.eBayPaymentStatus.ToString()
						: string.Empty,
					PaymentMethods = o.PaymentMethods == null ? null : string.Join(",", o.PaymentMethods),
					ShippingAddressData = o.ShippingAddress == null ? null : o.ShippingAddress.ConvertToDatabaseType(),
				};

				if (o.ExternalTransaction != null && o.ExternalTransaction.Length > 0) {
					item.ExternalTransactionData = new EBayDatabaseExternalTransactionList();
					foreach (var et in o.ExternalTransaction) {
						if (et == null)
							continue;

						var exItem = new EBayDatabaseExternalTransactionItem();

						exItem.TransactionID = et.ExternalTransactionID;
						exItem.TransactionTime = et.ExternalTransactionTimeSpecified
							? et.ExternalTransactionTime
							: (DateTime?)null;
						exItem.FeeOrCreditAmount = ConvertToBaseCurrency(et.FeeOrCreditAmount, et.ExternalTransactionTime);
						exItem.PaymentOrRefundAmount = ConvertToBaseCurrency(
							et.PaymentOrRefundAmount,
							et.ExternalTransactionTime
						);

						item.ExternalTransactionData.Add(exItem);
					} // for each
				} // if

				if (o.TransactionArray != null && o.TransactionArray.Length > 0) {
					item.TransactionData = new EbayDatabaseTransactionDataList();

					foreach (var td in o.TransactionArray) {
						if (td == null || td.Item == null)
							continue;

						var itemType = td.Item;

						var trItem = new EbayDatabaseTransactionDataItem {
							CreatedDate = td.CreatedDate,
							QuantityPurchased = td.QuantityPurchased,
							PaymentHoldStatus = td.Status != null && td.Status.PaymentHoldStatusSpecified
								? td.Status.PaymentHoldStatus.ToString()
								: string.Empty,
							PaymentMethodUsed = td.Status != null && td.Status.PaymentMethodUsedSpecified
								? td.Status.PaymentMethodUsed.ToString()
								: string.Empty,
							TransactionPrice = ConvertToBaseCurrency(td.TransactionPrice, td.CreatedDate),
							ItemSKU = itemType.SKU,
							ItemID = itemType.ItemID,
							ItemPrivateNotes = itemType.PrivateNotes,
							ItemSellerInventoryID = itemType.SellerInventoryID,
							eBayTransactionId = td.TransactionID
						};

						item.TransactionData.Add(trItem);
					} // for each
				} // if

				rez.Add(item);
			} // for each o in data

			return rez;
		} // ParseOrdersInfo

		private AmountInfo ConvertToBaseCurrency(AmountType sourceAmaontType, DateTime createdTime) {
			if (sourceAmaontType == null)
				return null;

			return Helper.CurrencyConverter.ConvertToBaseCurrency(
				sourceAmaontType.currencyID.ToString(),
				sourceAmaontType.Value,
				createdTime
			);
		} // ConvertToBaseCurrency

		private MP_EBayOrderItemDetail FindEBayOrderItemInfo(
			IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
			DataProviderCreationInfo info,
			string itemID,
			RequestsCounterData requestCounter,
			ElapsedTimeInfo elapsedTimeInfo
		) {
			if (!this.settings.DownloadCategories)
				return null;

			int mpID = databaseCustomerMarketPlace.Id;

			IMarketplaceType marketplace = databaseCustomerMarketPlace.Marketplace;

			var eBayItemInfoData = new eBayFindOrderItemInfoData(itemID);

			var eBayOrderItemInfo = Helper.FindEBayOrderItemInfo(eBayItemInfoData, elapsedTimeInfo, mpID);

			if (eBayOrderItemInfo == null) {
				var providerGetItemInfo = new DataProviderGetItemInfo(info);

				var req = new eBayRequestItemInfoData(eBayItemInfoData);

				ResultInfoEbayItemInfo ebayItemInfo = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
					elapsedTimeInfo,
					mpID,
					ElapsedDataMemberType.RetrieveDataFromExternalService,
					() => providerGetItemInfo.GetItem(req)
				);

				requestCounter.Add(ebayItemInfo.RequestsCounter);

				var newEBayOrderItemInfo = new EbayDatabaseOrderItemInfo {
					ItemID = ebayItemInfo.ItemID,
					PrimaryCategory = FindCategory(marketplace, ebayItemInfo.PrimaryCategory, elapsedTimeInfo, mpID),
					// SecondaryCategory = FindCategory(marketplace, ebayItemInfo.SecondaryCategory, elapsedTimeInfo, mpID),
					// FreeAddedCategory = FindCategory(marketplace, ebayItemInfo.FreeAddedCategory, elapsedTimeInfo, mpID),
					Title = ebayItemInfo.Title,
				};

				eBayOrderItemInfo = Helper.SaveEBayOrderItemInfo(newEBayOrderItemInfo, elapsedTimeInfo, mpID);
			} // if

			return eBayOrderItemInfo;
		} // FindEBayOrderItemInfo

		private MP_EbayAmazonCategory FindCategory(
			IMarketplaceType marketplace,
			eBayCategoryInfo data,
			ElapsedTimeInfo elapsedTimeInfo,
			int mpID
		) {
			if (data == null)
				return null;

			return Helper.FindEBayAmazonCategory(marketplace, data.CategoryId, elapsedTimeInfo, mpID) ??
				Helper.AddEbayCategory(marketplace, data, elapsedTimeInfo, mpID);
		} // FindCategory

		private readonly EbayServiceConnectionInfo connectionInfo;
		private readonly IEbayMarketplaceSettings settings;
	} // class eBayRetriveDataHelper
} // namespace
