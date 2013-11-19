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
	using EZBob.DatabaseLib.DatabaseWrapper.Inventory;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;
	using EZBob.DatabaseLib.DatabaseWrapper.Products;
	using EZBob.DatabaseLib.DatabaseWrapper.ValueType;
	using EZBob.DatabaseLib.Model.Marketplaces.Amazon;
	using AmazonDbLib;
	using AmazonServiceLib;
	using AmazonServiceLib.Common;
	using AmazonServiceLib.Config;
	using AmazonServiceLib.Inventory.Model;
	using AmazonServiceLib.MarketWebService.Model;
	using AmazonServiceLib.Orders.Model;
	using AmazonServiceLib.UserInfo;
	using CommonLib;
	using CommonLib.TimePeriodLogic;
	using MarketplaceWebServiceProducts;
	using StructureMap;
	using EZBob.DatabaseLib.Model.Database;
	using log4net;

	public class AmazonRetriveDataHelper : MarketplaceRetrieveDataHelperBase<AmazonDatabaseFunctionType>
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(AmazonRetriveDataHelper));
        private readonly AmazonServiceConnectionInfo _ConnectionInfo;
		private readonly IAmazonMarketplaceSettings _AmazonSettings;
        public AmazonRetriveDataHelper(DatabaseDataHelper helper, DatabaseMarketplaceBase<AmazonDatabaseFunctionType> marketplace)
            : base(helper, marketplace)
        {
            var connectionInfo = ObjectFactory.GetInstance<IAmazonMarketPlaceTypeConnection>();

            _ConnectionInfo = AmazonServiceConnectionFactory.CreateConnection(connectionInfo);
			_AmazonSettings = ObjectFactory.GetInstance<IAmazonMarketplaceSettings>();
        }

		protected override void InternalUpdateInfoFirst( IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, MP_CustomerMarketplaceUpdatingHistory historyRecord )
        {
			log.DebugFormat("InternalUpdateInfoFirst customer {0} amazon mp {1}", databaseCustomerMarketPlace.Customer.Id, databaseCustomerMarketPlace.DisplayName);
            var securityInfo = RetrieveCustomerSecurityInfo<AmazonSecurityInfo>(databaseCustomerMarketPlace);

            UpdateClientOrdersInfo(databaseCustomerMarketPlace, securityInfo, ActionAccessType.Full, historyRecord);
			//UpdateClientInventoryInfo( databaseCustomerMarketPlace, securityInfo, ActionAccessType.Full, historyRecord );
            UpdateClientFeedbackInfo(databaseCustomerMarketPlace, securityInfo, historyRecord);            
        }

		protected override void InternalUpdateInfo( IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, MP_CustomerMarketplaceUpdatingHistory historyRecord )
        {
			log.DebugFormat("InternalUpdateInfo customer {0} amazon mp {1}", databaseCustomerMarketPlace.Customer.Id, databaseCustomerMarketPlace.DisplayName);
			var securityInfo = RetrieveCustomerSecurityInfo<AmazonSecurityInfo>( databaseCustomerMarketPlace );

			UpdateClientOrdersInfo( databaseCustomerMarketPlace, securityInfo, ActionAccessType.Limit, historyRecord );
			//UpdateClientInventoryInfo( databaseCustomerMarketPlace, securityInfo, ActionAccessType.Limit, historyRecord );
			UpdateClientFeedbackInfo( databaseCustomerMarketPlace, securityInfo, historyRecord );
        }

		protected override void AddAnalysisValues(IDatabaseCustomerMarketPlace marketPlace, AnalysisDataInfo data) {
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

			if (!oLatestFeedback.FeedbackByPeriodItems.IsEmpty) {
				foreach (MP_AmazonFeedbackItem afp in oLatestFeedback.FeedbackByPeriodItems) {
					var timePeriod = TimePeriodFactory.CreateById(afp.TimePeriod.InternalId);

					var c = new AnalysisDataParameterInfo("Number of reviews", timePeriod, DatabaseValueType.Integer, afp.Count);
					var g = new AnalysisDataParameterInfo("Negative Feedback rate", timePeriod, DatabaseValueType.Integer, afp.Negative);
					var n = new AnalysisDataParameterInfo("Neutral Feedback rate", timePeriod, DatabaseValueType.Integer, afp.Neutral);
					var p = new AnalysisDataParameterInfo("Positive Feedback Rate", timePeriod, DatabaseValueType.Integer, afp.Positive);

					if (timePeriod.TimePeriodType == TimePeriodEnum.Year) {
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
				data.AddData(oLatestFeedback.HistoryRecord.UpdatingStart.Value, feedBackParams);
		} // AddAnalysisValues

		public override IMarketPlaceSecurityInfo RetrieveCustomerSecurityInfo( int customerMarketPlaceId )
		{
			return RetrieveCustomerSecurityInfo<AmazonSecurityInfo>( GetDatabaseCustomerMarketPlace( customerMarketPlaceId ) );
		}

        /*public void UpdateClientInventoryInfo( IDatabaseCustomer databaseCustomer, ActionAccessType access )
        {
            base.UpdateAllDataFor( customerMarketPlace => UpdateClientInventoryInfo(customerMarketPlace, access), databaseCustomer );
        }

        private void UpdateClientInventoryInfo( IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, ActionAccessType access )
        {
            var securityInfo = RetrieveCustomerSecurityInfo<AmazonSecurityInfo>( databaseCustomerMarketPlace );

            UpdateClientInventoryInfo( databaseCustomerMarketPlace, securityInfo, _ConnectionInfo, access );
        }*/

        private void UpdateClientInventoryInfo(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, AmazonSecurityInfo securityInfo, ActionAccessType access, MP_CustomerMarketplaceUpdatingHistory historyRecord)
        {
            Helper.CustomerMarketplaceUpdateAction(CustomerMarketplaceUpdateActionType.UpdateInventoryInfo, databaseCustomerMarketPlace, historyRecord, () =>
	            {
					var elapsedTimeInfo = new ElapsedTimeInfo();
					DateTime? startDate = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds( elapsedTimeInfo,
									ElapsedDataMemberType.RetrieveDataFromDatabase,
									() => Helper.GetLastInventoryRequest( databaseCustomerMarketPlace ) );
					
		            var now = DateTime.UtcNow;

		            if (!startDate.HasValue)
		            {
			            startDate = now.AddYears(-1);
		            }
		            var fromDate = startDate.Value;
		            var toDate = now;

		            var amazonInventoryRequestInfo = new AmazonInventoryRequestInfo
			                                            {
				                                            MarketplaceId = securityInfo.MarketplaceId,
				                                            MerchantId = securityInfo.MerchantId,
				                                            StartDate = fromDate,
															EndDate = toDate,
															ErrorRetryingInfo = _AmazonSettings.ErrorRetryingInfo
			                                            };

					var inventories = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds( elapsedTimeInfo,
									ElapsedDataMemberType.RetrieveDataFromExternalService,
									() => AmazonServiceHelper.GetUserInventorList( _ConnectionInfo, amazonInventoryRequestInfo, access ) );

		            ParceAndSaveInventoryInfo(databaseCustomerMarketPlace, inventories, historyRecord, elapsedTimeInfo);

					return new UpdateActionResultInfo
					{
						Name = UpdateActionResultType.InventoryItemsCount,
						Value = inventories == null ? null : (object)inventories.Count,
						RequestsCounter = inventories == null ? null : inventories.RequestsCounter,
						ElapsedTime = elapsedTimeInfo
					};
	            }
            );
        }

        private void UpdateClientOrdersInfo(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, AmazonSecurityInfo securityInfo, ActionAccessType access, MP_CustomerMarketplaceUpdatingHistory historyRecord)
        {
			log.DebugFormat("UpdateClientOrdersInfo customer {0}, amazon mp {1}, access {2}", databaseCustomerMarketPlace.Customer.Id, databaseCustomerMarketPlace.DisplayName, access);

            Helper.CustomerMarketplaceUpdateAction(CustomerMarketplaceUpdateActionType.UpdateOrdersInfo, databaseCustomerMarketPlace, historyRecord, () =>
				{
					// save data to order table
					//var webServiceConfigurator = CreateServiceReportsConfigurator(connectionInfo);
					var elapsedTimeInfo = new ElapsedTimeInfo();
					DateTime? startDate = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds( elapsedTimeInfo,
									ElapsedDataMemberType.RetrieveDataFromDatabase,
									() => Helper.GetLastAmazonOrdersRequest( databaseCustomerMarketPlace ) );

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
													ErrorRetryingInfo = _AmazonSettings.ErrorRetryingInfo,
													CustomerId = databaseCustomerMarketPlace.Customer.Id
												};

					DateTime submittedDate;

					log.InfoFormat("Fetching amazon orders for customer:{0} marketplace:{1}",
												  databaseCustomerMarketPlace.Customer.Id, databaseCustomerMarketPlace.Id);
					
					var orders = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds( elapsedTimeInfo,
									ElapsedDataMemberType.RetrieveDataFromExternalService,
									() => AmazonServiceHelper.GetListOrders( _ConnectionInfo, amazonOrdersRequestInfo, access ) );

					if ( orders != null )
					{
						var bestSaledOrderItemList = AnalyseOrder( orders );

						if ( bestSaledOrderItemList != null )
						{
							foreach ( var orderItem2 in bestSaledOrderItemList )
							{
								var orderItems = GetOrderItems(securityInfo, access, orderItem2, elapsedTimeInfo, orders);

							    orderItem2.OrderedItemsList = orderItems;

								if ( orderItems != null )
								{
									foreach ( var orderItem in orderItems )
									{
										orderItem.Categories = GetAndSaveAmazonProcuctCategoryByProductSellerSku( databaseCustomerMarketPlace, securityInfo, orderItem.SellerSKU, access, orders.RequestsCounter, elapsedTimeInfo );
									}
								}
							}
						}

						ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds( elapsedTimeInfo,
									ElapsedDataMemberType.StoreDataToDatabase,
									() => Helper.StoreAmazonOrdersData( databaseCustomerMarketPlace, /*ordersList,*/ orders, historyRecord ) );

						submittedDate = orders.SubmittedDate;
					}
					else
					{
						submittedDate = now;
					}

					var allOrders = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds( elapsedTimeInfo,
									ElapsedDataMemberType.RetrieveDataFromDatabase,
									() => Helper.GetAllAmazonOrdersData( submittedDate, databaseCustomerMarketPlace ) );

					var aggregatedData = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds( elapsedTimeInfo,
									ElapsedDataMemberType.AggregateData,
									() => CreateOrdersAggregationInfo( allOrders, Helper.CurrencyConverter ) );
					// Save
					ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds( elapsedTimeInfo,
									ElapsedDataMemberType.StoreAggregatedData,
									() => Helper.StoreToDatabaseAggregatedData( databaseCustomerMarketPlace, aggregatedData, historyRecord ) );


					return new UpdateActionResultInfo
					{
						Name = UpdateActionResultType.OrdersCount,
						Value = orders == null ? null : (object)orders.Count,
						RequestsCounter = orders == null ? null : orders.RequestsCounter,
						ElapsedTime = elapsedTimeInfo
					};

				}
			);
		}

        private AmazonOrderItemDetailsList GetOrderItems(AmazonSecurityInfo securityInfo, ActionAccessType access,
                                                         AmazonOrderItem2 orderItem2, ElapsedTimeInfo elapsedTimeInfo,
                                                         AmazonOrdersList2 orders)
        {
            var itemsRequestInfo = new AmazonOrdersItemsRequestInfo
                {
                    MarketplaceId = securityInfo.MarketplaceId,
                    MerchantId = securityInfo.MerchantId,
                    OrderId = orderItem2.AmazonOrderId,
                    ErrorRetryingInfo = _AmazonSettings.ErrorRetryingInfo
                };

            AmazonOrderItemDetailsList orderItems =
                ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
                                                                               ElapsedDataMemberType
                                                                                   .RetrieveDataFromExternalService,
                                                                               () =>
                                                                               AmazonServiceHelper.GetListItemsOrdered(
                                                                                   _ConnectionInfo, itemsRequestInfo, access,
                                                                                   orders.RequestsCounter));
            return orderItems;
        }

        private IEnumerable<AmazonOrderItem2> AnalyseOrder( AmazonOrdersList2 orders, int maxNumberOfItems = 10 )
		{
			if ( orders == null )
			{
				return null;
			}

			var shippedOrders = orders.Where( o => o.OrderStatus == AmazonOrdersList2ItemStatusType.Shipped ).ToList();

			if ( shippedOrders.Count() <= maxNumberOfItems )
			{
				return orders;
			}

		    var rez =
		        shippedOrders.GroupBy(x => new {x.OrderTotal.Value, x.OrderTotal.CurrencyCode, x.NumberOfItemsShipped},
		                              (key, group) =>
		                              new
		                                  {
		                                      Price = key.Value,
		                                      key.CurrencyCode,
		                                      key.NumberOfItemsShipped,
		                                      Counter = group.Count()
		                                  }).OrderByDescending(x => x.Counter).Take(maxNumberOfItems);
                

			return rez.Select( x => shippedOrders.First( oi => oi.OrderTotal.Value == x.Price &&
														oi.NumberOfItemsShipped == x.NumberOfItemsShipped &&
														oi.OrderTotal.CurrencyCode == x.CurrencyCode ) ).ToList();
		}

		private MP_EbayAmazonCategory[] GetAndSaveAmazonProcuctCategory( IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
												AmazonProductsRequestBase requestInfo,
												ActionAccessType access,
												RequestsCounterData requestCounter,
												ElapsedTimeInfo elapsedTimeInfo )
		{
			MP_EbayAmazonCategory[] categories = null;


			AmazonProductItemBase productItem = null;
			try
			{
				productItem = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds( elapsedTimeInfo,
									ElapsedDataMemberType.RetrieveDataFromExternalService,
									() => AmazonServiceHelper.GetProductCategories( _ConnectionInfo, requestInfo, access, requestCounter ) );
			}
			catch ( MarketplaceWebServiceProductsException )
			{
				// продукт не найдет либо невозможно получить
			}

			if ( productItem != null )
			{
				var marketplace = databaseCustomerMarketPlace.Marketplace;
				categories = Helper.AddAmazonCategories( marketplace, productItem, elapsedTimeInfo );
			}
			return categories;
		}

		private MP_EbayAmazonCategory[] GetAndSaveAmazonProcuctCategoryByProductAsin( IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
												AmazonSecurityInfo securityInfo,
												string asin,
												ActionAccessType access,
												RequestsCounterData requestCounter, 
												ElapsedTimeInfo elapsedTimeInfo)
		{
			var categories = Helper.FindAmazonCategoryByProductAsin( asin, elapsedTimeInfo );

			if ( categories == null )
			{
				var requestInfo = new AmazonProductsRequestInfoByAsin
				{
					MarketplaceId = securityInfo.MarketplaceId,
					MerchantId = securityInfo.MerchantId,
					ProductASIN = asin,
					ErrorRetryingInfo = _AmazonSettings.ErrorRetryingInfo
				};
				categories = GetAndSaveAmazonProcuctCategory( databaseCustomerMarketPlace, requestInfo, access, requestCounter, elapsedTimeInfo );
			}

			return categories;
		}

		private MP_EbayAmazonCategory[] GetAndSaveAmazonProcuctCategoryByProductSellerSku( IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
												AmazonSecurityInfo securityInfo,
												string sellerSku,
												ActionAccessType access,
												RequestsCounterData requestCounter, 
												ElapsedTimeInfo elapsedTimeInfo)
		{

			var categories = Helper.FindAmazonCategoryByProductSellerSKU( sellerSku, elapsedTimeInfo );

			if ( categories == null )
			{
				var requestInfo = new AmazonProductsRequestInfoBySellerSku
				{
					MarketplaceId = securityInfo.MarketplaceId,
					MerchantId = securityInfo.MerchantId,
					SellerSku = sellerSku,
					ErrorRetryingInfo = _AmazonSettings.ErrorRetryingInfo
				};
				categories = GetAndSaveAmazonProcuctCategory( databaseCustomerMarketPlace, requestInfo, access, requestCounter, elapsedTimeInfo );
			}

			return categories;
		}

		//private readonly ConcurrentDictionary<string, AmazonProductItemBase> _ProductsCache = new ConcurrentDictionary<string, AmazonProductItemBase>();

		/*public void UpdateClientOrdersInfo( IDatabaseCustomer databaseCustomer, ActionAccessType access )
        {
            base.UpdateAllDataFor( customerMarketPlace => UpdateClientOrdersInfo(customerMarketPlace , access ), databaseCustomer );
        }

        private void UpdateClientOrdersInfo( IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, ActionAccessType access )
        {
            var securityInfo = RetrieveCustomerSecurityInfo<AmazonSecurityInfo>( databaseCustomerMarketPlace );

            UpdateClientOrdersInfo( databaseCustomerMarketPlace, securityInfo, _ConnectionInfo, access);
        }*/

		/*public void StoreCustomerMarketPlace( ICustomerMarketPlace databaseCustomerMarketPlace )
		{
			var marketPlace = databaseCustomerMarketPlace.MarketPlace;
			var databaseCustomer = databaseCustomerMarketPlace.Customer;
			var securityInfo = databaseCustomerMarketPlace.SecurityData;
			var securityData = SerializeDataHelper.Serialize( securityInfo );

			Helper.AddCustomerMarketPlace(databaseCustomer, marketPlace, securityData, "");
		}*/

		private IEnumerable<IWriteDataInfo<AmazonDatabaseFunctionType>> CreateOrdersAggregationInfo(AmazonOrdersList2 orders,  ICurrencyConvertor currencyConverter)
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
			
			var timePeriodData = DataAggregatorHelper.GetOrdersForPeriods(orders, (submittedDate, o) => new AmazonOrdersList2(submittedDate, o));
			var factory = new AmazonOrdersAgregatorFactory();

			return DataAggregatorHelper.AggregateData( factory, timePeriodData, aggregateFunctionArray, updated, currencyConverter );
        }

        private void ParceAndSaveInventoryInfo(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, AmazonInventoryData data, MP_CustomerMarketplaceUpdatingHistory historyRecord, ElapsedTimeInfo elapsedTimeInfo)
        {            
            if (data == null)
            {
                return;
            }
			
            var submittedDate = data.SubmittedDate;

			var databaseInventoryItems = data.Select(s => new DatabaseInventoryItem
                                                            {
                                                                ItemId = s.ItemID,
																Amount = new AmountInfo { Value = s.Price, CurrencyCode = CurrencyConvertor.BaseCurrency },
                                                                Quantity = s.Quantity,
                                                                Sku = s.SKU
                                                            });

            var databaseInvetoryList = new DatabaseInventoryList(submittedDate, databaseInventoryItems)
            {
                UseAFN = data.UseAFN,
            };

			ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds( elapsedTimeInfo,
									ElapsedDataMemberType.StoreDataToDatabase,
									() => Helper.StoreToDatabaseInventoryData( databaseCustomerMarketPlace, databaseInvetoryList, historyRecord ) );

			var totalItemInInventory = new WriteDataInfo<AmazonDatabaseFunctionType>
			{
				UpdatedDate = submittedDate,
				Value = databaseInvetoryList.Count,
				TimePeriodType = TimePeriodEnum.Lifetime,
				FunctionType = AmazonDatabaseFunctionType.InventoryTotalItems
			};

			var sum = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds( elapsedTimeInfo,
									ElapsedDataMemberType.AggregateData,
									() => databaseInvetoryList.Sum( i => i.Amount.Value * i.Quantity ) );

	        var totalValueOfInventory = new WriteDataInfo<AmazonDatabaseFunctionType>
			{
				UpdatedDate = submittedDate,
				TimePeriodType = TimePeriodEnum.Lifetime,
				FunctionType = AmazonDatabaseFunctionType.InventoryTotalValue,
				Value = sum
			};

			ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds( elapsedTimeInfo,
									ElapsedDataMemberType.StoreAggregatedData,
									() => Helper.StoreToDatabaseAggregatedData( databaseCustomerMarketPlace, new[] { totalItemInInventory, totalValueOfInventory, }, historyRecord ) );

        }

        /*public void UpdateClientFeedbackInfo( IDatabaseCustomer databaseCustomer )
        {
            base.UpdateAllDataFor( UpdateClientFeedbackInfo, databaseCustomer );
        }

        public void UpdateClientFeedbackInfo( IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, MP_CustomerMarketPlaceUpdatingHistory historyRecord )
        {
            AmazonSecurityInfo securityInfo = RetrieveCustomerSecurityInfo<AmazonSecurityInfo>( databaseCustomerMarketPlace );

            UpdateClientFeedbackInfo( databaseCustomerMarketPlace, securityInfo, historyRecord);
        }*/

        private void UpdateClientFeedbackInfo(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, AmazonSecurityInfo securityInfo, MP_CustomerMarketplaceUpdatingHistory historyRecord)
        {
            Helper.CustomerMarketplaceUpdateAction(CustomerMarketplaceUpdateActionType.UpdateFeedbackInfo, databaseCustomerMarketPlace, historyRecord, () =>
	            {
					var elapsedTimeInfo = new ElapsedTimeInfo();
		            var request = new AmazonUserInfo
			                            {
				                            MerchantId = securityInfo.MerchantId,
			                            };

					var amazonUserRatingInfo = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds( elapsedTimeInfo,
									ElapsedDataMemberType.RetrieveDataFromExternalService,
									() => AmazonServiceHelper.GetUserStatisticsInfo( request ) );

					ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds( elapsedTimeInfo,
									ElapsedDataMemberType.StoreDataToDatabase,
									() => ParceAndSaveUserFeedbackInfo( databaseCustomerMarketPlace, amazonUserRatingInfo, historyRecord ) );

					return new UpdateActionResultInfo
					{
						Name = UpdateActionResultType.FeedbackRaiting,
						Value = amazonUserRatingInfo == null? null : (object)amazonUserRatingInfo.Rating,
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
