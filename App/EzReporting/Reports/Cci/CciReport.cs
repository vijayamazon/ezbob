using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ezbob.Database;
using Ezbob.Logger;

namespace Reports {
	#region class CciReport

	public class CciReport : SafeLog {
		#region public

		#region constructor

		public CciReport(AConnection oDB, ASafeLog log = null) : base(log) {
			m_oDB = oDB;
		} // constructor

		#endregion constructor

		#region method Run

		public List<CciReportItem> Run() {
			var ei = new EarnedInterest(m_oDB, EarnedInterest.WorkingMode.CciCustomers, DateTime.UtcNow, DateTime.UtcNow, this);

			SortedDictionary<int, decimal> oEarnedInterestList = ei.Run();
			
			var oResult = new List<CciReportItem>();

			m_oDB.ForEachRow(
				(oRow, ignored) => { oResult.Add(new CciReportItem(oRow, oEarnedInterestList)); return ActionResult.Continue; },
				"RptCciData",
				CommandSpecies.StoredProcedure
			);

			return oResult;
		} // Run

		#endregion method Run

		#endregion public

		#region private

		private readonly AConnection m_oDB;

		#endregion private
	} // class CciReport

	#endregion class CciReport
} // namespace Reports
