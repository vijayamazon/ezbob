namespace Ezbob.Backend.Strategies.Misc {
	using System;
	using Ezbob.Backend.Models;
	using Ezbob.Database;

	public class GetSlidersData : AStrategy {

		public GetSlidersData(int customerID) {

			this.customerID = customerID;

		}//constructor

		public override string Name { get { return "GetSlidersData"; } }

		public override void Execute() {

			Result = LoadFromDb(this.customerID);
			Log.Info("Load sliders data from DB complete.");

		}//Execute

		private SlidersDataModel LoadFromDb(int customerId) {
			try {
				SlidersDataModel data = DB.FillFirst<SlidersDataModel>("GetSlidersData", CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerID", customerId));

				return data;
			} catch (Exception ex) {
				Log.Warn(ex, "Failed to load sliders data from DB");
				throw;
			}

		}//LoadFromDb

		public SlidersDataModel Result { get; set; }
		private readonly int customerID;
	}
}

