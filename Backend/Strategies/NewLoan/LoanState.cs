﻿namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DbConstants;
	using Ezbob.Backend.CalculateLoan.Models;
	using Ezbob.Backend.CalculateLoan.Models.Helpers;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Models.NewLoan;
	using Ezbob.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using StructureMap;

	//public enum BadCustomerStatuses {
	//	// name = Id in [dbo].[CustomerStatuses]
	//	Default = 1,
	//	Bad = 7,
	//	WriteOff = 8,
	//	DebtManagement = 9,
	//	Liquidation = 24,
	//	Bankruptcy = 26,
	//} // enum CustomerStatus

	/// <summary>
	/// Transform Loan or NL_Model to LoanCalculatorModel
	/// </summary>
	/// <typeparam name="T">Loan or NL_Model</typeparam>
	public class LoanState<T> : AStrategy {

		public LoanState(T t, int loanID, DateTime? stateDate) {

			this.loanID = loanID;

			if (t.GetType() == typeof(Loan)) {
				this.tLoan = t as Loan;
				this.tNLLoan = null;

			} else if (t.GetType() == typeof(NL_Model)) {
				this.tNLLoan = t as NL_Model;
				this.tLoan = null;
			}

			StateDate = stateDate ?? DateTime.UtcNow;
		}

		public override string Name { get { return "LoanState"; } }

		public DateTime StateDate { get; set; }

		public override void Execute() {
			try {

				// new loan structure
				if (this.tNLLoan != null) {

					// TODO
					// cal SP - load NL by id [NL_LoanStateLoad]

					// loan - flat data
					//		fees - list
					//		historeis - list
					//			history entry: 
					//				schedules - list
					//  logic payments - list
					 



					return;
				}

				if (this.tLoan != null) {

					LoanRepository loanRep = ObjectFactory.GetInstance<LoanRepository>();
					this.tLoan = loanRep.Get(this.loanID);

					this.customerID = this.tLoan.Customer.Id;

					Log.Debug("LoanState--->Loan1: \n {0}", this.tLoan);

					// init model - loan' properties
					this.CalcModel = new LoanCalculatorModel {
						LoanAmount = this.tLoan.LoanAmount,
						LoanIssueTime = this.tLoan.Date,
						RepaymentIntervalType = RepaymentIntervalTypes.Month, // default, old loan does not contain the property
						RepaymentCount = this.tLoan.Schedule.Count,
						MonthlyInterestRate = this.tLoan.InterestRate,
						InterestOnlyRepayments = 0 //InterestOnlyRepayments = this.Model.InterestOnlyRepaymentCount ?? 0 // TODO: exists in old loan?
					};

					// schedules
					foreach (LoanScheduleItem s in this.tLoan.Schedule) {
						ScheduledItem sch = new ScheduledItem(s.Date) { // DateTime scheduledDate
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
					SetBadPeriods();
				}

			} catch (Exception loanStateEx) {
				Log.Alert("Failed to load loan state err: {0}", loanStateEx);
			}

		}

		private void SetBadPeriods() {

			List<CustomerStatusTransition> statusesHistory = new List<CustomerStatusTransition>();

			DB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					statusesHistory.Add(sr.Fill<CustomerStatusTransition>());
					return ActionResult.Continue;
				},
				"LoadCustomerStatusHistory",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@CustomerID", this.customerID),
				new QueryParameter("@DateEnd", StateDate)
			);

			//	Console.WriteLine(statusesHistory.Count);

			//	statusesHistory.ForEach(x => Console.WriteLine(x.ToString()));

			if (statusesHistory.Count == 0)
				return;

			List<CustomerStatusTransition> badStarts = statusesHistory.Where(s => s.NewIsDefault).ToList();

			if (badStarts.Count == 0)
				return;

			List<BadPeriod> badsList = new List<BadPeriod>();

			//badStarts.ForEach(b => Console.WriteLine("started===================" + b));

			List<CustomerStatusTransition> badEnds = statusesHistory.Where(s => s.OldIsDefault).ToList();

			//badEnds.ForEach(bb => Console.WriteLine("ended--------------" + bb));

			CustomerStatusTransition lastEnd = new CustomerStatusTransition() { ChangeDate = StateDate };
			if (badEnds.Count > 0) {
				lastEnd.OldIsDefault = badEnds.Last()
					.NewIsDefault;
				lastEnd.NewIsDefault = lastEnd.NewIsDefault;
			}

			//	badEnds.Add(lastEnd);

			if (badStarts.Count > 0) {
				foreach (CustomerStatusTransition s in badStarts) {
					int index = badStarts.LastIndexOf(s);
					try {
						BadPeriod i = new BadPeriod(s.ChangeDate.Date, badEnds.ElementAt(index).ChangeDate.Date);
						if (!badsList.Contains(i))
							badsList.Add(i);
					} catch (Exception) {
						// ignored
					}
				}
			}

			if (badsList.Count > 0) {
				this.CalcModel.BadPeriods.AddRange(badsList);
			}
		}

		private Loan tLoan;
		private readonly NL_Model tNLLoan;
		private readonly int loanID;

		private int customerID;

		// result
		public LoanCalculatorModel CalcModel;

	}
}