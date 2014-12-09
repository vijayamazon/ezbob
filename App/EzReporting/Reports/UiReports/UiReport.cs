using System;
using System.Collections.Generic;
using System.Data.Common;
using Ezbob.Database;
using Ezbob.Logger;
using Ezbob.Utils;

namespace Reports {

	public class UiReport : SafeLog {

		public UiReport(AConnection oDB, DateTime oDateStart, DateTime oDateEnd, ASafeLog log) : base(log) {
			VerboseLogging = false;

			m_oDB = oDB;
			m_oDateStart = oDateStart;
			m_oDateEnd = oDateEnd;

			m_oUiActions = new SortedDictionary<int, string>();
			m_oAddresses = new SortedDictionary<int, AddressInfo>();
			m_oDirectors = new SortedDictionary<int, int>();
			m_oAccounts = new SortedDictionary<int, int>();
			m_oResult = new SortedDictionary<int, UiReportItem>();

			m_oCurHandler = null;

			m_oControlGroups = new SortedDictionary<UiItemGroups, SortedDictionary<int, string>>();
			foreach (UiItemGroups nItemType in UiItemGroupsSequence.Get())
				m_oControlGroups[nItemType] = new SortedDictionary<int, string>();
		} // constructor

		public List<UiReportItem> Run() {
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

			var oOut = new List<UiReportItem>();

			foreach (KeyValuePair<int, UiReportItem> pair in m_oResult) {
				pair.Value.Generate();
				oOut.Add(pair.Value);
			} // for each

			Msg("Generating report complete.");

			oOut.Sort();

			return oOut;
		} // Run

		public bool VerboseLogging { get; set; }

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

				case "LinkedAccounts":
					if (VerboseLogging)
						Debug("UiReport: current row handler set to SaveLinkedAccounts.");

					m_oCurHandler = SaveLinkedAccounts;
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

		private void SaveUiAction(DbDataReader oRow) {
			int nID = Convert.ToInt32(oRow["UiActionID"]);
			string sName = Convert.ToString(oRow["UiActionName"]);

			m_oUiActions[nID] = sName;

			if (VerboseLogging)
				Debug("UiReport: UiAction[{0}] = {1}", nID, sName);
		} // SaveUiAction

		private void SaveUiControl(DbDataReader oRow) {
			int nID = Convert.ToInt32(oRow["UiControlID"]);
			string sName = Convert.ToString(oRow["UiControlName"]);
			int nGroupID = Convert.ToInt32(oRow["UiControlGroupID"]);

			AddControlToItemGroup(nID, sName, nGroupID);

			if (VerboseLogging)
				Debug("UiReport: UiControl[{0}] = {1}", nID, sName);
		} // SaveUiControl

		private void SaveCustomer(DbDataReader oRow) {
			var oCustomer = new CustomerInfo(oRow, m_oAddresses, m_oDirectors, m_oAccounts);

			m_oResult[oCustomer.ID] = new UiReportItem(oCustomer, m_oControlGroups);

			if (VerboseLogging)
				Debug("UiReport: customer {0}", oCustomer);
		} // SaveCustomer

		private void SaveAddress(DbDataReader oRow) {
			int nCustomerID = Convert.ToInt32(oRow["CustomerID"]);
			string sLabel = null;

			if (m_oAddresses.ContainsKey(nCustomerID)) {
				m_oAddresses[nCustomerID].Add(oRow);

				if (VerboseLogging)
					sLabel = "existing";
			}
			else {
				var ai = new AddressInfo(nCustomerID, oRow);
				m_oAddresses[ai.CustomerID] = ai;

				if (VerboseLogging)
					sLabel = "new";
			} // if

			if (VerboseLogging)
				Debug("UiReport: {1} address {0}", m_oAddresses[nCustomerID], sLabel);
		} // SaveAddress

