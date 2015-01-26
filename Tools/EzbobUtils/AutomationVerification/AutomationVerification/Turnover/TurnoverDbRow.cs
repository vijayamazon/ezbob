// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TurnoverDbRow.cs" company="">
//     </copyright>
// <summary>
// The turnover db row.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AutomationCalculator.Turnover {
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Lingvo;
	using JetBrains.Annotations;
	using System;
	using System.Globalization;

	/// <summary>
	/// The turnover db row.
	/// </summary>
	public class TurnoverDbRow : AResultRow {
		/// <summary>
		/// Gets or sets the turnover.
		/// </summary>
		[UsedImplicitly]
		public decimal Turnover { get; set; }

		/// <summary>
		/// Gets or sets the the month.
		/// </summary>
		[UsedImplicitly]
		public DateTime TheMonth { get; set; }

		/// <summary>
		/// Gets or sets the distance.
		/// </summary>
		[UsedImplicitly]
		public int Distance { get; set; }

		/// <summary>
		/// Gets or sets the current month.
		/// </summary>
		[UsedImplicitly]
		public DateTime CurrentMonth { get; set; }

		/// <summary>
		/// Gets or sets the mp id.
		/// </summary>
		[UsedImplicitly]
		public int MpID { get; set; }

		/// <summary>
		/// Gets or sets the GUID mp type id.
		/// </summary>
		[UsedImplicitly]
		public Guid MpTypeID { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether is payment account.
		/// </summary>
		[UsedImplicitly]
		public bool IsPaymentAccount { get; set; }

		/// <summary>
		/// Gets the month count.
		/// </summary>
		public int MonthCount {
			get { return this.Distance + 1; }
		} // MonthCount

		/// <summary>
		/// The write to log.
		/// </summary>
		/// <param name="oLog">The o log.</param>
		public void WriteToLog(ASafeLog oLog) {
			if (oLog == null)
				return;

			oLog.Debug(
				"One month turnover for customer marketplace {0} ('{1}'...'{2}' - {3}) - mp id {4} ({5}) of type {6}",
				this.Turnover,
				this.TheMonth.ToString("MMM yyyy", CultureInfo.InvariantCulture),
				this.CurrentMonth.ToString("d/MMM/yyyy", CultureInfo.InvariantCulture),
				Grammar.Number(this.Distance, "month"),
				this.MpID,
				this.IsPaymentAccount ? "payment account" : "online marketplace",
				this.MpTypeID);
		} // WriteToLog
	} // class TurnoverDbRow
} // namespace
