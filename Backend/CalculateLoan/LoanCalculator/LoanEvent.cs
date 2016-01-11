namespace Ezbob.Backend.CalculateLoan.LoanCalculator {
	using System;
	using Ezbob.Backend.ModelsWithDB.NewLoan;

	internal class LoanEvent {

		// default ctor
		public LoanEvent(DateTime date, int priority = 0) {
			EventTime = date;
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
		public LoanEvent(DateTime date, NL_Payments payment, int priority = 0, bool chargeBackPayment = false, bool chargeBackPaymentRecorded = false )
			: this(new DateTime(date.Year, date.Month, date.Day, 23, 59, 58), priority) {

			if (chargeBackPayment) {
				if (chargeBackPaymentRecorded) {
					ChargebackPaymentRecorded = payment;
					return;
				}
				ChargebackPaymentCancelled = payment;
				return;
			}
			
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
		
		public NL_Payments ChargebackPaymentRecorded { get; set; }
		public NL_Payments ChargebackPaymentCancelled { get; set; }

		public NL_LoanSchedules ScheduleItem { get; set; }
		public NL_LoanRollovers Rollover { get; set; } 
		public Action Action { get; set; }

		public NL_LoanHistory CurrentHistory { get; internal set; }

		public decimal OpenPrincipalForPeriod { get; internal set; }

		public decimal EarnedInterestForPeriod { get; internal set; }

		private readonly int priority;

		private object _currentID;

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
				if (ChargebackPaymentRecorded != null)
					return 5;
				if (ChargebackPaymentCancelled != null)
					return 6;
				return 0;
			}
		}

		public object GetTypeID() {
			if (History != null) {
				this._currentID = History.LoanHistoryID;
			}

			if (ScheduleItem != null) {
				this._currentID = ScheduleItem.LoanScheduleID;
			}

			if (Payment != null) {
				this._currentID = Payment.PaymentID;
			}

			if (ChargebackPaymentRecorded != null) {
				this._currentID = ChargebackPaymentRecorded.PaymentID;
			}

			if (ChargebackPaymentCancelled != null){
				this._currentID = ChargebackPaymentCancelled.PaymentID;
			}

			if (Fee != null){
				this._currentID = Fee.LoanFeeID;return this._currentID;
			}

			if (Rollover != null) {
				this._currentID = Rollover.LoanRolloverID;
			}

			return this._currentID;
		}

		public override string ToString() {
			return string.Format("EventTime={0}, EventType={1}, [ID]={2}, OpenPrincipalForPeriod={3} EarnedInterestForPeriod={4}", EventTime, GetTypeString(), GetTypeID(), OpenPrincipalForPeriod, EarnedInterestForPeriod); 
		}

		public string GetTypeString() {
			if (History != null)
				return "History";

			if (ScheduleItem != null)
				return "ScheduleItem";

			if (Payment != null)
				return "Payment";

			if (ChargebackPaymentRecorded != null)
				return "ChargebackPaymentRecorded";

			if (ChargebackPaymentCancelled != null)
				return "ChargebackPaymentCancelled";

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