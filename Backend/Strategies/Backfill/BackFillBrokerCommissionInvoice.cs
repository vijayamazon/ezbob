namespace Ezbob.Backend.Strategies.Backfill {
	using System;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Strategies.MailStrategies.Broker;
	using Ezbob.Database;
	using Ezbob.Utils;

	public class BackFillBrokerCommissionInvoice : AStrategy {
		public override string Name { get { return "BackFilllBrokerCommissionInvoice"; } }

		public override void Execute() {
			var lst = DB.Fill<InvoiceData>(
				"GetBrokerCommissionsForInvoiceBackFill",
				CommandSpecies.StoredProcedure
			);

			Log.Debug("{0} invoices to backfill loaded.", lst.Count);

			this.pc = new ProgressCounter("{0} invoices processed.", Log, 50);

			foreach (var id in lst)
				BackFillOneBrokerCommissionInvoice(id);

			this.pc.Log();
		} // Execute

		private void BackFillOneBrokerCommissionInvoice(InvoiceData id) {
			try {
				BrokerCommissionInvoice brokerCommissionInvoice = new BrokerCommissionInvoice(
					new BrokerInvoiceCommissionModel {
						BankAccount = id.BankAccount,
						SortCode = id.SortCode,
						InvoiceID = id.LoanBrokerCommissionID,
						CustomerName = id.CustomerName,
						CommissionAmount = id.CommissionAmount,
						CommissionTime = id.PaidDate,
						BrokerID = id.BrokerID,
					}
				);

				brokerCommissionInvoice.Execute();

				DB.ExecuteNonQuery(
					"UpdateBrokerCommissionsInvoiceBackFill",
					CommandSpecies.StoredProcedure,
					new QueryParameter("@LoanBrokerCommissionID", id.LoanBrokerCommissionID)
				);

				Log.Info(
					"Broker Invoice Backfill for broker {0} commission id {1}.",
					id.BrokerID,
					id.LoanBrokerCommissionID
				);
			} catch (Exception ex) {
				Log.Error(
					ex,
					"Failed sending invoice to broker {0} commission id {1}.",
					id.BrokerID,
					id.LoanBrokerCommissionID
				);
			} // try

			this.pc.Next();
		} // BackFillOneBrokerCommissionInvoice

		private ProgressCounter pc;

		private class InvoiceData {
			public int LoanBrokerCommissionID { get; set; }
			public int BrokerID { get; set; }
			public decimal CommissionAmount { get; set; }
			public DateTime PaidDate { get; set; }
			public string SortCode { get; set; }
			public string BankAccount { get; set; }
			public string CustomerName { get; set;}
		} // class InvoiceData
	} // class BackFillBrokerCommissionInvoice
} // namespace
