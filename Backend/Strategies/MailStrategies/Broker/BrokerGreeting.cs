using System.Collections.Generic;
using Ezbob.Database;
using Ezbob.Logger;

namespace EzBob.Backend.Strategies.MailStrategies {
	using UserManagement.EmailConfirmation;

	public class BrokerGreeting : ABrokerMailToo {
		#region public

		#region constructor

		public BrokerGreeting(int nBrokerID, AConnection oDb, ASafeLog oLog) : base(nBrokerID, true, oDb, oLog) {
		} // constructor

		#endregion constructor

		public override string Name { get { return "Broker greeting"; } } // Name

		#endregion public

		#region protected

		#region method SetTemplateAndVariables

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

		#endregion method SetTemplateAndVariables

		#region property BrokerID

		protected virtual int BrokerID {
			get { return CustomerId; } // get
			set { CustomerId = value; } // set
		} // BrokerID

		#endregion property BrokerID

		#region property BrokerData

		protected virtual BrokerData BrokerData {
			get { return (BrokerData)CustomerData; } // get
			set { CustomerData = value; } // set
		} // BrokerData

		#endregion property BrokerData

		#region method LoadRecipientData

		protected override void LoadRecipientData() {
			Log.Debug("Loading broker data...");

			BrokerData = new BrokerData(this, BrokerID, DB);
			BrokerData.Load();

			Log.Debug("Loading broker data complete.");
		} // LoadRecipientData

		#endregion method LoadRecipientData

		#endregion protected
	} // class BrokerGreeting
} // namespace
