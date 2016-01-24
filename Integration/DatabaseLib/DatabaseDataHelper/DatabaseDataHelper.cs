namespace EZBob.DatabaseLib {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Net;
	using Model.Database.Mapping;
	using Newtonsoft.Json;
	using Model.Database.Broker;
	using Model.Database.Loans;
	using DatabaseWrapper;
	using DatabaseWrapper.Order;
	using Model;
	using Model.Database;
	using Model.Database.Repository;
	using Model.Loans;
	using EzBob.CommonLib;
	using EzBob.CommonLib.ReceivedDataListLogic;
	using EzBob.CommonLib.TimePeriodLogic;
	using NHibernate;
	using NHibernate.Linq;
	using Repository;
	using StructureMap;
	using log4net;
	using Iesi.Collections.Generic;
	using Model.Marketplaces.FreeAgent;
	using Model.Marketplaces.Sage;
	using Model.Marketplaces.Yodlee;

	public enum CustomerMarketplaceUpdateActionType {
		UpdateInventoryInfo,
		UpdateOrdersInfo,
		UpdateFeedbackInfo,
		UpdateUserInfo,
		UpdateAccountInfo,
		TeraPeakSearchBySeller,
		EbayGetOrders,
		UpdateTransactionInfo
	} // enum CustomerMarketplaceUpdateActionType

	public partial class DatabaseDataHelper : IDatabaseDataHelper {
		public BrokerRepository BrokerRepository { get { return _brokerRepository; } }

		public ICurrencyConvertor CurrencyConverter { get { return _CurrencyConvertor; } }

		public ExperianDirectorRepository ExperianDirectorRepository { get { return m_oExperianDirectorRepository; } }

		public LoanTransactionMethodRepository LoanTransactionMethodRepository { get { return _loanTransactionMethodRepository; } }

		public ILoanTypeRepository LoanTypeRepository { get { return _LoanTypeRepository; } }

		public TrustPilotStatusRepository TrustPilotStatusRepository { get { return _trustPilotStatusRepository; } }

		public WizardStepRepository WizardSteps {
			get { return _wizardStepRepository; }
		}

		public string WizardStepSequence {
			get {
				var oResult = new {
					online = new Dictionary<string, object>(),
					offline = new Dictionary<string, object>()
				};

				_wizardStepSequenceRepository.GetAll().ForEach(wss => {
					if (wss.OnlineProgressBarPct.HasValue)
						oResult.online[wss.Name()] = new { position = wss.OnlineProgressBarPct.Value, type = wss.WizardStep.ID };

					if (wss.OfflineProgressBarPct.HasValue)
						oResult.offline[wss.Name()] = new { position = wss.OfflineProgressBarPct.Value, type = wss.WizardStep.ID };
				});

				return JsonConvert.SerializeObject(oResult);
			}
		}

		public DatabaseDataHelper(ISession session) {
			_session = session;

			_MarketPlaceRepository = new MarketPlaceRepository(session);
			_CustomerMarketplaceRepository = new CustomerMarketPlaceRepository(session);
			_AnalysisFunctionTimePeriodRepository = new AnalysisFunctionTimePeriodRepository(session);
			_CustomerRepository = new CustomerRepository(session);
			_EbayUserAddressDataRepository = new EbayUserAddressDataRepository(session);

			_CurrencyRateRepository = ObjectFactory.GetInstance<CurrencyRateRepository>();
			_CurrencyConvertor = new CurrencyConvertor(_CurrencyRateRepository);
			_EBayOrderItemInfoRepository = new EBayOrderItemInfoRepository(session);
			_EbayAmazonCategoryRepository = new EbayAmazonCategoryRepository(session);
			_AmazonOrderItemDetailRepository = new AmazonOrderItemDetailRepository(session);
			_MP_EbayOrderRepository = new MP_EbayOrderRepository(session);
			_MP_EbayTransactionsRepository = new MP_EbayTransactionsRepository(session);
			_LoanTypeRepository = new LoanTypeRepository(session);
			_FreeAgentCompanyRepository = new MP_FreeAgentCompanyRepository(session);
			_FreeAgentUsersRepository = new MP_FreeAgentUsersRepository(session);
			_FreeAgentExpenseCategoryRepository = new MP_FreeAgentExpenseCategoryRepository(session);
			this.yodleeTransactionCategoriesRepository = new MP_YodleeTransactionCategoriesRepository(session);
			_SagePaymentStatusRepository = new MP_SagePaymentStatusRepository(session);
			_loanTransactionMethodRepository = new LoanTransactionMethodRepository(session);
			_amazonMarketPlaceTypeRepository = new AmazonMarketPlaceTypeRepository(session);
			_wizardStepSequenceRepository = new WizardStepSequenceRepository(session);

			this.yodleeBanksRepository = new YodleeBanksRepository(session);

			_wizardStepRepository = new WizardStepRepository(session);
			_trustPilotStatusRepository = new TrustPilotStatusRepository(session);
			_brokerRepository = new BrokerRepository(session);
			customerMarketPlaceUpdatingHistoryRepository = new CustomerMarketPlaceUpdatingHistoryRepository(session);

			m_oExperianDirectorRepository = new ExperianDirectorRepository(session);
		}

		public int AddFreeAgentExpenseCategory(FreeAgentExpenseCategory category) {
			MP_FreeAgentExpenseCategory dbCategory = _FreeAgentExpenseCategoryRepository.Find(category);

			if (dbCategory != null) {
				dbCategory.category_group = category.category_group;
				dbCategory.description = category.description;
				dbCategory.nominal_code = category.nominal_code;
				dbCategory.allowable_for_tax = category.allowable_for_tax;
				dbCategory.tax_reporting_name = category.tax_reporting_name;
				dbCategory.auto_sales_tax_rate = category.auto_sales_tax_rate;
				_FreeAgentExpenseCategoryRepository.SaveOrUpdate(dbCategory);

				return dbCategory.Id;
			} // if

			dbCategory = new MP_FreeAgentExpenseCategory {
				category_group = category.category_group,
				url = category.url,
				description = category.description,
				nominal_code = category.nominal_code,
				allowable_for_tax = category.allowable_for_tax,
				tax_reporting_name = category.tax_reporting_name,
				auto_sales_tax_rate = category.auto_sales_tax_rate
			};

			return (int)_FreeAgentExpenseCategoryRepository.Save(dbCategory);
		} // AddFreeAgentExpenseCategory

		public IDatabaseCustomerMarketPlace CreateDatabaseCustomerMarketPlace(string marketPlaceName, IMarketplaceType databaseMarketplace, Customer databaseCustomer) {
			MP_CustomerMarketPlace mpCustomerMarketPlace = _CustomerMarketplaceRepository.Get(databaseCustomer.Id, databaseMarketplace.InternalId, marketPlaceName);
			return CreateDatabaseCustomerMarketPlace(databaseCustomer, databaseMarketplace, mpCustomerMarketPlace, mpCustomerMarketPlace.Id);
		}

		public IDatabaseCustomerMarketPlace CreateDatabaseCustomerMarketPlace(Customer databaseCustomer, IMarketplaceType databaseMarketplace, MP_CustomerMarketPlace cm, int customerMarketPlaceId) {
			cm.SetIMarketplaceType(databaseMarketplace);
			return cm;
			//return new DatabaseCustomerMarketPlace(customerMarketPlaceId, cm.DisplayName, cm.SecurityData, databaseCustomer, databaseMarketplace);
		}

		public void CustomerMarketplaceUpdateAction(
			CustomerMarketplaceUpdateActionType updateActionType,
			IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
			MP_CustomerMarketplaceUpdatingHistory historyRecord,
			Func<IUpdateActionResultInfo> action
		) {
			var actionData = new DatabaseCustomerMarketplaceUpdateActionData {
				ActionName = updateActionType,
				UpdatingStart = DateTime.UtcNow
			};

			MP_CustomerMarketplaceUpdatingActionLog actionLog;
			StoreCustomerMarketplaceUpdateActionDataStart(databaseCustomerMarketPlace, historyRecord, actionData, out actionLog);

			try {
				try {
					LogDataStart(updateActionType, databaseCustomerMarketPlace);

					IUpdateActionResultInfo result = action();

					LogDataEnd(updateActionType, databaseCustomerMarketPlace);

					if (result != null) {
						actionData.ControlValueName = result.Name;
						actionData.ControlValue = result.Value;
						actionData.RequestsCounter = result.RequestsCounter;
						actionData.ElapsedTime = result.ElapsedTime;
					}

					actionData.UpdatingEnd = DateTime.UtcNow;
				} catch (WebException ex) {
					using (var httpWebResponse = ex.Response as HttpWebResponse) {
						if (httpWebResponse != null) {
							LogDataException(updateActionType, databaseCustomerMarketPlace, string.Format("Headers : {0}", httpWebResponse.Headers));
							LogDataException(updateActionType, databaseCustomerMarketPlace, string.Format("Status Code : {0}", httpWebResponse.StatusCode));
							LogDataException(updateActionType, databaseCustomerMarketPlace, string.Format("Status Description : {0}", httpWebResponse.StatusDescription));
						}
					}

					throw;
				}
			} catch (Exception ex) {
				actionData.Error = ex.Message;
				actionData.UpdatingEnd = DateTime.UtcNow;

				LogDataException(updateActionType, databaseCustomerMarketPlace, ex);
				throw;
			} finally {
				StoreCustomerMarketplaceUpdateActionDataEnd(databaseCustomerMarketPlace, historyRecord, actionData, actionLog);
			}
		}

		public EkmOrdersList GetAllEkmOrdersData(DateTime submittedDate, IDatabaseCustomerMarketPlace databaseCustomerMarketPlace) {
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace.Id);

			var orders = new EkmOrdersList(submittedDate);

			orders.AddRange(customerMarketPlace.EkmOrders.SelectMany(ekmOrder => ekmOrder.OrderItems).Select(o => new EkmOrderItem {
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

		public FreeAgentExpensesList GetAllFreeAgentExpensesData(DateTime submittedDate, IDatabaseCustomerMarketPlace databaseCustomerMarketPlace) {
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace.Id);

			var expenses = new FreeAgentExpensesList(submittedDate);

			var dbExpenses = customerMarketPlace.FreeAgentRequests.SelectMany(freeAgentRequest => freeAgentRequest.Expenses).OrderByDescending(expense => expense.Request.Id).Distinct(new FreeAgentExpenseComparer()).OrderByDescending(expense => expense.dated_on);

			expenses.AddRange(dbExpenses.Select(o => new FreeAgentExpense {
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
				attachment = new FreeAgentExpenseAttachment {
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

		public FreeAgentInvoicesList GetAllFreeAgentInvoicesData(DateTime submittedDate, IDatabaseCustomerMarketPlace databaseCustomerMarketPlace) {
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace.Id);

			var invoices = new FreeAgentInvoicesList(submittedDate);

			var dbInvoices = customerMarketPlace.FreeAgentRequests.SelectMany(freeAgentRequest => freeAgentRequest.Invoices).OrderByDescending(invoice => invoice.Request.Id).Distinct(new FreeAgentInvoiceComparer()).OrderByDescending(invoice => invoice.dated_on);

			invoices.AddRange(dbInvoices.Select(o => new FreeAgentInvoice {
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

		public PayPointOrdersList GetAllPayPointOrdersData(DateTime submittedDate, IDatabaseCustomerMarketPlace databaseCustomerMarketPlace) {
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace.Id);

			var orders = new PayPointOrdersList(submittedDate);

			orders.AddRange(customerMarketPlace.PayPointOrders.SelectMany(anOrder => anOrder.OrderItems).Select(x => new PayPointOrderItem {
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
		}

		public SageExpendituresList GetAllSageExpendituresData(DateTime submittedDate, IDatabaseCustomerMarketPlace databaseCustomerMarketPlace) {
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace.Id);

			var expenditures = new SageExpendituresList(submittedDate);

			var dbExpenditure = customerMarketPlace.SageRequests.SelectMany(sageRequest => sageRequest.Expenditures).OrderByDescending(expenditure => expenditure.Request.Id).Distinct(new SageExpenditureComparer()).OrderByDescending(expenditure => expenditure.date);

			expenditures.AddRange(SageExpendituresConverter.GetSageExpenditures(dbExpenditure));

			return expenditures;
		}

		public SageIncomesList GetAllSageIncomesData(DateTime submittedDate, IDatabaseCustomerMarketPlace databaseCustomerMarketPlace) {
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace.Id);

			var incomes = new SageIncomesList(submittedDate);

			var dbIncomes = customerMarketPlace.SageRequests.SelectMany(sageRequest => sageRequest.Incomes).OrderByDescending(income => income.Request.Id).Distinct(new SageIncomeComparer()).OrderByDescending(income => income.date);

			incomes.AddRange(SageIncomesConverter.GetSageIncomes(dbIncomes));

			return incomes;
		}

		public SagePurchaseInvoicesList GetAllSagePurchaseInvoicesData(DateTime submittedDate, IDatabaseCustomerMarketPlace databaseCustomerMarketPlace) {
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace.Id);

			var purchaseInvoices = new SagePurchaseInvoicesList(submittedDate);

			var dbPurchaseInvoices = customerMarketPlace.SageRequests.SelectMany(sageRequest => sageRequest.PurchaseInvoices).OrderByDescending(purchaseInvoice => purchaseInvoice.Request.Id).Distinct(new SagePurchaseInvoiceComparer()).OrderByDescending(purchaseInvoice => purchaseInvoice.date);

			purchaseInvoices.AddRange(SagePurchaseInvoicesConverter.GetSagePurchaseInvoices(dbPurchaseInvoices));

			return purchaseInvoices;
		}

		public SageSalesInvoicesList GetAllSageSalesInvoicesData(DateTime submittedDate, IDatabaseCustomerMarketPlace databaseCustomerMarketPlace) {
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace.Id);

			var salesInvoices = new SageSalesInvoicesList(submittedDate);

			var dbSalesInvoices = customerMarketPlace.SageRequests.SelectMany(sageRequest => sageRequest.SalesInvoices).OrderByDescending(salesInvoice => salesInvoice.Request.Id).Distinct(new SageSalesInvoiceComparer()).OrderByDescending(salesInvoice => salesInvoice.date);

			salesInvoices.AddRange(SageSalesInvoicesConverter.GetSageSalesInvoices(dbSalesInvoices));

			return salesInvoices;
		}

		public Customer GetCustomerInfo(int clientId) { return _CustomerRepository.Get(clientId); }

		public MP_CustomerMarketPlace GetCustomerMarketPlace(int customerMarketPlaceId) {
			return _CustomerMarketplaceRepository.Get(customerMarketPlaceId);
		}

		public IEnumerable<IDatabaseCustomerMarketPlace> GetCustomerMarketPlaceList(Customer customer, IMarketplaceType databaseMarketplace) {
			MP_MarketplaceType marketplaceType = _MarketPlaceRepository.Get(databaseMarketplace.InternalId);

			var data = _CustomerMarketplaceRepository.Get(customer, marketplaceType);

			return data.Select(cm => CreateDatabaseCustomerMarketPlace(customer, databaseMarketplace, cm, cm.Id)).ToList();
		}

		public IDatabaseCustomerMarketPlace GetDatabaseCustomerMarketPlace(IMarketplaceType marketplaceType, int customerMarketPlaceId) {
			MP_CustomerMarketPlace mp = GetCustomerMarketPlace(customerMarketPlaceId);
			var customer = mp.Customer;
			return CreateDatabaseCustomerMarketPlace(mp.DisplayName, marketplaceType, customer);
		}

		// GetAnalyisisFunctions
		public DateTime GetEkmDeltaPeriod(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace) {
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace.Id);

			var def = DateTime.Today.AddYears(-1);

			// We want data from the 1st of the month
			def = def.AddDays(-def.Day)
				.AddDays(1)
				.Date;

			try {
				MP_EkmOrder o = customerMarketPlace.EkmOrders.OrderBy(x => x.Id).AsQueryable().Last();
				return o == null ? def : o.Created.AddMonths(-1);
			} catch (Exception) {
				return def;
			}
		}

		// FindCustomerByEmail
		public IEnumerable<IDatabaseCustomerMarketPlace> GetEnabledCustomerMarketPlaceList(Customer customer, IMarketplaceType databaseMarketplace) {
			MP_MarketplaceType marketplaceType = _MarketPlaceRepository.Get(databaseMarketplace.InternalId);

			var data = _CustomerMarketplaceRepository.Get(customer, marketplaceType);

			return
				data.Select(cm => CreateDatabaseCustomerMarketPlace(customer, databaseMarketplace, cm, cm.Id))
					.Where(mp => !mp.Disabled)
					.ToList();
		}

		public MP_CustomerMarketPlace GetExistsCustomerMarketPlace(string marketPlaceName, IMarketplaceType marketplaceType, Customer customer) {
			return _CustomerMarketplaceRepository.Get(customer.Id, marketplaceType.InternalId, marketPlaceName);
		}

		public DateTime? GetFreeAgentExpenseDeltaPeriod(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace) {
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace.Id);

			MP_FreeAgentRequest request = customerMarketPlace.FreeAgentRequests.OrderBy(x => x.Id).AsQueryable().LastOrDefault();
			if (request == null) {
				return null;
			}

			MP_FreeAgentExpense item = request.Expenses.OrderBy(x => x.dated_on).AsQueryable().LastOrDefault();
			DateTime? latestExistingDate = item != null ? item.dated_on : request.Created;
			return latestExistingDate.HasValue ? latestExistingDate.Value.AddMonths(-1) : DateTime.UtcNow.AddYears(-1);
		}

		public int GetFreeAgentInvoiceDeltaPeriod(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace) {
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace.Id);

			MP_FreeAgentRequest order = customerMarketPlace.FreeAgentRequests.OrderBy(x => x.Id).AsQueryable().LastOrDefault();
			if (order == null) {
				return -1;
			}

			MP_FreeAgentInvoice item = order.Invoices.OrderBy(x => x.dated_on).AsQueryable().LastOrDefault();
			DateTime latestExistingDate = item != null && item.dated_on.HasValue ? item.dated_on.Value : order.Created;

			DateTime later = DateTime.UtcNow;
			int monthDiff = 1;
			while (later > latestExistingDate) {
				later = later.AddMonths(-1);
				monthDiff++;
			}

			if (monthDiff == 0) {
				monthDiff = -1;
			}

			return monthDiff;
		}

		public MP_CustomerMarketplaceUpdatingHistory GetHistoryItem(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace) {
			MP_CustomerMarketplaceUpdatingHistory historyItem =
				customerMarketPlaceUpdatingHistoryRepository.GetByMarketplaceId(databaseCustomerMarketPlace.Id).FirstOrDefault();

			_Log.DebugFormat(
				"History item for customer market place id {0} is {1}.",
				databaseCustomerMarketPlace.Id,
				historyItem == null ? "NULL" : "not null"
			);

			return historyItem;
		}

		public string GetPayPointDeltaPeriod(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace) {
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace.Id);

			DateTime dThen = DateTime.Today.AddYears(-1);

			try {
				MP_PayPointOrder ppo = customerMarketPlace.PayPointOrders.OrderBy(x => x.Id).AsQueryable().LastOrDefault();

				if (ppo != null) {
					MP_PayPointOrderItem item = ppo.OrderItems.OrderBy(x => x.date).AsQueryable().LastOrDefault();
					if (item != null && item.date.HasValue) {
						dThen = item.date.Value.AddMonths(-1);
					} else {
						dThen = ppo.Created.AddMonths(-1);
					}
				}
			} catch (Exception) {
				// so what? ignored.
			}

			return
				dThen.Year +
				dThen.Month.ToString("00") + "-" +
				DateTime.Today.Year +
				DateTime.Today.Month.ToString("00");
		}

		public DateTime? GetSageDeltaPeriod(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace) {
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace.Id);

			MP_SageRequest request = customerMarketPlace.SageRequests.OrderBy(x => x.Id).AsQueryable().LastOrDefault();
			if (request == null) {
				return null;
			}

			MP_SageSalesInvoice lastSalesInvoice = request.SalesInvoices.OrderBy(x => x.date).AsQueryable().LastOrDefault();
			MP_SageIncome lastIncome = request.Incomes.OrderBy(x => x.date).AsQueryable().LastOrDefault();
			MP_SagePurchaseInvoice lastPurchaseInvoice = request.PurchaseInvoices.OrderBy(x => x.date).AsQueryable().LastOrDefault();
			MP_SageExpenditure lastExpenditure = request.Expenditures.OrderBy(x => x.date).AsQueryable().LastOrDefault();

			DateTime latestDate = request.Created;
			if (lastSalesInvoice != null && lastSalesInvoice.date.HasValue && lastSalesInvoice.date > latestDate) {
				latestDate = lastSalesInvoice.date.Value;
			}
			if (lastPurchaseInvoice != null && lastPurchaseInvoice.date.HasValue && lastPurchaseInvoice.date > latestDate) {
				latestDate = lastPurchaseInvoice.date.Value;
			}
			if (lastIncome != null && lastIncome.date.HasValue && lastIncome.date > latestDate) {
				latestDate = lastIncome.date.Value;
			}
			if (lastExpenditure != null && lastExpenditure.date.HasValue && lastExpenditure.date > latestDate) {
				latestDate = lastExpenditure.date.Value;
			}

			return latestDate.AddMonths(-1);
		}

		public List<MP_SagePaymentStatus> GetSagePaymentStatuses() {
			var result = new List<MP_SagePaymentStatus>();
			result.AddRange(_SagePaymentStatusRepository.GetAll());
			return result;
		}

		public void InitFunctionTimePeriod() {
			foreach (var timePeriod in TimePeriodBase.AllTimePeriods) {
				var period = _AnalysisFunctionTimePeriodRepository.Get(timePeriod.InternalId) ?? new MP_AnalysisFunctionTimePeriod { InternalId = timePeriod.InternalId };

				period.Name = timePeriod.Name;
				period.Description = timePeriod.Description;

				_AnalysisFunctionTimePeriodRepository.SaveOrUpdate(period);
			}
		}

		//public CustomerLoyaltyProgramPointsRepository CustomerLoyaltyPoints { get { Session.Evict(_CustomerLoyaltyPoints); return _CustomerLoyaltyPoints; } }

		public void StoreEkmOrdersData(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, EkmOrdersList ordersData, MP_CustomerMarketplaceUpdatingHistory historyRecord) {
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace.Id);

			LogData("Orders Data", customerMarketPlace, ordersData);

			if (ordersData == null) {
				return;
			}

			DateTime submittedDate = DateTime.UtcNow;

			var mpOrder = new MP_EkmOrder {
				CustomerMarketPlace = customerMarketPlace,
				Created = submittedDate,
				HistoryRecord = historyRecord
			};

			ordersData.ForEach(
				dataItem => {
					var mpOrderItem = new MP_EkmOrderItem {
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

		public void StoreFreeAgentCompanyData(MP_FreeAgentCompany company) {
			_FreeAgentCompanyRepository.SaveOrUpdate(company);
		}

		public void UpdateMarketplaceDisplayName(MP_CustomerMarketPlace customerMarketPlace, string newName) {
			customerMarketPlace.DisplayName = newName;
			_CustomerMarketplaceRepository.Update(customerMarketPlace);
		} // UpdateMarketplaceDisplayName

		public MP_FreeAgentRequest StoreFreeAgentRequestAndInvoicesAndExpensesData(
			IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
			FreeAgentInvoicesList invoices,
			FreeAgentExpensesList expenses,
			MP_CustomerMarketplaceUpdatingHistory historyRecord
		) {
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace.Id);

			LogData("Invoices Data", customerMarketPlace, invoices);
			LogData("Expenses Data", customerMarketPlace, expenses);

			DateTime submittedDate = DateTime.UtcNow;
			var mpRequest = new MP_FreeAgentRequest {
				CustomerMarketPlace = customerMarketPlace,
				Created = submittedDate,
				HistoryRecord = historyRecord
			};

			invoices.ForEach(
				dataItem => {
					var invoice = new MP_FreeAgentInvoice {
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
					foreach (var item in dataItem.invoice_items) {
						var mpItem = new MP_FreeAgentInvoiceItem {
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
				dataItem => {
					var expense = new MP_FreeAgentExpense {
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

					if (dataItem.attachment != null) {
						expense.attachment_url = dataItem.attachment.url;
						expense.attachment_content_src = dataItem.attachment.content_src;
						expense.attachment_content_type = dataItem.attachment.content_type;
						expense.attachment_file_name = dataItem.attachment.file_name;
						expense.attachment_file_size = dataItem.attachment.file_size;
						expense.attachment_description = dataItem.attachment.description;
					} // if

					expense.Category = _FreeAgentExpenseCategoryRepository.Get(dataItem.categoryItem.Id);

					mpRequest.Expenses.Add(expense);
				});

			customerMarketPlace.FreeAgentRequests.Add(mpRequest);
			_CustomerMarketplaceRepository.Update(customerMarketPlace);

			return mpRequest;
		}

		public void StoreFreeAgentUsersData(List<MP_FreeAgentUsers> users) {
			foreach (MP_FreeAgentUsers user in users) {
				_FreeAgentUsersRepository.SaveOrUpdate(user);
			}
		}

		public void StorePayPointOrdersData(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, PayPointOrdersList ordersData, MP_CustomerMarketplaceUpdatingHistory historyRecord) {
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace.Id);

			LogData("PayPoint Orders Data", customerMarketPlace, ordersData);

			if (ordersData == null) {
				return;
			}

			DateTime submittedDate = DateTime.UtcNow;
			var mpOrder = new MP_PayPointOrder {
				CustomerMarketPlace = customerMarketPlace,
				Created = submittedDate,
				HistoryRecord = historyRecord
			};

			ordersData.ForEach(
				dataItem => {
					var mpOrderItem = new MP_PayPointOrderItem {
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

		public void StoreSageData(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, SageSalesInvoicesList salesInvoices, SagePurchaseInvoicesList purchaseInvoices, SageIncomesList incomes, SageExpendituresList expenditures, MP_CustomerMarketplaceUpdatingHistory historyRecord) {
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace.Id);

			LogData("SalesInvoices Data", customerMarketPlace, salesInvoices);
			LogData("PurchaseInvoices Data", customerMarketPlace, purchaseInvoices);
			LogData("Incomes Data", customerMarketPlace, incomes);
			LogData("Expenditures Data", customerMarketPlace, expenditures);

			DateTime submittedDate = DateTime.UtcNow;
			var mpRequest = new MP_SageRequest {
				CustomerMarketPlace = customerMarketPlace,
				Created = submittedDate,
				HistoryRecord = historyRecord
			};

			salesInvoices.ForEach(
				dataItem => {
					var salesInvoice = new MP_SageSalesInvoice {
						Request = mpRequest,
						SageId = dataItem.SageId,
						invoice_number = dataItem.invoice_number,
						StatusId = dataItem.status,
						due_date = dataItem.due_date,
						date = dataItem.date,
						void_reason = dataItem.void_reason,
						outstanding_amount = dataItem.outstanding_amount,
						total_net_amount = dataItem.total_net_amount,
						total_tax_amount = dataItem.total_tax_amount,
						tax_scheme_period_id = dataItem.tax_scheme_period_id,
						carriage = dataItem.carriage,
						CarriageTaxCodeId = dataItem.carriage_tax_code,
						carriage_tax_rate_percentage = dataItem.carriage_tax_rate_percentage,
						ContactId = dataItem.contact,
						contact_name = dataItem.contact_name,
						main_address = dataItem.main_address,
						delivery_address = dataItem.delivery_address,
						delivery_address_same_as_main = dataItem.delivery_address_same_as_main,
						reference = dataItem.reference,
						notes = dataItem.notes,
						terms_and_conditions = dataItem.terms_and_conditions,
						lock_version = dataItem.lock_version,
						Items = new HashedSet<MP_SageSalesInvoiceItem>()
					};

					foreach (var item in dataItem.line_items) {
						var mpItem = new MP_SageSalesInvoiceItem {
							Invoice = salesInvoice,
							description = item.description,
							quantity = item.quantity,
							unit_price = item.unit_price,
							net_amount = item.net_amount,
							tax_amount = item.tax_amount,
							TaxCodeId = item.tax_code,
							tax_rate_percentage = item.tax_rate_percentage,
							unit_price_includes_tax = item.unit_price_includes_tax,
							LedgerAccountId = item.ledger_account,
							product_code = item.product_code,
							ProductId = item.product,
							ServiceId = item.service,
							lock_version = item.lock_version
						};
						salesInvoice.Items.Add(mpItem);
					}

					mpRequest.SalesInvoices.Add(salesInvoice);
				});

			purchaseInvoices.ForEach(
				dataItem => {
					var purchaseInvoice = new MP_SagePurchaseInvoice {
						Request = mpRequest,
						SageId = dataItem.SageId,
						StatusId = dataItem.status,
						due_date = dataItem.due_date,
						date = dataItem.date,
						void_reason = dataItem.void_reason,
						outstanding_amount = dataItem.outstanding_amount,
						total_net_amount = dataItem.total_net_amount,
						total_tax_amount = dataItem.total_tax_amount,
						tax_scheme_period_id = dataItem.tax_scheme_period_id,
						ContactId = dataItem.contact,
						contact_name = dataItem.contact_name,
						main_address = dataItem.main_address,
						delivery_address = dataItem.delivery_address,
						delivery_address_same_as_main = dataItem.delivery_address_same_as_main,
						reference = dataItem.reference,
						notes = dataItem.notes,
						terms_and_conditions = dataItem.terms_and_conditions,
						lock_version = dataItem.lock_version
					};

					purchaseInvoice.Items = new HashedSet<MP_SagePurchaseInvoiceItem>();
					foreach (var item in dataItem.line_items) {
						var mpItem = new MP_SagePurchaseInvoiceItem {
							PurchaseInvoice = purchaseInvoice,
							description = item.description,
							quantity = item.quantity,
							unit_price = item.unit_price,
							net_amount = item.net_amount,
							tax_amount = item.tax_amount,
							TaxCodeId = item.tax_code,
							tax_rate_percentage = item.tax_rate_percentage,
							unit_price_includes_tax = item.unit_price_includes_tax,
							LedgerAccountId = item.ledger_account,
							product_code = item.product_code,
							ProductId = item.product,
							ServiceId = item.service,
							lock_version = item.lock_version
						};
						purchaseInvoice.Items.Add(mpItem);
					}

					mpRequest.PurchaseInvoices.Add(purchaseInvoice);
				});

			incomes.ForEach(
				dataItem => {
					var income = new MP_SageIncome {
						Request = mpRequest,
						SageId = dataItem.SageId,
						date = dataItem.date,
						invoice_date = dataItem.invoice_date,
						amount = dataItem.amount,
						tax_amount = dataItem.tax_amount,
						gross_amount = dataItem.gross_amount,
						tax_percentage_rate = dataItem.tax_percentage_rate,
						TaxCodeId = dataItem.tax_code,
						tax_scheme_period_id = dataItem.tax_scheme_period_id,
						reference = dataItem.reference,
						ContactId = dataItem.contact,
						SourceId = dataItem.source,
						DestinationId = dataItem.destination,
						voided = dataItem.voided,
						lock_version = dataItem.lock_version
					};

					mpRequest.Incomes.Add(income);
				});

			expenditures.ForEach(
				dataItem => {
					var expenditure = new MP_SageExpenditure {
						Request = mpRequest,
						SageId = dataItem.SageId,
						date = dataItem.date,
						invoice_date = dataItem.invoice_date,
						amount = dataItem.amount,
						tax_amount = dataItem.tax_amount,
						gross_amount = dataItem.gross_amount,
						tax_percentage_rate = dataItem.tax_percentage_rate,
						TaxCodeId = dataItem.tax_code,
						tax_scheme_period_id = dataItem.tax_scheme_period_id,
						reference = dataItem.reference,
						ContactId = dataItem.contact,
						SourceId = dataItem.source,
						DestinationId = dataItem.destination,
						voided = dataItem.voided,
						lock_version = dataItem.lock_version
					};

					mpRequest.Expenditures.Add(expenditure);
				});

			customerMarketPlace.SageRequests.Add(mpRequest);
			_CustomerMarketplaceRepository.Update(customerMarketPlace);
		}

		public void StoreSagePaymentStatuses(List<SagePaymentStatus> paymentStatuses) {
			foreach (SagePaymentStatus paymentStatus in paymentStatuses) {
				MP_SagePaymentStatus dbStatus = _SagePaymentStatusRepository.GetAll().FirstOrDefault(a => a.SageId == paymentStatus.SageId);
				if (dbStatus == null) {
					dbStatus = new MP_SagePaymentStatus { SageId = paymentStatus.SageId, name = paymentStatus.name };
				}
				_SagePaymentStatusRepository.SaveOrUpdate(dbStatus);
			}
		}

		public void WriteToLog(string message, WriteLogType messageType = WriteLogType.Info, Exception ex = null) {
			WriteLoggerHelper.Write(message, messageType, null, ex);
			Debug.WriteLine(message);
		}

		internal void UpdateCustomerMarketplaceData(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, Action<MP_CustomerMarketplaceUpdatingHistory> a) {
			MP_CustomerMarketplaceUpdatingHistory historyItem = GetHistoryItem(databaseCustomerMarketPlace);

			try {
				a(historyItem);
			} catch (Exception e) {
				throw new MarketplaceException(databaseCustomerMarketPlace, e);
			}
		}

		private class AnalysisFunctionData {
			public DateTime afDate { get; private set; }

			public Guid fid { get; private set; }
			public Guid fpid { get; private set; }
			public string value { get; private set; }
			public AnalysisFunctionData(object[] val) {
				fid = (Guid)val[0];
				fpid = (Guid)val[1];
				value = (string)val[2];
				afDate = (DateTime)val[3];
			} // constructor
		}

		private MP_AnalysisFunctionTimePeriod GetTimePeriod(ITimePeriod databaseTimePeriod) {
			return _AnalysisFunctionTimePeriodRepository.Get(databaseTimePeriod.InternalId);
		}

		// WizardStepSequence
		private void LogData(string funcName, MP_CustomerMarketPlace customerMarketPlace, IReceivedDataList data) {
			if (data == null) {
				WriteToLog(string.Format("{0} - {1}. Customer {2}, Market Place {3}: Invalid Call", funcName, customerMarketPlace.Marketplace.Name, customerMarketPlace.Customer.Name, customerMarketPlace.DisplayName), WriteLogType.Error);
				return;
			}

			if (data.Count == 0) {
				WriteToLog(string.Format("{0} - {1} - Request date: {2}. Customer {3}, Market Place {4}: NO Data", funcName, customerMarketPlace.Marketplace.Name, data.SubmittedDate, customerMarketPlace.Customer.Name, customerMarketPlace.DisplayName));
			} else {
				WriteToLog(string.Format("{0} - {1} - Request date: {2}.Customer {3}, Market Place {4}: Received {5} records", funcName, customerMarketPlace.Marketplace.Name, data.SubmittedDate, customerMarketPlace.Customer.Name, customerMarketPlace.DisplayName, data.Count));
			}
		}

		private void LogDataEnd(CustomerMarketplaceUpdateActionType updateActionType, IDatabaseCustomerMarketPlace databaseCustomerMarketPlace) {
			WriteToLog(string.Format("MP = {1} Action = {0} UMI = {2} ended!", updateActionType, databaseCustomerMarketPlace.Marketplace.DisplayName, databaseCustomerMarketPlace.Id));
		}

		private void LogDataException(CustomerMarketplaceUpdateActionType updateActionType, IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, string text) {
			WriteToLog(string.Format("MP = {1} Action = {0} UMI = {2} Text: {3}\n", updateActionType, databaseCustomerMarketPlace.Marketplace.DisplayName, databaseCustomerMarketPlace.Id, text), WriteLogType.Error);
		}

		private void LogDataException(CustomerMarketplaceUpdateActionType updateActionType, IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, Exception exception) {
			WriteToLog(string.Format("MP = {1} Action = {0} UMI = {2} Exception\n", updateActionType, databaseCustomerMarketPlace.Marketplace.DisplayName, databaseCustomerMarketPlace.Id), WriteLogType.Error, exception);
		}

		private void LogDataStart(CustomerMarketplaceUpdateActionType updateActionType, IDatabaseCustomerMarketPlace databaseCustomerMarketPlace) {
			WriteToLog(string.Format("MP = {1} Action = {0} UMI = {2} starting...", updateActionType, databaseCustomerMarketPlace.Marketplace.DisplayName, databaseCustomerMarketPlace.Id));
		}

		private void StoreCustomerMarketplaceUpdateActionDataEnd(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, MP_CustomerMarketplaceUpdatingHistory historyRecord, DatabaseCustomerMarketplaceUpdateActionData actionData, MP_CustomerMarketplaceUpdatingActionLog actionLog) {
			actionLog.UpdatingEnd = actionData.UpdatingEnd;
			actionLog.ControlValue = actionData.ControlValue == null ? null : actionData.ControlValue.ToString();
			actionLog.ControlValueName = !actionData.ControlValueName.HasValue ? null : actionData.ControlValueName.Value.ToString();
			actionLog.Error = actionData.Error;
			actionLog.ElapsedTime = new DatabaseElapsedTimeInfo(actionData.ElapsedTime);

			if (actionData.RequestsCounter != null && actionData.RequestsCounter.Any()) {
				actionData.RequestsCounter.ForEach(r => actionLog.RequestsCounter.Add(new MP_CustomerMarketplaceUpdatingCounter {
					Action = actionLog,
					Created = r.Created,
					Details = r.Details,
					Method = r.Method
				}));
			}

			historyRecord.ActionLog.Add(actionLog);

			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace.Id);

			_CustomerMarketplaceRepository.Update(customerMarketPlace);
		}

		private void StoreCustomerMarketplaceUpdateActionDataStart(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, MP_CustomerMarketplaceUpdatingHistory historyRecord, DatabaseCustomerMarketplaceUpdateActionData actionData, out MP_CustomerMarketplaceUpdatingActionLog actionLog) {
			actionLog = new MP_CustomerMarketplaceUpdatingActionLog {
				HistoryRecord = historyRecord,
				ActionName = actionData.ActionName.ToString(),
				UpdatingStart = actionData.UpdatingStart,
			};

			historyRecord.ActionLog.Add(actionLog);

			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace.Id);

			_CustomerMarketplaceRepository.Update(customerMarketPlace);
		}

		private static readonly ILog _Log = LogManager.GetLogger(typeof(DatabaseDataHelper));

		private readonly AnalysisFunctionTimePeriodRepository _AnalysisFunctionTimePeriodRepository;
		private readonly BrokerRepository _brokerRepository;
		private readonly ICurrencyConvertor _CurrencyConvertor;
		private readonly ICurrencyRateRepository _CurrencyRateRepository;
		private readonly CustomerMarketPlaceRepository _CustomerMarketplaceRepository;
		private readonly CustomerRepository _CustomerRepository;
		private readonly MP_FreeAgentCompanyRepository _FreeAgentCompanyRepository;
		private readonly MP_FreeAgentExpenseCategoryRepository _FreeAgentExpenseCategoryRepository;
		private readonly MP_FreeAgentUsersRepository _FreeAgentUsersRepository;
		private readonly LoanTransactionMethodRepository _loanTransactionMethodRepository;
		private readonly ILoanTypeRepository _LoanTypeRepository;
		private readonly MarketPlaceRepository _MarketPlaceRepository;
		private readonly MP_SagePaymentStatusRepository _SagePaymentStatusRepository;
		private readonly TrustPilotStatusRepository _trustPilotStatusRepository;
		private readonly WizardStepRepository _wizardStepRepository;
		private readonly WizardStepSequenceRepository _wizardStepSequenceRepository;
		private readonly CustomerMarketPlaceUpdatingHistoryRepository customerMarketPlaceUpdatingHistoryRepository;
		private readonly ExperianDirectorRepository m_oExperianDirectorRepository;
		private readonly ISession _session;
	} // class DatabaseDataHelper
} // namespace
