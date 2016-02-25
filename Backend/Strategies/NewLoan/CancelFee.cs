namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Backend.Strategies.NewLoan.Exceptions;
	using Ezbob.Database;

	/// <summary>
	/// </summary>
	public class CancelFee : AStrategy {

		public CancelFee(NL_LoanFees fee) {
			Fee = fee;
			this.strategyArgs = new object[] { Fee };
		}

		public override string Name { get { return "CancelFee"; } }
		public NL_LoanFees Fee { get; private set; }
		public string Error { get; private set; }
		private readonly object[] strategyArgs;

		public override void Execute() {
			if (!CurrentValues.Instance.NewLoanRun) {
				NL_AddLog(LogType.Info, "NL disabled by configuration", null, null, null, null);
				return;
			}

			NL_AddLog(LogType.Info, "Started", this.strategyArgs, Error, null, null);

			if (Fee == null || Fee.LoanID == 0) {
				Error = NL_ExceptionLoanNotFound.DefaultMessage;
				NL_AddLog(LogType.InputInvalid, NL_ExceptionLoanNotFound.DefaultMessage, this.strategyArgs, null, Error, null);
				return;
			}

			if (!Array.Exists<NLFeeTypes>(NL_Model.NLFeeTypesEditable, element => element == (NLFeeTypes)Fee.LoanFeeTypeID)) {
				Error = string.Format("Fee type '{0}' not editable", Enum.GetName(typeof(NLFeeTypes), Fee.LoanFeeTypeID));
				NL_AddLog(LogType.InputInvalid, "InputInvalid", this.strategyArgs, null, Error, null);
				return;
			}

			try {

				DB.ExecuteNonQuery("NL_LoanFeeCancel", CommandSpecies.StoredProcedure,
					new QueryParameter("LoanFeeID", Fee.LoanFeeID),
					new QueryParameter("DeletedByUserID", Fee.DeletedByUserID),
					new QueryParameter("DisabledTime", DateTime.UtcNow),
					new QueryParameter("Notes", Fee.Notes));

				NL_AddLog(LogType.Info, "Ended", this.strategyArgs, Error, null, null);

				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
				Error = ex.Message;
				Log.Alert(ex, "Failed to disable fee");
				NL_AddLog(LogType.Error, "Failed", this.strategyArgs, Error, ex.ToString(), ex.StackTrace);
			}
	
		}
	} // class CancelFee
} // ns