		private void SaveDirector(DbDataReader oRow) {
			int nID = Convert.ToInt32(oRow["CustomerID"]);
			int nCount = Convert.ToInt32(oRow["DirectorCount"]);

			m_oDirectors[nID] = nCount;

			if (VerboseLogging)
				Debug("UiReport: director[{0}] = {1}", nID, nCount);
		} // SaveDirector

		private void SaveLinkedAccounts(DbDataReader oRow) {
			int nID = Convert.ToInt32(oRow["CustomerID"]);
			int nCount = Convert.ToInt32(oRow["AccountCount"]);

			m_oAccounts[nID] = nCount;

			if (VerboseLogging)
				Debug("UiReport: linked accounts[{0}] = {1}", nID, nCount);
		} // SaveLinkedAccounts

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

		private void AddControlToItemGroup(int nControlID, string sControlName, int nGroupID) {
			if (nGroupID == 3) {
				m_oControlGroups[UiItemGroups.LinkAccounts][nControlID] = sControlName;
				return;
			} // if

			if (!sControlName.StartsWith("personal-info:"))
				return;

			switch (sControlName.Substring(14)) {
			case "first_name":
			case "gender":
			case "last_name":
			case "birth_date_day":
			case "birth_date_month":
			case "birth_date_year":
			case "marital_status":
			case "middle_name":
			case "own_other_property":
				m_oControlGroups[UiItemGroups.PersonalInfo][nControlID] = sControlName;
				break;

			case "residential_status":
			case "time_at_address":
				m_oControlGroups[UiItemGroups.HomeAddress][nControlID] = sControlName;
				break;

			case "daytime_phone":
			case "mobile_phone":
			case "consent_to_search":
			case "continue":
				m_oControlGroups[UiItemGroups.ContactDetails][nControlID] = sControlName;
				break;

			case "add_director":
			case "director_birth_date_day":
			case "director_birth_date_month":
			case "director_birth_date_year":
			case "director_email":
			case "director_first_name":
			case "director_gender":
			case "director_last_name":
			case "director_middle_name":
			case "director_phone":
			case "remove_director":
				m_oControlGroups[UiItemGroups.AdditionalDirectors][nControlID] = sControlName;
				break;

			case "type_of_business":
			case "online_turnover":
			case "overall_turnover":
			case "company_continue":

			case "employee_count":
			case "employee_count_change":
			case "top_earning_employee_count":
			case "bottom_earning_employee_count":
			case "total_monthly_salary":
				m_oControlGroups[UiItemGroups.CompanyInfo][nControlID] = sControlName;
				break;

			case "limited_company_name":
			case "limited_company_number":
			case "limited_phone_number":
			case "limited_property_owned_by_company":
			case "limited_rent_months_left)":
			case "limited_years_in_company":

			case "nonlimited_company_name":
			case "nonlimited_phone_number":
			case "nonlimited_property_owned_by_company":
			case "nonlimited_rent_months_left)":
			case "nonlimited_time_at_address":
			case "nonlimited_time_in_business":
			case "nonlimited_years_in_company":
				m_oControlGroups[UiItemGroups.CompanyDetails][nControlID] = sControlName;
				break;
			} // switch
		} // AddControlToItemGroup

		private ProgressCounter m_oProgress;

		private readonly SortedDictionary<int, string> m_oUiActions;
		private readonly SortedDictionary<int, AddressInfo> m_oAddresses;
		private readonly SortedDictionary<int, int> m_oDirectors;
		private readonly SortedDictionary<int, int> m_oAccounts;

		private readonly SortedDictionary<int, UiReportItem> m_oResult;

		private readonly AConnection m_oDB;
		private readonly DateTime m_oDateStart;
		private readonly DateTime m_oDateEnd;

		private Action<DbDataReader> m_oCurHandler;

		private readonly SortedDictionary<UiItemGroups, SortedDictionary<int, string>> m_oControlGroups;

	} // class UiReport

} // namespace Reports
