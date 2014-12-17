namespace EZBob.DatabaseLib.Model.Database {
	using System;
	using System.Linq;
	using EzBob.CommonLib.MarketplaceSpecificTypes.TeraPeakOrdersData;
	using NHibernate;
	using NHibernate.Linq;
	using StructureMap;

	public class TeraPeackHelper {
		public TeraPeackHelper() {
			_session = ObjectFactory.GetInstance<ISession>();
		}

		public void StoretoDatabaseTeraPeakOrdersData(MP_CustomerMarketPlace customerMarketPlace, TeraPeakDatabaseSellerData data, MP_CustomerMarketplaceUpdatingHistory historyRecord) {
			var order = new MP_TeraPeakOrder {
				CustomerMarketPlace = customerMarketPlace,
				Created = data.Submitted,
				HistoryRecord = historyRecord
			};

			DateTime? lastItemEndDate = null;

			if (data.Count > 0) {
				lastItemEndDate = data.Max(o => o.EndDate);
				data.ForEach(o => order.OrderItems.Add(CreateOrderItem(order, o)));
			}

			order.LastOrderItemEndDate = lastItemEndDate;
			customerMarketPlace.TeraPeakOrders.Add(order);
		}

		private MP_TeraPeakOrderItem CreateOrderItem(MP_TeraPeakOrder order, TeraPeakDatabaseSellerDataItem orderItem) {
			var mpTeraPeakOrderItem = new MP_TeraPeakOrderItem {
				Order = order,
				Bids = orderItem.Bids,
				ItemsOffered = orderItem.ItemsOffered,
				ItemsSold = orderItem.ItemsSold,
				Listings = orderItem.Listings,
				Revenue = orderItem.Revenue,
				SuccessRate = orderItem.SuccessRate,
				Successful = orderItem.Successful,
				AverageSellersPerDay = orderItem.AverageSellersPerDay,
				Transactions = orderItem.Transactions,
				StartDate = orderItem.StartDate,
				EndDate = orderItem.EndDate,
				RangeMarker = orderItem.RangeMarker
			};

			CreateCategoryStatistics(mpTeraPeakOrderItem, orderItem);

			return mpTeraPeakOrderItem;
		}

		private void CreateCategoryStatistics(MP_TeraPeakOrderItem mpTeraPeakOrderItem, TeraPeakDatabaseSellerDataItem orderItem) {
			foreach (var category in orderItem.Categories) {
				var tpCategory = GetOrCreateCategory(category.Category);
				var stat = new MP_TeraPeakCategoryStatistics() {
					Category = tpCategory,
					OrderItem = mpTeraPeakOrderItem,
					ItemsSold = category.ItemsSold,
					Listings = category.Listings,
					Revenue = category.Revenue,
					SuccessRate = category.SuccessRate,
					Successful = category.Successful
				};
				mpTeraPeakOrderItem.CategoryStatistics.Add(stat);
			}
		}

		private MP_TeraPeakCategory GetOrCreateCategory(TeraPeakCategory category) {
			var tpCategory = _session.Get<MP_TeraPeakCategory>(category.Id);

			if (tpCategory != null)
				return tpCategory;

			tpCategory = new MP_TeraPeakCategory {
				Id = category.Id,
				FullName = category.FullName,
				Name = category.Name,
				Level = category.Level,
				ParentCategoryID = category.ParentCategoryID
			};

			_session.Save(tpCategory);

			return tpCategory;
		}

		private ISession _session;
	}
}
