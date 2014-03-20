namespace EzBob.Backend.Strategies.Broker {
	using Ezbob.Database;
	using Ezbob.Logger;

	#region class BrokerLeadCanFillWizard

	public class BrokerLeadCanFillWizard: AStrategy {
		#region public

		#region constructor

		public BrokerLeadCanFillWizard(int nLeadID, string sLeadEmail, string sContactEmail, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_nRequestedLeadID = nLeadID;
			m_sRequestedLeadEmail = sLeadEmail;
			m_sContactEmail = sContactEmail;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Broker lead can fill wizard"; } // get
		} // Name

		#endregion property Name

		#region property CustomerID

		public int CustomerID {
			get { return ResultRow == null ? 0 : ResultRow.CustomerID; }
		} // CustomerID

		#endregion property CustomerID

		#region property LeadID

		public int LeadID { get { return ResultRow == null ? 0 : ResultRow.LeadID; } } // LeadID

		#endregion property LeadID

		#region property LeadEmail

		public string LeadEmail { get { return ResultRow == null ? string.Empty : ResultRow.LeadEmail; }} // LeadEmail

		#endregion property LeadEmail

		#region property FirstName

		public string FirstName { get { return ResultRow == null ? string.Empty : ResultRow.FirstName; }} // FirstName

		#endregion property FirstName

		#region property LastName

		public string LastName { get { return ResultRow == null ? string.Empty : ResultRow.LastName; }} // LastName

		#endregion property LastName

		#region method Execute

		public override void Execute() {
			Init();

			if (StoredProc.HasValidParameters())
				ResultRow = StoredProc.FillFirst<LeadDetailsResultRow>();
		} // Execute

		#endregion method Execute

		#endregion public

		#region protected

		protected virtual LeadDetailsResultRow ResultRow { get; set; }
		protected virtual AStoredProcedure StoredProc { get; set; }

		#region method Init

		protected virtual void Init() {
			ResultRow = null;

			StoredProc = new SpBrokerLeadCanFillWizard(DB, Log) {
				LeadID = m_nRequestedLeadID,
				LeadEmail = m_sRequestedLeadEmail,
				ContactEmail = m_sContactEmail,
			};
		} // Init

		#endregion method Init

		#region class LeadDetailsResultRow

		protected class LeadDetailsResultRow : AResultRow {
			#region property CustomerID

			public int CustomerID { get; set; } // CustomerID

			#endregion property CustomerID

			#region property LeadID

			public int LeadID { get; set; } // LeadID

			#endregion property LeadID

			#region property LeadEmail

			public string LeadEmail { get; set; } // LeadEmail

			#endregion property LeadEmail

			#region property FirstName

			public string FirstName { get; set; } // FirstName

			#endregion property FirstName

			#region property LastName

			public string LastName { get; set; } // LastName

			#endregion property LastName
		} // LeadDetailsResultRow

		#endregion class LeadDetailsResultRow

		#endregion protected

		#region private

		private readonly int m_nRequestedLeadID;
		private readonly string m_sRequestedLeadEmail;
		private readonly string m_sContactEmail;

		#region class SpBrokerLeadCanFillWizard

		private class SpBrokerLeadCanFillWizard : AStoredProcedure {
			#region constructor

			public SpBrokerLeadCanFillWizard(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {} // constructor

			#endregion constructor

			#region method HasValidParameters

			public override bool HasValidParameters() {
				if (string.IsNullOrWhiteSpace(ContactEmail))
					return false;

				if ((LeadID > 0) && !string.IsNullOrWhiteSpace(LeadEmail))
					return false;

				return true;
			} // HasValidParameters

			#endregion method HasValidParameters

			#region property LeadID

			public int LeadID { get; set; } // LeadID

			#endregion property LeadID

			#region property LeadEmail

			public string LeadEmail { get; set; } // LeadEmail

			#endregion property LeadEmail

			#region property ContactEmail

			public string ContactEmail { get; set; } // ContactEmail

			#endregion property ContactEmail

			#region method GetName

			protected override string GetName() {
				return "BrokerLeadCanFillWizard";
			} // GetName

			#endregion method GetName
		} // class SpBrokerLeadCanFillWizard

		#endregion class SpBrokerLeadCanFillWizard

		#endregion private
	} // class BrokerLeadCanFillWizard

	#endregion class BrokerLeadCanFillWizard
} // namespace EzBob.Backend.Strategies.Broker
