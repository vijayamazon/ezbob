namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Linq;
	using DbConstants;
	using Ezbob.Backend.CalculateLoan.Models;
	using Ezbob.Backend.CalculateLoan.Models.Helpers;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Models.NewLoan;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using StructureMap;

	/// <summary>
	/// Transform Loan or NL_Model to LoanCalculatorModel
	/// </summary>
	public class LoanState : AStrategy {
		public LoanState(NL_Model t, long loanID, int customerId, DateTime? stateDate) {
			this.loanID = loanID;
			this.customerID = customerId;

			this.tNLLoan = t;

			StateDate = stateDate ?? DateTime.UtcNow;
		} // constructor

		public override string Name { get { return "LoanState"; } }

		public DateTime StateDate { get; set; }

		// Result.
		public LoanCalculatorModel CalcModel { get; private set; }

		public override void Execute() {
			try {
				LoadNewLoanStructure();
			} catch (Exception loanStateEx) {
				Log.Alert(loanStateEx, "Failed to load loan state.");
			} // try
		} // Execute

		private void LoadNewLoanStructure() {
			// TODO: revive

			/*
			NL_Loans loan = DB.FillFirst<NL_Loans>(
				"NL_LoansGet",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@loanID", this.loanID),
				new QueryParameter("@Now", StateDate)
			);

			// Init loan's properties.
			this.CalcModel = new LoanCalculatorModel {
				LoanAmount = loan.InitialLoanAmount,
				LoanIssueTime = loan.IssuedTime,
				RepaymentIntervalType = (RepaymentIntervalTypes)loan.RepaymentIntervalTypeID,
				RepaymentCount = loan.RepaymentCount,
				MonthlyInterestRate = loan.InterestRate,
				InterestOnlyRepayments = loan.InterestOnlyRepaymentCount,
			};

			List<NL_LoanHistory> history = DB.Fill<NL_LoanHistory>(
				"NL_LoanHistoryGet",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@LoanID", this.loanID),
				new QueryParameter("@Now", StateDate)
			);

			foreach (NL_LoanHistory h in history) {
				this.CalcModel.OpenPrincipalHistory.Add(new OpenPrincipal {
					Date = h.EventTime,
					Amount = h.Amount,
				});
			} // for each

			List<NL_LoanSchedules> schedules = DB.Fill<NL_LoanSchedules>(
				"NL_LoanSchedulesGet",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@LoanID", this.loanID),
				new QueryParameter("@Now", StateDate)
			);

			// Schedules.
			foreach (NL_LoanSchedules s in schedules) {
				ScheduledItem sch = new ScheduledItem(s.PlannedDate) {
					Date = s.PlannedDate,
					ClosedDate = s.ClosedTime,
					Principal = s.Principal,
					InterestRate = s.InterestRate,
				};
				this.CalcModel.Schedule.Add(sch);
			} // for each

			DB.ForEachRowSafe(
				sr => {
					this.CalcModel.Repayments.Add(new Repayment(sr["Time"], sr["Principal"], sr["Interest"], sr["Fees"]));
				},
				"NL_PaymentsGet",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@LoanID", this.loanID),
				new QueryParameter("@Now", StateDate)
			);

			DB.ForEachRowSafe(
				sr => {
					this.CalcModel.Fees.Add(new Fee(sr["AssignTime"], sr["Amount"], (FeeTypes)(int)sr["LoanFeeTypeID"]));
				},
				"NL_LoansFeesGet",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@LoanID", this.loanID),
				new QueryParameter("@Now", StateDate)
			);

			DB.ForEachRowSafe(
				sr => {
					DateTime? start = sr["StartDate"];
					DateTime? end = sr["EndDate"];
					DateTime? deactivation = sr["DeactivationDate"];
					DateTime? activation = sr["ActivationDate"];

					if ((start == null) || (end == null))
						return;

					bool isActive = deactivation.HasValue
						? (activation <= StateDate) && (deactivation >= StateDate)
						: (activation <= StateDate);

					this.CalcModel.FreezePeriods.Add(
						new InterestFreeze(start.Value, end.Value, sr["InterestRate"], isActive)
					);
				},
				"NL_InterestFreezeGet",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@LoanID", this.loanID)
			);

			SetBadPeriods();
			*/
		} // LoadNewLoanStructure

		private void SetBadPeriods() {
			List<CustomerStatusTransition> statusesHistory = new List<CustomerStatusTransition>();

			DB.ForEachRowSafe(
				sr => statusesHistory.Add(sr.Fill<CustomerStatusTransition>()),
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

			// badStarts.ForEach(b => Console.WriteLine("started===================" + b));

			List<CustomerStatusTransition> badEnds = statusesHistory.Where(s => s.OldIsDefault).ToList();

			// badEnds.ForEach(bb => Console.WriteLine("ended--------------" + bb));

			CustomerStatusTransition lastEnd = new CustomerStatusTransition { ChangeDate = StateDate };
			if (badEnds.Count > 0) {
				lastEnd.OldIsDefault = badEnds.Last().NewIsDefault;
				lastEnd.NewIsDefault = lastEnd.NewIsDefault;
			} // if

			// badEnds.Add(lastEnd);

			if (badStarts.Count > 0) {
				foreach (CustomerStatusTransition s in badStarts) {
					int index = badStarts.LastIndexOf(s);
					try {
						BadPeriod i = new BadPeriod(s.ChangeDate.Date, badEnds.ElementAt(index).ChangeDate.Date);

						if (!badsList.Contains(i))
							badsList.Add(i);
					} catch {
						// Ignored.
					} // try
				} // for each
			} // if

			if (badsList.Count > 0)
				this.CalcModel.BadPeriods.AddRange(badsList);
		} // SetBadPeriods

		private readonly NL_Model tNLLoan;
		private readonly long loanID;
		private readonly int customerID;
	} // class LoanState
} // namespace
