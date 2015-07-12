namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Globalization;
	using System.Linq;
	using System.Web;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Backend.Models.NewLoan;
	using Ezbob.Utils;
	using EzBob.Models;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Database.UserManagement;
	using EZBob.DatabaseLib.Model.Loans;
	using MailApi;
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

			this.cultureInfo = new CultureInfo("en-GB");

			this.emailToAddress = CurrentValues.Instance.EzbobMailTo;
			this.emailFromAddress = CurrentValues.Instance.MailSenderEmail;
			this.emailFromName = CurrentValues.Instance.MailSenderName;
		}

		public override string Name { get { return "RescheduleLoan"; } }
		public ReschedulingArgument ReschedulingArguments;	// input
		public ReschedulingResult Result;	// output

		public override void Execute() {

			if (!this.ReschedulingArguments.RescheduleIn && this.ReschedulingArguments.PaymentPerInterval == null) {
				this.Result.Error = "Weekly/monthly payment amount for OUT rescheduling not provided";
				return;
			}

			try {

				GetCurrentLoanState();

				// check status, don't continue for "PaidOff"
				if (this.tLoan.Status == LoanStatus.PaidOff) {
					this.Result.Error = string.Format("Loan ID {0} paid off. Loan balance: {1}", this.tLoan.Id, 0m.ToString("C2", this.cultureInfo));
					return;
				}

				// input validation for "IN"
				if (this.ReschedulingArguments.RescheduleIn && (this.ReschedulingArguments.ReschedulingDate > this.Result.LoanCloseDate)) {
					this.Result.Error = "Within loan arrangement is impossible";
					return;
				}

				// if sent "default" value (0), replace by default calculated
				if (!this.ReschedulingArguments.RescheduleIn && this.ReschedulingArguments.PaymentPerInterval == 0)
					this.ReschedulingArguments.PaymentPerInterval = this.Result.DefaultPaymentPerInterval;

				Log.Debug("\n\n======RE-SCHEDULING===========ARGUMENTS:====={0} Context.UserID: {1}", this.ReschedulingArguments, Context.UserID);
				Log.Debug("==========================LoanState: {0} : \n {0}", this.tLoan);

				// check Marking loan {0} as 'PaidOff' in \ezbob\Integration\DatabaseLib\Model\Loans\Loan.cs(362)
				var calc = new LoanRepaymentScheduleCalculator(this.tLoan, DateTime.UtcNow, CurrentValues.Instance.AmountToChargeFrom);

				try {
					if (calc.NextEarlyPayment() == 0) {
						this.Result.Error = string.Format("Loan {0} marked as 'PaidOff'. Loan balance: {1}", this.tLoan.Id, 0m.ToString("C2", this.cultureInfo));
						return;
					}
					// ReSharper disable once CatchAllClause
				} catch (Exception calcEx) {
					Log.Info("LoanRepaymentScheduleCalculator NextEarlyPayment EXCEPTION: {0}", calcEx.Message);
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

				decimal I = calc.NextEarlyPayment();
				decimal P = this.tLoan.Principal;
				decimal F = this.tLoan.Charges.Sum(f => f.Amount);
				decimal r = this.tLoan.InterestRate;
				// ReSharper disable once TooWideLocalVariableScope
				decimal x = 0m;
				this.Result.ReschedulingBalance = (P + I + F);
				DateTime firstItemDate = this.ReschedulingArguments.ReschedulingDate.AddDays(1);

				// OUT
				if (this.ReschedulingArguments.RescheduleIn == false) {

					// too much payment per interval
					if (this.ReschedulingArguments.PaymentPerInterval > this.Result.ReschedulingBalance) {
						// ReSharper disable once PossibleInvalidOperationException
						this.message = string.Format("The entered amount accedes the outstanding balance of {0} for payment of {1}",
							this.Result.ReschedulingBalance.ToString("C2", this.cultureInfo), this.ReschedulingArguments.PaymentPerInterval.Value.ToString("C2", this.cultureInfo));
						this.Result.Error = this.message;
						return;
					}

					// ReSharper disable once PossibleInvalidOperationException
					decimal m = (decimal)this.ReschedulingArguments.PaymentPerInterval;

					var k = (int)Math.Ceiling(this.Result.ReschedulingBalance / (m - this.Result.ReschedulingBalance * r));

					Log.Debug("k: {0}, P: {1}, I: {2}, F: {3}, r: {4}, oustandingBalance: {5}, m: {6}", k, P, I, F, r, this.Result.ReschedulingBalance, m);

					// uncovered loan - too small payment per interval
					if (k < 0) {
						this.Result.Error = "Chosen amount is not sufficient for covering the loan overtime, i.e. accrued interest will be always greater than the repaid amount per payment";
						return;
					}

					this.Result.LoanCloseDate = firstItemDate.AddMonths(k);

					int n = (int)Math.Ceiling(P / (m - P * r));

					x = this.Result.ReschedulingBalance * r * (int)((k + 1) / 2) - P * r * (int)((n + 1) / 2);

					Log.Debug("n: {0}, k: {1}, P: {2}, I: {3}, F: {4}, r: {5}, oustandingBalance: {6}, m: {7}, X: {8}", n, k, P, I, F, r, this.Result.ReschedulingBalance, m, x);
				}

				// 3. intervals number
				this.Result.IntervalsNum = this.ReschedulingArguments.ReschedulingRepaymentIntervalType == RepaymentIntervalTypes.Month ?
					MiscUtils.DateDiffInMonths(firstItemDate, this.Result.LoanCloseDate.Date) : MiscUtils.DateDiffInWeeks(firstItemDate, this.Result.LoanCloseDate.Date);

				if (this.Result.IntervalsNum == 0) {
					this.Result.Error = "Rescheduling impossible (calculated payments number 0)";
					return;
				}

				decimal balance = P;
				decimal iPrincipal = Decimal.Round(P / this.Result.IntervalsNum);
				decimal firstPrincipal = (P - iPrincipal * (this.Result.IntervalsNum - 1));

				Log.Debug("Periods: {0}, newInstalment: {1}, close date: {2}, balance: {3}, firstItemDate: {4}, firstPrincipal: {5}", this.Result.IntervalsNum, iPrincipal, this.Result.LoanCloseDate, this.Result.ReschedulingBalance, firstItemDate, firstPrincipal);

				// add new re-scheduled items, both for IN/OUT
				int position = this.tLoan.Schedule.Count;
				for (int j = 0; j < this.Result.IntervalsNum; j++) {

					DateTime iStartDate = this.ReschedulingArguments.ReschedulingRepaymentIntervalType == RepaymentIntervalTypes.Month ? firstItemDate.AddMonths(j) : firstItemDate.AddDays(7 * j);
					DateTime iEndDate = this.ReschedulingArguments.ReschedulingRepaymentIntervalType == RepaymentIntervalTypes.Month ? iStartDate.AddMonths(1) : firstItemDate.AddDays(7);
					decimal iInterestRate = calc.GetInterestRate(iStartDate, iEndDate);
					//Log.Debug("Start: {0}, End: {1}, Rate: {2}", iStartDate, itEndDate, interestRate);

					decimal iLoanRepayment = (j == 0) ? firstPrincipal : iPrincipal;
					balance -= iLoanRepayment;

					LoanScheduleItem item = new LoanScheduleItem() {
						Date = iStartDate.Date,
						InterestRate = r,
						Status = LoanScheduleStatus.StillToPay,
						Loan = this.tLoan,
						LoanRepayment = iLoanRepayment,
						Interest = iInterestRate * iLoanRepayment,
						Balance = balance,
						Position = ++position
					};
					this.tLoan.Schedule.Add(item);
				}

				Log.Debug("--------------Loan modified: \n {0}", this.tLoan);

				//  after modification
				if (CheckValidateLoanState(calc) == false)
					return;

				Log.Debug("--------------Loan recalculated: \n {0}", this.tLoan);

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
						this.message = string.Format("{0}ly payment of {1} not sufficient to pay the loan outstanding balance " +
							//"(in {2} new payments). " +
							"Accrued interest: {2}, accumulated fees: {3}, first new instalment: {4}. " +
							"You can choose to reduce the accumulated fees & interest by clearing them via manual payment, before setting the new payment schedule.",
							this.ReschedulingArguments.ReschedulingRepaymentIntervalType,
							this.ReschedulingArguments.PaymentPerInterval.Value.ToString("C2", this.cultureInfo),
							//this.Result.IntervalsNum,
							overInstalment.Interest.ToString("C2", this.cultureInfo), //I.ToString("C2", this.cultureInfo),
							overInstalment.Fees.ToString("C2", this.cultureInfo),
							overInstalment.AmountDue.ToString("C2", this.cultureInfo)
							);
						this.Result.Error = this.message;
						return;
					}
				}

				if (!this.ReschedulingArguments.SaveToDB)
					return;

				LoanRescheduleSave();

				// ReSharper disable once CatchAllClause
			} catch (Exception e) {
				Log.Alert(e, "Failed to get rescheduling data for loan {0}", this.ReschedulingArguments.LoanID);
			}
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
				this.Result.LoanInterestRate = this.tLoan.InterestRate;
				LoanScheduleItem lastScheduleItem = this.tLoan.Schedule.OrderBy(s => s.Date).LastOrDefault();
				if (lastScheduleItem != null) {
					this.Result.LoanCloseDate = lastScheduleItem.Date; // 'maturity date'
				}

				//Log.Debug("==========================LoanState: {0} : \n {0}", this.tLoan);

				var defaultPaymentPerInterval = this.tLoan.Schedule.OrderBy(s => s.Date).LastOrDefault();
				this.Result.DefaultPaymentPerInterval = defaultPaymentPerInterval == null ? 0 : defaultPaymentPerInterval.LoanRepayment;

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

				//save LoanChangesHistory (loan state before changes) before re-schedule
				this.loanHistory.User = ObjectFactory.GetInstance<UsersRepository>().Get(Context.UserID);
				ObjectFactory.GetInstance<LoanChangesHistoryRepository>().Save(this.loanHistory);

				try {

					this.tLoan.Status = LoanStatus.Late;
					this.tLoan.LastRecalculation = DateTime.UtcNow;
					this.tLoan.Modified = true;

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
			this.message = string.Format(
				"<h3>CustomerID: {0}; UserID: {1}</h3><p>"
				 + "<h4>Arguments</h4>: {2} <br/>"
				 + "<h4>Result</h4>: {3} <br/>"
				 + "<h4>ERROR</h4>: {4} <br/></p>",

				this.tLoan.Customer.Id, Context.UserID
				, (this.ReschedulingArguments)
				, (this.Result)
				, (transactionEx == null ? "NO errors" : transactionEx.ToString())
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