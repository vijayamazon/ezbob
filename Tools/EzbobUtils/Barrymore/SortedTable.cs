using System;
using System.Collections.Generic;

namespace Ezbob.Utils {
	#region class SortedTable

	public class SortedTable<TRowKey, TColumnKey, TData> {
		#region public

		#region constructor

		public SortedTable() {
			m_oData = new SortedDictionary<TRowKey, SortedDictionary<TColumnKey, TData>>();
		} // constructor

		#endregion constructor

		#region method Contains

		public bool Contains(TRowKey oRowKey, TColumnKey oColumnKey) {
			if (m_oData.ContainsKey(oRowKey)) {
				SortedDictionary<TColumnKey, TData> oRow = m_oData[oRowKey];

				return oRow.ContainsKey(oColumnKey);
			} // if

			return false;
		} // Contains

		#endregion method Contains

		#region indexer

		public TData this[TRowKey oRowKey, TColumnKey oColumnKey] {
			get {
				if (m_oData.ContainsKey(oRowKey)) {
					SortedDictionary<TColumnKey, TData> oRow = m_oData[oRowKey];

					if (oRow.ContainsKey(oColumnKey))
						return oRow[oColumnKey];
				} // if

				throw new KeyNotFoundException("Cannot find element by requested keys");
			} // get
			set {
				SortedDictionary<TColumnKey, TData> oRow;

				if (m_oData.ContainsKey(oRowKey))
					oRow = m_oData[oRowKey];
				else {
					oRow = new SortedDictionary<TColumnKey, TData>();
					m_oData[oRowKey] = oRow;
				} // if

				oRow[oColumnKey] = value;
			} // set
		} // indexer

		#endregion indexer

		#region method ForEach

		public void ForEach(Action<TRowKey, TColumnKey, TData> oAction) {
			if (ReferenceEquals(oAction, null))
				throw new ArgumentNullException("oAction", "Action not specified.");

			foreach (var pair in m_oData)
				foreach (var raip in pair.Value)
					oAction(pair.Key, raip.Key, raip.Value);
		} // ForEach

		public void ForEachRow(Action<TRowKey, SortedDictionary<TColumnKey, TData>> oAction) {
			if (ReferenceEquals(oAction, null))
				throw new ArgumentNullException("oAction", "Action not specified.");

			foreach (var pair in m_oData)
				oAction(pair.Key, pair.Value);
		} // ForEach

		#endregion method ForEach

		#endregion public

		#region private

		private SortedDictionary<TRowKey, SortedDictionary<TColumnKey, TData>> m_oData;

		#endregion private
	} // class SortedTable

	#endregion class SortedTable
} // namespace Ezbob.Utils
