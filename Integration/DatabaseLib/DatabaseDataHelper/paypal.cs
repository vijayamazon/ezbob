namespace EZBob.DatabaseLib {
	using System;
	using System.Collections.Generic;
	using Common;
	using DatabaseWrapper;
	using DatabaseWrapper.AccountInfo;
	using DatabaseWrapper.Transactions;
	using Model.Database;
	using NHibernate.Linq;

	public partial class DatabaseDataHelper {
		public PayPalTransactionsList GetAllPayPalTransactions(DateTime submittedDate, IDatabaseCustomerMarketPlace databaseCustomerMarketPlace) {
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace.Id);

			var data = new PayPalTransactionsList(submittedDate);

			customerMarketPlace.PayPalTransactions.ForEach(tr => tr.TransactionItems.ForEach(t => data.Add(new PayPalTransactionItem {
				Created = t.Created,
				Type = t.Type,
				FeeAmount = t.Currency == null ? null : _CurrencyConvertor.ConvertToBaseCurrency(t.Currency.Name, t.FeeAmount, t.Created),
				GrossAmount = t.Currency == null ? null : _CurrencyConvertor.ConvertToBaseCurrency(t.Currency.Name, t.GrossAmount, t.Created),
				NetAmount = t.Currency == null ? null : _CurrencyConvertor.ConvertToBaseCurrency(t.Currency.Name, t.NetAmount, t.Created),
				Status = t.Status,
				Timezone = t.TimeZone,
				TransactionId = t.PayPalTransactionId
			})));

			return data;

		}

		public MP_PayPalTransaction SavePayPalTransactionInfo(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, List<PayPalTransactionItem> data, MP_CustomerMarketplaceUpdatingHistory historyRecord, MP_PayPalTransaction mpTransaction) {
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace.Id);

			if (data == null) {
				return mpTransaction;
			}

			if (mpTransaction == null) {
				mpTransaction = new MP_PayPalTransaction {
					CustomerMarketPlace = customerMarketPlace,
					Created = DateTime.UtcNow,
					HistoryRecord = historyRecord
				};
			}

			if (data.Count != 0) {
				foreach (var dataItem in data) {
					var mpTransactionItem = new MP_PayPalTransactionItem2 {
						Transaction = mpTransaction,
						Created = dataItem.Created,
						Currency = _CurrencyRateRepository.GetCurrencyOrCreate("GBP"),
						FeeAmount =
							_CurrencyConvertor.ConvertToBaseCurrency(
								dataItem.FeeAmount ?? new AmountInfo { CurrencyCode = "GBP", Value = 0 }, dataItem.Created).Value,
						GrossAmount =
							_CurrencyConvertor.ConvertToBaseCurrency(
								dataItem.GrossAmount ?? new AmountInfo { CurrencyCode = "GBP", Value = 0 }, dataItem.Created).Value,
						NetAmount =
							_CurrencyConvertor.ConvertToBaseCurrency(
								dataItem.NetAmount ?? new AmountInfo { CurrencyCode = "GBP", Value = 0 }, dataItem.Created).Value,
						TimeZone = dataItem.Timezone,
						Status = dataItem.Status,
						Type = dataItem.Type,
						PayPalTransactionId = dataItem.TransactionId
					};

					mpTransaction.TransactionItems.Add(mpTransactionItem);
				}
			}

			customerMarketPlace.PayPalTransactions.Add(mpTransaction);
			_CustomerMarketplaceRepository.Update(customerMarketPlace);

			return mpTransaction;
		}

		public void SaveOrUpdateAcctountInfo(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, PayPalPersonalData data) {
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace.Id);

			if (customerMarketPlace.PersonalInfo == null) {
				customerMarketPlace.PersonalInfo = new MP_PayPalPersonalInfo {
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
			info.Phone = data.Phone == "0" ? null : data.Phone;
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

		public DateTime? GetLastPayPalTransactionRequest(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace) {
			return _CustomerMarketplaceRepository.GetLastPayPalTransactionRequest(databaseCustomerMarketPlace);
		}

		public DateTime? GetLastPayPalTransactionDate(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace) {
			return _CustomerMarketplaceRepository.GetLastPayPalTransactionDate(databaseCustomerMarketPlace);
		}
	}
}
