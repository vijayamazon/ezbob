namespace AutomationCalculator.AutoDecision.AutoApproval {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	public class CalculatedTurnover {
		#region public

		#region constructor

		public CalculatedTurnover() {
			m_oData = new SortedDictionary<int, OneValue>();
		} // constructor

		#endregion constructor

		#region method Add

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

		public decimal this[int nMonthCount] {
			get { return m_oData.ContainsKey(nMonthCount) ? m_oData[nMonthCount].Value : 0; } // get
		} // indexer

		#endregion indexer

		#endregion public

		#region private

		#region class Row

		private class Row {
			#region public

			[UsedImplicitly]
			public int MpID { get; set; }

			[UsedImplicitly]
			public Guid MpTypeInternalID { get; set; }

			[UsedImplicitly]
			public string TurnoverType { get; set; }

			[UsedImplicitly]
			public decimal Turnover { get; set; }

			[UsedImplicitly]
			public decimal Annualized { get; set; }

			[UsedImplicitly]
			public int MonthCount { get; set; }

			[UsedImplicitly]
			public int DayCount { get; set; }

			[UsedImplicitly]
			public DateTime DateFrom { get; set; }

			[UsedImplicitly]
			public DateTime DateTo { get; set; }

			#region property IsTotal

			public bool IsTotal {
				get { return TurnoverType == Total; } // get
			} // IsTotal

			#endregion property IsTotal

			#region method WriteToLog

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
