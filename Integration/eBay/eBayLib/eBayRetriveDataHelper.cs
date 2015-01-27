namespace EzBob.eBayLib {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.DatabaseWrapper;
	using EZBob.DatabaseLib.DatabaseWrapper.EbayFeedbackData;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;
	using EZBob.DatabaseLib.Model.Database;
	using CommonLib;
	using CommonLib.MarketplaceSpecificTypes.TeraPeakOrdersData;
	using CommonLib.TimePeriodLogic;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using TeraPeakServiceLib;
	using TeraPeakServiceLib.Requests.SellerResearch;
	using Config;
	using eBayServiceLib;
	using eBayServiceLib.Common;
	using eBayServiceLib.TradingServiceCore;
	using eBayServiceLib.TradingServiceCore.DataProviders.Model.Base;
	using eBayServiceLib.TradingServiceCore.DataProviders.Model.TokenDependant;
	using eBayServiceLib.TradingServiceCore.ResultInfos;
	using eBayServiceLib.TradingServiceCore.TokenProvider;
	using eBayServiceLib.com.ebay.developer.soap;
	using Ezbob.Database;
	using StructureMap;

	public class eBayRetriveDataHelper : MarketplaceRetrieveDataHelperBase {
		public eBayRetriveDataHelper(
			DatabaseDataHelper helper,
			DatabaseMarketplaceBaseBase marketplace
		)
			: base(helper, marketplace) {
			_Settings = ObjectFactory.GetInstance<IEbayMarketplaceSettings>();

			var ebayConnectionInfo = ObjectFactory.GetInstance<IEbayMarketplaceTypeConnection>();

			_EbayConnectionInfo = eBayServiceHelper.CreateConnection(ebayConnectionInfo);
		} // constructor

		public static IEnumerable<string> GetTopSealedProductItems(EbayDatabaseOrdersList orders, int countTopItems = 10) {
			return orders.AsParallel().Where(o => o.TransactionData != null)
				.SelectMany(o => o.TransactionData)
				.GroupBy(tr => tr.ItemID, (key, group) => new { ItemId = key, Sum = group.Sum(t => t.QuantityPurchased) })
				.OrderByDescending(x => x.Sum)
				.Take(countTopItems)
				.Where(a => a.ItemId != null).Select(a => a.ItemId).ToList();
		}

		public ResultInfoEbayUser GetCustomerUserInfo(eBaySecurityInfo data) {
			DataProviderCreationInfo info = CreateProviderCreationInfo(data);
			return GetCustomerUserInfo(info);
		}

		public override IMarketPlaceSecurityInfo RetrieveCustomerSecurityInfo(int customerMarketPlaceId) {
			return RetrieveCustomerSecurityInfo<eBaySecurityInfo>(GetDatabaseCustomerMarketPlace(customerMarketPlaceId));
		}

		public override void Update(int nCustomerMarketplaceID) {
			UpdateCustomerMarketplaceFirst(nCustomerMarketplaceID);
		} // Update

		internal enum UpdateStrategyType {
			OnlyTeraPeak,
			EbayGetOrdersAfterTeraPeak
		}

		protected override void InternalUpdateInfo(
			IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
			MP_CustomerMarketplaceUpdatingHistory historyRecord
		) {
			var info = CreateProviderCreationInfo(RetrieveCustomerSecurityInfo<eBaySecurityInfo>(databaseCustomerMarketPlace));

			UpdateTeraPeakOrders(databaseCustomerMarketPlace, databaseCustomerMarketPlace.DisplayName, historyRecord);
			CheckTokenStatus(databaseCustomerMarketPlace, info, historyRecord);
			UpdateAccountInfo(databaseCustomerMarketPlace, info, historyRecord);
			UpdateUserInfo(databaseCustomerMarketPlace, info, historyRecord);
			UpdateFeedbackInfo(databaseCustomerMarketPlace, info, historyRecord);
		}

		protected override ElapsedTimeInfo RetrieveAndAggregate(
			IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
			MP_CustomerMarketplaceUpdatingHistory historyRecord
		) {
			// This method is not implemented here because elapsed time is got from over source.
			throw new NotImplementedException();
		}

		private void CheckTokenStatus(
			IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
			DataProviderCreationInfo info,
			MP_CustomerMarketplaceUpdatingHistory historyRecord
		) {
			Helper.CustomerMarketplaceUpdateAction(CustomerMarketplaceUpdateActionType.UpdateAccountInfo, databaseCustomerMarketPlace, historyRecord, () => {
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
			});
		}

		private DataProviderCreationInfo CreateProviderCreationInfo(eBaySecurityInfo securityInfo) {
			var connectionInfo = _EbayConnectionInfo;
			IServiceTokenProvider serviceTokenProvider = new ServiceTokenProviderCustom(securityInfo.Token);
			IEbayServiceProvider serviceProvider = new EbayTradingServiceProvider(connectionInfo);

			return new DataProviderCreationInfo(serviceProvider) {
				ServiceTokenProvider = serviceTokenProvider,
				Settings = _Settings
			};
		}

		private ResultInfoEbayUser GetCustomerUserInfo(DataProviderCreationInfo info) {
			return DataProviderUserInfo.GetDataAboutMySelf(info);
		}

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
				new Tuple<TimePeriodEnum, FeedbackSummaryPeriodCodeType>(TimePeriodEnum.Month, FeedbackSummaryPeriodCodeType.ThirtyDays),
				new Tuple<TimePeriodEnum, FeedbackSummaryPeriodCodeType>(TimePeriodEnum.Year, FeedbackSummaryPeriodCodeType.FiftyTwoWeeks)
			};

			foreach (var pair in timePeriodsRaiting) {
				var timePeriod = pair.Item1;
				var periodCodeType = pair.Item2;

				data.RaitingByPeriod.Add(timePeriod, new DatabaseEbayRaitingDataByPeriod(timePeriod) {
					Communication = feedbackInfo.GetRaitingData(periodCodeType, FeedbackRatingDetailCodeType.Communication),
					ItemAsDescribed = feedbackInfo.GetRaitingData(periodCodeType, FeedbackRatingDetailCodeType.ItemAsDescribed),
					ShippingAndHandlingCharges = feedbackInfo.GetRaitingData(periodCodeType, FeedbackRatingDetailCodeType.ShippingAndHandlingCharges),
					ShippingTime = feedbackInfo.GetRaitingData(periodCodeType, FeedbackRatingDetailCodeType.ShippingTime)
				});
			} // for

			Helper.StoreEbayFeedbackData(databaseCustomerMarketPlace, data, historyRecord);
		}

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
		}

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

					var ebayRaitingInfo = resultInfo == null
						? null
						: resultInfo.GetRaitingData(FeedbackSummaryPeriodCodeType.FiftyTwoWeeks, FeedbackRatingDetailCodeType.ShippingAndHandlingCharges);

					return new UpdateActionResultInfo {
						Name = UpdateActionResultType.FeedbackRaiting,
						Value = ebayRaitingInfo == null ? null : (object)ebayRaitingInfo.Value,
						RequestsCounter = resultInfo == null ? null : resultInfo.RequestsCounter,
						ElapsedTime = elapsedTimeInfo
					};
				}
			);
		}

		private void UpdateTeraPeakOrders(
			IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
			string ebayUserID,
			MP_CustomerMarketplaceUpdatingHistory historyRecord
		) {
			Helper.CustomerMarketplaceUpdateAction(
				CustomerMarketplaceUpdateActionType.TeraPeakSearchBySeller,
				databaseCustomerMarketPlace,
				historyRecord,
				() => {
					var elapsedTimeInfo = new ElapsedTimeInfo();

					// By default, retrieve last year data.
					int countMonthsForRetrieveData = MaxPossibleRetriveMonthsFromTeraPeak;

					DateTime now = DateTime.UtcNow;

					var sellerInfo = new TeraPeakSellerInfo(ebayUserID);

					TeraPeakDatabaseSellerData allTeraPeakData = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
						elapsedTimeInfo,
						databaseCustomerMarketPlace.Id,
						ElapsedDataMemberType.RetrieveDataFromDatabase,
						() => Helper.GetAllTeraPeakDataWithFullRange(now, databaseCustomerMarketPlace)
					);

					if (allTeraPeakData.Count > 0) {
						DateTime lastDate = allTeraPeakData.Max(i => i.EndDate);
						countMonthsForRetrieveData = lastDate.GetCountMonthsToByEntire(now);
					} // if

					if (countMonthsForRetrieveData <= 0) {
						log.Debug("Count months for retrieve data is {0} - nothing to update.", countMonthsForRetrieveData);
						return null;
					} // if

					var ranges = new SearchQueryDatesRangeListData();

					if (countMonthsForRetrieveData > MaxPossibleRetriveMonthsFromTeraPeak)
						countMonthsForRetrieveData = MaxPossibleRetriveMonthsFromTeraPeak;

					var startRequestDate = now.Date.AddMonths(1 - countMonthsForRetrieveData);

					var peakRequestDataInfo = new TeraPeakRequestDataInfo {
						StepType = TeraPeakRequestStepEnum.ByMonth,
						CountSteps = countMonthsForRetrieveData,
						StartDate = startRequestDate,
					};

					ranges.AddRange(TerapeakRequestsQueue.CreateQueriesDates(peakRequestDataInfo, now));

					var requestInfoByRange = new TeraPeakRequestInfo(sellerInfo, ranges, _Settings.ErrorRetryingInfo);

					var teraPeakDatabaseSellerDataByRange = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
						elapsedTimeInfo,
						databaseCustomerMarketPlace.Id,
						ElapsedDataMemberType.RetrieveDataFromExternalService,
						() => TeraPeakService.SearchBySeller(requestInfoByRange)
					);

					ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
						elapsedTimeInfo,
						databaseCustomerMarketPlace.Id,
						ElapsedDataMemberType.StoreDataToDatabase,
						() => Helper.StoretoDatabaseTeraPeakOrdersData(databaseCustomerMarketPlace, teraPeakDatabaseSellerDataByRange, historyRecord)
					);

					return new UpdateActionResultInfo {
						Name = UpdateActionResultType.TeraPeakOrdersCount,
						Value = teraPeakDatabaseSellerDataByRange.Count,
						RequestsCounter = teraPeakDatabaseSellerDataByRange.RequestsCounter,
						ElapsedTime = elapsedTimeInfo
					};
				} // action as argument
			);
		}

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

		private const int MaxPossibleRetriveMonthsFromTeraPeak = 12;

		private static readonly ASafeLog log = new SafeILog(typeof(eBayRetriveDataHelper));
		private readonly EbayServiceConnectionInfo _EbayConnectionInfo;
		private readonly IEbayMarketplaceSettings _Settings;
	} // class eBayRetriveDataHelper
} // namespace
