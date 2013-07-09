using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper;
using EZBob.DatabaseLib.DatabaseWrapper.AccountInfo;
using EZBob.DatabaseLib.DatabaseWrapper.AmazonFeedbackData;
using EZBob.DatabaseLib.DatabaseWrapper.EbayFeedbackData;
using EZBob.DatabaseLib.DatabaseWrapper.FunctionValues;
using EZBob.DatabaseLib.DatabaseWrapper.Functions;
using EZBob.DatabaseLib.DatabaseWrapper.Inventory;
using EZBob.DatabaseLib.DatabaseWrapper.Order;
using EZBob.DatabaseLib.DatabaseWrapper.Products;
using EZBob.DatabaseLib.DatabaseWrapper.Transactions;
using EZBob.DatabaseLib.DatabaseWrapper.UsersData;
using EZBob.DatabaseLib.Exceptions;
using EZBob.DatabaseLib.Model;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using EZBob.DatabaseLib.Model.Loans;
using EzBob.CommonLib;
using EzBob.CommonLib.MarketplaceSpecificTypes.TeraPeakOrdersData;
using EzBob.CommonLib.ReceivedDataListLogic;
using EzBob.CommonLib.TimePeriodLogic;
using NHibernate;
using NHibernate.Linq;
using Scorto.NHibernate.Repository;
using StructureMap;
using log4net;
using Iesi.Collections.Generic;

namespace EZBob.DatabaseLib
{
	using EzBob.CommonLib.Security;
	using Model.Marketplaces.FreeAgent;
	using Model.Marketplaces.Yodlee;

    public enum CustomerMarketplaceUpdateActionType
	{
		UpdateInventoryInfo,
		UpdateOrdersInfo,
		UpdateFeedbackInfo,
		UpdateUserInfo,
		UpdateAccountInfo,
		TeraPeakSearchBySeller,
		EbayGetOrders,
		UpdateTransactionInfo
	}

	public class DatabaseDataHelper : IDatabaseDataHelper
	{
		private static readonly ILog _Log = LogManager.GetLogger(typeof(DatabaseDataHelper));

		private readonly ValueTypeRepository _ValueTypeRepository;
		private readonly CustomerRepository _CustomerRepository;
		private readonly MarketPlaceRepository _MarketPlaceRepository;
		private readonly CustomerMarketPlaceRepository _CustomerMarketplaceRepository;
		private readonly AnalysisFunctionTimePeriodRepository _AnalysisFunctionTimePeriodRepository;
		private readonly AnalyisisFunctionRepository _AnalyisisFunctionRepository;
		private readonly AnalyisisFunctionValueRepository _AnalyisisFunctionValueRepository;
		private readonly DatabaseFunctionValuesWriterHelper _FunctionValuesWriterHelper;
		private readonly EbayUserAddressDataRepository _EbayUserAddressDataRepository;
		private readonly ICurrencyRateRepository _CurrencyRateRepository;
		private readonly ICurrencyConvertor _CurrencyConvertor;
		private readonly EBayOrderItemInfoRepository _EBayOrderItemInfoRepository;
		private readonly EbayAmazonCategoryRepository _EbayAmazonCategoryRepository;
		private readonly AmazonOrderItemDetailRepository _AmazonOrderItemDetailRepository;
		private readonly MP_EbayOrderRepository _MP_EbayOrderRepository;
		private readonly MP_EbayTransactionsRepository _MP_EbayTransactionsRepository;
		private readonly ConcurrentDictionary<string, MP_EBayOrderItemDetail> _CacheEBayOrderItemInfo = new ConcurrentDictionary<string, MP_EBayOrderItemDetail>();
		private readonly ConcurrentDictionary<IMarketplaceType, ConcurrentDictionary<string, MP_EbayAmazonCategory>> _CacheEBayamazonCategory = new ConcurrentDictionary<IMarketplaceType, ConcurrentDictionary<string, MP_EbayAmazonCategory>>();
		private readonly ConcurrentDictionary<string, MP_EbayAmazonCategory[]> _CacheAmazonCategoryByProductKey = new ConcurrentDictionary<string, MP_EbayAmazonCategory[]>();
		private readonly ILoanTypeRepository _LoanTypeRepository;
		private readonly CustomerLoyaltyProgramPointsRepository _CustomerLoyaltyPoints;
		private readonly MP_FreeAgentCompanyRepository _FreeAgentCompanyRepository;
		private readonly MP_FreeAgentUsersRepository _FreeAgentUsersRepository;
		private readonly MP_FreeAgentExpenseCategoryRepository _FreeAgentExpenseCategoryRepository;
		private readonly IConfigurationVariablesRepository _ConfigurationVariables;
	    private readonly IMP_YodleeTransactionCategoriesRepository _MP_YodleeTransactionCategoriesRepository;
		private ISession _session;

		public DatabaseDataHelper(ISession session)
		{
			_session = session;

			_MarketPlaceRepository = new MarketPlaceRepository(session);
			_CustomerMarketplaceRepository = new CustomerMarketPlaceRepository(session);
			_AnalysisFunctionTimePeriodRepository = new AnalysisFunctionTimePeriodRepository(session);
			_AnalyisisFunctionRepository = new AnalyisisFunctionRepository(session);
			_AnalyisisFunctionValueRepository = new AnalyisisFunctionValueRepository(session);
			_CustomerRepository = new CustomerRepository(session);
			_ValueTypeRepository = new ValueTypeRepository(session);
			_EbayUserAddressDataRepository = new EbayUserAddressDataRepository(session);
			_FunctionValuesWriterHelper = new DatabaseFunctionValuesWriterHelper(this, _AnalyisisFunctionValueRepository, _CustomerMarketplaceRepository, _AnalysisFunctionTimePeriodRepository, _AnalyisisFunctionRepository);
			_CurrencyRateRepository = ObjectFactory.GetInstance<CurrencyRateRepository>();
			_CurrencyConvertor = new CurrencyConvertor(_CurrencyRateRepository);
			_EBayOrderItemInfoRepository = new EBayOrderItemInfoRepository(session);
			_EbayAmazonCategoryRepository = new EbayAmazonCategoryRepository(session);
			_AmazonOrderItemDetailRepository = new AmazonOrderItemDetailRepository(session);
			_MP_EbayOrderRepository = new MP_EbayOrderRepository(session);
			_MP_EbayTransactionsRepository = new MP_EbayTransactionsRepository(session);
			_LoanTypeRepository = new LoanTypeRepository(session);
			_CustomerLoyaltyPoints = new CustomerLoyaltyProgramPointsRepository(session);
			_FreeAgentCompanyRepository = new MP_FreeAgentCompanyRepository(session);
			_FreeAgentUsersRepository = new MP_FreeAgentUsersRepository(session);
			_FreeAgentExpenseCategoryRepository = new MP_FreeAgentExpenseCategoryRepository(session);
			_ConfigurationVariables = new ConfigurationVariablesRepository(session);
            _MP_YodleeTransactionCategoriesRepository = new MP_YodleeTransactionCategoriesRepository(session);
		}

		public IConfigurationVariablesRepository ConfigurationVariables { get { return _ConfigurationVariables; } }

		public ILoanTypeRepository LoanTypeRepository { get { return _LoanTypeRepository; } }

		public CustomerLoyaltyProgramPointsRepository CustomerLoyaltyPoints { get { _session.Evict(_CustomerLoyaltyPoints); return _CustomerLoyaltyPoints; } }

		public ICurrencyConvertor CurrencyConverter
		{
			get { return _CurrencyConvertor; }
		}

		public Customer GetCustomerInfo(int clientId)
		{
			return FindCustomer(clientId);
		}

		private Customer FindCustomer(int id)
		{
			var client = _CustomerRepository.Get(id);
			if (client == null)
			{
				throw new InvalidCustomerException(id);
			}
			return client;
		}

		public Customer FindCustomerByEmail(string sEmail)
		{
			return _CustomerRepository.TryGetByEmail(sEmail);
		} // FindCustomerByEmail

		public void UpdateCustomerMarketPlace(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace)
		{
			var oldData = GetCustomerMarketPlace(databaseCustomerMarketPlace);

			var customer = databaseCustomerMarketPlace.Customer;
			IMarketplaceType databaseMarketplaceType = databaseCustomerMarketPlace.Marketplace;

			MP_MarketplaceType marketplaceType = GetMarketPlace(databaseMarketplaceType);
			oldData.Customer = customer;
			oldData.Marketplace = marketplaceType;
			oldData.SecurityData = databaseCustomerMarketPlace.SecurityData;
			oldData.DisplayName = databaseCustomerMarketPlace.DisplayName;

			_CustomerMarketplaceRepository.Update(oldData);
		}

		public IEnumerable<IDatabaseCustomerMarketPlace> GetCustomerMarketPlaceList(Customer customer, IMarketplaceType databaseMarketplace)
		{
			MP_MarketplaceType marketplaceType = GetMarketPlace(databaseMarketplace);

			var data = _CustomerMarketplaceRepository.Get(customer, marketplaceType);

			return data.Select(cm => CreateDatabaseCustomerMarketPlace(customer, databaseMarketplace, cm, cm.Id)).ToList();
		}

		public MP_CustomerMarketPlace GetCustomerMarketPlace(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace)
		{
			return GetCustomerMarketPlace(databaseCustomerMarketPlace.Id);
		}

		public IDatabaseCustomerMarketPlace GetDatabaseCustomerMarketPlace(IMarketplaceType marketplaceType, int customerMarketPlaceId)
		{
			MP_CustomerMarketPlace mp = GetCustomerMarketPlace(customerMarketPlaceId);
			var customer = mp.Customer;
			return CreateDatabaseCustomerMarketPlace(mp.DisplayName, marketplaceType, customer);
		}

		public MP_CustomerMarketPlace GetCustomerMarketPlace(int customerMarketPlaceId)
		{
			return _CustomerMarketplaceRepository.Get(customerMarketPlaceId);
		}

