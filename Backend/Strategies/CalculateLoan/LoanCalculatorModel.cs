namespace Ezbob.Backend.Strategies.CalculateLoan {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DbConstants;
	using Ezbob.Backend.Strategies.CalculateLoan.Helpers;
	using Ezbob.Utils.Lingvo;

	public class LoanCalculatorModel {
		public LoanCalculatorModel() {
			this.loanAmount = 0;
			this.repaymentCount = 1;
			this.interestOnlyMonths = 0;
			this.monthlyInterestRate = 0.06m;

			DiscountPlan = new List<decimal>();
			Schedule = new List<ScheduledPayment>();
			Repayments = new List<Repayment>();
			Fees = new List<Fee>();
			BadPeriods = new BadPeriods();
			FreezePeriods = new InterestFreezePeriods();
		} // constructor

		public void ValidateSchedule() {
			if (Schedule.Count < 1)
				throw new Exception("No loan schedule found.");

			for (int i = 0; i < Schedule.Count; i++)
				if (Schedule[i].Date == null)
					throw new Exception("No date specified for scheduled payment #" + (i + 1));
		} // ValidateSchedule

		public void SetDiscountPlan(params decimal[] deltas) {
			DiscountPlan.Clear();
			DiscountPlan.AddRange(deltas);
		} // SetDiscountPlan

		/// <summary>
		/// Creates a deep copy of current model.
		/// </summary>
		/// <returns>A copy of current model.</returns>
		public LoanCalculatorModel DeepClone() {
			var lcm = new LoanCalculatorModel {
				LoanAmount = LoanAmount,
				LoanIssueTime = LoanIssueTime,
				RepaymentCount = RepaymentCount,
				InterestOnlyMonths = InterestOnlyMonths,
				RepaymentIntervalType = RepaymentIntervalType,
				MonthlyInterestRate = MonthlyInterestRate,
			};

			lcm.DiscountPlan.AddRange(DiscountPlan);

			lcm.Schedule.AddRange(Schedule.Select(v => v.DeepClone()));
			lcm.Repayments.AddRange(Repayments.Select(v => v.DeepClone()));
			lcm.Fees.AddRange(Fees.Select(v => v.DeepClone()));

			lcm.BadPeriods.DeepCloneFrom(BadPeriods);
			lcm.FreezePeriods.DeepCloneFrom(FreezePeriods);

			return lcm;
		} // DeepClone

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString() {
			var os = new StringBuilder();

			os.Append("\tLoan calculation model - begin:\n");

			os.AppendFormat(
				"\t{3} issued on {0} for {1} each {2} at monthly rate {4}.\n",
				LoanIssueTime.MomentStr(),
				Grammar.Number(RepaymentCount, "repayment"),
				RepaymentIntervalType == RepaymentIntervalTypes.Month
					? "month"
					: Grammar.Number((int)RepaymentIntervalType, "day"),
				LoanAmount.ToString("C2", Library.Instance.Culture),
				MonthlyInterestRate.ToString("P2", Library.Instance.Culture)
			);

			if (DiscountPlan.Count > 0) {
				os.AppendFormat(
					"\tDiscount plan: {0}.\n",
					string.Join(", ", DiscountPlan.Select(x => x.ToString("P1", Library.Instance.Culture)))
				);
			} else
				os.Append("\tNo discount plan.\n");

			if (Schedule.Count > 0)
				os.AppendFormat("\tSchedule:\n\t\t{0}.\n", string.Join("\n\t\t", Schedule));
			else
				os.Append("\tNo schedule.\n");

			if (Repayments.Count > 0)
				os.AppendFormat("\tRepayments:\n\t\t{0}.\n", string.Join("\n\t\t", Repayments));
			else
				os.Append("\tNo repayments.\n");

			if (Fees.Count > 0)
				os.AppendFormat("\tFees:\n\t\t{0}.\n", string.Join("\n\t\t", Fees));
			else
				os.Append("\tNo fees.\n");

			os.AppendFormat("\tBad periods: {0}\n", BadPeriods);

			if (FreezePeriods.Count > 0)
				os.AppendFormat("\tInterest freeze periods: {0}.\n", FreezePeriods);
			else
				os.Append("\tNo interest freeze periods.\n");

			os.Append("\tLoan calculation model - end.\n");

			return os.ToString();
		} // ToString

		public DateTime LoanIssueTime { get; set; }

		public DateTime LastScheduledDate {
			get {
				ValidateSchedule();

				// ReSharper disable once PossibleInvalidOperationException
				// ValidateSchedule() eliminates arriving to this point if there are NULLs.
				return Schedule.Last().Date.Value.Date;
			} // get
		} // LastScheduledDate

		public int RepaymentCount {
			get { return this.repaymentCount; }
			set {
				if (value < 1)
					throw new ArgumentOutOfRangeException("Repayment count is negative: " + value + ".", (Exception)null);

				this.repaymentCount = value;
			} // set
		} // RepaymentCount

		public int InterestOnlyMonths {
			get { return this.interestOnlyMonths; }
			set {
				if (value < 0) {
					throw new ArgumentOutOfRangeException(
						"Interest only months is negative: " + value + ".", (Exception)null
					);
				} // if

				this.interestOnlyMonths = value;
			} // set
		} // InterestOnlyMonths

		public decimal LoanAmount {
			get { return this.loanAmount; }
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException("Loan amount is negative: " + value + ".", (Exception)null);

				this.loanAmount = value;
			} // set
		} // LoanAmount

		public decimal MonthlyInterestRate {
			get { return this.monthlyInterestRate; }
			set {
				if (value < 0) {
					throw new ArgumentOutOfRangeException(
						"Monthly interest rate is out of range: " + value + ".",
						(Exception)null
					);
				} // if

				this.monthlyInterestRate = value;
			} // set
		} // MonthlyInterestRate

		public bool IsMonthly {
			get { return RepaymentIntervalType == RepaymentIntervalTypes.Month; }
		} // IsMonthly

		public RepaymentIntervalTypes RepaymentIntervalType { get; set; }

		public List<decimal> DiscountPlan { get; private set; }

		public List<ScheduledPayment> Schedule { get; private set; }
		public List<Repayment> Repayments { get; private set; }
		public List<Fee> Fees { get; private set; }
		public BadPeriods BadPeriods { get; private set; }
		public InterestFreezePeriods FreezePeriods { get; private set; }

		private int repaymentCount;
		private int interestOnlyMonths;
		private decimal loanAmount;
		private decimal monthlyInterestRate;
	} // class LoanCalculatorModel
} // namespace
