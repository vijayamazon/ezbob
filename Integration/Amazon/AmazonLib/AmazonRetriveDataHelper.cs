namespace EzBob.AmazonLib
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.DatabaseWrapper;
	using EZBob.DatabaseLib.DatabaseWrapper.AmazonFeedbackData;
	using EZBob.DatabaseLib.DatabaseWrapper.FunctionValues;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;
	using EZBob.DatabaseLib.DatabaseWrapper.Products;
	using EZBob.DatabaseLib.DatabaseWrapper.ValueType;
	using EZBob.DatabaseLib.Model.Marketplaces.Amazon;
	using AmazonDbLib;
	using AmazonServiceLib;
	using AmazonServiceLib.Common;
	using AmazonServiceLib.Config;
	using AmazonServiceLib.Products;
	using AmazonServiceLib.Orders.Model;
	using AmazonServiceLib.UserInfo;
	using CommonLib;
	using CommonLib.TimePeriodLogic;
	using Ezbob.Utils;
	using MarketplaceWebServiceProducts;
	using StructureMap;
	using EZBob.DatabaseLib.Model.Database;
	using log4net;
	using ConfigManager;

	public class AmazonRetriveDataHelper : MarketplaceRetrieveDataHelperBase<AmazonDatabaseFunctionType>
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(AmazonRetriveDataHelper));
		private readonly AmazonServiceConnectionInfo _ConnectionInfo;
		private readonly ErrorRetryingInfo _AmazonSettings;
		public AmazonRetriveDataHelper(DatabaseDataHelper helper, DatabaseMarketplaceBase<AmazonDatabaseFunctionType> marketplace)
			: base(helper, marketplace)
		{
			var connectionInfo = ObjectFactory.GetInstance<IAmazonMarketPlaceTypeConnection>();

			_ConnectionInfo = AmazonServiceConnectionFactory.CreateConnection(connectionInfo);
			_AmazonSettings = new ErrorRetryingInfo((bool)CurrentValues.Instance.AmazonEnableRetrying, CurrentValues.Instance.AmazonMinorTimeoutInSeconds, CurrentValues.Instance.AmazonUseLastTimeOut);

			_AmazonSettings.Info = new ErrorRetryingItemInfo[2];
			_AmazonSettings.Info[0] = new ErrorRetryingItemInfo(CurrentValues.Instance.AmazonIterationSettings1Index, CurrentValues.Instance.AmazonIterationSettings1CountRequestsExpectError, CurrentValues.Instance.AmazonIterationSettings1TimeOutAfterRetryingExpiredInMinutes);
			_AmazonSettings.Info[1] = new ErrorRetryingItemInfo(CurrentValues.Instance.AmazonIterationSettings2Index, CurrentValues.Instance.AmazonIterationSettings2CountRequestsExpectError, CurrentValues.Instance.AmazonIterationSettings2TimeOutAfterRetryingExpiredInMinutes);
		}

		protected override void InternalUpdateInfoFirst(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, MP_CustomerMarketplaceUpdatingHistory historyRecord)
		{
			log.DebugFormat("InternalUpdateInfoFirst customer {0} amazon mp {1}", databaseCustomerMarketPlace.Customer.Id, databaseCustomerMarketPlace.DisplayName);
			var securityInfo = RetrieveCustomerSecurityInfo<AmazonSecurityInfo>(databaseCustomerMarketPlace);

			UpdateClientOrdersInfo(databaseCustomerMarketPlace, securityInfo, ActionAccessType.Full, historyRecord);
			//UpdateClientInventoryInfo( databaseCustomerMarketPlace, securityInfo, ActionAccessType.Full, historyRecord );
			UpdateClientFeedbackInfo(databaseCustomerMarketPlace, securityInfo, historyRecord);
		}

		public override void Update(int nCustomerMarketplaceID)
		{
			UpdateCustomerMarketplaceFirst(nCustomerMarketplaceID);
		} // Update

		protected override ElapsedTimeInfo RetrieveAndAggregate(
			IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
			MP_CustomerMarketplaceUpdatingHistory historyRecord
		)
		{
			// This method is not implemented here because elapsed time is got from over source.
			throw new NotImplementedException();
		}

		protected override void InternalUpdateInfo(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, MP_CustomerMarketplaceUpdatingHistory historyRecord)
		{
			log.DebugFormat("InternalUpdateInfo customer {0} amazon mp {1}", databaseCustomerMarketPlace.Customer.Id, databaseCustomerMarketPlace.DisplayName);
			var securityInfo = RetrieveCustomerSecurityInfo<AmazonSecurityInfo>(databaseCustomerMarketPlace);

			UpdateClientOrdersInfo(databaseCustomerMarketPlace, securityInfo, ActionAccessType.Limit, historyRecord);
			//UpdateClientInventoryInfo( databaseCustomerMarketPlace, securityInfo, ActionAccessType.Limit, historyRecord );
			UpdateClientFeedbackInfo(databaseCustomerMarketPlace, securityInfo, historyRecord);
		}

		protected override void AddAnalysisValues(IDatabaseCustomerMarketPlace marketPlace, AnalysisDataInfo data)
		{
			List<MP_AmazonFeedback> feedbacks = Helper
				.GetAmazonFeedback()
				.Where(f =>
					(f.CustomerMarketPlace.Id == marketPlace.Id) &&
					(f.HistoryRecord != null) &&
					(f.HistoryRecord.UpdatingStart != null) &&
					(f.HistoryRecord.UpdatingEnd != null)
				)
				.OrderBy(x => x.Created)
				.ToList();

			if (!feedbacks.Any())
				return;

			MP_AmazonFeedback oLatestFeedback = feedbacks.Last();

			var feedBackParams = new List<IAnalysisDataParameterInfo>();

			if (!oLatestFeedback.FeedbackByPeriodItems.IsEmpty)
			{
				foreach (MP_AmazonFeedbackItem afp in oLatestFeedback.FeedbackByPeriodItems)
				{
					var timePeriod = TimePeriodFactory.CreateById(afp.TimePeriod.InternalId);

					var c = new AnalysisDataParameterInfo("Number of reviews", timePeriod, DatabaseValueType.Integer, afp.Count);
					var g = new AnalysisDataParameterInfo("Negative Feedback rate", timePeriod, DatabaseValueType.Integer, afp.Negative);
					var n = new AnalysisDataParameterInfo("Neutral Feedback rate", timePeriod, DatabaseValueType.Integer, afp.Neutral);
					var p = new AnalysisDataParameterInfo("Positive Feedback Rate", timePeriod, DatabaseValueType.Integer, afp.Positive);

					if (timePeriod.TimePeriodType == TimePeriodEnum.Year)
					{
						var sum = afp.Positive + afp.Neutral + afp.Neutral;

						feedBackParams.Add(new AnalysisDataParameterInfo(
							"Positive %",
							timePeriod,
							DatabaseValueType.Double,
							sum == 0 ? 0 : (afp.Positive * 100) / sum
						));
					} // if

					feedBackParams.AddRange(new[] { c, n, g, p, });
				} // for each
			} // if

			if (feedBackParams.Count > 0)
			{
				if (data.Data != null && data.Data.Count > 0)
				{
					DateTime lastDate = data.Data.Keys.Max();
					data.Data[lastDate].AddRange(feedBackParams);
				}
				else
				{
					data.AddData(oLatestFeedback.HistoryRecord.UpdatingStart.Value, feedBackParams);
				}
			}
		} // AddAnalysisValues

		public override IMarketPlaceSecurityInfo RetrieveCustomerSecurityInfo(int customerMarketPlaceId)
		{
			return RetrieveCustomerSecurityInfo<AmazonSecurityInfo>(GetDatabaseCustomerMarketPlace(customerMarketPlaceId));
		}

		private void UpdateClientOrdersInfo(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, AmazonSecurityInfo securityInfo, ActionAccessType access, MP_CustomerMarketplaceUpdatingHistory historyRecord)
		{
			log.DebugFormat("UpdateClientOrdersInfo customer {0}, amazon mp {1}, access {2}", databaseCustomerMarketPlace.Customer.Id, databaseCustomerMarketPlace.DisplayName, access);

			Helper.CustomerMarketplaceUpdateAction(CustomerMarketplaceUpdateActionType.UpdateOrdersInfo, databaseCustomerMarketPlace, historyRecord, () =>
			{
				// save data to order table
				//var webServiceConfigurator = CreateServiceReportsConfigurator(connectionInfo);
				var elapsedTimeInfo = new ElapsedTimeInfo();
				DateTime? startDate = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
								databaseCustomerMarketPlace.Id,
								ElapsedDataMemberType.RetrieveDataFromDatabase,
								() => Helper.GetLastAmazonOrderDate(databaseCustomerMarketPlace));

				var now = DateTime.UtcNow;

				if (!startDate.HasValue)
				{
					startDate = now.AddYears(-1);
				}
				var fromDate = startDate.Value;
				var toDate = now;

				var amazonOrdersRequestInfo = new AmazonOrdersRequestInfo
				{
					StartDate = fromDate,
					EndDate = toDate,
					MarketplaceId = securityInfo.MarketplaceId,
					MerchantId = securityInfo.MerchantId,
					ErrorRetryingInfo = _AmazonSettings,
					CustomerId = databaseCustomerMarketPlace.Customer.Id
				};

				DateTime submittedDate = now;

				log.InfoFormat("Fetching amazon orders for customer:{0} marketplace:{1}",
											  databaseCustomerMarketPlace.Customer.Id, databaseCustomerMarketPlace.Id);


				MP_AmazonOrder amazonOrder = null;

				RequestsCounterData requestsCounter = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
								databaseCustomerMarketPlace.Id,
								ElapsedDataMemberType.RetrieveDataFromExternalService,
								() => AmazonServiceHelper.GetListOrders(_ConnectionInfo, amazonOrdersRequestInfo, access, data =>
								{
									amazonOrder = Helper.StoreAmazonOrdersData(databaseCustomerMarketPlace, data, historyRecord, amazonOrder);
									return true;
								}));


				var allOrders = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
									databaseCustomerMarketPlace.Id,
									ElapsedDataMemberType.RetrieveDataFromDatabase,
									() => Helper.GetAllAmazonOrdersData(submittedDate, databaseCustomerMarketPlace));
				allOrders.RequestsCounter = requestsCounter;
				if (allOrders.Count > 0)
				{
					var bestSaledOrderItemList = AnalyseOrder(allOrders).ToList();

					if (bestSaledOrderItemList.Any())
					{
						foreach (var orderItem2 in bestSaledOrderItemList)
						{
							var orderItems = GetOrderItems(securityInfo, access, orderItem2, elapsedTimeInfo, allOrders, databaseCustomerMarketPlace.Id);

							orderItem2.OrderedItemsList = orderItems;

							if (orderItems != null)
							{
								foreach (var orderItem in orderItems)
								{
									orderItem.Categories = GetAndSaveAmazonProcuctCategoryByProductSellerSku(
										databaseCustomerMarketPlace,
										securityInfo,
										orderItem.SellerSKU,
										access, allOrders.RequestsCounter,
										elapsedTimeInfo);
								}
							}
						}
					}

					ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
								databaseCustomerMarketPlace.Id,
								ElapsedDataMemberType.StoreDataToDatabase,
								() => Helper.StoreAmazonOrdersDetailsData(databaseCustomerMarketPlace, bestSaledOrderItemList));
				}


				var aggregatedData = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
								databaseCustomerMarketPlace.Id,
								ElapsedDataMemberType.AggregateData,
								() => CreateOrdersAggregationInfo(allOrders, Helper.CurrencyConverter));
				// Save
				ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
								databaseCustomerMarketPlace.Id,
								ElapsedDataMemberType.StoreAggregatedData,
								() => Helper.StoreToDatabaseAggregatedData(databaseCustomerMarketPlace, aggregatedData, historyRecord));

				return new UpdateActionResultInfo
				{
					Name = UpdateActionResultType.OrdersCount,
					Value = (object)allOrders.Count,
					RequestsCounter = allOrders.RequestsCounter,
					ElapsedTime = elapsedTimeInfo
				};
			}
			);
		}

		private AmazonOrderItemDetailsList GetOrderItems(AmazonSecurityInfo securityInfo, ActionAccessType access,
														 AmazonOrderItem orderItem2, ElapsedTimeInfo elapsedTimeInfo,
														 AmazonOrdersList orders, int mpId)
		{
			var itemsRequestInfo = new AmazonOrdersItemsRequestInfo
			{
				MarketplaceId = securityInfo.MarketplaceId,
				MerchantId = securityInfo.MerchantId,
				OrderId = orderItem2.OrderId,
				ErrorRetryingInfo = _AmazonSettings
			};

			AmazonOrderItemDetailsList orderItems =
				ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo, mpId,
																			   ElapsedDataMemberType
																				   .RetrieveDataFromExternalService,
																			   () =>
																			   AmazonServiceHelper.GetListItemsOrdered(
																				   _ConnectionInfo, itemsRequestInfo, access,
																				   orders.RequestsCounter));
			return orderItems;
		}

		private IEnumerable<AmazonOrderItem> AnalyseOrder(AmazonOrdersList orders, int maxNumberOfItems = 10)
		{
			if (orders == null)
			{
				return null;
			}

			var shippedOrders = orders.Where(o => o.OrderStatus == AmazonOrdersList2ItemStatusType.Shipped).ToList();

			if (shippedOrders.Count() <= maxNumberOfItems)
			{
				return orders;
			}

			var rez =
				shippedOrders.GroupBy(x => new { x.OrderTotal.Value, x.OrderTotal.CurrencyCode, x.NumberOfItemsShipped },
									  (key, group) =>
									  new
									  {
										  Price = key.Value,
										  key.CurrencyCode,
										  key.NumberOfItemsShipped,
										  Counter = group.Count()
									  }).OrderByDescending(x => x.Counter).Take(maxNumberOfItems);


			return rez.Select(x => shippedOrders.First(oi => oi.OrderTotal.Value == x.Price &&
														oi.NumberOfItemsShipped == x.NumberOfItemsShipped &&
														oi.OrderTotal.CurrencyCode == x.CurrencyCode)).ToList();
		}

		private MP_EbayAmazonCategory[] GetAndSaveAmazonProcuctCategory(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
												AmazonProductsRequestInfoBySellerSku requestInfo,
												ActionAccessType access,
												RequestsCounterData requestCounter,
												ElapsedTimeInfo elapsedTimeInfo)
		{
			MP_EbayAmazonCategory[] categories = null;


			AmazonProductItemBase productItem = null;
			try
			{
				productItem = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
									databaseCustomerMarketPlace.Id,
									ElapsedDataMemberType.RetrieveDataFromExternalService,
									() => AmazonServiceHelper.GetProductCategories(_ConnectionInfo, requestInfo, access, requestCounter));
			}
			catch (MarketplaceWebServiceProductsException)
			{
				// продукт не найдет либо невозможно получить
			}

			if (productItem != null)
			{
				var marketplace = databaseCustomerMarketPlace.Marketplace;
				categories = Helper.AddAmazonCategories(marketplace, productItem, elapsedTimeInfo, databaseCustomerMarketPlace.Id);
			}
			return categories;
		}

		private MP_EbayAmazonCategory[] GetAndSaveAmazonProcuctCategoryByProductSellerSku(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
												AmazonSecurityInfo securityInfo,
												string sellerSku,
												ActionAccessType access,
												RequestsCounterData requestCounter,
												ElapsedTimeInfo elapsedTimeInfo)
		{

			var categories = Helper.FindAmazonCategoryByProductSellerSKU(sellerSku, elapsedTimeInfo, databaseCustomerMarketPlace.Id);

			if (categories == null)
			{
				var requestInfo = new AmazonProductsRequestInfoBySellerSku
				{
					MarketplaceId = securityInfo.MarketplaceId,
					MerchantId = securityInfo.MerchantId,
					SellerSku = sellerSku,
					ErrorRetryingInfo = _AmazonSettings
				};
				categories = GetAndSaveAmazonProcuctCategory(databaseCustomerMarketPlace, requestInfo, access, requestCounter, elapsedTimeInfo);
			}

			return categories;
		}

		private IEnumerable<IWriteDataInfo<AmazonDatabaseFunctionType>> CreateOrdersAggregationInfo(AmazonOrdersList orders, ICurrencyConvertor currencyConverter)
		{

			var aggregateFunctionArray = new[]
					{
						AmazonDatabaseFunctionType.AverageItemsPerOrder,
						AmazonDatabaseFunctionType.AverageSumOfOrder, 
						AmazonDatabaseFunctionType.CancelledOrdersCount, 
						AmazonDatabaseFunctionType.NumOfOrders, 						
						AmazonDatabaseFunctionType.TotalItemsOrdered, 
						AmazonDatabaseFunctionType.TotalSumOfOrders, 
						AmazonDatabaseFunctionType.TotalSumOfOrdersAnnualized, 
						AmazonDatabaseFunctionType.OrdersCancellationRate, 
					};

			var updated = orders.SubmittedDate;

			var timePeriodData = DataAggregatorHelper.GetOrdersForPeriods(orders, (submittedDate, o) => new AmazonOrdersList(submittedDate, o));
			var factory = new AmazonOrdersAgregatorFactory();

			return DataAggregatorHelper.AggregateData(factory, timePeriodData, aggregateFunctionArray, updated, currencyConverter);
		}

		private void UpdateClientFeedbackInfo(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, AmazonSecurityInfo securityInfo, MP_CustomerMarketplaceUpdatingHistory historyRecord)
		{
			Helper.CustomerMarketplaceUpdateAction(CustomerMarketplaceUpdateActionType.UpdateFeedbackInfo, databaseCustomerMarketPlace, historyRecord, () =>
			{
				var elapsedTimeInfo = new ElapsedTimeInfo();
				var request = new AmazonUserInfo
				{
					MerchantId = securityInfo.MerchantId,
				};

				var amazonUserRatingInfo = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
								databaseCustomerMarketPlace.Id,
								ElapsedDataMemberType.RetrieveDataFromExternalService,
								() => AmazonServiceHelper.GetUserStatisticsInfo(request));

				ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
								databaseCustomerMarketPlace.Id,
								ElapsedDataMemberType.StoreDataToDatabase,
								() => ParceAndSaveUserFeedbackInfo(databaseCustomerMarketPlace, amazonUserRatingInfo, historyRecord));

				return new UpdateActionResultInfo
				{
					Name = UpdateActionResultType.FeedbackRaiting,
					Value = amazonUserRatingInfo == null ? null : (object)amazonUserRatingInfo.Rating,
					RequestsCounter = amazonUserRatingInfo == null ? null : amazonUserRatingInfo.RequestsCounter,
					ElapsedTime = elapsedTimeInfo
				};

			}
			);
		}

		private void ParceAndSaveUserFeedbackInfo(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, AmazonUserRatingInfo ratingInfo, MP_CustomerMarketplaceUpdatingHistory historyRecord)
		{
			var submittedDate = ratingInfo.SubmittedDate;

			var data = new DatabaseAmazonFeedbackData(submittedDate);

			data.UserRaining = ratingInfo.Rating;

			var timePeriodsFeedback = new[]
				{					
					TimePeriodEnum.Month,
					TimePeriodEnum.Month3,
					TimePeriodEnum.Year,
					TimePeriodEnum.Lifetime
				};

			foreach (var timePeriod in timePeriodsFeedback)
			{
				FeedbackPeriod amazonTimePeriod = AmazonDatabaseTypeConverter.ConvertToAmazonTimePeriod(timePeriod);

				var count = ratingInfo.GetFeedbackValue(amazonTimePeriod, FeedbackType.Count);
				var negativePercent = ratingInfo.GetFeedbackValue(amazonTimePeriod, FeedbackType.Negative);
				var neutralPercent = ratingInfo.GetFeedbackValue(amazonTimePeriod, FeedbackType.Neutral);
				var positivePercent = ratingInfo.GetFeedbackValue(amazonTimePeriod, FeedbackType.Positive);


				var negativeNumber = ConvertToNumber(negativePercent, count);
				var neutralNumber = ConvertToNumber(neutralPercent, count);
				var positiveNumber = ConvertToNumber(positivePercent, count);

				//Debug.Assert( count == negativeNumber + neutralNumber + positiveNumber );

				data.FeedbackByPeriod.Add(timePeriod, new DatabaseAmazonFeedbackDataByPeriod(timePeriod)
				{
					Count = count,
					Negative = negativeNumber,
					Neutral = neutralNumber,
					Positive = positiveNumber
				});
			}

			Helper.StoreAmazonFeedbackData(databaseCustomerMarketPlace, data, historyRecord);
		}

		private int? ConvertToNumber(int? percent, int? count)
		{
			if (!percent.HasValue || !count.HasValue)
			{
				return null;
			}

			double percentValue = percent.Value;
			double countValue = count.Value;

			double rez = percentValue / 100 * countValue;

			return (int)Math.Round(rez, MidpointRounding.AwayFromZero);

		}
	}
}
