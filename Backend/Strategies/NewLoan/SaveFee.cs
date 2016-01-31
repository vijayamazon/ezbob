namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Backend.Strategies.NewLoan.Exceptions;
	using Ezbob.Database;

	/// <summary>
	/// Add new fee or update existing fee
	/// </summary>
	public class SaveFee : AStrategy {

		public SaveFee(NL_LoanFees fee) {
			Fee = fee;
			this.strategyArgs = new object[] { Fee  };
		}

		public override string Name { get { return "SaveFee"; } }
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

			if (!Array.Exists<NLFeeTypes>(NL_Model.NLFeeTypesEditable, element => element == (NLFeeTypes)Fee.LoanFeeTypeID) && Fee.LoanFeeID > 0L) {
				Error = string.Format("Fee type '{0}' not editable", Enum.GetName(typeof(NLFeeTypes), Fee.LoanFeeTypeID));
				NL_AddLog(LogType.InputInvalid, "InputInvalid", this.strategyArgs, null, Error, null);
				return;
			}

			if (Fee.Amount==0) {
				Error = "Zero fees are not allowed";
				NL_AddLog(LogType.InputInvalid, NL_ExceptionLoanNotFound.DefaultMessage, this.strategyArgs, null, Error, null);
				return;
			}

			// minDate: 'Fee cannot be added before loan starts or before paid installment'
			// Zero fees are not allowed

			List<NL_LoanFees> nlFees = new List<NL_LoanFees>();

			try {

				// new fee
				if (Fee.LoanFeeID == 0L) {
					Fee.CreatedTime = DateTime.UtcNow;
					nlFees.Add(Fee);
					DB.ExecuteNonQuery("NL_LoanFeesSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanFees>("Tbl", nlFees));
				} else {
					// editable fields :Amount, AssignTime, Notes (type???)
					DB.ExecuteNonQuery("NL_LoanFeesUpdate", CommandSpecies.StoredProcedure,
							new QueryParameter("LoanFeeID", Fee.LoanFeeID),
							new QueryParameter("UpdatedByUserID", Fee.UpdatedByUserID),
							new QueryParameter("UpdateTime", DateTime.UtcNow),
							new QueryParameter("Amount", Fee.Amount),
							new QueryParameter("AssignTime", Fee.AssignTime),
							new QueryParameter("Notes", Fee.Notes));
				}

				//nlFees.Clear();
				//nlFees.AddRange(DB.Fill<NL_LoanFees>("NL_LoanFeesGet", CommandSpecies.StoredProcedure, new QueryParameter("@LoanID", Fee.LoanID)).ToList());
				//NL_AddLog(LogType.Info, "End", this.strategyArgs, nlFees.FirstOrDefault(f=>f.Amount==Fee.Amount && f.AssignTime.Date.Equals(Fee.AssignTime.Date) && f.AssignedByUserID.Equals(Fee.AssignedByUserID) && f.CreatedTime.Date.Equals(Fee.CreatedTime.Date)), Error, null);
				
				NL_AddLog(LogType.Info, "End", this.strategyArgs, Fee, Error, null);

				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
				Error = ex.Message;
				Log.Alert(ex, "Failed to save fee");
				NL_AddLog(LogType.Error, "Failed", this.strategyArgs, Error, ex.ToString(), ex.StackTrace);
			}
		}
	} // class AddPayment
} // ns