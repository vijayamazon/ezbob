namespace Ezbob.Backend.Strategies.CalculateLoan {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DbConstants;
	using Ezbob.Utils.Lingvo;

	public class LoanCalculatorModel {
		public LoanCalculatorModel() {
			this.loanAmount = 0;
			this.repaymentCount = 1;
			this.interestOnlyMonths = 0;
			this.interestRate = 0.06m;

			DiscountPlan = new List<decimal>();
			Schedule = new List<ScheduledPayment>();
			Repayments = new List<ActualRepayment>();
			Fees = new List<Fee>();
			BadPeriods = new BadPeriods();
			FreezePeriods = new InterestFreezePeriods();
		} // constructor

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
				InterestRate = InterestRate,
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

			os.AppendFormat(
				"\tIssued on {0} for {1} each {2}.\n",
				LoanIssueTime.MomentStr(),
				Grammar.Number(RepaymentCount, "repayment"),
				RepaymentIntervalType == RepaymentIntervalTypes.Month
					? "month"
					: Grammar.Number((int)RepaymentIntervalType, "month")
			);

			if (DiscountPlan.Count > 0)
				os.AppendFormat("\tDiscount plan: {0}.\n", string.Join(", ", DiscountPlan.Select(x => x.ToString("P6"))));
			else
				os.Append("\tNo discount plan.\n");

			if (Schedule.Count > 0)
				os.AppendFormat("\tSchedule: {0}.\n", string.Join("; ", Schedule));
			else
				os.Append("\tNo schedule.\n");

			if (Repayments.Count > 0)
				os.AppendFormat("\tRepayments: {0}.\n", string.Join("; ", Repayments));
			else
				os.Append("\tNo repayments.\n");

			if (Fees.Count > 0)
				os.AppendFormat("\tFees: {0}.\n", string.Join("; ", Fees));
			else
				os.Append("\tNo fees.\n");

			os.AppendFormat("\tBad periods: {0}\n", BadPeriods);

			if (FreezePeriods.Count > 0)
				os.AppendFormat("\tInterest freeze periods: {0}.\n", FreezePeriods);
			else
				os.Append("\tNo interest freeze periods.\n");

			return os.ToString();
		} // ToString

		public DateTime LoanIssueTime { get; set; }

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

		public decimal InterestRate {
			get { return this.interestRate; }
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException("Interest rate is out of range: " + value + ".", (Exception)null);

				this.interestRate = value;
			} // set
		} // InterestRate

		public bool IsMonthly {
			get { return RepaymentIntervalType == RepaymentIntervalTypes.Month; }
		} // IsMonthly

		public RepaymentIntervalTypes RepaymentIntervalType { get; set; }

		public List<decimal> DiscountPlan { get; private set; }

		public List<ScheduledPayment> Schedule { get; private set; }
		public List<ActualRepayment> Repayments { get; private set; }
		public List<Fee> Fees { get; private set; }
		public BadPeriods BadPeriods { get; private set; }
		public InterestFreezePeriods FreezePeriods { get; private set; }

		private int repaymentCount;
		private int interestOnlyMonths;
		private decimal loanAmount;
		private decimal interestRate;
	} // class LoanCalculatorModel
} // namespace
