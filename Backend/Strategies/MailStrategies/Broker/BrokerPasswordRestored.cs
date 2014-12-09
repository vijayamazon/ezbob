namespace EzBob.Backend.Strategies.MailStrategies {
	using Ezbob.Database;
	using Ezbob.Logger;

	public class BrokerPasswordRestored : PasswordRestored {

		public BrokerPasswordRestored(int nBrokerID, AConnection oDb, ASafeLog oLog) : base(nBrokerID, oDb, oLog) {
		} // constructor

		public override string Name { get { return "Broker Password Restored"; } } // Name

		protected override string Salutation {
			get { return "Dear broker"; }
		} // Salutation

		protected override void LoadRecipientData() {
			Log.Debug("Loading broker data...");

			BrokerData = new BrokerData(this, BrokerID, DB);
			BrokerData.Load();

			Log.Debug("Loading broker data complete.");
		} // LoadRecipientData

		protected virtual int BrokerID {
			get { return CustomerId; } // get
			set { CustomerId = value; } // set
		} // BrokerID

		protected virtual BrokerData BrokerData {
			get { return (BrokerData)CustomerData; } // get
			set { CustomerData = value; } // set
		} // BrokerData

	} // class BrokerPasswordRestored
} // namespace
