namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using ConfigManager;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Database;

	public class GetLastOffer : AStrategy {
		public GetLastOffer(int customerID) {
			this.customerID = customerID;
		} // constructor

		public override string Name { get { return "GetLastOffer"; } }
		public string Error { get; private set; }

		public override void Execute() {
			if (!CurrentValues.Instance.NewLoanRun) {
				NL_AddLog(LogType.Info, "NL disabled by configuration", null, null, null, null);
				return;
			}

			NL_AddLog(LogType.Info, "Strategy Start", this.customerID, null, null, null);

			try {
				Offer = DB.FillFirst<NL_Offers>("NL_OffersGetLast", CommandSpecies.StoredProcedure, new QueryParameter("CustomerID", this.customerID));

				NL_AddLog(LogType.Info, "Strategy End", this.customerID, Offer, null, null);

				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
				Error = ex.Message;
				NL_AddLog(LogType.Error, "Strategy Faild", this.customerID, null, ex.ToString(), ex.StackTrace);
			}
		} // Execute

		public NL_Offers Offer { get; private set; }
		private readonly int customerID;
	} // class GetLastOffer
} // ns
