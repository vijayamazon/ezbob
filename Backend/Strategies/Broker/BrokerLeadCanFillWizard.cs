namespace Ezbob.Backend.Strategies.Broker {
	using Ezbob.Database;
	using Ezbob.Logger;
	using EZBob.DatabaseLib.Model.Database;

	public class BrokerLeadCanFillWizard: AStrategy {
		public BrokerLeadCanFillWizard(int nLeadID, string sLeadEmail, string sContactEmail, CustomerOriginEnum origin) {
			m_nRequestedLeadID = nLeadID;
			m_sRequestedLeadEmail = sLeadEmail;
			m_sContactEmail = sContactEmail;
			this.origin = (int)origin;
		} // constructor

		public override string Name {
			get { return "Broker lead can fill wizard"; } // get
		} // Name

		public int CustomerID {
			get { return ResultRow == null ? 0 : ResultRow.CustomerID; }
		} // CustomerID

		public int LeadID { get { return ResultRow == null ? 0 : ResultRow.LeadID; } } // LeadID

		public string LeadEmail { get { return ResultRow == null ? string.Empty : ResultRow.LeadEmail; }} // LeadEmail

		public string FirstName { get { return ResultRow == null ? string.Empty : ResultRow.FirstName; }} // FirstName

		public string LastName { get { return ResultRow == null ? string.Empty : ResultRow.LastName; }} // LastName

		public override void Execute() {
			Init();

			ResultRow = StoredProc.FillFirst<LeadDetailsResultRow>();
		} // Execute

		protected BrokerLeadCanFillWizard() {}

		protected virtual LeadDetailsResultRow ResultRow { get; set; }
		protected virtual AStoredProcedure StoredProc { get; set; }

		protected virtual void Init() {
			ResultRow = null;

			StoredProc = new SpBrokerLeadCanFillWizard(DB, Log) {
				LeadID = m_nRequestedLeadID,
				LeadEmail = m_sRequestedLeadEmail,
				ContactEmail = m_sContactEmail,
				Origin = this.origin,
			};
		} // Init

		protected class LeadDetailsResultRow : AResultRow {
			public int CustomerID { get; set; } // CustomerID

			public int LeadID { get; set; } // LeadID

			public string LeadEmail { get; set; } // LeadEmail

			public string FirstName { get; set; } // FirstName

			public string LastName { get; set; } // LastName
		} // LeadDetailsResultRow

		private readonly int m_nRequestedLeadID;
		private readonly string m_sRequestedLeadEmail;
		private readonly string m_sContactEmail;
		private readonly int origin;

		private class SpBrokerLeadCanFillWizard : AStoredProc {
			public SpBrokerLeadCanFillWizard(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {} // constructor

			public override bool HasValidParameters() {
				if (string.IsNullOrWhiteSpace(ContactEmail) || (Origin <= 0))
					return false;

				if ((LeadID > 0) && !string.IsNullOrWhiteSpace(LeadEmail))
					return false;

				return true;
			} // HasValidParameters

			public int LeadID { get; set; } // LeadID

			public string LeadEmail { get; set; } // LeadEmail

			public string ContactEmail { get; set; } // ContactEmail

			public int Origin { get; set; } // Origin
		} // class SpBrokerLeadCanFillWizard
	} // class BrokerLeadCanFillWizard
} // namespace Ezbob.Backend.Strategies.Broker
