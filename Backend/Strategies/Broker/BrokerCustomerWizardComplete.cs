namespace Ezbob.Backend.Strategies.Broker {
	using System.Threading;
	using Ezbob.Database;
	using MailStrategies;
		
	public class BrokerCustomerWizardComplete : AStrategy {
		public BrokerCustomerWizardComplete(int nCustomerID) {
			this.customerID = nCustomerID;
		} // constructor

		public override string Name {
			get { return "Broker customer wizard complete"; } // get
		} // Name

		public override void Execute() {
			new BrokerDeleteCustomerLead(DB, Log) {
				CustomerID = this.customerID,
				ReasonCode = BrokerDeleteCustomerLead.DeleteReasonCode.SignedUp.ToString(),
			}.ExecuteNonQuery();

			//wait the timeout period before sending the under review email if was automatic decision don't send the under review email
			Thread.Sleep(ConfigManager.CurrentValues.Instance.WizardAutomationTimeout * 1000);
			SafeReader sr = DB.GetFirst("GetCustomerDetailsForStateCalculation", CommandSpecies.StoredProcedure, new QueryParameter("CustomerId", this.customerID));
			string status = sr["Status"];
			if (status == "Rejected" || status == "Approved") {
				Log.Info("Customer {0} was {1}, not sending under review email", this.customerID, status);
				return;
			}

			new EmailUnderReview(this.customerID).Execute();

			
		} // Execute

		private readonly int customerID;

	} // class BrokerCustomerWizardComplete
} // namespace Ezbob.Backend.Strategies.Broker
