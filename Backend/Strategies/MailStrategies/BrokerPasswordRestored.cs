namespace EzBob.Backend.Strategies.MailStrategies {
	using Ezbob.Database;
	using Ezbob.Logger;

	public class BrokerPasswordRestored : PasswordRestored {
		#region constructor

		public BrokerPasswordRestored(int nBrokerID, string password, AConnection oDb, ASafeLog oLog) : base(nBrokerID, password, oDb, oLog) {
		} // constructor

		#endregion constructor

		public override string Name { get { return "Broker Password Restored"; } } // Name

		#region property ProfilePage

		protected override string ProfilePage {
			get { return "https://app.ezbob.com/Broker#login"; }
		} // ProfilePage

		#endregion property ProfilePage

		#region property Salutation

		protected override string Salutation {
			get { return "Dear broker"; }
		} // Salutation

		#endregion property Salutation

		#region method LoadRecipientData

		protected override void LoadRecipientData() {
			Log.Debug("Loading broker data...");

			BrokerData = new BrokerData();

			BrokerData.Load(BrokerID, DB);

			Log.Debug("Loading broker data complete.");
		} // LoadRecipientData

		#endregion method LoadRecipientData

		#region properties

		protected virtual int BrokerID {
			get { return CustomerId; } // get
			set { CustomerId = value; } // set
		} // BrokerID

		protected virtual CustomerData BrokerData {
			get { return CustomerData; } // get
			set { CustomerData = value; } // set
		} // BrokerData

		#endregion properties
	} // class BrokerPasswordRestored
} // namespace
