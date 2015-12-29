namespace Ezbob.Backend.Strategies.NewLoan.Collection {
	using System;
	using System.Collections.Generic;
	using ConfigManager;
	using Ezbob.Backend.Models;
	using Ezbob.Database;

	public class LateLoanCured : Misc.SetLateLoanStatus {

		public LateLoanCured(DateTime? runTime) {
			if (runTime != null)
				now = (DateTime)runTime;
			else
				now = DateTime.UtcNow;

			base.now = now;
			this.loansList = new List<CollectionDataModel>();
		} // constructor

		public new DateTime now { get; private set; }
		public override string Name { get { return "LateLoanCured"; } }
		private readonly List<CollectionDataModel> loansList;

		public override void Execute() {
			if (!CurrentValues.Instance.NewLoanRun) {
				NL_AddLog(LogType.Info, "NL disabled by configuration", null, null, null, null);
				return;
			}

			NL_AddLog(LogType.Info, "Strategy Start", now, null, null, null);

			try {

				//-----------Change status to enabled for cured loans--------------------------------
				DB.ForEachRowSafe((sr, bRowsetStart) => {
					var model = new CollectionDataModel {
						CustomerID = sr["CustomerID"],
						LoanID = sr["OldLoanID"],
						LoanHistoryID = sr["LoanHistoryID"],
						NLLoanID = sr["LoanID"]
					};
					if (CurrentValues.Instance.SendCollectionMailOnNewLoan == false) {
						model.UpdateCustomerAllowed = false;
					}
					this.loansList.Add(model);
					return ActionResult.Continue;
				}, "NL_CuredLoansGet", CommandSpecies.StoredProcedure);

				foreach (var model in this.loansList) {
					HandleCuredLoan(model.CustomerID, model.LoanID, model);
				}

				NL_AddLog(LogType.Info, "Strategy end", now, null, null, null);

				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
				NL_AddLog(LogType.Error, "Strategy failed", null, ex.Message, ex.ToString(), ex.StackTrace);
			}
		}//Execute

		/*private void HandleCuredLoan(int customerID, int loanID, long loanHistoryID) {
			//TODO Don't delete will be uncomment on fully NL activation.
			var collectionChangeStatus = new LateCustomerStatusChange(customerID, loanID, CollectionStatusNames.Enabled, CollectionType.Cured, this.now);
			collectionChangeStatus.Execute();
			new AddCollectionLog(new CollectionLog() {
				LoanID = loanID,
				TimeStamp = this.now,
				Type = collectionChangeStatus.Type.ToString(),
				CustomerID = customerID,
				LoanHistoryID = loanHistoryID,
				Comments = string.Empty,
				Method = CollectionMethod.ChangeStatus.ToString()
			}).Execute();
		}//HandleCuredLoan  */

	}// class CollectionCuredLoans
} // namespace
