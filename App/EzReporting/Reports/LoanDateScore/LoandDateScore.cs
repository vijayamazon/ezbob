using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Text;
using System.Xml;
using Ezbob.Database;
using Ezbob.Logger;

namespace Reports {
	using System.Globalization;

	#region class LoanDateScore

	public class LoanDateScore : SafeLog {
		#region public

		#region constructor

		public LoanDateScore(AConnection oDB, ASafeLog log = null) : base(log) {
			m_oDB = oDB;

			VerboseLogging = false;
		} // constructor

		#endregion constructor

		#region method Run

		public SortedDictionary<int, LoanDateScoreItem>  Run() {
			m_oResult = new SortedDictionary<int, LoanDateScoreItem>();

			m_oDB.ForEachRow(
				HandleLoanDateRow,
				"RptLoanDateScore",
				CommandSpecies.StoredProcedure
			);


			m_oDB.ForEachRow(
				HandleCompanyRow,
				"RptLoanDateScoreNDSPCII",
				CommandSpecies.StoredProcedure
			);

			return m_oResult;
		} // Run

		#endregion method Run

		#region method ToOutput

		public void ToOutput(string sFileName) {
			var fout = new StreamWriter(sFileName, false, Encoding.UTF8);

			fout.WriteLine("Customer ID;Last Loan Date;Incorporation date;Company score;Company Score Date;NDSPCII;NDSPCII Date;Company reg #;Company name");

			foreach (KeyValuePair<int, LoanDateScoreItem> pair in m_oResult)
				pair.Value.ToOutput(fout);

			fout.Close();
		} // ToOutput

		#endregion method ToOutput

		#region property VerboseLogging

		public bool VerboseLogging { get; set; }

		#endregion property VerboseLogging

		#endregion public

		#region private

		#region method HandleLoanDateRow

		private ActionResult HandleLoanDateRow(DbDataReader oRow, bool bStartOfRowset) {
			var oItem = new LoanDateScoreItem(oRow, this);

			m_oResult[oItem.CustomerID] = oItem;

			return ActionResult.Continue;
		} // HandleLoanDateRow

		#endregion method HandleLoanDateRow

		#region method HandleCompanyRow

		private ActionResult HandleCompanyRow(DbDataReader oRow, bool bStartOfRowset) {
			int nCustomerID = Convert.ToInt32(oRow["CustomerID"]);

			if (m_oResult.ContainsKey(nCustomerID))
				m_oResult[nCustomerID].Add(oRow);

			return ActionResult.Continue;
		} // HandleCompanyRow

		#endregion method HandleCompanyRow

		private readonly AConnection m_oDB;
		private SortedDictionary<int, LoanDateScoreItem> m_oResult;

		#endregion private
	} // class LoanDateScore

	#endregion class LoanDateScore
} // namespace Reports
