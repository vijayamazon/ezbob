namespace Reports.UiReportsExt {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Data.Common;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;

	public class UiReportExt : SafeLog {
		public UiReportExt(AConnection oDB, DateTime oDateStart, DateTime oDateEnd, ASafeLog log) : base(log) {
			VerboseLogging = false;

			m_oDB = oDB;
			m_oDateStart = oDateStart;
			m_oDateEnd = oDateEnd;

			m_oControls = new SortedDictionary<int, UiControlData>();
			m_oCustomers = new SortedDictionary<int, CustomerData>();
			m_oCustomerControl = new SortedTable<int, int, bool>();

			m_oCurHandler = null;
		} // constructor

		public Tuple<DataTable, ColumnInfo[]> Run() {
			m_oDB.ForEachRow(
				HandleRow,
				"RptUiReportExtData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@DateStart", m_oDateStart),
				new QueryParameter("@DateEnd", m_oDateEnd)
			);

			if (m_oProgress != null)
				m_oProgress.Log();

			Msg("Generating report...");

			var oOutput = new DataTable();

			var oColumns = new List<ColumnInfo>();

			oOutput.Columns.Add("Origin", typeof(string));
			oOutput.Columns.Add("UserID", typeof(int));
			oOutput.Columns.Add("FirstName", typeof(string));
			oOutput.Columns.Add("LastName", typeof(string));
			oOutput.Columns.Add("WizardStepName", typeof(string));
			oOutput.Columns.Add("TypeOfBusiness", typeof(string));
			oOutput.Columns.Add("Offline", typeof(string));

			oColumns.Add(new ColumnInfo("Origin", "Origin"));
			oColumns.Add(new ColumnInfo("UserID", "UserID"));
			oColumns.Add(new ColumnInfo("FirstName", "FirstName"));
			oColumns.Add(new ColumnInfo("LastName", "LastName"));
			oColumns.Add(new ColumnInfo("WizardStepName", "WizardStepName"));
			oColumns.Add(new ColumnInfo("TypeOfBusiness", "TypeOfBusiness"));
			oColumns.Add(new ColumnInfo("Offline", "Offline"));

			Msg("Generating report complete.");

			var oControls = new List<UiControlData>(m_oControls.Values);
			oControls.Sort();

			foreach (var oCtrl in oControls) {
				oOutput.Columns.Add(oCtrl.Name, typeof (string));
				oColumns.Add(new ColumnInfo(oCtrl.Name, oCtrl.Name));
			} // for

			var oCustomers = new List<CustomerData>(m_oCustomers.Values);
			oCustomers.Sort();

			foreach (var oCustomer in oCustomers) {
				var oRow = new List<object> {
					oCustomer.Origin,
					oCustomer.ID, oCustomer.FirstName, oCustomer.Surname,
					oCustomer.WizardStepName, oCustomer.TypeOfBusiness,
					oCustomer.IsOffline ? "offline" : "online"
				};

				var oCollectedData = m_oCustomerControl.Contains(oCustomer.ID) ? m_oCustomerControl[oCustomer.ID] : null;

				foreach (var oCtrl in oControls)
					oRow.Add(oCollectedData != null && oCollectedData.ContainsKey(oCtrl.ID) ? "x" : string.Empty);

				oOutput.Rows.Add(oRow.ToArray());
			} // for each customer

			return new Tuple<DataTable, ColumnInfo[]>(oOutput, oColumns.ToArray());
		} // Run

		public bool VerboseLogging { get; set; }

		private ActionResult HandleRow(DbDataReader oRow, bool bRowSetStart) {
			if (bRowSetStart) {
				switch (oRow["TableName"].ToString()) {
				case "UiControls":
					if (VerboseLogging)
						Debug("UiReportExt: current row handler set to SaveUiControl.");

					m_oCurHandler = SaveUiControl;
					break;

				case "Customer":
					if (VerboseLogging)
						Debug("UiReportExt: current row handler set to SaveCustomer.");

					m_oCurHandler = SaveCustomer;
					break;

				case "UiEvents":
					if (VerboseLogging)
						Debug("UiReportExt: current row handler set to ProcessEvent.");

					m_oCurHandler = ProcessEvent;

					m_oProgress = new ProgressCounter("{0} events processed", this);
					break;

				default:
					Alert("UiReportExt: no handler found for table {0}!", oRow["TableName"]);
					m_oCurHandler = null;
					break;
				} // switch
			} // if

			if (m_oCurHandler == null)
				throw new ArgumentException("No current row handler set.");

			m_oCurHandler(oRow);

			return ActionResult.Continue;
		} // HandleRow

		private void SaveUiControl(DbDataReader oRow) {
			int nID = Convert.ToInt32(oRow["UiControlID"]);
			string sName = Convert.ToString(oRow["UiControlName"]);
			int nPosition = Convert.ToInt32(oRow["Position"]);

			var oCtrl = new UiControlData { ID = nID, Name = sName, Position = nPosition };
			m_oControls[nID] = oCtrl;

			if (VerboseLogging)
				Debug("UiReportExt: UiControl[{0}] = {1}", nID, oCtrl);
		} // SaveUiControl

		private void SaveCustomer(DbDataReader oRow) {
			var oCustomer = new CustomerData(oRow);

			m_oCustomers[oCustomer.ID] = oCustomer;

			if (VerboseLogging)
				Debug("UiReportExt: customer {0}", oCustomer);
		} // SaveCustomer

		private void ProcessEvent(DbDataReader oRow) {
			int nControlID = Convert.ToInt32(oRow["UiControlID"]);
			int nCustomerID = Convert.ToInt32(oRow["UserID"]);

			m_oCustomerControl[nCustomerID, nControlID] = true;

			if (VerboseLogging)
				Debug("UiReportExt: customer {0} event {1}", nCustomerID, nControlID);

			m_oProgress++;
		} // ProcessEvent

		private ProgressCounter m_oProgress;

		private readonly SortedDictionary<int, UiControlData> m_oControls;
		private readonly SortedDictionary<int, CustomerData> m_oCustomers;
		private readonly SortedTable<int, int, bool> m_oCustomerControl;

		private readonly AConnection m_oDB;
		private readonly DateTime m_oDateStart;
		private readonly DateTime m_oDateEnd;

		private Action<DbDataReader> m_oCurHandler;
	} // class UiReportExt
} // namespace
