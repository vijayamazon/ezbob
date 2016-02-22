namespace Ezbob.Backend.CalculateLoan.Models {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DbConstants;
	using Ezbob.Backend.CalculateLoan.Models.Exceptions;
	using Ezbob.Backend.CalculateLoan.Models.Helpers;
	using Ezbob.Utils.Lingvo;

	public class LoanCalculatorModel {
		public LoanCalculatorModel() {
			this.interestOnlyRepayments = 0;

			DiscountPlan = new List<decimal>();

			Repayments = new List<Repayment>();
			Fees = new List<Fee>();
			BadPeriods = new List<BadPeriod>();
			FreezePeriods = new List<InterestFreeze>();

			LoanHistory = new List<LoanHistory> { new LoanHistory() };
		} // constructor

		public List<BadPeriod> BadPeriods { get; private set; }

		// TODO: revive?

		/*

		public DateTime LoanIssueTime {
			get { return LoanHistory.First().Time; }
		} // LastIssueTime

		public DateTime LoanIssueDate { get { return LoanIssueTime.Date; } }

		public DateTime LastScheduledDate { get { return LoanHistory.Last().LastScheduledDate; } }

		*/

		public List<decimal> DiscountPlan { get; private set; }

		public List<LoanHistory> LoanHistory { get; private set; }

		public List<Repayment> Repayments { get; private set; }

		public List<Fee> Fees { get; private set; }

		public List<InterestFreeze> FreezePeriods { get; set; }

		public int InterestOnlyRepayments {
			get { return this.interestOnlyRepayments; }
			set {
				if (value < 0)
					throw new NegativeInterestOnlyRepaymentCountException(value);
				this.interestOnlyRepayments = value;
			} // set
		} // InterestOnlyRepayments

		public decimal IssuedAmount {
			get { return LoanHistory.First().Amount; }
		} // IssuedAmount

		/// <summary>
		/// Actual amount transfered to customer.
		/// </summary>
		/// <remarks>
		/// Currently (July 28 2015) actual issued amount is loan amount (principal) without set up fee.
		/// If and when "set up fee on top" is implemented this property should be updated to take
		/// on top set up fee into account.
		/// </remarks>
		public decimal ActualIssuedAmount {
			get {
				decimal setupFee = Fees.Where(fee => fee.FeeType == FeeTypes.SetupFee).Sum(fee => fee.Amount);

				return IssuedAmount - setupFee;
			} // get
		} // ActualIssuedAmount

		public void ValidateSchedule() {
			if (LoanHistory.Count < 1)
				throw new NoScheduleException();

			LoanHistory last = null;

			foreach (LoanHistory lh in LoanHistory) {
				lh.ValidateSchedule();

				if (last == null) {
					last = lh;
					continue;
				} // if

				if (last.Time >= lh.Time)
					throw new WrongLoanHistoryOrderException(last, lh);

				last = lh;
			} // for each
		} // ValidateSchedule

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString() {
			var os = new StringBuilder();

			os.Append("\tLoan calculation model - begin:\n");

			os.AppendFormat("\tLoan history ({0}) - begin:\n", Grammar.Number(LoanHistory.Count, "item"));

			os.AppendFormat("\t{0}\n", string.Join("\n\t", LoanHistory));

			os.AppendFormat("\tLoan history ({0}) - end.\n", Grammar.Number(LoanHistory.Count, "item"));

			if (DiscountPlan.Count > 0) {
				os.AppendFormat(
					"\tDiscount plan: {0}.\n",
					string.Join(", ", DiscountPlan.Select(x => x.ToString("P1", Library.Instance.Culture)))
				);
			} else
				os.Append("\tNo discount plan.\n");

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

			os.Append("\tLoan calculation model - end.\n");

			return os.ToString();
		} // ToString

		/// <summary>
		/// Creates a deep copy of current model.
		/// </summary>
		/// <returns>A copy of current model.</returns>
		public LoanCalculatorModel DeepClone() {
			var lcm = new LoanCalculatorModel {
				InterestOnlyRepayments = InterestOnlyRepayments,
			};

			lcm.DiscountPlan.AddRange(DiscountPlan);
			lcm.Repayments.AddRange(Repayments.Select(v => v.DeepClone()));
			lcm.Fees.AddRange(Fees.Select(v => v.DeepClone()));
			lcm.BadPeriods.AddRange(BadPeriods.Select(v => v.DeepClone()));

			lcm.LoanHistory.Clear();
			lcm.LoanHistory.AddRange(LoanHistory.Select(v => v.DeepClone()));

			return lcm;
		} // DeepClone

		private int interestOnlyRepayments;
	} // class LoanCalculatorModel
} // namespace