namespace Ezbob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;
	using Ezbob.Backend.Strategies.MailStrategies.API;
	using UserManagement.EmailConfirmation;

	public class BrokerGreeting : ABrokerMailToo {
		public BrokerGreeting(int nBrokerID) : base(nBrokerID, true) {
		} // constructor

		public override string Name { get { return "Broker greeting"; } } // Name

		protected override void SetTemplateAndVariables() {
			TemplateName = "Broker greeting";

			var ecg = new EmailConfirmationGenerate(BrokerData.UserID);
			ecg.Execute();

			Variables = new Dictionary<string, string> {
				{ "BrokerName", BrokerData.FirmName },
				{ "ContactName", BrokerData.FullName },
				{ "Link", string.Format("{0}/confirm/{1}", BrokerData.OriginSite, ecg.Token) }
			};
		} // SetTemplateAndVariables

		protected virtual int BrokerID {
			get { return CustomerId; } // get
			set { CustomerId = value; } // set
		} // BrokerID

		protected virtual BrokerData BrokerData {
			get { return (BrokerData)CustomerData; } // get
			set { CustomerData = value; } // set
		} // BrokerData

		protected override void LoadRecipientData() {
			Log.Debug("Loading broker data...");

			BrokerData = new BrokerData(this, BrokerID, DB);
			BrokerData.Load();

			Log.Debug("Loading broker data complete.");
		} // LoadRecipientData

		protected override Addressee[] GetRecipients() {
			var aryAddresses = new List<Addressee>();

			if (!string.IsNullOrWhiteSpace(BrokerData.Email))
				aryAddresses.Add(new Addressee(BrokerData.Email, userID: BrokerData.BrokerID, isBroker: true, origin: BrokerData.Origin, addSalesforceActivity: false));

			return aryAddresses.ToArray();
		} // GetRecipients
	} // class BrokerGreeting
} // namespace
