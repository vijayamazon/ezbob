namespace Reports.Alibaba.Funnel {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;
	using OfficeOpenXml;

	public class FunnelCreator : IAlibaba {

		// ReSharper disable UnusedParameter.Local
		// oDateEnd: for future use.
		public FunnelCreator(DateTime? oDateEnd, AConnection oDB, ASafeLog oLog) {
			if (oDB == null)
				throw new Exception("Database connection not specified for Funnel report.");

			m_oDateEnd = oDateEnd;
			m_oDB = oDB;
			m_oLog = oLog ?? new SafeLog();

			Report = new ExcelPackage();
		} // constructor
		// ReSharper restore UnusedParameter.Local

		public void Generate() {
			Report = new ExcelPackage();

			new Funnel(Report, null, m_oDateEnd, m_oDB, m_oLog).Generate();

			m_oDB.ForEachRowSafe(
				sr => new Funnel(Report, sr["BatchName"], m_oDateEnd, m_oDB, m_oLog).Generate(),
				"RptAlibabaFunnel_LoadBatchNames",
				CommandSpecies.StoredProcedure
			);

			Report.AutoFitColumns();
		} // Generate

		public ExcelPackage Report { get; private set; }

		private readonly ASafeLog m_oLog;
		private readonly AConnection m_oDB;
		private readonly DateTime? m_oDateEnd;

	} // class FunnelCreator
} // namespace
