namespace Ezbob.Backend.Strategies.NewLoan.Collection
{
    using System;
    using System.Collections.Generic;
    using ConfigManager;
    using DbConstants;
    using Ezbob.Backend.ModelsWithDB.NewLoan;
    using Ezbob.Database;

    /// <summary>
    /// Set Late Loan Status
    /// </summary>
    public class SetLateLoanStatus : AStrategy
    {
        private DateTime now;
        public override string Name { get { return "Set Late Loan Status"; } }
        public override void Execute()
        {
            this.now = DateTime.UtcNow;
            try
            {
                DB.ForEachRowSafe((sr, bRowsetStart) =>
                {
                    MarkLoanAsLate(sr);
                    return ActionResult.Continue;
                }, "NL_GetLoansToCollect",
                CommandSpecies.StoredProcedure, new QueryParameter("Now", this.now));
            }
            catch (Exception ex)
            {
                NL_AddLog(LogType.Error, "Strategy Faild", null, null, ex.ToString(), ex.StackTrace);
            }

        }//Execute

        private void MarkLoanAsLate(SafeReader sr)
        {
            //For each loan schedule marks it as late, it's loan as late, applies fee if needed
            int id = sr["LoanScheduleID"];
            int loanId = sr["LoanId"];
            int customerId = sr["CustomerId"];
            DateTime scheduleDate = sr["ScheduleDate"];
            string loanStatus = sr["LoanStatus"];
            string scheduleStatus = sr["ScheduleStatus"];
            decimal interest = sr["Interest"];

	        int statusLateID = 3;

			//statusLateID == (int)NLLoanStatuses.Late;

			if (statusLateID == (int)NLLoanStatuses.Late ) //loanStatus != "Late")
            {
                DB.ExecuteNonQuery(
                    "NL_UpdateLoanStatusToLate", CommandSpecies.StoredProcedure,
                    new QueryParameter("LoanId", loanId),
                    new QueryParameter("CustomerId", customerId),
                    new QueryParameter("PaymentStatus", "Late"),
                    new QueryParameter("LoanStatus", "Late")
                    );
            }

            if (scheduleStatus != "Late")
            {
                DB.ExecuteNonQuery("NL_UpdateLoanScheduleStatus", CommandSpecies.StoredProcedure,
                    new QueryParameter("Id", id), new QueryParameter("Status", "Late"));
            }

            int daysBetween = (int)(this.now - scheduleDate).TotalDays;
            NLFeeTypes nlFeeType = GetNLFeeTypes(daysBetween, interest);

            if (nlFeeType != NLFeeTypes.None)
            {
                if (!IsStopLateFees(loanId))
                {
                    List<NL_LoanFees> nlFees = new List<NL_LoanFees>() {
                        new NL_LoanFees() {
                            AssignedByUserID = 1,
                            LoanID = loanId,
                            Amount = GetLateFeesAmount(nlFeeType),
                            DeletedByUserID = null,
                            AssignTime = this.now,
                            CreatedTime = this.now,
                            DisabledTime = null,
                            LoanFeeTypeID = (int)nlFeeType,
                            Notes = String.Empty
                        }
                    };

                    try
                    {
                        DB.ExecuteNonQuery("NL_LoanFeesSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanFees>("Tbl", nlFees));
                        Log.Info("add new late fee and mark loan as late for customer {0}", customerId);
                        Log.Info("Applied late charge for customer {0} loan {1} : {2}", customerId, loanId, false);
                    }
                    catch (Exception ex)
                    {
                        NL_AddLog(LogType.Error, "Strategy Faild - Failed to add late fees", null, null, ex.ToString(), ex.StackTrace);
                    }

                }
            } // if
        }

        private bool IsStopLateFees(int loanId)
        {
            DateTime utcNow = DateTime.UtcNow;
            NL_LoanOptions existsOptions = DB.FillFirst<NL_LoanOptions>("NL_LoanOptionsGet", CommandSpecies.StoredProcedure, new QueryParameter("@LoanID", loanId));
            if (existsOptions.StopLateFeeFromDate == null)
                return false;
            if (existsOptions.StopLateFeeToDate != null)
                return (utcNow >= existsOptions.StopLateFeeFromDate && utcNow <= existsOptions.StopLateFeeToDate);
            return this.now >= existsOptions.StopLateFeeFromDate;
        }

        private decimal GetLateFeesAmount(NLFeeTypes nlFeeType)
        {
            switch (nlFeeType)
            {
                case NLFeeTypes.LatePaymentFee:
                    return CurrentValues.Instance.LatePaymentCharge.ID;
                case NLFeeTypes.AdminFee:
                    return CurrentValues.Instance.AdministrationCharge.ID;
                case NLFeeTypes.PartialPaymentFee:
                    return CurrentValues.Instance.PartialPaymentCharge.ID;
            }
            return 0;
        }

        private NLFeeTypes GetNLFeeTypes(int daysBetween, decimal interest)
        {
            //  use NLFeeTypes
            if (daysBetween >= CurrentValues.Instance.CollectionPeriod1 && daysBetween < CurrentValues.Instance.CollectionPeriod2)
            {
                return NLFeeTypes.LatePaymentFee;
            }
            if (daysBetween >= CurrentValues.Instance.CollectionPeriod2 && daysBetween < CurrentValues.Instance.CollectionPeriod3 && interest > 0)
            {
                return NLFeeTypes.AdminFee;
            }
            if (daysBetween >= CurrentValues.Instance.CollectionPeriod2 && daysBetween < CurrentValues.Instance.CollectionPeriod3 && interest <= 0)
            {
                return NLFeeTypes.PartialPaymentFee;
            }//if
            return NLFeeTypes.None;
        } //CalculateFee

    }// class CollectionScanner
} // namespace
