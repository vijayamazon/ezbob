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
	using PayPoint;
	using StructureMap;

	public class RescheduleOutLoan<T> : AStrategy {

		public RescheduleOutLoan(T t, ReschedulingArgument reschedulingArgument) {

			this.ReschedulingArguments = reschedulingArgument;
			this.Result = new ReschedulingResult();

			if (t.GetType() == typeof(Loan)) {
				this.tLoan = t as Loan;
			} else if (t.GetType() == typeof(NL_Model)) {
				this.tNLLoan = t as NL_Model;
			}

			this.Result.LoanID = this.ReschedulingArguments.LoanID;
			this.Result.ReschedulingRepaymentIntervalType = this.ReschedulingArguments.ReschedulingRepaymentIntervalType;

			Log.Debug(this.ReschedulingArguments);
		}

		public override string Name { get { return "RescheduleOutLoan"; } }

		/// <exception cref="ReschedulingOutPaymentPerIntervalExcpetion">Condition. </exception>
		public override void Execute() {

			string message;

			if (this.ReschedulingArguments.PaymentPerInterval == null) {
				message = string.Format("No valid monthly/weekly (ReschedulingArguments.PaymentPerInterval) amount sent");
				Log.Alert(message);
				throw new ReschedulingOutPaymentPerIntervalExcpetion(message);
			}

			if (this.tLoan != null) {

				try {

					LoanRepository loanRep = ObjectFactory.GetInstance<LoanRepository>();
					Loan loan = loanRep.Get(this.ReschedulingArguments.LoanID);

					decimal loanBaLance = loan.Balance;

					// loan close date ('maturity date')
					this.ReschedulingArguments.LoanCloseDate = loan.Schedule.OrderBy(s => s.Date).Last().Date;
					this.Result.LoanCloseDate = this.ReschedulingArguments.LoanCloseDate;

					var loanState = new LoanState<Loan>(this.tLoan, this.ReschedulingArguments.LoanID, this.ReschedulingArguments.ReschedulingDate);

					// load loan state in a new calculator format
					loanState.Execute();

					LoanCalculatorModel calcModel = loanState.CalcModel;

					// instance of loan calculator - for current balance
					ALoanCalculator nlCalculator = new LegacyLoanCalculator(calcModel);

					// get current outstanding balance
					this.ReschedulingArguments.ReschedulingBalance = nlCalculator.CalculateBalance(this.ReschedulingArguments.ReschedulingDate, false);

					// TODO - remove the row (overrides balance from the new loan calculator)
					//this.ReschedulingArguments.ReschedulingBalance = this.tLoan.Balance;

					Console.WriteLine("Loan.Balance: {0}, Calc-r balance: {1}", loanBaLance, this.ReschedulingArguments.ReschedulingBalance);

					if (this.ReschedulingArguments.ReschedulingRepaymentIntervalType == RepaymentIntervalTypes.Month)
						this.Result.IntervalsNum = MiscUtils.DateDiffInMonths(this.ReschedulingArguments.ReschedulingDate, this.ReschedulingArguments.LoanCloseDate);

					if (this.ReschedulingArguments.ReschedulingRepaymentIntervalType == RepaymentIntervalTypes.Week)
						this.Result.IntervalsNum = MiscUtils.DateDiffInWeeks(this.ReschedulingArguments.ReschedulingDate, this.ReschedulingArguments.LoanCloseDate);

					// new instance of loan calculator - for new schedules list
					LoanCalculatorModel calculatorModel = new LoanCalculatorModel() {
						LoanIssueTime = this.ReschedulingArguments.ReschedulingDate,
						LoanAmount = this.ReschedulingArguments.ReschedulingBalance,
						RepaymentCount = this.Result.IntervalsNum,
						MonthlyInterestRate = loan.InterestRate,
						InterestOnlyRepayments = 0,
						RepaymentIntervalType = this.ReschedulingArguments.ReschedulingRepaymentIntervalType
					};

					Log.Debug("Calc model for new schedules list: " + calculatorModel);

					ALoanCalculator calculator = new LegacyLoanCalculator(calculatorModel);

					Log.Debug("Intervals for outstanding balance {0} is {1}, loan original close date ('maturity date'): {2}, reschedule date: {3}, interval type: {4}",
							this.ReschedulingArguments.ReschedulingBalance, calculatorModel.RepaymentCount, this.ReschedulingArguments.LoanCloseDate, this.ReschedulingArguments.ReschedulingDate
							, this.ReschedulingArguments.ReschedulingRepaymentIntervalType);

					// populate result object
					this.Result.ReschedulingBalance = calculatorModel.LoanAmount;
					this.Result.IntervalsNum = calculatorModel.RepaymentCount;

					// new schedules
					List<ScheduledItemWithAmountDue> shedules = calculator.CreateScheduleAndPlan(false);

					//shedules.ForEach(s => Log.Debug(s.ToString()));

					// TEMPORARY
					//if (this.ReschedulingArguments.SaveToDB == false)
					//	return;

					// remove future schedules
					List<LoanScheduleItem> removedSchedules = new List<LoanScheduleItem>(loan.Schedule.Where(x => x.Date >= this.ReschedulingArguments.ReschedulingDate));

					foreach (LoanScheduleItem r in removedSchedules) {
						Log.Debug("REMOVED: {0}", r);
						loan.Schedule.Remove(r);
					}

					// mark Late as LateRescheduled - to prevent processing by PayPointCharger
					// Display LateRescheduled as Late everywhere

					foreach (LoanScheduleItem l in loan.Schedule.Where(x => x.Date < this.ReschedulingArguments.ReschedulingDate && x.Status == LoanScheduleStatus.Late)) {
						Log.Debug("PASSED LATE: {0}", l);
						l.Status = LoanScheduleStatus.LateRescheduled;
					}

					// should not be this kind of items - FOR TEST ONLY!!!!!!!!!!!!!!!!!!!!
					foreach (LoanScheduleItem l in loan.Schedule.Where(x => x.Date < this.ReschedulingArguments.ReschedulingDate && x.Status == LoanScheduleStatus.StillToPay)) {
						Log.Debug("PASSED OPENED: {0}", l);
						l.Status = LoanScheduleStatus.StillToPayRescheduled;
					}

					// add new schedules
					foreach (ScheduledItemWithAmountDue s in shedules) {
						LoanScheduleItem item = new LoanScheduleItem() {
							Date = s.Date,
							LoanRepayment = Math.Round(s.Principal, 2),  // p in schedule
							Interest = Math.Round(s.AccruedInterest, 2),
							Status = LoanScheduleStatus.StillToPay
						};
						item.AmountDue = Math.Round(s.Principal + item.Interest + item.Fees);
						loan.Schedule.Add(item);
					}

					Log.Debug("===========================================>Loan2: \n {0}", loan);

					// save modifications to DB
					// removed - will be deleted;
					// passed late - status changed
					// new schedules - will be inserted;
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