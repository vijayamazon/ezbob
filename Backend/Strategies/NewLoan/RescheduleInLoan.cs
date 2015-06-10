namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Backend.CalculateLoan.LoanCalculator;
	using Ezbob.Backend.CalculateLoan.Models;
	using Ezbob.Backend.CalculateLoan.Models.Helpers;
	using Ezbob.Backend.Models.NewLoan;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using StructureMap;

	public class RescheduleInLoan<T> : AStrategy {

		public RescheduleInLoan(T t, ReschedulingArgument reschedulingArgument) {

			this.ReschedulingArguments = reschedulingArgument;
			this.Result = new ReschedulingResult();

			if (t.GetType() == typeof(Loan)) {
				this.tLoan = t as Loan;
			} else if (t.GetType() == typeof(NL_Model)) {
				this.tNLLoan = t as NL_Model;
			}

			this.Result.LKind = t.GetType();
			this.Result.LoanID = this.ReschedulingArguments.LoanID;
			this.Result.ReschedulingRepaymentIntervalType = this.ReschedulingArguments.ReschedulingRepaymentIntervalType;

			Console.WriteLine(t.GetType());
		}

		public override string Name { get { return "RescheduleInLoan"; } }

		public override void Execute() {

			if (this.tLoan != null) {

				try {

					LoanRepository loanRep = ObjectFactory.GetInstance<LoanRepository>();
					Loan loan = loanRep.Get(this.ReschedulingArguments.LoanID);

					this.ReschedulingArguments.LoanCloseDate = loan.Schedule.OrderBy(s => s.Date)
						.Last()
						.Date;

					if (this.ReschedulingArguments.ReschedulingDate > this.ReschedulingArguments.LoanCloseDate) {
						Log.Info("Re-scheduling date {0} outside of loan period (maturity date) {1}", this.ReschedulingArguments.ReschedulingDate, this.ReschedulingArguments.LoanCloseDate);
						return;
					}

					var strategyLoanState = new LoanState<Loan>(this.tLoan, this.ReschedulingArguments.LoanID);

					// load loan state in a new calculator format
					strategyLoanState.Execute();
					LoanCalculatorModel calcModel = strategyLoanState.CalcModel;

					// new calc-r
					ALoanCalculator nlCalculator = new LegacyLoanCalculator(calcModel);

					// get outstanding balance for rescheduling
					this.ReschedulingArguments.ReschedulingBalance = nlCalculator.BalanceForRescheduling(this.ReschedulingArguments);

					this.Result.ReschedulingBalance = (decimal)this.ReschedulingArguments.ReschedulingBalance;

					Log.Debug(this.ReschedulingArguments);

					// get new schedules
					List<ScheduledItemWithAmountDue> newSchedules = nlCalculator.RescheduleToIntervals(this.ReschedulingArguments);

					newSchedules.ForEach(s => Log.Debug(s.ToString()));

					this.Result.IntervalsNum = newSchedules.Count;

					if (this.ReschedulingArguments.SaveToDB == false)
						return;

					//	Log.Debug("===========================================>Loan1: \n {0}", loan);

					List<LoanScheduleItem> removedSchedules = new List<LoanScheduleItem>(loan.Schedule.Where(x => x.Date >= this.ReschedulingArguments.ReschedulingDate));

					foreach (LoanScheduleItem r in removedSchedules) {
						Log.Debug("REMOVED: {0}", r);
						loan.Schedule.Remove(r);
					}

					foreach (ScheduledItemWithAmountDue s in newSchedules) {
						Log.Debug(s.ToString());
						LoanScheduleItem item = new LoanScheduleItem();
						item.Date = s.Date;
						item.LoanRepayment = s.Principal; // p in schedule
						item.Interest = s.Principal * s.InterestRate;
						//		item.Fees = s.Fee;
						item.AmountDue = s.Principal + item.Interest + item.Fees;
						item.Status = LoanScheduleStatus.StillToPay;
						loan.Schedule.Add(item);
					}

					Log.Debug("===========================================>Loan2: \n {0}", loan);

					// save modifications to DB
					//	loanRep.SaveOrUpdate(loan);

				} catch (Exception e) {
					Log.Alert(e, "Failed to get rescheduling data for loan {0}", this.ReschedulingArguments.LoanID);
					Console.WriteLine(e);
				}
			}

			// new loan structure TODO
			if (this.tNLLoan != null) {
				Log.Debug("NEW LOAN STRUCTURE NOT IMPLEMENTED YET");
			}

		}


		private readonly Loan tLoan;
		private readonly NL_Model tNLLoan;

		// input
		public ReschedulingArgument ReschedulingArguments;

		// output
		public ReschedulingResult Result;
	}


}