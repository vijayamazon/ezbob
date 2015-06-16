namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DbConstants;
	using Ezbob.Backend.CalculateLoan.Models;
	using Ezbob.Backend.CalculateLoan.Models.Helpers;
	using Ezbob.Backend.Models.NewLoan;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using global::Reports;
	using NHibernate.Linq;
	using StructureMap;

	public enum BadCustomerStatuses {
		// name = Id in [dbo].[CustomerStatuses]
		Default = 1,
		Bad = 7,
		WriteOff = 8,
		DebtManagement = 9,
		Liquidation = 24,
		Bankruptcy = 26
	} // enum CustomerStatus

	/// <summary>
	/// Transform Loan or NL_Model to LoanCalculatorModel
	/// </summary>
	/// <typeparam name="T">Loan or NL_Model</typeparam>
	public class LoanState<T> : AStrategy {

		public LoanState(T t, int loanID, DateTime? stateDate) {

			this.loanID = loanID;

			if (t.GetType() == typeof(Loan)) {
				this.tLoan = t as Loan;
			} else if (t.GetType() == typeof(NL_Model)) {
				this.tNLLoan = t as NL_Model;
			}

			StateDate = stateDate ?? DateTime.UtcNow;
			this.badStatusesNames = Enum.GetNames(typeof(BadCustomerStatuses));
		}

		public DateTime StateDate { get; set; }

		public override string Name { get { return "LoanState"; } }

		public override void Execute() {

			/*	List<OpenPrincipal> openPrincipalHistory = new List<OpenPrincipal>();
				List < decimal> discountPlan = new List<decimal>();
				//List<ScheduledItem> schedules = new List<ScheduledItem>();
				//List<Repayment> repayments = new List<Repayment>();
				List<Fee> fees = new List<Fee>();
				BadPeriods badPeriods = new BadPeriods();*/

			/*// schedules
	this.tLoan.Schedule.ForEach(s => Log.Debug(s.ToString()));
	Console.WriteLine();
	// all transactions
	this.tLoan.Transactions.ForEach(s => Log.Debug(s.ToString()));
	Console.WriteLine();
	// Paypoint transactions
	this.tLoan.TransactionsWithPaypoint.ForEach(s => Log.Debug(s.ToString()));
	Console.WriteLine();
	// successfull Paypoint transactions
	this.tLoan.TransactionsWithPaypointSuccesefull.ForEach(s => Log.Debug(s.ToString()));
	Console.WriteLine();
	// charges
	this.tLoan.Charges.ForEach(s => Log.Debug(s.ToString()));
	Console.WriteLine();
	// Schedule Transactions
	this.tLoan.ScheduleTransactions.ForEach(s => Log.Debug(s.ToString()));
	Console.WriteLine();
	// Rollovers
	this.tLoan.Schedule.SelectMany(s => s.Rollovers).OrderBy(c => c.Created).ForEach(s => Log.Debug(s.ToString()));
	*/
			/*	Transactions
				  TransactionsWithPaypoint
				  ScheduleTransactions
				  PacnetTransactions
				  InterestFreeze
				  ActiveInterestFreeze
			 
				  TransactionsWithPaypointSuccesefull - Transactions
				  Charges - Fees
				  Schedule - Schedule					
				  Rollovers - Schedule.SelectMany(s => s.Rollovers) 
			  */

			if (this.tLoan != null) {

				LoanRepository loanRep = ObjectFactory.GetInstance<LoanRepository>();
				this.tLoan = loanRep.Get(this.loanID);


				Log.Debug("LoanState--->Loan1: \n {0}", this.tLoan);

				this.CalcModel = new LoanCalculatorModel {
					LoanAmount = this.tLoan.LoanAmount,
					LoanIssueTime = this.tLoan.Date,
					RepaymentIntervalType = RepaymentIntervalTypes.Month,  // default, old loan does not contain the property
					RepaymentCount = this.tLoan.Schedule.Count,
					MonthlyInterestRate = this.tLoan.InterestRate,
					InterestOnlyRepayments = 0 //InterestOnlyRepayments = this.Model.InterestOnlyRepaymentCount ?? 0 // TODO: exists in old loan?
				};

				// schedules
				foreach (LoanScheduleItem s in this.tLoan.Schedule) {
					ScheduledItem sch= new ScheduledItem(s.Date) { // DateTime scheduledDate
						Date = s.Date,
						ClosedDate = null, //???
						Principal = s.Principal,
						InterestRate = s.InterestRate
					};
					this.CalcModel.Schedule.Add(sch);
				}
				
				// successfull paypoint transactions
				foreach (PaypointTransaction p in this.tLoan.TransactionsWithPaypointSuccesefull) {
					this.CalcModel.Repayments.Add(new Repayment( // DateTime time, decimal principal, decimal interest, decimal fees
						p.PostDate, p.LoanRepayment, p.Interest, p.Fees
					));
				}

				// fees 
				foreach (var c in this.tLoan.Charges) {
					this.CalcModel.Fees.Add(new Fee( // DateTime assignTime, decimal amount
						c.Date, c.Amount
					));
				}

				// interest freeze
				foreach (LoanInterestFreeze fi in this.tLoan.ActiveInterestFreeze) {

					// time interval not defined well
					if (fi.StartDate == null)
						continue;

					// time interval not defined well
					if (fi.EndDate == null)
						continue;

					bool isActive;

					if (fi.DeactivationDate.HasValue)
						isActive = (fi.ActivationDate <= StateDate) && (fi.DeactivationDate >= StateDate);
					else
						isActive = (fi.ActivationDate <= StateDate);

					this.CalcModel.FreezePeriods.Add( // DateTime start, DateTime end, decimal? nInterestRate, bool isActive
						new InterestFreeze(startDate: (DateTime)fi.StartDate, endDate: (DateTime)fi.EndDate, interestRate: fi.InterestRate, isActive: isActive)
					);
				}
	
				// "bad" periods
				SetBadPariods();

				// rollovers
				//IEnumerable<PaymentRollover> rollovers = this.tLoan.Schedule.SelectMany(s => s.Rollovers).Where(s=>s.CustomerConfirmationDate!=null);
				//foreach (PaymentRollover r in rollovers) { // DateTime created, DateTime expired, decimal fee
				//	this.CalcModel.Rollovers = new Rollovers(r.Created, r.ExpiryDate, r.Payment);
				//}
				//Log.Debug(this.CalcModel.ToString());

				return;
			}

			// new loan structure
			if (this.tNLLoan != null) {
				// TODO
				// cal SP - load nl by id
			}

		}

		private void SetBadPariods() {
			CustomerStatusHistory customerStatusesHistory = new CustomerStatusHistory(this.tLoan.Customer.Id, StateDate, DB);

			List<CustomerStatusChange> statusesHistory = new List<CustomerStatusChange>();

			foreach (KeyValuePair<int, List<CustomerStatusChange>> pair in customerStatusesHistory.FullData.Data)
				statusesHistory = pair.Value;

			if (statusesHistory.Count > 0) {

//statusesHistory.ForEach(xx => Log.Debug(xx));

				List<CustomerStatusChange> badStarts = statusesHistory.Where(s => this.badStatusesNames.Contains<string>(s.NewStatus.ToString())).ToList();

				if(badStarts.Count == 0)
					return;

				List<BadPeriod> badsList = new List<BadPeriod>();

//badStarts.ForEach(b => Console.WriteLine("started===================" + b));

				List<CustomerStatusChange> badEnds = statusesHistory.Where(s => this.badStatusesNames.Contains(s.OldStatus.ToString())).ToList();

				//Console.WriteLine("ends count==========" + badEnds.Count);
//badEnds.ForEach(bb => Console.WriteLine("ended--------------" + bb));

				CustomerStatusChange lastEnd = new CustomerStatusChange() {ChangeDate = StateDate};
				if (badEnds.Count > 0) {
					lastEnd.OldStatus = badEnds.Last().NewStatus;
					lastEnd.NewStatus = lastEnd.OldStatus;
				}

				badEnds.Add(lastEnd);

				if (badStarts.Count > 0) {
					foreach (CustomerStatusChange s in badStarts) {
						int index = badStarts.LastIndexOf(s);
						try {
							BadPeriod i = new BadPeriod(s.ChangeDate.Date, badEnds.ElementAt(index).ChangeDate.Date);
							if (!badsList.Contains(i))
								badsList.Add(i);
						} catch (Exception e) {
							// ignored
						}
					}
				}

				if (badsList.Count > 0) {
					this.CalcModel.BadPeriods.AddRange(badsList);
				}
			}
		}

		private Loan tLoan;
		private readonly NL_Model tNLLoan;
		private readonly int loanID;
		private readonly string[] badStatusesNames;

		// result
		public LoanCalculatorModel CalcModel;

	}
}