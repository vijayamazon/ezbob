namespace EZBob.DatabaseLib {
	#region using

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Common;
	using DatabaseWrapper;
	using DatabaseWrapper.Order;
	using Model.Database;
	using NHibernate.Linq;

	#endregion using

	public partial class DatabaseDataHelper {
		#region Channel Grabber flavour

		#region HMRC

		#region method StoreHmrcVatReturnData

		public void StoreHmrcVatReturnData(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, InternalDataList ordersData, MP_CustomerMarketplaceUpdatingHistory historyRecord, int nSourceID) {
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace.Id);

			LogData("HMRC VAT Return Data", customerMarketPlace, ordersData);

			if (ordersData == null)
				return;

			DateTime submittedDate = DateTime.UtcNow;

			ordersData.ForEach(ve => {
				var dataItem = (VatReturnEntry)ve;

				string sBizAddr = string.Join("\n", dataItem.BusinessAddress);

				Business biz = _businessRepository.GetAll().FirstOrDefault(b => (b.Name == dataItem.BusinessName) && (b.Address == sBizAddr));

				if (biz == null) {
					biz = new Business {
						Name = dataItem.BusinessName,
						Address = sBizAddr,
						RegistrationNo = dataItem.RegistrationNo, 
					};

					_businessRepository.SaveOrUpdate(biz);
				} // if

				var oRecord = new MP_VatReturnRecord {
					CustomerMarketPlace = customerMarketPlace,
					Created = submittedDate,
					HistoryRecord = historyRecord,
					Business = biz,
					DateDue = dataItem.DateDue,
					DateFrom = dataItem.DateFrom,
					DateTo = dataItem.DateTo,
					Period = dataItem.Period,
					RegistrationNo = dataItem.RegistrationNo,
					SourceID = nSourceID,
				};

				foreach (KeyValuePair<string, Coin> pair in dataItem.Data) {
					string sName = pair.Key;

					MP_VatReturnEntryName oVreName = _vatReturnEntryNameRepositry.GetAll().FirstOrDefault(n => n.Name == sName);

					if (oVreName == null) {
						oVreName = new MP_VatReturnEntryName {
							Name = sName
						};

						_vatReturnEntryNameRepositry.SaveOrUpdate(oVreName);
					} // if

					oRecord.Entries.Add(new MP_VatReturnEntry {
						Amount = pair.Value.Amount,
						CurrencyCode = pair.Value.CurrencyCode,
						Name = oVreName,
						Record = oRecord
					});
				} // for each box

				customerMarketPlace.VatReturnRecords.Add(oRecord);
			});

			_CustomerMarketplaceRepository.Update(customerMarketPlace);
		} // StoreHmrcVatReturnData

		#endregion method StoreHmrcVatReturnData

		#region method StoreHmrcRtiTaxMonthData

		public void StoreHmrcRtiTaxMonthData(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, InternalDataList ordersData, MP_CustomerMarketplaceUpdatingHistory historyRecord, int nSourceID) {
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace.Id);

			LogData("HMRC RTI Tax Months Data", customerMarketPlace, ordersData);

			if (ordersData == null)
				return;

			var oRecord = new MP_RtiTaxMonthRecord {
				CustomerMarketPlace = customerMarketPlace,
				Created = DateTime.UtcNow,
				HistoryRecord = historyRecord,
				SourceID = nSourceID,
			};

			ordersData.ForEach(vx => {
				var dataItem = (RtiTaxMonthEntry)vx;

				oRecord.Entries.Add(new MP_RtiTaxMonthEntry {
					DateStart = dataItem.DateStart,
					DateEnd = dataItem.DateEnd,
					AmountPaid = dataItem.AmountPaid.Amount,
					AmountDue = dataItem.AmountDue.Amount,
					CurrencyCode = dataItem.AmountPaid.CurrencyCode,
					Record = oRecord,
				});
			});

			customerMarketPlace.RtiTaxMonthRecords.Add(oRecord);
			_CustomerMarketplaceRepository.Update(customerMarketPlace);
		} // StoreHmrcRtiTaxMonthData

		#endregion method StoreHmrcRtiTaxMonthData

		#endregion HMRC

		#region Channel Grabber

		#region method GetAllChannelGrabberOrdersData

		public InternalDataList GetAllChannelGrabberOrdersData(DateTime submittedDate, IDatabaseCustomerMarketPlace databaseCustomerMarketPlace) {
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace.Id);

			var orders = new InternalDataList(submittedDate);

			orders.AddRange(customerMarketPlace.ChannelGrabberOrders.SelectMany(anOrder => anOrder.OrderItems).Select(o => new ChannelGrabberOrderItem {
				CurrencyCode = o.CurrencyCode,
				OrderStatus = o.OrderStatus,
				NativeOrderId = o.NativeOrderId,
				PaymentDate = o.PaymentDate,
				PurchaseDate = o.PurchaseDate,
				TotalCost = o.TotalCost,
				IsExpense = o.IsExpense
			}).Distinct(new InternalOrderComparer()));

			return orders;
		} // GetAllChannelGrabberOrdersData

		#endregion method GetAllChannelGrabberOrdersData

		#region method StoreChannelGrabberOrdersData

		public void StoreChannelGrabberOrdersData(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, InternalDataList ordersData, MP_CustomerMarketplaceUpdatingHistory historyRecord, int nIgnoredHere) {
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace.Id);

			LogData("ChannelGrabber Orders Data", customerMarketPlace, ordersData);

			if (ordersData == null)
				return;

			DateTime submittedDate = DateTime.UtcNow;
			var mpOrder = new MP_ChannelGrabberOrder {
				CustomerMarketPlace = customerMarketPlace,
				Created = submittedDate,
				HistoryRecord = historyRecord
			};

			ordersData.ForEach(di => {
				var dataItem = (ChannelGrabberOrderItem)di;

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

		#endregion method StoreChannelGrabberOrdersData

		#endregion Channel Grabber

		#endregion Channel Grabber flavour
	} // class DatabaseDataHelper
} // namespace EZBob.DatabaseLib
