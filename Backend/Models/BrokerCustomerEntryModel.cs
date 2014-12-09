namespace Ezbob.Backend.Models {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.Serialization;

	[DataContract(IsReference = true)]
	public class BrokerCustomerEntry {

		public BrokerCustomerEntry() {
			m_oMps = new SortedDictionary<string, int>();
			LoanDate = ms_oLongTimeAgo;
		} // constructor

		[DataMember]
		public int CustomerID { get; set; }

		[DataMember]
		public string RefNumber { get; set; }

		[DataMember]
		public string FirstName { get; set; }

		[DataMember]
		public string LastName { get; set; }

		[DataMember]
		public string Email { get; set; }

		[DataMember]
		public string WizardStep { get; set; }

		[DataMember]
		public string Status { get; set; }

		[DataMember]
		public DateTime ApplyDate { get; set; }

		[DataMember]
		public string Marketplaces { get; set; }

		[DataMember]
		public decimal LoanAmount { get; set; }

		[DataMember]
		public DateTime LoanDate { get; set; }

		[DataMember]
		public decimal SetupFee { get; set; }

		[DataMember]
		public int LeadID { get; set; }

		[DataMember]
		public bool IsLeadDeleted { get; set; }

		[DataMember]
		public DateTime LastInvitationSent { get; set; }

		public void AddMpLoan(string sMpTypeName, decimal nLoanAmount, DateTime? oLoanDate, decimal nSetupFee) {
			if (nLoanAmount > 0) {
				if (LoanDate == ms_oLongTimeAgo) {
					LoanDate = oLoanDate.Value;
					LoanAmount = nLoanAmount;
					SetupFee = nSetupFee;
				}
				else if (LoanDate > oLoanDate.Value) {
					LoanDate = oLoanDate.Value;
					LoanAmount = nLoanAmount;
					SetupFee = nSetupFee;
				} // if
			} // if

			if (!string.IsNullOrWhiteSpace(sMpTypeName)) {
				if (m_oMps.ContainsKey(sMpTypeName))
					m_oMps[sMpTypeName]++;
				else
					m_oMps[sMpTypeName] = 1;
			} // if

			Marketplaces = string.Join(", ", m_oMps.Select(kv => string.Format("{0} {1}", kv.Value, kv.Key)));
		} // AddMpLoan

		public BrokerCustomerEntry SetLead(int nLeadID, bool bIsLeadDeleted, DateTime oLastInvitationSent, string sFirstName, string sLastName) {
			LeadID = nLeadID;
			IsLeadDeleted = bIsLeadDeleted;
			LastInvitationSent = oLastInvitationSent;

			if (string.IsNullOrWhiteSpace(FirstName))
				FirstName = sFirstName;

			if (string.IsNullOrWhiteSpace(LastName))
				LastName = sLastName;

			return this;
		} // SetLead

		private static readonly DateTime ms_oLongTimeAgo = new DateTime(1976, 7, 1).Date;

		private readonly SortedDictionary<string, int> m_oMps;

	} // class BrokerCustomerEntry
} // namespace EzBob.Backend.Strategies.Broker
