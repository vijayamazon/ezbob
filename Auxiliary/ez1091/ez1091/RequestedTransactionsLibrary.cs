using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ezbob.Logger;

namespace ez1091 {

	class RequestedTransactionsLibrary {

		public RequestedTransactionsLibrary(ASafeLog log) {
			m_oLog = new SafeLog(log);

			m_oByDate = new SortedDictionary<DateTime, List<RequestedTransaction>>();
			m_oByLoan = new SortedDictionary<int, List<RequestedTransaction>>();

			LoanIDs = string.Empty;
			Count = 0;
		} // constructor

		public string LoanIDs {
			get {
				if (m_sLoanIDs == string.Empty)
					m_sLoanIDs = string.Join(",", m_oByLoan.Keys);

				return m_sLoanIDs;
			} // get

			private set {
				m_sLoanIDs = (value ?? "").Trim();
			} // set
		} // LoanIDs

		private string m_sLoanIDs;

		public int Count { get; private set; }

		public RequestedTransactionsLibrary Add(string sRawData) {
			var rt = new RequestedTransaction(sRawData, m_oLog);

			LoanIDs = string.Empty;

			if (!m_oByDate.ContainsKey(rt.Date))
				m_oByDate[rt.Date] = new List<RequestedTransaction>();

			m_oByDate[rt.Date].Add(rt);

			if (!m_oByLoan.ContainsKey(rt.LoanID))
				m_oByLoan[rt.LoanID] = new List<RequestedTransaction>();

			m_oByLoan[rt.LoanID].Add(rt);

			Count++;

			return this;
		} // Add

		private readonly SortedDictionary<DateTime, List<RequestedTransaction>> m_oByDate;
		private readonly SortedDictionary<int, List<RequestedTransaction>> m_oByLoan;
		private readonly SafeLog m_oLog;

	} // class RequestedTransactionsLibrary

} // namespace ez1091
