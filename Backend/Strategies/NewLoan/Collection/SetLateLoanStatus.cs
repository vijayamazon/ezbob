namespace Ezbob.Backend.Strategies.NewLoan.Collection {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Database;

	public class SetLateLoanStatus : AStrategy {

		public SetLateLoanStatus(DateTime? runTime) {
			if (runTime != null)
				now = (DateTime)runTime;
			else
				now = DateTime.UtcNow;

			this.loansList = new List<NLLateLoansJobModel>();
		}

		public DateTime now { get; private set; }
		public override string Name { get { return "SetLateLoanStatus"; } }
		private readonly List<NLLateLoansJobModel> loansList;

		public override void Execute() {
			if (!CurrentValues.Instance.NewLoanRun) {
				NL_AddLog(LogType.Info, "NL disabled by configuration", null, null, null, null);
				return;
			}

			NL_AddLog(LogType.Info, "Strategy Start", now, null, null, null);

			try {
				DB.ForEachRowSafe((sr, bRowsetStart) => {
					NLLateLoansJobModel model = new NLLateLoansJobModel() {
						LoanScheduleID = sr["LoanScheduleID"],
						LoanID = sr["LoanID"],
						OldLoanID = sr["OldLoanID"],
						CustomerID = sr["CustomerID"],
						PlannedDate = sr["PlannedDate"],
						LoanStatus = (NLLoanStatuses)Enum.Parse(typeof(NLLoanStatuses), sr["LoanStatus"]),
						ScheduleStatus = (NLScheduleStatuses)Enum.Parse(typeof(NLScheduleStatuses), sr["ScheduleStatus"])
					};
					this.loansList.Add(model);
					return ActionResult.Continue;
				}, "NL_LateLoansGet", CommandSpecies.StoredProcedure, new QueryParameter("Now", now));
				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
				NL_AddLog(LogType.Error, "Strategy Failed", null, null, ex.ToString(), ex.StackTrace);
			}

			foreach (NLLateLoansJobModel model in this.loansList) {
				Log.Info(model.ToString());
				NLMarkLoanAsLate(model);
			}

			NL_AddLog(LogType.Info, "Strategy end", now, null, null, null);

		}//Execute

		private void NLMarkLoanAsLate(NLLateLoansJobModel model) {
			Log.Debug("NLMarkLoanAsLate: {0}", model.ToString());
			NL_AddLog(LogType.Info, "NLMarkLoanAsLate", model, null, null, null);

			if (model.LoanStatus != NLLoanStatuses.Late) {
				// DON'T REMOVE!!!!!!!!!! SHOULD BE UNCOMMENT AFTER "old" job cancellation
				//DB.ExecuteNonQuery(
				//	"UpdateCustomer", CommandSpecies.StoredProcedure,
				//	new QueryParameter("CustomerId", customerId),
				//	new QueryParameter("LoanStatus", "Late"),
				//	 new QueryParameter("IsWasLate", true)
				//	);

				DB.ExecuteNonQuery("NL_LoanUpdate", CommandSpecies.StoredProcedure,
					new QueryParameter("LoanID", model.LoanID),
					new QueryParameter("LoanStatusID", (int)NLLoanStatuses.Late));

				Log.Debug("Updating nlloan {0} to late", model.LoanID);
				NL_AddLog(LogType.Info, "updating loan to late", model, model.LoanID, null, null);
			}

			if (model.ScheduleStatus != NLScheduleStatuses.Late) {

				DB.ExecuteNonQuery("NL_LoanSchedulesUpdate", CommandSpecies.StoredProcedure,
					new QueryParameter("LoanScheduleID", model.LoanScheduleID),
					new QueryParameter("LoanScheduleStatusID", (int)NLScheduleStatuses.Late));

				Log.Debug("Updating schedule {0} to late", model.LoanScheduleID);
				NL_AddLog(LogType.Info, "updating schedule to late", model, model.LoanScheduleID, null, null);
			}

			GetLoanState loanState = new GetLoanState(model.CustomerID, model.LoanID, now);
			loanState.Execute();
			decimal interest =  loanState.Result.Interest; // TODO check: real unpaid interest for this date here

			if (!LateFeesAllowed(loanState.Result.Loan.LoanOptions, model.LoanID)) {
				Log.Debug("late fees for loan {0} not allowed", model.LoanID);
				NL_AddLog(LogType.Info, "Late fees not allowed", model, loanState.Result.Loan.LoanOptions, null, null);
				return;
			}

			int daysLate = (int)(now - model.PlannedDate).TotalDays;
			int feeAmount;
			NLFeeTypes feeType;
			NL_Model.CalculateFee(daysLate, interest, out feeAmount, out feeType);

			Log.Debug("calculated feeAmount={0}, FeeType={1} daysLate={2} schedule={3} loan={4}", feeAmount, (int)feeType, daysLate, model.LoanScheduleID, model.LoanID);
			NL_AddLog(LogType.Info, "calculated fee data", model, new object[] { daysLate, interest, feeAmount, feeType }, null, null);

			if (feeType != NLFeeTypes.None) {

				// check if this fee type for this schedule already assigned

				// get next schedule date
				NL_LoanSchedules nextSchedule = null;
				loanState.Result.Loan.Histories.ForEach(h => nextSchedule = h.Schedule.FirstOrDefault(s => s.PlannedDate > model.PlannedDate));
				DateTime dateTo = nextSchedule == null ? model.PlannedDate : nextSchedule.PlannedDate;

				// between this and nect schedules same fee alread assigned
				if (loanState.Result.Loan.Fees.FirstOrDefault(f => f.LoanFeeTypeID == (int)feeType && f.AssignTime.Date <= dateTo.Date && f.AssignTime.Date >= model.PlannedDate) != null) {
					Log.Debug("NL: Tried to apply already assigned late charge for customer {0} loan {1}: feetype: {2}", model.CustomerID, model.LoanID, feeType);
					NL_AddLog(LogType.Info, "LatefeeExists", model, feeType, null, null);
					return;
				}

				NL_LoanFees lateFee = new NL_LoanFees() {
					AssignedByUserID = 1,
					LoanID = model.LoanID,
					Amount = feeAmount,
					AssignTime = now.Date,
					CreatedTime = now,
					LoanFeeTypeID = (int)feeType,
					Notes = daysLate + " days late;schedule " + model.LoanScheduleID,
					DeletedByUserID = null,
					DisabledTime = null
				};
				var nlfList = new List<NL_LoanFees>();
				nlfList.Add(lateFee);
				try {
					DB.ExecuteNonQuery("NL_LoanFeesSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanFees>("Tbl", nlfList));

					Log.Debug("NL: Applied late charge for customer {0} loan {1}: data: {2}", model.CustomerID, model.LoanID, lateFee);
					NL_AddLog(LogType.Info, "Latefee", model, lateFee, null, null);

					// ReSharper disable once CatchAllClause
				} catch (Exception ex) {
					Log.Alert("NL: Failed to add late fee for customer {0} loan {1}: data: {2}", model.CustomerID, model.LoanID, lateFee);
					NL_AddLog(LogType.Error, "Failed to add late fee", model, lateFee, ex.ToString(), ex.StackTrace);
				}
			}
		} //NL_MarkLoanAsLate

		private bool LateFeesAllowed(NL_LoanOptions options, long loanID) {
			if (options.StopLateFeeToDate != null && options.StopLateFeeFromDate != null && (now >= options.StopLateFeeFromDate.Value.Date && now <= options.StopLateFeeToDate.Value.Date)) {
				Log.Info("NL: not applying late fee for loan {0} - auto late fee is disabled from {1:d} to {2:d}", loanID, options.StopLateFeeFromDate, options.StopLateFeeToDate);
				return false;
			}
			return true;
		}

	}// class CollectionScanner
} // namespace
