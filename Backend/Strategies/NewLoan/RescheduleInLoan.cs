namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.CalculateLoan.LoanCalculator;
	using Ezbob.Backend.CalculateLoan.Models;
	using Ezbob.Backend.CalculateLoan.Models.Helpers;
	using Ezbob.Backend.Models.NewLoan;
	using EZBob.DatabaseLib.Model.Database.Loans;

	public class RescheduleInLoan<T> : AStrategy {

		public RescheduleInLoan(T t, ReschedulingArgument reschedulingArgument) {
			
			this.ReschedulingArguments = reschedulingArgument;

			if (t.GetType() == typeof(Loan)) {
				this.tLoan = t as Loan;
			} else if (t.GetType() == typeof(NL_Model)) {
				this.tNLLoan = t as NL_Model;
			}

			Console.WriteLine(t.GetType());
		}

		public override string Name { get { return "RescheduleInLoan"; } }

		public override void Execute() {

			if (this.tLoan != null) {

				var strategyLoanState = new LoanState<Loan>(new Loan(), this.ReschedulingArguments.LoanID);
				
				try {

					// load loan state in a new calculator format
					strategyLoanState.Execute();
					LoanCalculatorModel calcModel = strategyLoanState.CalcModel;

					// new calc-r
					ALoanCalculator nlCalculator = new LegacyLoanCalculator(calcModel);

					// get outstanding balance for rescheduling
					this.ReschedulingArguments.ReschedulingBalance = nlCalculator.BalanceForRescheduling(this.ReschedulingArguments);

					// get new schedules
					List<ScheduledItemWithAmountDue> newSchedules = nlCalculator.RescheduleToIntervals(this.ReschedulingArguments);

					// display
					if(this.ReschedulingArguments.SaveToDB == false)
						return;

					// save new schedules to DB
					foreach (ScheduledItemWithAmountDue s in newSchedules) {
						Log.Debug(s.ToString());
					}

				} catch (Exception e) {
					Console.WriteLine(e);
				}
			}

			// new loan structure
			if (this.tNLLoan != null) { }

		}


		private readonly Loan tLoan;
		private readonly NL_Model tNLLoan;

		// result
		public ReschedulingArgument ReschedulingArguments;


	}


}