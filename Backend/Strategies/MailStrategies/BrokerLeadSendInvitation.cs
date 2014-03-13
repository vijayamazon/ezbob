namespace EzBob.Backend.Strategies.MailStrategies {
	using System;
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class BrokerLeadSendInvitation : AMailStrategyBase {
		#region public

		#region constructor

		public BrokerLeadSendInvitation(int nLeadID, string sBrokerContactEmail, AConnection oDB, ASafeLog oLog) : base(nLeadID, true, oDB, oLog) {
			m_sBrokerContactEmail = sBrokerContactEmail;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name { get { return "Broker lead send invitation"; } } // Name

		#endregion property Name

		#endregion public

		#region protected

		#region method LoadCustomerData

		protected override void LoadCustomerData() {
			Log.Debug("loading broker lead data...");

			LeadData = new BrokerLeadData(m_sBrokerContactEmail);

			LeadData.Load(CustomerId, DB);

			Log.Debug("loading broker lead complete.");
		} // LoadCustomerData

		#endregion method LoadCustomerData

		#region method SetTemplateAndVariables

		// TODO: real template and its arguments
		protected override void SetTemplateAndVariables() {
			TemplateName = "Greeting - Offline";

			Variables = new Dictionary<string, string> {
				{"Email", CustomerData.Mail},
				{"ConfirmEmailAddress", "-+-+-+-+-"}
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

		#endregion private
	} // class BrokerLeadSendInvitation
} // namespace
