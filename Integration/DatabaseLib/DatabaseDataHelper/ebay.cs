namespace EZBob.DatabaseLib {

	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using DatabaseWrapper;
	using DatabaseWrapper.AccountInfo;
	using DatabaseWrapper.EbayFeedbackData;
	using DatabaseWrapper.Order;
	using DatabaseWrapper.UsersData;
	using EzBob.CommonLib.MarketplaceSpecificTypes.TeraPeakOrdersData;
	using Ezbob.Utils;
	using Model.Database;
	using Model.Database.Repository;
	using EzBob.CommonLib;
	using EzBob.CommonLib.TimePeriodLogic;
	using Model.Marketplaces.Amazon;
	using NHibernate.Linq;
	using Repository;
	using Iesi.Collections.Generic;

	public partial class DatabaseDataHelper {
		private readonly EbayUserAddressDataRepository _EbayUserAddressDataRepository;
		private readonly EBayOrderItemInfoRepository _EBayOrderItemInfoRepository;
		private readonly EbayAmazonCategoryRepository _EbayAmazonCategoryRepository;
		private readonly MP_EbayOrderRepository _MP_EbayOrderRepository;
		private readonly MP_EbayTransactionsRepository _MP_EbayTransactionsRepository;
		private readonly ConcurrentDictionary<string, MP_EBayOrderItemDetail> _CacheEBayOrderItemInfo = new ConcurrentDictionary<string, MP_EBayOrderItemDetail>();
		private readonly ConcurrentDictionary<IMarketplaceType, ConcurrentDictionary<string, MP_EbayAmazonCategory>> _CacheEBayamazonCategory = new ConcurrentDictionary<IMarketplaceType, ConcurrentDictionary<string, MP_EbayAmazonCategory>>();
		private readonly ConcurrentDictionary<string, MP_EbayAmazonCategory[]> _CacheAmazonCategoryByProductKey = new ConcurrentDictionary<string, MP_EbayAmazonCategory[]>();

		public void StoretoDatabaseTeraPeakOrdersData(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, TeraPeakDatabaseSellerData data, MP_CustomerMarketplaceUpdatingHistory historyRecord) {
			if (data == null) {
				WriteToLog("StoreTeraPeakUserData: invalid data to store", WriteLogType.Error);
				return;
			}

			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace.Id);

			var helper = new TeraPeackHelper();
			helper.StoretoDatabaseTeraPeakOrdersData(customerMarketPlace, data, historyRecord);

			_CustomerMarketplaceRepository.Update(customerMarketPlace);
		}

		public bool ExistsTeraPeakOrdersData(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace) {
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace.Id);
			return customerMarketPlace.TeraPeakOrders.Count > 0;
		}

		public TeraPeakDatabaseSellerData GetAllTeraPeakDataWithFullRange(DateTime submittedDate, IDatabaseCustomerMarketPlace databaseCustomerMarketPlace) {
			return _CustomerMarketplaceRepository.GetAllTeraPeakDataWithFullRange(submittedDate, databaseCustomerMarketPlace);
		}

		public void AddEbayOrdersData(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, EbayDatabaseOrdersList data, MP_CustomerMarketplaceUpdatingHistory historyRecord)
		{
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace.Id);

			LogData("Orders Data", customerMarketPlace, data);

			Debug.Assert(data != null);

			if (data == null)
			{
				return;
			}

			var mpOrder = new MP_EbayOrder
			{
				CustomerMarketPlace = customerMarketPlace,
				Created = data.SubmittedDate.ToUniversalTime(),
				HistoryRecord = historyRecord
			};

			if (data.Count != 0)
			{
				data.ForEach(
					databaseOrder =>
					{
						var mpOrderItem = new MP_EbayOrderItem
						{
							Order = mpOrder,
							CreatedTime = databaseOrder.CreatedTime,
							PaymentTime = databaseOrder.PaymentTime,
							ShippedTime = databaseOrder.ShippedTime,
							BuyerName = databaseOrder.BuyerName,
							AdjustmentAmount = databaseOrder.AdjustmentAmount,
							AmountPaid = databaseOrder.AmountPaid,
							SubTotal = databaseOrder.SubTotal,
							Total = databaseOrder.Total,
							CheckoutStatus = databaseOrder.CheckoutStatus,
							OrderStatus = databaseOrder.OrderStatus.ToString(),
							PaymentStatus = databaseOrder.PaymentStatus,
							PaymentHoldStatus = databaseOrder.PaymentHoldStatus,
							PaymentMethod = databaseOrder.PaymentMethod,
							PaymentMethodsList = databaseOrder.PaymentMethods,
							ShippingAddress = CreateAddressInDatabase(databaseOrder.ShippingAddressData),

						};

						if (databaseOrder.TransactionData != null && databaseOrder.TransactionData.HasData)
						{
							mpOrderItem.Transactions.AddAll(databaseOrder.TransactionData.Select(t =>
							{
								var tr = new MP_EbayTransaction
								{
									OrderItem = mpOrderItem,
									CreatedDate = t.CreatedDate,
									PaymentHoldStatus = t.PaymentHoldStatus,
									PaymentMethodUsed = t.PaymentMethodUsed,
									QuantityPurchased = t.QuantityPurchased,
									TransactionPrice = t.TransactionPrice,
									ItemID = t.ItemID,
									ItemPrivateNotes = t.ItemPrivateNotes,
									ItemSKU = t.ItemSKU,
									ItemSellerInventoryID = t.ItemSellerInventoryID,
									eBayTransactionId = t.eBayTransactionId,

								};
								return tr;
							}).ToArray());
						}

						if (databaseOrder.ExternalTransactionData != null && databaseOrder.ExternalTransactionData.HasData)
						{
							mpOrderItem.ExternalTransactions.AddAll(databaseOrder.ExternalTransactionData.Select(t =>
							{
								return new MP_EbayExternalTransaction
								{
									OrderItem = mpOrderItem,
									TransactionID = t.TransactionID,
									TransactionTime = t.TransactionTime,
									FeeOrCreditAmount = t.FeeOrCreditAmount,
									PaymentOrRefundAmount = t.PaymentOrRefundAmount
								};

							}).ToArray());
						}
						mpOrder.OrderItems.Add(mpOrderItem);
					}
					);
			}

			customerMarketPlace.EbayOrders.Add(mpOrder);

			_CustomerMarketplaceRepository.Update(customerMarketPlace);

		}

		public void UpdateOrderItemsInfo(IEnumerable<MP_EBayOrderItemDetail> mpEBayOrderItemDetails, ElapsedTimeInfo elapsedTimeInfo, int mpId)
		{
			if (mpEBayOrderItemDetails == null || !mpEBayOrderItemDetails.Any())
			{
				return;
			}

			foreach (MP_EBayOrderItemDetail mpEBayOrderItemDetail in mpEBayOrderItemDetails)
			{
				if (mpEBayOrderItemDetail == null)
				{
					continue;
				}
				var detail = mpEBayOrderItemDetail;

				var items = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
									mpId,
									ElapsedDataMemberType.RetrieveDataFromDatabase,
									() => _MP_EbayTransactionsRepository.GetAllItemsWithItemsID(detail.ItemID));

				if (items == null || items.Count == 0)
				{
					continue;
				}
				foreach (var mpEbayTransaction in items)
				{
					mpEbayTransaction.OrderItemDetail = mpEBayOrderItemDetail;
					var transaction = mpEbayTransaction;

					ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
									mpId,
									ElapsedDataMemberType.StoreDataToDatabase,
									() => _MP_EbayTransactionsRepository.SaveOrUpdate(transaction));
				}
			}
		}

		private Iesi.Collections.Generic.ISet<MP_AmazonOrderItemDetailCatgory> CreateLinkCollection(MP_AmazonOrderItemDetail orderItemDetail, ICollection<MP_EbayAmazonCategory> categories)
		{
			if (categories == null)
			{
				return null;
			}
			return new HashedSet<MP_AmazonOrderItemDetailCatgory>(
				categories.Select(c =>
					new MP_AmazonOrderItemDetailCatgory
					{
						Category = _EbayAmazonCategoryRepository.Get(c.Id),
						OrderItemDetail = orderItemDetail
					}).ToArray());
		}

		public DateTime? GetLastEbayOrdersRequest(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace)
		{
			return _CustomerMarketplaceRepository.GetLastEbayOrdersRequest(databaseCustomerMarketPlace);
		}

		public void StoreEbayUserData(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, IDatabaseEbayUserData data, MP_CustomerMarketplaceUpdatingHistory historyRecord)
		{
			if (data == null)
			{
				WriteToLog("StoreEbayUserData: invalid data to store", WriteLogType.Error);
				return;
			}

			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace.Id);

			var userData = new MP_EbayUserData
			{

				CustomerMarketPlaceId = customerMarketPlace.Id,
				Created = data.SubmittedDate.ToUniversalTime(),
				BillingEmail = data.BillingEmail,
				EIASToken = data.EIASToken,
				EMail = data.EMail,
				FeedbackPrivate = data.FeedbackPrivate,
				FeedbackRatingStar = data.FeedbackRatingStar,
				FeedbackScore = data.FeedbackScore,
				IDChanged = data.IDChanged,
				IDLastChanged = data.IDLastChanged,
				IDVerified = data.IDVerified,
				NewUser = data.NewUser,
				PayPalAccountStatus = data.PayPalAccountStatus,
				PayPalAccountType = data.PayPalAccountType,
				QualifiesForSelling = data.QualifiesForSelling,
				RegistrationAddress = CreateAddressInDatabase(data.RegistrationAddress),
				RegistrationDate = data.RegistrationDate,
				SellerInfo = new EbaySellerInfo
				{
					SellerInfoQualifiesForB2BVAT = data.QualifiesForB2BVAT,
					SellerInfoSellerBusinessType = data.SellerBusinessType,
					SellerInfoStoreOwner = data.StoreOwner,
					SellerInfoStoreSite = data.StoreSite,
					SellerInfoStoreURL = data.StoreURL,
					SellerInfoTopRatedProgram = data.TopRatedProgram,
					SellerInfoTopRatedSeller = data.TopRatedSeller,
					SellerPaymentAddress = CreateAddressInDatabase(data.SellerPaymentAddress)
				},
				Site = data.Site,
				SkypeID = data.SkypeID,
				UserID = data.UserID,
				eBayGoodStanding = data.eBayGoodStanding,
				HistoryRecord = historyRecord
			};

			customerMarketPlace.EbayUserData.Add(userData);

			_CustomerMarketplaceRepository.Update(customerMarketPlace);
		}

		private MP_EbayUserAddressData CreateAddressInDatabase(DatabaseShipingAddress data)
		{
			if (data == null)
			{
				return null;
			}

			var address = new MP_EbayUserAddressData
			{
				AddressID = data.AddressID,
				FirstName = data.FirstName,
				Phone = data.Phone,
				Street1 = data.Street1,
				LastName = data.LastName,
				CityName = data.CityName,
				CountryCode = data.CountryCode,
				CountryName = data.CountryName,
				PostalCode = data.PostalCode,
				StateOrProvince = data.StateOrProvince,
				Street2 = data.Street2,
				Name = data.Name,
				Street = data.Street,
				AddressOwner = data.AddressOwner,
				AddressRecordType = data.AddressRecordType,
				AddressStatus = data.AddressStatus,
				AddressUsage = data.AddressUsage,
				CompanyName = data.CompanyName,
				County = data.County,
				ExternalAddressID = data.ExternalAddressID,
				InternationalName = data.InternationalName,
				InternationalStateAndCity = data.InternationalStateAndCity,
				InternationalStreet = data.InternationalStreet,
				Phone2 = data.Phone2,
				Phone2AreaOrCityCode = data.Phone2AreaOrCityCode,
				Phone2CountryCode = data.Phone2CountryCode,
				Phone2CountryPrefix = data.Phone2CountryPrefix,
				Phone2LocalNumber = data.Phone2LocalNumber,
				PhoneAreaOrCityCode = data.PhoneAreaOrCityCode,
				PhoneCountryCode = data.PhoneCountryCode,
				PhoneCountryCodePrefix = data.Phone2CountryPrefix,
				PhoneLocalNumber = data.Phone2LocalNumber,
			};

			_EbayUserAddressDataRepository.Save(address);

			return address;
		}

		public void StoreEbayUserAccountData(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, IDatabaseEbayAccountInfo data, MP_CustomerMarketplaceUpdatingHistory historyRecord)
		{
			if (data == null)
			{
				WriteToLog("StoreEbayUserData: invalid data to store", WriteLogType.Error);
				return;
			}

			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace.Id);
			try
			{
				var accountData = new MP_EbayUserAccountData
					{
						CustomerMarketPlace = customerMarketPlace,
						Created = data.SubmittedDate.ToUniversalTime(),
						Currency = data.Currency,
						Id = 0,
						PaymentMethod = data.PaymentMethod,
						AccountId = data.AccountId,
						AccountState = data.AccountState,
						AmountPastDueValue = data.AmountPastDueValue,
						BankAccountInfo = data.BankAccountInfo,
						BankModifyDate = data.BankModifyDate,
						CreditCardExpiration = data.CreditCardExpiration,
						CreditCardInfo = data.CreditCardInfo,
						CreditCardModifyDate = data.CreditCardModifyDate,
						CurrentBalance = data.CurrentBalance,
						PastDue = data.PastDue,
						HistoryRecord = historyRecord
					};

				if (data.AdditionalAccount != null && data.AdditionalAccount.Length > 0)
				{
					data.AdditionalAccount.ForEach(
						a => accountData.EbayUserAdditionalAccountData.Add(
							new MP_EbayUserAdditionalAccountData
								{
									Currency = a.Currency,
									AccountCode = a.AccountCode,
									Balance = a.Balance.Value,
									EbayUserAccountData = accountData
								}));
				}

				customerMarketPlace.EbayUserAccountData.Add(accountData);

				_CustomerMarketplaceRepository.Update(customerMarketPlace);
			}
			catch (Exception ex)
			{
				WriteToLog("StoreEbayUserData: failed to store data", WriteLogType.Error, ex);
			}
		}

		public void StoreEbayFeedbackData(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, DatabaseEbayFeedbackData data, MP_CustomerMarketplaceUpdatingHistory historyRecord)
		{
			if (data == null)
			{
				WriteToLog("StoreEbayUserData: invalid data to store", WriteLogType.Error);
				return;
			}

			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace.Id);

			var feedBack = new MP_EbayFeedback
			{
				CustomerMarketPlace = customerMarketPlace,
				Created = data.Submitted,
				RepeatBuyerCount = data.RepeatBuyerCount,
				RepeatBuyerPercent = data.RepeatBuyerPercent,
				TransactionPercent = data.TransactionPercent,
				UniqueBuyerCount = data.UniqueBuyerCount,
				UniqueNegativeCount = data.UniqueNegativeCount,
				UniqueNeutralCount = data.UniqueNeutralCount,
				UniquePositiveCount = data.UniquePositiveCount,
				HistoryRecord = historyRecord
			};

			if (data.FeedbackByPeriod != null && data.FeedbackByPeriod.Count > 0)
			{
				feedBack.FeedbackByPeriodItems.AddAll(data.FeedbackByPeriod.Values.Select(f => new MP_EbayFeedbackItem
				{
					EbayFeedback = feedBack,
					Negative = f.Negative,
					Positive = f.Positive,
					Neutral = f.Neutral,
					TimePeriod = GetTimePeriod(TimePeriodFactory.Create(f.TimePeriod))
				}).ToList());
			}

			if (data.RaitingByPeriod != null && data.RaitingByPeriod.Count > 0)
			{
				feedBack.RaitingByPeriodItems.AddAll(data.RaitingByPeriod.Values.Select(r => new MP_EbayRaitingItem
				{
					EbayFeedback = feedBack,
					Communication = r.Communication,
					ItemAsDescribed = r.ItemAsDescribed,
					ShippingAndHandlingCharges = r.ShippingAndHandlingCharges,
					ShippingTime = r.ShippingTime,
					TimePeriod = GetTimePeriod(TimePeriodFactory.Create(r.TimePeriod))

				}).ToList());
			}

			customerMarketPlace.EbayFeedback.Add(feedBack);

			_CustomerMarketplaceRepository.Update(customerMarketPlace);
		}

		public EbayDatabaseOrdersList GetAllEBayOrders(DateTime submittedDate, IDatabaseCustomerMarketPlace databaseCustomerMarketPlace)
		{
			var orders = new EbayDatabaseOrdersList(submittedDate);

			var dbOrders = _MP_EbayOrderRepository.GetOrdersItemsByMakretplaceId(databaseCustomerMarketPlace.Id);

			orders.AddRange(dbOrders.Select(o =>
			{
				EBayOrderStatusCodeType orderStatus;
				Enum.TryParse(o.OrderStatus, out orderStatus);
				return new EbayDatabaseOrderItem
				{
					PaymentMethod = o.PaymentMethod,
					AmountPaid = _CurrencyConvertor.ConvertToBaseCurrency(o.AmountPaid, o.CreatedTime),
					OrderStatus = orderStatus,
					PaymentHoldStatus = o.PaymentHoldStatus,
					ShippingAddressData = Convert(o.ShippingAddress),
					BuyerName = o.BuyerName,
					CheckoutStatus = o.CheckoutStatus,
					PaymentMethods = o.PaymentMethod,
					SubTotal = _CurrencyConvertor.ConvertToBaseCurrency(o.SubTotal, o.CreatedTime),
					Total = _CurrencyConvertor.ConvertToBaseCurrency(o.Total, o.CreatedTime),
					AdjustmentAmount = _CurrencyConvertor.ConvertToBaseCurrency(o.AdjustmentAmount, o.CreatedTime),
					PaymentTime = o.PaymentTime,
					TransactionData = new EbayDatabaseTransactionDataList(o.Transactions.Select(t => new EbayDatabaseTransactionDataItem
					{
						CreatedDate = t.CreatedDate,
						PaymentHoldStatus = t.PaymentHoldStatus,
						QuantityPurchased = t.QuantityPurchased,
						PaymentMethodUsed = t.PaymentMethodUsed,
						TransactionPrice = _CurrencyConvertor.ConvertToBaseCurrency(t.TransactionPrice, t.CreatedDate),
						ItemID = t.ItemID,
						ItemPrivateNotes = t.ItemPrivateNotes,
						ItemSKU = t.ItemSKU,
						ItemSellerInventoryID = t.ItemSellerInventoryID,
						eBayTransactionId = t.eBayTransactionId,
						OrderItemDetail = t.OrderItemDetail
					})),
					CreatedTime = o.CreatedTime,
					PaymentStatus = o.PaymentStatus,
					ShippedTime = o.ShippedTime,
					ExternalTransactionData = new EBayDatabaseExternalTransactionList(o.ExternalTransactions.Select(t => new EBayDatabaseExternalTransactionItem
					{
						FeeOrCreditAmount = t.FeeOrCreditAmount,
						PaymentOrRefundAmount = t.PaymentOrRefundAmount,
						TransactionID = t.TransactionID,
						TransactionTime = t.TransactionTime
					}))
				};
			}));

			return orders;
		}

		private static DatabaseShipingAddress Convert(MP_EbayUserAddressData address)
		{
			return new DatabaseShipingAddress
			{
				FirstName = address.FirstName,
				Phone = address.Phone,
				Street1 = address.Street1,
				LastName = address.LastName,
				CityName = address.CityName,
				CountryName = address.CountryName,
				PostalCode = address.PostalCode,
				StateOrProvince = address.StateOrProvince,
				Street2 = address.Street2,
				Name = address.Name,
				Street = address.Street,
				AddressOwner = address.AddressOwner,
				AddressRecordType = address.AddressRecordType,
				AddressStatus = address.AddressStatus,
				AddressUsage = address.AddressUsage,
				CompanyName = address.CompanyName,
				County = address.County,
				ExternalAddressID = address.ExternalAddressID,
				InternationalName = address.InternationalName,
				InternationalStateAndCity = address.InternationalStateAndCity,
				InternationalStreet = address.InternationalStreet,
				Phone2 = address.Phone2,
				Phone2AreaOrCityCode = address.Phone2AreaOrCityCode,
				Phone2CountryCode = address.Phone2CountryCode,
				Phone2CountryPrefix = address.Phone2CountryPrefix,
				Phone2LocalNumber = address.Phone2LocalNumber,
				PhoneAreaOrCityCode = address.PhoneAreaOrCityCode,
				PhoneCountryCode = address.PhoneCountryCode,
				AddressID = address.AddressID,
				CountryCode = address.CountryCode,
				PhoneCountryCodePrefix = address.PhoneCountryCodePrefix,
				PhoneLocalNumber = address.PhoneLocalNumber
			};
		}
		//not in use
		public bool ExistsEBayOrderItemInfo(eBayFindOrderItemInfoData eBayFindOrderItemInfoData, ElapsedTimeInfo elapsedTimeInfo, int mpId)
		{
			MP_EBayOrderItemDetail value;
			if (!_CacheEBayOrderItemInfo.TryGetValue(eBayFindOrderItemInfoData.ItemId, out value))
			{
				value = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
									mpId,
									ElapsedDataMemberType.RetrieveDataFromDatabase,
									() => _EBayOrderItemInfoRepository.FindItem(eBayFindOrderItemInfoData));

				if (value != null)
				{
					_CacheEBayOrderItemInfo.TryAdd(value.ItemID, value);
				}
			}

			return value != null;
		}

		public MP_EBayOrderItemDetail FindEBayOrderItemInfo(eBayFindOrderItemInfoData eBayFindOrderItemInfoData, ElapsedTimeInfo elapsedTimeInfo, int mpId)
		{
			if (eBayFindOrderItemInfoData == null || eBayFindOrderItemInfoData.ItemId == null)
			{
				return null;
			}

			MP_EBayOrderItemDetail value;
			if (!_CacheEBayOrderItemInfo.TryGetValue(eBayFindOrderItemInfoData.ItemId, out value))
			{
				value = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,mpId,
									ElapsedDataMemberType.RetrieveDataFromDatabase,
									() => _EBayOrderItemInfoRepository.FindItem(eBayFindOrderItemInfoData));

				if (value != null)
				{
					_CacheEBayOrderItemInfo.TryAdd(value.ItemID, value);
				}
			}

			return value;

		}

		public MP_EBayOrderItemDetail SaveEBayOrderItemInfo(EbayDatabaseOrderItemInfo data, ElapsedTimeInfo elapsedTimeInfo, int mpId)
		{
			MP_EbayAmazonCategory mpEbayAmazonCategory = null;
			if (data.PrimaryCategory != null)
			{
				mpEbayAmazonCategory = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,mpId,
									ElapsedDataMemberType.RetrieveDataFromDatabase,
									() => _EbayAmazonCategoryRepository.Get(data.PrimaryCategory.Id));
			}

			var item = new MP_EBayOrderItemDetail
			{
				ItemID = data.ItemID,
				PrimaryCategory = mpEbayAmazonCategory,
				//SecondaryCategory = data.SecondaryCategory == null ? null : _EbayAmazonCategoryRepository.Get( data.SecondaryCategory.Id ),
				//FreeAddedCategory = data.FreeAddedCategory == null ? null : _EbayAmazonCategoryRepository.Get( data.FreeAddedCategory.Id ),
				Title = data.Title,
			};

			ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,mpId,
									ElapsedDataMemberType.StoreDataToDatabase,
									() => _EBayOrderItemInfoRepository.Save(item));

			_CacheEBayOrderItemInfo.TryAdd(item.ItemID, item);

			return item;
		}

		public MP_EbayAmazonCategory FindEBayAmazonCategory(IMarketplaceType marketplace, string categoryId, ElapsedTimeInfo elapsedTimeInfo, int mpId)
		{
			MP_EbayAmazonCategory value;
			var cache = GetCache(marketplace);

			if (!cache.TryGetValue(categoryId, out value))
			{
				value = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,mpId,
									ElapsedDataMemberType.RetrieveDataFromDatabase,
									() => _EbayAmazonCategoryRepository.FindItem(categoryId));

				if (value != null)
				{
					AddCategoryToCache(marketplace, value);
				}
			}

			return value;
		}

		public MP_EbayAmazonCategory AddEbayCategory(IMarketplaceType marketplace, eBayCategoryInfo data, ElapsedTimeInfo elapsedTimeInfo, int mpId)
		{
			var item = new MP_EbayAmazonCategory
			{
				CategoryId = data.CategoryId,
				IsVirtual = data.IsVirtual,
				Name = data.Name,
				Marketplace = _MarketPlaceRepository.Get(marketplace.InternalId)
			};

			ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,mpId,
									ElapsedDataMemberType.StoreDataToDatabase,
									() => _EbayAmazonCategoryRepository.Save(item));

			AddCategoryToCache(marketplace, item);

			return item;
		}

		private ConcurrentDictionary<string, MP_EbayAmazonCategory> GetCache(IMarketplaceType marketplace)
		{
			ConcurrentDictionary<string, MP_EbayAmazonCategory> cache;
			if (!_CacheEBayamazonCategory.TryGetValue(marketplace, out cache))
			{
				cache = new ConcurrentDictionary<string, MP_EbayAmazonCategory>();

				if (!_CacheEBayamazonCategory.TryAdd(marketplace, cache))
				{
					_CacheEBayamazonCategory.TryGetValue(marketplace, out cache);
				}
			}

			return cache;
		}

		public IQueryable<MP_EbayFeedback> GetEbayFeedback()
		{
			return _session.Query<MP_EbayFeedback>();
		}

		private void AddCategoryToCache(IMarketplaceType marketplace, MP_EbayAmazonCategory item) {
			var cache = GetCache(marketplace);

			cache.TryAdd(item.CategoryId, item);
		}
	} // class DatabaseDataHelper

	public class eBayFindOrderItemInfoData
	{
		public eBayFindOrderItemInfoData(string itemId)
		{
			ItemId = itemId;
		} // constructor

		public string ItemId { get; private set; }
	} // class eBayFindOrderItemInfoData

	public class eBayCategoryInfo
	{
		public string CategoryId { get; set; }
		public string Name { get; set; }
		public bool? IsVirtual { get; set; }
		//public string[] ParentIdList { get; set; }
	} // class eBayCategoryInfo

} // namespace EZBob.DatabaseLib
