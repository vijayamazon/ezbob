namespace Ezbob.Backend.Strategies.MailStrategies {
	using System;
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;
	using EZBob.DatabaseLib.Model.Database;

	public class BrokerLeadSendInvitation : AMailStrategyBase {
		public BrokerLeadSendInvitation(
			int nLeadID,
			string sBrokerContactEmail,
			CustomerOriginEnum origin
		) : this(nLeadID, sBrokerContactEmail, (int)origin) {
		} // constructor

		public BrokerLeadSendInvitation(
			int nLeadID,
			string sBrokerContactEmail,
			int origin
		) : base(nLeadID, true) {
			m_sBrokerContactEmail = sBrokerContactEmail;
			this.origin = origin;
			m_oSp = new BrokerLeadSaveInvitationToken(DB, Log);
		} // constructor

		public override string Name { get { return "Broker lead send invitation"; } } // Name

		protected override void LoadRecipientData() {
			Log.Debug("Loading broker lead data...");

			LeadData = new BrokerLeadData(m_sBrokerContactEmail, this.origin, this, LeadID, DB);
			LeadData.Load();

			Log.Debug("Loading broker lead complete.");
		} // LoadRecipientData

		protected override void SetTemplateAndVariables() {
			m_oSp.Token = Guid.NewGuid();
			m_oSp.LeadID = LeadID;

			m_oSp.ExecuteNonQuery();

			TemplateName = "Broker lead invitation";

			Variables = new Dictionary<string, string> {
				{ "CustomerName", LeadData.FullName },
				{ "FirmName", LeadData.FirmName },
				{ "InvitationLink", LeadData.OriginSite + "?bloken=" + m_oSp.Token.ToString("N") },
			};
		} // SetTemplateAndVariables

		protected override void ActionAtEnd() {
			DB.ExecuteNonQuery("BrokerLeadSetInvitationSetDate",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@LeadID", LeadID),
				new QueryParameter("@DateInvitationSent", DateTime.UtcNow)
			);
		} // ActionAtEnd

		protected virtual BrokerLeadData LeadData {
			get { return (BrokerLeadData)CustomerData; } // get
			set { CustomerData = value; } // set
		} // LeadData

		protected virtual int LeadID {
			get { return CustomerId; } // get
			set { CustomerId = value; } // set
		} // LeadID

		private readonly string m_sBrokerContactEmail;
		private readonly int origin;
		private readonly BrokerLeadSaveInvitationToken m_oSp;

		private class BrokerLeadSaveInvitationToken : AStoredProcedure {
			public BrokerLeadSaveInvitationToken(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {} // constructor

			public int LeadID { get; set; }

			public Guid Token { get; set; }

			public DateTime DateCreated {
				get { return DateTime.UtcNow; }
				set { } // keep it here
			} // DateCreated

			public override bool HasValidParameters() {
				return (LeadID > 0) && (Token != null) && (Token != Guid.Empty);
			} // HasValidParameters
		} // BrokerLeadSaveInvitationToken
	} // class BrokerLeadSendInvitation
} // namespace
