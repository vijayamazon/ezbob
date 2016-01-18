namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using ConfigManager;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Database;
	using Ezbob.Utils;

	public class AddLoanOptions : AStrategy {

		public List<string> PropertiesUpdateList { get; set; }
		public int? oldLoanId { get; set; }
		public string Error { get; set; }

		public long LoanOptionsID { get; set; }

		private readonly NL_LoanOptions nlLoanOptions;
		public AddLoanOptions(NL_LoanOptions loanOptions, int? OldLoanId, List<String> PropertiesUpdateList = null) {
			this.nlLoanOptions = loanOptions;
			oldLoanId = OldLoanId;
			this.PropertiesUpdateList = PropertiesUpdateList;
		} //constructor

		public override string Name { get { return "AddLoanOptions"; } }

		public override void Execute() {
			if (!CurrentValues.Instance.NewLoanRun) {
				NL_AddLog(LogType.Info, "NL disabled by configuration", null, null, null, null);
				return;
			}

			NL_AddLog(LogType.Info, "Strategy Start", this.nlLoanOptions, null, null, null);
			try {
				long newLoanId = 0;

				if (oldLoanId == null) {
					NL_AddLog(LogType.DataExsistense, "oldLoanID not found ", this.nlLoanOptions, null, null, null);
					return;
				}

				newLoanId = DB.ExecuteScalar<long>("NL_LoanIdByOldLoanId", CommandSpecies.StoredProcedure, new QueryParameter("OldLoanID", oldLoanId));
				Log.Debug("LoanID {0} for oldID {1}", oldLoanId, newLoanId);
				if (newLoanId <= 0) {
					NL_AddLog(LogType.DataExsistense, "LoanID not found ", new object[] { oldLoanId }, null, null, null);
					return;
				}
				
				this.nlLoanOptions.LoanID = newLoanId;

				NL_LoanOptions existsOptions = DB.FillFirst<NL_LoanOptions>("NL_LoanOptionsGet", CommandSpecies.StoredProcedure, new QueryParameter("LoanID", this.nlLoanOptions.LoanID));

				PropertyInfo[] props = typeof(NL_LoanOptions).GetProperties();

				existsOptions.Traverse((ignored, pi) => {
					if (pi.GetValue(this.nlLoanOptions) != null)
						pi.SetValue(existsOptions, pi.GetValue(this.nlLoanOptions));
				});

				if (PropertiesUpdateList != null) {
					foreach (var updateProperty in PropertiesUpdateList) {
						PropertyInfo pi = this.nlLoanOptions.GetType().GetProperty(updateProperty);
						var fromClient = pi.GetValue(this.nlLoanOptions);
						pi.SetValue(existsOptions, fromClient, null);
					}
				}

				LoanOptionsID = DB.ExecuteScalar<long>("NL_LoanOptionsSave",
					CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanOptions>("Tbl", existsOptions),
					new QueryParameter("LoanID", this.nlLoanOptions.LoanID));

				NL_AddLog(LogType.Info, "Strategy End", this.nlLoanOptions, this.LoanOptionsID, null, null);

				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
				Log.Alert("Failed to save NL_LoanOptions, oldLoanID: {0}, LoanID: {1}, ex: {2}", oldLoanId, this.nlLoanOptions.LoanID, ex);
				Error = string.Format("Failed to save NL_LoanOptions, oldLoanID: {0}, LoanID: {1}, ex: {2}", oldLoanId, this.nlLoanOptions.LoanID, ex.Message);
				NL_AddLog(LogType.Error, "Strategy failed", this.nlLoanOptions, null, ex.ToString(), ex.StackTrace);
			}

		}//Execute


	}//class AddLoanOptions
}//ns
