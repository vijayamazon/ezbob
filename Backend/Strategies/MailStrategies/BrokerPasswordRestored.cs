namespace EzBob.Backend.Strategies.MailStrategies {
	using Broker;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class BrokerPasswordRestored : PasswordRestored {
		#region constructor

		public BrokerPasswordRestored(int customerId, string password, AConnection oDb, ASafeLog oLog) : base(customerId, password, oDb, oLog) {
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

		#region method LoadCustomerData

		protected override void LoadCustomerData() {
			Log.Debug("loading broker data...");

			CustomerData = new BrokerData();

			CustomerData.Load(CustomerId, DB);

			Log.Debug("loading broker data complete.");
		} // LoadCustomerData

		#endregion method LoadCustomerData
	} // class BrokerPasswordRestored
} // namespace
