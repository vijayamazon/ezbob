namespace Ezbob.Backend.CalculateLoan.Models.Helpers {
	using System;
	using System.Collections.Generic;
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

		public decimal OpenPrincipal {
			get {
				decimal repaid = HasRepayments ? this.principalRepayments.Sum(pr => pr.Amount) : 0;
				return Principal - repaid;
			} // get
		} // OpenPrincipal

		public bool HasRepayments {
			get { return this.principalRepayments.Count > 0; }
		} // HasRepayments

		/// <exception cref="TooEarlyPrincipalRepaymentException">Condition. </exception>
		/// <exception cref="NegativeRepaymentAmountException">Condition. </exception>
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

		public override string ToString() {
			string closedDateStr = ClosedDate.HasValue
				? string.Format(" (closed on {0})", ClosedDate.DateStr())
				: string.Format(" (open principal {0})", OpenPrincipal.ToString("C2", Library.Instance.Culture));

			string repaymentsStr = HasRepayments
				? string.Format(" (repaid: {0})", string.Join(", ", this.principalRepayments))
				: string.Empty;

			return string.Format(
				"on {0}: {1} at {2}{3}{4}",
				Date.DateStr(),
				Principal.ToString("C2", Library.Instance.Culture),
				InterestRate.ToString("P1", Library.Instance.Culture),
				closedDateStr,
				repaymentsStr
			);
		} // ToString

		private readonly List<RepaidPrincipal> principalRepayments;

		private DateTime? closedDate;
	} // class ScheduledItem
} // namespace
