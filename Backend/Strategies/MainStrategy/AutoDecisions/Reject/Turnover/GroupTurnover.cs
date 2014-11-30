namespace EzBob.Backend.Strategies.MainStrategy.AutoDecisions.Reject.Turnover {
	using System.Collections.Generic;

	/// <summary>
	/// Contains customer turnover based on all customer marketplaces.
	/// Turnover data is fed via Add method.
	/// </summary>
	internal class GroupTurnover<T> where T: IPeriodValue, new() {
		#region public

		#region constructor

		public GroupTurnover() {
			m_oData = new SortedDictionary<int, IPeriodValue>();
		} // constructor

		#endregion constructor

		#region method Add

		/// <summary>
		/// Feeds turnover for one marketplace and one period.
		/// </summary>
		public void Add(Row r) {
			if (m_oData.ContainsKey(r.MonthCount))
				m_oData[r.MonthCount].Add(r);
			else {
				var v = new T();
				v.Add(r);
				m_oData[r.MonthCount] = v;
			}
		} // Add

		#endregion method Add

		#region indexer

		/// <summary>
		/// Gets customer turnover for specific period.
		/// </summary>
		/// <param name="nMonthCount">Number of months in the period.</param>
		/// <returns>Turnover for requested period.</returns>
		public decimal this[int nMonthCount] {
			get { return m_oData.ContainsKey(nMonthCount) ? m_oData[nMonthCount].Value : 0; } // get
		} // indexer

		#endregion indexer

		#endregion public

		#region private

		private readonly SortedDictionary<int, IPeriodValue> m_oData;

		#endregion private
	} // class GroupTurnover
} // namespace
