namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Models {
	using System;
	using System.Text;
	using Ezbob.Backend.ModelsWithDB.NewLoan;

	public class LoanEvent {

		// default ctor
		public LoanEvent(DateTime date, int priority = 0) {
			Date = date;
			this.priority = priority;
		}
		// history
		public LoanEvent(DateTime date, NL_LoanHistory history, int priority = 0): this(new DateTime(date.Year, date.Month, date.Day, 00, 00, 00), priority) {
			History = history;
		}
		// fee
		public LoanEvent(DateTime date, NL_LoanFees fee): this(new DateTime(date.Year, date.Month, date.Day, 23, 59, 57)) {
			Fee = fee;
		}
		// payment
		public LoanEvent(DateTime date, NL_Payments payment, int priority = 0): this(new DateTime(date.Year, date.Month, date.Day, 23, 59, 58), priority) {
			Payment = payment;
		}
		//	installment processed at the end of day
		public LoanEvent(DateTime date, NL_LoanSchedules scheduleItem, int priority = 0): this(new DateTime(date.Year, date.Month, date.Day, 23, 59, 59), priority) {
			Installment = scheduleItem;
		}
		// rollover
		public LoanEvent(DateTime date, NL_LoanRollovers rollover): this(date) {
			Rollover = rollover;
		}
		// action
		public LoanEvent(DateTime date, Action action, int priority = 0): this(date, priority) {
			Action = action;
		}
		
		public DateTime Date { get; set; }
		public NL_LoanSchedules Installment { get; set; }
		public NL_Payments Payment { get; set; }
		public NL_LoanFees Fee { get; set; }
		public NL_LoanHistory History { get; set; }
		public NL_LoanRollovers Rollover { get; set; } // not supported yet
		public Action Action { get; set; } // not supported yet (re-scheduling, etc)
		
		public NL_LoanHistory CurrentHistory { get; set; }

		// used to hold data of the current history
		//public decimal LoanAmount { get; set; }
		//public decimal RepaymentIntervalTypeID { get; set; }
		//public decimal RepaymentCount { get; set; }
		//public decimal InterestRate { get; set; }
		//public int InterestOnlyRepaymentCount { get; set; }

		/*//// A, i.e. total principal of loan == initialAmount
		//protected decimal totalPrincipal { get; set; }
		//	Money that really have customer. Loan balance without interest and fees. деньги, которые реально находятся у клиента на руках. баланс кредита без процентов и fee. 
		protected decimal openPrincipal { get; set; } //private decimal _principal;
		// principal paid on the date of event
		protected decimal paidPrincipal { get; set; }

		//// total fees assigned to loan
		//protected decimal totalFees { get; set; }
		// to be paid
		protected decimal openFees { get; set; }
		// fees paid on the date of event
		protected decimal paidFees { get; set; }

		// total loan interest (earned interest). Доход банка за все время заема
		//protected decimal totalInterest { get; set; } // _totalInterestToPay
		// to be paid
		protected decimal openInterest { get; set; }
		// interest paid on the date of event
		protected decimal paidInterest { get; set; }*/
		

		private readonly int priority;

		public int Priority {
			get {
				if (this.priority != 0)
					return this.priority;
				if (Payment != null)
					return 1;
				if (Installment != null)
					return 2;
				if (Action != null)
					return 3;
				return 0;
			}
		}

		public override string ToString() {
			return string.Format("{0}, {1}", Date, GetTypeString());
		}

		public string GetTypeString() {
			StringBuilder sb = new StringBuilder();
			if (History != null) {
				sb.Append("History:\t")
					.Append(string.Format("Amount: {0}, RepaymentCount: {1}, InterestRate: {2}, Description: {3}",
						History.Amount, History.RepaymentCount, History.InterestRate, History.Description)); //.Append(Environment.NewLine).Append(AStringable.GetHeadersLine(typeof(NL_LoanHistory))).Append(History);
				return sb.ToString();
			}
			if (Installment != null) {
				sb.Append("Installment:\t").Append(Installment.ToBaseString()); //.Append(Environment.NewLine).Append(AStringable.GetHeadersLine(typeof(NL_LoanSchedules))).Append(Installment);
				return sb.ToString();
			}
			if (Payment != null) {
				sb.Append("Payment:\t").Append(string.Format("Amount: {0}, PaymentMethodID: {1}, PaymentTime: {2}, PaymentStatusID: {3}", 
					Payment.Amount, Payment.PaymentMethodID, Payment.PaymentTime, Payment.PaymentStatusID)); //.Append(Environment.NewLine).Append(AStringable.GetHeadersLine(typeof(NL_Payments))).Append(Payment);
				return sb.ToString();
			}
			if (Fee != null) {
				sb.Append("Fee:\t").Append(string.Format("Type: {0}, AssignTime: {1}, Amount: {2}", Fee.LoanFeeTypeID, Fee.AssignTime, Fee.Amount)); //.Append(Environment.NewLine).Append(AStringable.GetHeadersLine(typeof(NL_LoanFees))).Append(Fee);
				return sb.ToString();
			}
			if (Rollover != null) {
				sb.Append("Rollover:\t"); //.Append(Environment.NewLine).Append(AStringable.GetHeadersLine(typeof(NL_LoanRollovers))).Append(Rollover);
				return sb.ToString();
			}
			if (Action != null) {
				sb.Append("Action:\t"); //.Append(Environment.NewLine).Append(Environment.NewLine).Append(Action);
				return sb.ToString();
			}
			return "Unknown";
		}

		
	}
}