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
		public DateTime? HmrcUpdateTime { get; private set; }
		public DateTime? OnlineUpdateTime { get; private set; }

		public bool HasHmrc { get; private set; }
		public bool HasOnline { get; private set; }

		public CalculatedTurnover() {
			HasHmrc = false;
			HasOnline = false;

			HmrcUpdateTime = null;
			OnlineUpdateTime = null;

			m_oOnline = new SortedDictionary<int, OneValue>();
			m_oHmrc = new SortedDictionary<int, decimal>();
		} // constructor

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

			if (r.MpTypeInternalID == Hmrc) {
				HasHmrc = true;

				m_oHmrc[r.MonthCount] = r.Turnover;

				if (r.LastUpdateTime != null) {
					if (HmrcUpdateTime == null)
						HmrcUpdateTime = r.LastUpdateTime;
					else if (r.LastUpdateTime.Value < HmrcUpdateTime.Value)
						HmrcUpdateTime = r.LastUpdateTime;
				} // if
			}
			else {
				HasOnline = true;

				if (r.LastUpdateTime != null) {
					if (OnlineUpdateTime == null)
						OnlineUpdateTime = r.LastUpdateTime;
					else if (r.LastUpdateTime.Value < OnlineUpdateTime.Value)
						OnlineUpdateTime = r.LastUpdateTime;
				} // if

				if (m_oOnline.ContainsKey(r.MonthCount))
					m_oOnline[r.MonthCount].Add(r);
				else
					m_oOnline[r.MonthCount] = new OneValue(r);
			} // if
		} // Add

		public decimal GetOnline(int nMonthCount) {
			return m_oOnline.ContainsKey(nMonthCount) ? m_oOnline[nMonthCount].Value : 0;
		} // indexer

		public decimal GetHmrc(int nMonthCount) {
			return m_oHmrc.ContainsKey(nMonthCount) ? m_oHmrc[nMonthCount] : 0;
		} // indexer

		/// <summary>
		/// Input turnover data for one marketplace and one period.
		/// Requested period length is <see cref="MonthCount"/>.
		/// Actual period is between <see cref="DateFrom"/> and <see cref="DateTo"/> inclusive.
		/// </summary>
		public class Row {
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

			/// <summary>
			/// Time when last marketplace update has completed.
			/// </summary>
			public DateTime? LastUpdateTime { get; set; }

			/// <summary>
			/// Returns 'true' if this row should be included in total period calculation.
			/// </summary>
			public bool IsTotal {
				get { return TurnoverType == Total; } // get
			} // IsTotal

			/// <summary>
			/// Writes this instance to log.
			/// </summary>
			/// <param name="oLog"></param>
			public void WriteToLog(ASafeLog oLog) {
				if (oLog == null)
					return;

				oLog.Debug(
					"Turnover for customer marketplace (last updated on '{9}'):\n" +
					"\tmarketplace: id {1}, type {2}\n" +
					"\t{0} (because of type {4})\n" +
					"\tturnover: {3}, month count: {5}, day count: {6}\n" +
					"\tdata from: {7} to {8} inclusive",
					IsTotal ? "Accepted" : "Ignored",
					MpID,
					MpTypeInternalID,
					Turnover,
					TurnoverType,
					MonthCount,
					DayCount,
					DateFrom.ToString("d/MMM/yyyy", CultureInfo.InvariantCulture),
					DateTo.ToString("d/MMM/yyyy", CultureInfo.InvariantCulture),
					LastUpdateTime == null ? "never" : LastUpdateTime.Value.ToString("d/MMM/yyyy", CultureInfo.InvariantCulture)
				);
			} // WriteToLog

			private const string Total = "Total";
		} // Row

		private class OneValue {
			public OneValue(Row r) {
				m_nEbay = 0;
				m_nOther = 0;
				m_nPayPal = 0;

				Add(r);
			} // constructor

			public void Add(Row r) {
				if (r.MpTypeInternalID == Ebay)
					m_nEbay += r.Turnover;
				else if (r.MpTypeInternalID == PayPal)
					m_nPayPal += r.Turnover;
				else
					m_nOther += r.Turnover;
			} // Add

			public decimal Value {
				get { return Math.Max(m_nEbay, m_nPayPal) + m_nOther; } // get
			} // Value

			private decimal m_nEbay;
			private decimal m_nPayPal;
			private decimal m_nOther;

			private static readonly Guid Ebay   = new Guid("A7120CB7-4C93-459B-9901-0E95E7281B59");
			private static readonly Guid PayPal = new Guid("3FA5E327-FCFD-483B-BA5A-DC1815747A28");
		} // OneValue

		private readonly SortedDictionary<int, OneValue> m_oOnline;
		private readonly SortedDictionary<int, decimal> m_oHmrc;

		private static readonly Guid Hmrc = new Guid("AE85D6FC-DBDB-4E01-839A-D5BD055CBAEA");
	} // class CalculatedTurnover
} // namespace
