﻿namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Database;

	public class SaveRolloverOpportunity : AStrategy {

		public SaveRolloverOpportunity(NL_LoanRollovers r) {
			rollover = r;
			this.strategyArgs = new object[]{r};
		}

		public override string Name { get { return "SaveRolloverOpportunity"; } }
		
		public string Error;
		private readonly object[] strategyArgs;
		public long RolloverID { get; private set; }
		public NL_LoanRollovers rollover { get; private set; }
	

		public override void Execute() {

			// TODO EZ-4329 records opportunity for rollover (insert into NL_LoanRollovers) + record into NL_LoanFees with feetype of RolloverFee

			NL_AddLog(LogType.Info, "Started", this.strategyArgs, this.Error, null, null);

			ConnectionWrapper pconn = DB.GetPersistent();

			try {

				pconn.BeginTransaction();

				rollover.LoanRolloverID = DB.ExecuteScalar<long>("NL_LoanRolloversSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanRollovers>("Tbl", rollover));
				RolloverID = rollover.LoanRolloverID;

			} catch (Exception ex) {

				pconn.Rollback();

				this.Error = ex.Message;
				Log.Error("Failed to save rollover {0}. err: {1}", rollover, this.Error);

				NL_AddLog(LogType.Error, "Strategy Faild - Rollback", rollover, this.Error, ex.ToString(), ex.StackTrace);

			}
		}

	} // class AddRolloverOpportunity
} // ns