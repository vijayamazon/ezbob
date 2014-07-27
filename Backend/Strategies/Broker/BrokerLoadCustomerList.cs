namespace EzBob.Backend.Strategies.Broker {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;

	#region class BrokerLoadCustomerList

	public class BrokerLoadCustomerList : AStrategy {
		#region public

		#region constructor

		public BrokerLoadCustomerList(string sContactEmail, int nBrokerID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_oSpCustomers = new SpBrokerLoadCustomerList(DB, Log) {
				ContactEmail = sContactEmail,
				BrokerID = nBrokerID,
			};

			m_oSpLeads = new SpBrokerLoadLeadList(DB, Log) {
				ContactEmail = sContactEmail,
				BrokerID = nBrokerID,
			};

			m_oCustomers = new SortedDictionary<int, BrokerCustomerEntry>();
			m_oLeads = new List<BrokerCustomerEntry>();
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Broker load customer list"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			m_oSpCustomers.ForEachResult<SpBrokerLoadCustomerList.ResultRow>(row => {
				if (!m_oCustomers.ContainsKey(row.CustomerID)) {
					m_oCustomers[row.CustomerID] = new BrokerCustomerEntry {
						CustomerID = row.CustomerID,
						RefNumber = row.RefNumber,
						FirstName = row.FirstName,
						LastName = row.LastName,
						Email = row.Email,
						WizardStep = row.WizardStep,
						Status = row.Status,
						ApplyDate = row.ApplyDate,
					};
				} // if

				m_oCustomers[row.CustomerID].AddMpLoan(row.MpTypeName, row.LoanAmount, row.LoanDate, row.SetupFee);

				return ActionResult.Continue;
			});

			m_oSpLeads.ForEachResult<SpBrokerLoadLeadList.ResultRow>(row => {
				if ((row.CustomerID > 0) && m_oCustomers.ContainsKey(row.CustomerID))
					m_oCustomers[row.CustomerID].SetLead(row.LeadID, row.IsDeleted, row.DateLastInvitationSent, row.FirstName, row.LastName);
				else if (row.CustomerID == 0) {
					m_oLeads.Add(new BrokerCustomerEntry {
						CustomerID = 0,
						RefNumber = "",
						FirstName = row.FirstName,
						LastName = row.LastName,
						Email = row.Email,
						WizardStep = "",
						Status = "Application not started",
						ApplyDate = row.DateCreated,
					}.SetLead(row.LeadID, row.IsDeleted, row.DateLastInvitationSent, row.FirstName, row.LastName));
				} // if

				return ActionResult.Continue;
			});
		} // Execute

		#endregion method Execute

		#region property Customers

		public List<BrokerCustomerEntry> Customers {
			get {
				List<BrokerCustomerEntry> lst = m_oCustomers.Values.ToList();
				lst.AddRange(m_oLeads);
				return lst;
			} // get
		} // Customers

		#endregion property Customers

		#endregion public

		#region private

		private readonly SortedDictionary<int, BrokerCustomerEntry> m_oCustomers;
		private readonly List<BrokerCustomerEntry> m_oLeads;

		private readonly SpBrokerLoadCustomerList m_oSpCustomers;

		private readonly SpBrokerLoadLeadList m_oSpLeads;

		#region class SpBrokerLoadCustomerList

		private class SpBrokerLoadCustomerList : AStoredProc {
			#region constructor

			public SpBrokerLoadCustomerList(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {} // constructor

			#endregion constructor

			#region method HasValidParameters

			public override bool HasValidParameters() {
				return !string.IsNullOrWhiteSpace(ContactEmail) || (BrokerID > 0);
			} // HasValidParameters

			#endregion method HasValidParameters

			#region property ContactEmail

			public string ContactEmail { get; set; }

			#endregion property ContactEmail

			#region property BrokerID

			public int BrokerID { get; set; }

			#endregion property BrokerID

			#region class ResultRow

			public class ResultRow : AResultRow {
				public int CustomerID { get; set; }
				public string FirstName { get; set; }
				public string LastName { get; set; }
				public string Email { get; set; }
				public string RefNumber { get; set; }
				public string WizardStep { get; set; }
				public string Status { get; set; }
				public DateTime ApplyDate { get; set; }
				public string MpTypeName { get; set; }
				public decimal LoanAmount { get; set; }
				public DateTime LoanDate { get; set; }
				public decimal SetupFee { get; set; }
			} // class ResultRow

			#endregion class ResultRow
		} // class SpBrokerLoadCustomerList

		#endregion class SpBrokerLoadCustomerList

		#region class SpBrokerLoadLeadList

		private class SpBrokerLoadLeadList : AStoredProc {
			#region constructor

			public SpBrokerLoadLeadList(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) { } // constructor

			#endregion constructor

			#region method HasValidParameters

			public override bool HasValidParameters() {
				return !string.IsNullOrWhiteSpace(ContactEmail) || (BrokerID > 0);
			} // HasValidParameters

			#endregion method HasValidParameters

			#region property ContactEmail

			public string ContactEmail { get; set; }

			#endregion property ContactEmail

			#region property BrokerID

			public int BrokerID { get; set; }

			#endregion property BrokerID

			#region class ResultRow

			public class ResultRow : AResultRow {
				public int LeadID { get; set; }
				public int CustomerID { get; set; }
				public string FirstName { get; set; }
				public string LastName { get; set; }
				public string Email { get; set; }
				public DateTime DateCreated { get; set; }
				public DateTime DateLastInvitationSent { get; set; }
				public bool IsDeleted { get; set; }
			} // class ResultRow

			#endregion class ResultRow
		} // class SpBrokerLoadLeadList

		#endregion class SpBrokerLoadLeadList

		#endregion private
	} // class BrokerLoadCustomerList

	#endregion class BrokerLoadCustomerList
} // namespace EzBob.Backend.Strategies.Broker
