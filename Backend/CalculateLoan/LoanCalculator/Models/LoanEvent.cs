namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Models {
	using System;
	using System.Text;
	using Ezbob.Backend.ModelsWithDB.NewLoan;

	public class LoanEvent {

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

		public LoanEvent(DateTime date, NL_LoanFees fee)
			: this(new DateTime(date.Year, date.Month, date.Day, 23, 59, 57)) {
			Fee = fee;
		}

		public LoanEvent(DateTime date, NL_Payments payment, int priority = 0)
			: this(new DateTime(date.Year, date.Month, date.Day, 23, 59, 58), priority) {
			Payment = payment;
		}

		//	installment processed at the end of day
		public LoanEvent(DateTime date, NL_LoanSchedules scheduleItem, int priority = 0): this(new DateTime(date.Year, date.Month, date.Day, 23, 59, 59), priority) {
			Installment = scheduleItem;
		}
		
		public LoanEvent(DateTime date, Action action, int priority = 0)
			: this(date, priority) {
			Console.WriteLine("Action:  {0}==={1}; {2}", date, action, priority);
			Action = action;
		}

		public LoanEvent(DateTime date, int priority = 0) {
			Date = date;
			this.priority = priority;
		}

		public LoanEvent(DateTime date, NL_LoanRollovers rollover)
			: this(date) {
			Rollover = rollover;
		}

		public override string ToString() {
			return string.Format("{0}, {1}", Date, GetTypeString());
		}

		public string GetTypeString() {
			StringBuilder sb = new StringBuilder();
			if (Installment != null) {
				sb.Append("Installment").Append(Environment.NewLine).Append(AStringable.GetHeadersLine(typeof(NL_LoanSchedules)))
				.Append(Installment);
				return sb.ToString();
			}
			if (Payment != null) {
				sb.Append("Payment").Append(Environment.NewLine).Append(AStringable.GetHeadersLine(typeof(NL_Payments))).Append(Payment);
				return sb.ToString();
			}
			if (Fee != null) {
				sb.Append("Fee").Append(Environment.NewLine).Append(AStringable.GetHeadersLine(typeof(NL_LoanFees))).Append(Fee);
				return sb.ToString();
			}
			if (Rollover != null) {
				sb.Append("Rollover").Append(Environment.NewLine).Append(AStringable.GetHeadersLine(typeof(NL_LoanRollovers))).Append(Rollover);
				return sb.ToString();
			}
			if (Action != null) {
				sb.Append("Action").Append(Environment.NewLine).Append(Environment.NewLine).Append(Action);
				return sb.ToString();
			}
			return "Unknown";
		}

		public DateTime Date { get; set; }
		public NL_LoanSchedules Installment { get; set; }
		public NL_Payments Payment { get; set; }
		public NL_LoanFees Fee { get; set; }

		public NL_LoanRollovers Rollover { get; set; } // not supported yet

		public Action Action { get; set; } // not supported yet (re-scheduling, etc)

		
	}
}