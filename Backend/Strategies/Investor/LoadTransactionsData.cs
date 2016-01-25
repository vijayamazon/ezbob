namespace Ezbob.Backend.Strategies.Investor {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.Models.Investor;
	using Ezbob.Backend.ModelsWithDB.OpenPlatform;
	using Ezbob.Database;

	public class LoadTransactionsData : AStrategy {

		public LoadTransactionsData(int investorID, int bankAccountTypeID) {
			this.investorID = investorID;
			this.bankAccountTypeID = bankAccountTypeID;
		}//constructor

		public override string Name { get { return "LoadTransactionsData"; } }

		public override void Execute() {
			Result = LoadFromDb(this.investorID, this.bankAccountTypeID);
			Log.Info("Load transactions data from DB complete.");
		}//Execute

		private List<TransactionsDataModel> LoadFromDb(int investorId, int bankAccountTypeId) {
			try {
				if (this.bankAccountTypeID == (int)I_InvestorAccountTypeEnum.Funding)
					this.spName = "I_InvestorLoadFundingTransactionsData";

				if (this.bankAccountTypeID == (int)I_InvestorAccountTypeEnum.Repayments)
					this.spName = "I_InvestorLoadRepaymentsTransactionsData";

				List<TransactionsDataModel> data = DB.Fill<TransactionsDataModel>(this.spName, CommandSpecies.StoredProcedure,
				new QueryParameter("InvestorID", investorId),
				new QueryParameter("BankAccountTypeID", bankAccountTypeId));
				return data;
			} catch (Exception ex) {
				Log.Warn(ex, "Failed to load transactions data from DB");
				throw;
			}//try
		}//LoadFromDb

		public List<TransactionsDataModel> Result { get; set; }
		private readonly int investorID;
		private readonly int bankAccountTypeID;
		private string spName;
	}//LoadTransactionsData
}//ns