		private MP_AnalysisFunctionTimePeriod GetTimePeriod(ITimePeriod databaseTimePeriod)
		{
			return _AnalysisFunctionTimePeriodRepository.Get(databaseTimePeriod.InternalId);
		}

		internal MP_AnalyisisFunction GetFunction(IDatabaseFunction databaseFunction)
		{
			return _AnalyisisFunctionRepository.Get(databaseFunction.InternalId);
		}

		public void InitFunctionTimePeriod()
		{
			foreach (var timePeriod in TimePeriodBase.AllTimePeriods)
			{
				var period = _AnalysisFunctionTimePeriodRepository.Get(timePeriod.InternalId) ?? new MP_AnalysisFunctionTimePeriod { InternalId = timePeriod.InternalId };

				period.Name = timePeriod.Name;
				period.Description = timePeriod.Description;

				_AnalysisFunctionTimePeriodRepository.SaveOrUpdate(period);
			}
		}

		public void StoreToDatabaseInventoryData(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, DatabaseInventoryList data, MP_CustomerMarketplaceUpdatingHistory updatingHistoryRecord)
		{
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace);

			LogData("Inventory Data", customerMarketPlace, data);

			if (data == null)
			{
				return;
			}

			var mpInventory = new MP_EbayAmazonInventory
				{
					CustomerMarketPlace = customerMarketPlace,
					Created = data.SubmittedDate.ToUniversalTime(),
					AmazonUseAFN = data.UseAFN,
					HistoryRecord = updatingHistoryRecord
				};

			if (data.Count != 0)
			{

				data.ForEach(
					databaseInventoryItem =>
					{
						var mpInventoryItem = new MP_EbayAmazonInventoryItem
												{
													Inventory = mpInventory,
													BidCount = databaseInventoryItem.BidCount,
													ItemId = databaseInventoryItem.ItemId,
													Amount = _CurrencyConvertor.ConvertToBaseCurrency(databaseInventoryItem.Amount, data.SubmittedDate),
													Quantity = databaseInventoryItem.Quantity,
													Sku = databaseInventoryItem.Sku,

												};

						mpInventory.InventoryItems.Add(mpInventoryItem);
					}
					);
			}
			customerMarketPlace.Inventory.Add(mpInventory);
			_CustomerMarketplaceRepository.Update(customerMarketPlace);
		}

		private void LogData(string funcName, MP_CustomerMarketPlace customerMarketPlace, IReceivedDataList data)
		{
			if (data == null)
			{
				WriteToLog(string.Format("{0} - {1}. Customer {2}, Market Place {3}: Invalid Call", funcName, customerMarketPlace.Marketplace.Name, customerMarketPlace.Customer.Name, customerMarketPlace.DisplayName), WriteLogType.Error);
				return;
			}

			if (data.Count == 0)
			{
				WriteToLog(string.Format("{0} - {1} - Request date: {2}. Customer {3}, Market Place {4}: NO Data", funcName, customerMarketPlace.Marketplace.Name, data.SubmittedDate, customerMarketPlace.Customer.Name, customerMarketPlace.DisplayName));
			}
			else
			{
				WriteToLog(string.Format("{0} - {1} - Request date: {2}.Customer {3}, Market Place {4}: Received {5} records", funcName, customerMarketPlace.Marketplace.Name, data.SubmittedDate, customerMarketPlace.Customer.Name, customerMarketPlace.DisplayName, data.Count));
			}
		}

		private void UpdateCustomerMarketPlaceDataStart(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, out MP_CustomerMarketplaceUpdatingHistory historyItem)
		{
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace);
			WriteToLog(string.Format("Start Update Data for Customer Market Place: id: {0}, name: {1} ", customerMarketPlace.Id, customerMarketPlace.DisplayName));
			customerMarketPlace.UpdatingStart = DateTime.UtcNow;
			customerMarketPlace.UpdatingEnd = null;

			historyItem = new MP_CustomerMarketplaceUpdatingHistory
														{
															CustomerMarketPlace = customerMarketPlace,
															UpdatingStart = customerMarketPlace.UpdatingStart,
															UpdatingEnd = null,
														};

			customerMarketPlace.UpdatingHistory.Add(historyItem);

