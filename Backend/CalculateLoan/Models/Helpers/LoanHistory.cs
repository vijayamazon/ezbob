namespace Ezbob.Backend.CalculateLoan.Models.Helpers {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DbConstants;
	using Ezbob.Backend.CalculateLoan.Models.Exceptions;
	using Ezbob.Backend.Extensions;
	using Ezbob.Utils.Lingvo;

	public class LoanHistory {
		public LoanHistory() {
			this.amount = 0;
			this.repaymentCount = 1;
			this.monthlyInterestRate = 0.06m;

			RepaymentIntervalType = RepaymentIntervalTypes.Month;

			IsFirst = true;

			Schedule = new List<ScheduledItem>();
		} // constructor

		public DateTime Date {
			get { return Time.Date; }
		} // Date

		public DateTime Time { get; set; }

		public bool IsFirst { get; set; }

		public RepaymentIntervalTypes RepaymentIntervalType { get; set; }

		public decimal Amount {
			get { return this.amount; }
			set {
				if (value < 0)
					throw new NegativeOpenPrincipalException(value);

				this.amount = value;
			} // set
		} // Amount

		public int RepaymentCount {
			get { return this.repaymentCount; }
			set {
				if (value < 0)
					throw new NegativeRepaymentCountException(value);
				this.repaymentCount = value;
			} // set
		} // RepaymentCount

		public decimal MonthlyInterestRate {
			get { return this.monthlyInterestRate; }
			set {
				if (value < 0)
					throw new NegativeMonthlyInterestRateException(value);
				this.monthlyInterestRate = value;
			} // set
		} // MonthlyInterestRate

		public List<ScheduledItem> Schedule { get; private set; }

		public LoanHistory DeepClone() {
			var res = new LoanHistory {
				Time = Time,
				Amount = Amount,
				RepaymentCount = RepaymentCount,
				MonthlyInterestRate = MonthlyInterestRate,
				RepaymentIntervalType = RepaymentIntervalType,
			};

			Schedule.AddRange(Schedule.Select(v => v.DeepClone()));

			return res;
		} // DeepClone

		public DateTime LastScheduledDate { get { return Schedule.Last().Date; } }

		public void ValidateSchedule() {
			if (Schedule.Count < 1)
				throw new NoScheduleException();

			ScheduledItem lastSch = null;

			foreach (ScheduledItem sch in Schedule) {
				if (lastSch != null)
					if (lastSch.Date >= sch.Date)
						throw new WrongInstallmentOrderException(lastSch, sch);

				lastSch = sch;
			} // for each
		} // ValidateSchedule

		public LoanHistory Clear() {
			this.amount = 0;
			this.repaymentCount = 0;
			this.monthlyInterestRate = 0;
			RepaymentIntervalType = RepaymentIntervalTypes.Month;
			IsFirst = true;
			Schedule.Clear();
			return this;
		} // Clear

		public override string ToString() {
			StringBuilder os = new StringBuilder();

			os.AppendFormat(
				"\tOpen principal {0} {1} on {2} for {3} every {4} at monthly rate {5}.\n",
				Amount.ToString("C2", Library.Instance.Culture),
				IsFirst ? "issued" : "rescheduled",
				Time.MomentStr(),
				Grammar.Number(RepaymentCount, "repayment"),
				RepaymentIntervalType == RepaymentIntervalTypes.Month
					? "month"
					: Grammar.Number((int)RepaymentIntervalType, "day"),
				MonthlyInterestRate.ToString("P2", Library.Instance.Culture)
			);

			if (Schedule.Count > 0)
				os.AppendFormat("\tSchedule:\n\t\t{0}.\n", string.Join("\n\t\t", Schedule));
			else
				os.Append("\tNo schedule.\n");

			return os.ToString();
		} // ToString

		private decimal amount;
		private int repaymentCount;
		private decimal monthlyInterestRate;
	} // class LoanHistory
} // namespace
