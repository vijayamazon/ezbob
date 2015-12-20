namespace Ezbob.Backend.Strategies.NewLoan.Collection {
	using System;
	using System.Collections.Generic;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Database;

	/// <summary>
	/// Set Late Loan Status
	/// </summary>
	public class SetLateLoanStatus : AStrategy {
		private DateTime now;
		public override string Name { get { return "Set Late Loan Status"; } }

		public override void Execute() {
			if (!Convert.ToBoolean(CurrentValues.Instance.NewLoanRun.Value))
				return;

			this.now = DateTime.UtcNow;
			try {
				//-----------Select relevan loans----------------------------------------------------
				DB.ForEachRowSafe((sr, bRowsetStart) => {
					//-----------Mark Loans as Late----------------------------------------------------
					MarkLoanAsLate(sr);
					return ActionResult.Continue;
				}, "NL_LateLoansGet",
				CommandSpecies.StoredProcedure, new QueryParameter("Now", this.now));

			} catch (Exception ex) {
				NL_AddLog(LogType.Error, "Strategy Faild", null, null, ex.ToString(), ex.StackTrace);
			}

		}//Execute

		private void MarkLoanAsLate(SafeReader sr) {
			//For each loan schedule marks it as late, it's loan as late, applies fee if needed
			int id = sr["LoanScheduleID"];
			int loanId = sr["LoanId"];
			int customerId = sr["CustomerId"];
			DateTime scheduleDate = sr["ScheduleDate"];
			NLLoanStatuses loanStatus = (NLLoanStatuses)Enum.Parse(typeof(NLLoanStatuses), sr["LoanStatus"]);
			NLLoanScheduleStatus scheduleStatus = (NLLoanScheduleStatus)Enum.Parse(typeof(NLLoanScheduleStatus), sr["ScheduleStatus"]);
			decimal interest = sr["Interest"];

			if (loanStatus != NLLoanStatuses.Late) { //loanStatus != "Late")

				// DONE SO AFRE WITH "old" SetLateLoanStatus.cs job
				// DON'T REMOVE!!!!!!!!!! SHOULD BE UNCOMMENT AFTER "old" job cancelation
				//DB.ExecuteNonQuery(
				//	"UpdateCustomer", CommandSpecies.StoredProcedure,
				//	new QueryParameter("CustomerId", customerId),
				//	new QueryParameter("LoanStatus", "Late"),
				//	 new QueryParameter("IsWasLate", true)
				//	);


				DB.ExecuteNonQuery(
					"NL_LoanUpdate", CommandSpecies.StoredProcedure,
					new QueryParameter("LoanID", loanId),
					new QueryParameter("LoanStatusID", (int)NLLoanStatuses.Late)
				);
			}

			if (scheduleStatus != NLLoanScheduleStatus.Late) {
				DB.ExecuteNonQuery("NL_LoanSchedulesUpdate", CommandSpecies.StoredProcedure,
					new QueryParameter("LoanScheduleID", id),
					new QueryParameter("LoanScheduleStatusID", (int)NLLoanScheduleStatus.Late));
			}

			int daysBetween = (int)(this.now - scheduleDate).TotalDays;
			NLFeeTypes nlFeeType = GetNLFeeTypes(daysBetween, interest);

			if (nlFeeType != NLFeeTypes.None) {
				if (!IsStopLateFees(loanId)) {
					List<NL_LoanFees> nlFees = new List<NL_LoanFees>() {
                        new NL_LoanFees() {
                            AssignedByUserID = 1,
                            LoanID = loanId,
                            Amount = NL_Model.GetLateFeesAmount(nlFeeType),
                            DeletedByUserID = null,
                            AssignTime = this.now,
                            CreatedTime = this.now,
                            DisabledTime = null,
                            LoanFeeTypeID = (int)nlFeeType,
                            Notes = String.Empty
                        }
                    };

					try {
						DB.ExecuteNonQuery("NL_LoanFeesSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanFees>("Tbl", nlFees));
						Log.Info("add new late fee and mark loan as late for customer {0}", customerId);
						Log.Info("Applied late charge for customer {0} loan {1} : {2}", customerId, loanId, false);
					} catch (Exception ex) {
						NL_AddLog(LogType.Error, "Strategy Faild - Failed to add late fees", null, null, ex.ToString(), ex.StackTrace);
					}

				}
			} // if
		}

		private bool IsStopLateFees(int loanId) {
			DateTime utcNow = DateTime.UtcNow;
			NL_LoanOptions existsOptions = DB.FillFirst<NL_LoanOptions>("NL_LoanOptionsGet", CommandSpecies.StoredProcedure, new QueryParameter("@LoanID", loanId));
			if (existsOptions.StopLateFeeFromDate == null)
				return false;
			if (existsOptions.StopLateFeeToDate != null)
				return (utcNow >= existsOptions.StopLateFeeFromDate && utcNow <= existsOptions.StopLateFeeToDate);
			return this.now >= existsOptions.StopLateFeeFromDate;
		}

		private NLFeeTypes GetNLFeeTypes(int daysBetween, decimal interest) {
			//  use NLFeeTypes
			if (daysBetween >= CurrentValues.Instance.CollectionPeriod1 && daysBetween < CurrentValues.Instance.CollectionPeriod2) {
				return NLFeeTypes.LatePaymentFee;
			}
			if (daysBetween >= CurrentValues.Instance.CollectionPeriod2 && daysBetween < CurrentValues.Instance.CollectionPeriod3 && interest > 0) {
				return NLFeeTypes.AdminFee;
			}
			if (daysBetween >= CurrentValues.Instance.CollectionPeriod2 && daysBetween < CurrentValues.Instance.CollectionPeriod3 && interest <= 0) {
				return NLFeeTypes.PartialPaymentFee;
			}//if
			return NLFeeTypes.None;
		} //CalculateFee

	}// class CollectionScanner
} // namespace
