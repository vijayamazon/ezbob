namespace Ezbob.Backend.Strategies.Backfill {
	using System;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Strategies.MailStrategies.Broker;
	using Ezbob.Database;

	public class BackFillBrokerCommissionInvoice : AStrategy {
		public override string Name { get { return "BackFilllBrokerCommissionInvoice"; } }

		public override void Execute() {
			DB.ForEachRowSafe(
				BackFillOneBrokerCommissionInvoice,
				"GetBrokerCommissionsForInvoiceBackFill",
				CommandSpecies.StoredProcedure
			);
		} // Execute

		private void BackFillOneBrokerCommissionInvoice(SafeReader sr) {
			try {
				int loanBrokerCommissionID = sr["LoanBrokerCommissionID"];
				int brokerID = sr["BrokerID"];
				decimal commissionAmount = sr["CommissionAmount"];
				DateTime commissionTime = sr["PaidDate"];
				string sortCode = sr["SortCode"];
				string bankAccount = sr["BankAccount"];
				string customerName = sr["CustomerName"];

				BrokerCommissionInvoice brokerCommissionInvoice = new BrokerCommissionInvoice(
					new BrokerInvoiceCommissionModel {
						BankAccount = bankAccount,
						SortCode = sortCode,
						InvoiceID = loanBrokerCommissionID,
						CustomerName = customerName,
						CommissionAmount = commissionAmount,
						CommissionTime = commissionTime,
						BrokerID = brokerID
					}
				);

				brokerCommissionInvoice.Execute();

				DB.ExecuteNonQuery(
					"UpdateBrokerCommissionsInvoiceBackFill",
					CommandSpecies.StoredProcedure,
					new QueryParameter("@LoanBrokerCommissionID", loanBrokerCommissionID)
				);

				Log.Info("Broker Invoice Backfill for broker {0} commission id {1}", brokerID, loanBrokerCommissionID);
			} catch (Exception ex) {
				Log.Error(
					ex,
					"Failed sending invoice to broker {0} loanBrokerCommissionID {1}",
					sr["BrokerID"],
					sr["LoanBrokerCommissionID"]
				);
			} // try
		} // BackFillOneBrokerCommissionInvoice
	} // class BackFillBrokerCommissionInvoice
} // namespace
