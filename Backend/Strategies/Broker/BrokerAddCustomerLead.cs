namespace EzBob.Backend.Strategies.Broker {
	using System;
	using Exceptions;
	using Ezbob.Database;
	using Ezbob.Logger;
	using EzBob.Backend.Strategies.MailStrategies;

	public class BrokerAddCustomerLead : AStrategy {

		public BrokerAddCustomerLead(string sLeadFirstName, string sLeadLastName, string sLeadEmail, string sLeadAddMode, string sContactEmail, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_oSp = new SpBrokerAddCustomerLead(DB, Log) {
				LeadFirstName = sLeadFirstName,
				LeadLastName = sLeadLastName,
				LeadEmail = sLeadEmail,
				LeadAddMode = sLeadAddMode,
				ContactEmail = (sContactEmail ?? string.Empty).Trim(),
			};

			m_oResultRow = null;
		} // constructor

		public override string Name {
			get { return "Broker: add customer lead"; }
		} // Name

		public override void Execute() {
			if (!m_oSp.HasValidParameters())
				return;

			m_oResultRow = m_oSp.FillFirst<SpBrokerAddCustomerLead.ResultRow>();

			if (!string.IsNullOrWhiteSpace(m_oResultRow.ErrorMsg))
				throw new StrategyWarning(this, m_oResultRow.ErrorMsg);

			if (m_oResultRow.LeadID < 1)
				throw new StrategyWarning(this, "Failed to add a customer lead.");

			if (SendEmail)
				new BrokerLeadSendInvitation(m_oResultRow.LeadID, m_oSp.ContactEmail, DB, Log).Execute();
		} // Execute

		private readonly SpBrokerAddCustomerLead m_oSp;
		private SpBrokerAddCustomerLead.ResultRow m_oResultRow;

		private bool SendEmail {
			get { return (m_oResultRow != null) && (m_oResultRow.SendEmail != 0); } // get
		} // SendEmail

		private class SpBrokerAddCustomerLead : AStoredProcedure {

			public SpBrokerAddCustomerLead(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {} // constructor

			public override bool HasValidParameters() {
				return
					!string.IsNullOrWhiteSpace(LeadFirstName) &&
					!string.IsNullOrWhiteSpace(LeadLastName) &&
					!string.IsNullOrWhiteSpace(LeadEmail) &&
					!string.IsNullOrWhiteSpace(LeadAddMode) &&
					!string.IsNullOrWhiteSpace(ContactEmail);
			} // HasValidParameters

			public string LeadFirstName { get; set; }
			public string LeadLastName { get; set; }
			public string LeadEmail { get; set; }
			public string LeadAddMode { get; set; }
			public string ContactEmail { get; set; }

			public DateTime DateCreated {
				get { return DateTime.UtcNow; }
				set { 
					// nothing here, but it has to be here.
				} // set
			} // DateCreated

			public class ResultRow : AResultRow {
				public string ErrorMsg { get; set; }
				public int LeadID { get; set; }

				[FieldName("SendEmailOnCreate")]
				public int SendEmail { get; set; }
			} // ResultRow

			protected override string GetName() {
				return "BrokerAddCustomerLead";
			} // GetName

		} // SpBrokerAddCustomerLead

	} // class BrokerAddCustomerLead
} // namespace EzBob.Backend.Strategies.Broker
