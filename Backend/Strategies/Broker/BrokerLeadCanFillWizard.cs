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
			m_oResultRow = null;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Broker lead can fill wizard"; } // get
		} // Name

		#endregion property Name

		#region property CustomerID

		public int CustomerID {
			get { return m_oResultRow == null ? 0 : m_oResultRow.CustomerID; }
		} // CustomerID

		#endregion property CustomerID

		#region property LeadID

		public int LeadID { get { return m_oResultRow == null ? 0 : m_oResultRow.LeadID; } } // LeadID

		#endregion property LeadID

		#region property LeadEmail

		public string LeadEmail { get { return m_oResultRow == null ? string.Empty : m_oResultRow.LeadEmail; }} // LeadEmail

		#endregion property LeadEmail

		#region property FirstName

		public string FirstName { get { return m_oResultRow == null ? string.Empty : m_oResultRow.FirstName; }} // FirstName

		#endregion property FirstName

		#region property LastName

		public string LastName { get { return m_oResultRow == null ? string.Empty : m_oResultRow.LastName; }} // LastName

		#endregion property LastName

		#region method Execute

		public override void Execute() {
			var sp = new SpBrokerLeadCanFillWizard(DB, Log) {
				LeadID = m_nRequestedLeadID,
				LeadEmail = m_sRequestedLeadEmail,
				ContactEmail = m_sContactEmail,
			};

			if (sp.HasValidParameters()) {
				sp.ForEachResult(row => {
					m_oResultRow = (SpBrokerLeadCanFillWizard.ResultRow)row;
					return ActionResult.SkipAll;
				});
			} // if
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private readonly int m_nRequestedLeadID;
		private readonly string m_sRequestedLeadEmail;
		private readonly string m_sContactEmail;
		private SpBrokerLeadCanFillWizard.ResultRow m_oResultRow;

		#region class SpBrokerLeadCanFillWizard

		internal class SpBrokerLeadCanFillWizard : AStoredProcedure {
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

			#region class ResultRow

			public class ResultRow : AResultRow {
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
			} // ResultRow

			#endregion class ResultRow
		} // class SpBrokerLeadCanFillWizard

		#endregion class SpBrokerLeadCanFillWizard

		#endregion private
	} // class BrokerLeadCanFillWizard

	#endregion class BrokerLeadCanFillWizard
} // namespace EzBob.Backend.Strategies.Broker
