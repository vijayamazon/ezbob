namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Database;

	/// <summary>
	/// Load NL Loan from DB into NL_Model 
	/// </summary>
	public class LoanState : AStrategy {

		public LoanState(NL_Model nlModel, long loanID, DateTime? stateDate) {

			Result = nlModel;
			this.loanID = loanID;

			StateDate = stateDate ?? DateTime.UtcNow;
		} // constructor

		public override string Name { get { return "LoanState"; } }

		public NL_Model Result { get; private set; }
		private readonly long loanID;
		public DateTime StateDate { get; set; }
		public string Error;

		public override void Execute() {
			try {

				// loan
				Result.Loan = new NL_Loans();
				Result.Loan = DB.FillFirst<NL_Loans>("NL_LoansGet",
					CommandSpecies.StoredProcedure,
					new QueryParameter("@loanID", this.loanID)
					//,new QueryParameter("@Now", StateDate)
				);

				// histories
				Result.Loan.Histories.Clear();
				Result.Loan.Histories = DB.Fill<NL_LoanHistory>("NL_LoanHistoryGet",
					CommandSpecies.StoredProcedure,
					new QueryParameter("@LoanID", this.loanID),
					new QueryParameter("@Now", StateDate)
				);

				// schedules
				foreach (NL_LoanHistory h in Result.Loan.Histories) {
					h.Schedule = DB.Fill<NL_LoanSchedules>("NL_LoanSchedulesGet",
						CommandSpecies.StoredProcedure,
						new QueryParameter("@LoanID", this.loanID),
						new QueryParameter("@Now", StateDate)
					);
				}

				// loan fees
				Result.Loan.Fees.Clear();
				Result.Loan.Fees = DB.Fill<NL_LoanFees>("NL_LoansFeesGet",
					CommandSpecies.StoredProcedure,
					new QueryParameter("@LoanID", this.loanID)
					//,new QueryParameter("@Now", StateDate)
				);

				// interest freezes
				Result.Loan.FreezeInterestIntervals.Clear();
				Result.Loan.FreezeInterestIntervals = DB.Fill<NL_LoanInterestFreeze>("NL_LoanInterestFreezeGet",
					CommandSpecies.StoredProcedure,
					new QueryParameter("@LoanID", this.loanID)
				);

				// loan options
				Result.Loan.LoanOptions.Clear();
				Result.Loan.LoanOptions = DB.Fill<NL_LoanOptions>("NL_LoanOptionsGet",
					CommandSpecies.StoredProcedure,
					new QueryParameter("@LoanID", this.loanID)
				);

				// payments (logical loan transactions)
				Result.Loan.Payments.Clear();
				Result.Loan.Payments = DB.Fill<NL_Payments>("NL_PaymentsGet",
					CommandSpecies.StoredProcedure,
					new QueryParameter("@LoanID", this.loanID),
					new QueryParameter("@Now", StateDate)
				);

				if (Result.Loan.Payments.Count > 0) {

					Result.Loan.SchedulePayments.Clear();
					Result.Loan.SchedulePayments = DB.Fill<NL_LoanSchedulePayments>("NL_LoansSchedulePaymentsGet",
						CommandSpecies.StoredProcedure,
						new QueryParameter("@LoanID", this.loanID)
					);

					Result.Loan.FeePayments.Clear();
					Result.Loan.FeePayments = DB.Fill<NL_LoanFeePayments>("NL_LoanFeePaymentsGet",
						CommandSpecies.StoredProcedure,
						new QueryParameter("@LoanID", this.loanID)
					);
				}

				SetBadPeriods(); // TODO

				// ReSharper disable once CatchAllClause
			} catch (Exception loanStateEx) {
				Log.Alert(loanStateEx, "Failed to load loan state.");
			} // try
		} // Execute

		private void LoadNewLoanStructure() {

			// Init loan's properties.
			//this.CalcModel = new LoanCalculatorModel {
			//	LoanAmount = loan.InitialLoanAmount,
			//	LoanIssueTime = loan.IssuedTime,
			//	RepaymentIntervalType = (RepaymentIntervalTypes)loan.RepaymentIntervalTypeID,
			//	RepaymentCount = loan.RepaymentCount,
			//	MonthlyInterestRate = loan.InterestRate,
			//	InterestOnlyRepayments = loan.InterestOnlyRepaymentCount,
			//};

			//List<NL_LoanHistory> history = DB.Fill<NL_LoanHistory>(
			//	"NL_LoanHistoryGet",
			//	CommandSpecies.StoredProcedure,
			//	new QueryParameter("@LoanID", this.loanID),
			//	new QueryParameter("@Now", StateDate)
			//);

			//foreach (NL_LoanHistory h in history) {
			//	this.CalcModel.OpenPrincipalHistory.Add(new OpenPrincipal {
			//		Date = h.EventTime,
			//		Amount = h.Amount,
			//	});
			//} // for each

			//List<NL_LoanSchedules> schedules = DB.Fill<NL_LoanSchedules>(
			//	"NL_LoanSchedulesGet",
			//	CommandSpecies.StoredProcedure,
			//	new QueryParameter("@LoanID", this.loanID),
			//	new QueryParameter("@Now", StateDate)
			//);

			//// Schedules.
			//foreach (NL_LoanSchedules s in schedules) {
			//	ScheduledItem sch = new ScheduledItem(s.PlannedDate) {
			//		Date = s.PlannedDate,
			//		ClosedDate = s.ClosedTime,
			//		Principal = s.Principal,
			//		InterestRate = s.InterestRate,
			//	};
			//	this.CalcModel.Schedule.Add(sch);
			//} // for each

			/*DB.ForEachRowSafe(
				sr => {
					this.CalcModel.Repayments.Add(new Repayment(sr["Time"], sr["Principal"], sr["Interest"], sr["Fees"]));
				},
				"NL_PaymentsGet",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@LoanID", this.loanID),
				new QueryParameter("@Now", StateDate)
			);*/

			//DB.ForEachRowSafe(
			//	sr => {
			//		this.CalcModel.Fees.Add(new Fee(sr["AssignTime"], sr["Amount"], (FeeTypes)(int)sr["LoanFeeTypeID"]));
			//	},
			//	"NL_LoansFeesGet",
			//	CommandSpecies.StoredProcedure,
			//	new QueryParameter("@LoanID", this.loanID),
			//	new QueryParameter("@Now", StateDate)
			//);

			//DB.ForEachRowSafe(
			//	sr => {
			//		DateTime? start = sr["StartDate"];
			//		DateTime? end = sr["EndDate"];
			//		DateTime? deactivation = sr["DeactivationDate"];
			//		DateTime? activation = sr["ActivationDate"];

			//		if ((start == null) || (end == null))
			//			return;

			//		bool isActive = deactivation.HasValue
			//			? (activation <= StateDate) && (deactivation >= StateDate)
			//			: (activation <= StateDate);

			//		this.CalcModel.FreezePeriods.Add(
			//			new InterestFreeze(start.Value, end.Value, sr["InterestRate"], isActive)
			//		);
			//	},
			//	"NL_InterestFreezeGet",
			//	CommandSpecies.StoredProcedure,
			//	new QueryParameter("@LoanID", this.loanID)
			//);

			//SetBadPeriods();

		} // LoadNewLoanStructure

		private void SetBadPeriods() {
			// TODO: revive
			/*
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
			*/
		} // SetBadPeriods


	} // class LoanState
} // namespace
