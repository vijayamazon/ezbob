using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using Ezbob.Database;
using Ezbob.Logger;
using Ezbob.Utils;

namespace Reports {
	#region class UiReport

	public class UiReport : SafeLog {
		#region public

		#region constructor

		public UiReport(AConnection oDB, DateTime oDateStart, DateTime oDateEnd, ASafeLog log) : base(log) {
			VerboseLogging = false;

			m_oDB = oDB;
			m_oDateStart = oDateStart;
			m_oDateEnd = oDateEnd;

			m_oUiActions = new SortedDictionary<int, string>();
			m_oUiControls = new SortedDictionary<int, string>();
			m_oAddresses = new SortedDictionary<AddressKey, int>();
			m_oDirectors = new SortedDictionary<int, int>();
			m_oResult = new SortedDictionary<int, UiReportItem>();

			m_oCurHandler = null;
		} // constructor

		#endregion constructor

		#region method Run

		public SortedDictionary<int, UiReportItem> Run() {
			m_oDB.ForEachRow(
				HandleRow,
				"RptUiReportData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@DateStart", m_oDateStart),
				new QueryParameter("@DateEnd", m_oDateEnd)
			);

			if (m_oProgress != null)
				m_oProgress.Log();

			Msg("Generating report...");

			foreach (KeyValuePair<int, UiReportItem> pair in m_oResult)
				pair.Value.Generate();

			Msg("Generating report complete.");

			return m_oResult;
		} // Run

		#endregion method Run

		#region property VerboseLogging

		public bool VerboseLogging { get; set; }

		#endregion property VerboseLogging

		#endregion public

		#region private

		#region method HandleRow

		private ActionResult HandleRow(DbDataReader oRow, bool bRowSetStart) {
			if (bRowSetStart) {
				switch (oRow["TableName"].ToString()) {
				case "UiActions":
					if (VerboseLogging)
						Debug("UiReport: current row handler set to SaveUiAction.");

					m_oCurHandler = SaveUiAction;
					break;

				case "UiControls":
					if (VerboseLogging)
						Debug("UiReport: current row handler set to SaveUiControl.");

					m_oCurHandler = SaveUiControl;
					break;

				case "Customer":
					if (VerboseLogging)
						Debug("UiReport: current row handler set to SaveCustomer.");

					m_oCurHandler = SaveCustomer;
					break;

				case "Address":
					if (VerboseLogging)
						Debug("UiReport: current row handler set to SaveAddress.");

					m_oCurHandler = SaveAddress;
					break;

				case "Director":
					if (VerboseLogging)
						Debug("UiReport: current row handler set to SaveDirector.");

					m_oCurHandler = SaveDirector;
					break;

				case "UiEvents":
					if (VerboseLogging)
						Debug("UiReport: current row handler set to ProcessEvent.");

					m_oCurHandler = ProcessEvent;

					m_oProgress = new ProgressCounter("{0} events processed", this);
					break;

				default:
					Alert("UiReport: no handler found for table {0}!", oRow["TableName"]);
					m_oCurHandler = null;
					break;
				} // switch
			} // if

			if (m_oCurHandler == null)
				throw new ArgumentException("No current row handler set.");

			m_oCurHandler(oRow);

			return ActionResult.Continue;
		} // HandleRow

		#endregion method HandleRow

		#region method SaveUiAction

		private void SaveUiAction(DbDataReader oRow) {
			int nID = Convert.ToInt32(oRow["UiActionID"]);
			string sName = Convert.ToString(oRow["UiActionName"]);

			m_oUiActions[nID] = sName;

			if (VerboseLogging)
				Debug("UiReport: UiAction[{0}] = {1}", nID, sName);
		} // SaveUiAction

		#endregion method SaveUiAction

		#region method SaveUiControl

		private void SaveUiControl(DbDataReader oRow) {
			int nID = Convert.ToInt32(oRow["UiControlID"]);
			string sName = Convert.ToString(oRow["UiControlName"]);

			m_oUiControls[nID] = sName;

			UiReportItem.AddControlToItemGroup(nID, sName);

			if (VerboseLogging)
				Debug("UiReport: UiControl[{0}] = {1}", nID, sName);
		} // SaveUiControl

		#endregion method SaveUiControl

		#region method SaveCustomer

		private void SaveCustomer(DbDataReader oRow) {
			var oCustomer = new CustomerInfo(oRow);

			m_oResult[oCustomer.ID] = new UiReportItem(oCustomer);

			if (VerboseLogging)
				Debug("UiReport: customer {0}", oCustomer);
		} // SaveCustomer

		#endregion method SaveCustomer

		#region method SaveAddress

		private void SaveAddress(DbDataReader oRow) {
			var ak = new AddressKey(oRow);

			int nCount = Convert.ToInt32(oRow["AddressCount"]);

			m_oAddresses[ak] = nCount;

			if (VerboseLogging)
				Debug("UiReport: address[{0}] = {1}", ak, nCount);
		} // SaveAddress

		#endregion method SaveAddress

		#region method SaveDirector

		private void SaveDirector(DbDataReader oRow) {
			int nID = Convert.ToInt32(oRow["CustomerID"]);
			int nCount = Convert.ToInt32(oRow["DirectorCount"]);

			m_oDirectors[nID] = nCount;

			if (VerboseLogging)
				Debug("UiReport: director[{0}] = {1}", nID, nCount);
		} // SaveDirector

		#endregion method SaveDirector

		#region method ProcessEvent

		private void ProcessEvent(DbDataReader oRow) {
			var oEvent = new UiEvent(oRow);

			if (VerboseLogging)
				Debug("UiReport: event {0}", oEvent);

			if (!m_oResult.ContainsKey(oEvent.UserID))
				Alert("Event for unknown user {0}", oEvent.UserID);
			else
				m_oResult[oEvent.UserID].AddEvent(oEvent);

			m_oProgress++;
		} // ProcessEvent

		#endregion method ProcessEvent

		#region properties

		private ProgressCounter m_oProgress;

		private readonly SortedDictionary<int, string> m_oUiActions;
		private readonly SortedDictionary<int, string> m_oUiControls;
		private readonly SortedDictionary<AddressKey, int> m_oAddresses;
		private readonly SortedDictionary<int, int> m_oDirectors;

		private readonly SortedDictionary<int, UiReportItem> m_oResult;

		private readonly AConnection m_oDB;
		private readonly DateTime m_oDateStart;
		private readonly DateTime m_oDateEnd;

		private Action<DbDataReader> m_oCurHandler;

		#endregion properties

		#endregion private
	} // class UiReport

	#endregion class UiReport
} // namespace Reports
