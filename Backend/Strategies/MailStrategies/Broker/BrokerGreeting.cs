using System.Collections.Generic;
using Ezbob.Database;
using Ezbob.Logger;

namespace EzBob.Backend.Strategies.MailStrategies {
	using UserManagement.EmailConfirmation;

	public class BrokerGreeting : ABrokerMailToo {

		public BrokerGreeting(int nBrokerID, AConnection oDb, ASafeLog oLog) : base(nBrokerID, true, oDb, oLog) {
		} // constructor

		public override string Name { get { return "Broker greeting"; } } // Name

		protected override void SetTemplateAndVariables() {
			TemplateName = "Broker greeting";

			var ecg = new EmailConfirmationGenerate(BrokerData.UserID, DB, Log);
			ecg.Execute();

			Variables = new Dictionary<string, string> {
				{ "BrokerName", BrokerData.FirmName },
				{ "ContactName", BrokerData.FullName },
				{ "Link", string.Format("{0}/confirm/{1}", CustomerSite, ecg.Token) }
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

	} // class BrokerGreeting
} // namespace
