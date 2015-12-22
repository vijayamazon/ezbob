namespace Ezbob.Backend.Strategies.NewLoan.Collection {
	using System;
	using System.Collections.Generic;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Database;
	using Ezbob.Utils;

	/// <summary>
	/// Set Late Loan Status
	/// </summary>
	public class SetLateLoanStatus : AStrategy {
		
		private DateTime now;
		public override string Name { get { return "SetLateLoanStatus"; } }

		public override void Execute() {
			if (!Convert.ToBoolean(CurrentValues.Instance.NewLoanRun.Value))
				return;

			this.now = DateTime.UtcNow;

			NL_AddLog(LogType.Info, "Strategy Start", this.now, null, null, null);

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
			int scheduleID = sr["LoanScheduleID"];
			int loanID = sr["LoanId"];
			int customerID = sr["CustomerId"];
			DateTime scheduleDate = sr["ScheduleDate"];
			NLLoanStatuses loanStatus = (NLLoanStatuses)Enum.Parse(typeof(NLLoanStatuses), sr["LoanStatus"]);
			NLLoanScheduleStatus scheduleStatus = (NLLoanScheduleStatus)Enum.Parse(typeof(NLLoanScheduleStatus), sr["ScheduleStatus"]);

			GetLoanState loanState = new GetLoanState(customerID, loanID, this.now);
			loanState.Execute();
			NL_Model nlModel = loanState.Result;
			decimal interest = nlModel.Interest; // TODO check: real unpaid interest for this date here

			NL_AddLog(LogType.Info, "MarkLoanAsLate", this.now, sr, null, null);

			if (loanStatus != NLLoanStatuses.Late) { //loanStatus != "Late")

				// DONE SO AFRE WITH "old" SetLateLoanStatus.cs job
				// DON'T REMOVE!!!!!!!!!! SHOULD BE UNCOMMENT AFTER "old" job cancelation
				//DB.ExecuteNonQuery(
				//	"UpdateCustomer", CommandSpecies.StoredProcedure,
				//	new QueryParameter("CustomerId", customerId),
				//	new QueryParameter("LoanStatus", "Late"),
				//	 new QueryParameter("IsWasLate", true)
				//	);

				NL_AddLog(LogType.Info, "NL_LoanUpdate-late", this.now, loanID, null, null);

				DB.ExecuteNonQuery("NL_LoanUpdate", CommandSpecies.StoredProcedure,
					new QueryParameter("LoanID", loanID),
					new QueryParameter("LoanStatusID", (int)NLLoanStatuses.Late)
				);
			}

			if (scheduleStatus != NLLoanScheduleStatus.Late) {
				NL_AddLog(LogType.Info, "NL_LoanSchedulesUpdate-late", this.now, sr, null, null);

				DB.ExecuteNonQuery("NL_LoanSchedulesUpdate", CommandSpecies.StoredProcedure,
					new QueryParameter("LoanScheduleID", scheduleID),
					new QueryParameter("LoanScheduleStatusID", (int)NLLoanScheduleStatus.Late));
			}

			if (!LateFeesAllowed(nlModel.Loan.LoanOptions, loanID)) {
				NL_AddLog(LogType.Info, "LateFeesAllowed not allowed", this.now, new object[] { sr, nlModel.Loan.LoanOptions }, null, null);
				return;
			}

			int daysBetween = (int)(this.now - scheduleDate).TotalDays;
			int feeAmount;
			NLFeeTypes nlFeeType;
			MiscUtils.CalculateFee(daysBetween, interest, out feeAmount, out nlFeeType);

			NL_AddLog(LogType.Info, "NLFeeTypes by late days of schedule", this.now, new object[] { sr, daysBetween, interest, feeAmount, nlFeeType }, null, null);

			if (nlFeeType != NLFeeTypes.None) {
				List<NL_LoanFees> nlFees = new List<NL_LoanFees>() {new NL_LoanFees() {
                            AssignedByUserID = 1,
                            LoanID = loanID,
                            Amount = feeAmount, // NL_Model.GetLateFeesAmount(nlFeeType),
                            DeletedByUserID = null,
                            AssignTime = this.now,
                            CreatedTime = this.now,
                            DisabledTime = null,
                            LoanFeeTypeID = (int)nlFeeType,
                            Notes = "SetLateLoanStatus scheduleID="+ scheduleID
                        }
                    };

				try {

					DB.ExecuteNonQuery("NL_LoanFeesSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanFees>("Tbl", nlFees));

					Log.Info("NL: Applied late charge for customer {0} loan {1}: data: {2}", customerID, loanID, sr);
					NL_AddLog(LogType.Info, "Applied late charge", this.now, new object[] { sr, nlFees }, null, null);

					// ReSharper disable once CatchAllClause
				} catch (Exception ex) {
					NL_AddLog(LogType.Error, "Strategy failed to add late fees", this.now, new object[] { sr, nlFees }, ex.ToString(), ex.StackTrace);
				}

			} // if
		}

		//ApplyLateCharge
		//if (((loanOptions.StopLateFeeFromDate.HasValue && now >= loanOptions.StopLateFeeFromDate.Value) &&
		//			(loanOptions.StopLateFeeToDate.HasValue && now <= loanOptions.StopLateFeeToDate.Value)) ||
		//			(!loanOptions.StopLateFeeFromDate.HasValue && !loanOptions.StopLateFeeToDate.HasValue)) {
		//			Log.InfoFormat("not applying late fee for loan {0} - auto late fee is disabled", loanId);
		//			return false;
		//		}

		protected bool LateFeesAllowed(NL_LoanOptions options, long loanID) {
			if (options.StopLateFeeToDate != null && options.StopLateFeeFromDate != null && (this.now >= options.StopLateFeeFromDate.Value.Date && this.now <= options.StopLateFeeToDate.Value.Date)) {
				Log.Info("NL: not applying late fee for loan {0} - auto late fee is disabled from {1:d} to {2:d}", loanID, options.StopLateFeeFromDate, options.StopLateFeeToDate);
				return false;
			}
			return true;
		}

		//private NLFeeTypes GetNLFeeTypes(int daysBetween, decimal interest) {
		//	//  use NLFeeTypes
		//	if (daysBetween >= CurrentValues.Instance.CollectionPeriod1 && daysBetween < CurrentValues.Instance.CollectionPeriod2) {
		//		return NLFeeTypes.LatePaymentFee;
		//	}
		//	if (daysBetween >= CurrentValues.Instance.CollectionPeriod2 && daysBetween < CurrentValues.Instance.CollectionPeriod3 && interest > 0) {
		//		return NLFeeTypes.AdminFee;
		//	}
		//	if (daysBetween >= CurrentValues.Instance.CollectionPeriod2 && daysBetween < CurrentValues.Instance.CollectionPeriod3 && interest <= 0) {
		//		return NLFeeTypes.PartialPaymentFee;
		//	}//if
		//	return NLFeeTypes.None;
		//}

		/*private void CalculateFee(int daysBetween, decimal interest, out int feeAmount, out NLFeeTypes feeType) {
			feeAmount = 0;
			feeType = NLFeeTypes.None;

			if (daysBetween >= CurrentValues.Instance.CollectionPeriod1 && daysBetween < CurrentValues.Instance.CollectionPeriod2) {
				feeAmount = CurrentValues.Instance.LatePaymentCharge;
				feeType = NLFeeTypes.LatePaymentFee;
			} else if (daysBetween >= CurrentValues.Instance.CollectionPeriod2 && daysBetween < CurrentValues.Instance.CollectionPeriod3 && interest > 0) {
				feeAmount = CurrentValues.Instance.AdministrationCharge;
				feeType = NLFeeTypes.AdminFee;
			} else if (daysBetween >= CurrentValues.Instance.CollectionPeriod2 && daysBetween < CurrentValues.Instance.CollectionPeriod3 && interest <= 0) {
				feeAmount = CurrentValues.Instance.PartialPaymentCharge;
				feeType = NLFeeTypes.PartialPaymentFee;
			}//if
		}//CalculateFee*/

	}// class CollectionScanner
} // namespace
