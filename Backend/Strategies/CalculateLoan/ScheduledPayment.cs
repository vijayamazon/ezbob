﻿namespace Ezbob.Backend.Strategies.CalculateLoan {
	using System;
	using System.Globalization;

	public class ScheduledPayment {
		public ScheduledPayment() {
			Date = null;
			Principal = 0;
			InterestRate = 0;
		} // constructor

		public DateTime? Date { get; set; }
		public decimal Principal { get; set; }
		public decimal InterestRate { get; set; }

		public ScheduledPayment DeepClone() {
			return new ScheduledPayment {
				Date = this.Date,
				Principal = this.Principal,
				InterestRate = this.InterestRate,
			};
		} // DeepClone

		public override string ToString() {
			return string.Format(
				"on {0}: {1} at {2}",
				Date.DateStr(),
				Principal.ToString("C2", Culture),
				InterestRate.ToString("P1", Culture)
			);
		} // ToString

		private static CultureInfo Culture {
			get { return Library.Instance.Culture; }
		} // Culture
	} // class TransactionData
} // namespace Reports