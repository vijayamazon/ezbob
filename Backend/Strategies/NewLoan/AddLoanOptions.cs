namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Database;
	using Ezbob.Utils;

	public class AddLoanOptions : AStrategy {

		public AddLoanOptions(NL_LoanOptions loanOptions, int? OldLoanId, List<String> PropertiesUpdateList = null) {
			this.loanOptions = loanOptions;
			oldLoanId = OldLoanId;
			this.PropertiesUpdateList = PropertiesUpdateList;
		}//constructor

		public override string Name { get { return "AddLoanOptions"; } }

		public long LoanOptionsID { get; set; }
		public string Error { get; set; }
		private int? oldLoanId { get; set; }
		public List<String> PropertiesUpdateList { get; set; }

		private readonly NL_LoanOptions loanOptions;

		public override void Execute() {
			try {

				if (oldLoanId != null)
					this.loanOptions.LoanID = DB.ExecuteScalar<long>("GetNewLoanIdByOldLoanId", CommandSpecies.StoredProcedure, new QueryParameter("@LoanID", oldLoanId));

				NL_LoanOptions existsOptions = DB.FillFirst<NL_LoanOptions>("NL_LoanOptionsGet", CommandSpecies.StoredProcedure, new QueryParameter("@LoanID", this.loanOptions.LoanID));

				PropertyInfo[] props = typeof(NL_LoanOptions).GetProperties();

				existsOptions.Traverse((ignored, pi) => {
					if (pi.GetValue(this.loanOptions) != null)
						pi.SetValue(existsOptions, pi.GetValue(this.loanOptions));
				});

				foreach (var updateProperty in PropertiesUpdateList) {
					PropertyInfo pi = this.loanOptions.GetType().GetProperty(updateProperty);
					var fromClient = pi.GetValue(this.loanOptions);
					pi.SetValue(existsOptions, fromClient, null);
				}

				LoanOptionsID = DB.ExecuteScalar<long>("NL_LoanOptionsSave",
					CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanOptions>("Tbl", existsOptions),
					new QueryParameter("@LoanID", this.loanOptions.LoanID));

				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
				Log.Alert("Failed to save NL_LoanOptions, oldLoanID: {0}, LoanID: {1}, ex: {2}", oldLoanId, this.loanOptions.LoanID, ex);
				Error = string.Format("Failed to save NL_LoanOptions, oldLoanID: {0}, LoanID: {1}, ex: {2}", oldLoanId, this.loanOptions.LoanID, ex.Message);
			}
		}//Execute

	}//class AddLoanOptions
}//ns
