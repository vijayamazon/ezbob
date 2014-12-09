using System;
using System.Collections.Generic;
using System.Data;

namespace Reports {

	public class UiReportItemGroupData {

		public UiReportItemGroupData(CustomerInfo oCustomerInfo, UiItemGroups nItemGroup, SortedDictionary<int, string> oRelevantControls) {
			m_oCustomerInfo = oCustomerInfo;
			m_nItemGroup = nItemGroup;
			m_oRelevantControls = oRelevantControls;
			m_nCount = 0;
		} // constructor

		public UiReportItemGroupAction Action { get; set; }
		public DateTime? StartTime { get; set; }
		public DateTime? EndTime { get; set; }

		public void Generate() {
			if (m_oCustomerInfo.HasAllData(m_nItemGroup))
				Action = UiReportItemGroupAction.Completed;
			else if (m_nCount > 0)
				Action = UiReportItemGroupAction.Started;
			else
				Action = UiReportItemGroupAction.NeverStarted;
		} // Generate

		public void AddEvent(UiEvent oEvent) {
			if (!m_oRelevantControls.ContainsKey(oEvent.ControlID))
				return;

			m_nCount++;

			StartTime = (!StartTime.HasValue || (oEvent.EventTime < StartTime.Value)) ? oEvent.EventTime : StartTime;
			EndTime = (!EndTime.HasValue || (oEvent.EventTime > EndTime.Value)) ? oEvent.EventTime : EndTime;
		} // AddEvent

		public void ToRow(List<object> oRow) {
			string sPrefix = string.Empty;

			switch (Action) {
			case UiReportItemGroupAction.NeverStarted:
				oRow.Add(string.Empty);
				return;

			case UiReportItemGroupAction.Started:
				sPrefix = "Spent";
				break;

			case UiReportItemGroupAction.Completed:
				sPrefix = "F";
				break;

			default:
				throw new ArgumentOutOfRangeException();
			} // switch

			string sTime = string.Empty;

			if (StartTime.HasValue && EndTime.HasValue) {
				TimeSpan oDuration = EndTime.Value - StartTime.Value;

				if (oDuration.TotalSeconds < 60)
					sTime = oDuration.Seconds + "s";
				else if (oDuration.TotalMinutes < 60)
					sTime = oDuration.Minutes + ":" + oDuration.Seconds.ToString("00");
				else
					sTime = (oDuration.Days > 0 ? oDuration.Days + "d " : "") + oDuration.Hours + ":" + oDuration.Minutes.ToString("00") + ":" + oDuration.Seconds.ToString("00");
			} // if

			oRow.Add(sPrefix + " " + sTime);
		} // ToRow

		public override string ToString() {
			return string.Format("{0} {1} {2} {3}", m_nItemGroup, Action, StartTime, EndTime);
		} // ToString

		private readonly SortedDictionary<int, string> m_oRelevantControls;
		private int m_nCount;
		private readonly CustomerInfo m_oCustomerInfo;
		private readonly UiItemGroups m_nItemGroup;

	} // class UiReportItemGroupData

} // namespace Reports
