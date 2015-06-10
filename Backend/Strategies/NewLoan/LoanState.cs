namespace Ezbob.Backend.Strategies.NewLoan {
	using System.Collections.Generic;
	using System.Linq;
	using DbConstants;
	using Ezbob.Backend.CalculateLoan.Models;
	using Ezbob.Backend.CalculateLoan.Models.Helpers;
	using Ezbob.Backend.Models.NewLoan;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Loans;
	using StructureMap;

	public class LoanState<T> : AStrategy {

		public LoanState(T t, int loanID) {

			this.loanID = loanID;

			if (t.GetType() == typeof(Loan)) {
				this.tLoan = t as Loan;
			} else if (t.GetType() == typeof(NL_Model)) {
				this.tNLLoan = t as NL_Model;
			}
		}

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

			/*  Transactions
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

				Log.Debug("===========================================>Loan1: \n {0}", this.tLoan);

				this.CalcModel = new LoanCalculatorModel {
					LoanAmount = this.tLoan.LoanAmount,
					LoanIssueTime = this.tLoan.Date,
					RepaymentIntervalType = RepaymentIntervalTypes.Month, // TODO put real here  //(RepaymentIntervalTypes)Enum.ToObject(typeof(RepaymentIntervalTypesId), Model.RepaymentIntervalTypeID),
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
						p.PostDate,
						p.LoanRepayment,
						p.Interest,
						p.Fees
					));
				}
				
				// fees 
				foreach (var c in this.tLoan.Charges) {
					this.CalcModel.Fees.Add(new Fee( // DateTime assignTime, decimal amount
						c.Date,
						c.Amount
					));
				}

				// rollovers
				IEnumerable<PaymentRollover> rollovers = this.tLoan.Schedule.SelectMany(s => s.Rollovers).Where(s=>s.CustomerConfirmationDate!=null);

				foreach (PaymentRollover r in rollovers) {
					// DateTime created, DateTime expired, decimal fee
					this.CalcModel.Rollovers = new Rollovers(r.Created, r.ExpiryDate, r.Payment);
				}

				Log.Debug(this.CalcModel.ToString());

				return;
			}

			// new loan structure
			if (this.tNLLoan != null) {
				// cal SP - load nl loan by id
			}

		}

		private Loan tLoan;
		private readonly NL_Model tNLLoan;
		private readonly int loanID;

		// result
		public LoanCalculatorModel CalcModel;


	}


}