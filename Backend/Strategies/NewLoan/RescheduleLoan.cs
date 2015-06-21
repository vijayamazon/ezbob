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

				this.loanRep = ObjectFactory.GetInstance<LoanRepository>();

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

					//decimal k = Math.Ceiling((A + F) / (m - (A + F) * r));

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
					Log.Debug("'IN' rescheduling : Intervals for outstanding balance {0} is {1}, loan original close date ('maturity date'): {2}, reschedule date: {3}, interval type: {4}",
						this.Result.ReschedulingBalance, calculatorModel.RepaymentCount, this.Result.LoanCloseDate, this.ReschedulingArguments.ReschedulingDate
						, this.ReschedulingArguments.ReschedulingRepaymentIntervalType);

				} else {
					Log.Debug("'OUT' rescheduling: Intervals for outstanding balance {0} is {1}, loan original close date ('maturity date'): {2}, reschedule date: {3}, interval type: {4}, payment per interval: {5}",
						this.Result.ReschedulingBalance, calculatorModel.RepaymentCount, this.Result.LoanCloseDate, this.ReschedulingArguments.ReschedulingDate
						, this.ReschedulingArguments.ReschedulingRepaymentIntervalType
						, this.ReschedulingArguments.PaymentPerInterval);

					//calculatorModel.Fees = this.CalcModel.Fees;
				}

				ALoanCalculator calculator = new LegacyLoanCalculator(calculatorModel);
				// new schedules
				List<ScheduledItemWithAmountDue> shedules = calculator.CreateScheduleAndPlan();

				this.Result.RescheduledLoanCloseDate = shedules.OrderBy(s => s.Date).Last().Date;

				LoanRescheduleSave(shedules);
				NLRescheduleSave(shedules);

			} catch (Exception e) {
				Log.Alert(e, "Failed to get rescheduling data for loan {0}", this.ReschedulingArguments.LoanID);
				//Console.WriteLine(e);
			}
		}

		private void GetCurrentLoanBalance() {

			if (this.tLoan != null) {

				this.tLoan = this.loanRep.Get(this.ReschedulingArguments.LoanID);

				decimal loanBaLance = this.tLoan.Balance;

				this.Result.LoanInterestRate = this.tLoan.InterestRate;

				// loan close date ('maturity date')
				this.Result.LoanCloseDate = this.tLoan.Schedule.OrderBy(s => s.Date).Last().Date;

				var loanState = new LoanState<Loan>(this.tLoan, this.ReschedulingArguments.LoanID, this.ReschedulingArguments.ReschedulingDate);
				// 1. calculator model
				// load loan state in a new calculator format
				loanState.Execute();

				this.CalcModel = loanState.CalcModel;
				// 2. get current balance
				// instance of loan calculator - for current balance
				ALoanCalculator nlCalculator = new LegacyLoanCalculator(this.CalcModel);

				// get current outstanding balance
				this.Result.ReschedulingBalance = nlCalculator.CalculateBalance(this.ReschedulingArguments.ReschedulingDate, false);

				// TODO - remove the row (overrides balance from the new loan calculator)
				// this.Result.ReschedulingBalance =   this.tLoan.Balance;

			//	Console.WriteLine("Loan.Balance: {0}, Calc-r balance: {1}", loanBaLance, this.Result.ReschedulingBalance);

				// future unpaid fees
				this.Result.Fees = this.CalcModel.Fees
					//.Where(f => f.AssignDate <= this.ReschedulingArguments.ReschedulingDate)
					.Sum(f => f.Amount);

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

			if (this.tLoan != null) {

				List<LoanScheduleDeleted> deletedList = new List<LoanScheduleDeleted>();

				foreach (var r in this.tLoan.Schedule.ToList<LoanScheduleItem>()) {
					if (r.Date >= this.ReschedulingArguments.ReschedulingDate)
						this.tLoan.Schedule.Remove(r);
					if (r.Date <= this.ReschedulingArguments.ReschedulingDate && r.Status == LoanScheduleStatus.Late) {
						this.tLoan.Schedule.Remove(r);
						//deletedList.Add(new LoanScheduleDeleted().CloneScheduleItem(r));
						this.tLoan.TryAddRemovedOnReschedule(new LoanScheduleDeleted().CloneScheduleItem(r));
					}
					if (r.Date <= this.ReschedulingArguments.ReschedulingDate && r.Status == LoanScheduleStatus.StillToPay) {
						this.tLoan.Schedule.Remove(r);
						//deletedList.Add(new LoanScheduleDeleted().CloneScheduleItem(r));
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

						// Interest to be repaid  Доход банка от кредита. Имеется в виду, доход, который банк получит после выплаты всего платежа.
						Interest = Decimal.Round(s.AccruedInterest, 2),

						//Sum to be repaid, including principal, interest and fees Сумма к оплате по кредиту, включающая выплату по телу, проценты и комисии
						//AmountDue = Decimal.Round((s.Principal + s.AccruedInterest), 2),

						Status = LoanScheduleStatus.StillToPay,
						Loan = this.tLoan,
						Fees = 0.0000m,

					};

					//item.AmountDue = (s.Principal + s.AccruedInterest + item.Fees);

					this.tLoan.Schedule.Add(item);
				}

				Log.Debug(">>>>>>>>>>>>>>>>>>Loan modified schedules: \n {0}", this.tLoan);

				try {
					this.loanRep.BeginTransaction();
					this.loanRep.EnsureTransaction(() => {
						this.loanRep.SaveOrUpdate(this.tLoan);
					});
					this.loanRep.CommitTransaction();
				} catch (Exception transactionEx) {
					this.loanRep.RollbackTransaction();
					this.message = string.Format("Re-schedule rolled back. Arguments {0}, err: {1}", this.ReschedulingArguments, transactionEx);
					Log.Alert(this.message);
					this.Result.Error = "ReschedulingSaveException";
				}
			}
		}




		private void bkp_LoanRescheduleSave(List<ScheduledItemWithAmountDue> shedules) {

			if (this.ReschedulingArguments.SaveToDB == false)
				return;

			Log.Debug("NEW SCHEDULE==================");
			shedules.ForEach(s => Log.Debug(s.ToString()));

			if (this.tLoan != null) {

				var currentList = this.tLoan.Schedule;

				// DB update/replace
				// remove future schedules
				List<LoanScheduleItem> removedSchedules = new List<LoanScheduleItem>(this.tLoan.Schedule.Where(x => x.Date >= this.ReschedulingArguments.ReschedulingDate));


				// remove future
				foreach (LoanScheduleItem r in this.tLoan.Schedule.Where(x => x.Date >= this.ReschedulingArguments.ReschedulingDate)) {
					//Log.Debug("REMOVED: {0}", r);
					currentList.Remove(r);
				}

				List<LoanScheduleDeleted> deletedSchedules = new List<LoanScheduleDeleted>();

				var lates = this.tLoan.Schedule.Where(x => x.Date < this.ReschedulingArguments.ReschedulingDate && x.Status == LoanScheduleStatus.Late);

				// remove past LATE
				foreach (LoanScheduleItem l in this.tLoan.Schedule.Where(x => x.Date < this.ReschedulingArguments.ReschedulingDate && x.Status == LoanScheduleStatus.Late)) {
					//Log.Debug("PASSED LATE: {0}", l);
					//l.Status = LoanScheduleStatus.RescheduledLate;
					//deletedSchedules.Add(new LoanScheduleDeleted().CloneScheduleItem(l));
					currentList.Remove(l);
					//this.tLoan.TryAddRemovedOnReschedule(new LoanScheduleDeleted().CloneScheduleItem(l));
				}

				// should not be this kind of items - FOR TEST ONLY!!!!!!!!!!!!!!!!!!!!
				// remove past StillToPay
				foreach (LoanScheduleItem l in this.tLoan.Schedule.Where(x => x.Date < this.ReschedulingArguments.ReschedulingDate && x.Status == LoanScheduleStatus.StillToPay)) {
					//Log.Debug("PASSED OPENED: {0}", l);
					//l.Status = LoanScheduleStatus.RescheduledStillToPay;
					//deletedSchedules.Add(new LoanScheduleDeleted().CloneScheduleItem(l));
					currentList.Remove(l);
					//this.tLoan.TryAddRemovedOnReschedule(new LoanScheduleDeleted().CloneScheduleItem(l));
				}

				//	deletedSchedules.ForEach(d => Log.Debug(d));

				// add new schedules
				foreach (ScheduledItemWithAmountDue s in shedules.OrderBy(pp => pp.Position)) {
					LoanScheduleItem item = new LoanScheduleItem() {
						Date = s.Date,
						// Interest rate for current period Процентная ставка, которая действует(ла) на протажении данного периода.
						InterestRate = Decimal.Round(s.InterestRate, 7),

						// principal to be repaid Выплата по телу кредита
						LoanRepayment = Decimal.Round(s.Principal, 2),

						// Interest to be repaid  Доход банка от кредита. Имеется в виду, доход, который банк получит после выплаты всего платежа.
						Interest = Decimal.Round(s.AccruedInterest, 2),

						//Sum to be repaid, including principal, interest and fees Сумма к оплате по кредиту, включающая выплату по телу, проценты и комисии
						//AmountDue = Decimal.Round((s.Principal + s.AccruedInterest), 2),

						Status = LoanScheduleStatus.StillToPay,
						Loan = this.tLoan,
						Fees = 0.0000m
					};

					//item.AmountDue = (s.Principal + s.AccruedInterest + item.Fees);
					//item.UpdateStatus(this.ReschedulingArguments.ReschedulingDate);
					currentList.Add(item);
				}

				//this.tLoan.RemovedOnReschedule = deletedSchedules;
				this.tLoan.Schedule = currentList;

				Log.Debug(">>>>>>>>>>>>>>>>>>Loan modified: \n {0}", this.tLoan);


				/*try {
					var calc = new LoanRepaymentScheduleCalculator(this.tLoan, this.ReschedulingArguments.ReschedulingDate, CurrentValues.Instance.AmountToChargeFrom);
					calc.GetState();
					Log.Debug(">>>>>>>>>>>>>>>>>>Loan after LoanRepaymentScheduleCalculator.GetState : \n {0}", this.tLoan);
				} catch (Exception e) {
					Log.Debug(e.Message);
				}*/

				// save modifications to DB
				// removed - will be deleted;
				// passed late - status changed
				// new schedules - will be inserted;
				//this.loanRep.SaveOrUpdate(this.tLoan);

				// commented tempraray, don't delete
				// move "deleted" past schedule items to LoanScheduleDeleted table (copy of LoanSchedule)
				//ILoanScheduleDeletedRepository deletedItemsRep = ObjectFactory.GetInstance<LoanScheduleDeletedRepository>();
				//foreach ( LoanScheduleDeleted d in deletedSchedules) {
				//	deletedItemsRep.Delete(d);
				//}
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

		private readonly LoanRepository loanRep;

		private LoanCalculatorModel CalcModel;

		//private string[] customerGoodStatuses = new[] {
		//	 "Enabled"
		//	, "Risky"
		//	,"Bad"
		//	,"1 - 14 days missed"
		//	,"15 - 30 days missed"
		//	,"31 - 45 days missed"
		//	,"46 - 60 days missed"
		//	,"60 - 90 days missed"
		//	,"Collection: Site Visit"
		//};



		/*private string[] customerStatusPreventRescheduling = new [] {
			
Fraud suspect
Fraud 
disabled 
write off 
Legal 
Default 
90+ days missed 
Legal-claim process 
Legal - apply for judgement 
legal-bailliff 
Legal Charging Order 
IVA_CVA
liquidation
Insolvency
Bankruptcy
Legal CCJ 
Debt Management 
Collection: Tracing 
And two recently added to the customer statuses table:
Cust Insolvent Proceed PG
PG Insolvent Proceed Cust*/


	}
}