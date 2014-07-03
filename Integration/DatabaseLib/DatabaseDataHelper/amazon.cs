namespace EZBob.DatabaseLib {
	#region using

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DatabaseWrapper;
	using DatabaseWrapper.AmazonFeedbackData;
	using DatabaseWrapper.Order;
	using DatabaseWrapper.Products;
	using Ezbob.Utils;
	using Model.Database;
	using Model.Database.Repository;
	using EzBob.CommonLib;
	using EzBob.CommonLib.TimePeriodLogic;
	using Model.Marketplaces.Amazon;
	using NHibernate.Linq;
	using Repository;

	#endregion using

	public partial class DatabaseDataHelper {
		private readonly AmazonOrderItemDetailRepository _AmazonOrderItemDetailRepository;
		private readonly AmazonMarketPlaceTypeRepository _amazonMarketPlaceTypeRepository;

		public void StoreAmazonOrdersData(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, AmazonOrdersList2 ordersData2, MP_CustomerMarketplaceUpdatingHistory historyRecord)
		{
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace.Id);

			LogData("Orders Data", customerMarketPlace, ordersData2);

			if (ordersData2 == null)
			{
				return;
			}

			DateTime submittedDate = ordersData2.SubmittedDate;

			var mpOrder = new MP_AmazonOrder
			{
				CustomerMarketPlace = customerMarketPlace,
				Created = submittedDate.ToUniversalTime(),
				HistoryRecord = historyRecord
			};

			if (ordersData2.Count > 0)
			{
				ordersData2.ForEach(
					dataItem =>
					{
						var mpOrderItem2 = new MP_AmazonOrderItem2
						{
							Order = mpOrder,
							OrderId = dataItem.AmazonOrderId,
							OrderStatus = dataItem.OrderStatus.ToString(),
							PurchaseDate = dataItem.PurchaseDate,
							LastUpdateDate = dataItem.LastUpdateDate,
							NumberOfItemsShipped = dataItem.NumberOfItemsShipped,
							NumberOfItemsUnshipped = dataItem.NumberOfItemsUnshipped,
							OrderTotal = _CurrencyConvertor.ConvertToBaseCurrency(dataItem.OrderTotal, dataItem.PurchaseDate),
							SellerOrderId = dataItem.SellerOrderId,
						};

						if (dataItem.OrderedItemsList != null)
						{
							mpOrderItem2.OrderItemDetails.AddAll(dataItem.OrderedItemsList.Select(
								i =>
								{
									var mpAmazonOrderItemDetail = new MP_AmazonOrderItemDetail
									{

										OrderItem = mpOrderItem2,
										SellerSKU = i.SellerSKU,
										Title = i.Title,
										ASIN = i.ASIN,
										CODFee = i.CODFee,
										CODFeeDiscount = i.CODFeeDiscount,
										GiftMessageText = i.GiftMessageText,
										GiftWrapLevel = i.GiftWrapLevel,
										GiftWrapPrice = i.GiftWrapPrice,
										GiftWrapTax = i.GiftWrapTax,
										ItemPrice = i.ItemPrice,
										ItemTax = i.ItemTax,
										OrderItemId = i.OrderItemId,
										PromotionDiscount = i.PromotionDiscount,
										QuantityOrdered = i.QuantityOrdered,
										QuantityShipped = i.QuantityShipped,
										ShippingDiscount = i.ShippingDiscount,
										ShippingPrice = i.ShippingPrice,
										ShippingTax = i.ShippingTax
									};

									mpAmazonOrderItemDetail.OrderItemCategories = CreateLinkCollection(mpAmazonOrderItemDetail, i.Categories);

									return mpAmazonOrderItemDetail;

								}).ToArray());
						}

						if (dataItem.PaymentsInfo != null)
						{
							dataItem.PaymentsInfo.ForEach(
								i => mpOrderItem2.PaymentsInfo.Add(new MP_AmazonOrderItem2Payment
								{
									OrderItem = mpOrderItem2,
									MoneyInfo = _CurrencyConvertor.ConvertToBaseCurrency(i.MoneyInfo, dataItem.PurchaseDate),
									SubPaymentMethod = i.SubPaymentMethod
								}));

						}

						mpOrder.OrderItems2.Add(mpOrderItem2);
					});
			}
			customerMarketPlace.AmazonOrders.Add(mpOrder);
			_CustomerMarketplaceRepository.Update(customerMarketPlace);
		}

		public DateTime? GetLastAmazonOrdersRequest(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace)
		{
			return _CustomerMarketplaceRepository.GetLastAmazonOrdersRequest(databaseCustomerMarketPlace);
		}

		public void StoreAmazonFeedbackData(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, DatabaseAmazonFeedbackData data, MP_CustomerMarketplaceUpdatingHistory historyRecord)
		{
			if (data == null)
			{
				WriteToLog("StoreAmazonUserData: invalid data to store", WriteLogType.Error);
				return;
			}

			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace.Id);

			var feedBack = new MP_AmazonFeedback
			{
				CustomerMarketPlace = customerMarketPlace,
				Created = data.Submitted,
				UserRaining = data.UserRaining,
				HistoryRecord = historyRecord
			};

			if (data.FeedbackByPeriod != null && data.FeedbackByPeriod.Count > 0)
			{
				feedBack.FeedbackByPeriodItems.AddAll(data.FeedbackByPeriod.Values.Select(f => new MP_AmazonFeedbackItem
				{
					AmazonFeedback = feedBack,
					Count = f.Count,
					Negative = f.Negative,
					Positive = f.Positive,
					Neutral = f.Neutral,
					TimePeriod = GetTimePeriod(TimePeriodFactory.Create(f.TimePeriod))
				}).ToList());
			}

			customerMarketPlace.AmazonFeedback.Add(feedBack);

			_CustomerMarketplaceRepository.Update(customerMarketPlace);
		}

		public AmazonOrdersList2 GetAllAmazonOrdersData(DateTime submittedDate, IDatabaseCustomerMarketPlace databaseCustomerMarketPlace)
		{
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace.Id);

			var orders = new AmazonOrdersList2(submittedDate);

			orders.AddRange(customerMarketPlace.AmazonOrders.SelectMany(amazonOrder => amazonOrder.OrderItems2).Select(o =>
			{
				AmazonOrdersList2ItemStatusType orderStatus;
				Enum.TryParse(o.OrderStatus, out orderStatus);
				return new AmazonOrderItem2
				{
					AmazonOrderId = o.OrderId,
					OrderStatus = orderStatus,
					PurchaseDate = o.PurchaseDate,
					LastUpdateDate = o.LastUpdateDate,
					NumberOfItemsShipped = o.NumberOfItemsShipped,
					NumberOfItemsUnshipped = o.NumberOfItemsUnshipped,
					OrderTotal = _CurrencyConvertor.ConvertToBaseCurrency(o.OrderTotal, o.PurchaseDate),
					PaymentsInfo =
						new AmazonOrderItem2PaymentsInfoList(o.PaymentsInfo.Select(pi => new AmazonOrderItem2PaymentInfoListItem
						{
							MoneyInfo = _CurrencyConvertor.ConvertToBaseCurrency(pi.MoneyInfo, o.PurchaseDate),
							SubPaymentMethod = pi.SubPaymentMethod
						})),
					SellerOrderId = o.SellerOrderId,
				};
			}));

			return orders;
		}

		public MP_EbayAmazonCategory[] AddAmazonCategories(IMarketplaceType marketplace, AmazonProductItemBase productItem, ElapsedTimeInfo elapsedTimeInfo, int mpId)
		{
			var categories = new List<MP_EbayAmazonCategory>();

			foreach (AmazonProductCategory amazonProductCategory in productItem.Categories)
			{
				var cat = FindEBayAmazonCategory(marketplace, amazonProductCategory.CategoryId, elapsedTimeInfo, mpId) ?? AddAmazonCategory(marketplace, amazonProductCategory, elapsedTimeInfo, mpId);

				categories.Add(cat);

			}
			_CacheAmazonCategoryByProductKey.TryAdd(productItem.Key, categories.ToArray());

			return categories.ToArray();
		}

		private MP_EbayAmazonCategory AddAmazonCategory(IMarketplaceType marketplace, AmazonProductCategory amazonProductCategory, ElapsedTimeInfo elapsedTimeInfo,int mpId)
		{
			var cat = new MP_EbayAmazonCategory
			{
				CategoryId = amazonProductCategory.CategoryId,
				Name = amazonProductCategory.CategoryName,
				Marketplace = _MarketPlaceRepository.Get(marketplace.InternalId),
				Parent = amazonProductCategory.Parent == null ? null : FindEBayAmazonCategory(marketplace, amazonProductCategory.Parent.CategoryId, elapsedTimeInfo, mpId) ?? AddAmazonCategory(marketplace, amazonProductCategory.Parent, elapsedTimeInfo, mpId)
			};

			AddCategoryToCache(marketplace, cat);

			ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
									mpId,
									ElapsedDataMemberType.StoreDataToDatabase,
									() => _EbayAmazonCategoryRepository.Save(cat));

			return cat;
		}

		public MP_EbayAmazonCategory[] FindAmazonCategoryByProductAsin(string asin, ElapsedTimeInfo elapsedTimeInfo, int mpId)
		{
			MP_EbayAmazonCategory[] cat;

			if (!_CacheAmazonCategoryByProductKey.TryGetValue(asin, out cat))
			{
				cat = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
									mpId,
									ElapsedDataMemberType.RetrieveDataFromDatabase,
									() => _AmazonOrderItemDetailRepository.FindCategoriesByAsin(asin));
				if (cat != null)
				{
					_CacheAmazonCategoryByProductKey.TryAdd(asin, cat);
				}
			}

			return cat;
		}

		public MP_EbayAmazonCategory[] FindAmazonCategoryByProductSellerSKU(string sellerSKU, ElapsedTimeInfo elapsedTimeInfo, int mpId)
		{
			MP_EbayAmazonCategory[] cat;

			if (!_CacheAmazonCategoryByProductKey.TryGetValue(sellerSKU, out cat))
			{
				cat = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
									mpId,
									ElapsedDataMemberType.RetrieveDataFromDatabase,
									() => _AmazonOrderItemDetailRepository.FindCategoriesBySellectSku(sellerSKU));
				if (cat != null)
				{
					_CacheAmazonCategoryByProductKey.TryAdd(sellerSKU, cat);
				}
			}

			return cat;
		}

		public IQueryable<MP_AmazonFeedback> GetAmazonFeedback()
		{
			return _session.Query<MP_AmazonFeedback>();
		}
	} // class DatabaseDataHelper
} // namespace EZBob.DatabaseLib
