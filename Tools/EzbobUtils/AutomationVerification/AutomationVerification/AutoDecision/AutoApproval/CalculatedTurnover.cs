namespace AutomationCalculator.AutoDecision.AutoApproval {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	/// <summary>
	/// Contains customer turnover based on all customer marketplaces.
	/// Turnover data is fed via Add method.
	/// </summary>
	public class CalculatedTurnover {
		#region public

		#region constructor

		public CalculatedTurnover() {
			m_oData = new SortedDictionary<int, OneValue>();
		} // constructor

		#endregion constructor

		#region method Add

		/// <summary>
		/// Feeds turnover for one marketplace and one period.
		/// </summary>
		/// <param name="sr">Turnover data. See <see cref="Row"/> for row format.</param>
		/// <param name="oLog">Log, to write turnover data for debugging.</param>
		public void Add(SafeReader sr, ASafeLog oLog) {
			Row r = sr.Fill<Row>();

			r.WriteToLog(oLog);

			if (!r.IsTotal)
				return;

			if (m_oData.ContainsKey(r.MonthCount))
				m_oData[r.MonthCount].Add(r);
			else
				m_oData[r.MonthCount] = new OneValue(r);
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

		#region class Row

		/// <summary>
		/// Input turnover data for one marketplace and one period.
		/// Requested period length is <see cref="MonthCount"/>.
		/// Actual period is between <see cref="DateFrom"/> and <see cref="DateTo"/> inclusive.
		/// </summary>
		public class Row {
			#region public

			/// <summary>
			/// Marketplace ID (MP_CustomerMarketplace.Id).
			/// </summary>
			[UsedImplicitly]
			public int MpID { get; set; }

			/// <summary>
			/// Marketplace type (MP_MarketplaceType.InternalId).
			/// </summary>
			[UsedImplicitly]
			public Guid MpTypeInternalID { get; set; }

			/// <summary>
			/// Turnover type, usually is 'Total'.
			/// For eBay there can be other values which describe the 'Total' parts.
			/// Only 'Total' is used for turnover calculation, others are for debug purposes.
			/// </summary>
			[UsedImplicitly]
			public string TurnoverType { get; set; }

			/// <summary>
			/// Actual turnover value.
			/// </summary>
			[UsedImplicitly]
			public decimal Turnover { get; set; }

			/// <summary>
			/// Annualized turnover value, calculated as <see cref="Turnover "/> / <see cref="DayCount"/> * 365.
			/// </summary>
			[UsedImplicitly]
			public decimal Annualized { get; set; }

			/// <summary>
			/// Requested period length in months.
			/// </summary>
			[UsedImplicitly]
			public int MonthCount { get; set; }

			/// <summary>
			/// Actual number of days having transactions.
			/// </summary>
			[UsedImplicitly]
			public int DayCount { get; set; }

			/// <summary>
			/// The first day having a transaction in this period.
			/// </summary>
			[UsedImplicitly]
			public DateTime DateFrom { get; set; }

			/// <summary>
			/// The last day having a transaction in this period.
			/// </summary>
			[UsedImplicitly]
			public DateTime DateTo { get; set; }

			#region property IsTotal

			/// <summary>
			/// Returns 'true' if this row should be included in total period calculation.
			/// </summary>
			public bool IsTotal {
				get { return TurnoverType == Total; } // get
			} // IsTotal

			#endregion property IsTotal

			#region method WriteToLog

			/// <summary>
			/// Writes this instance to log.
			/// </summary>
			/// <param name="oLog"></param>
			public void WriteToLog(ASafeLog oLog) {
				if (oLog == null)
					return;

				oLog.Debug(
					"Turnover for customer marketplace:\n" +
					"\tmarketplace: id {1}, type {2}\n" +
					"\t{0} (because of type {5})\n" +
					"\tturnover: {3}, annualized: {4}\n" +
					"\tmonth count: {6}, day count: {7}\n" +
					"\tdata from: {8} to {9} inclusive",
					IsTotal ? "Accepted" : "Ignored",
					MpID,
					MpTypeInternalID,
					Turnover,
					Annualized,
					TurnoverType,
					MonthCount,
					DayCount,
					DateFrom.ToString("d/MMM/yyyy", CultureInfo.InvariantCulture),
					DateTo.ToString("d/MMM/yyyy", CultureInfo.InvariantCulture)
				);
			} // WriteToLog

			#endregion method WriteToLog

			#endregion public

			private const string Total = "Total";
		} // Row

		#endregion class Row

		#endregion public

		#region private

		#region class OneValue

		private class OneValue {
			#region public

			#region constructor

			public OneValue(Row r) {
				m_nEbay = 0;
				m_nOther = 0;
				m_nPayPal = 0;

				Add(r);
			} // constructor

			#endregion constructor

			#region method Add

			public void Add(Row r) {
				if (r.MpTypeInternalID == Ebay)
					m_nEbay += r.Turnover;
				else if (r.MpTypeInternalID == PayPal)
					m_nPayPal += r.Turnover;
				else
					m_nOther += r.Turnover;
			} // Add

			#endregion method Add

			#region property Value

			public decimal Value {
				get { return Math.Max(m_nEbay, m_nPayPal) + m_nOther; } // get
			} // Value

			#endregion property Value

			#endregion public

			#region private

			private decimal m_nEbay;
			private decimal m_nPayPal;
			private decimal m_nOther;

			private static readonly Guid Ebay   = new Guid("A7120CB7-4C93-459B-9901-0E95E7281B59");
			private static readonly Guid PayPal = new Guid("3FA5E327-FCFD-483B-BA5A-DC1815747A28");

			#endregion private
		} // OneValue

		#endregion class OneValue

		private readonly SortedDictionary<int, OneValue> m_oData;

		#endregion private
	} // class CalculatedTurnover
} // namespace
