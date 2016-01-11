namespace Ezbob.Backend.Strategies.NewLoan.Collection {
	using System;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Backend.Models;
	using Ezbob.Database;

	/// <summary>
	/// Late Customer Status Change
	/// </summary>
	public class LateCustomerStatusChange : AStrategy {
		public LateCustomerStatusChange(int customerID, int loanID, CollectionStatusNames status, CollectionType type, DateTime now) {
			CustomerID = customerID;
			LoanID = loanID;
			Status = status;
			Type = type;
			this.now = now;
		}

		private readonly DateTime now;
		public override string Name { get { return "LateCustomerStatusChange"; } }
		public int CustomerID { get; set; }
		public int LoanID { get; set; }
		public CollectionStatusNames Status { get; set; }
		public CollectionType Type { get; set; }

		public override void Execute() {
			if (!CurrentValues.Instance.NewLoanRun) {
				NL_AddLog(LogType.Info, "NL disabled by configuration", null, null, null, null);
				return;
			}
			try {
				NL_AddLog(LogType.Info, "Strategy Start", new object[] { this.CustomerID, this.LoanID, this.Status, this.Type }, null, null, null);
				Log.Info("Changing collection status to customer {0} loan {1} type {2} status {3}", CustomerID, LoanID, Type, Status);
				// updats dbo.Customer.CollectionStatus, CollectionDescription; add record to CustomerStatusHistory
				bool wasChanged = DB.ExecuteScalar<bool>("UpdateCollectionStatus",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerID", CustomerID),
					new QueryParameter("CollectionStatus", (int)Status),
					new QueryParameter("Now", this.now));
				if (!wasChanged) {
					Log.Info("ChangeStatus to customer {0} loan {1} status {2} was not changed - customer already in this status", CustomerID, LoanID, Status);
				}
				//TODO update loan collection status if want to be on loan level and not on customer level. WILL BE IMPLEMENTED IN BI JOB (fill in GetLoanDBState table???)
				Log.Info("update loan collection status if want to be on loan level and not on customer level for customer {0}, loan {1}", CustomerID, LoanID);
				NL_AddLog(LogType.Info, "Strategy End", null, new object[] { this.CustomerID, this.LoanID, this.Status, this.Type }, null, null);
			} catch (Exception ex) {
				NL_AddLog(LogType.Error, "Strategy Faild", null, null, ex.ToString(), ex.StackTrace);
			}

			//ChangeStatus
		}//Execute

	}// class SetLateLoanStatus
} // namespace
