﻿using System;
using System.Collections.Generic;

namespace Reports {
	internal class LoanStatsMarketplaces {
		#region public

		public static SortedDictionary<int, string> MarketplaceTypes { get; private set; }

		#region constructor

		public LoanStatsMarketplaces(int nMarketplaceTypeID, string sMarketplaceTypeName, DateTime oCreated) {
			if (MarketplaceTypes == null)
				MarketplaceTypes = new SortedDictionary<int, string>();

			m_oMarketplaces = new SortedDictionary<int, SortedSet<DateTime>>();

			Add(nMarketplaceTypeID, sMarketplaceTypeName, oCreated);
		} // constructor

		#endregion constructor

		#region method Add

		public void Add(int nMarketplaceTypeID, string sMarketplaceTypeName, DateTime oCreated) {
			if (!MarketplaceTypes.ContainsKey(nMarketplaceTypeID))
				MarketplaceTypes[nMarketplaceTypeID] = sMarketplaceTypeName;

			if (!m_oMarketplaces.ContainsKey(nMarketplaceTypeID))
				m_oMarketplaces[nMarketplaceTypeID] = new SortedSet<DateTime>();

			m_oMarketplaces[nMarketplaceTypeID].Add(oCreated);
		} // Add

		#endregion method Add

		#region method Count

		public static SortedDictionary<int, int> Count() {
			var oCount = new SortedDictionary<int, int>();

			foreach (KeyValuePair<int, string> pair in MarketplaceTypes)
				oCount[pair.Key] = 0;

			return oCount;
		} // Count

		public SortedDictionary<int, int> Count(DateTime oEdge) {
			var oResult = Count();

			foreach (KeyValuePair<int, SortedSet<DateTime>> pair in m_oMarketplaces) {
				int nMarketplaceID = pair.Key;

				foreach (DateTime dateTime in pair.Value) {
					if (dateTime <= oEdge)
						oResult[nMarketplaceID]++;
					else
						break;
				} // for each
			} // for each

			return oResult;
		} // Count

		#endregion method Count

		#endregion public

		#region private

		private readonly SortedDictionary<int, SortedSet<DateTime>> m_oMarketplaces;

		#endregion private
	} // class LoanStatsMarketplaces
} // namespace
