﻿namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Backend.Models.NewLoan;
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

			this.cultureInfo = new CultureInfo("en-GB");

		    this.emailToAddress = CurrentValues.Instance.EzbobTechMailTo;
			this.emailFromAddress = CurrentValues.Instance.MailSenderEmail;
			this.emailFromName = CurrentValues.Instance.MailSenderName;
		}

		public override string Name { get { return "RescheduleLoan"; } }
		public ReschedulingArgument ReschedulingArguments;	// input
		public ReschedulingResult Result;	// output

		public override void Execute() {

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
					ExitStrategy("Exit1");
					return;
				}

				// check status, don't continue for "PaidOff"
				if (this.tLoan.Status == LoanStatus.PaidOff) {
					this.Result.Error = string.Format("Loan ID {0} paid off. Loan balance: {1}", this.tLoan.Id, 0m.ToString("C2", this.cultureInfo));
					this.Result.BlockAction = true;
					ExitStrategy("Exit2");
					return;
				}

				// input validation for "IN"
				if (this.ReschedulingArguments.RescheduleIn && (this.ReschedulingArguments.ReschedulingDate > this.Result.LoanCloseDate)) {
					this.Result.Error = "Within loan arrangement is impossible";
					this.Result.BlockAction = true;
					ExitStrategy("Exit3");
					return;
				}

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
						ExitStrategy("Exit4");
						return;
					}
					// ReSharper disable once CatchAllClause
				} catch (Exception calcEx) {
					Log.Info("LoanRepaymentScheduleCalculator NextEarlyPayment EXCEPTION: {0}", calcEx.Message);
				}

				// remove unpaid (lates, stilltopays passed) and future schedule items
				foreach (var rmv in this.tLoan.Schedule.ToList<LoanScheduleItem>()) {

					// if loan has future items that already paid ("paid early"), re-scheduling not allowed
					if ((rmv.Status == LoanScheduleStatus.Paid || rmv.Status == LoanScheduleStatus.PaidOnTime || rmv.Status == LoanScheduleStatus.PaidEarly) && rmv.Date > this.ReschedulingArguments.ReschedulingDate) {
						this.Result.Error = string.Format("Currently it is not possible to apply rescheduling future if payment/s relaying in the future have been already covered with early made payment, partially or entirely. " +
							"You can apply rescheduling option after [last covered payment day].");
						this.Result.BlockAction = true;
						ExitStrategy("Exit5");
						return;
					}

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

				calc.GetState(); // reload state after removing unpaid (lates, stilltopays passed) and future schedule items

				decimal I = calc.NextEarlyPayment();
				decimal P = this.tLoan.Principal;
				decimal F = (this.tLoan.Charges.Sum(f => f.Amount) - this.tLoan.Charges.Sum(f => f.AmountPaid));
				decimal r = this.tLoan.InterestRate;
				// ReSharper disable once TooWideLocalVariableScope
				decimal x = 0m;
				this.Result.ReschedulingBalance = (P + I + F);
				DateTime firstItemDate = this.ReschedulingArguments.ReschedulingDate.Date.AddDays(1);

				Log.Debug("--------------P: {0}, I: {1}, F: {2}, Result.LoanCloseDate: {3}", P, I, F, this.Result.LoanCloseDate.Date);

				// 3. intervals number

				// IN
				if (this.ReschedulingArguments.RescheduleIn) {

					this.Result.IntervalsNum = this.ReschedulingArguments.ReschedulingRepaymentIntervalType == RepaymentIntervalTypes.Month ? MiscUtils.DateDiffInMonths(firstItemDate.Date, this.Result.LoanCloseDate.Date) : MiscUtils.DateDiffInWeeks(firstItemDate.Date, this.Result.LoanCloseDate.Date);

					// adjust intervals number +1 if needed
					DateTime rescheduledCloseDate = this.ReschedulingArguments.ReschedulingRepaymentIntervalType == RepaymentIntervalTypes.Month ? firstItemDate.AddMonths(this.Result.IntervalsNum) : firstItemDate.AddDays(this.Result.IntervalsNum * 7);

					Log.Debug("rescheduledCloseDate: {0}, Result.IntervalsNum: {1}, Result.LoanCloseDate: {2}", rescheduledCloseDate, this.Result.IntervalsNum, this.Result.LoanCloseDate.Date);

					TimeSpan ts = this.Result.LoanCloseDate.Date.Subtract(rescheduledCloseDate.Date);

					if (ts.Days > 0) 
						this.Result.IntervalsNum += 1;

					Log.Debug("rescheduledCloseDate: {0}, Result.IntervalsNum: {1}, Result.LoanCloseDate: {2}, dDays: {3}", rescheduledCloseDate, this.Result.IntervalsNum, this.Result.LoanCloseDate.Date, ts.Days);
				}

				// OUT
				if (this.ReschedulingArguments.RescheduleIn == false) {

					// too much payment per interval
					if (this.ReschedulingArguments.PaymentPerInterval > this.Result.ReschedulingBalance) {
						// ReSharper disable once PossibleInvalidOperationException
						this.message = string.Format("The entered amount accedes the outstanding balance of {0} for payment of {1}",
							this.Result.ReschedulingBalance.ToString("C2", this.cultureInfo), this.ReschedulingArguments.PaymentPerInterval.Value.ToString("C2", this.cultureInfo));
						this.Result.Error = this.message;
						ExitStrategy("Exit6");
						return;
					}

					// ReSharper disable once PossibleInvalidOperationException
					decimal m = (decimal)this.ReschedulingArguments.PaymentPerInterval;

					// System.DivideByZeroException: Attempted to divide by zero prevent
					decimal kDiv = (m - this.Result.ReschedulingBalance * r);
					if (kDiv == 0) {
						kDiv = 1;
					}

					var k = (int)Math.Ceiling(this.Result.ReschedulingBalance / kDiv); //(m - this.Result.ReschedulingBalance * r));

					Log.Debug("k: {0}, P: {1}, I: {2}, F: {3}, r: {4}, oustandingBalance: {5}, m: {6}", k, P, I, F, r, this.Result.ReschedulingBalance, m);

					// uncovered loan - too small payment per interval
					if (k < 0) {
						this.Result.Error = "Chosen amount is not sufficient for covering the loan overtime, i.e. accrued interest will be always greater than the repaid amount per payment";
						ExitStrategy("Exit7");
						return;
					}

					this.Result.LoanCloseDate = this.ReschedulingArguments.ReschedulingRepaymentIntervalType == RepaymentIntervalTypes.Month ? firstItemDate.AddMonths(k) : firstItemDate.AddDays(k * 7);

					this.Result.IntervalsNum = k;

					int n = (int)Math.Ceiling(P / (m - P * r));

					x = this.Result.ReschedulingBalance * r * (int)((k + 1) / 2) - P * r * (int)((n + 1) / 2);

					Log.Debug("n: {0}, k: {1}, P: {2}, I: {3}, F: {4}, r: {5}, oustandingBalance: {6}, m: {7}, X: {8}, closeDate: {9}, Result.IntervalsNum: {10}(==k)", n, k, P, I, F, r, this.Result.ReschedulingBalance, m, x, this.Result.LoanCloseDate, this.Result.IntervalsNum);
				}

				Log.Debug("close date: {0}, intervals: {1}", this.Result.LoanCloseDate, this.Result.IntervalsNum);

				if (this.Result.IntervalsNum == 0) {
					this.Result.Error = "Rescheduling impossible (calculated payments number 0)";
					this.Result.BlockAction = true;
					ExitStrategy("Exit8");
					return;
				}

				decimal balance = P;
				decimal iPrincipal = Decimal.Round(P / this.Result.IntervalsNum);
				decimal firstPrincipal = (P - iPrincipal * (this.Result.IntervalsNum - 1));

				//check "first iPrincipal negative" case: if first iPrincipal <= 0, remove this and reduce this.Result.IntervalsNum
				if (firstPrincipal <= 0) {
					Log.Debug("AAA Periods: {0}, newInstalment: {1}, close date: {2}, balance: {3}, firstItemDate: {4}, firstPrincipal: {5}, " +
									"P: {6}, I: {7}, F: {8}, r: {9}", this.Result.IntervalsNum, iPrincipal, this.Result.LoanCloseDate, this.Result.ReschedulingBalance, firstItemDate, firstPrincipal, P, I, F, r);
					this.Result.IntervalsNum -= 1;
					firstPrincipal = iPrincipal;

					if ((iPrincipal * (this.Result.IntervalsNum - 1) + firstPrincipal) != balance) {
						this.Result.Error = "Failed to create new schedule.";
						ExitStrategy("Exit9");
					}
				}

				Log.Debug("Periods: {0}, newInstalment: {1}, close date: {2}, balance: {3}, firstItemDate: {4}, firstPrincipal: {5}, " +
					"P: {6}, I: {7}, F: {8}, r: {9}", this.Result.IntervalsNum, iPrincipal, this.Result.LoanCloseDate, this.Result.ReschedulingBalance, firstItemDate, firstPrincipal, P, I, F, r);

				// add new re-scheduled items, both for IN/OUT
				int position = this.tLoan.Schedule.Count;
				for (int j = 0; j < this.Result.IntervalsNum; j++) {

					DateTime iStartDate = this.ReschedulingArguments.ReschedulingRepaymentIntervalType == RepaymentIntervalTypes.Month ? firstItemDate.AddMonths(j) : firstItemDate.AddDays(7 * j);

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
					ExitStrategy("Exit10");
					return;
				}

				Log.Debug("\n--------------Loan recalculated: \n {0}", this.tLoan);

				// prevent schedules with negative iPrincipal (i.e. LoanRepayment:-4.00)
				var negativeIPrincipal = this.tLoan.Schedule.FirstOrDefault(s => s.LoanRepayment < 0);
				if (negativeIPrincipal != null) {
					this.Result.Error = "Negative principal in loan schedule";
					ExitStrategy("Exit11");
					return;
				}

				// prevent "paidEarly" for newly created schedule items
				var newPaidEarly = this.tLoan.Schedule.FirstOrDefault(s => s.Date > this.ReschedulingArguments.ReschedulingDate && s.Status == LoanScheduleStatus.PaidEarly);
				if (newPaidEarly != null) {
					this.Result.Error = "Wrong balance for re-scheduling calculated. Please, contact support.";
					ExitStrategy("Exit12");
					return;
				}

				var firstRescheduledItem = this.tLoan.Schedule.FirstOrDefault(s => s.Date.Date == firstItemDate.Date);
				if (firstRescheduledItem != null) {
					this.Result.FirstPaymentInterest = firstRescheduledItem.Interest;
				}

				if (this.ReschedulingArguments.RescheduleIn == false) { // OUT

					// IS NOT POSSIBLE WITH OLD CALCULATOR< DON'T DELETE
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
						ExitStrategy("Exit13");
						return;
					}
				}

				if (!this.ReschedulingArguments.SaveToDB) {
					ExitStrategy("Exit14");
					return;
				}

				LoanRescheduleSave();

				// ReSharper disable once CatchAllClause
			} catch (Exception e) {
				Log.Alert(e, "Failed to get rescheduling data for loan {0}", this.ReschedulingArguments.LoanID);
			}
		}

		private void ExitStrategy(string logMessage) {
			Log.Debug(logMessage + ": " + this.Result.Error);
			this.loanRep.Clear();
			this.loanRep.RollbackTransaction();
		}

		/// <summary>
		///  distribute/offset X - the difference between earned interest for n(P) and k(P+I+F)
		/// </summary>
		/// <param name="X"></param>
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

					SendMail("Re-schedule rolled back", transactionEx);
				}

				this.loanRep.Clear();
			}
		}

		/// <summary>
		/// sending mail on re-schedule saving
		/// </summary>
		/// <param name="subject"></param>
		/// <param name="transactionEx"></param>
		private void SendMail(string subject, Exception transactionEx = null) {

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

			this.message = string.Format(
				"<h3>CustomerID: {0}; UserID: {1}</h3><p>"
				 + "<h4>Arguments</h4> {2}"
				 + "<h4>Result</h4> {3}"
				 + "<h4>Error</h4> {4}"
				 + "<h4>Loan state before action</h4> {5}"
				 + "<h4>Current schedule - after action</h4> {6}</p>",

				this.tLoan.Customer.Id, Context.UserID
				, (this.ReschedulingArguments)
				, (this.Result)
				, (transactionEx == null ? "NO errors" : transactionEx.ToString())
				, (sb.ToString().Length > 0) ? sb.ToString() : "not found"
				, (currentStateStr.ToString().Length > 0) ? currentStateStr.ToString() : "not found"
			);
			new Mail().Send(
				this.emailToAddress,
				null,					// message text
				this.message,			//html
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
		private readonly string	emailFromAddress;
		private readonly string	emailFromName;


	}
}