namespace Ezbob.Backend.Strategies.Investor {
	using System;
	using Ezbob.Backend.Models.Investor;
	using Ezbob.Database;

	public class LoadSchedulerData : AStrategy {

		public LoadSchedulerData(int investorID) {
			this.investorID = investorID;
		}//constructor

		public override string Name { get { return "LoadSchedulerData"; } }

		public override void Execute() {
			Result = LoadFromDb(this.investorID);
			Log.Info("Load scheduler data from DB complete.");
		}//Execute

		private SchedulerDataModel LoadFromDb(int investorId) {
			try {
				SchedulerDataModel data = DB.FillFirst<SchedulerDataModel>("I_InvestorGetSchedulerData", CommandSpecies.StoredProcedure,
				new QueryParameter("InvestorID", investorId));
				return data;
			} catch (Exception ex) {
				Log.Warn(ex, "Failed to load scheduler data from DB");
				throw;
			}//try
		}//LoadFromDb

		public SchedulerDataModel Result { get; set; }
		private readonly int investorID;
	}//LoadSchedulerData
}//ns
