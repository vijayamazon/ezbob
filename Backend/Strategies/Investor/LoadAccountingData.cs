namespace Ezbob.Backend.Strategies.Investor {
	using System;
	using System.Linq;
	using System.Collections.Generic;
	using Ezbob.Backend.Models.Investor;
	using Ezbob.Database;

	public class LoadAccountingData : AStrategy {

		public override string Name { get { return "LoadAccountingData"; } }

		public override void Execute() {
			Result = LoadFromDb();
			Log.Info("Load accounting data from DB complete.");
			/*
			var resultFiltered = new List<AccountingDataModel>();
			this.currentInvestorID = -1;

			foreach (var investorDataSet in Result) {
				if (this.currentInvestorID < 0 || resultFiltered.All(x => x.InvestorID != investorDataSet.InvestorID)) {
					if (!investorDataSet.IsRepaymentsBankAccountActive) 
						investorDataSet.AccumulatedRepayments = 0;
					
					resultFiltered.Add(investorDataSet);
				} else {
					var anotherDataSetOfTheSameInvestor = resultFiltered.Find(x => x.InvestorID == investorDataSet.InvestorID);
					if (investorDataSet.IsRepaymentsBankAccountActive) {
						if (anotherDataSetOfTheSameInvestor.IsRepaymentsBankAccountActive)
							Log.Info("Multiple active repayments bank accounts for InvestorID=" + investorDataSet.InvestorID);
						resultFiltered.Remove(anotherDataSetOfTheSameInvestor);
						resultFiltered.Add(investorDataSet);
					}//if
				}//if

				this.currentInvestorID = investorDataSet.InvestorID;
			}//foreach
			
			Result = resultFiltered;
			*/
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
