namespace UiEventAddressFixer {
	using System;
	using System.Collections.Generic;
	using System.Data.Common;
	using System.Linq;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;

	class Program {
		static void Main(string[] args) {
			new Program().Run();
		} // Main

		private Program() {
			m_oLog = new ConsoleLog(new FileLog("UiEventAddressFixer"));

			var env = new Ezbob.Context.Environment(m_oLog);
			m_oDB = new SqlConnection(env, m_oLog);

			m_oEvents = new SortedTable<int, long, EventInfo>();
			m_oLastKnown = new SortedSet<string>();
		} // constructor

		private void Run() {
			m_oEventsProgress = new ProgressCounter("{0} user events processed.", m_oLog, 100);
			m_oDB.ForEachRow(
				SaveEvent,
				Query(true),
				CommandSpecies.Text
			);
			m_oEventsProgress.Log();

			m_oAddressProgress = new ProgressCounter("{0} address events processed.", m_oLog, 100);
			m_oDB.ForEachRow(
				ProcessAddress,
				Query(false),
				CommandSpecies.Text
			);
			m_oAddressProgress.Log();

			m_oLog.Info("Last known events - begin:");
			foreach (string s in m_oLastKnown)
				m_oLog.Info(s);
			m_oLog.Info("Last known events - end");
		} // Run

		private ActionResult ProcessAddress(DbDataReader oRow, bool bStartOfSet) {
			var ei = new EventInfo(oRow);

			if (!m_oEvents.Contains(ei.UserID)) {
				// m_oLog.Info("Previous events not found for user {0}", ei.UserID);
			}
			else {
				SortedDictionary<long, EventInfo> lst = m_oEvents[ei.UserID];

				EventInfo oLastKnown = lst.Values.LastOrDefault(x => x.EventID < ei.EventID);

				if (oLastKnown == null) {
					// m_oLog.Info("User {0}, event {1} - not found", ei.UserID, ei.EventID);
				}
				else {
					m_oLog.Info("User {0}, event {1}, last known {2}", ei.UserID, ei.EventID, oLastKnown);

					string sAddressType = DetectAddressType(oLastKnown);

					if (sAddressType == string.Empty)
						m_oLastKnown.Add(oLastKnown.ControlName + " " + oLastKnown.Args);
					else {
						string sQuery = string.Format("UPDATE UiEvents SET ControlHtmlID = '{0}' WHERE UiEventID = {1}", sAddressType, ei.EventID);
						m_oDB.ExecuteNonQuery(sQuery, CommandSpecies.Text);
					} // if
				} // if
			} // if

			m_oAddressProgress++;
			return ActionResult.Continue;
		} // ProcessAddress

		private string DetectAddressType(EventInfo ei) {
			if (ei.ControlName.StartsWith("link-account:"))
				return "personal";

			if (ei.ControlName == "personal-info:type_of_business") {
				switch (ei.Args) {
				case "Limited":
				case "LLP":
					return "limited";
				case "SoleTrader":
				case "PShip3P":
				case "PShip":
					return "nonlimited";
				default:
					return "company";
				} // switch
			} // if

			switch (ei.ControlName) {
			case "personal-info:add_director":
			case "personal-info:director_birth_date_year":
			case "personal-info:director_email":
			case "personal-info:director_phone":
			case "personal-info:remove_director":
				return "director";

			case "wizard:complete":
			case "personal-info:birth_date_day":
			case "personal-info:birth_date_year":
			case "personal-info:consent_to_search":
			case "personal-info:continue":
			case "personal-info:daytime_phone":
			case "personal-info:first_name":
			case "personal-info:gender":
			case "personal-info:last_name":
			case "personal-info:marital_status":
			case "personal-info:middle_name":
			case "personal-info:mobile_phone":
			case "personal-info:residential_status":
			case "personal-info:time_at_address":
				return "personal";

			case "personal-info:own_other_property":
				return "other-property";

			case "personal-info:company_continue":
			case "personal-info:capital_expenditure":
			case "personal-info:employee_count":
			case "personal-info:online_turnover":
			case "personal-info:overall_turnover":
			case "personal-info:top_earning_employee_count":
			case "personal-info:total_monthly_salary":
				return "company";

			case "personal-info:limited_company_name":
			case "personal-info:limited_company_number":
			case "personal-info:limited_phone_number":
				return "limited";

			case "personal-info:nonlimited_company_name":
			case "personal-info:nonlimited_phone_number":
			case "personal-info:nonlimited_property_owned_by_company":
			case "personal-info:nonlimited_time_at_address":
			case "personal-info:nonlimited_time_in_business":
				return "nonlimited";
			} // switch

			return string.Empty;
		} // DetectAddressType

		private ActionResult SaveEvent(DbDataReader oRow, bool bStartOfSet) {
			var ei = new EventInfo(oRow);

			m_oEvents[ei.UserID, ei.EventID] = ei;

			m_oEventsProgress++;

			return ActionResult.Continue;
		} // SaveEvent

		private string Query(bool bReference) {
			return @"SELECT
	e.UiEventID,
	e.UserID,
	e.UiControlID,
	c.UiControlName,
	e.EventArguments
FROM
	UiEvents e
	INNER JOIN Customer u ON e.UserID = u.Id AND u.IsTest = 0
	INNER JOIN UiControls c ON e.UiControlID = c.UiControlID
WHERE
	c.UiControlName" + (bReference ? " NOT" : "") + @" LIKE 'address%'
ORDER BY
	e.UserID,
	e.UiEventID";
		} // Query

		private readonly ASafeLog m_oLog;
		private readonly AConnection m_oDB;

		private readonly SortedTable<int, long, EventInfo> m_oEvents;
		private ProgressCounter m_oEventsProgress;
		private ProgressCounter m_oAddressProgress;
		private readonly SortedSet<string> m_oLastKnown;
	} // class Program
} // namespace
