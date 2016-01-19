namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using ConfigManager;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Database;

	public class AddLoanLegals : AStrategy {
		public AddLoanLegals(int customerID, NL_LoanLegals loanLegals) {
			this.customerID = customerID;
			this.loanLegals = loanLegals;
		} //constructor

		public override string Name { get { return "AddLoanLegal"; } }

		public override void Execute() {
			if (!CurrentValues.Instance.NewLoanRun) {
				NL_AddLog(LogType.Info, "NL disabled by configuration", null, null, null, null);
				return;
			}

			NL_AddLog(LogType.Info, "Strategy Start", this.loanLegals, null, Error, null);

			try {
				GetLastOffer g = new GetLastOffer(this.customerID);
				g.Execute();
				var lastOffer = g.Offer;

				if (lastOffer.OfferID > 0) {
					this.loanLegals.OfferID = lastOffer.OfferID;
				} else {
					Log.Info("Last offer not found");
					Error = "Last offer not found";
					NL_AddLog(LogType.DataExsistense, "Strategy Faild - Last offer not found", this.loanLegals, null, Error, null);
					return;
				}

				this.loanLegals.OfferID = lastOffer.OfferID;
				LoanLegalsID = DB.ExecuteScalar<long>("NL_LoanLegalsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanLegals>("Tbl", this.loanLegals));

				NL_AddLog(LogType.Info, "Strategy End", this.loanLegals, LoanLegalsID, Error, null);

				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
				Error = ex.Message;
				Log.Alert("Failed to add NL_LoanLegals. err: {0}", ex);
				Error = "Failed to add NL_LoanLegals. Err: " + ex.Message;
				NL_AddLog(LogType.Error, "Strategy Faild", this.loanLegals, null, Error, ex.StackTrace);
			}

		}//Execute

		public long LoanLegalsID { get; set; }
		public string Error { get; set; }

		private readonly int customerID;
		private readonly NL_LoanLegals loanLegals;
	}
}//ns
