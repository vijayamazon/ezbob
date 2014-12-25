namespace EzBob.eBayLib {
	using System.Collections.Generic;
	using System.Linq;
	using EzBob.CommonLib.MarketplaceSpecificTypes.TeraPeakOrdersData;

	public class TopCategoriesAggregator {
		public string GetTopCategories(IEnumerable<TeraPeakDatabaseSellerDataItem> orders) {
			var categories = orders.Where(o => o.Categories != null).SelectMany(o => o.Categories);

			var dict = new Dictionary<int, CategoryAndListings>();

			foreach (var cat in categories) {
				if (!dict.ContainsKey(cat.Category.Id)) {
					dict[cat.Category.Id] = new CategoryAndListings(cat.Category, cat.Listings);
				} else {
					dict[cat.Category.Id].AddListings(cat.Listings);
				}
			}

			if (dict.Count == 0)
				return "";

			return string.Join(", ", dict.Values.OrderBy(cl => cl.Listings).Select(cl => cl.Category.Name));
		}

		private class CategoryAndListings {
			public TeraPeakCategory Category { get { return _category; } }

			public int Listings { get { return _listings; } }

			public CategoryAndListings(TeraPeakCategory category, int listings) {
				_category = category;
				_listings = listings;
			}

			public void AddListings(int listings) {
				_listings += listings;
			}

			private TeraPeakCategory _category;
			private int _listings;
		}
	}
}