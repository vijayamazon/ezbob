namespace Ezbob.Backend.Strategies.Investor {
	using System;
	using System.Linq;
	using System.Collections.Generic;
	using Ezbob.Backend.Models.Investor;
	using Ezbob.Database;

	public class LoadAccountingData : AStrategy {

		public LoadAccountingData() {}//constructor

		public override string Name { get { return "LoadAccountingData"; } }

		public override void Execute() {
			Result = LoadFromDb();
			Log.Info("Load accounting data from DB complete.");

			var resultFiltered = new List<AccountingDataModel>();
			currentInvestorID = -1;

			foreach (var investorDataSet in Result) {

				if (!investorDataSet.IsRepaymentsBankAccountActive) {
					investorDataSet.IsInvestorActive = false;
					investorDataSet.AccumulatedRepayments = 0;
				}

				if (currentInvestorID < 0 || resultFiltered.All(x => x.InvestorID != investorDataSet.InvestorID))
					resultFiltered.Add(investorDataSet);
				else {
					var anotherDataSetOfTheSameInvestor = resultFiltered.Find(x => x.InvestorID == investorDataSet.InvestorID);
					if (!resultFiltered.Find(x => x.InvestorID == investorDataSet.InvestorID).IsRepaymentsBankAccountActive) {
						resultFiltered.Remove(anotherDataSetOfTheSameInvestor);
						resultFiltered.Add(investorDataSet);
					}
				}

				currentInvestorID = investorDataSet.InvestorID;
				Result = resultFiltered;
			}
		}//Execute

		private List<AccountingDataModel> LoadFromDb() {
			try {
				List<AccountingDataModel> data = DB.Fill<AccountingDataModel>("I_InvestorLoadAccountingData", CommandSpecies.StoredProcedure);
				return data;
			} catch (Exception ex) {
				Log.Warn(ex, "Failed to load accounting data from DB");
				throw;
			}

		}//LoadFromDb

		public List<AccountingDataModel> Result { get; set; }
		private int currentInvestorID;
	}
}
