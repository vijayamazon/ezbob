namespace EzBob.Backend.Strategies.MailStrategies {
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class BrokerPasswordChanged : PasswordChanged {
		#region constructor

		public BrokerPasswordChanged(int nBrokerID, Password oPassword, AConnection oDB, ASafeLog oLog) : base(nBrokerID, oPassword, oDB, oLog) {
		} // constructor

		public BrokerPasswordChanged(int nBrokerID, string sPassword, AConnection oDB, ASafeLog oLog) : base(nBrokerID, sPassword, oDB, oLog) {
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Broker password changed"; }
		} // Name

		#endregion property Name

		#region method LoadRecipientData

		protected override void LoadRecipientData() {
			Log.Debug("Loading broker data...");

			BrokerData = new BrokerData(this, BrokerID, DB);
			BrokerData.Load();

			Log.Debug("Loading broker data complete.");
		} // LoadRecipientData

		#endregion method LoadRecipientData

		#region property FirstName

		protected override string FirstName {
			get { return BrokerData.FirstName; } // get
		} // FirstName

		#endregion property FirstName

		#region properties

		protected virtual int BrokerID {
			get { return CustomerId; } // get
			set { CustomerId = value; } // set
		} // BrokerID

		protected virtual BrokerData BrokerData {
			get { return (BrokerData)CustomerData; } // get
			set { CustomerData = value; } // set
		} // BrokerData

		#endregion properties
	} // class BrokerPasswordChanged
} // namespace