			_CustomerMarketplaceRepository.Update(customerMarketPlace);
		}

		private void UpdateCustomerMarketPlaceDataEnd(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, MP_CustomerMarketplaceUpdatingHistory updatingHistoryRecord, Exception exception)
		{
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace);

			customerMarketPlace.UpdatingEnd = DateTime.UtcNow;

			var statusText = "Successfully";

			if (exception != null)
			{
				updatingHistoryRecord.Error = exception.Message;
				statusText = "With error!";
			}

			WriteToLog(string.Format("End update data for umi: id: {0}, name: {1}. {2}", customerMarketPlace.Id, customerMarketPlace.DisplayName, statusText));
			updatingHistoryRecord.UpdatingEnd = customerMarketPlace.UpdatingEnd;
			_CustomerMarketplaceRepository.Update(customerMarketPlace);
		}

		internal void UpdateCustomerMarketplaceData(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, Action<MP_CustomerMarketplaceUpdatingHistory> a)
		{
			Exception ex = null;
			MP_CustomerMarketplaceUpdatingHistory historyItem = null;
			try
			{
				UpdateCustomerMarketPlaceDataStart(databaseCustomerMarketPlace, out historyItem);

				a(historyItem);
			}
			catch (Exception e)
			{
				ex = e;
				throw new MarketplaceException(databaseCustomerMarketPlace, ex);
			}
			finally
			{
				UpdateCustomerMarketPlaceDataEnd(databaseCustomerMarketPlace, historyItem, ex);
			}
		}

		public void AddEbayOrdersData(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, EbayDatabaseOrdersList data, MP_CustomerMarketplaceUpdatingHistory historyRecord)
		{
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace);

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

		public void UpdateOrderItemsInfo(IEnumerable<MP_EBayOrderItemDetail> mpEBayOrderItemDetails, ElapsedTimeInfo elapsedTimeInfo)
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
									ElapsedDataMemberType.StoreDataToDatabase,
									() => _MP_EbayTransactionsRepository.SaveOrUpdate(transaction));
				}
			}
		}


		public IDatabaseCustomerMarketPlace SaveOrUpdateCustomerMarketplace(string displayname,
																			IMarketplaceType marketplaceType,
																			IMarketPlaceSecurityInfo securityData,
																			Customer customer)
		{
			var serializedSecurityData = SerializeDataHelper.Serialize(securityData);
			return SaveOrUpdateCustomerMarketplace(displayname, marketplaceType, serializedSecurityData, customer);
		}


		public IDatabaseCustomerMarketPlace SaveOrUpdateCustomerMarketplace(string displayname,
																			IMarketplaceType marketplaceType,
																			string password,
																			Customer customer)
		{
			var serializedSecurityData = Encryptor.EncryptBytes(password);
			return SaveOrUpdateCustomerMarketplace(displayname, marketplaceType, serializedSecurityData, customer);
		}

		public IDatabaseCustomerMarketPlace SaveOrUpdateCustomerMarketplace(string displayname, IMarketplaceType marketplaceType, byte[] serializedSecurityData, Customer customer)
		{
			int customerMarketPlaceId;
			var now = DateTime.UtcNow;

			var customerMarketPlace = GetExistsCustomerMarketPlace(displayname, marketplaceType, customer);

			if (customerMarketPlace != null)
			{
				customerMarketPlaceId = customerMarketPlace.Id;
				customerMarketPlace.SecurityData = serializedSecurityData;
				_CustomerMarketplaceRepository.Update(customerMarketPlace);
			}
			else
			{
				customerMarketPlace = new MP_CustomerMarketPlace
					{
						SecurityData = serializedSecurityData,
						Customer = customer,
						Marketplace = _MarketPlaceRepository.Get(marketplaceType.InternalId),
						DisplayName = displayname,
						Created = now,
					};


				customerMarketPlaceId = (int)_CustomerMarketplaceRepository.Save(customerMarketPlace);
			}

			customerMarketPlace.Updated = now;

			return CreateDatabaseCustomerMarketPlace(customer, marketplaceType, customerMarketPlace, customerMarketPlaceId);
		}

		public MP_CustomerMarketPlace GetExistsCustomerMarketPlace(string marketPlaceName, IMarketplaceType marketplaceType, Customer customer)
		{
			return _CustomerMarketplaceRepository.Get(customer.Id, marketplaceType.InternalId, marketPlaceName);
		}

		public MP_MarketplaceType GetMarketPlace(int marketPlaceId)
		{
			return _MarketPlaceRepository.Get(marketPlaceId);
		}

		private MP_MarketplaceType GetMarketPlace(IMarketplaceType databaseMarketplace)
		{
			return _MarketPlaceRepository.Get(databaseMarketplace.InternalId);
		}


		public IDatabaseCustomerMarketPlace CreateDatabaseCustomerMarketPlace(string marketPlaceName, IMarketplaceType databaseMarketplace, Customer databaseCustomer)
		{
			MP_CustomerMarketPlace mpCustomerMarketPlace = GetExistsCustomerMarketPlace(marketPlaceName, databaseMarketplace, databaseCustomer);
			return CreateDatabaseCustomerMarketPlace(databaseCustomer, databaseMarketplace, mpCustomerMarketPlace, mpCustomerMarketPlace.Id);
		}

		public IDatabaseCustomerMarketPlace CreateDatabaseCustomerMarketPlace(Customer databaseCustomer, IMarketplaceType databaseMarketplace, MP_CustomerMarketPlace cm, int customerMarketPlaceId)
		{
			return new DatabaseCustomerMarketPlace(customerMarketPlaceId, cm.DisplayName, cm.SecurityData, databaseCustomer, databaseMarketplace);
		}

		public void StoreAmazonOrdersData(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,/* AmazonOrdersList ordersData,*/ AmazonOrdersList2 ordersData2, MP_CustomerMarketplaceUpdatingHistory historyRecord)
		{
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace);

			//LogData( "Old Orders Data", customerMarketPlace, ordersData );
			LogData("Orders Data", customerMarketPlace, ordersData2);

			if ( /*ordersData == null &&*/ ordersData2 == null)
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


			#region not used
			/*if ( ordersData != null && ordersData.Count != 0 )
			{
				ordersData.ForEach(
					dataItem =>
						{
							var mpOrderItem = new MP_AmazonOrderItem
							    {
							        Order = mpOrder,
							        BayerEmail = dataItem.BayerEmail,
							        BayerPhone = dataItem.BayerPhone,
							        Currency = dataItem.Currency,
							        BayerName = dataItem.BayerName,
							        DeliveryEndDate = dataItem.DeliveryEndDate,
							        DeliveryInstructions = dataItem.DeliveryInstructions,
							        DeliveryStartDate = dataItem.DeliveryStartDate,
							        DeliveryTimeZone = dataItem.DeliveryTimeZone,
							        ItemPrice = dataItem.ItemPrice,
							        ItemTax = dataItem.ItemTax,
							        OrderId = dataItem.OrderId,
							        OrderItemId = dataItem.OrderItemId,
							        PaymentsDate = dataItem.PaymentsDate,
							        ProductName = dataItem.ProductName,
							        PurchaseDate = dataItem.PurchaseDate,
							        QuantityPurchased = dataItem.QuantityPurchased,
							        RecipientName = dataItem.RecipientName,
							        SalesChennel = dataItem.SalesChennel,
							        ShipCityName = dataItem.ShipCityName,
							        ShipCountryName = dataItem.ShipCountryName,
							        ShipPhone = dataItem.ShipPhone,
							        ShipPostalCode = dataItem.ShipPostalCode,
							        ShipRecipient = dataItem.ShipRecipient,
							        ShipServiceLevel = dataItem.ShipServiceLevel,
							        ShipStateOrProvince = dataItem.ShipStateOrProvince,
							        ShipStreet = dataItem.ShipStreet,
							        ShipStreet1 = dataItem.ShipStreet1,
							        ShipStreet2 = dataItem.ShipStreet2,
							        ShipingPrice = dataItem.ShipingPrice,
							        ShipingTax = dataItem.ShipingTax,
							        Sku = dataItem.Sku
							    };

							mpOrder.OrderItems.Add(mpOrderItem);
						}
					);

				
			}*/
			#endregion

			if (ordersData2.Count > 0)
			{
				ordersData2.ForEach(
					dataItem =>
					{
						var mpOrderItem2 = new MP_AmazonOrderItem2
						{
							Order = mpOrder,
							PaymentMethod = dataItem.PaymentMethod,
							OrderId = dataItem.AmazonOrderId,
							OrderStatus = dataItem.OrderStatus.ToString(),
							BuyerEmail = dataItem.BuyerEmail,
							BuyerName = dataItem.BuyerName,
							PurchaseDate = dataItem.PurchaseDate,
							ShipServiceLevel = dataItem.ShipServiceLevel,
							FulfillmentChannel = dataItem.FulfillmentChannel,
							LastUpdateDate = dataItem.LastUpdateDate,
							MarketplaceId = dataItem.MarketplaceId,
							NumberOfItemsShipped = dataItem.NumberOfItemsShipped,
							NumberOfItemsUnshipped = dataItem.NumberOfItemsUnshipped,
							OrderChannel = dataItem.OrderChannel,
							OrderTotal = _CurrencyConvertor.ConvertToBaseCurrency(dataItem.OrderTotal, dataItem.PurchaseDate),
							SalesChannel = dataItem.SalesChannel,
							SellerOrderId = dataItem.SellerOrderId,
							ShipmentAddress = dataItem.ShipmentAddress,
							ShipmentServiceLevelCategory = dataItem.ShipmentServiceLevelCategory
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

		public void StorePayPointOrdersData(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, PayPointOrdersList ordersData, MP_CustomerMarketplaceUpdatingHistory historyRecord)
		{
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace);

			LogData("PayPoint Orders Data", customerMarketPlace, ordersData);

			if (ordersData == null)
			{
				return;
			}

			DateTime submittedDate = DateTime.UtcNow;
			var mpOrder = new MP_PayPointOrder
			{
				CustomerMarketPlace = customerMarketPlace,
				Created = submittedDate,
				HistoryRecord = historyRecord
			};

			ordersData.ForEach(
				dataItem =>
				{
					var mpOrderItem = new MP_PayPointOrderItem
					{
						Order = mpOrder,
						acquirer = dataItem.acquirer,
						amount = dataItem.amount,
						auth_code = dataItem.auth_code,
						authorised = dataItem.authorised,
						card_type = dataItem.card_type,
						cid = dataItem.cid,
						classType = dataItem.classType,
						company_no = dataItem.company_no,
						country = dataItem.country,
						currency = dataItem.currency,
						cv2avs = dataItem.cv2avs,
						date = dataItem.date,
						deferred = dataItem.deferred,
						emvValue = dataItem.emvValue,
						ExpiryDate = dataItem.ExpiryDate,
						fraud_code = dataItem.fraud_code,
						FraudScore = dataItem.FraudScore,
						ip = dataItem.ip,
						lastfive = dataItem.lastfive,
						merchant_no = dataItem.merchant_no,
						message = dataItem.message,
						MessageType = dataItem.MessageType,
						mid = dataItem.mid,
						name = dataItem.name,
						options = dataItem.options,
						start_date = dataItem.start_date,
						status = dataItem.status,
						tid = dataItem.tid,
						trans_id = dataItem.trans_id
					};
					mpOrder.OrderItems.Add(mpOrderItem);
				});

			customerMarketPlace.PayPointOrders.Add(mpOrder);
			_CustomerMarketplaceRepository.Update(customerMarketPlace);
		}

		public void StoreEkmOrdersData(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, EkmOrdersList ordersData, MP_CustomerMarketplaceUpdatingHistory historyRecord)
		{
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace);

			LogData("Orders Data", customerMarketPlace, ordersData);

			if (ordersData == null)
			{
				return;
			}

			DateTime submittedDate = DateTime.UtcNow;
			var mpOrder = new MP_EkmOrder
			{
				CustomerMarketPlace = customerMarketPlace,
				Created = submittedDate,
				HistoryRecord = historyRecord
			};

			ordersData.ForEach(
				dataItem =>
				{
					var mpOrderItem = new MP_EkmOrderItem
					{
						Order = mpOrder,
						CompanyName = dataItem.CompanyName,
						CustomerId = dataItem.CustomerID,
						EmailAddress = dataItem.EmailAddress,
						FirstName = dataItem.EmailAddress,
						LastName = dataItem.LastName,
						OrderDate = dataItem.OrderDate,
						OrderDateIso = dataItem.OrderDateIso,
						OrderNumber = dataItem.OrderNumber,
						OrderStatus = dataItem.OrderStatus,
						OrderStatusColour = dataItem.OrderStatusColour,
						TotalCost = dataItem.TotalCost,
					};
					mpOrder.OrderItems.Add(mpOrderItem);
				});

			customerMarketPlace.EkmOrders.Add(mpOrder);
			_CustomerMarketplaceRepository.Update(customerMarketPlace);
		}

		public MP_FreeAgentRequest StoreFreeAgentRequestAndInvoicesAndExpensesData(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, FreeAgentInvoicesList invoices, FreeAgentExpensesList expenses, MP_CustomerMarketplaceUpdatingHistory historyRecord)
		{
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace);

			LogData("Invoices Data", customerMarketPlace, invoices);
			LogData("Expenses Data", customerMarketPlace, expenses);

			DateTime submittedDate = DateTime.UtcNow;
			var mpRequest = new MP_FreeAgentRequest
			{
				CustomerMarketPlace = customerMarketPlace,
				Created = submittedDate,
				HistoryRecord = historyRecord
			};

			invoices.ForEach(
				dataItem =>
				{
					var invoice = new MP_FreeAgentInvoice
					{
						Request = mpRequest,
						url = dataItem.url,
						contact = dataItem.contact,
						dated_on = dataItem.dated_on,
						due_on = dataItem.due_on,
						reference = dataItem.reference,
						currency = dataItem.currency,
						exchange_rate = dataItem.exchange_rate,
						net_value = dataItem.net_value,
						total_value = dataItem.total_value,
						paid_value = dataItem.paid_value,
						due_value = dataItem.due_value,
						status = dataItem.status,
						omit_header = dataItem.omit_header,
						payment_terms_in_days = dataItem.payment_terms_in_days,
						paid_on = dataItem.paid_on
					};

					invoice.Items = new HashedSet<MP_FreeAgentInvoiceItem>();
					foreach (var item in dataItem.invoice_items)
					{
						var mpItem = new MP_FreeAgentInvoiceItem
						{
							Invoice = invoice,
							url = item.url,
							position = item.position,
							description = item.description,
							item_type = item.item_type,
							price = item.price,
							quantity = item.quantity,
							category = item.category
						};
						invoice.Items.Add(mpItem);
					}

					mpRequest.Invoices.Add(invoice);
				});

			expenses.ForEach(
				dataItem =>
				{
					var expense = new MP_FreeAgentExpense
					{
						Request = mpRequest,
						url = dataItem.url,
						username = dataItem.user,
						category = dataItem.category,
						currency = dataItem.currency,
						dated_on = dataItem.dated_on,
						gross_value = dataItem.gross_value,
						native_gross_value = dataItem.native_gross_value,
						sales_tax_rate = dataItem.sales_tax_rate,
						sales_tax_value = dataItem.sales_tax_value,
						native_sales_tax_value = dataItem.native_sales_tax_value,
						description = dataItem.description,
						manual_sales_tax_amount = dataItem.manual_sales_tax_amount,
						updated_at = dataItem.updated_at,
						created_at = dataItem.created_at,
						
					};
					if (dataItem.attachment != null)
					{
						expense.attachment_url = dataItem.attachment.url;
						expense.attachment_content_src = dataItem.attachment.content_src;
						expense.attachment_content_type = dataItem.attachment.content_type;
						expense.attachment_file_name = dataItem.attachment.file_name;
						expense.attachment_file_size = dataItem.attachment.file_size;
						expense.attachment_description = dataItem.attachment.description;
					}
					expense.Category = _FreeAgentExpenseCategoryRepository.Get(dataItem.categoryItem.Id);

					mpRequest.Expenses.Add(expense);
				});

			customerMarketPlace.FreeAgentRequests.Add(mpRequest);
			_CustomerMarketplaceRepository.Update(customerMarketPlace);

			return mpRequest;
		}

		public void StoreFreeAgentCompanyData(MP_FreeAgentCompany company)
		{
			_FreeAgentCompanyRepository.SaveOrUpdate(company);
		}

		public void StoreFreeAgentUsersData(List<MP_FreeAgentUsers> users)
		{
			foreach (MP_FreeAgentUsers user in users)
			{
				_FreeAgentUsersRepository.SaveOrUpdate(user);
			}
		}

		public void StoreYodleeOrdersData(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, YodleeOrderDictionary ordersData, MP_CustomerMarketplaceUpdatingHistory historyRecord)
		{
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace);

			// LogData("Yodlee Orders Data", customerMarketPlace, ordersData);

			if (ordersData == null)
			{
				return;
			}

			DateTime submittedDate = DateTime.UtcNow;
			var mpOrder = new MP_YodleeOrder
			{
				CustomerMarketPlace = customerMarketPlace,
				Created = submittedDate,
				HistoryRecord = historyRecord
			};


			foreach (var item in ordersData.Data.Keys)
			{
				var mpOrderItem = new MP_YodleeOrderItem
					{
						Order = mpOrder,
						isSeidFromDataSource = item.isSeidFromDataSource,
						isSeidFromDataSourceSpecified = item.isSeidFromDataSourceSpecified,
						isSeidMod = item.isSeidMod,
						isSeidModSpecified = item.isSeidModSpecified,
						acctTypeId = item.acctTypeId,
						acctTypeIdSpecified = item.acctTypeIdSpecified,
						acctType = item.acctType,
						localizedAcctType = item.localizedAcctType,
						srcElementId = item.srcElementId,
						individualInformationId = item.individualInformationId,
						individualInformationIdSpecified = item.individualInformationIdSpecified,
						bankAccountId = item.bankAccountId,
						bankAccountIdSpecified = item.bankAccountIdSpecified,
						customName = item.customName,
						customDescription = item.customDescription,
						isDeleted = item.isDeleted,
						isDeletedSpecified = item.isDeletedSpecified,
						lastUpdated = item.lastUpdated,
						lastUpdatedSpecified = item.lastUpdatedSpecified,
						hasDetails = item.hasDetails,
						hasDetailsSpecified = item.hasDetailsSpecified,
						interestRate = item.interestRate,
						interestRateSpecified = item.interestRateSpecified,
						accountNumber = item.accountNumber,
						link = item.link,
						accountHolder = item.accountHolder,
						tranListToDate =
							(item.tranListToDate != null && item.tranListToDate.dateSpecified) ? item.tranListFromDate.date : null,
						tranListFromDate =
							(item.tranListFromDate != null && item.tranListFromDate.dateSpecified)
								? item.tranListFromDate.date
								: null,
						availableBalance =
							(item.availableBalance != null && item.availableBalance.amountSpecified)
								? item.availableBalance.amount
								: null,
						availableBalanceCurrency =
							(item.availableBalance != null && item.availableBalance.amountSpecified)
								? item.availableBalance.currencyCode
								: null,
						currentBalance =
							(item.currentBalance != null && item.currentBalance.amountSpecified)
								? item.currentBalance.amount
								: null,
						currentBalanceCurrency =
							(item.currentBalance != null && item.currentBalance.amountSpecified)
								? item.currentBalance.currencyCode
								: null,
						interestEarnedYtd =
							(item.interestEarnedYtd != null && item.interestEarnedYtd.amountSpecified)
								? item.interestEarnedYtd.amount
								: null,
						interestEarnedYtdCurrency =
							(item.interestEarnedYtd != null && item.interestEarnedYtd.amountSpecified)
								? item.interestEarnedYtd.currencyCode
								: null,
						prevYrInterest =
							(item.prevYrInterest != null && item.prevYrInterest.amountSpecified)
								? item.prevYrInterest.amount
								: null,
						prevYrInterestCurrency =
							(item.prevYrInterest != null && item.prevYrInterest.amountSpecified)
								? item.prevYrInterest.currencyCode
								: null,
						overdraftProtection =
							(item.overdraftProtection != null && item.overdraftProtection.amountSpecified)
								? item.overdraftProtection.amount
								: null,
						overdraftProtectionCurrency =
							(item.overdraftProtection != null && item.overdraftProtection.amountSpecified)
								? item.overdraftProtection.currencyCode
								: null,
						term = item.term,
						accountName = item.accountName,
						annualPercentYield = item.annualPercentYield,
						annualPercentYieldSpecified = item.annualPercentYieldSpecified,
						routingNumber = item.routingNumber,
						maturityDate =
							(item.maturityDate != null && item.maturityDate.dateSpecified) ? item.maturityDate.date : null,
						asOfDate =
							(item.asOfDate != null && item.asOfDate.dateSpecified) ? item.asOfDate.date : mpOrder.Created,
						accountNicknameAtSrcSite = item.accountNicknameAtSrcSite,
						isPaperlessStmtOn = item.isPaperlessStmtOn,
						isPaperlessStmtOnSpecified = item.isPaperlessStmtOnSpecified,
						siteAccountStatusSpecified = item.siteAccountStatusSpecified,
						created = item.created,
						createdSpecified = item.createdSpecified,
						nomineeName = item.nomineeName,
						secondaryAccountHolderName = item.secondaryAccountHolderName,
						accountOpenDate =
							(item.accountOpenDate != null && item.accountOpenDate.dateSpecified)
								? item.accountOpenDate.date
								: null,
						accountCloseDate =
							(item.accountOpenDate != null && item.accountCloseDate.dateSpecified)
								? item.accountCloseDate.date
								: null,
						maturityAmount =
							(item.maturityAmount != null && item.maturityAmount.amountSpecified)
								? item.maturityAmount.amount
								: null,
						maturityAmountCurrency =
							(item.maturityAmount != null && item.maturityAmount.amountSpecified)
								? item.maturityAmount.currencyCode
								: null,
						taxesWithheldYtd =
							(item.taxesWithheldYtd != null && item.taxesWithheldYtd.amountSpecified)
								? item.taxesWithheldYtd.amount
								: null,
						taxesWithheldYtdCurrency =
							(item.taxesWithheldYtd != null && item.taxesWithheldYtd.amountSpecified)
								? item.taxesWithheldYtd.currencyCode
								: null,
						taxesPaidYtd =
							(item.taxesPaidYtd != null && item.taxesPaidYtd.amountSpecified) ? item.taxesPaidYtd.amount : null,
						taxesPaidYtdCurrency =
							(item.taxesPaidYtd != null && item.taxesPaidYtd.amountSpecified)
								? item.taxesPaidYtd.currencyCode
								: null,
						budgetBalance =
							(item.budgetBalance != null && item.budgetBalance.amountSpecified) ? item.budgetBalance.amount : null,
						budgetBalanceCurrency =
							(item.budgetBalance != null && item.budgetBalance.amountSpecified)
								? item.budgetBalance.currencyCode
								: null,
						straightBalance =
							(item.straightBalance != null && item.straightBalance.amountSpecified)
								? item.straightBalance.amount
								: null,
						straightBalanceCurrency =
							(item.straightBalance != null && item.straightBalance.amountSpecified)
								? item.straightBalance.currencyCode
								: null,
						accountClassificationSpecified = item.accountClassificationSpecified,
					};


				foreach (var bankTransaction in ordersData.Data[item])
				{
					var orderBankTransaction = new MP_YodleeOrderItemBankTransaction
						{
							YodleeOrderItem = mpOrderItem,
							isSeidFromDataSource = bankTransaction.isSeidFromDataSource,
							isSeidFromDataSourceSpecified = bankTransaction.isSeidFromDataSourceSpecified,
							isSeidMod = bankTransaction.isSeidMod,
							isSeidModSpecified = bankTransaction.isSeidModSpecified,
							srcElementId = bankTransaction.srcElementId,
							transactionTypeId = bankTransaction.transactionTypeId,
							transactionTypeIdSpecified = bankTransaction.transactionTypeIdSpecified,
							transactionType = bankTransaction.transactionType,
							localizedTransactionType = bankTransaction.localizedTransactionType,
							transactionStatusId = bankTransaction.transactionStatusId,
							transactionStatusIdSpecified = bankTransaction.transactionStatusIdSpecified,
							transactionStatus = bankTransaction.transactionStatus,
							localizedTransactionStatus = bankTransaction.localizedTransactionStatus,
							transactionBaseTypeId = bankTransaction.transactionBaseTypeId,
							transactionBaseTypeIdSpecified = bankTransaction.transactionBaseTypeIdSpecified,
							transactionBaseType = bankTransaction.transactionBaseType,
							localizedTransactionBaseType = bankTransaction.localizedTransactionBaseType,
							categoryId = bankTransaction.categoryId,
							categoryIdSpecified = bankTransaction.categoryIdSpecified,
							bankTransactionId = bankTransaction.bankTransactionId,
							bankTransactionIdSpecified = bankTransaction.bankTransactionIdSpecified,
							bankAccountId = bankTransaction.bankAccountId,
							bankAccountIdSpecified = bankTransaction.bankAccountIdSpecified,
							bankStatementId = bankTransaction.bankStatementId,
							bankStatementIdSpecified = bankTransaction.bankStatementIdSpecified,
							isDeleted = bankTransaction.isDeleted,
							isDeletedSpecified = bankTransaction.isDeletedSpecified,
							lastUpdated = bankTransaction.lastUpdated,
							lastUpdatedSpecified = bankTransaction.lastUpdatedSpecified,
							hasDetails = bankTransaction.hasDetails,
							hasDetailsSpecified = bankTransaction.hasDetailsSpecified,
							transactionId = bankTransaction.transactionId,
							transactionCategory = _MP_YodleeTransactionCategoriesRepository.GetYodleeTransactionCategoryByCategoryId(bankTransaction.transactionCategoryId),
							siteCategoryType = bankTransaction.siteCategoryType,
							siteCategory = bankTransaction.siteCategory,
							classUpdationSource = bankTransaction.classUpdationSource,
							lastCategorised = bankTransaction.lastCategorised,
							transactionDate =
								(bankTransaction.transactionDate != null && bankTransaction.transactionDate.dateSpecified)
									? bankTransaction.transactionDate.date
									: null,
							isReimbursable = bankTransaction.isReimbursable,
							isReimbursableSpecified = bankTransaction.isReimbursableSpecified,
							mcCode = bankTransaction.mcCode,
							prevLastCategorised = bankTransaction.prevLastCategorised,
							prevLastCategorisedSpecified = bankTransaction.prevLastCategorisedSpecified,
							naicsCode = bankTransaction.naicsCode,
							runningBalance =
								(bankTransaction.runningBalance != null && bankTransaction.runningBalance.amountSpecified)
									? bankTransaction.runningBalance.amount
									: null,
							runningBalanceCurrency =
								(bankTransaction.runningBalance != null && bankTransaction.runningBalance.amountSpecified)
									? bankTransaction.runningBalance.currencyCode
									: null,
							userDescription = bankTransaction.userDescription,
							customCategoryId = bankTransaction.customCategoryId,
							customCategoryIdSpecified = bankTransaction.customCategoryIdSpecified,
							memo = bankTransaction.memo,
							parentId = bankTransaction.parentId,
							parentIdSpecified = bankTransaction.parentIdSpecified,
							isOlbUserDesc = bankTransaction.isOlbUserDesc,
							isOlbUserDescSpecified = bankTransaction.isOlbUserDescSpecified,
							categorisationSourceId = bankTransaction.categorisationSourceId,
							plainTextDescription = bankTransaction.plainTextDescription,
							splitType = bankTransaction.splitType,
							categoryLevelId = bankTransaction.categoryLevelId,
							categoryLevelIdSpecified = bankTransaction.categoryLevelIdSpecified,
							calcRunningBalance =
								(bankTransaction.calcRunningBalance != null && bankTransaction.calcRunningBalance.amountSpecified)
									? bankTransaction.calcRunningBalance.amount
									: null,
							calcRunningBalanceCurrency =
								(bankTransaction.calcRunningBalance != null && bankTransaction.calcRunningBalance.amountSpecified)
									? bankTransaction.calcRunningBalance.currencyCode
									: null,
							category = bankTransaction.category,
							link = bankTransaction.link,
							postDate =
								(bankTransaction.postDate != null && bankTransaction.postDate.dateSpecified)
									? bankTransaction.postDate.date
									: null,
							prevTransactionCategoryId = bankTransaction.prevTransactionCategoryId,
							prevTransactionCategoryIdSpecified = bankTransaction.prevTransactionCategoryIdSpecified,
							isBusinessExpense = bankTransaction.isBusinessExpense,
							isBusinessExpenseSpecified = bankTransaction.isBusinessExpenseSpecified,
							descriptionViewPref = bankTransaction.descriptionViewPref,
							descriptionViewPrefSpecified = bankTransaction.descriptionViewPrefSpecified,
							prevCategorisationSourceId = bankTransaction.prevCategorisationSourceId,
							prevCategorisationSourceIdSpecified = bankTransaction.prevCategorisationSourceIdSpecified,
							transactionAmount =
								(bankTransaction.transactionAmount != null && bankTransaction.transactionAmount.amountSpecified)
									? bankTransaction.transactionAmount.amount
									: null,
							transactionAmountCurrency =
								(bankTransaction.transactionAmount != null && bankTransaction.transactionAmount.amountSpecified)
									? bankTransaction.transactionAmount.currencyCode
									: null,
							transactionPostingOrder = bankTransaction.transactionPostingOrder,
							transactionPostingOrderSpecified = bankTransaction.transactionPostingOrderSpecified,
							checkNumber = bankTransaction.checkNumber,
							description = bankTransaction.description,
							isTaxDeductible = bankTransaction.isTaxDeductible,
							isTaxDeductibleSpecified = bankTransaction.isTaxDeductibleSpecified,
							isMedicalExpense = bankTransaction.isMedicalExpense,
							isMedicalExpenseSpecified = bankTransaction.isMedicalExpenseSpecified,
							categorizationKeyword = bankTransaction.categorizationKeyword,
							sourceTransactionType = bankTransaction.sourceTransactionType,
						};
					mpOrderItem.OrderItemBankTransactions.Add(orderBankTransaction);
				}
				mpOrder.OrderItems.Add(mpOrderItem);
			}

			customerMarketPlace.YodleeOrders.Add(mpOrder);
			_CustomerMarketplaceRepository.Update(customerMarketPlace);
		}

		public void StoreChannelGrabberOrdersData(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, ChannelGrabberOrdersList ordersData, MP_CustomerMarketplaceUpdatingHistory historyRecord) {
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace);

			LogData("ChannelGrabber Orders Data", customerMarketPlace, ordersData);

			if (ordersData == null)
				return;

			DateTime submittedDate = DateTime.UtcNow;
			var mpOrder = new MP_ChannelGrabberOrder {
				CustomerMarketPlace = customerMarketPlace,
				Created = submittedDate,
				HistoryRecord = historyRecord
			};

			ordersData.ForEach(dataItem => {
				var mpOrderItem = new MP_ChannelGrabberOrderItem {
					Order = mpOrder,
					NativeOrderId = dataItem.NativeOrderId,
					TotalCost = dataItem.TotalCost,
					CurrencyCode = dataItem.CurrencyCode,
					PaymentDate = dataItem.PaymentDate,
					PurchaseDate = dataItem.PurchaseDate,
					OrderStatus = dataItem.OrderStatus,
					IsExpense = dataItem.IsExpense
				};

				mpOrder.OrderItems.Add(mpOrderItem);
			});

			customerMarketPlace.ChannelGrabberOrders.Add(mpOrder);
			_CustomerMarketplaceRepository.Update(customerMarketPlace);
		} // StoreChannelGrabberOrdersData

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

		public void SaveOrUpdateAcctountInfo(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, PayPalPersonalData data)
		{
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace);

			if (customerMarketPlace.PersonalInfo == null)
			{
				customerMarketPlace.PersonalInfo = new MP_PayPalPersonalInfo
					{
						CustomerMarketPlace = customerMarketPlace,
					};

				_CustomerMarketplaceRepository.Save(customerMarketPlace);
			}

			MP_PayPalPersonalInfo info = customerMarketPlace.PersonalInfo;
			info.Updated = data.SubmittedDate;
			info.BusinessName = data.BusinessName;
			info.City = data.AddressCity;
			info.FirstName = data.FirstName;
			info.Country = data.AddressCountry;
			info.DateOfBirth = data.BirthDate;
			info.Phone = data.Phone;
			info.EMail = data.Email;
			info.LastName = data.LastName;
			info.FullName = data.FullName;
			info.PlayerId = data.PlayerId;
			info.Postcode = data.AddressPostCode;
			info.State = data.AddressState;
			info.Street1 = data.AddressStreet1;
			info.Street2 = data.AddressStreet2;

			_CustomerMarketplaceRepository.SaveOrUpdate(customerMarketPlace);


		}


		public void StoreToDatabaseAggregatedData<TEnum>(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, IEnumerable<IWriteDataInfo<TEnum>> dataInfo, MP_CustomerMarketplaceUpdatingHistory historyRecord)
		{
			_FunctionValuesWriterHelper.SetRangeOfData(databaseCustomerMarketPlace, dataInfo, historyRecord);
		}

		public void SetData<TEnum>(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, IWriteDataInfo<TEnum> data, MP_CustomerMarketplaceUpdatingHistory historyRecord)
		{
			_FunctionValuesWriterHelper.SetData(databaseCustomerMarketPlace, data, historyRecord);
		}

		public IDatabaseAnalysisFunctionValues GetData<TEnum>(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, TEnum functionType, TimePeriodEnum timePeriodType)
		{
			return _FunctionValuesWriterHelper.GetData(databaseCustomerMarketPlace, functionType, timePeriodType);
		}

		public void SavePayPalTransactionInfo(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, PayPalTransactionsList data, MP_CustomerMarketplaceUpdatingHistory historyRecord)
		{
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace);

			LogData("Transaction Data", customerMarketPlace, data);

			if (data == null)
			{
				return;
			}
			var mpTransaction = new MP_PayPalTransaction
			{
				CustomerMarketPlace = customerMarketPlace,
				Created = data.SubmittedDate.ToUniversalTime(),
				HistoryRecord = historyRecord
			};

			if (data.Count != 0)
			{
				data.ForEach(
					dataItem =>
					{
						var mpTransactionItem = new MP_PayPalTransactionItem
							{
								Transaction = mpTransaction,
								Created = dataItem.Created,
								FeeAmount = _CurrencyConvertor.ConvertToBaseCurrency(dataItem.FeeAmount, dataItem.Created),
								GrossAmount = _CurrencyConvertor.ConvertToBaseCurrency(dataItem.GrossAmount, dataItem.Created),
								NetAmount = _CurrencyConvertor.ConvertToBaseCurrency(dataItem.NetAmount, dataItem.Created),
								TimeZone = dataItem.Timezone,
								Status = dataItem.Status,
								Type = dataItem.Type,
								Payer = dataItem.Payer,
								PayerDisplayName = dataItem.PayerDisplayName,
								PayPalTransactionId = dataItem.TransactionId
							};

						mpTransaction.TransactionItems.Add(mpTransactionItem);
					}
					);
			}

			customerMarketPlace.PayPalTransactions.Add(mpTransaction);
			_CustomerMarketplaceRepository.Update(customerMarketPlace);
		}

		#region Last Request Dates
		public DateTime? GetLastPayPalTransactionRequest(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace)
		{
			return _CustomerMarketplaceRepository.GetLastPayPalTransactionRequest(databaseCustomerMarketPlace);
		}

		/*public DateTime? GetLastEbayOrdersRequestWithTerapeak( IDatabaseCustomerMarketPlace databaseCustomerMarketPlace )
		{
			return GetLastTeraPeakOrdersRequestData( databaseCustomerMarketPlace )?? GetLastEbayOrdersRequest( databaseCustomerMarketPlace );
		}*/

		public DateTime? GetLastTeraPeakOrdersRequestData(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace)
		{
			return _CustomerMarketplaceRepository.GetLastTeraPeakOrdersRequestData(databaseCustomerMarketPlace);
		}
		public DateTime? GetLastEbayOrdersRequest(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace)
		{
			return _CustomerMarketplaceRepository.GetLastEbayOrdersRequest(databaseCustomerMarketPlace);
		}

		public DateTime? GetLastAmazonOrdersRequest(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace)
		{
			return _CustomerMarketplaceRepository.GetLastAmazonOrdersRequest(databaseCustomerMarketPlace);
		}

		public DateTime? GetLastInventoryRequest(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace)
		{
			return _CustomerMarketplaceRepository.GetLastInventoryRequest(databaseCustomerMarketPlace);
		}
		#endregion

		public void StoreEbayUserData(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, IDatabaseEbayUserData data, MP_CustomerMarketplaceUpdatingHistory historyRecord)
		{
			if (data == null)
			{
				WriteToLog("StoreEbayUserData: invalid data to store", WriteLogType.Error);
				return;
			}

			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace);

			var userData = new MP_EbayUserData
				{

					CustomerMarketPlace = customerMarketPlace,
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

			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace);

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

		public void StoreEbayFeedbackData(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, DatabaseEbayFeedbackData data, MP_CustomerMarketplaceUpdatingHistory historyRecord)
		{
			if (data == null)
			{
				WriteToLog("StoreEbayUserData: invalid data to store", WriteLogType.Error);
				return;
			}

			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace);

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

		public void StoreAmazonFeedbackData(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, DatabaseAmazonFeedbackData data, MP_CustomerMarketplaceUpdatingHistory historyRecord)
		{
			if (data == null)
			{
				WriteToLog("StoreEbayUserData: invalid data to store", WriteLogType.Error);
				return;
			}

			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace);

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

		public void StoretoDatabaseTeraPeakOrdersData(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, TeraPeakDatabaseSellerData data, MP_CustomerMarketplaceUpdatingHistory historyRecord)
		{
			if (data == null)
			{
				WriteToLog("StoreEbayUserData: invalid data to store", WriteLogType.Error);
				return;
			}

            MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace);

		    var helper = new TeraPeackHelper();
            helper.StoretoDatabaseTeraPeakOrdersData(customerMarketPlace, data, historyRecord);

			_CustomerMarketplaceRepository.Update(customerMarketPlace);
		}

		public bool ExistsTeraPeakOrdersData(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace)
		{
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace);
			return customerMarketPlace.TeraPeakOrders.Count > 0;
		}

		public void CustomerMarketplaceUpdateAction(CustomerMarketplaceUpdateActionType updateActionType, IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, MP_CustomerMarketplaceUpdatingHistory historyRecord, Func<IUpdateActionResultInfo> action)
		{
			var actionData = new DatabaseCustomerMarketplaceUpdateActionData
			{
				ActionName = updateActionType,
				UpdatingStart = DateTime.UtcNow
			};

			MP_CustomerMarketplaceUpdatingActionLog actionLog;
			StoreCustomerMarketplaceUpdateActionDataStart(databaseCustomerMarketPlace, historyRecord, actionData, out actionLog);

			try
			{
				try
				{
					LogDataStart(updateActionType, databaseCustomerMarketPlace);

					IUpdateActionResultInfo result = action();

					LogDataEnd(updateActionType, databaseCustomerMarketPlace);

					if (result != null)
					{
						actionData.ControlValueName = result.Name;
						actionData.ControlValue = result.Value;
						actionData.RequestsCounter = result.RequestsCounter;
						actionData.ElapsedTime = result.ElapsedTime;
					}

					actionData.UpdatingEnd = DateTime.UtcNow;
				}
				catch (WebException ex)
				{
					using (var httpWebResponse = ex.Response as HttpWebResponse)
					{
						if (httpWebResponse != null)
						{
							LogDataException(updateActionType, databaseCustomerMarketPlace, string.Format("Headers : {0}", httpWebResponse.Headers));
							LogDataException(updateActionType, databaseCustomerMarketPlace, string.Format("Status Code : {0}", httpWebResponse.StatusCode));
							LogDataException(updateActionType, databaseCustomerMarketPlace, string.Format("Status Description : {0}", httpWebResponse.StatusDescription));
						}
					}

					throw;
				}
			}
			catch (Exception ex)
			{
				actionData.Error = ex.Message;
				actionData.UpdatingEnd = DateTime.UtcNow;

				LogDataException(updateActionType, databaseCustomerMarketPlace, ex);
				throw;
			}
			finally
			{
				StoreCustomerMarketplaceUpdateActionDataEnd(databaseCustomerMarketPlace, historyRecord, actionData, actionLog);
			}
		}

		private void StoreCustomerMarketplaceUpdateActionDataStart(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, MP_CustomerMarketplaceUpdatingHistory historyRecord, DatabaseCustomerMarketplaceUpdateActionData actionData, out MP_CustomerMarketplaceUpdatingActionLog actionLog)
		{
			actionLog = new MP_CustomerMarketplaceUpdatingActionLog
			{
				HistoryRecord = historyRecord,
				ActionName = actionData.ActionName.ToString(),
				UpdatingStart = actionData.UpdatingStart,
			};

			historyRecord.ActionLog.Add(actionLog);

			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace);

			_CustomerMarketplaceRepository.Update(customerMarketPlace);
		}

		private void StoreCustomerMarketplaceUpdateActionDataEnd(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, MP_CustomerMarketplaceUpdatingHistory historyRecord, DatabaseCustomerMarketplaceUpdateActionData actionData, MP_CustomerMarketplaceUpdatingActionLog actionLog)
		{
			actionLog.UpdatingEnd = actionData.UpdatingEnd;
			actionLog.ControlValue = actionData.ControlValue == null ? null : actionData.ControlValue.ToString();
			actionLog.ControlValueName = !actionData.ControlValueName.HasValue ? null : actionData.ControlValueName.Value.ToString();
			actionLog.Error = actionData.Error;
			actionLog.ElapsedTime = new DatabaseElapsedTimeInfo(actionData.ElapsedTime);

			if (actionData.RequestsCounter != null && actionData.RequestsCounter.Any())
			{
				actionData.RequestsCounter.ForEach(r => actionLog.RequestsCounter.Add(new MP_CustomerMarketplaceUpdatingCounter
						{
							Action = actionLog,
							Created = r.Created,
							Details = r.Details,
							Method = r.Method

						}));


			}



			historyRecord.ActionLog.Add(actionLog);

			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace);

			_CustomerMarketplaceRepository.Update(customerMarketPlace);
		}

		private void LogDataException(CustomerMarketplaceUpdateActionType updateActionType, IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, string text)
		{
			WriteToLog(string.Format("MP = {1} Action = {0} UMI = {2} Text: {3}\n", updateActionType, databaseCustomerMarketPlace.Marketplace.DisplayName, databaseCustomerMarketPlace.Id, text), WriteLogType.Error);
		}
		private void LogDataException(CustomerMarketplaceUpdateActionType updateActionType, IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, Exception exception)
		{
			WriteToLog(string.Format("MP = {1} Action = {0} UMI = {2} Exception\n", updateActionType, databaseCustomerMarketPlace.Marketplace.DisplayName, databaseCustomerMarketPlace.Id), WriteLogType.Error, exception);
		}

		private void LogDataStart(CustomerMarketplaceUpdateActionType updateActionType, IDatabaseCustomerMarketPlace databaseCustomerMarketPlace)
		{
			WriteToLog(string.Format("MP = {1} Action = {0} UMI = {2} starting...", updateActionType, databaseCustomerMarketPlace.Marketplace.DisplayName, databaseCustomerMarketPlace.Id));
		}

		private void LogDataEnd(CustomerMarketplaceUpdateActionType updateActionType, IDatabaseCustomerMarketPlace databaseCustomerMarketPlace)
		{
			WriteToLog(string.Format("MP = {1} Action = {0} UMI = {2} ended!", updateActionType, databaseCustomerMarketPlace.Marketplace.DisplayName, databaseCustomerMarketPlace.Id));
		}

		public AmazonOrdersList2 GetAllAmazonOrdersData(DateTime submittedDate, IDatabaseCustomerMarketPlace databaseCustomerMarketPlace)
		{
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace);

			var orders = new AmazonOrdersList2(submittedDate);

			orders.AddRange(customerMarketPlace.AmazonOrders.SelectMany(amazonOrder => amazonOrder.OrderItems2).Select(o =>
				{
					AmazonOrdersList2ItemStatusType orderStatus;
					Enum.TryParse(o.OrderStatus, out orderStatus);
					return new AmazonOrderItem2
								{
									AmazonOrderId = o.OrderId,
									BuyerEmail = o.BuyerEmail,
									PaymentMethod = o.PaymentMethod,
									OrderStatus = orderStatus,
									BuyerName = o.BuyerName,
									PurchaseDate = o.PurchaseDate,
									ShipServiceLevel = o.ShipServiceLevel,
									FulfillmentChannel = o.FulfillmentChannel,
									LastUpdateDate = o.LastUpdateDate,
									NumberOfItemsShipped = o.NumberOfItemsShipped,
									NumberOfItemsUnshipped = o.NumberOfItemsUnshipped,
									OrderChannel = o.OrderChannel,
									OrderTotal = _CurrencyConvertor.ConvertToBaseCurrency(o.OrderTotal, o.PurchaseDate),
									PaymentsInfo = new AmazonOrderItem2PaymentsInfoList(o.PaymentsInfo.Select(pi => new AmazonOrderItem2PaymentInfoListItem
									{
										MoneyInfo = _CurrencyConvertor.ConvertToBaseCurrency(pi.MoneyInfo, o.PurchaseDate),
										SubPaymentMethod = pi.SubPaymentMethod
									})),
									SalesChannel = o.SalesChannel,
									SellerOrderId = o.SellerOrderId,
									ShipmentAddress = o.ShipmentAddress,
									ShipmentServiceLevelCategory = o.ShipmentServiceLevelCategory

								};
				}));

			return orders;
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

		public TeraPeakDatabaseSellerData GetAllTeraPeakDataWithFullRange(DateTime submittedDate, IDatabaseCustomerMarketPlace databaseCustomerMarketPlace)
		{
			return _CustomerMarketplaceRepository.GetAllTeraPeakDataWithFullRange(submittedDate, databaseCustomerMarketPlace);
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

		public PayPalTransactionsList GetAllPayPalTransactions(DateTime submittedDate, IDatabaseCustomerMarketPlace databaseCustomerMarketPlace)
		{
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace);

			var data = new PayPalTransactionsList(submittedDate);

			customerMarketPlace.PayPalTransactions.ForEach(tr => tr.TransactionItems.ForEach(t => data.Add(new PayPalTransactionItem
				{
					Created = t.Created,
					Type = t.Type,
					FeeAmount = _CurrencyConvertor.ConvertToBaseCurrency(t.FeeAmount, t.Created),
					GrossAmount = _CurrencyConvertor.ConvertToBaseCurrency(t.GrossAmount, t.Created),
					NetAmount = _CurrencyConvertor.ConvertToBaseCurrency(t.NetAmount, t.Created),
					Payer = t.Payer,
					PayerDisplayName = t.PayerDisplayName,
					Status = t.Status,
					Timezone = t.TimeZone,
					TransactionId = t.PayPalTransactionId
				})));

			return data;

		}

		public bool ExistsEBayOrderItemInfo(eBayFindOrderItemInfoData eBayFindOrderItemInfoData, ElapsedTimeInfo elapsedTimeInfo)
		{
			MP_EBayOrderItemDetail value;
			if (!_CacheEBayOrderItemInfo.TryGetValue(eBayFindOrderItemInfoData.ItemId, out value))
			{
				value = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
									ElapsedDataMemberType.RetrieveDataFromDatabase,
									() => _EBayOrderItemInfoRepository.FindItem(eBayFindOrderItemInfoData));

				if (value != null)
				{
					_CacheEBayOrderItemInfo.TryAdd(value.ItemID, value);
				}
			}

			return value != null;
		}

		public MP_EBayOrderItemDetail FindEBayOrderItemInfo(eBayFindOrderItemInfoData eBayFindOrderItemInfoData, ElapsedTimeInfo elapsedTimeInfo)
		{
			if (eBayFindOrderItemInfoData == null || eBayFindOrderItemInfoData.ItemId == null)
			{
				return null;
			}

			MP_EBayOrderItemDetail value;
			if (!_CacheEBayOrderItemInfo.TryGetValue(eBayFindOrderItemInfoData.ItemId, out value))
			{
				value = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
									ElapsedDataMemberType.RetrieveDataFromDatabase,
									() => _EBayOrderItemInfoRepository.FindItem(eBayFindOrderItemInfoData));

				if (value != null)
				{
					_CacheEBayOrderItemInfo.TryAdd(value.ItemID, value);
				}
			}

			return value;

		}

		public MP_EBayOrderItemDetail SaveEBayOrderItemInfo(EbayDatabaseOrderItemInfo data, ElapsedTimeInfo elapsedTimeInfo)
		{
			MP_EbayAmazonCategory mpEbayAmazonCategory = null;
			if (data.PrimaryCategory != null)
			{
				mpEbayAmazonCategory = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
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

			ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
									ElapsedDataMemberType.StoreDataToDatabase,
									() => _EBayOrderItemInfoRepository.Save(item));

			_CacheEBayOrderItemInfo.TryAdd(item.ItemID, item);

			return item;
		}

		public MP_EbayAmazonCategory FindEBayAmazonCategory(IMarketplaceType marketplace, string categoryId, ElapsedTimeInfo elapsedTimeInfo)
		{
			MP_EbayAmazonCategory value;
			var cache = GetCache(marketplace);

			if (!cache.TryGetValue(categoryId, out value))
			{
				value = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
									ElapsedDataMemberType.RetrieveDataFromDatabase,
									() => _EbayAmazonCategoryRepository.FindItem(categoryId));

				if (value != null)
				{
					AddCategoryToCache(marketplace, value);
				}
			}

			return value;
		}

		public MP_EbayAmazonCategory AddEbayCategory(IMarketplaceType marketplace, eBayCategoryInfo data, ElapsedTimeInfo elapsedTimeInfo)
		{
			var item = new MP_EbayAmazonCategory
			{
				CategoryId = data.CategoryId,
				IsVirtual = data.IsVirtual,
				Name = data.Name,
				Marketplace = GetMarketPlace(marketplace)
			};

			ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
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

		private void AddCategoryToCache(IMarketplaceType marketplace, MP_EbayAmazonCategory item)
		{
			var cache = GetCache(marketplace);

			cache.TryAdd(item.CategoryId, item);
		}

		public MP_EbayAmazonCategory[] AddAmazonCategories(IMarketplaceType marketplace, AmazonProductItemBase productItem, ElapsedTimeInfo elapsedTimeInfo)
		{
			var categories = new List<MP_EbayAmazonCategory>();

			foreach (AmazonProductCategory amazonProductCategory in productItem.Categories)
			{
				var cat = FindEBayAmazonCategory(marketplace, amazonProductCategory.CategoryId, elapsedTimeInfo) ?? AddAmazonCategory(marketplace, amazonProductCategory, elapsedTimeInfo);

				categories.Add(cat);

			}
			_CacheAmazonCategoryByProductKey.TryAdd(productItem.Key, categories.ToArray());

			return categories.ToArray();
		}

		private MP_EbayAmazonCategory AddAmazonCategory(IMarketplaceType marketplace, AmazonProductCategory amazonProductCategory, ElapsedTimeInfo elapsedTimeInfo)
		{
			var cat = new MP_EbayAmazonCategory
			{
				CategoryId = amazonProductCategory.CategoryId,
				Name = amazonProductCategory.CategoryName,
				Marketplace = GetMarketPlace(marketplace),
				Parent = amazonProductCategory.Parent == null ? null : FindEBayAmazonCategory(marketplace, amazonProductCategory.Parent.CategoryId, elapsedTimeInfo) ?? AddAmazonCategory(marketplace, amazonProductCategory.Parent, elapsedTimeInfo)
			};

			AddCategoryToCache(marketplace, cat);

			ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
									ElapsedDataMemberType.StoreDataToDatabase,
									() => _EbayAmazonCategoryRepository.Save(cat));

			return cat;
		}

		public MP_EbayAmazonCategory[] FindAmazonCategoryByProductAsin(string asin, ElapsedTimeInfo elapsedTimeInfo)
		{
			MP_EbayAmazonCategory[] cat;

			if (!_CacheAmazonCategoryByProductKey.TryGetValue(asin, out cat))
			{
				cat = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
									ElapsedDataMemberType.RetrieveDataFromDatabase,
									() => _AmazonOrderItemDetailRepository.FindCategoriesByAsin(asin));
				if (cat != null)
				{
					_CacheAmazonCategoryByProductKey.TryAdd(asin, cat);
				}
			}

			return cat;
		}

		public MP_EbayAmazonCategory[] FindAmazonCategoryByProductSellerSKU(string sellerSKU, ElapsedTimeInfo elapsedTimeInfo)
		{
			MP_EbayAmazonCategory[] cat;

			if (!_CacheAmazonCategoryByProductKey.TryGetValue(sellerSKU, out cat))
			{
				cat = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
									ElapsedDataMemberType.RetrieveDataFromDatabase,
									() => _AmazonOrderItemDetailRepository.FindCategoriesBySellectSku(sellerSKU));
				if (cat != null)
				{
					_CacheAmazonCategoryByProductKey.TryAdd(sellerSKU, cat);
				}
			}

			return cat;
		}

		public void WriteToLog(string message, WriteLogType messageType = WriteLogType.Info, Exception ex = null)
		{
			WriteLoggerHelper.Write(message, messageType, null, ex);
			Debug.WriteLine(message);
		}

		public IQueryable<MP_AnalyisisFunctionValue> GetAnalyisisFunctions()
		{
			return _session.Query<MP_AnalyisisFunctionValue>();
		}

		public IQueryable<MP_EbayFeedback> GetEbayFeedback()
		{
			return _session.Query<MP_EbayFeedback>();
		}

		public IQueryable<MP_AmazonFeedback> GetAmazonFeedback()
		{
			return _session.Query<MP_AmazonFeedback>();
		}

		public IQueryable<MP_YodleeOrderItem> GetYodleeOrderItems()
		{
			return _session.Query<MP_YodleeOrderItem>();
		}

		public IQueryable<MP_YodleeOrderItemBankTransaction> GetYodleeOrderItemBankTransactions()
		{
			return _session.Query<MP_YodleeOrderItemBankTransaction>();
		}

		public EkmOrdersList GetAllEkmOrdersData(DateTime submittedDate, IDatabaseCustomerMarketPlace databaseCustomerMarketPlace)
		{
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace);

			var orders = new EkmOrdersList(submittedDate);

			orders.AddRange(customerMarketPlace.EkmOrders.SelectMany(ekmOrder => ekmOrder.OrderItems).Select(o => new EkmOrderItem
			{
				OrderNumber = o.OrderNumber,
				CustomerID = o.CustomerId,
				CompanyName = o.CompanyName,
				FirstName = o.FirstName,
				LastName = o.LastName,
				EmailAddress = o.EmailAddress,
				TotalCost = o.TotalCost,
				OrderDate = o.OrderDate,
				OrderStatus = o.OrderStatus,
				OrderDateIso = o.OrderDateIso,
				OrderStatusColour = o.OrderStatusColour,

			}).Distinct(new EkmOrderComparer()));

			return orders;
		}

		public FreeAgentInvoicesList GetAllFreeAgentInvoicesData(DateTime submittedDate, IDatabaseCustomerMarketPlace databaseCustomerMarketPlace)
		{
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace);

			var invoices = new FreeAgentInvoicesList(submittedDate);

			var dbInvoices = customerMarketPlace.FreeAgentRequests.SelectMany(freeAgentRequest => freeAgentRequest.Invoices).OrderByDescending(invoice => invoice.Request.Id).Distinct(new FreeAgentInvoiceComparer()).OrderByDescending(invoice => invoice.dated_on);
			
			invoices.AddRange(dbInvoices.Select(o => new FreeAgentInvoice
			{
				url = o.url,
				contact = o.contact,
				dated_on = o.dated_on,
				due_on = o.due_on,
				reference = o.reference,
				currency = o.currency,
				exchange_rate = o.exchange_rate,
				net_value = o.net_value,
				total_value = o.total_value,
				paid_value = o.paid_value,
				due_value = o.due_value,
				status = o.status,
				omit_header = o.omit_header,
				payment_terms_in_days = o.payment_terms_in_days,
				paid_on = o.paid_on
			}));

			return invoices;
		}

		public FreeAgentExpensesList GetAllFreeAgentExpensesData(DateTime submittedDate, IDatabaseCustomerMarketPlace databaseCustomerMarketPlace)
		{
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace);

			var expenses = new FreeAgentExpensesList(submittedDate);

			var dbExpenses = customerMarketPlace.FreeAgentRequests.SelectMany(freeAgentRequest => freeAgentRequest.Expenses).OrderByDescending(expense => expense.Request.Id).Distinct(new FreeAgentExpenseComparer()).OrderByDescending(expense => expense.dated_on);

			expenses.AddRange(dbExpenses.Select(o => new FreeAgentExpense
			{
				url = o.url,
				user = o.username,
				category = o.category,
				dated_on = o.dated_on,
				currency = o.currency,
				gross_value = o.gross_value,
				native_gross_value = o.native_gross_value,
				sales_tax_rate = o.sales_tax_rate,
				sales_tax_value = o.sales_tax_value,
				native_sales_tax_value = o.native_sales_tax_value,
				description = o.description,
				manual_sales_tax_amount = o.manual_sales_tax_amount,
				updated_at = o.updated_at,
				created_at = o.created_at,
				attachment = new FreeAgentExpenseAttachment
				{
					url = o.attachment_url,
					content_src = o.attachment_content_src,
					content_type = o.attachment_content_type,
					file_name = o.attachment_file_name,
					file_size = o.attachment_file_size,
					description = o.attachment_description
				}
			}));

			return expenses;
		}

		public ChannelGrabberOrdersList GetAllChannelGrabberOrdersData(DateTime submittedDate, IDatabaseCustomerMarketPlace databaseCustomerMarketPlace)
		{
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace);

			var orders = new ChannelGrabberOrdersList(submittedDate);

			orders.AddRange(customerMarketPlace.ChannelGrabberOrders.SelectMany(anOrder => anOrder.OrderItems).Select(o => new ChannelGrabberOrderItem {
				CurrencyCode = o.CurrencyCode,
				OrderStatus = o.OrderStatus,
				NativeOrderId = o.NativeOrderId,
				PaymentDate = o.PaymentDate,
				PurchaseDate = o.PurchaseDate,
				TotalCost = o.TotalCost,
				IsExpense = o.IsExpense
			}).Distinct(new ChannelGrabberOrderComparer()));

			return orders;
		} // GetAllChannelGrabberOrdersData

		public bool HasYodleeOrders(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace)
		{
			return !GetCustomerMarketPlace(databaseCustomerMarketPlace).YodleeOrders.IsEmpty;
		} // HasYodleeOrders

		public DateTime GetEkmDeltaPeriod(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace)
		{
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace);

			var def = DateTime.Today.AddYears(-1);

			try
			{
				MP_EkmOrder o = customerMarketPlace.EkmOrders.OrderBy(x => x.Id).AsQueryable().Last();
				return o == null ? def : o.Created.AddMonths(-1);
			}
			catch (Exception)
			{
				return def;
			}
		} // GetEkmDeltaPeriod

		public int GetFreeAgentInvoiceDeltaPeriod(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace)
		{
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace);

			MP_FreeAgentRequest order = customerMarketPlace.FreeAgentRequests.OrderBy(x => x.Id).AsQueryable().LastOrDefault();
			if (order == null)
			{
				return -1;
			}

			MP_FreeAgentInvoice item = order.Invoices.OrderBy(x => x.dated_on).AsQueryable().LastOrDefault();
			DateTime latestExistingDate = item != null ? item.dated_on : order.Created;

			DateTime later = DateTime.UtcNow;
			int monthDiff = 1;
			while (later > latestExistingDate)
			{
				later = later.AddMonths(-1);
				monthDiff++;
			}

			if (monthDiff == 0)
			{
				monthDiff = -1;
			}

			return monthDiff;
		} // GetFreeAgentInvoiceDeltaPeriod

		public DateTime? GetFreeAgentExpenseDeltaPeriod(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace)
		{
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace);

			MP_FreeAgentRequest order = customerMarketPlace.FreeAgentRequests.OrderBy(x => x.Id).AsQueryable().LastOrDefault();
			if (order == null)
			{
				return null;
			}

			MP_FreeAgentExpense item = order.Expenses.OrderBy(x => x.dated_on).AsQueryable().LastOrDefault();
			DateTime latestExistingDate = item != null ? item.dated_on : order.Created;
			return latestExistingDate.AddMonths(-1);
		} // GetFreeAgentExpenseDeltaPeriod

		public string GetPayPointDeltaPeriod(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace)
		{
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace);

			DateTime dThen = DateTime.Today.AddYears(-1);

			try
			{
				MP_PayPointOrder ppo = customerMarketPlace.PayPointOrders.OrderBy(x => x.Id).AsQueryable().LastOrDefault();

				if (ppo != null)
				{
					MP_PayPointOrderItem item = ppo.OrderItems.OrderBy(x => x.date).AsQueryable().LastOrDefault();
					if (item != null && item.date.HasValue)
					{
						dThen = item.date.Value.AddMonths(-1);
					}
					else
					{
						dThen = ppo.Created.AddMonths(-1);
					}
				}
			}
			catch (Exception)
			{
				// so what? ignored.
			}

			return
				dThen.Year +
				dThen.Month.ToString("00") + "-" +
				DateTime.Today.Year +
				DateTime.Today.Month.ToString("00");
		} // GetPayPointDeltaPeriod

		public PayPointOrdersList GetAllPayPointOrdersData(DateTime submittedDate, IDatabaseCustomerMarketPlace databaseCustomerMarketPlace)
		{
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace);

			var orders = new PayPointOrdersList(submittedDate);

			orders.AddRange(customerMarketPlace.PayPointOrders.SelectMany(anOrder => anOrder.OrderItems).Select(x => new PayPointOrderItem
			{
				acquirer = x.acquirer,
				amount = x.amount,
				auth_code = x.auth_code,
				authorised = x.authorised,
				card_type = x.card_type,
				cid = x.cid,
				classType = x.classType,
				company_no = x.company_no,
				country = x.country,
				currency = x.currency,
				cv2avs = x.cv2avs,
				date = x.date,
				start_date = x.start_date,
				ExpiryDate = x.ExpiryDate,
				deferred = x.deferred,
				emvValue = x.emvValue,
				fraud_code = x.fraud_code,
				FraudScore = x.FraudScore,
				ip = x.ip,
				lastfive = x.lastfive,
				merchant_no = x.merchant_no,
				message = x.message,
				MessageType = x.MessageType,
				mid = x.mid,
				name = x.name,
				options = x.options,
				status = x.status,
				tid = x.tid,
				trans_id = x.trans_id
			}).Distinct(new PayPointOrderComparer()));

			return orders;
		} // GetAllPayPointOrdersData
		
		public Dictionary<string, FreeAgentExpenseCategory> GetExpenseCategories()
		{
			var categoriesMap = new Dictionary<string, FreeAgentExpenseCategory>();
			foreach (MP_FreeAgentExpenseCategory dbCategory in _FreeAgentExpenseCategoryRepository.GetAll())
			{
				var category = new FreeAgentExpenseCategory
					{
						Id = dbCategory.Id,
						category_group = dbCategory.category_group,
						url = dbCategory.url,
						description = dbCategory.description,
						nominal_code = dbCategory.nominal_code,
						allowable_for_tax = dbCategory.allowable_for_tax,
						tax_reporting_name = dbCategory.tax_reporting_name,
						auto_sales_tax_rate = dbCategory.auto_sales_tax_rate
					};

				categoriesMap.Add(category.url, category);
			}

			return categoriesMap;
		}

		public int AddExpenseCategory(FreeAgentExpenseCategory category)
		{
			var dbCategory = new MP_FreeAgentExpenseCategory
				{
					category_group = category.category_group,
					url = category.url,
					description = category.description,
					nominal_code = category.nominal_code,
					allowable_for_tax = category.allowable_for_tax,
					tax_reporting_name = category.tax_reporting_name,
					auto_sales_tax_rate = category.auto_sales_tax_rate
				};
			return (int)_FreeAgentExpenseCategoryRepository.Save(dbCategory);
		}
	}

	public class eBayFindOrderItemInfoData
	{
		public eBayFindOrderItemInfoData(string itemId)
		{
			ItemId = itemId;
		}

		public string ItemId { get; private set; }
	}

	public class eBayCategoryInfo
	{
		public string CategoryId { get; set; }

		public string Name { get; set; }

		public bool? IsVirtual { get; set; }

		//public string[] ParentIdList { get; set; }
	}
}
