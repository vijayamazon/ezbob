namespace Ezbob.Backend.Strategies.Investor {
	using Ezbob.Backend.Models.Investor;
	using System;
	using Ezbob.Database;

	public class ManageInvestorDetails : AStrategy {
		public ManageInvestorDetails(InvestorModel investor) {
			this.Investor = investor;
		}//ctor

		public override string Name { get { return "ManageInvestorDetails"; } }

		public override void Execute() {
			DateTime now = DateTime.UtcNow;
			var con = DB.GetPersistent();
			con.BeginTransaction();

			try {
				QueryParameter[] queryParameters = {
                    new QueryParameter("InvestorID", this.Investor.InvestorID),
					new QueryParameter("Name", this.Investor.Name), 
                    new QueryParameter("FundingLimitForNotification", this.Investor.FundingLimitForNotification), 
					new QueryParameter("IsActive", this.Investor.IsActive),
					new QueryParameter("InvestorTypeID", this.Investor.InvestorType.InvestorTypeID),
					new QueryParameter("Timestamp", now)
                };

				DB.ExecuteNonQuery(con, "I_InvestorDetailsUpdate", CommandSpecies.StoredProcedure, queryParameters);
			} catch (Exception ex) {
				Log.Warn(ex, "Failed to update investor {0} on DB", this.Investor.InvestorID);
				con.Rollback();
				Result = false;
				throw;
			}//try

			con.Commit();
			Result = true;
			Log.Info("Update investor {0} details data into DB complete.", this.Investor.InvestorID);
		}//Execute

		public bool Result { get; set; }
		private readonly InvestorModel Investor;
	}//ManageInvestorDetails
}//ns

