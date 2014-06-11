namespace EzBob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;
	using API;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class BrokerFillForCustomerComplete : Greeting {
		#region public

		#region constructor

		public BrokerFillForCustomerComplete(int nBrokerID, int nCustomerID, AConnection oDB, ASafeLog oLog) : base(nCustomerID, "", oDB, oLog) {
			BrokerID = nBrokerID;

			m_aryTemplateNames = new string[] {
				"Broker fill for customer greeting",
				"Broker fill for customer complete",
			};
		} // constructor

		#endregion constructor

		public override string Name { get { return "Broker fill for customer complete"; } } // Name

		#region method Execute

		public override void Execute() {
			for (m_nCurrentPassNumber = 0; m_nCurrentPassNumber < 2; m_nCurrentPassNumber++)
				base.Execute();
		} // Execute

		#endregion method Execute

		#endregion public

		#region protected

		protected virtual int BrokerID { get; private set; } // BrokerID

		protected virtual BrokerData BrokerData { get; set; } // BrokerData

		#region method LoadRecipientData

		protected override void LoadRecipientData() {
			if (m_nCurrentPassNumber != 0)
				return;

			base.LoadRecipientData();

			Log.Debug("Loading broker data...");

			BrokerData = new BrokerData(this, BrokerID, DB);
			BrokerData.Load();

			Log.Debug("Loading broker data complete.");
		} // LoadRecipientData

		#endregion method LoadRecipientData

		#region method SetTemplateAndVariables

		protected override void SetTemplateAndVariables() {
			TemplateName = m_aryTemplateNames[m_nCurrentPassNumber];

			if (m_nCurrentPassNumber == 0) {
				Variables = new Dictionary<string, string> {
					{ "FirstName", CustomerData.FirstName },
					{ "LastName", CustomerData.Surname },
					{ "CustomerName", CustomerData.FirstName + " " + CustomerData.Surname },
					{ "FirmName", BrokerData.FirmName },
					{ "ContactName", BrokerData.FullName },
					{ "ContactEmail", BrokerData.Email },
					{ "CustomerSiteLink", CustomerSite },
					{ "BrokerSiteLink", BrokerSite + "#customer/" + CustomerData.RefNum },
				};
			} // if first pass
		} // SetTemplateAndVariables

		#endregion method SetTemplateAndVariables

		#region method GetRecipients

		protected override Addressee[] GetRecipients() {
			if (m_nCurrentPassNumber == 0)
				return base.GetRecipients();

			return new [] { new Addressee(BrokerData.Email) };
		} // GetRecipients

		#endregion method GetRecipients

		#region method ActionAtEnd

		protected override void ActionAtEnd() {
			if (m_nCurrentPassNumber == 0)
				base.ActionAtEnd();
		} // ActionAtEnd

		#endregion method ActionAtEnd

		#endregion protected

		#region private

		private int m_nCurrentPassNumber;
		private readonly string[] m_aryTemplateNames;

		#endregion private
	} // class BrokerFillForCustomerComplete
} // namespace
