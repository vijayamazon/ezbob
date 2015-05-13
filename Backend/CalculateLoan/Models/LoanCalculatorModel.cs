﻿namespace Ezbob.Backend.CalculateLoan.Models {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DbConstants;
	using Ezbob.Backend.CalculateLoan.Models.Helpers;
	using Ezbob.Backend.Extensions;
	using Ezbob.Utils.Lingvo;

	public class LoanCalculatorModel {
		public LoanCalculatorModel() {
			this.loanAmount = 0;
			this.repaymentCount = 1;
			this.interestOnlyRepayments = 0;
			this.monthlyInterestRate = 0.06m;

			DiscountPlan = new List<decimal>();
			Schedule = new List<Scheduled>();
			Repayments = new List<Repayment>();
			Fees = new List<Fee>();
			BadPeriods = new BadPeriods();
		} // constructor

		public void ValidateSchedule() {
			if (Schedule.Count < 1)
				throw new Exception("No loan schedule found.");
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
				InterestOnlyRepayments = InterestOnlyRepayments,
				RepaymentIntervalType = RepaymentIntervalType,
				MonthlyInterestRate = MonthlyInterestRate,
			};

			lcm.DiscountPlan.AddRange(DiscountPlan);

			lcm.Schedule.AddRange(Schedule.Select(v => v.DeepClone()));
			lcm.Repayments.AddRange(Repayments.Select(v => v.DeepClone()));
			lcm.Fees.AddRange(Fees.Select(v => v.DeepClone()));

			lcm.BadPeriods.DeepCloneFrom(BadPeriods);

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

			os.Append("\tLoan calculation model - end.\n");

			return os.ToString();
		} // ToString

		public DateTime LoanIssueTime { get; set; }

		public DateTime LastScheduledDate {
			get {
				ValidateSchedule();
				return Schedule.Last().Date;
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

		public int InterestOnlyRepayments {
			get { return this.interestOnlyRepayments; }
			set {
				if (value < 0) {
					throw new ArgumentOutOfRangeException(
						"Interest only months is negative: " + value + ".", (Exception)null
					);
				} // if

				this.interestOnlyRepayments = value;
			} // set
		} // InterestOnlyRepayments

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

		public void SetScheduleCloseDatesFromPayments() {
			ValidateSchedule();

			var qsp = new Queue<ScheduledPaymentWithRepayment>();

			foreach (Scheduled s in Schedule)
				qsp.Enqueue(new ScheduledPaymentWithRepayment(s));

			if (Repayments.Count < 1)
				return;

			ScheduledPaymentWithRepayment curSchedule = qsp.Dequeue();

			for (var i = 0; i < Repayments.Count; i++) {
				Repayment curRepayment = Repayments[i];

				decimal currentPaidPrincipal = curRepayment.Principal;

				while (currentPaidPrincipal > 0) {
					currentPaidPrincipal = curSchedule.AddPayment(currentPaidPrincipal, curRepayment.Date);

					if (curSchedule.ScheduledPayment.ClosedDate.HasValue) {
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

		public RepaymentIntervalTypes RepaymentIntervalType { get; set; }

		public List<decimal> DiscountPlan { get; private set; }

		public List<Scheduled> Schedule { get; private set; }
		public List<Repayment> Repayments { get; private set; }
		public List<Fee> Fees { get; private set; }
		public BadPeriods BadPeriods { get; private set; }

		private class ScheduledPaymentWithRepayment {
			public ScheduledPaymentWithRepayment(Scheduled sp) {
				ScheduledPayment = sp;
				this.openPrincipal = ScheduledPayment.Principal;
				ScheduledPayment.ClosedDate = null;
			} // constructor

			public decimal AddPayment(decimal payment, DateTime closeDate) {
				if (payment == 0)
					return 0;

				if (payment >= this.openPrincipal) {
					payment -= this.openPrincipal;
					ScheduledPayment.ClosedDate = closeDate;
					this.openPrincipal = 0;
					return payment;
				} // if

				this.openPrincipal -= payment;
				return 0;
			} // AddPayment

			public Scheduled ScheduledPayment { get; private set; }

			private decimal openPrincipal;
		} // class ScheduledPaymentWithRepayment

		private int repaymentCount;
		private int interestOnlyRepayments;
		private decimal loanAmount;
		private decimal monthlyInterestRate;
	} // class LoanCalculatorModel
} // namespace
