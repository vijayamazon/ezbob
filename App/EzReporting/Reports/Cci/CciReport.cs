namespace Reports.Cci {
	using System;
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class CciReport : SafeLog {
		#region public

		#region constructor

		public CciReport(AConnection oDB, ASafeLog log = null) : base(log) {
			m_oDB = oDB;
		} // constructor

		#endregion constructor

		#region method Run

		public List<CciReportItem> Run() {
			var ei = new EarnedInterest.EarnedInterest(m_oDB, EarnedInterest.EarnedInterest.WorkingMode.CciCustomers, false, DateTime.UtcNow, DateTime.UtcNow, this);

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
} // namespace
