namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Backend.Strategies.NewLoan.DAL;
	using Ezbob.Database;
	using NHibernate.Linq;
	using StructureMap.Attributes;

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

		//[SetterProperty]
		//public ILoanDAL LoanDAL { get; set; }

		public override void Execute() {
			try {
				// TODO replace with DAL calls

				// loan
				Result.Loan = new NL_Loans();
				Result.Loan = LoanDAL.GetLoan(this.loanID);
				/*Result.Loan = DB.FillFirst<NL_Loans>("NL_LoansGet",
					CommandSpecies.StoredProcedure,
					new QueryParameter("@loanID", this.loanID)
				);*/

				// histories
				Result.Loan.Histories.Clear();
				Result.Loan.Histories = LoanDAL.GetLoanHistories(this.loanID, StateDate); /*DB.Fill<NL_LoanHistory>("NL_LoanHistoryGet",
					CommandSpecies.StoredProcedure,
					new QueryParameter("@LoanID", this.loanID),
					new QueryParameter("@Now", StateDate)
				);*/

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
				List<NL_LoanFees> fees=  DB.Fill<NL_LoanFees>("NL_LoansFeesGet",
					CommandSpecies.StoredProcedure,
					new QueryParameter("@LoanID", this.loanID));

				// filter cnacelled/deleted fees on LoanState strategy
				fees.Where(f => f.DisabledTime != null || f.DeletedByUserID >0).ForEach(f => Result.Loan.Fees.Add(f));;

				// interest freezes
				Result.Loan.FreezeInterestIntervals.Clear();
				List<NL_LoanInterestFreeze> freezes= DB.Fill<NL_LoanInterestFreeze>("NL_LoanInterestFreezeGet",
					CommandSpecies.StoredProcedure,
					new QueryParameter("@LoanID", this.loanID));

				// filter cancelled (deactivated) periods
				// TODO: take in consideration stateDate
				// freezes.Where(fr => fr.DeactivationDate != null).ForEach(fr => Result.Loan.FreezeInterestIntervals.Add(fr));
				freezes.ForEach(fr => Result.Loan.FreezeInterestIntervals.Add(fr));

				// loan options
				Result.Loan.LoanOptions = DB.FillFirst<NL_LoanOptions>("NL_LoanOptionsGet",
					CommandSpecies.StoredProcedure,
					new QueryParameter("@LoanID", this.loanID)
				);

				// TODO combine all payments+ transactions to one SP

				// payments (logical loan transactions)
				Result.Loan.Payments.Clear();
				Result.Loan.Payments = DB.Fill<NL_Payments>("NL_PaymentsGet",
					CommandSpecies.StoredProcedure,
					new QueryParameter("@LoanID", this.loanID),
					new QueryParameter("@Now", StateDate)
				);
				
				foreach (NL_Payments p in Result.Loan.Payments) {

					p.SchedulePayments.Clear();
					p.SchedulePayments = DB.Fill<NL_LoanSchedulePayments>("NL_LoanSchedulePaymentsGet",
						CommandSpecies.StoredProcedure,
						new QueryParameter("@LoanID", this.loanID)
					);

					p.FeePayments.Clear();
					p.FeePayments = DB.Fill<NL_LoanFeePayments>("NL_LoanFeePaymentsGet",
						CommandSpecies.StoredProcedure,
						new QueryParameter("@LoanID", this.loanID)
					);
				}

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
