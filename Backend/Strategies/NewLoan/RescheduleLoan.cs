namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DbConstants;
	using Ezbob.Backend.CalculateLoan.LoanCalculator;
	using Ezbob.Backend.CalculateLoan.Models;
	using Ezbob.Backend.CalculateLoan.Models.Helpers;
	using Ezbob.Backend.Models.NewLoan;
	using Ezbob.Utils;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using StructureMap;

	public class RescheduleLoan<T> : AStrategy {

		public RescheduleLoan(T t, ReschedulingArgument reschedulingArgument) {

			this.ReschedulingArguments = reschedulingArgument;
			this.Result = new ReschedulingResult();

			if (t.GetType() == typeof(Loan)) {
				this.tLoan = t as Loan;
				this.tNLLoan = null;
			} else if (t.GetType() == typeof(NL_Model)) {
				this.tNLLoan = t as NL_Model;
				this.tLoan = null;
			}

			this.Result.LoanID = this.ReschedulingArguments.LoanID;
			this.Result.ReschedulingRepaymentIntervalType = this.ReschedulingArguments.ReschedulingRepaymentIntervalType;

			Log.Debug(this.ReschedulingArguments);
		}

		public override string Name { get { return "RescheduleLoan"; } }
		// input
		public ReschedulingArgument ReschedulingArguments;
		// output
		public ReschedulingResult Result;

		/// <exception cref="ReschedulingOutPaymentPerIntervalException">Condition. </exception>
		public override void Execute() {

			if (!this.ReschedulingArguments.RescheduleIn && this.ReschedulingArguments.PaymentPerInterval == null) {
				this.message = string.Format("Weekly/monthly payment amount for OUT rescheduling not provided");
				Log.Info(this.message);
				throw new ReschedulingOutPaymentPerIntervalException(this.message);
			}

			try {

				GetCurrentLoanBalance();

				//  if Re date is later than loan "maturity" date - "IN" rescheduling can't be proceeded
				if (this.ReschedulingArguments.RescheduleIn && (this.ReschedulingArguments.ReschedulingDate > this.Result.LoanCloseDate)) {
					this.message = string.Format("Re-scheduling date {0} within loan period (maturity date) {1}", this.ReschedulingArguments.ReschedulingDate, this.Result.LoanCloseDate);
					Log.Info(this.message);
					throw new ReschedulingInPeriodException(this.message);
				}

				if (this.ReschedulingArguments.RescheduleIn) {

					// 3. intervals number
					this.Result.IntervalsNum = MiscUtils.DateDiffInMonths(this.ReschedulingArguments.ReschedulingDate, this.Result.LoanCloseDate);
					this.Result.IntervalsNumWeeks = MiscUtils.DateDiffInWeeks(this.ReschedulingArguments.ReschedulingDate, this.Result.LoanCloseDate);

				} else {	// OUT

					// 3. intervals number
					this.Result.IntervalsNum = (int)Math.Ceiling((decimal)(this.Result.ReschedulingBalance / this.ReschedulingArguments.PaymentPerInterval));
					this.Result.IntervalsNumWeeks = MiscUtils.DateDiffInWeeks(this.ReschedulingArguments.ReschedulingDate, this.ReschedulingArguments.ReschedulingDate.AddMonths(this.Result.IntervalsNum));
				}

				if (!this.ReschedulingArguments.SaveToDB)
					return;

				// 4. new schedules list 
				// new instance of loan calculator - for new schedules list
				LoanCalculatorModel calculatorModel = new LoanCalculatorModel() {
					LoanIssueTime = this.ReschedulingArguments.ReschedulingDate,
					LoanAmount = this.Result.ReschedulingBalance,
					RepaymentCount = (this.ReschedulingArguments.ReschedulingRepaymentIntervalType == RepaymentIntervalTypes.Month)?this.Result.IntervalsNum:this.Result.IntervalsNumWeeks,
					MonthlyInterestRate = this.tLoan.InterestRate,
					InterestOnlyRepayments = 0,
					RepaymentIntervalType = this.ReschedulingArguments.ReschedulingRepaymentIntervalType
				};

				if (this.ReschedulingArguments.RescheduleIn) {
					Log.Debug("'IN' rescheduling : Intervals for outstanding balance {0} is {1}, loan original close date ('maturity date'): {2}, reschedule date: {3}, interval type: {4}",
						this.ReschedulingArguments.ReschedulingBalance, calculatorModel.RepaymentCount, this.ReschedulingArguments.LoanCloseDate, this.ReschedulingArguments.ReschedulingDate
						, this.ReschedulingArguments.ReschedulingRepaymentIntervalType);

				} else {
					Log.Debug("'OUT' rescheduling: Intervals for outstanding balance {0} is {1}, loan original close date ('maturity date'): {2}, reschedule date: {3}, interval type: {4}, payment per interval: {5}",
						this.ReschedulingArguments.ReschedulingBalance, calculatorModel.RepaymentCount, this.ReschedulingArguments.LoanCloseDate, this.ReschedulingArguments.ReschedulingDate
						, this.ReschedulingArguments.ReschedulingRepaymentIntervalType
						, this.ReschedulingArguments.PaymentPerInterval);
				}

				ALoanCalculator calculator = new LegacyLoanCalculator(calculatorModel);
				// new schedules
				List<ScheduledItemWithAmountDue> shedules = calculator.CreateScheduleAndPlan();

				this.Result.RescheduledLoanCloseDate = shedules.OrderBy(s => s.Date).Last().Date;

				LoanRescheduleSave(shedules);
				NLRescheduleSave(shedules);

			} catch (Exception e) {
				Log.Alert(e, "Failed to get rescheduling data for loan {0}", this.ReschedulingArguments.LoanID);
				Console.WriteLine(e);
			}
		}

		private void GetCurrentLoanBalance() {

			if (this.tLoan != null) {

				LoanRepository loanRep = ObjectFactory.GetInstance<LoanRepository>();
				this.tLoan = loanRep.Get(this.ReschedulingArguments.LoanID);

				decimal loanBaLance = this.tLoan.Balance;

				this.Result.LoanInterestRate = this.tLoan.InterestRate;

				// loan close date ('maturity date')
				this.Result.LoanCloseDate = this.tLoan.Schedule.OrderBy(s => s.Date).Last().Date;
		
				var loanState = new LoanState<Loan>(this.tLoan, this.ReschedulingArguments.LoanID, this.ReschedulingArguments.ReschedulingDate);
				// 1. calculator model
				// load loan state in a new calculator format
				loanState.Execute();

				LoanCalculatorModel calcModel = loanState.CalcModel;
				// 2. get current balance
				// instance of loan calculator - for current balance
				ALoanCalculator nlCalculator = new LegacyLoanCalculator(calcModel);

				// get current outstanding balance
				this.Result.ReschedulingBalance = nlCalculator.CalculateBalance(this.ReschedulingArguments.ReschedulingDate, false);

				// TODO - remove the row (overrides balance from the new loan calculator)
				// this.Result.ReschedulingBalance =   this.tLoan.Balance;

				Console.WriteLine("Loan.Balance: {0}, Calc-r balance: {1}", loanBaLance, this.Result.ReschedulingBalance);

				return;
			}

			if (this.tNLLoan != null) {
				// new loan structure TODO
				Log.Debug("NEW LOAN STRUCTURE NOT IMPLEMENTED YET");
			}
		}


		/// <summary>
		/// Saving rescheduling results for Loan ("old" loan)
		/// </summary>
		/// <param name="shedules"></param>
		private void LoanRescheduleSave(List<ScheduledItemWithAmountDue> shedules) {

			if (this.ReschedulingArguments.SaveToDB == false)
				return;

			//shedules.ForEach(s => Log.Debug(s.ToString()));

			if (this.tLoan != null) {
				
				/*	
				 * LoanChangesHistory - saving loan state in json - before schedule modifications in EditLoanController 
				 * 
				 * var historyItem = new LoanChangesHistory {
						Data = _loanModelBuilder.BuildModel(loan).ToJSON(),
						Date = DateTime.UtcNow,
						Loan = loan,
						User = _context.User
					};
					_history.Save(historyItem);*/

				// DB update/replace
				// remove future schedules
				List<LoanScheduleItem> removedSchedules = new List<LoanScheduleItem>(this.tLoan.Schedule.Where(x => x.Date >= this.ReschedulingArguments.ReschedulingDate));

				foreach (LoanScheduleItem r in removedSchedules) {
					Log.Debug("REMOVED: {0}", r);
					this.tLoan.Schedule.Remove(r);
				}

				// mark Late as LateRescheduled - to prevent processing by PayPointCharger
				// Display LateRescheduled as Late everywhere

				foreach (LoanScheduleItem l in this.tLoan.Schedule.Where(x => x.Date < this.ReschedulingArguments.ReschedulingDate && x.Status == LoanScheduleStatus.Late)) {
					Log.Debug("PASSED LATE: {0}", l);
					l.Status = LoanScheduleStatus.LateRescheduled;
				}

				// should not be this kind of items - FOR TEST ONLY!!!!!!!!!!!!!!!!!!!!
				foreach (LoanScheduleItem l in this.tLoan.Schedule.Where(x => x.Date < this.ReschedulingArguments.ReschedulingDate && x.Status == LoanScheduleStatus.StillToPay)) {
					Log.Debug("PASSED OPENED: {0}", l);
					l.Status = LoanScheduleStatus.StillToPayRescheduled;
				}

				// add new schedules
				foreach (ScheduledItemWithAmountDue s in shedules) {
					LoanScheduleItem item = new LoanScheduleItem() {
						Date = s.Date,
						LoanRepayment = Math.Round(s.Principal, 2), // p in schedule
						Interest = Math.Round(s.AccruedInterest, 2),
						Status = LoanScheduleStatus.StillToPay
					};
					item.AmountDue = Math.Round(s.Principal + item.Interest + item.Fees);
					this.tLoan.Schedule.Add(item);
				}

				Log.Debug("===========================================>Loan2: \n {0}", this.tLoan);

				// save modifications to DB
				// removed - will be deleted;
				// passed late - status changed
				// new schedules - will be inserted;
				//	loanRep.SaveOrUpdate(loan);
			}
		}

		/// <summary>
		/// Saving rescheduling results for NL ("new" loan)
		/// </summary>
		/// <param name="shedules"></param>
		private void NLRescheduleSave(List<ScheduledItemWithAmountDue> shedules) {

			if (this.ReschedulingArguments.SaveToDB == false)
				return;

			if (this.tNLLoan != null) {
				//TODO
			}
		}

		private Loan tLoan;
		private readonly NL_Model tNLLoan;

		private string message;
	}
}