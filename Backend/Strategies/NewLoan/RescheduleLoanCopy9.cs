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

	public class RescheduleLoanCopy9<T> : AStrategy {

		public RescheduleLoanCopy9(T t, ReschedulingArgument reschedulingArgument) {

			this.ReschedulingArguments = reschedulingArgument;

			if (t.GetType() == typeof(Loan)) {
				this.tLoan = t as Loan;
				this.tNLLoan = null;
				this.loanRep = ObjectFactory.GetInstance<LoanRepository>();
				this.loanRep.Clear();

			} else if (t.GetType() == typeof(NL_Model)) {
				this.tNLLoan = t as NL_Model;
				this.tLoan = null;
			}

			this.Result = new ReschedulingResult();
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

			Log.Debug("\n\n\n ==================================================================RE-SCHEDULING===========ARGUMENTS:==================================" + this.ReschedulingArguments);

			try {

				GetCurrentLoanState();

				// input validation for "IN"
				if (this.ReschedulingArguments.RescheduleIn && (this.ReschedulingArguments.ReschedulingDate > this.Result.LoanCloseDate)) {
					this.Result.Error = "Within loan arrangement is impossible";
					return;
				}

				// 3. intervals number
				if (this.ReschedulingArguments.RescheduleIn) {

					this.Result.IntervalsNum = MiscUtils.DateDiffInMonths(this.ReschedulingArguments.ReschedulingDate.Date, this.Result.LoanCloseDate.Date);
					this.Result.IntervalsNumWeeks = MiscUtils.DateDiffInWeeks(this.ReschedulingArguments.ReschedulingDate.Date, this.Result.LoanCloseDate.Date);

					Log.Debug("IN IntervalsNum: {0}, IntervalsNumWeeks: {1}", this.Result.IntervalsNum, this.Result.IntervalsNumWeeks);

					if (!this.ReschedulingArguments.SaveToDB)
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

				// OUT
				if (this.ReschedulingArguments.RescheduleIn == false) {

					// get I - Accrued interest to pay untill Rescheduling date
					DateTime sDate0 = this.ReschedulingArguments.ReschedulingDate; // this.ReschedulingArguments.ReschedulingRepaymentIntervalType == RepaymentIntervalTypes.Month ? this.ReschedulingArguments.ReschedulingDate.AddMonths(1) : this.ReschedulingArguments.ReschedulingDate.AddDays(7);
					DateTime endDate0 = this.ReschedulingArguments.ReschedulingRepaymentIntervalType == RepaymentIntervalTypes.Month ? sDate0.AddMonths(1) : sDate0.AddDays(7);
					decimal interestRate0 = calc.GetInterestRate(sDate0, endDate0);
					LoanScheduleItem accruedInterestItem = new LoanScheduleItem() {
						Date = sDate0.Date,
						InterestRate = interestRate0,
						Status = LoanScheduleStatus.StillToPay,
						Loan = this.tLoan,
						Balance = 0
					};
					this.tLoan.Schedule.Add(accruedInterestItem);
					calc.GetState();
					I = Decimal.Ceiling(calc.InterestToPay);
					//Log.Debug("-------------------------------I= {0}", I);
					//Log.Debug("---------------------Accrued interest till now : \n {0}", this.tLoan);
					// remove "check accrued interest" payment
					this.tLoan.Schedule.Remove(accruedInterestItem);
					// ### get I - Accrued interest to pay untill Rescheduling date

					// ReSharper disable once PossibleInvalidOperationException
					decimal m = (decimal)this.ReschedulingArguments.PaymentPerInterval;
					int k = (int)Math.Ceiling((P + I + F) / (m - (P + I + F) * r));

					// unsufficient payment per interval
					if (k < 0) {
						this.message = string.Format("{0}ly payment of {1} is not sufficient to pay the loan outstanding balance. Accrued interest until {2} is {3}, accumulated fees: {4}",
							this.ReschedulingArguments.ReschedulingRepaymentIntervalType, this.ReschedulingArguments.PaymentPerInterval.Value.ToString("C2", this.cultureInfo), this.ReschedulingArguments.ReschedulingDate.Date.DateStr(), I.ToString("C2", this.cultureInfo), calc.FeesToPay.ToString("C2", this.cultureInfo));
						this.Result.Error = this.message;
						return;
					}

					this.Result.LoanCloseDate = this.ReschedulingArguments.ReschedulingDate.AddMonths(k+1);

					this.Result.IntervalsNum = MiscUtils.DateDiffInMonths(this.ReschedulingArguments.ReschedulingDate.Date, this.Result.LoanCloseDate.Date);
					this.Result.IntervalsNumWeeks = MiscUtils.DateDiffInWeeks(this.ReschedulingArguments.ReschedulingDate.Date, this.Result.LoanCloseDate.Date);

					Log.Debug("OUT PaymentPerInterval: {0}, k: {1}, new close date: {2}", this.ReschedulingArguments.PaymentPerInterval, k, this.Result.LoanCloseDate);
				}

				if (!this.ReschedulingArguments.SaveToDB)
					return;

				periods = (this.ReschedulingArguments.ReschedulingRepaymentIntervalType == RepaymentIntervalTypes.Month ? this.Result.IntervalsNum : this.Result.IntervalsNumWeeks) ;

				decimal newInstallmentPrincipal =  Decimal.Ceiling((P) / periods);

				DateTime firstInstallmentDate = this.ReschedulingArguments.ReschedulingRepaymentIntervalType == RepaymentIntervalTypes.Month ? this.ReschedulingArguments.ReschedulingDate.AddMonths(1) : this.ReschedulingArguments.ReschedulingDate.AddDays(7);

				Log.Debug("P: {0}; newInstallmentPrincipal: {1}, periods: {2}, I: {3}, firsrt date: {4}", P, newInstallmentPrincipal, periods, I, firstInstallmentDate);

				int order = this.tLoan.Schedule.Count;
				for (int j = 1; j <= periods; j++) {
					DateTime startDate = this.ReschedulingArguments.ReschedulingRepaymentIntervalType == RepaymentIntervalTypes.Month ? firstInstallmentDate.AddMonths(j) : firstInstallmentDate.AddDays(j * 7);
					//Log.Debug("Start: {0}", startDate);
					LoanScheduleItem item = new LoanScheduleItem() {
						Date = startDate.Date,
						InterestRate = this.tLoan.InterestRate,
						Status = LoanScheduleStatus.StillToPay,
						Loan = this.tLoan,
						Position = order++
					};
					this.tLoan.Schedule.Add(item);
				}

				// update balance
				decimal balance = 0;
				for (int j = periods; j > 0; j--) {
					LoanScheduleItem s = this.tLoan.Schedule.FirstOrDefault(sc => sc.Position == j);
					if (s != null) {

						DateTime startDate = this.ReschedulingArguments.ReschedulingRepaymentIntervalType == RepaymentIntervalTypes.Month ? s.Date.AddMonths(-1) : s.Date.AddDays(-7);
						decimal interestRate = calc.GetInterestRate(startDate, s.Date);
						Log.Debug("Start: {0}, End: {1}, Rate: {2}", startDate, s.Date, interestRate);

						s.InterestRate = interestRate;
						s.Balance = balance;
						s.LoanRepayment = newInstallmentPrincipal;
						
						balance += Decimal.Ceiling(newInstallmentPrincipal + interestRate * newInstallmentPrincipal);
					}
				}
				
				/*decimal balance = 0;
				int order = this.tLoan.Schedule.Count + periods;
				for (int j = periods; j > 0; j--) {
					DateTime endDate = this.ReschedulingArguments.ReschedulingRepaymentIntervalType == RepaymentIntervalTypes.Month ?
						this.ReschedulingArguments.ReschedulingDate.AddMonths(j) : this.ReschedulingArguments.ReschedulingDate.AddDays(j * 7);
					DateTime startDate = this.ReschedulingArguments.ReschedulingRepaymentIntervalType == RepaymentIntervalTypes.Month ?
						endDate.AddMonths(-1) : endDate.AddDays(-7);
					decimal interestRate = calc.GetInterestRate(startDate, endDate);
					Log.Debug("Start: {0}, End: {1}, Rate: {2}", startDate, endDate, interestRate);
					LoanScheduleItem item = new LoanScheduleItem() {
						Date = startDate.Date,
						InterestRate = interestRate,
						Status = LoanScheduleStatus.StillToPay,
						Loan = this.tLoan,
						Balance = balance,
						Position = order
					};
					this.tLoan.Schedule.Add(item);
					order--;
					balance += Decimal.Ceiling(newInstallmentPrincipal + interestRate * newInstallmentPrincipal);
				}

				foreach (LoanScheduleItem s in this.tLoan.Schedule.OrderBy(s => s.Date).ToList()) {
					this.tLoan.Schedule.Insert(index: (int)s.Position, item: s);
				}*/

				Log.Debug(">>>>>>>>>>>>>>>>>>Loan modified: \n {0}", this.tLoan);
				
				calc.GetState();

				Log.Debug("--------------Loan recalculated: \n {0}", this.tLoan);

				// unsufficient payment per period
				LoanScheduleItem overInstallment = this.tLoan.Schedule.First(s => s.AmountDue > this.ReschedulingArguments.PaymentPerInterval);
				if (overInstallment != null) {
					this.message = string.Format("{0}ly payment of {1} is not sufficient to pay the loan outstanding balance. Accrued interest: {2}, accumulated fees: {3} ",
						// ReSharper disable once PossibleInvalidOperationException
						this.ReschedulingArguments.ReschedulingRepaymentIntervalType, this.ReschedulingArguments.PaymentPerInterval.Value.ToString("C2", this.cultureInfo), 
						I,
						//overInstallment.Interest.ToString("C2", this.cultureInfo), 
						overInstallment.Fees.ToString("C2", this.cultureInfo));
					this.Result.Error = this.message;
					return;
				}

				// check "too much" payment per interval
				if (!this.ReschedulingArguments.RescheduleIn) {
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

				//LoanRescheduleSave();

			} catch (Exception e) {
				Log.Alert(e, "Failed to get rescheduling data for loan {0}", this.ReschedulingArguments.LoanID);

			}
		}



		private void GetCurrentLoanState() {
			if (this.tLoan != null) {
				this.tLoan = this.loanRep.Get(this.ReschedulingArguments.LoanID);
				this.Result.LoanInterestRate = this.tLoan.InterestRate;
				LoanScheduleItem lastScheduleItem = this.tLoan.Schedule.OrderBy(s => s.Date).LastOrDefault();
				if (lastScheduleItem != null) {
					this.Result.LoanCloseDate = lastScheduleItem.Date; // 'maturity date'
				}
				
				Log.Debug("==========================LoanState: {0} : \n {0}", this.tLoan);
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