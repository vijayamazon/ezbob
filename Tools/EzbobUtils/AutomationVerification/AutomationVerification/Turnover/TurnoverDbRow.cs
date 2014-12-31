namespace AutomationCalculator.Turnover {
	using System;
	using System.Globalization;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Lingvo;
	using JetBrains.Annotations;

	public class TurnoverDbRow : AResultRow {
		[UsedImplicitly]
		public decimal Turnover { get; set; }

		[UsedImplicitly]
		public DateTime TheMonth { get; set; }

		[UsedImplicitly]
		public int Distance { get; set; }

		[UsedImplicitly]
		public DateTime CurrentMonth { get; set; }

		[UsedImplicitly]
		public int MpID { get; set; }

		[UsedImplicitly]
		public Guid MpTypeID { get; set; }

		[UsedImplicitly]
		public bool IsPaymentAccount { get; set; }

		public int MonthCount {
			get { return Distance + 1; } // get
		} // MonthCount

		public void WriteToLog(ASafeLog oLog) {
			if (oLog == null)
				return;

			oLog.Debug(
				"One month turnover for customer marketplace {0} ('{1}'...'{2}' - {3}) - mp id {4} ({5}) of type {6}",
				Turnover,
				TheMonth.ToString("MMM yyyy", CultureInfo.InvariantCulture),
				CurrentMonth.ToString("d/MMM/yyyy", CultureInfo.InvariantCulture),
				Grammar.Number(Distance, "month"),
				MpID,
				(IsPaymentAccount ? "payment account" : "online marketplace"),
				MpTypeID
				);
		} // WriteToLog
	} // class TurnoverDbRow
} // namespace
