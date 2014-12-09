namespace EZBob.DatabaseLib {
	using System;
	using System.Linq;
	using DatabaseWrapper;
	using DatabaseWrapper.Order;
	using Model.Database;
	using NHibernate.Linq;

	public partial class DatabaseDataHelper {

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

	} // class DatabaseDataHelper
} // namespace EZBob.DatabaseLib
