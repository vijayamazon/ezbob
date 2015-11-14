﻿namespace Ezbob.Backend.CalculateLoan.LoanCalculator {
	using System;
	using Ezbob.Backend.ModelsWithDB.NewLoan;

	internal class LoanEvent {

		// default ctor
		public LoanEvent(DateTime date, int priority = 0) {
			EventTime = date;
			this.priority = priority;
		}
		// history
		public LoanEvent(DateTime date, NL_LoanHistory history, int priority = 0)
			: this(new DateTime(date.Year, date.Month, date.Day, 00, 00, 00), priority) {
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
			ScheduleItem = scheduleItem;
		}
		// rollover
		public LoanEvent(DateTime date, NL_LoanRollovers rollover): this(date) {
			Rollover = rollover;
		}

		// action
		public LoanEvent(DateTime date, Action action, int priority = 0): this(date, priority) {
			Action = action;
		}

		public DateTime EventTime { get; set; }
		
		public NL_LoanHistory History { get; set; }
		public NL_LoanFees Fee { get; set; }
		public NL_Payments Payment { get; set; }
		public NL_LoanSchedules ScheduleItem { get; set; }
		public NL_LoanRollovers Rollover { get; set; } // not supported yet
		public Action Action { get; set; } // not supported yet (re-scheduling, etc)

		public NL_LoanHistory CurrentHistory { get; set; }

		public decimal EarnedInterestForPeriod { get; set; }

		private readonly int priority;

		public int Priority {
			get {
				if (this.priority != 0)
					return this.priority;

				if (Fee != null)
					return 1;
				if (Payment != null)
					return 2;
				if (ScheduleItem != null)
					return 3;
				if (Action != null)
					return 4;

				return 0;
			}
		}

		public override string ToString() {
			return string.Format("{0}, {1}", EventTime, GetTypeString());
		}

		public string GetTypeString() {
			if (History != null)
				return "History";

			if (ScheduleItem != null)
				return "ScheduleItem";

			if (Payment != null)
				return "Payment";

			if (Fee != null)
				return "Fee";

			if (Rollover != null)
				return "Rollover";

			if (Action != null)
				return "Action";

			return "Unknown";
		}


	}
}