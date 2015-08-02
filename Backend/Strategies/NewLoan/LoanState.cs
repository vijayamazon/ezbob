namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Collections.Generic;
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
	/// <typeparam name="T">Loan or NL_Model</typeparam>
	public class LoanState<T> : AStrategy {
		public LoanState(T t, int loanID, int customerId, DateTime? stateDate) {
			this.loanID = loanID;
			this.customerID = customerId;

			if (t.GetType() == typeof(Loan)) {
				this.tLoan = t as Loan;
				this.tNLLoan = null;
			} else if (t.GetType() == typeof(NL_Model)) {
				this.tNLLoan = t as NL_Model;
				this.tLoan = null;
			} // if

			StateDate = stateDate ?? DateTime.UtcNow;
		} // constructor

		public override string Name { get { return "LoanState"; } }

		public DateTime StateDate { get; set; }

		// Result.
		public LoanCalculatorModel CalcModel;

		public override void Execute() {
			try {
				if (this.tNLLoan != null)
					LoadNewLoanStructure();
				else if (this.tLoan != null)
					LoadOldLoanStructure();
			} catch (Exception loanStateEx) {
				Log.Alert(loanStateEx, "Failed to load loan state.");
			} // try
		} // Execute

		private void LoadOldLoanStructure() {
			LoanRepository loanRep = ObjectFactory.GetInstance<LoanRepository>();
			this.tLoan = loanRep.Get(this.loanID);

			Log.Debug("LoanState--->Loan1: \n {0}", this.tLoan);

			// Init loan's properties
			this.CalcModel = new LoanCalculatorModel {
				LoanAmount = this.tLoan.LoanAmount,
				LoanIssueTime = this.tLoan.Date,
				RepaymentIntervalType = RepaymentIntervalTypes.Month, // default, old loan does not contain the property
				RepaymentCount = this.tLoan.Schedule.Count,
				MonthlyInterestRate = this.tLoan.InterestRate,
				InterestOnlyRepayments = 0, // default, old loan does not contain the property
			};

			// Schedules.
			foreach (LoanScheduleItem s in this.tLoan.Schedule) {
				ScheduledItem sch = new ScheduledItem(s.Date) {
					Date = s.Date,
					ClosedDate = null, //???
					Principal = s.Principal,
					InterestRate = s.InterestRate
				};

				this.CalcModel.Schedule.Add(sch);
			} // for each

			// Successful incoming transactions.
			foreach (PaypointTransaction p in this.tLoan.TransactionsWithPaypointSuccesefull)
				this.CalcModel.Repayments.Add(new Repayment(p.PostDate, p.LoanRepayment, p.Interest, p.Fees));

			// Fees.
			foreach (var c in this.tLoan.Charges)
				this.CalcModel.Fees.Add(new Fee(c.Date, c.Amount, FeeTypes.AdminFee));

			// Interest freeze periods.
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

				this.CalcModel.FreezePeriods.Add(
					new InterestFreeze((DateTime)fi.StartDate, (DateTime)fi.EndDate, fi.InterestRate, isActive)
				);
			} // for each

			SetBadPeriods();
		} // LoadOldLoanStructure

		private void LoadNewLoanStructure() {
			NL_Loans loan = DB.FillFirst<NL_Loans>(
				"NL_LoansGet",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@loanID", this.loanID)
			);

			// Init loan's properties.
			this.CalcModel = new LoanCalculatorModel {
				LoanAmount = loan.InitialLoanAmount,
				LoanIssueTime = loan.IssuedTime,
				RepaymentIntervalType = (RepaymentIntervalTypes)loan.RepaymentIntervalTypeID,
				RepaymentCount = loan.RepaymentCount,
				MonthlyInterestRate = loan.InterestRate,
				InterestOnlyRepayments = loan.InterestOnlyRepaymentCount ?? 0,
			};

			List<NL_LoanSchedules> schedules = DB.Fill<NL_LoanSchedules>(
				"NL_LoanSchedulesGet",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@LoanID", this.loanID)
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
				new QueryParameter("@LoanID", this.loanID)
			);

			DB.ForEachRowSafe(
				sr => {
					this.CalcModel.Fees.Add(new Fee(sr["AssignTime"], sr["Amount"], (FeeTypes)(int)sr["LoanFeeTypeID"]));
				},
				"NL_LoansFeesGet",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@LoanID", this.loanID)
			);

			DB.ForEachRowSafe(
				sr => {
					DateTime? start = sr["StartDate"];
					DateTime? end = sr["EndDate"];
					DateTime? deactivation = sr["DeactivationDate"];
					DateTime? activation = sr["ActivationDate"];

					if ((start != null) && (end != null)) {
						bool isActive = deactivation.HasValue
							? (activation <= StateDate) && (deactivation >= StateDate)
							: (activation <= StateDate);

						this.CalcModel.FreezePeriods.Add(
							new InterestFreeze(start.Value, end.Value, sr["InterestRate"], isActive)
						);
					} // if
				},
				"NL_InterestFreezeGet",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@LoanID", this.loanID)
			);

			SetBadPeriods();
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

		private Loan tLoan;
		private readonly NL_Model tNLLoan;
		private readonly int loanID;
		private readonly int customerID;
	} // class LoanState
} // namespace
