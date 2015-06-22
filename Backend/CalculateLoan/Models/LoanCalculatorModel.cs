namespace Ezbob.Backend.CalculateLoan.Models {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DbConstants;
	using Ezbob.Backend.CalculateLoan.Models.Exceptions;
	using Ezbob.Backend.CalculateLoan.Models.Helpers;
	using Ezbob.Backend.Extensions;
	using Ezbob.Utils.Lingvo;

	public class LoanCalculatorModel {
		public LoanCalculatorModel() {
			this.loanAmount = 0;
			this.repaymentCount = 1;
			this.interestOnlyRepayments = 0;
			this.monthlyInterestRate = 0.06m;

			RepaymentIntervalType = RepaymentIntervalTypes.Month;
			OpenPrincipalHistory = new List<OpenPrincipal>();
			DiscountPlan = new List<decimal>();
			Schedule = new List<ScheduledItem>();
			Repayments = new List<Repayment>();
			Fees = new List<Fee>();
			BadPeriods = new List<BadPeriod>();
			FreezePeriods = new List<InterestFreeze>();
		} // constructor

		public RepaymentIntervalTypes RepaymentIntervalType { get; set; }

		public List<BadPeriod> BadPeriods { get; private set; }
		//public Rollovers Rollovers { get; set; }

		public DateTime LoanIssueTime { get; set; }
		public DateTime LoanIssueDate { get { return LoanIssueTime.Date; } }
		public DateTime LastScheduledDate { get { return Schedule.Last().Date; } }

		public List<decimal> DiscountPlan { get; private set; }
		public List<OpenPrincipal> OpenPrincipalHistory { get; private set; }
		public List<ScheduledItem> Schedule { get; private set; }
		public List<Repayment> Repayments { get; private set; }
		public List<Fee> Fees { get; private set; }
		public List<InterestFreeze> FreezePeriods { get; set; }

		/// <exception cref="NegativeRepaymentCountException" accessor="set">Condition. </exception>
		public int RepaymentCount {
			get { return this.repaymentCount; }
			set {
				//Console.WriteLine(value);
				//Console.WriteLine(value.GetType());
				if (value < 0)
					throw new NegativeRepaymentCountException(value);

				this.repaymentCount = value;
			} // set
		} // RepaymentCount

		/// <exception cref="NegativeInterestOnlyRepaymentCountException" accessor="set">Condition. </exception>
		public int InterestOnlyRepayments {
			get { return this.interestOnlyRepayments; }
			set {
				if (value < 0)
					throw new NegativeInterestOnlyRepaymentCountException(value);

				this.interestOnlyRepayments = value;
			} // set
		} // InterestOnlyRepayments

		/// <exception cref="NegativeLoanAmountException" accessor="set">Condition. </exception>
		public decimal LoanAmount {
			get { return this.loanAmount; }
			set {
				if (value <= 0)
					throw new NegativeLoanAmountException(value);

				this.loanAmount = value;
			} // set
		} // LoanAmount

		/// <exception cref="NegativeMonthlyInterestRateException" accessor="set">Condition. </exception>
		public decimal MonthlyInterestRate {
			get { return this.monthlyInterestRate; }
			set {
				if (value < 0)
					throw new NegativeMonthlyInterestRateException(value);

				this.monthlyInterestRate = value;
			} // set
		} // MonthlyInterestRate

		public void SetDiscountPlan(params decimal[] deltas) {
			DiscountPlan.Clear();
			DiscountPlan.AddRange(deltas);
		} // SetDiscountPlan

		public bool IsMonthly {
			get { return RepaymentIntervalType == RepaymentIntervalTypes.Month; }
		} // IsMonthly

		/// <exception cref="NoScheduleException">Condition. </exception>
		/// <exception cref="WrongInstallmentOrderException">Condition. </exception>
		/// <exception cref="WrongFirstOpenPrincipalException">Condition. </exception>
		/// <exception cref="TooLateOpenPrincipalException">Condition. </exception>
		/// <exception cref="WrongOpenPrincipalOrderException">Condition. </exception>
		/// <exception cref="NegativeLoanAmountException">Condition. </exception>
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

			var lic = new OpenPrincipal { Date = LoanIssueTime, Amount = LoanAmount, };

			if (OpenPrincipalHistory.Count < 1)
				OpenPrincipalHistory.Add(lic);

			OpenPrincipal lastOp = OpenPrincipalHistory.First();

			if ((lastOp.Date != lic.Date) || (lastOp.Amount != lic.Amount))
				throw new WrongFirstOpenPrincipalException(lastOp, lic);

			lastOp = null;

			foreach (OpenPrincipal op in OpenPrincipalHistory) {
				if (op.Date > LastScheduledDate)
					throw new TooLateOpenPrincipalException(op, LastScheduledDate);

				if (lastOp != null)
					if (lastOp.Date >= op.Date)
						throw new WrongOpenPrincipalOrderException(lastOp, op);

				lastOp = op;
			} // for each item
		} // ValidateSchedule


		/// <exception cref="NoScheduleException">Condition. </exception>
		/// <exception cref="WrongInstallmentOrderException">Condition. </exception>
		/// <exception cref="WrongFirstOpenPrincipalException">Condition. </exception>
		/// <exception cref="TooLateOpenPrincipalException">Condition. </exception>
		/// <exception cref="WrongOpenPrincipalOrderException">Condition. </exception>
		/// <exception cref="NegativeLoanAmountException">Condition. </exception>
		public void SetScheduleCloseDatesFromPayments() {
			ValidateSchedule();

			var qsp = new Queue<ScheduledItem>();

			foreach (ScheduledItem s in Schedule) {
				s.ClearRepayments();
				qsp.Enqueue(s);
			} // for each

			if (Repayments.Count < 1)
				return;

			ScheduledItem curSchedule = qsp.Dequeue();

			for (var i = 0; i < Repayments.Count; i++) {
				Repayment curRepayment = Repayments[i];

				decimal currentPaidPrincipal = curRepayment.Principal;

				while (currentPaidPrincipal > 0) {
					currentPaidPrincipal = curSchedule.AddPrincipalRepayment(currentPaidPrincipal, curRepayment.Date);

					if (curSchedule.ClosedDate.HasValue) {
						if (qsp.Count > 0)
							curSchedule = qsp.Dequeue();
						else {
							curSchedule = null;
							break;
						} // if
					} // if
				} // while

				if (curSchedule == null)
					break;
			} // for each repayment
		} // SetScheduleCloseDatesFromPayments



		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		/// <exception cref="NegativeRepaymentCountException">Condition. </exception>
		/// <exception cref="NegativeMonthlyInterestRateException">Condition. </exception>
		/// <exception cref="NegativeLoanAmountException">Condition. </exception>
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

			if (OpenPrincipalHistory.Count > 0)
				os.AppendFormat("\tOpen principal:\n\t\t{0}.\n", string.Join("\n\t\t", OpenPrincipalHistory));
			else
				os.Append("\tNo open principal history.\n");

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

			if (BadPeriods.Count > 0)
				os.AppendFormat("\tBadPeriods:\n\t\t{0}.\n", string.Join("\n\t\t", BadPeriods));
			else
				os.Append("\tNo BadPeriods.\n");

			if (FreezePeriods.Count > 0)
				os.AppendFormat("\t FreezePeriods:\n\t\t{0}.\n", string.Join("\n\t\t", FreezePeriods));
			else
				os.Append("\tNo FreezePeriods.\n");

			//if (Rollovers != null)
			//     os.AppendFormat("\tRollovers:\n\t\t{0}.\n", string.Join("\n\t\t", Rollovers));
			//else
			//     os.Append("\tNo rollovers.\n");

			os.Append("\tLoan calculation model - end.\n");

			return os.ToString();
		} // ToString

		/// <summary>
		/// Creates a deep copy of current model.
		/// </summary>
		/// <returns>A copy of current model.</returns>
		/// <exception cref="NegativeLoanAmountException">Condition. </exception>
		/// <exception cref="NegativeRepaymentCountException">Condition. </exception>
		/// <exception cref="NegativeInterestOnlyRepaymentCountException">Condition. </exception>
		/// <exception cref="NegativeMonthlyInterestRateException">Condition. </exception>
		//public LoanCalculatorModel DeepClone() {
		//     var lcm = new LoanCalculatorModel {
		//            LoanAmount = LoanAmount,
		//            LoanIssueTime = LoanIssueTime,
		//            RepaymentCount = RepaymentCount,
		//            InterestOnlyRepayments = InterestOnlyRepayments,
		//            RepaymentIntervalType = RepaymentIntervalType,
		//            MonthlyInterestRate = MonthlyInterestRate,
		//     };
		//     lcm.DiscountPlan.AddRange(DiscountPlan);
		//     lcm.OpenPrincipalHistory.AddRange(OpenPrincipalHistory.Select(v => v.DeepClone()));
		//     lcm.Schedule.AddRange(Schedule.Select(v => v.DeepClone()));
		//     lcm.Repayments.AddRange(Repayments.Select(v => v.DeepClone()));
		//     lcm.Fees.AddRange(Fees.Select(v => v.DeepClone()));
		//     lcm.BadPeriods.DeepCloneFrom(BadPeriods);
		//     return lcm;
		//} // DeepClone

		private int repaymentCount;
		private int interestOnlyRepayments;
		private decimal loanAmount;
		private decimal monthlyInterestRate;

	}// class LoanCalculatorModel
} // namespace