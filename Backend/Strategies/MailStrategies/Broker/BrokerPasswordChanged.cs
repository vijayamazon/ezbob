namespace Ezbob.Backend.Strategies.MailStrategies {
	using Ezbob.Backend.Models;

	public class BrokerPasswordChanged : PasswordChanged {
		public BrokerPasswordChanged(int nBrokerID, DasKennwort oPassword) : base(nBrokerID, oPassword) {
		} // constructor

		public BrokerPasswordChanged(int nBrokerID, string sPassword) : base(nBrokerID, sPassword) {
		} // constructor

		public override string Name {
			get { return "Broker password changed"; }
		} // Name

		protected override void LoadRecipientData() {
			Log.Debug("Loading broker data...");

			BrokerData = new BrokerData(this, BrokerID, DB);
			BrokerData.Load();

			Log.Debug("Loading broker data complete.");
		} // LoadRecipientData

		protected override string FirstName {
			get { return BrokerData.FirstName; } // get
		} // FirstName

		protected virtual int BrokerID {
			get { return CustomerId; } // get
			set { CustomerId = value; } // set
		} // BrokerID

		protected virtual BrokerData BrokerData {
			get { return (BrokerData)CustomerData; } // get
			set { CustomerData = value; } // set
		} // BrokerData
	} // class BrokerPasswordChanged
} // namespace
