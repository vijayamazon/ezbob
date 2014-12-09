namespace Reports {
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class LoanDateScore : SafeLog {

		public LoanDateScore(AConnection oDB, ASafeLog log = null) : base(log) {
			m_oDB = oDB;

			VerboseLogging = false;
		} // constructor

		public SortedDictionary<int, LoanDateScoreItem>  Run() {
			m_oResult = new SortedDictionary<int, LoanDateScoreItem>();

			m_oDB.ForEachRowSafe(
				HandleLoanDateRow,
				"RptLoanDateScore",
				CommandSpecies.StoredProcedure
			);

			m_oDB.ForEachRowSafe(
				HandleCompanyRow,
				"RptLoanDateScoreNDSPCII",
				CommandSpecies.StoredProcedure
			);

			foreach (KeyValuePair<int, LoanDateScoreItem> pair in m_oResult)
				pair.Value.LoadLastScore(m_oDB);

			return m_oResult;
		} // Run

		public void ToOutput(string sFileName) {
			var fout = new StreamWriter(sFileName, false, Encoding.UTF8);

			fout.WriteLine("{0}{1}{2}",
				"Customer ID,Last Loan Date,Incorporation date,Company score,Company Score Date,",
				"NDSPCII,NDSPCII Date,Company reg #,Company name,Credit limit,",
				"NL Commercial Delphi Score,Probability of Default Score,Stability Odds"
			);

			foreach (KeyValuePair<int, LoanDateScoreItem> pair in m_oResult)
				pair.Value.ToOutput(fout);

			fout.Close();
		} // ToOutput

		public bool VerboseLogging { get; set; }

		private ActionResult HandleLoanDateRow(SafeReader oRow, bool bStartOfRowset) {
			var oItem = new LoanDateScoreItem(oRow, this);

			m_oResult[oItem.CustomerID] = oItem;

			return ActionResult.Continue;
		} // HandleLoanDateRow

		private ActionResult HandleCompanyRow(SafeReader oRow, bool bStartOfRowset) {
			int nCustomerID = oRow["CustomerID"];

			if (m_oResult.ContainsKey(nCustomerID))
				m_oResult[nCustomerID].Add(oRow, m_oDB);

			return ActionResult.Continue;
		} // HandleCompanyRow

		private readonly AConnection m_oDB;
		private SortedDictionary<int, LoanDateScoreItem> m_oResult;

	} // class LoanDateScore

} // namespace Reports
