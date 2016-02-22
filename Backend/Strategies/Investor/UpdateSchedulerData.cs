namespace Ezbob.Backend.Strategies.Investor {
	using System;
	using Ezbob.Database;

	public class UpdateSchedulerData : AStrategy {
		public UpdateSchedulerData(
			int underwriterID, 
			int investorID, 
			decimal monthlyFundingCapital,
			int fundsTransferDate, 
			string fundsTransferSchedule, 
			string repaymentsTransferSchedule) {
			this.underwriterID = underwriterID;
			this.investorID = investorID;
			this.monthlyFundingCapital = monthlyFundingCapital;
			this.fundsTransferDate = fundsTransferDate;
			this.repaymentsTransferSchedule = repaymentsTransferSchedule;
			this.fundsTransferSchedule = fundsTransferSchedule;
		}//ctor

		public override string Name { get { return "UpdateSchedulerData"; } }

		public override void Execute() {
			try {
				var InvestorID = DB.ExecuteScalar<int>("I_InvestorUpdateSchedulerData",
					CommandSpecies.StoredProcedure,
					new QueryParameter("InvestorID", this.investorID),
					new QueryParameter("MonthlyFundingCapital", this.monthlyFundingCapital),
					new QueryParameter("FundsTransferDate", this.fundsTransferDate),
					new QueryParameter("FundsTransferSchedule", this.fundsTransferSchedule),
					new QueryParameter("RepaymentsTransferSchedule", this.repaymentsTransferSchedule));

			} catch (Exception ex) {
				Log.Warn(ex, "Failed to update scheduler data for investor {0} into DB by UW {1}", this.investorID, this.underwriterID);
				Result = false;
				throw;
			}//try

			Result = true;
			Log.Info("Updating scheduler data for investor {0} into DB  by UW {1} complete.", this.investorID, this.underwriterID);
		}//Execute

		public bool Result { get; set; }
		private readonly int underwriterID;
		private readonly int investorID;
		private readonly string fundsTransferSchedule;
		private readonly decimal monthlyFundingCapital;
		private readonly int fundsTransferDate;
		private readonly string repaymentsTransferSchedule;
	}//UpdateSchedulerData
}//ns


