namespace EzBob.Backend.Strategies.MainStrategy.AutoDecisions.Reject.Turnover {
	using System;
	using System.Globalization;
	using Ezbob.Logger;
	using JetBrains.Annotations;

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
		/// This account type is payment or not.
		/// </summary>
		[UsedImplicitly]
		public bool IsPaymentAccount { get; set; }

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
				"Turnover for customer {9}:\n" +
				"\tmarketplace: id {1}, type {2}\n" +
				"\t{0} (because of type {4})\n" +
				"\tturnover: {3},month count: {5}, day count: {6}\n" +
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
				IsPaymentAccount ? "payment account" : "marketplace"
			);
		} // WriteToLog

		private const string Total = "Total";
	} // Row
} // namespace
