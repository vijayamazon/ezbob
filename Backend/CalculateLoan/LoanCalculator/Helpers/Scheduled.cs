namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Helpers {
	using System;
	using System.Globalization;
	using Ezbob.Backend.Extensions;

	public class Scheduled {
		public Scheduled(DateTime scheduledDate) {
			Date = scheduledDate.Date;
			ClosedDate = null;
			Principal = 0;
			InterestRate = 0;
		} // constructor

		public DateTime Date { get; set; }

		public DateTime? ClosedDate {
			get { return this.closedDate; }
			set { this.closedDate = value == null ? (DateTime?)null : value.Value.Date; }
		} // ClosedDate

		public decimal Principal { get; set; }
		public decimal InterestRate { get; set; }

		public bool IsClosedOn(DateTime date) {
			return ClosedDate.HasValue && (ClosedDate.Value.Date <= date.Date);
		} // IsClosedOn

		public Scheduled DeepClone() {
			return new Scheduled(Date) {
				ClosedDate = ClosedDate,
				Principal = Principal,
				InterestRate = InterestRate,
			};
		} // DeepClone

		public override string ToString() {
			string closedDateStr = ClosedDate.HasValue
				? string.Format(" (closed on {0})", ClosedDate.DateStr())
				: string.Empty;

			return string.Format(
				"on {0}: {1} at {2}{3}",
				Date.DateStr(),
				Principal.ToString("C2", Culture),
				InterestRate.ToString("P1", Culture),
				closedDateStr
			);
		} // ToString

		private static CultureInfo Culture {
			get { return Library.Instance.Culture; }
		} // Culture

		private DateTime? closedDate;
	} // class Scheduled
} // namespace
