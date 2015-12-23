namespace Ezbob.Backend.Strategies.NewLoan.Collection {
	using System;
	using System.Collections.Generic;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Database;
	using Ezbob.Utils;

	public class SetLateLoanStatus : AStrategy {

		public SetLateLoanStatus(DateTime? runTime) {
			if (runTime != null)
				now = (DateTime)runTime;
			else
				now = DateTime.UtcNow;
		}

		public DateTime now { get; private set; }
		public override string Name { get { return "SetLateLoanStatus"; } }

		public override void Execute() {

			if (!Convert.ToBoolean(CurrentValues.Instance.NewLoanRun.Value))
				return;

			NL_AddLog(LogType.Info, "Strategy Start", now, null, null, null);

			try {
				//-----------Select relevan loans----------------------------------------------------
				DB.ForEachRowSafe((sr, bRowsetStart) => {
					//-----------Mark Loans as Late----------------------------------------------------
					MarkLoanAsLate(sr);
					return ActionResult.Continue;
				}, "NL_LateLoansGet",
				CommandSpecies.StoredProcedure, new QueryParameter("Now", now));

				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
				NL_AddLog(LogType.Error, "Strategy Faild", null, null, ex.ToString(), ex.StackTrace);
			}

		}//Execute

		private void MarkLoanAsLate(SafeReader sr) {
			//For each loan schedule marks it as late, it's loan as late, applies fee if needed
			int scheduleID = sr["LoanScheduleID"];
			int loanID = sr["LoanId"];
			int customerID = sr["CustomerId"];
			DateTime scheduleDate = sr["ScheduleDate"];
			NLLoanStatuses loanStatus = (NLLoanStatuses)Enum.Parse(typeof(NLLoanStatuses), sr["LoanStatus"]);
			NLScheduleStatuses scheduleStatus = (NLScheduleStatuses)Enum.Parse(typeof(NLScheduleStatuses), sr["ScheduleStatus"]);

			GetLoanState loanState = new GetLoanState(customerID, loanID, now);
			loanState.Execute();
			NL_Model nlModel = loanState.Result;
			decimal interest = nlModel.Interest; // TODO check: real unpaid interest for this date here

			var args = new object[] { customerID, loanID, loanStatus, scheduleID, scheduleStatus, scheduleDate, now };

			NL_AddLog(LogType.Info, "MarkLoanAsLate", args, null, null, null);

			if (loanStatus != NLLoanStatuses.Late) { //loanStatus != "Late")

				// DONE SO AFRE WITH "old" SetLateLoanStatus.cs job
				// DON'T REMOVE!!!!!!!!!! SHOULD BE UNCOMMENT AFTER "old" job cancelation
				//DB.ExecuteNonQuery(
				//	"UpdateCustomer", CommandSpecies.StoredProcedure,
				//	new QueryParameter("CustomerId", customerId),
				//	new QueryParameter("LoanStatus", "Late"),
				//	 new QueryParameter("IsWasLate", true)
				//	);

				NL_AddLog(LogType.Info, "update loan", args, loanID, null, null);

				DB.ExecuteNonQuery("NL_LoanUpdate", CommandSpecies.StoredProcedure, new QueryParameter("LoanID", loanID), new QueryParameter("LoanStatusID", (int)NLLoanStatuses.Late));
			}

			if (scheduleStatus != NLScheduleStatuses.Late) {

				NL_AddLog(LogType.Info, "update schedule ", args, scheduleID, null, null);

				DB.ExecuteNonQuery("NL_LoanSchedulesUpdate", CommandSpecies.StoredProcedure, new QueryParameter("LoanScheduleID", scheduleID), new QueryParameter("LoanScheduleStatusID", (int)NLScheduleStatuses.Late));
			}

			if (!LateFeesAllowed(nlModel.Loan.LoanOptions, loanID)) {
				NL_AddLog(LogType.Info, "late fees not allowed", args, nlModel.Loan.LoanOptions, null, null);
				return;
			}

			int daysBetween = (int)(now - scheduleDate).TotalDays;
			int feeAmount;
			NLFeeTypes nlFeeType;
			NL_Model.CalculateFee(daysBetween, interest, out feeAmount, out nlFeeType);

			NL_AddLog(LogType.Info, "late fee data", args, new object[] { sr, daysBetween, interest, feeAmount, nlFeeType }, null, null);

			if (nlFeeType != NLFeeTypes.None) {
				List<NL_LoanFees> nlFees = new List<NL_LoanFees>() {new NL_LoanFees() {
                            AssignedByUserID = 1,
                            LoanID = loanID,
                            Amount = feeAmount, 
                            DeletedByUserID = null,
                            AssignTime = now,
                            CreatedTime = now,
                            DisabledTime = null,
                            LoanFeeTypeID = (int)nlFeeType,
                            Notes = "SetLateLoanStatus scheduleID="+ scheduleID
						}
                    };

				try {

					DB.ExecuteNonQuery("NL_LoanFeesSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanFees>("Tbl", nlFees));

					Log.Info("NL: Applied late charge for customer {0} loan {1}: data: {2}", customerID, loanID, sr);
					NL_AddLog(LogType.Info, "Late fee", args, nlFees, null, null);

					// ReSharper disable once CatchAllClause
				} catch (Exception ex) {
					NL_AddLog(LogType.Error, "Strategy failed to add late fees", args, nlFees, ex.ToString(), ex.StackTrace);
				}

			} // if
		}
		
		protected bool LateFeesAllowed(NL_LoanOptions options, long loanID) {
			if (options.StopLateFeeToDate != null && options.StopLateFeeFromDate != null && (now >= options.StopLateFeeFromDate.Value.Date && now <= options.StopLateFeeToDate.Value.Date)) {
				Log.Info("NL: not applying late fee for loan {0} - auto late fee is disabled from {1:d} to {2:d}", loanID, options.StopLateFeeFromDate, options.StopLateFeeToDate);
				return false;
			}
			return true;
		}

	}// class CollectionScanner
} // namespace
