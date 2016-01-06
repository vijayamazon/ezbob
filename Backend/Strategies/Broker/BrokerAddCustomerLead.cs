namespace Ezbob.Backend.Strategies.Broker {
	using System;
	using Exceptions;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Backend.Strategies.MailStrategies;
	using EZBob.DatabaseLib.Model.Database;
	using JetBrains.Annotations;

	public class BrokerAddCustomerLead : AStrategy {
		public BrokerAddCustomerLead(
			string sLeadFirstName,
			string sLeadLastName,
			string sLeadEmail,
			string sLeadAddMode,
			string sContactEmail,
			CustomerOriginEnum origin
		) {
			m_oSp = new SpBrokerAddCustomerLead(DB, Log) {
				LeadFirstName = sLeadFirstName,
				LeadLastName = sLeadLastName,
				LeadEmail = sLeadEmail,
				LeadAddMode = sLeadAddMode,
				ContactEmail = (sContactEmail ?? string.Empty).Trim(),
				Origin = (int)origin
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
				new BrokerLeadSendInvitation(m_oResultRow.LeadID, m_oSp.ContactEmail, m_oSp.Origin).Execute();
		} // Execute

		private readonly SpBrokerAddCustomerLead m_oSp;
		private SpBrokerAddCustomerLead.ResultRow m_oResultRow;

		private bool SendEmail {
			get { return (m_oResultRow != null) && (m_oResultRow.SendEmail != 0); } // get
		} // SendEmail

		private class SpBrokerAddCustomerLead : AStoredProc {
			public SpBrokerAddCustomerLead(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) { } // constructor

			public override bool HasValidParameters() {
				return
					!string.IsNullOrWhiteSpace(LeadFirstName) &&
					!string.IsNullOrWhiteSpace(LeadLastName) &&
					!string.IsNullOrWhiteSpace(LeadEmail) &&
					!string.IsNullOrWhiteSpace(LeadAddMode) &&
					!string.IsNullOrWhiteSpace(ContactEmail) &&
					(Origin > 0);
			} // HasValidParameters

			[UsedImplicitly]
			public string LeadFirstName { get; set; }
			[UsedImplicitly]
			public string LeadLastName { get; set; }
			[UsedImplicitly]
			public string LeadEmail { get; set; }
			[UsedImplicitly]
			public string LeadAddMode { get; set; }
			[UsedImplicitly]
			public string ContactEmail { get; set; }
			[UsedImplicitly]
			public int Origin { get; set; }

			[UsedImplicitly]
			public DateTime DateCreated {
				get { return DateTime.UtcNow; }
				// ReSharper disable ValueParameterNotUsed
				set {
					// nothing here, but it has to be here.
				} // set
				// ReSharper restore ValueParameterNotUsed
			} // DateCreated

			public class ResultRow : AResultRow {
				public string ErrorMsg { get; [UsedImplicitly] set; }
				public int LeadID { get; [UsedImplicitly] set; }

				[FieldName("SendEmailOnCreate")]
				public int SendEmail { get; [UsedImplicitly] set; }
			} // ResultRow
		} // SpBrokerAddCustomerLead
	} // class BrokerAddCustomerLead
} // namespace Ezbob.Backend.Strategies.Broker
