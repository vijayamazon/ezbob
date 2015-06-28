namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Backend.Extensions;
	using Ezbob.Backend.Models.NewLoan;
	using Ezbob.Utils;
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
			} else if (t.GetType() == typeof(NL_Model)) {
				this.tNLLoan = t as NL_Model;
				this.tLoan = null;
			}

			this.Result.LoanID = this.ReschedulingArguments.LoanID;
			this.Result.ReschedulingRepaymentIntervalType = this.ReschedulingArguments.ReschedulingRepaymentIntervalType;

			this.cultureInfo = new CultureInfo("en-GB");
		}

		public override string Name { get { return "RescheduleLoan"; } }
		// input
		public ReschedulingArgument ReschedulingArguments;
		// output
		public ReschedulingResult Result;

		public override void Execute() {
			
			if (!this.ReschedulingArguments.RescheduleIn && this.ReschedulingArguments.PaymentPerInterval == null) {
				this.Result.Error = "Weekly/monthly payment amount for OUT rescheduling not provided";
				return;
			}

			Log.Debug("\n\n\n ==================================================================RE-SCHEDULING=============================================" + this.ReschedulingArguments);

			try {

				GetCurrentLoanState();

				// input validation for "IN"
				if (this.ReschedulingArguments.RescheduleIn && (this.ReschedulingArguments.ReschedulingDate > this.Result.LoanCloseDate)) {
					this.Result.Error = "Within loan arrangement is impossible";
					return;
				}

				// remove unpaid (lates, stilltopays passed) and future schedule items
				foreach (var rmv in this.tLoan.Schedule.ToList<LoanScheduleItem>()) {
					if (rmv.Date >= this.ReschedulingArguments.ReschedulingDate)
						this.tLoan.Schedule.Remove(rmv);
					if (rmv.Date <= this.ReschedulingArguments.ReschedulingDate && rmv.Status == LoanScheduleStatus.Late) {
						this.tLoan.Schedule.Remove(rmv);
						this.tLoan.TryAddRemovedOnReschedule(new LoanScheduleDeleted().CloneScheduleItem(rmv));
					}
					if (rmv.Date <= this.ReschedulingArguments.ReschedulingDate && rmv.Status == LoanScheduleStatus.StillToPay) {
						this.tLoan.Schedule.Remove(rmv);
						this.tLoan.TryAddRemovedOnReschedule(new LoanScheduleDeleted().CloneScheduleItem(rmv));
					}
				}

				decimal P = this.tLoan.Principal;
				this.Result.ReschedulingBalance = P;
				decimal F = this.tLoan.Charges.Sum(f => f.Amount);
				decimal r = this.tLoan.InterestRate;
				int periods = 0;
				decimal I = 0m;
				var calc = new LoanRepaymentScheduleCalculator(this.tLoan, DateTime.UtcNow, CurrentValues.Instance.AmountToChargeFrom);

				// 3. intervals number
				if (this.ReschedulingArguments.RescheduleIn) {

					this.Result.IntervalsNum = MiscUtils.DateDiffInMonths(this.ReschedulingArguments.ReschedulingDate, this.Result.LoanCloseDate);
					this.Result.IntervalsNumWeeks = MiscUtils.DateDiffInWeeks(this.ReschedulingArguments.ReschedulingDate, this.Result.LoanCloseDate);

					Log.Debug("IN IntervalsNum: {0}, IntervalsNumWeeks: {1}", this.Result.IntervalsNum, this.Result.IntervalsNumWeeks);

				} else {	// OUT

					// get I - Accrued interest to pay untill Rescheduling date
					DateTime sDate0 = this.ReschedulingArguments.ReschedulingDate; // this.ReschedulingArguments.ReschedulingRepaymentIntervalType == RepaymentIntervalTypes.Month ? this.ReschedulingArguments.ReschedulingDate.AddMonths(1) : this.ReschedulingArguments.ReschedulingDate.AddDays(7);
					DateTime endDate0 = this.ReschedulingArguments.ReschedulingRepaymentIntervalType == RepaymentIntervalTypes.Month ? sDate0.AddMonths(1) : sDate0.AddDays(7);
					decimal interestRate0 = calc.GetInterestRate(sDate0, endDate0);
					this.tLoan.Schedule.Add(new LoanScheduleItem() {
						Date = sDate0.Date,
						InterestRate = interestRate0,
						Status = LoanScheduleStatus.StillToPay,
						Loan = this.tLoan,
						Balance = 0
					});
					calc.GetState();
					I = Decimal.Ceiling(calc.InterestToPay);
					//Log.Debug("-------------------------------I= {0}", I);
					//Log.Debug("---------------------Accrued interest estimaltion : \n {0}", this.tLoan);
					// remove "check accrued interest" payment
					foreach (var rmv in this.tLoan.Schedule.ToList<LoanScheduleItem>()) {
						if (rmv.Date >= this.ReschedulingArguments.ReschedulingDate)
							this.tLoan.Schedule.Remove(rmv);
					}
					// ### get I - Accrued interest to pay untill Rescheduling date

					// ReSharper disable once PossibleInvalidOperationException
					decimal m = (decimal)this.ReschedulingArguments.PaymentPerInterval;
					//int n = (int)Math.Ceiling(P / ((m - P) * r));
					int k = (int)Math.Ceiling((P + I + F) / (m - (P + I + F) * r));
					// unsufficient payment per interval
					if (k < 0) {
						this.message = string.Format("{0}ly payment of {1} is not sufficient to pay the loan outstanding balance. Accrued interest until {2} is {3}, accumulated fees: {4}",
							this.ReschedulingArguments.ReschedulingRepaymentIntervalType, this.ReschedulingArguments.PaymentPerInterval.Value.ToString("C2", this.cultureInfo), this.ReschedulingArguments.ReschedulingDate.Date.DateStr(), I.ToString("C2", this.cultureInfo), calc.FeesToPay.ToString("C2", this.cultureInfo));
						this.Result.Error = this.message;
						return;
					}

					this.Result.LoanCloseDate = this.ReschedulingArguments.ReschedulingDate.AddMonths((k + 1));

					this.Result.IntervalsNum = MiscUtils.DateDiffInMonths(this.ReschedulingArguments.ReschedulingDate, this.Result.LoanCloseDate);
					this.Result.IntervalsNumWeeks = MiscUtils.DateDiffInWeeks(this.ReschedulingArguments.ReschedulingDate, this.Result.LoanCloseDate);

					Log.Debug("OUT PaymentPerInterval: {0}, k: {1}", this.ReschedulingArguments.PaymentPerInterval, k);
				}

				periods = this.ReschedulingArguments.ReschedulingRepaymentIntervalType == RepaymentIntervalTypes.Month ? this.Result.IntervalsNum : this.Result.IntervalsNumWeeks;

				decimal newInstallmentPrincipal =  Decimal.Ceiling((P) / periods);

				Log.Debug("P: {0}; newInstallmentPrincipal: {1}, periods: {2}, I: {3}", P, newInstallmentPrincipal, periods, I);
	
				decimal balance = 0;
				for (int j = periods; j > 0; j--) {
					DateTime sDate = this.ReschedulingArguments.ReschedulingRepaymentIntervalType == RepaymentIntervalTypes.Month ? this.ReschedulingArguments.ReschedulingDate.AddMonths(j) : this.ReschedulingArguments.ReschedulingDate.AddDays(j*7);
					DateTime endDate = this.ReschedulingArguments.ReschedulingRepaymentIntervalType == RepaymentIntervalTypes.Month ? sDate.AddMonths(1) : sDate.AddDays(7);
					decimal interestRate = calc.GetInterestRate(sDate, endDate);
					Log.Debug("Start: {0}, End: {1}, Rate: {2}", sDate, endDate, interestRate);
					LoanScheduleItem item = new LoanScheduleItem() {
						Date = sDate.Date,
						InterestRate = interestRate,
						Status = LoanScheduleStatus.StillToPay,
						Loan = this.tLoan,
						Balance = balance
					};
					this.tLoan.Schedule.Add(item);
					balance += Decimal.Ceiling(newInstallmentPrincipal + interestRate * newInstallmentPrincipal);
				}
				Log.Debug(">>>>>>>>>>>>>>>>>>Loan modified: \n {0}", this.tLoan);

				calc.GetState();

				Log.Debug("--------------Loan recalculated: \n {0}", this.tLoan);

				if (!this.ReschedulingArguments.RescheduleIn) {
					// check "too much" payment per interval
					List<LoanScheduleItem> paidEarlyInstallment = this.tLoan.Schedule.Where(s => s.Date.Date >= this.ReschedulingArguments.ReschedulingDate && s.Status == LoanScheduleStatus.PaidEarly).ToList();
					int paidEarlyCount = paidEarlyInstallment.Count;
					if (paidEarlyCount > 0) {
						this.message = string.Format("{0}ly payment of {1} GBP more than minimum, needed for paying a loan. {2} future installments of new schedule registered as \"paid early\"",
							// ReSharper disable once PossibleInvalidOperationException
							this.ReschedulingArguments.ReschedulingRepaymentIntervalType, this.ReschedulingArguments.PaymentPerInterval.Value.ToString("C2", this.cultureInfo), paidEarlyCount);
						this.Result.Error = this.message;
						return;
					}
				}

				if (!this.ReschedulingArguments.SaveToDB)
					return;

				// FOR NL ONLY
				// 4. new schedules list 
				// new instance of loan calculator - for new schedules list
				//LoanCalculatorModel calculatorModel = new LoanCalculatorModel() {
				//	LoanIssueTime = this.ReschedulingArguments.ReschedulingDate,
				//	LoanAmount = this.Result.ReschedulingBalance,
				//	RepaymentCount = (this.ReschedulingArguments.ReschedulingRepaymentIntervalType == RepaymentIntervalTypes.Month) ? this.Result.IntervalsNum : this.Result.IntervalsNumWeeks,
				//	MonthlyInterestRate = this.Result.LoanInterestRate,
				//	InterestOnlyRepayments = 0,
				//	RepaymentIntervalType = this.ReschedulingArguments.ReschedulingRepaymentIntervalType
				//};
				//if (this.ReschedulingArguments.RescheduleIn) {
				//	Log.Debug("================='IN': Outstanding balance: {0}, Intervals: {1}, loan original close date: {2}, reschedule date: {3}, interval type: {4}",
				//		this.Result.ReschedulingBalance, calculatorModel.RepaymentCount, this.Result.LoanCloseDate, this.ReschedulingArguments.ReschedulingDate
				//		, this.ReschedulingArguments.ReschedulingRepaymentIntervalType);
				//} else {
				//	Log.Debug("==============='OUT': Outstanding balance: {0}, Intervals: {1}, loan original close date: {2}, reschedule date: {3}, interval type: {4}, payment per interval: {5}",
				//		this.Result.ReschedulingBalance, calculatorModel.RepaymentCount, this.Result.LoanCloseDate, this.ReschedulingArguments.ReschedulingDate
				//		, this.ReschedulingArguments.ReschedulingRepaymentIntervalType
				//		, this.ReschedulingArguments.PaymentPerInterval);
				//	//calculatorModel.Fees = this.CalcModel.Fees;
				//}

				//ALoanCalculator calculator = new LegacyLoanCalculator(calculatorModel);
				//List<ScheduledItemWithAmountDue> shedules = calculator.CreateScheduleAndPlan(false);

				LoanRescheduleSave();

			} catch (Exception e) {
				Log.Alert(e, "Failed to get rescheduling data for loan {0}", this.ReschedulingArguments.LoanID);

			}
		}



		private void GetCurrentLoanState() {
			if (this.tLoan != null) {

				this.tLoan = this.loanRep.Get(this.ReschedulingArguments.LoanID);
				this.Result.LoanInterestRate = this.tLoan.InterestRate;
				var lastScheduleItem = this.tLoan.Schedule.OrderBy(s => s.Date).LastOrDefault();
				if (lastScheduleItem != null) {
					this.Result.LoanCloseDate = lastScheduleItem.Date; // 'maturity date'
				}
				Log.Debug("==========================LoanState: {0}", this.tLoan);
				return;
			}
			if (this.tNLLoan != null) {
				// new loan structure TODO
				Log.Debug("NEW LOAN STRUCTURE NOT IMPLEMENTED YET");
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="rescheduledLoan"></param>
		//private void LoanRescheduleSave(Loan rescheduledLoan) {
		private void LoanRescheduleSave() {

			if (this.ReschedulingArguments.SaveToDB == false)
				return;

			if (this.tLoan != null && this.tLoan.Schedule.Count > 0) {
				try {
					this.loanRep.EvictAll();
					this.loanRep.Evict(this.tLoan);
					this.loanRep.BeginTransaction();
					this.loanRep.EnsureTransaction(() => {
						this.loanRep.SaveOrUpdate(this.tLoan);
					});
					this.loanRep.CommitTransaction();
				} catch (Exception transactionEx) {
					this.loanRep.RollbackTransaction();
					this.message = string.Format("Re-schedule rolled back. Arguments {0}, err: {1}", this.ReschedulingArguments, transactionEx);
					Log.Alert(this.message);
					this.Result.Error = "Failed to save new schedules list to DB. Try again, please.";
				}
			}
		}



		private Loan tLoan;
		private readonly NL_Model tNLLoan;
		private string message;
		private readonly LoanRepository loanRep;
		private readonly CultureInfo cultureInfo;

	}
}