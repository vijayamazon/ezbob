﻿namespace Ezbob.Backend.Strategies.NewLoan {
	 using System;
	 using System.Globalization;
	 using System.Linq;
	 using System.Text;
	 using ConfigManager;
	 using DbConstants;
	 using Ezbob.Backend.Models.NewLoan;
	 using Ezbob.Backend.ModelsWithDB.NewLoan;
	 using Ezbob.Utils;
	 using EzBob.Models;
	 using EZBob.DatabaseLib.Model.Database.Loans;
	 using EZBob.DatabaseLib.Model.Database.UserManagement;
	 using EZBob.DatabaseLib.Model.Loans;
	 using MailApi;
	 using Newtonsoft.Json;
	 using PaymentServices.Calculators;
	 using StructureMap;

	 public class RescheduleLoan<T> : AStrategy {

		 public RescheduleLoan(T t, ReschedulingArgument reschedulingArgument) {
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
			 this.Result.BlockAction = false;

			 // used in client's calendar for re-date selecting and in strategy for validations
			 this.Result.ReschedulingIntervalStart = DateTime.UtcNow.Date.AddDays(1);

			 if (this.ReschedulingArguments.RescheduleIn)
				 this.Result.ReschedulingIntervalEnd = this.Result.ReschedulingIntervalStart.AddDays(30);

			 // if today date sent - set re-date to tomorrow, otherwise set the date that sent
			 this.Result.FirstItemDate = (this.ReschedulingArguments.ReschedulingDate.Date == DateTime.UtcNow.Date) ? this.Result.ReschedulingIntervalStart : this.ReschedulingArguments.ReschedulingDate.Date;

			 this.cultureInfo = new CultureInfo("en-GB");

			 this.emailToAddress = CurrentValues.Instance.EzbobTechMailTo;
			 this.emailFromAddress = CurrentValues.Instance.MailSenderEmail;
			 this.emailFromName = CurrentValues.Instance.MailSenderName;

			 this.sendDebugMail = CurrentValues.Instance.ReschedulingDebugMail;
			 this.toAddressDebugMail = CurrentValues.Instance.ReschedulingDebugMailAddress;
		 }

		 public override string Name { get { return "RescheduleLoan"; } }
		 public ReschedulingArgument ReschedulingArguments;	// input
		 public ReschedulingResult Result;	// output

		 public override void Execute() {

			 NL_AddLog(LogType.Info, "Strategy Start", this.ReschedulingArguments, null, null, null);

			 if (!this.ReschedulingArguments.RescheduleIn && this.ReschedulingArguments.PaymentPerInterval == null) {
				 this.Result.Error = "Weekly/monthly payment amount for OUT rescheduling not provided";
				 this.loanRep.Clear();
				 return;
			 }

			 try {

				 this.loanRep.BeginTransaction();

				 GetCurrentLoanState();

				 if (this.tLoan == null) {
					 this.Result.Error = string.Format("Loan ID {0} not found", this.ReschedulingArguments.LoanID);
					 this.Result.BlockAction = true;
					 ExitStrategy("Exit_1");
					 return;
				 }

				 // check status, don't continue for "PaidOff"
				 if (this.tLoan.Status == LoanStatus.PaidOff) {
					 this.Result.Error = string.Format("Loan ID {0} paid off. Loan balance: {1}", this.tLoan.Id, 0m.ToString("C2", this.cultureInfo));
					 this.Result.BlockAction = true;
					 ExitStrategy("Exit_2", this.sendDebugMail);
					 return;
				 }

				 // input validation for "IN"
				 if (this.ReschedulingArguments.RescheduleIn && (this.Result.FirstItemDate > this.Result.LoanCloseDate)) {
					 this.Result.Error = "Within loan arrangement is impossible";
					 this.Result.BlockAction = true;
					 ExitStrategy("Exit_3", this.sendDebugMail);
					 return;
				 }

				 // "IN" - check between interval boundaries
				 if (this.ReschedulingArguments.RescheduleIn && (this.Result.FirstItemDate < this.Result.ReschedulingIntervalStart || this.Result.FirstItemDate > this.Result.ReschedulingIntervalEnd)) {
					 this.Result.Error = "Wrong re-scheduling date sent (any day on the calendar within next 30 days allowed)";
					 this.Result.BlockAction = true;
					 ExitStrategy("Exit_3a", this.sendDebugMail);
					 return;
				 }

				 // "OUT" - check past date
				 if (this.ReschedulingArguments.RescheduleIn == false && this.Result.FirstItemDate.Date < this.Result.ReschedulingIntervalStart) {
					 this.Result.Error = "Wrong re-scheduling date sent (only future date allowed)";
					 this.Result.BlockAction = true;
					 ExitStrategy("Exit_3b", this.sendDebugMail);
					 return;
				 }

				 this.Result.OpenPrincipal = this.tLoan.Principal;

				 // if sent "default" value (0), replace by default calculated
				 if (!this.ReschedulingArguments.RescheduleIn && this.ReschedulingArguments.PaymentPerInterval == 0)
					 this.ReschedulingArguments.PaymentPerInterval = this.Result.DefaultPaymentPerInterval;

				 Log.Debug("\n==========RE-SCHEDULING======ARGUMENTS: {0}==========LoanState: {1}\n", this.ReschedulingArguments, this.tLoan);

				 // check Marking loan {0} as 'PaidOff' in \ezbob\Integration\DatabaseLib\Model\Loans\Loan.cs(362)
				 var calc = new LoanRepaymentScheduleCalculator(this.tLoan, DateTime.UtcNow, CurrentValues.Instance.AmountToChargeFrom);

				 try {
					 if (calc.NextEarlyPayment() == 0) {
						 this.Result.Error = string.Format("Loan {0} marked as 'PaidOff'. Loan balance: {1}", this.tLoan.Id, 0m.ToString("C2", this.cultureInfo));
						 this.Result.BlockAction = true;
						 ExitStrategy("Exit_4", this.sendDebugMail);
						 return;
					 }
					 // ReSharper disable once CatchAllClause
				 } catch (Exception calcEx) {
					 Log.Info("LoanRepaymentScheduleCalculator NextEarlyPayment EXCEPTION: {0}", calcEx.Message);
				 }

				 var lastPaidSchedule = this.tLoan.Schedule.OrderBy(s => s.Date).LastOrDefault(s => (s.Status == LoanScheduleStatus.Paid || s.Status == LoanScheduleStatus.PaidOnTime || s.Status == LoanScheduleStatus.PaidEarly));

				 // if StopFutureInterest checked - add active "freeze inteval" from FirstItemDate untill NoLimitDate
				 if (this.ReschedulingArguments.RescheduleIn == false && this.ReschedulingArguments.StopFutureInterest) {
					 //TODO : also add it to NL_InterestFreeze && deactivating.
					 LoanInterestFreeze freeze = new LoanInterestFreeze {
						 Loan = this.tLoan,
						 StartDate = lastPaidSchedule != null ? lastPaidSchedule.Date : this.tLoan.Date.Date, //this.Result.FirstItemDate,
						 EndDate = this.noLimitDate, // NoLimitDate from LoanEditorController.cs - move to common area
						 InterestRate = 0,
						 ActivationDate = this.Result.FirstItemDate,
						 DeactivationDate = null
					 };
					 if (!this.tLoan.InterestFreeze.Contains(freeze))
						 this.tLoan.InterestFreeze.Add(freeze);

					 calc.GetState(); // reload state with freeze consideration
				 }

				 decimal totalEarlyPayment = calc.TotalEarlyPayment();
				 decimal P = this.Result.OpenPrincipal;
				 decimal F = calc.FeesToPay;	// unpaid fees
				 //decimal I = lastPaidSchedule != null ? (calc.GetInterestRate(lastPaidSchedule.Date.AddDays(1), this.Result.FirstItemDate) *P) : (calc.GetInterestRate(this.tLoan.Date.Date.AddDays(1), this.Result.FirstItemDate) * P); // unpaid interest till rescheduling start date
				 decimal I = (totalEarlyPayment - P - F);	// unpaid interest till first rescheduled item
				 I = I < 0 ? 0 : I; // bugfix EZ-4236
				 decimal r = ((this.ReschedulingArguments.RescheduleIn == false && this.ReschedulingArguments.StopFutureInterest)) ? 0 : this.tLoan.InterestRate;

				 this.Result.ReschedulingBalance = (P + I + F); // not final - add to I period from rescheduling date untill new maturity date

				 Log.Debug("--------------P: {0}, I: {1}, F: {2}, LoanCloseDate: {3}, totalEarlyPayment: {4}, r: {5}, ReschedulingBalance: {6}, \n lastPaidSchedule: {7}",
					 P,
					 I,
					 F,
					 this.Result.LoanCloseDate.Date,
					 totalEarlyPayment,
					 r,
					 this.Result.ReschedulingBalance,
					 lastPaidSchedule);

				 // 3. intervals number

				 // IN
				 if (this.ReschedulingArguments.RescheduleIn) {
					 // add "grace" period - 14 days to maturity date
					 DateTime closeDateWithGrace = this.Result.LoanCloseDate.Date.AddDays(14);

					 this.Result.IntervalsNum = this.ReschedulingArguments.ReschedulingRepaymentIntervalType == RepaymentIntervalTypes.Month ? MiscUtils.DateDiffInMonths(this.Result.FirstItemDate, closeDateWithGrace) : MiscUtils.DateDiffInWeeks(this.Result.FirstItemDate, closeDateWithGrace);

					 // adjust intervals number +1 if needed
					 DateTime rescheduledCloseDate = this.ReschedulingArguments.ReschedulingRepaymentIntervalType == RepaymentIntervalTypes.Month ?
						 this.Result.FirstItemDate.AddMonths(this.Result.IntervalsNum) :
						 this.Result.FirstItemDate.AddDays(this.Result.IntervalsNum * 7);

					 Log.Debug("rescheduledCloseDate: {0}, Result.IntervalsNum: {1}, Result.LoanCloseDate: {2}, closeDateWithGrace: {3}", rescheduledCloseDate, this.Result.IntervalsNum, this.Result.LoanCloseDate.Date, closeDateWithGrace);

					 TimeSpan ts = closeDateWithGrace.Date.Subtract(rescheduledCloseDate.Date);

					 if (ts.Days > 0)
						 this.Result.IntervalsNum += 1;

					 Log.Debug("Adjusted intervals: rescheduledCloseDate: {0}, Result.IntervalsNum: {1}, Result.LoanCloseDate: {2}, closeDateWithGrace: {3}, dDays: {4}", rescheduledCloseDate, this.Result.IntervalsNum, this.Result.LoanCloseDate.Date, closeDateWithGrace, ts.Days);
				 }

				 // OUT - real intervals (k) calculation
				 if (this.ReschedulingArguments.RescheduleIn == false) {

					 // too much payment per interval
					 if (this.ReschedulingArguments.PaymentPerInterval > this.Result.ReschedulingBalance) {
						 // ReSharper disable once PossibleInvalidOperationException
						 this.message = string.Format("The entered amount accedes the outstanding balance of {0} for payment of {1}",
							 this.Result.ReschedulingBalance.ToString("C2", this.cultureInfo), this.ReschedulingArguments.PaymentPerInterval.Value.ToString("C2", this.cultureInfo));
						 this.Result.Error = this.message;
						 ExitStrategy("Exit_6", this.sendDebugMail);
						 return;
					 }

					 // ReSharper disable once PossibleInvalidOperationException
					 decimal m = (decimal)this.ReschedulingArguments.PaymentPerInterval;

					 // System.DivideByZeroException: Attempted to divide by zero prevent
					 decimal kDiv = (m - this.Result.ReschedulingBalance * r);
					 if (kDiv == 0)
						 kDiv = 1;

					 var k = (int)Math.Ceiling(this.Result.ReschedulingBalance / kDiv);
					 this.Result.IntervalsNum = k;

					 Log.Debug("k: {0}, P: {1}, I: {2}, F: {3}, r: {4}, oustandingBalance: {5}, m: {6}, StopFutureInterest: {7}", k, P, I, F, r, this.Result.ReschedulingBalance, m, this.ReschedulingArguments.StopFutureInterest);

					 // uncovered loan - too small payment per interval
					 if (k < 0) {
						 this.Result.Error = "Chosen amount is not sufficient for covering the loan overtime, i.e. accrued interest will be always greater than the repaid amount per payment";
						 ExitStrategy("Exit_7", this.sendDebugMail);
						 return;
					 }

					 this.Result.LoanCloseDate = this.ReschedulingArguments.ReschedulingRepaymentIntervalType == RepaymentIntervalTypes.Month ? this.Result.FirstItemDate.AddMonths(k) : this.Result.FirstItemDate.AddDays(k * 7);

					 Log.Debug("new close date: {0}", this.Result.LoanCloseDate);

					 // DON'T DELETE - can be used in new calculator
					 //int n = (int)Math.Ceiling(P / (m - P * r));
					 //decimal x = 0m; = this.Result.ReschedulingBalance * r * (int)((k + 1) / 2) - P * r * (int)((n + 1) / 2);
					 //Log.Debug("n: {0}, k: {1}, P: {2}, I: {3}, F: {4}, r: {5}, oustandingBalance: {6}, m: {7}, X: {8}, closeDate: {9}, Result.IntervalsNum: {10}", n, k, P, I, F, r, this.Result.ReschedulingBalance, m, x, this.Result.LoanCloseDate, this.Result.IntervalsNum);
				 }

				 Log.Debug("close date: {0}, intervals: {1}", this.Result.LoanCloseDate, this.Result.IntervalsNum);

				 if (this.Result.IntervalsNum == 0) {
					 this.Result.Error = "Rescheduling impossible (calculated payments number 0)";
					 this.Result.BlockAction = true;
					 ExitStrategy("Exit_8", this.sendDebugMail);
					 return;
				 }

				 // remove unpaid (lates, stilltopays passed) and future unpaid schedule items
				 foreach (var rmv in this.tLoan.Schedule.ToList<LoanScheduleItem>()) {
					 // if loan has future items that already paid ("paid early"), re-scheduling not allowed
					 if ((rmv.Status == LoanScheduleStatus.Paid || rmv.Status == LoanScheduleStatus.PaidOnTime || rmv.Status == LoanScheduleStatus.PaidEarly) && rmv.Date > this.Result.FirstItemDate) {
						 this.Result.Error = string.Format("Currently it is not possible to apply rescheduling future if payment/s relaying in the future have been already covered with early made payment, partially or entirely. " +
							 "You can apply rescheduling option after [last covered payment day].");
						 this.Result.BlockAction = true;
						 ExitStrategy("Exit_5", this.sendDebugMail);
						 return;
					 }
					 if (rmv.Date >= this.Result.FirstItemDate)
						 this.tLoan.Schedule.Remove(rmv);
					 if (rmv.Date <= this.Result.FirstItemDate && rmv.Status == LoanScheduleStatus.Late) {
						 this.tLoan.Schedule.Remove(rmv);
						 this.tLoan.TryAddRemovedOnReschedule(new LoanScheduleDeleted().CloneScheduleItem(rmv));
					 }
					 if (rmv.Date <= this.Result.FirstItemDate && rmv.Status == LoanScheduleStatus.StillToPay) {
						 this.tLoan.Schedule.Remove(rmv);
						 this.tLoan.TryAddRemovedOnReschedule(new LoanScheduleDeleted().CloneScheduleItem(rmv));
					 }
				 }

				 decimal balance = P;
				 decimal iPrincipal = Decimal.Round(P / this.Result.IntervalsNum);
				 decimal firstPrincipal = (P - iPrincipal * (this.Result.IntervalsNum - 1));

				 //check "first iPrincipal negative" case: if first iPrincipal <= 0, remove this and reduce this.Result.IntervalsNum
				 if (firstPrincipal < 0) {
					 Log.Debug("AAA Periods: {0}, newInstalment: {1}, close date: {2}, balance: {3}, firstItemDate: {4}, firstPrincipal: {5}, " +
									 "P: {6}, I: {7}, F: {8}, r: {9}", this.Result.IntervalsNum, iPrincipal, this.Result.LoanCloseDate, this.Result.ReschedulingBalance, this.Result.FirstItemDate, firstPrincipal, P, I, F, r);
					 this.Result.IntervalsNum -= 1;
					 firstPrincipal = iPrincipal;
					 if ((iPrincipal * (this.Result.IntervalsNum - 1) + firstPrincipal) != balance) {
						 this.Result.Error = "Failed to create new schedule.";
						 ExitStrategy("Exit_9", this.sendDebugMail);
					 }
				 }

				 Log.Debug("Periods: {0}, newInstalment: {1}, close date: {2}, balance: {3}, firstItemDate: {4}, firstPrincipal: {5}, " +
					 "P: {6}, I: {7}, F: {8}, r: {9}", this.Result.IntervalsNum, iPrincipal, this.Result.LoanCloseDate, this.Result.ReschedulingBalance, this.Result.FirstItemDate, firstPrincipal, P, I, F, r);

				 // add new re-scheduled items, both for IN/OUT
				 int position = this.tLoan.Schedule.Count;
				 for (int j = 0; j < this.Result.IntervalsNum; j++) {

					 DateTime iStartDate = this.ReschedulingArguments.ReschedulingRepaymentIntervalType == RepaymentIntervalTypes.Month ? this.Result.FirstItemDate.AddMonths(j) : this.Result.FirstItemDate.AddDays(7 * j);
					 decimal iLoanRepayment = (j == 0) ? firstPrincipal : iPrincipal;
					 balance -= iLoanRepayment;

					 LoanScheduleItem item = new LoanScheduleItem() {
						 Date = iStartDate.Date,
						 InterestRate = r,
						 Status = LoanScheduleStatus.StillToPay,
						 Loan = this.tLoan,
						 LoanRepayment = iLoanRepayment,
						 Balance = balance,
						 Position = ++position
					 };
					 this.tLoan.Schedule.Add(item);
				 }

				 //Log.Debug("--------------Loan modified: \n {0}", this.tLoan);

				 //  after modification
				 if (CheckValidateLoanState(calc) == false) {
					 ExitStrategy("Exit_10", this.sendDebugMail);
					 return;
				 }

				 Log.Debug("--------------Loan recalculated: \n {0}", this.tLoan);

				 // prevent schedules with negative iPrincipal (i.e. LoanRepayment:-4.00)
				 var negativeIPrincipal = this.tLoan.Schedule.FirstOrDefault(s => s.LoanRepayment < 0);
				 if (negativeIPrincipal != null) {
					 this.Result.Error = "Negative principal in loan schedule";
					 ExitStrategy("Exit_11", this.sendDebugMail);
					 return;
				 }

				 // prevent "paidEarly" for newly created schedule items
				 var newPaidEarly = this.tLoan.Schedule.FirstOrDefault(s => s.Date > this.ReschedulingArguments.ReschedulingDate && s.Status == LoanScheduleStatus.PaidEarly);
				 if (newPaidEarly != null) {
					 this.Result.Error = "Wrong balance for re-scheduling calculated. Please, contact support.";
					 ExitStrategy("Exit_12", this.sendDebugMail);
					 return;
				 }

				 var firstRescheduledItem = this.tLoan.Schedule.FirstOrDefault(s => s.Date.Date == this.Result.FirstItemDate);
				 if (firstRescheduledItem != null) {
					 this.Result.FirstPaymentInterest = firstRescheduledItem.Interest;
				 }

				 if (this.ReschedulingArguments.RescheduleIn == false) { // OUT

					 // NOT POSSIBLE WITH CURRENT CALCULATOR < DON'T DELETE
					 //OffsetX(x);
					 //if (CheckValidateLoanState(calc) == false)
					 //	return;
					 //Log.Debug("-------Loan recalculated+adjusted to X \n {0}", this.tLoan);

					 // unsufficient payment per period
					 LoanScheduleItem overInstalment = this.tLoan.Schedule.FirstOrDefault(s => s.AmountDue > this.ReschedulingArguments.PaymentPerInterval);
					 if (overInstalment != null) {
						 // ReSharper disable once PossibleInvalidOperationException
						 this.message = string.Format("{0}ly payment of {1} not sufficient to pay the loan outstanding balance. Accrued interest: {2}, accumulated fees: {3}, first new instalment: {4}. " +
							 "You can choose to reduce the accumulated fees & interest by clearing them via manual payment, before setting the new payment schedule.",
							 this.ReschedulingArguments.ReschedulingRepaymentIntervalType,
							 this.ReschedulingArguments.PaymentPerInterval.Value.ToString("C2", this.cultureInfo),
							 overInstalment.Interest.ToString("C2", this.cultureInfo), //I.ToString("C2", this.cultureInfo),
							 overInstalment.Fees.ToString("C2", this.cultureInfo),
							 overInstalment.AmountDue.ToString("C2", this.cultureInfo)
							 );
						 this.Result.Error = this.message;
						 ExitStrategy("Exit_13", this.sendDebugMail);
						 return;
					 }
				 }

				 if (!this.ReschedulingArguments.SaveToDB) {
					 ExitStrategy("Exit_14", this.sendDebugMail);
					 return;
				 }

				 LoanRescheduleSave();
				 NL_AddLog(LogType.Info, "Strategy End", this.ReschedulingArguments, this.Result, null, null);
				 // ReSharper disable once CatchAllClause
			 } catch (Exception ex) {
				 Log.Alert(ex, "Failed to get rescheduling data for loan {0}", this.ReschedulingArguments.LoanID);
				 NL_AddLog(LogType.Error, "Strategy Faild", this.ReschedulingArguments, null, ex.ToString(), ex.StackTrace);
			 }
		 }

		 private void ExitStrategy(string logMessage, bool sendMail = false) {

			 // for debug only
			 if (sendMail)
				 SendMail("Re-scheduling " + logMessage + ": " + this.Result.Error, this.toAddressDebugMail);

			 Log.Debug("{0}: {1}", logMessage, this.Result.Error);
			 this.loanRep.Clear();
			 this.loanRep.RollbackTransaction();
		 }

		 /// <summary>
		 ///  distribute/offset X - the difference between earned interest for n(P) and k(P+I+F)
		 /// </summary>
		 /// <param name="X"></param>
		 // ReSharper disable once UnusedMember.Local
		 private void OffsetX(decimal X) {
			 if (this.ReschedulingArguments.RescheduleIn == false && X > 0) {
				 Log.Debug("X: {0}", 0);
				 int position = this.tLoan.Schedule.Count - 1;
				 while (X > 0) {
					 LoanScheduleItem item = this.tLoan.Schedule.ElementAt(position);
					 //decimal interestDiff = X - item.Interest;
					 if ((X - item.Interest) > 0) {
						 item.InterestRate = 0; // 100% discount for this instalment
					 } else {
						 item.InterestRate = Decimal.Round(((((item.Interest - X) * 100 / item.Interest) * item.InterestRate) / 100), 6);
					 }
					 position--;
					 X -= item.Interest;
					 item.Interest = item.InterestRate * item.LoanRepayment;
				 }
			 }
		 }


		 private bool CheckValidateLoanState(LoanRepaymentScheduleCalculator calc) {
			 try {
				 calc.GetState();
				 // ReSharper disable once CatchAllClause
			 } catch (Exception e) {
				 this.Result.Error = e.Message;
				 Log.Alert("LoanRepaymentScheduleCalculator STATE EXCEPTION: {0}", e);
				 return false;
			 }

			 // additional validating - via "edit loan" GUI model
			 ChangeLoanDetailsModelBuilder changeLoanModelBuilder = new ChangeLoanDetailsModelBuilder();
			 EditLoanDetailsModel model = changeLoanModelBuilder.BuildModel(this.tLoan);
			 model.Validate();
			 if (model.HasErrors) {
				 this.Result.Error = string.Join("<br/>", model.Errors.ToArray());
				 Log.Alert(this.Result.Error);
				 return false;
			 }

			 return true;
		 }

		 private void GetCurrentLoanState() {
			 if (this.tLoan != null) {

				 this.tLoan = this.loanRep.Get(this.ReschedulingArguments.LoanID);

				 if (this.tLoan == null)
					 return;

				 this.Result.LoanInterestRate = this.tLoan.InterestRate;
				 LoanScheduleItem lastScheduleItem = this.tLoan.Schedule.OrderBy(s => s.Date).LastOrDefault();
				 if (lastScheduleItem != null) {
					 this.Result.LoanCloseDate = lastScheduleItem.Date; // 'maturity date'
				 }

				 this.Result.DefaultPaymentPerInterval = lastScheduleItem == null ? 0 : lastScheduleItem.LoanRepayment;

				 // hold LoanChangesHistory (loan state before changes) before re-schedule
				 this.loanHistory = new LoanChangesHistory {
					 Data = new ChangeLoanDetailsModelBuilder().BuildModel(this.tLoan).ToJSON(),
					 Date = this.ReschedulingArguments.ReschedulingDate,
					 Loan = this.tLoan
				 };

				 this.sbBeforeLoanState.Append(this.tLoan);
			 }

			 if (this.tNLLoan != null) {
				 // new loan structure TODO
				 Log.Debug("NEW LOAN STRUCTURE NOT IMPLEMENTED YET");
			 }

			 // if sent "default" value (0), replace by default calculated
			 if (!this.ReschedulingArguments.RescheduleIn && this.ReschedulingArguments.PaymentPerInterval == 0)
				 this.ReschedulingArguments.PaymentPerInterval = this.Result.DefaultPaymentPerInterval;
		 }

		 /// <summary>
		 /// saving "rescheduling" to DB
		 /// </summary>
		 private void LoanRescheduleSave() {

			 if (this.ReschedulingArguments.SaveToDB == false)
				 return;

			 if (this.tLoan != null && this.tLoan.Schedule.Count > 0) {

				 try {

					 //save LoanChangesHistory (loan state before changes) before re-schedule
					 this.loanHistory.User = ObjectFactory.GetInstance<UsersRepository>().Get(Context.UserID);
					 ObjectFactory.GetInstance<LoanChangesHistoryRepository>().Save(this.loanHistory);

					 Log.Debug("==========================Saving rescheduled loan: {0}", this.tLoan);
					 this.tLoan.Status = this.ReschedulingArguments.RescheduleIn ? LoanStatus.Live : LoanStatus.Late;
					 this.tLoan.LastRecalculation = DateTime.UtcNow;
					 this.tLoan.Modified = true;
					 this.tLoan.DateClosed = null;

					 this.loanRep.EvictAll();
					 this.loanRep.Evict(this.tLoan);

					 this.loanRep.BeginTransaction();
					 this.loanRep.EnsureTransaction(() => {
						 this.loanRep.SaveOrUpdate(this.tLoan);
					 });
					 this.loanRep.CommitTransaction();

					 SendMail("Re-schedule saved successfully");

					 // ReSharper disable once CatchAllClause
				 } catch (Exception transactionEx) {

					 this.loanRep.RollbackTransaction();
					 this.message = string.Format("Re-schedule rolled back. Arguments {0}, err: {1}", this.ReschedulingArguments, transactionEx);
					 Log.Alert(this.message);
					 this.Result.Error = "Failed to save new schedules list to DB. Try again, please.";

					 SendMail("Re-schedule rolled back", null, transactionEx);
				 }

				 this.loanRep.Clear();
			 }
		 }

		 /// <summary>
		 /// sending mail on re-schedule saving
		 /// </summary>
		 /// <param name="subject"></param>
		 /// <param name="toAddress"></param>
		 /// <param name="transactionEx"></param>
		 private void SendMail(string subject, string toAddress = null, Exception transactionEx = null) {
			 if (toAddress == null)
				 toAddress = this.emailToAddress;

			 subject = subject + " for customerID: " + this.tLoan.Customer.Id + ", by userID: " + Context.UserID + ", Loan ref: " + this.tLoan.RefNumber;

			 var stateBefore = JsonConvert.DeserializeObject<EditLoanDetailsModel>(this.loanHistory.Data).Items;
			 StringBuilder sb = new StringBuilder();
			 if (stateBefore != null) {
				 foreach (var i in stateBefore) {
					 sb.Append("<p>").Append(i).Append("</p>");
				 }
			 }

			 this.tLoan = this.loanRep.Get(this.ReschedulingArguments.LoanID);
			 var calc = new LoanRepaymentScheduleCalculator(this.tLoan, DateTime.UtcNow, CurrentValues.Instance.AmountToChargeFrom);
			 calc.GetState();
			 var currentState = new ChangeLoanDetailsModelBuilder().BuildModel(this.tLoan).Items;
			 StringBuilder currentStateStr = new StringBuilder();
			 if (currentState != null) {
				 foreach (var ii in currentState) {
					 currentStateStr.Append("<p>").Append(ii).Append("</p>");
				 }
			 }

			 string beforeStateString = this.sbBeforeLoanState.ToString();
			 string currentStateString = this.tLoan.ToString();

			 this.message = string.Format(
				 "<h3>CustomerID: {0}; UserID: {1}</h3>"
				  + "<h5>Arguments</h5>  <pre>{2}</pre>"
				  + "<h5>Result</h5>  <pre>{3}</pre>"
				  + "<h5>Error</h5> {4}"
				  + "<h5>Loan state before action</h5> <pre>{5}</pre>"
				  + "<h5>Current schedule - after action</h5> <pre>{6}</pre>"

				  + "<br/><br/><br/>===================================<h5>Loan state before in view model</h5> {7}"
				  + "<h5>Current schedule after action  in view model</h5> {8}",

				 this.tLoan.Customer.Id, Context.UserID
				 , (this.ReschedulingArguments)
				 , (this.Result)
				 , (transactionEx == null ? "NO errors" : transactionEx.ToString())
				 , (beforeStateString.Length > 0) ? beforeStateString : "not found"
				 , (currentStateString.Length > 0) ? currentStateString : "not found"
				 , (sb.ToString().Length > 0) ? sb.ToString() : "not found"
				 , (currentStateStr.ToString().Length > 0) ? currentStateStr.ToString() : "not found"
			 );
			 new Mail().Send(
				 toAddress,
				 null,					// message text
				 this.message,			// html
				 this.emailFromAddress,	// fromEmail
				 this.emailFromName,		// fromName
				 subject					// subject
			 );
		 } // SendMail

		 private Loan tLoan;
		 private readonly NL_Model tNLLoan;
		 private string message;
		 private readonly LoanRepository loanRep;
		 private readonly CultureInfo cultureInfo;
		 private LoanChangesHistory loanHistory;

		 private readonly string emailToAddress;
		 private readonly string emailFromAddress;
		 private readonly string emailFromName;

		 private readonly DateTime noLimitDate = new DateTime(2099, 1, 1);

		 private readonly StringBuilder sbBeforeLoanState = new StringBuilder();
		 private readonly bool sendDebugMail;
		 private readonly string toAddressDebugMail;
	 }
 }
