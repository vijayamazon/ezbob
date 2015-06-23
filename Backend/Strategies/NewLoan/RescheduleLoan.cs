﻿namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Backend.CalculateLoan.LoanCalculator;
	using Ezbob.Backend.CalculateLoan.Models;
	using Ezbob.Backend.CalculateLoan.Models.Helpers;
	using Ezbob.Backend.Models.NewLoan;
	using Ezbob.Utils;
	using EzBob.Models;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using PaymentServices.Calculators;
	using StructureMap;

	public class RescheduleLoan<T> : AStrategy {

		public RescheduleLoan(T t, ReschedulingArgument reschedulingArgument) {

			this.ReschedulingArguments = reschedulingArgument;

			this.Result = new ReschedulingResult();

			if (t.GetType() == typeof(Loan)) {
				this.tLoan = t as Loan;
				this.tNLLoan = null;

				this.loanRep = ObjectFactory.GetInstance<LoanRepository>();

				this.actual = new Loan();
				this.actual = this.loanRep.Get(this.ReschedulingArguments.LoanID);

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


		public override void Execute() {

			if (!this.ReschedulingArguments.RescheduleIn && this.ReschedulingArguments.PaymentPerInterval == null) {
				this.message = string.Format("Weekly/monthly payment amount for OUT rescheduling not provided");
				Log.Info(this.message);
				this.Result.Error = "ReschedulingOutPaymentPerIntervalException";
				return;
			}

			try {

				GetCurrentLoanBalance();

				if (this.ReschedulingArguments.RescheduleIn && (this.ReschedulingArguments.ReschedulingDate > this.Result.LoanCloseDate)) {
					this.message = string.Format("Re-scheduling date {0} within loan period (maturity date) {1}", this.ReschedulingArguments.ReschedulingDate, this.Result.LoanCloseDate);
					Log.Info(this.message);
					this.Result.Error = "ReschedulingInPeriodException";
					return;
				}

				// OVERRIDE new loan calc-r balance by "outstanding principal" !!!!!!!!!!!!!!!!!!!
				this.Result.ReschedulingBalance = this.tLoan.Principal;

				if (this.ReschedulingArguments.RescheduleIn) {

					// 3. intervals number
					this.Result.IntervalsNum = MiscUtils.DateDiffInMonths(this.ReschedulingArguments.ReschedulingDate, this.Result.LoanCloseDate);
					this.Result.IntervalsNumWeeks = MiscUtils.DateDiffInWeeks(this.ReschedulingArguments.ReschedulingDate, this.Result.LoanCloseDate);

				} else {	// OUT

					// 3. intervals number

					//n = A/(m-Ar)

					decimal A = this.Result.ReschedulingBalance;
					decimal m = (decimal)this.ReschedulingArguments.PaymentPerInterval;
					decimal F = this.Result.Fees ?? 0m;
					decimal r = this.Result.LoanInterestRate;

					decimal n = Math.Ceiling(A / (m - A * r));

					decimal k = Math.Ceiling((A + F) / (m - (A + F) * r));

					int months = Math.Abs((int)(n + 1));
					this.Result.LoanCloseDate = this.ReschedulingArguments.ReschedulingDate.AddMonths(months);

					//Console.WriteLine(this.Result.LoanCloseDate);

					this.Result.IntervalsNum = MiscUtils.DateDiffInMonths(this.ReschedulingArguments.ReschedulingDate, this.Result.LoanCloseDate);
					this.Result.IntervalsNumWeeks = MiscUtils.DateDiffInWeeks(this.ReschedulingArguments.ReschedulingDate, this.Result.LoanCloseDate);
				}

				if (!this.ReschedulingArguments.SaveToDB)
					return;

				// 4. new schedules list 
				// new instance of loan calculator - for new schedules list
				LoanCalculatorModel calculatorModel = new LoanCalculatorModel() {
					LoanIssueTime = this.ReschedulingArguments.ReschedulingDate,
					LoanAmount = this.Result.ReschedulingBalance,
					RepaymentCount = (this.ReschedulingArguments.ReschedulingRepaymentIntervalType == RepaymentIntervalTypes.Month) ? this.Result.IntervalsNum : this.Result.IntervalsNumWeeks,
					MonthlyInterestRate = this.Result.LoanInterestRate,
					InterestOnlyRepayments = 0,
					RepaymentIntervalType = this.ReschedulingArguments.ReschedulingRepaymentIntervalType
				};

				if (this.ReschedulingArguments.RescheduleIn) {
					Log.Debug("'IN': Outstanding balance: {0}, Intervals: {1}, loan original close date: {2}, reschedule date: {3}, interval type: {4}",
						this.Result.ReschedulingBalance, calculatorModel.RepaymentCount, this.Result.LoanCloseDate, this.ReschedulingArguments.ReschedulingDate
						, this.ReschedulingArguments.ReschedulingRepaymentIntervalType);

				} else {
					Log.Debug("'OUT': Outstanding balance: {0}, Intervals: {1}, loan original close date: {2}, reschedule date: {3}, interval type: {4}, payment per interval: {5}",
						this.Result.ReschedulingBalance, calculatorModel.RepaymentCount, this.Result.LoanCloseDate, this.ReschedulingArguments.ReschedulingDate
						, this.ReschedulingArguments.ReschedulingRepaymentIntervalType
						, this.ReschedulingArguments.PaymentPerInterval);

					//calculatorModel.Fees = this.CalcModel.Fees;
				}

				ALoanCalculator calculator = new LegacyLoanCalculator(calculatorModel);

				List<ScheduledItemWithAmountDue> shedules = calculator.CreateScheduleAndPlan(false);

				this.Result.RescheduledLoanCloseDate = shedules.OrderBy(s => s.Date).Last().Date;

				LoanRescheduleSave(shedules);

			} catch (Exception e) {
				Log.Alert(e, "Failed to get rescheduling data for loan {0}", this.ReschedulingArguments.LoanID);

			}
		}

		private void GetCurrentLoanBalance() {

			if (this.tLoan != null) {

				this.tLoan = this.loanRep.Get(this.ReschedulingArguments.LoanID);

				this.Result.LoanInterestRate = this.tLoan.InterestRate;

				// loan close date ('maturity date')
				this.Result.LoanCloseDate = this.tLoan.Schedule.OrderBy(s => s.Date).Last().Date;

				Log.Debug("==========================Loanstate: {0}", this.tLoan);

				return;
			}

			//if (this.tNLLoan != null) {
			//	// new loan structure TODO
			//	Log.Debug("NEW LOAN STRUCTURE NOT IMPLEMENTED YET");
			//}
		}


		/// <summary>
		/// Saving rescheduling results for Loan ("old" loan)
		/// </summary>
		/// <param name="shedules"></param>
		private void LoanRescheduleSave(List<ScheduledItemWithAmountDue> shedules) {

			if (this.ReschedulingArguments.SaveToDB == false)
				return;

			if (this.tLoan != null) {

				//Log.Debug("+++++++++++++NEW SCHEDULES+++++++++++");
				//shedules.ForEach(s => Log.Debug(s));

				ChangeLoanDetailsModelBuilder loanModelBuilder = new ChangeLoanDetailsModelBuilder();
				EditLoanDetailsModel model = new EditLoanDetailsModel();
				model = loanModelBuilder.BuildModel(this.actual);

				foreach (var r in this.tLoan.Schedule.ToList<LoanScheduleItem>()) {
					if (r.Date >= this.ReschedulingArguments.ReschedulingDate)
						this.tLoan.Schedule.Remove(r);
					// lates, stilltopays passed
					if (r.Date <= this.ReschedulingArguments.ReschedulingDate && r.Status == LoanScheduleStatus.Late) {
						this.tLoan.Schedule.Remove(r);
						this.tLoan.TryAddRemovedOnReschedule(new LoanScheduleDeleted().CloneScheduleItem(r));
					}
					if (r.Date <= this.ReschedulingArguments.ReschedulingDate && r.Status == LoanScheduleStatus.StillToPay) {
						this.tLoan.Schedule.Remove(r);
						this.tLoan.TryAddRemovedOnReschedule(new LoanScheduleDeleted().CloneScheduleItem(r));
					}
				}

				// add new schedules
				foreach (ScheduledItemWithAmountDue s in shedules.OrderBy(pp => pp.Position)) {
					LoanScheduleItem item = new LoanScheduleItem() {
						Date = s.Date,
						// Interest rate for current period Процентная ставка, которая действует(ла) на протажении данного периода.
						InterestRate = Decimal.Round(s.InterestRate, 7),
						// principal to be repaid Выплата по телу кредита
						LoanRepayment = Decimal.Round(s.Principal, 2),
						Status = LoanScheduleStatus.StillToPay,
						Loan = this.tLoan,
					};
					this.tLoan.Schedule.Add(item);
				}


				try {
					this.loanRep.BeginTransaction();
					this.loanRep.EnsureTransaction(() => {
						this.loanRep.SaveOrUpdate(this.tLoan);
					});
					this.loanRep.CommitTransaction();



					var loaan = this.loanRep.Get(this.tLoan.Id);
					model = loanModelBuilder.BuildModel(loaan);
					var loan = loanModelBuilder.CreateLoan(model);
					try {
						var calc = new LoanRepaymentScheduleCalculator(loan, DateTime.UtcNow, CurrentValues.Instance.AmountToChargeFrom);
						calc.GetState();
						Log.Debug(">>>>>>>>>>>>>>>>>>Loan  modified: \n {0}", loan);
					} catch (Exception e) {
						Console.WriteLine(e);
					}

				} catch (Exception transactionEx) {
					this.loanRep.RollbackTransaction();
					this.message = string.Format("Re-schedule rolled back. Arguments {0}, err: {1}", this.ReschedulingArguments, transactionEx);
					Log.Alert(this.message);
					this.Result.Error = "ReschedulingSaveException";
				}

			}
		}



		private Loan tLoan;

		private Loan actual;

		private readonly NL_Model tNLLoan;

		private string message;

		private readonly LoanRepository loanRep;

		private LoanCalculatorModel CalcModel;

	}
}