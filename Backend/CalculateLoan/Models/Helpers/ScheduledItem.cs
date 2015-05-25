namespace Ezbob.Backend.CalculateLoan.Models.Helpers {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using Ezbob.Backend.CalculateLoan.Models.Exceptions;
	using Ezbob.Backend.Extensions;

	public class ScheduledItem {
		public ScheduledItem(DateTime scheduledDate) {
			Date = scheduledDate.Date;
			ClosedDate = null;
			Principal = 0;
			InterestRate = 0;
			this.principalRepayments = new List<RepaidPrincipal>();
		} // constructor

		public DateTime Date { get; set; }

		public DateTime? ClosedDate {
			get { return this.closedDate; }
			set { this.closedDate = value == null ? (DateTime?)null : value.Value.Date; }
		} // ClosedDate

		public decimal Principal { get; set; }
		public decimal InterestRate { get; set; }
		public int Position { get; set; }

		public decimal OpenPrincipal {
			get {
				decimal repaid = HasRepayments ? this.principalRepayments.Sum(pr => pr.Amount) : 0;
				return Principal - repaid;
			} // get
		} // OpenPrincipal

		public bool HasRepayments {
			get { return this.principalRepayments.Count > 0; }
		} // HasRepayments

		public decimal AddPrincipalRepayment(decimal principal, DateTime time) {
			if (HasRepayments)
				if (time < this.principalRepayments.Last().Time)
					throw new TooEarlyPrincipalRepaymentException(time, this.principalRepayments.Last().Time);

			if (principal == 0)
				return 0;

			decimal currentOpenPrincipal = OpenPrincipal;

			if (principal >= currentOpenPrincipal) {
				this.principalRepayments.Add(new RepaidPrincipal(currentOpenPrincipal, time));
				ClosedDate = time;
				return principal - currentOpenPrincipal;
			} // if

			this.principalRepayments.Add(new RepaidPrincipal(principal, time));
			return 0;
		} // AddPrincipalRepayment

		public void ClearRepayments() {
			this.principalRepayments.Clear();
			ClosedDate = null;
		} // ClearRepayments

		public bool IsClosedOn(DateTime date) {
			return ClosedDate.HasValue && (ClosedDate.Value.Date <= date.Date);
		} // IsClosedOn

		public ScheduledItem DeepClone() {
			var res = new ScheduledItem(Date) {
				ClosedDate = ClosedDate,
				Principal = Principal,
				InterestRate = InterestRate,
			};

			res.principalRepayments.AddRange(this.principalRepayments.Select(pr => pr.DeepClone()));

			return res;
		} // DeepClone

		public class RepaidPrincipal {
			public RepaidPrincipal(decimal amount, DateTime time) {
				Amount = amount;
				Time = time;
			} // constructor

			public RepaidPrincipal DeepClone() {
				return new RepaidPrincipal(Amount, Time);
			} // DeepClone

			public decimal Amount {
				get { return this.amount; }

				private set {
					if (value <= 0)
						throw new NegativeRepaymentAmountException(value);

					this.amount = value;
				} // set
			} // Amount

			public DateTime Time { get; private set; }

			/// <summary>
			/// Returns a string that represents the current object.
			/// </summary>
			/// <returns>
			/// A string that represents the current object.
			/// </returns>
			public override string ToString() {
				return string.Format("{0} at {1}", Amount.ToString("C2", Culture), Time.MomentStr());
			} // ToString

			private decimal amount;
		} // class RepaidPrincipal

		public override string ToString() {
			string closedDateStr = ClosedDate.HasValue
				? string.Format(" (closed on {0})", ClosedDate.DateStr())
				: string.Format(" (open principal {0})", OpenPrincipal.ToString("C2", Culture));

			string repaymentsStr = HasRepayments
				? string.Format(" (repaid: {0})", string.Join(", ", this.principalRepayments))
				: string.Empty;

			return string.Format(
				"on {0}: {1} at {2}{3}{4}",
				Date.DateStr(),
				Principal.ToString("C2", Culture),
				InterestRate.ToString("P1", Culture),
				closedDateStr,
				repaymentsStr
			);
		} // ToString

		private readonly List<RepaidPrincipal> principalRepayments;

		private static CultureInfo Culture {
			get { return Library.Instance.Culture; }
		} // Culture

		private DateTime? closedDate;
	} // class ScheduledItem
} // namespace
