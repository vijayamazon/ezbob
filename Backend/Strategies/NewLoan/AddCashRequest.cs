namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using ConfigManager;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Database;

	public class AddCashRequest : AStrategy {
		public AddCashRequest(NL_CashRequests cashRequest) {
			this.cashRequest = cashRequest;
			Transaction = null;
		} // constructor

		public override string Name { get { return "AddCashRequest"; } }

		public ConnectionWrapper Transaction { get; set; }

		public override void Execute() {

			if (!CurrentValues.Instance.NewLoanRun) {
				NL_AddLog(LogType.Info, "NL disabled by configuration", null, null, null, null);
				return;
			}

			NL_AddLog(LogType.Info, "Strategy Start", this.cashRequest, null, Error, null);

			try {
				CashRequestID = DB.ExecuteScalar<long>(
					Transaction,
					"NL_CashRequestsSave",
					CommandSpecies.StoredProcedure,
					DB.CreateTableParameter("Tbl", this.cashRequest)
				);

				NL_AddLog(LogType.Info, "Strategy End", this.cashRequest, CashRequestID, Error, null);

				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
				Log.Alert(ex, "Failed to save NL_CashRequests, {0}", this.cashRequest);
				Error = string.Format("Failed to save NL_CashRequests, {0}, ex: {1}", this.cashRequest, ex.Message);
				NL_AddLog(LogType.Error, "Strategy Faild", this.cashRequest, null, Error, ex.StackTrace);
			} // try
		} // Execute

		public long CashRequestID { get; set; }
		public string Error { get; set; }

		private readonly NL_CashRequests cashRequest;
	} // class AddCashRequest
} // ns
