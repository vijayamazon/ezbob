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

		public int CustomerID { get; set; } // CustomerID

		#endregion property CustomerID

		#region property LeadID

		public int LeadID { get; set; } // LeadID

		#endregion property LeadID

		#region property LeadEmail

		public string LeadEmail { get; set; } // LeadEmail

		#endregion property LeadEmail

		#region method Execute

		public override void Execute() {
			if (string.IsNullOrWhiteSpace(m_sContactEmail))
				return;

			if ((m_nRequestedLeadID > 0) && !string.IsNullOrWhiteSpace(m_sRequestedLeadEmail))
				return;

			var sp = new SpBrokerLeadCanFillWizard(DB, Log) {
				LeadID = m_nRequestedLeadID,
				LeadEmail = m_sRequestedLeadEmail,
				ContactEmail = m_sContactEmail,
			};

			sp.ForEachResult(row => {
				var oRow = (SpBrokerLeadCanFillWizard.ResultRow)row;

				LeadID = oRow.LeadID;
				LeadEmail = oRow.LeadEmail;
				CustomerID = oRow.CustomerID;

				return ActionResult.SkipAll;
			});
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private readonly int m_nRequestedLeadID;
		private readonly string m_sRequestedLeadEmail;
		private readonly string m_sContactEmail;

		#region class SpBrokerLeadCanFillWizard

		internal class SpBrokerLeadCanFillWizard : AStoredProcedure {
			#region constructor

			public SpBrokerLeadCanFillWizard(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {} // constructor

			#endregion constructor

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
			} // ResultRow

			#endregion class ResultRow
		} // class SpBrokerLeadCanFillWizard

		#endregion class SpBrokerLeadCanFillWizard

		#endregion private
	} // class BrokerLeadCanFillWizard

	#endregion class BrokerLeadCanFillWizard
} // namespace EzBob.Backend.Strategies.Broker
