namespace Ezbob.Backend.Strategies.NewLoan {
	using Ezbob.Backend.Strategies.NewLoan.Exceptions;

	public class AcceptRollover : AStrategy {

		public AcceptRollover() {

			if (Context.CustomerID == null || Context.CustomerID == 0) {
				this.Error = NL_ExceptionCustomerNotFound.DefaultMessage;
				NL_AddLog(LogType.Error, "Strategy Faild", this.strategyArgs, null, this.Error, null);
				return;
			}

			CustomerID = (int)Context.CustomerID;

			this.strategyArgs = new object[] { CustomerID, Context.UserID };
		}

		public override string Name { get { return "AcceptRollover"; } }

		public int CustomerID { get; private set; }
		public string Error;

		private readonly object[] strategyArgs;

		public override void Execute() {

			// TODO EZ-4330
			/*
			1. records payment for rollover (open interest till accept day included + roolover fees assigned) - call AddPayment strategy
			2. update existing rollover opportunity record: [CustomerActionTime], [IsAccepted] in [dbo].[NL_LoanRollovers] table
			3. make rollover using calculator - add to NL model new history, rearrange schedules, statuses
			4. update DB: records new history, new schedule items; update previous schedule with appropriate statuses
			*/
		}

	} // class AcceptRollover
} // ns