namespace EzBob.Backend.Strategies.MailStrategies {
	using System;
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class BrokerLeadSendInvitation : AMailStrategyBase {
		#region public

		#region constructor

		public BrokerLeadSendInvitation(
			int nLeadID,
			string sBrokerContactEmail,
			AConnection oDB,
			ASafeLog oLog
		) : base(nLeadID, true, oDB, oLog) {
			m_sBrokerContactEmail = sBrokerContactEmail;
			m_oSp = new BrokerLeadSaveInvitationToken(DB, Log);
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name { get { return "Broker lead send invitation"; } } // Name

		#endregion property Name

		#endregion public

		#region protected

		#region method LoadRecipientData

		protected override void LoadRecipientData() {
			Log.Debug("Loading broker lead data...");

			LeadData = new BrokerLeadData(m_sBrokerContactEmail);

			LeadData.Load(LeadID, DB);

			Log.Debug("Loading broker lead complete.");
		} // LoadRecipientData

		#endregion method LoadRecipientData

		#region method SetTemplateAndVariables

		protected override void SetTemplateAndVariables() {
			m_oSp.Token = Guid.NewGuid();
			m_oSp.LeadID = LeadID;

			m_oSp.ExecuteNonQuery();

			// TODO: real template and its arguments
			TemplateName = "Greeting";

			Variables = new Dictionary<string, string> {
				{ "Email", LeadData.Mail },
				{ "ConfirmEmailAddress", CustomerSite + "?bloken=" + m_oSp.Token.ToString("N") }
			};
		} // SetTemplateAndVariables

		#endregion method SetTemplateAndVariables

		#region method ActionAtEnd

		protected override void ActionAtEnd() {
			DB.ExecuteNonQuery("BrokerLeadSetInvitationSetDate",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@LeadID", LeadID),
				new QueryParameter("@DateInvitationSent", DateTime.UtcNow)
			);
		} // ActionAtEnd

		#endregion method ActionAtEnd

		#region properties

		protected virtual CustomerData LeadData {
			get { return CustomerData; } // get
			set { CustomerData = value; } // set
		} // LeadData

		protected virtual int LeadID {
			get { return CustomerId; } // get
			set { CustomerId = value; } // set
		} // LeadID

		#endregion properties

		#endregion protected

		#region private

		private readonly string m_sBrokerContactEmail;
		private readonly BrokerLeadSaveInvitationToken m_oSp;

		#region class BrokerLeadSaveInvitationToken

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

		#endregion class BrokerLeadSaveInvitationToken

		#region property CustomerSite

		private string CustomerSite {
			get {
				if (string.IsNullOrWhiteSpace(m_sCustomerSite)) {
					DB.ForEachRowSafe(
						(sr, bRowsetStart) => {
							m_sCustomerSite = ((string)sr["Value"] ?? string.Empty).Trim();
							return ActionResult.SkipAll;
						},
						"LoadConfigurationVariable",
						CommandSpecies.StoredProcedure,
						new QueryParameter("@CfgVarName", "CustomerSite")
					);

					if (string.IsNullOrWhiteSpace(m_sCustomerSite))
						m_sCustomerSite = "https://app.ezbob.com";

					if (m_sCustomerSite.EndsWith("/"))
						m_sCustomerSite = m_sCustomerSite.Substring(0, m_sCustomerSite.Length - 1);
				} // if

				return m_sCustomerSite;
			} // get
		} // CustomerSite

		private string m_sCustomerSite;

		#endregion property CustomerSite

		#endregion private
	} // class BrokerLeadSendInvitation
} // namespace
