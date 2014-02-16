﻿namespace Ezbob.Backend.Models {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.Serialization;

	#region class BrokerCustomerEntry

	[DataContract]
	public class BrokerCustomerEntry {
		#region public

		#region constructor

		public BrokerCustomerEntry() {
			m_oMps = new SortedDictionary<string, int>();
			LoanDate = ms_oLongTimeAgo;
		} // constructor

		#endregion constructor

		#region properties

		[DataMember]
		public int CustomerID { get; set; }

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

		#endregion properties

		#region method AddMpLoan

		public void AddMpLoan(string sMpTypeName, decimal nLoanAmount, DateTime? oLoanDate) {
			if (nLoanAmount > 0) {
				if (LoanDate == ms_oLongTimeAgo) {
					LoanDate = oLoanDate.Value;
					LoanAmount = nLoanAmount;
				}
				else if (LoanDate > oLoanDate.Value) {
					LoanDate = oLoanDate.Value;
					LoanAmount = nLoanAmount;
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

		#endregion method AddMpLoan

		#endregion public

		#region private

		private static readonly DateTime ms_oLongTimeAgo = new DateTime(1976, 7, 1).Date;

		private readonly SortedDictionary<string, int> m_oMps;

		#endregion private
	} // class BrokerCustomerEntry

	#endregion class BrokerCustomerEntry
} // namespace EzBob.Backend.Strategies.Broker
