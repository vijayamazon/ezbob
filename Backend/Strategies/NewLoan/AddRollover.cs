namespace Ezbob.Backend.Strategies.NewLoan
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ConfigManager;
    using DbConstants;
    using Ezbob.Backend.ModelsWithDB.NewLoan;
    using Ezbob.Backend.Strategies.NewLoan.Exceptions;
    using Ezbob.Database;

	/// <summary>
	/// adding new rollover proposal for loan or updates existing valid rollover?
	/// </summary>
    public class AddRollover : AStrategy, Inlstrategy {

		public AddRollover(NL_LoanRollovers r, long loanID, bool saveRolloverFee = true) {
			rollover = r;
			LoanID = loanID;
			SaveRolloverFee = saveRolloverFee;
			this.strategyArgs = new object[] { LoanID, rollover, SaveRolloverFee };
		}

		public override string Name { get { return "AddRollover"; } }

		public NL_LoanRollovers rollover { get; private set; }
		public long RolloverID { get; private set; }
		public long LoanID { get; private set; }
		
		public bool SaveRolloverFee { get; private set; }

		public string Error { get; private set; }

		private readonly object[] strategyArgs;

		/// <exception cref="NL_ExceptionLoanNotFound">Condition. </exception>
		public override void Execute() {

            if (!IsNewLoanRunStrategy)
                return;

			// EZ-4329 
			// check if same rollover exists OK
			// add RolloverFee OK
			// add opportunity for rollover (insert into NL_LoanRollovers)  OK

			NL_AddLog(LogType.Info, "Started", this.strategyArgs, Error, null, null);

			if (LoanID == 0) {
				Error = NL_ExceptionLoanNotFound.DefaultMessage;
				NL_AddLog(LogType.Error, NL_ExceptionLoanNotFound.DefaultMessage, this.strategyArgs, null, Error, null);
				throw new NL_ExceptionLoanNotFound(Error);
			}

			ConnectionWrapper pconn = DB.GetPersistent();

			try {

				pconn.BeginTransaction();

				List<NL_LoanRollovers> loanRollovers = DB.Fill<NL_LoanRollovers>("NL_RolloversGet", CommandSpecies.StoredProcedure, new QueryParameter("@LoanID", LoanID));
				var rExists = loanRollovers.FirstOrDefault(r => r.CreationTime.Date.Equals(rollover.CreationTime.Date) && r.ExpirationTime.Date.Equals(rollover.ExpirationTime.Date));

				if (rExists != null) {
					Error = string.Format("Rollover opportunity for loan {0}, added at {1}, expired at {2}, already registered in the system.", LoanID, rollover.CreationTime, rollover.ExpirationTime);
					Log.Info(Error);
					NL_AddLog(LogType.Info, "Started", this.strategyArgs, Error, null, null);
					return;
				}

				if (SaveRolloverFee) {

					NL_LoanFees rolloverFee = new NL_LoanFees() {
						LoanID = LoanID,
						Amount = Decimal.Parse(CurrentValues.Instance.RolloverCharge.Value),
						AssignedByUserID = rollover.CreatedByUserID,
						AssignTime = rollover.CreationTime,
						CreatedTime = rollover.CreationTime,
						DisabledTime = rollover.ExpirationTime,
						LoanFeeTypeID = (int)NLFeeTypes.RolloverFee,
						Notes = string.Format("rollover {0:d}-{1:d}", rollover.CreationTime, rollover.ExpirationTime) 
					};

					// insert fees
					rollover.LoanFeeID = DB.ExecuteScalar<long>(pconn, "NL_LoanFeesSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanFees>("Tbl", rolloverFee));

					NL_AddLog(LogType.Info, "Rollover fee", this.strategyArgs, rollover.LoanFeeID, Error, null);
				}

				rollover.LoanRolloverID = DB.ExecuteScalar<long>(pconn, "NL_LoanRolloversSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanRollovers>("Tbl", rollover));
				
				pconn.Commit();

				RolloverID = rollover.LoanRolloverID;

				NL_AddLog(LogType.Info, "End", this.strategyArgs, rollover, Error, null);

			} catch (Exception ex) {

				pconn.Rollback();

				Error = ex.Message;
				Log.Error("Failed to save rollover {0}. err: {1}", rollover, Error);

				NL_AddLog(LogType.Error, "Strategy Faild - Rollback", rollover, Error, ex.ToString(), ex.StackTrace);
			}

		} // Execute
	} 
} 