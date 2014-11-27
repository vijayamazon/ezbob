namespace EzBob.eBayLib
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.DatabaseWrapper;
	using EZBob.DatabaseLib.DatabaseWrapper.EbayFeedbackData;
	using EZBob.DatabaseLib.DatabaseWrapper.FunctionValues;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;
	using EZBob.DatabaseLib.DatabaseWrapper.ValueType;
	using EZBob.DatabaseLib.Model.Database;
	using CommonLib;
	using CommonLib.MarketplaceSpecificTypes.TeraPeakOrdersData;
	using CommonLib.ReceivedDataListLogic;
	using CommonLib.TimePeriodLogic;
	using Ezbob.Utils;
	using TeraPeakServiceLib;
	using TeraPeakServiceLib.Requests.SellerResearch;
	using eBayDbLib;
	using Config;
	using eBayServiceLib;
	using eBayServiceLib.Common;
	using eBayServiceLib.TradingServiceCore;
	using eBayServiceLib.TradingServiceCore.DataInfos.Orders;
	using eBayServiceLib.TradingServiceCore.DataProviders.Model.Base;
	using eBayServiceLib.TradingServiceCore.DataProviders.Model.TokenDependant;
	using eBayServiceLib.TradingServiceCore.DataProviders.Model.TokenDependant.GetOrders;
	using eBayServiceLib.TradingServiceCore.ResultInfos;
	using eBayServiceLib.TradingServiceCore.ResultInfos.Orders;
	using eBayServiceLib.TradingServiceCore.TokenProvider;
	using eBayServiceLib.com.ebay.developer.soap;
	using StructureMap;

	public class eBayRetriveDataHelper : MarketplaceRetrieveDataHelperBase<eBayDatabaseFunctionType>
	{
		#region Nested Types

		internal enum UpdateStrategyType
		{
			OnlyTeraPeak,
			EbayGetOrdersAfterTeraPeak
		}

		internal abstract class UpdateStaretagy
		{
			public static UpdateStaretagy CreateStrategy(eBayRetriveDataHelper helper, UpdateStrategyType updateStrategyType)
			{
				switch (updateStrategyType)
				{
					case UpdateStrategyType.OnlyTeraPeak:
						return new UpdateStaretagyTeraPeakOnly(helper);

					case UpdateStrategyType.EbayGetOrdersAfterTeraPeak:
						return new UpdateStaretagyUpdateEbayGetOrdersAfterTeraPeak(helper);

					default:
						throw new NotSupportedException();
				}
			}

			protected eBayRetriveDataHelper Helper { get; private set; }

			protected UpdateStaretagy(eBayRetriveDataHelper helper)
			{
				Helper = helper;
			}

			public abstract void Run(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, DataProviderCreationInfo info, MP_CustomerMarketplaceUpdatingHistory historyRecord);

		}

		private class UpdateStaretagyTeraPeakOnly : UpdateStaretagy
		{
			public UpdateStaretagyTeraPeakOnly(eBayRetriveDataHelper helper) : base(helper)
			{
			}

			public override void Run(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, DataProviderCreationInfo info,
			                         MP_CustomerMarketplaceUpdatingHistory historyRecord)
			{
				Helper.UpdateTeraPeakOrders(databaseCustomerMarketPlace, databaseCustomerMarketPlace.DisplayName, historyRecord);

				Helper.CheckTokenStatus(databaseCustomerMarketPlace, info, historyRecord);
				Helper.UpdateAccountInfo(databaseCustomerMarketPlace, info, historyRecord);
				Helper.UpdateUserInfo(databaseCustomerMarketPlace, info, historyRecord);
				Helper.UpdateFeedbackInfo(databaseCustomerMarketPlace, info, historyRecord);
			}
		}

		private class UpdateStaretagyUpdateEbayGetOrdersAfterTeraPeak : UpdateStaretagy
		{
			public UpdateStaretagyUpdateEbayGetOrdersAfterTeraPeak(eBayRetriveDataHelper helper) : base(helper)
			{
			}

			public override void Run(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, DataProviderCreationInfo info,
			                         MP_CustomerMarketplaceUpdatingHistory historyRecord)
			{
				Helper.CheckTokenStatus(databaseCustomerMarketPlace, info, historyRecord);
				Helper.UpdateAccountInfo(databaseCustomerMarketPlace, info, historyRecord);
				Helper.UpdateUserInfo(databaseCustomerMarketPlace, info, historyRecord);
				Helper.UpdateFeedbackInfo(databaseCustomerMarketPlace, info, historyRecord);
				Helper.UpdateTeraPeakOrdersThenEbayOrders(databaseCustomerMarketPlace, info, databaseCustomerMarketPlace.DisplayName, historyRecord);
			}
		}
		#endregion

		#region Fields
		private static readonly int MaxPossibleRetriveMonthsFromTeraPeak = 12;

		private readonly EbayServiceConnectionInfo _EbayConnectionInfo;
		private readonly IEbayMarketplaceSettings _Settings;
		private readonly UpdateStaretagy _UpdateStaretagy;
		#endregion

		#region .ctor
		public eBayRetriveDataHelper(DatabaseDataHelper helper, DatabaseMarketplaceBase<eBayDatabaseFunctionType> marketplace)
			:base(helper, marketplace)
        {
			_Settings = ObjectFactory.GetInstance<IEbayMarketplaceSettings>();

			var ebayConnectionInfo = ObjectFactory.GetInstance<IEbayMarketplaceTypeConnection>();
			
			_EbayConnectionInfo = eBayServiceHelper.CreateConnection( ebayConnectionInfo );

			_UpdateStaretagy = UpdateStaretagy.CreateStrategy(this,
			                                                  _Settings.OrdersFromTeraPeakOnly
				                                                  ? UpdateStrategyType.OnlyTeraPeak
				                                                  : UpdateStrategyType.EbayGetOrdersAfterTeraPeak);
        }
		#endregion

		public override void Update(int nCustomerMarketplaceID) {
			UpdateCustomerMarketplaceFirst(nCustomerMarketplaceID);
		} // Update

		protected override ElapsedTimeInfo RetrieveAndAggregate(
			IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
			MP_CustomerMarketplaceUpdatingHistory historyRecord
		) {
			// This method is not implemented here because elapsed time is got from over source.
			throw new NotImplementedException();
		}

		protected override void InternalUpdateInfo( IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, MP_CustomerMarketplaceUpdatingHistory historyRecord )
		{
			var info = CreateProviderCreationInfo( databaseCustomerMarketPlace, _EbayConnectionInfo );

			_UpdateStaretagy.Run(databaseCustomerMarketPlace, info, historyRecord);					
		}

		protected override void AddAnalysisValues(IDatabaseCustomerMarketPlace marketPlace, AnalysisDataInfo data)
		{
			#region FeedBack

            var feedbacks = Helper.GetEbayFeedback()
                .Where(f => f.CustomerMarketPlace.Id == marketPlace.Id && f.HistoryRecord.UpdatingStart != null && f.HistoryRecord.UpdatingEnd != null)
                .Select(feedback => new{feedback, updatestart = feedback.HistoryRecord.UpdatingStart })
                .ToList();

			if ( feedbacks.Any() )
			{
				

				feedbacks.ForEach( fb =>
				{
				    var af = fb.feedback;

                    if(af != null)
					{
						var feedBackParams = new List<IAnalysisDataParameterInfo>();
					    DateTime? afDate = fb.updatestart;
					    var f = af.FeedbackByPeriodItems.ToList();
					    if ( f != null && f.Count > 0 )
					    {
						    f.ForEach( afp =>
						    {
							    var timePeriod = TimePeriodFactory.CreateById( afp.TimePeriod.InternalId );
							
							    var g = new AnalysisDataParameterInfo("Negative Feedback rate", timePeriod, DatabaseValueType.Integer, afp.Negative);
							    var n = new AnalysisDataParameterInfo("Neutral Feedback rate", timePeriod, DatabaseValueType.Integer, afp.Neutral);
							    var p = new AnalysisDataParameterInfo("Positive Feedback Rate", timePeriod, DatabaseValueType.Integer, afp.Positive);

							    if ( timePeriod.TimePeriodType == TimePeriodEnum.Year )
							    {
								    feedBackParams.Add( new AnalysisDataParameterInfo("Positive %", timePeriod, DatabaseValueType.Double, ( afp.Positive + afp.Neutral + afp.Neutral ) != 0 ? ( afp.Positive * 100 ) / ( afp.Positive + afp.Neutral + afp.Neutral ) : 0) );
							    }

							    feedBackParams.AddRange( new[] { n, g, p, } );
						    } );
					    }

					    if ( feedBackParams.Count > 0 )
						{
							if (data.Data != null && data.Data.Count > 0)
							{
								DateTime lastDate = data.Data.Keys.Max();
								data.Data[lastDate].AddRange(feedBackParams);
							}
							else
							{
								data.AddData(afDate.Value, feedBackParams);
							}
						}
                    }
				} );
                    
			}
			#endregion
		}

		public override IMarketPlaceSecurityInfo RetrieveCustomerSecurityInfo(int customerMarketPlaceId)
		{
			return RetrieveCustomerSecurityInfo<eBaySecurityInfo>( GetDatabaseCustomerMarketPlace( customerMarketPlaceId ) );
		}

		private ResultInfoEbayUser UpdateUserInfo( IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, DataProviderCreationInfo info, MP_CustomerMarketplaceUpdatingHistory historyRecord )
		{
			ResultInfoEbayUser resultInfo = null;

			Helper.CustomerMarketplaceUpdateAction( CustomerMarketplaceUpdateActionType.UpdateUserInfo, databaseCustomerMarketPlace, historyRecord, () =>
				{
					var elapsedTimeInfo = new ElapsedTimeInfo();
					resultInfo = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds( elapsedTimeInfo,
									databaseCustomerMarketPlace.Id,
									ElapsedDataMemberType.RetrieveDataFromExternalService,
									() => GetCustomerUserInfo( info ) );

					ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds( elapsedTimeInfo,
									databaseCustomerMarketPlace.Id,
									ElapsedDataMemberType.StoreDataToDatabase,
									() => SaveUserInfo( databaseCustomerMarketPlace, resultInfo, historyRecord ) );

					return new UpdateActionResultInfo
					{
						Name = UpdateActionResultType.FeedbackRatingStar,
						Value = resultInfo == null ? null : (object)resultInfo.FeedbackRatingStar,
						RequestsCounter = resultInfo == null ? null : resultInfo.RequestsCounter,
						ElapsedTime = elapsedTimeInfo
					};
				}
			);


			return resultInfo;
		}

		private void SaveUserInfo(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, ResultInfoEbayUser infoEbayUser, MP_CustomerMarketplaceUpdatingHistory historyRecord)
		{
			Helper.StoreEbayUserData( databaseCustomerMarketPlace, infoEbayUser, historyRecord );						
		}

		private void CheckTokenStatus( IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, DataProviderCreationInfo info, MP_CustomerMarketplaceUpdatingHistory historyRecord)
		{
			Helper.CustomerMarketplaceUpdateAction( CustomerMarketplaceUpdateActionType.UpdateAccountInfo, databaseCustomerMarketPlace, historyRecord, () =>
				{
					//var checker = new DataProviderGetTokenStatus(info);
					var elapsedTimeInfo = new ElapsedTimeInfo();
					//var result = checker.GetStatus();
					var checker = new DataProviderCheckAuthenticationToken( info );

					var result = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds( elapsedTimeInfo,
									databaseCustomerMarketPlace.Id,
									ElapsedDataMemberType.RetrieveDataFromExternalService,
									() => checker.Check() );

					return new UpdateActionResultInfo
						{
							Name = UpdateActionResultType.GetTokenStatus,
							RequestsCounter = result == null? null: result.RequestsCounter,
							ElapsedTime = elapsedTimeInfo
						};

				} );
		}

		private void UpdateAccountInfo(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, DataProviderCreationInfo info, MP_CustomerMarketplaceUpdatingHistory historyRecord)
		{
			Helper.CustomerMarketplaceUpdateAction( CustomerMarketplaceUpdateActionType.UpdateAccountInfo, databaseCustomerMarketPlace, historyRecord, () =>
				{
					var account = new DataProviderGetAccount( info );
					var elapsedTimeInfo = new ElapsedTimeInfo();
					var resultInfo = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds( elapsedTimeInfo,
									databaseCustomerMarketPlace.Id,
									ElapsedDataMemberType.RetrieveDataFromExternalService,
									() => account.GetAccount() );
					
					ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
									databaseCustomerMarketPlace.Id,
									ElapsedDataMemberType.StoreDataToDatabase, 
									() => SaveAccountInfo( databaseCustomerMarketPlace, resultInfo, historyRecord ));

					return new UpdateActionResultInfo
					{
						Name = UpdateActionResultType.CurrentBalance,
						Value = resultInfo == null ? null : (object)resultInfo.CurrentBalance,
						RequestsCounter = resultInfo == null? null: resultInfo.RequestsCounter,
						ElapsedTime = elapsedTimeInfo
					};
				}
			);
		}

		

		private void SaveAccountInfo(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, ResultInfoEbayAccount accountInfo, MP_CustomerMarketplaceUpdatingHistory historyRecord)
		{
			Helper.StoreEbayUserAccountData( databaseCustomerMarketPlace, accountInfo, historyRecord );
		}

		private void UpdateFeedbackInfo(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, DataProviderCreationInfo info, MP_CustomerMarketplaceUpdatingHistory historyRecord)
		{
			Helper.CustomerMarketplaceUpdateAction( CustomerMarketplaceUpdateActionType.UpdateFeedbackInfo, databaseCustomerMarketPlace, historyRecord, () =>
				{
					var elapsedTimeInfo = new ElapsedTimeInfo();
					var resultInfo = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds( elapsedTimeInfo,
									databaseCustomerMarketPlace.Id,
									ElapsedDataMemberType.RetrieveDataFromExternalService,
									() => DataProviderGetFeedback.GetFeedBack( info ) );

					ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds( elapsedTimeInfo,
									databaseCustomerMarketPlace.Id,
									ElapsedDataMemberType.StoreDataToDatabase,
									() => SaveFeedbackInfo( databaseCustomerMarketPlace, resultInfo, historyRecord ) );

					var ebayRaitingInfo = resultInfo == null? null: resultInfo.GetRaitingData(FeedbackSummaryPeriodCodeType.FiftyTwoWeeks, FeedbackRatingDetailCodeType.ShippingAndHandlingCharges);					
					
					return new UpdateActionResultInfo
					{
						Name = UpdateActionResultType.FeedbackRaiting,
						Value = ebayRaitingInfo  == null? null : (object)ebayRaitingInfo.Value,
						RequestsCounter = resultInfo == null ? null : resultInfo.RequestsCounter,
						ElapsedTime = elapsedTimeInfo
					};
				}
			);
			
		}

		private void SaveFeedbackInfo(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, ResultInfoEbayFeedBack feedbackInfo, MP_CustomerMarketplaceUpdatingHistory historyRecord)
		{
			if ( feedbackInfo == null )
			{
				return;
			}

			var submittedDate = feedbackInfo.SubmittedDate;

			var data = new DatabaseEbayFeedbackData(submittedDate)
			    {
			        RepeatBuyerCount = feedbackInfo.RepeatBuyerCount,
			        RepeatBuyerPercent = feedbackInfo.RepeatBuyerPercent,
			        TransactionPercent = feedbackInfo.TransactionPercent,
			        UniqueBuyerCount = feedbackInfo.UniqueBuyerCount,
			        UniqueNegativeCount = feedbackInfo.UniqueNegativeFeedbackCount,
			        UniquePositiveCount = feedbackInfo.UniquePositiveFeedbackCount,
			        UniqueNeutralCount = feedbackInfo.UniqueNeutralFeedbackCount
			    };

			var timePeriodsFeedback = new[]
				{
					TimePeriodEnum.Zero,
					TimePeriodEnum.Month,
					TimePeriodEnum.Month6,
					TimePeriodEnum.Year
				};

			foreach ( TimePeriodEnum timePeriod in timePeriodsFeedback )
			{
				data.FeedbackByPeriod.Add( timePeriod, new DatabaseEbayFeedbackDataByPeriod( timePeriod )
					{
						Negative = feedbackInfo.GetNegativeFeedbackByPeriod( timePeriod ),
						Neutral = feedbackInfo.GetNeutralFeedbackByPeriod( timePeriod ),
						Positive = feedbackInfo.GetPositiveFeedbackByPeriod( timePeriod ),
					} );

				
			}

			var timePeriodsRaiting = new[]
				{ 
					new KeyValuePair<TimePeriodEnum, FeedbackSummaryPeriodCodeType>(TimePeriodEnum.Month, FeedbackSummaryPeriodCodeType.ThirtyDays),
					new KeyValuePair<TimePeriodEnum, FeedbackSummaryPeriodCodeType>(TimePeriodEnum.Year, FeedbackSummaryPeriodCodeType.FiftyTwoWeeks)
				};

			foreach (var pair in timePeriodsRaiting)
			{
				var timePeriod = pair.Key;
				var periodCodeType = pair.Value;
				data.RaitingByPeriod.Add( timePeriod, new DatabaseEbayRaitingDataByPeriod( timePeriod )
					{
						Communication = feedbackInfo.GetRaitingData(periodCodeType, FeedbackRatingDetailCodeType.Communication),
						ItemAsDescribed = feedbackInfo.GetRaitingData(periodCodeType, FeedbackRatingDetailCodeType.ItemAsDescribed),
						ShippingAndHandlingCharges = feedbackInfo.GetRaitingData( periodCodeType, FeedbackRatingDetailCodeType.ShippingAndHandlingCharges ),
						ShippingTime = feedbackInfo.GetRaitingData(periodCodeType, FeedbackRatingDetailCodeType.ShippingTime)
					} );
			}

			Helper.StoreEbayFeedbackData( databaseCustomerMarketPlace, data, historyRecord );
		}

		private void UpdateTeraPeakOrdersThenEbayOrders(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
		                              DataProviderCreationInfo info,
		                              string ebayUserID,
		                              MP_CustomerMarketplaceUpdatingHistory historyRecord)
		{
			Helper.CustomerMarketplaceUpdateAction(CustomerMarketplaceUpdateActionType.UpdateOrdersInfo, databaseCustomerMarketPlace, historyRecord, () =>
			{
				var resultInfo = new UpdateActionResultInfo
				{
					Name = UpdateActionResultType.OrdersCount,
					Value = 0
				};
				DateTime? startDate = UpdateTeraPeakOrdersBeforeEbay(databaseCustomerMarketPlace, ebayUserID, historyRecord, resultInfo);
				UpdateEbayGetOrdersAfterTeraPeak(databaseCustomerMarketPlace, info, historyRecord, startDate, resultInfo);

				return resultInfo;
			}
				);
		}

		private void UpdateEbayGetOrdersAfterTeraPeak(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
	                                          DataProviderCreationInfo info, MP_CustomerMarketplaceUpdatingHistory historyRecord,
                                              DateTime? startDate, UpdateActionResultInfo countAllOrders)
	    {
	        Helper.CustomerMarketplaceUpdateAction(CustomerMarketplaceUpdateActionType.EbayGetOrders,
	                                                databaseCustomerMarketPlace, 
													historyRecord, 
													() =>
					{
						var now = DateTime.UtcNow;
						var elapsedTimeInfo = new ElapsedTimeInfo();
						if (!startDate.HasValue)
						{
							startDate = now.AddYears(-1);
						}
						var fromDate = startDate.Value;
						var toDate = now;

						ResultInfoOrders orders = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds( elapsedTimeInfo,
									databaseCustomerMarketPlace.Id,
									ElapsedDataMemberType.RetrieveDataFromExternalService,
									() => DataProviderGetOrders.GetOrders( info, new ParamsDataInfoGetOrdersFromDateToDateCreated( fromDate, toDate ) ) );

						var databaseOrdersList = ParseOrdersInfo(orders);

						ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds( elapsedTimeInfo,
									databaseCustomerMarketPlace.Id,
									ElapsedDataMemberType.StoreDataToDatabase,
									() => Helper.AddEbayOrdersData( databaseCustomerMarketPlace, databaseOrdersList, historyRecord ) );

						var allEBayOrders = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds( elapsedTimeInfo,
									databaseCustomerMarketPlace.Id,
									ElapsedDataMemberType.RetrieveDataFromDatabase,
									() => Helper.GetAllEBayOrders( orders.SubmittedDate, databaseCustomerMarketPlace ) );

						var allTeraPeakData = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds( elapsedTimeInfo,
									databaseCustomerMarketPlace.Id,
									ElapsedDataMemberType.RetrieveDataFromDatabase,
									() => Helper.GetAllTeraPeakDataWithFullRange( orders.SubmittedDate, databaseCustomerMarketPlace ) );

						if ( _Settings.DownloadCategories )
						{
							var topSealedProductItems = GetTopSealedProductItems( allEBayOrders );

							if ( topSealedProductItems != null )
							{
								var orderItemDetails = topSealedProductItems.Select( item => FindEBayOrderItemInfo( databaseCustomerMarketPlace, info, item, databaseOrdersList.RequestsCounter, elapsedTimeInfo ) ).Where( d => d != null).ToList();

								Helper.UpdateOrderItemsInfo(orderItemDetails, elapsedTimeInfo, databaseCustomerMarketPlace.Id);
							}
						}

						var compositeList = CompositeData( allTeraPeakData, allEBayOrders );

						if ( compositeList != null )
						{
							//ParceAndSaveOrdersAggregationInfo( databaseCustomerMarketPlace, allEBayOrders, _Helper.CurrencyConverter, historyRecord );
							var agInfo = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds( elapsedTimeInfo,
									databaseCustomerMarketPlace.Id,
									ElapsedDataMemberType.AggregateData,
									() => CreateAggregationInfo( compositeList, Helper.CurrencyConverter ) );
							// Save aggregated info
							ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds( elapsedTimeInfo,
									databaseCustomerMarketPlace.Id,
									ElapsedDataMemberType.StoreAggregatedData,
									() => Helper.StoreToDatabaseAggregatedData( databaseCustomerMarketPlace, agInfo, historyRecord ) );
						}

						var value = orders == null || orders.Orders == null ? 0 : orders.Orders.Count;

						countAllOrders.Value = value + (int)countAllOrders.Value;

						if ( countAllOrders.ElapsedTime == null )
						{
							countAllOrders.ElapsedTime = elapsedTimeInfo;
						}
						else
						{
							countAllOrders.ElapsedTime.MergeData( elapsedTimeInfo );
						}

						if ( countAllOrders.RequestsCounter == null )
						{
							countAllOrders.RequestsCounter = databaseOrdersList == null ? null : databaseOrdersList.RequestsCounter;
						}
						else
						{
							if ( databaseOrdersList != null )
							{
								countAllOrders.RequestsCounter.Add( databaseOrdersList.RequestsCounter );
							}
						}

						return new UpdateActionResultInfo
							{
								Name = UpdateActionResultType.eBayOrdersCount,
								Value = value,
								RequestsCounter = databaseOrdersList == null ? null : databaseOrdersList.RequestsCounter,
								ElapsedTime = elapsedTimeInfo
							};
					}
	            );	        
	    }
		
		private void UpdateTeraPeakOrders( IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
												string ebayUserID, 
												MP_CustomerMarketplaceUpdatingHistory historyRecord )
		{
			Helper.CustomerMarketplaceUpdateAction( CustomerMarketplaceUpdateActionType.TeraPeakSearchBySeller,
													databaseCustomerMarketPlace,
													historyRecord,
													() =>
					{
						var elapsedTimeInfo = new ElapsedTimeInfo();
						
						// по-умолчанию берем данные за 12 месяцев (на данный момент TeraPeak отдает информация только за 1 год)
						int countMonthsForRetrieveData = MaxPossibleRetriveMonthsFromTeraPeak;

						DateTime now = DateTime.UtcNow;

						var sellerInfo = new TeraPeakSellerInfo( ebayUserID );

						var allTeraPeakData = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
									databaseCustomerMarketPlace.Id,
									ElapsedDataMemberType.RetrieveDataFromDatabase,
										() => Helper.GetAllTeraPeakDataWithFullRange(now, databaseCustomerMarketPlace));

						if (allTeraPeakData.Count > 0)
						{
							DateTime lastDate = allTeraPeakData.Max(i => i.EndDate);

							countMonthsForRetrieveData = lastDate.GetCountMonthsToByEntire(now);
						}
						
						var ranges = new SearchQueryDatesRangeListData();

						if(countMonthsForRetrieveData > 0)
						{
							if ( countMonthsForRetrieveData > MaxPossibleRetriveMonthsFromTeraPeak )
							{
								countMonthsForRetrieveData = MaxPossibleRetriveMonthsFromTeraPeak;
							}

							var startRequestDate = now.Date.AddMonths( 1 - countMonthsForRetrieveData );

							var peakRequestDataInfo = new TeraPeakRequestDataInfo
							{
								StepType = TeraPeakRequestStepEnum.ByMonth,
								CountSteps = countMonthsForRetrieveData,
								StartDate = startRequestDate,
							};

							ranges.AddRange(TerapeakRequestsQueue.CreateQueriesDates(peakRequestDataInfo, now));
						}

						// Запрос на 1 месяц, чтобы посчитать агрегаты за 1 месяц
						// сдвижка на 1 секунду делается из-за специфики выбора диапазона для расчета агрегатов
						//var oneMonthRange = new SearchQueryDatesRange( now.AddMonths( -1 ).AddSeconds( 1 ), now, RangeMarkerType.Temporary);
						ranges.Add(new SearchQueryDatesRange(now.AddMonths(-1).AddSeconds(1), now, RangeMarkerType.Temporary));
						var requestInfoByRange = new TeraPeakRequestInfo(sellerInfo, ranges, _Settings.ErrorRetryingInfo);

						var teraPeakDatabaseSellerDataByRange = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
									databaseCustomerMarketPlace.Id,
									ElapsedDataMemberType.RetrieveDataFromExternalService,
									() => TeraPeakService.SearchBySeller(requestInfoByRange));

						
						ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
								databaseCustomerMarketPlace.Id,
								ElapsedDataMemberType.StoreDataToDatabase,
								() => Helper.StoretoDatabaseTeraPeakOrdersData(databaseCustomerMarketPlace, teraPeakDatabaseSellerDataByRange, historyRecord));
                        
						var agInfo = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds( elapsedTimeInfo,
									databaseCustomerMarketPlace.Id,
									ElapsedDataMemberType.AggregateData,
									() =>
									{
										if ( allTeraPeakData == null )
										{
											allTeraPeakData = new TeraPeakDatabaseSellerData( now );
										}
										
										allTeraPeakData.AddRange( teraPeakDatabaseSellerDataByRange );

										var receivedDataList = new MixedReceivedDataList( now, allTeraPeakData.Select( t => new MixedReceivedDataItem( t ) ) );

										return CreateAggregationInfo( receivedDataList, Helper.CurrencyConverter );
									} );

						// Save aggregated info
						ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds( elapsedTimeInfo,
									databaseCustomerMarketPlace.Id,
									ElapsedDataMemberType.StoreAggregatedData,
									() => Helper.StoreToDatabaseAggregatedData( databaseCustomerMarketPlace, agInfo, historyRecord ) );

						return new UpdateActionResultInfo
						{
							Name = UpdateActionResultType.TeraPeakOrdersCount,
							Value = teraPeakDatabaseSellerDataByRange.Count,
							RequestsCounter = teraPeakDatabaseSellerDataByRange.RequestsCounter,
							ElapsedTime = elapsedTimeInfo
						};
					} );
		}

		private DateTime? UpdateTeraPeakOrdersBeforeEbay(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
											   string ebayUserID,
                                               MP_CustomerMarketplaceUpdatingHistory historyRecord, UpdateActionResultInfo countAllOrders)
	    {
	        DateTime? startDate = null;
	        Helper.CustomerMarketplaceUpdateAction(CustomerMarketplaceUpdateActionType.TeraPeakSearchBySeller,
	                                                databaseCustomerMarketPlace, 
													historyRecord, 
													() =>
	                {
						var elapsedTimeInfo = new ElapsedTimeInfo();
						// для начала нужно получить данные от Terapeak
						var hasData = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds( elapsedTimeInfo,
									databaseCustomerMarketPlace.Id,
									ElapsedDataMemberType.RetrieveDataFromDatabase,
									() => Helper.ExistsTeraPeakOrdersData( databaseCustomerMarketPlace ) );
	                    // для пользователя от Tera Peak получем данные только 1 раз 
	                    if (!hasData)
	                    {
							var sellerInfo = new TeraPeakSellerInfo( ebayUserID );

							var countMonths = 10;
							var now = DateTime.UtcNow;
							startDate = now.Date.AddYears( -1 ).AddDays( -1 );
			
							var peakRequestDataInfo = new TeraPeakRequestDataInfo
							{
								StepType = TeraPeakRequestStepEnum.ByMonth,
								CountSteps = countMonths,
								StartDate = startDate.Value,				
							};

			
							var ranges = TerapeakRequestsQueue.CreateQueriesDates( peakRequestDataInfo, now );

                            var requestInfo = new TeraPeakRequestInfo(sellerInfo, ranges, _Settings.ErrorRetryingInfo);

							var teraPeakDatabaseSellerData = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds( elapsedTimeInfo,
									databaseCustomerMarketPlace.Id,
									ElapsedDataMemberType.RetrieveDataFromExternalService,
									() => TeraPeakService.SearchBySeller( requestInfo ) );

	                        if (teraPeakDatabaseSellerData != null && teraPeakDatabaseSellerData.Any())
	                        {
	                            startDate = teraPeakDatabaseSellerData.Max(o => o.EndDate);
								
	                        }

							ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds( elapsedTimeInfo,
									databaseCustomerMarketPlace.Id,
									ElapsedDataMemberType.StoreDataToDatabase,
									() => Helper.StoretoDatabaseTeraPeakOrdersData( databaseCustomerMarketPlace, teraPeakDatabaseSellerData, historyRecord ) );

	                        var value = teraPeakDatabaseSellerData == null ? 0 : teraPeakDatabaseSellerData.Count;

                            countAllOrders.Value = value + (int)countAllOrders.Value;
							countAllOrders.ElapsedTime = elapsedTimeInfo;
							countAllOrders.RequestsCounter = teraPeakDatabaseSellerData == null ? null : new RequestsCounterData( teraPeakDatabaseSellerData.RequestsCounter );
	                        return new UpdateActionResultInfo
	                            {
	                                Name = UpdateActionResultType.TeraPeakOrdersCount,
	                                Value = value,
									RequestsCounter = teraPeakDatabaseSellerData == null ? null : teraPeakDatabaseSellerData.RequestsCounter,
									ElapsedTime = elapsedTimeInfo
	                            };
	                    }
	                    else
	                    {
							startDate = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds( elapsedTimeInfo,
									databaseCustomerMarketPlace.Id,
									ElapsedDataMemberType.RetrieveDataFromDatabase,
									() => Helper.GetLastEbayOrdersRequest( databaseCustomerMarketPlace ) );
							
	                        return new UpdateActionResultInfo
	                            {
	                                Name = UpdateActionResultType.TeraPeakOrdersCount,
	                                Value = 0,
									RequestsCounter = null,
									ElapsedTime = elapsedTimeInfo
	                            };
	                    }
	                }
	            );
	        return startDate;
	    }

		public static IEnumerable<string> GetTopSealedProductItems( EbayDatabaseOrdersList orders, int countTopItems = 10 )
		{
			return orders.AsParallel().Where( o => o.TransactionData != null )
				.SelectMany( o => o.TransactionData )
				.GroupBy( tr => tr.ItemID, ( key, group ) => new { ItemId = key, Sum = group.Sum( t => t.QuantityPurchased ) } )
				.OrderByDescending( x => x.Sum )
				.Take( countTopItems )
				.Where(a => a.ItemId != null).Select( a => a.ItemId ).ToList();
		}

	    private IEnumerable<IWriteDataInfo<eBayDatabaseFunctionType>> CreateAggregationInfo(MixedReceivedDataList orders, ICurrencyConvertor currencyConverter)
		{
			var aggregateFunctionArray = new[]
					{
						eBayDatabaseFunctionType.AverageItemsPerOrder,
						eBayDatabaseFunctionType.AverageSumOfOrder, 
						eBayDatabaseFunctionType.CancelledOrdersCount,
						eBayDatabaseFunctionType.NumOfOrders,
						eBayDatabaseFunctionType.TotalItemsOrdered,
						eBayDatabaseFunctionType.TotalSumOfOrders,
						eBayDatabaseFunctionType.TotalSumOfOrdersAnnualized,
						eBayDatabaseFunctionType.OrdersCancellationRate,
                        eBayDatabaseFunctionType.TopCategories
					};

			var updated = orders.SubmittedDate;
			var timePeriodData = DataAggregatorHelper.GetOrdersForPeriodsEbay(orders);

			var factory = new MixedOrdersAggregatorFactory();

			return DataAggregatorHelper.AggregateData( factory, timePeriodData, aggregateFunctionArray, updated, currencyConverter );
		}

		private MixedReceivedDataList CompositeData(TeraPeakDatabaseSellerData allTeraPeakData, EbayDatabaseOrdersList allEBayOrders)
		{
			if (allTeraPeakData == null && allEBayOrders == null  )
			{
				return null;
			}

			if ( allTeraPeakData.Count == 0 )
			{
				return new MixedReceivedDataList( allEBayOrders.SubmittedDate, allEBayOrders.Select( ei => new MixedReceivedDataItem( ei ) ) );
			}

			if ( allEBayOrders.Count == 0 )
			{
				return new MixedReceivedDataList( allTeraPeakData.Submitted, allTeraPeakData.Select( t => new MixedReceivedDataItem( t ) ) );
			}

			DateTime submitDate = allTeraPeakData.Submitted > allEBayOrders.SubmittedDate ? allTeraPeakData.Submitted : allEBayOrders.SubmittedDate;

			var rez = new MixedReceivedDataList( submitDate, allTeraPeakData.Select( t => new MixedReceivedDataItem( t ) ) );

			var eOrders = allEBayOrders.OrderBy(eo => eo.RecordTime).Where(eo => !allTeraPeakData.Any(t => t.StartDate <= eo.RecordTime && t.EndDate >= eo.RecordTime)).Select(d => d).OrderBy(d => d.RecordTime);

			rez.AddRange( eOrders.Select( eo => new MixedReceivedDataItem( eo ) ) );

			return rez;

		}

		private EbayDatabaseOrdersList ParseOrdersInfo( ResultInfoOrders data )
		{
			var rez = new EbayDatabaseOrdersList( data.SubmittedDate ) 
			{
				RequestsCounter = data.RequestsCounter
			};

			foreach (var o in data)
			{
				if ( o == null )
				{
					continue;
				}
				var item = new EbayDatabaseOrderItem();
				item.CreatedTime = o.CreatedTimeSpecified ? o.CreatedTime.ToUniversalTime() : (DateTime?)null;
				item.ShippedTime = o.ShippedTimeSpecified ? o.ShippedTime.ToUniversalTime() : (DateTime?)null;
				item.PaymentTime = o.PaidTimeSpecified ? o.PaidTime.ToUniversalTime() : (DateTime?)null;
				item.BuyerName = o.BuyerUserID;
				item.AdjustmentAmount = ConvertToBaseCurrency( o.AdjustmentAmount, o.CreatedTime );
				item.AmountPaid = ConvertToBaseCurrency( o.AmountPaid, o.CreatedTime );
				item.SubTotal = ConvertToBaseCurrency( o.Subtotal, o.CreatedTime );
				item.Total = ConvertToBaseCurrency( o.Total, o.CreatedTime );
				item.OrderStatus = o.OrderStatusSpecified? ConvertOrderStatus( o.OrderStatus ) : EBayOrderStatusCodeType.Default;
				item.PaymentHoldStatus = o.PaymentHoldStatusSpecified? o.PaymentHoldStatus.ToString(): string.Empty;
				item.CheckoutStatus = o.CheckoutStatus != null && o.CheckoutStatus.StatusSpecified? o.CheckoutStatus.Status.ToString(): string.Empty;
				item.PaymentMethod = o.CheckoutStatus != null && o.CheckoutStatus.PaymentMethodSpecified ? o.CheckoutStatus.PaymentMethod.ToString() : string.Empty;
				item.PaymentStatus = o.CheckoutStatus != null && o.CheckoutStatus.eBayPaymentStatusSpecified? o.CheckoutStatus.eBayPaymentStatus.ToString() : string.Empty;
				item.PaymentMethods = o.PaymentMethods == null? null: string.Join( ",", o.PaymentMethods );
				item.ShippingAddressData = o.ShippingAddress == null ? null : o.ShippingAddress.ConvertToDatabaseType();

				if ( o.ExternalTransaction != null && o.ExternalTransaction.Length > 0)
				{
					item.ExternalTransactionData = new EBayDatabaseExternalTransactionList();
					foreach ( var et in o.ExternalTransaction )
					{
						if ( et == null )
						{
							continue;
						}
						var exItem = new EBayDatabaseExternalTransactionItem();

						exItem.TransactionID = et.ExternalTransactionID;
						exItem.TransactionTime = et.ExternalTransactionTimeSpecified? et.ExternalTransactionTime: (DateTime?)null;
						exItem.FeeOrCreditAmount = ConvertToBaseCurrency(et.FeeOrCreditAmount, et.ExternalTransactionTime);
						exItem.PaymentOrRefundAmount = ConvertToBaseCurrency(et.PaymentOrRefundAmount,et.ExternalTransactionTime);

						item.ExternalTransactionData.Add( exItem );
					}
					
				}

				if ( o.TransactionArray != null && o.TransactionArray.Length > 0 )
				{
					item.TransactionData = new EbayDatabaseTransactionDataList();

					foreach ( var td in o.TransactionArray )
					{
						if ( td == null || td.Item == null )
						{
							continue;
						}
						var itemType = td.Item;

						var trItem = new EbayDatabaseTransactionDataItem();

						trItem.CreatedDate = td.CreatedDate;
						trItem.QuantityPurchased = td.QuantityPurchased;
						trItem.PaymentHoldStatus = td.Status != null && td.Status.PaymentHoldStatusSpecified? td.Status.PaymentHoldStatus.ToString(): string.Empty;
						trItem.PaymentMethodUsed = td.Status != null && td.Status.PaymentMethodUsedSpecified ? td.Status.PaymentMethodUsed.ToString() : string.Empty;
						trItem.TransactionPrice = ConvertToBaseCurrency(td.TransactionPrice, td.CreatedDate);
						trItem.ItemSKU  = itemType.SKU;
						trItem.ItemID = itemType.ItemID;
						trItem.ItemPrivateNotes = itemType.PrivateNotes;
						trItem.ItemSellerInventoryID = itemType.SellerInventoryID;
						trItem.eBayTransactionId = td.TransactionID;

						item.TransactionData.Add( trItem );
					}
				}


				rez.Add( item );
			}

			return rez;
		}

		private AmountInfo ConvertToBaseCurrency(AmountType sourceAmaontType, DateTime createdTime)
		{
			if ( sourceAmaontType == null )
			{
				return null;
			}

			return Helper.CurrencyConverter.ConvertToBaseCurrency( sourceAmaontType.currencyID.ToString(), sourceAmaontType.Value, createdTime );
		}

		private MP_EBayOrderItemDetail FindEBayOrderItemInfo( IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, DataProviderCreationInfo info, string itemID, RequestsCounterData requestCounter, ElapsedTimeInfo elapsedTimeInfo )
		{
			if ( !_Settings.DownloadCategories )
			{
				return null;
			}

			IMarketplaceType marketplace = databaseCustomerMarketPlace.Marketplace;

			var eBayItemInfoData = new eBayFindOrderItemInfoData( itemID );

			var eBayOrderItemInfo = Helper.FindEBayOrderItemInfo( eBayItemInfoData, elapsedTimeInfo, databaseCustomerMarketPlace.Id );

			if ( eBayOrderItemInfo == null )
			{
				var providerGetItemInfo = new DataProviderGetItemInfo( info );
				var req = new eBayRequestItemInfoData( eBayItemInfoData );
				ResultInfoEbayItemInfo ebayItemInfo = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds( elapsedTimeInfo,
									databaseCustomerMarketPlace.Id,
									ElapsedDataMemberType.RetrieveDataFromExternalService,
									() => providerGetItemInfo.GetItem( req ) );
				requestCounter.Add( ebayItemInfo.RequestsCounter );

				var newEBayOrderItemInfo = new EbayDatabaseOrderItemInfo
				{
					ItemID = ebayItemInfo.ItemID,
					PrimaryCategory = FindCategory( marketplace, ebayItemInfo.PrimaryCategory, elapsedTimeInfo, databaseCustomerMarketPlace.Id),
					//SecondaryCategory = FindCategory( marketplace, ebayItemInfo.SecondaryCategory ),
					//FreeAddedCategory = FindCategory( marketplace, ebayItemInfo.FreeAddedCategory ),
					Title = ebayItemInfo.Title,

				};

				eBayOrderItemInfo = Helper.SaveEBayOrderItemInfo( newEBayOrderItemInfo, elapsedTimeInfo, databaseCustomerMarketPlace.Id);
			}

			return eBayOrderItemInfo;
		}

		private MP_EbayAmazonCategory FindCategory( IMarketplaceType marketplace, eBayCategoryInfo data, ElapsedTimeInfo elapsedTimeInfo, int mpId )
		{
			if ( data == null )
			{
				return null;
			}

			return Helper.FindEBayAmazonCategory( marketplace, data.CategoryId, elapsedTimeInfo, mpId ) ?? Helper.AddEbayCategory( marketplace, data, elapsedTimeInfo, mpId );
		}

		private EBayOrderStatusCodeType ConvertOrderStatus(OrderStatusCodeType orderStatus)
		{
			switch (orderStatus)
			{
				case OrderStatusCodeType.Shipped:
					return EBayOrderStatusCodeType.Shipped;

				case OrderStatusCodeType.Completed:
					return EBayOrderStatusCodeType.Completed;

				case OrderStatusCodeType.CustomCode:
					return EBayOrderStatusCodeType.CustomCode;

				case OrderStatusCodeType.Active:
					return EBayOrderStatusCodeType.Active;

				case OrderStatusCodeType.All:
					return EBayOrderStatusCodeType.All;

				case OrderStatusCodeType.Authenticated:
					return EBayOrderStatusCodeType.Authenticated;

				case OrderStatusCodeType.Cancelled:
					return EBayOrderStatusCodeType.Cancelled;

				case OrderStatusCodeType.Default:
					return EBayOrderStatusCodeType.Default;

				case OrderStatusCodeType.InProcess:
					return EBayOrderStatusCodeType.InProcess;

				case OrderStatusCodeType.Inactive:
					return EBayOrderStatusCodeType.Inactive;

				case OrderStatusCodeType.Invalid:
					return EBayOrderStatusCodeType.Inactive;

				default:
					throw new NotImplementedException();
			}
		}

		

		public ResultInfoEbayUser GetCustomerUserInfo(eBaySecurityInfo data)
		{
			DataProviderCreationInfo info = CreateProviderCreationInfo( data );
			return GetCustomerUserInfo( info );
		}

		private ResultInfoEbayUser GetCustomerUserInfo( DataProviderCreationInfo info )
		{
			return DataProviderUserInfo.GetDataAboutMySelf( info );
		}
		private DataProviderCreationInfo CreateProviderCreationInfo( eBaySecurityInfo securityInfo )
		{
			var connectionInfo = _EbayConnectionInfo;
			IServiceTokenProvider serviceTokenProvider = new ServiceTokenProviderCustom( securityInfo.Token );
			IEbayServiceProvider serviceProvider = new EbayTradingServiceProvider( connectionInfo );

			return new DataProviderCreationInfo(serviceProvider)
			{
				ServiceTokenProvider = serviceTokenProvider,
				Settings = _Settings
			};

		}

		private DataProviderCreationInfo CreateProviderCreationInfo( IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, EbayServiceConnectionInfo connectionInfo )
		{
			var securityInfo = RetrieveCustomerSecurityInfo<eBaySecurityInfo>( databaseCustomerMarketPlace );

			return CreateProviderCreationInfo( securityInfo );	
		}
	}	
}
