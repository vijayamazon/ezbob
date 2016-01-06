// ReSharper disable UnusedAutoPropertyAccessor.Local
// The comment above is here because there are many properties in this class and its subclasses that
// are used implicitly (filled via reflection when loaded from DB).
namespace Ezbob.Backend.Strategies.Broker {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using EZBob.DatabaseLib.Model.Database;

	public class BrokerLoadCustomerList : AStrategy {
		public BrokerLoadCustomerList(string sContactEmail, int brokerID, CustomerOriginEnum? origin) {
			m_oSpCustomers = new SpBrokerLoadCustomerList(DB, Log) {
				ContactEmail = sContactEmail,
				BrokerID = brokerID,
				Origin = origin == null ? 0 : (int)origin,
			};

			m_oSpLeads = new SpBrokerLoadLeadList(DB, Log) {
				ContactEmail = sContactEmail,
				BrokerID = brokerID,
				Origin = origin == null ? 0 : (int)origin,
			};

			m_oCustomers = new SortedDictionary<int, BrokerCustomerEntry>();
			m_oLeads = new List<BrokerCustomerEntry>();
		} // constructor

		public override string Name {
			get { return "Broker load customer list"; }
		} // Name

		public override void Execute() {
			m_oSpCustomers.ForEachResult<SpBrokerLoadCustomerList.ResultRow>(row => {
				m_oCustomers[row.CustomerID] = new BrokerCustomerEntry {
					CustomerID = row.CustomerID,
					RefNumber = row.RefNumber,
					FirstName = row.FirstName,
					LastName = row.LastName,
					Email = row.Email,
					IsWaitingForSignature = row.Signature,
					WizardStep = row.WizardStep,
					Status = row.Status,
					ApplyDate = row.ApplyDate,

					ApprovedAmount = row.ApprovedAmount,
					CommissionAmount = row.CommissionAmount,
					Marketplaces = row.Marketplaces,
					LoanAmount = row.LoanAmount,
					SetupFee = row.SetupFee
				};

				if (row.LoanDate != default(DateTime))
					m_oCustomers[row.CustomerID].LoanDate = row.LoanDate;

				if (row.CommissionPaymentDate != default(DateTime))
					m_oCustomers[row.CustomerID].CommissionPaymentDate = row.CommissionPaymentDate;

				return ActionResult.Continue;
			});

			m_oSpLeads.ForEachResult<SpBrokerLoadLeadList.ResultRow>(row => {
				if ((row.CustomerID > 0) && m_oCustomers.ContainsKey(row.CustomerID)) {
					m_oCustomers[row.CustomerID].SetLead(
						row.LeadID,
						row.IsDeleted,
						row.DateLastInvitationSent,
						row.FirstName,
						row.LastName
					);
				} else if (row.CustomerID == 0) {
					m_oLeads.Add(new BrokerCustomerEntry {
						CustomerID = 0,
						RefNumber = "",
						FirstName = row.FirstName,

						LastName = row.LastName,
						Email = row.Email,
						WizardStep = "",
						Status = "Application not started",
						ApplyDate = row.DateCreated,
						Marketplaces = "",
					}.SetLead(row.LeadID, row.IsDeleted, row.DateLastInvitationSent, row.FirstName, row.LastName));
				} // if

				return ActionResult.Continue;
			});
		} // Execute

		public List<BrokerCustomerEntry> Customers {
			get {
				List<BrokerCustomerEntry> lst = m_oCustomers.Values.ToList();
				lst.AddRange(m_oLeads);
				return lst;
			} // get
		} // Customers

		private readonly SortedDictionary<int, BrokerCustomerEntry> m_oCustomers;
		private readonly List<BrokerCustomerEntry> m_oLeads;

		private readonly SpBrokerLoadCustomerList m_oSpCustomers;

		private readonly SpBrokerLoadLeadList m_oSpLeads;

		private class SpBrokerLoadCustomerList : AStoredProc {
			public SpBrokerLoadCustomerList(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) { } // constructor

			public override bool HasValidParameters() {
				return (!string.IsNullOrWhiteSpace(ContactEmail) && (Origin > 0)) || (BrokerID > 0);
			} // HasValidParameters

			public string ContactEmail { get; set; }

			public int Origin { get; set; }

			public int BrokerID { get; set; }

			public class ResultRow : AResultRow {
				public int CustomerID { get; set; }
				public string FirstName { get; set; }
				public string LastName { get; set; }
				public string Email { get; set; }
				public bool Signature { get; set; }
				public string RefNumber { get; set; }
				public string WizardStep { get; set; }
				public string Status { get; set; } // need to change
				public DateTime ApplyDate { get; set; }
				public decimal LoanAmount { get; set; }
				public DateTime LoanDate { get; set; }
				public decimal SetupFee { get; set; }
				public decimal ApprovedAmount { get; set; }
				public decimal CommissionAmount { get; set; }
				public DateTime CommissionPaymentDate { get; set; }
				public string Marketplaces { get; set; }
			} // class ResultRow
		} // class SpBrokerLoadCustomerList

		private class SpBrokerLoadLeadList : AStoredProc {
			public SpBrokerLoadLeadList(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) { } // constructor

			public override bool HasValidParameters() {
				return (!string.IsNullOrWhiteSpace(ContactEmail) && (Origin > 0)) || (BrokerID > 0);
			} // HasValidParameters

			public string ContactEmail { get; set; }

			public int Origin { get; set; }

			public int BrokerID { get; set; }

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
		} // class SpBrokerLoadLeadList
	} // class BrokerLoadCustomerList
} // namespace Ezbob.Backend.Strategies.Broker
