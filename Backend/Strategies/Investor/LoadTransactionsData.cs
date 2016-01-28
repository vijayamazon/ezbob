namespace Ezbob.Backend.Strategies.Investor {
	using System;
	using System.Collections.Generic;
	using System.Linq;
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

				if (this.bankAccountTypeID == (int)I_InvestorAccountTypeEnum.Repayments) {
					this.spName = "I_InvestorLoadRepaymentsTransactionsData";
					this.bankTransactionsData = DB.Fill<TransactionsDataModel>("I_InvestorLoadRepaymentsBankTransactionsData", CommandSpecies.StoredProcedure,
					new QueryParameter("InvestorID", investorId),
					new QueryParameter("BankAccountTypeID", bankAccountTypeId));
				}

				List<TransactionsDataModel> data = DB.Fill<TransactionsDataModel>(this.spName, CommandSpecies.StoredProcedure,
				new QueryParameter("InvestorID", investorId),
				new QueryParameter("BankAccountTypeID", bankAccountTypeId));

				if (this.bankAccountTypeID == (int)I_InvestorAccountTypeEnum.Repayments) {
					data.AddRange(this.bankTransactionsData);
					data = data.OrderByDescending(t => t.Timestamp).ToList();
				}

				return data;

			} catch (Exception ex) {
				Log.Warn(ex, "Failed to load transactions data from DB");
				throw;
			}//try
		}//LoadFromDb

		public List<TransactionsDataModel> Result { get; set; }
		public List<TransactionsDataModel> bankTransactionsData; 
		private readonly int investorID;
		private readonly int bankAccountTypeID;
		private string spName;
	}//LoadTransactionsData
}//ns


