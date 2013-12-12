using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Reports {
	#region class UiReportItem

	public class UiReportItem {
		#region public

		#region method CreateTable

		public static DataTable CreateTable() {
			var oOutput = new DataTable();

			oOutput.Columns.Add("UserID", typeof(int));
			oOutput.Columns.Add("FirstName", typeof(string));
			oOutput.Columns.Add("LastName", typeof(string));

			foreach (UiItemGroups nItemType in UiItemGroupsSequence.Get())
				oOutput.Columns.Add(nItemType.ToString(), typeof(string));

			return oOutput;
		} // CreateTable
		#endregion method CreateTable

		#region constructor

		public UiReportItem(CustomerInfo oInfo, SortedDictionary<UiItemGroups, SortedDictionary<int, string>> oRelevantControlGroups) {
			CustomerInfo = oInfo;
			m_oData = new SortedDictionary<UiItemGroups, UiReportItemGroupData>();

			foreach (UiItemGroups nItemType in UiItemGroupsSequence.Get())
				m_oData[nItemType] = new UiReportItemGroupData(CustomerInfo, nItemType, oRelevantControlGroups[nItemType]);
		} // constructor

		#endregion constructor

		#region properties

		public CustomerInfo CustomerInfo { get; private set; }

		public UiReportItemGroupData PersonalInfo { get { return m_oData[UiItemGroups.PersonalInfo]; } }
		public UiReportItemGroupData HomeAddress { get { return m_oData[UiItemGroups.HomeAddress]; } }
		public UiReportItemGroupData ContactDetails { get { return m_oData[UiItemGroups.ContactDetails]; } }
		public UiReportItemGroupData CompanyInformation { get { return m_oData[UiItemGroups.CompanyInfo]; } }
		public UiReportItemGroupData CompanyDetails { get { return m_oData[UiItemGroups.CompanyDetails]; } }
		public UiReportItemGroupData AdditionalDirectors { get { return m_oData[UiItemGroups.AdditionalDirectors]; } }

		#endregion properties

		#region method Generate

		public void Generate() {
			foreach (UiItemGroups nItemType in UiItemGroupsSequence.Get())
				m_oData[nItemType].Generate();	
		} // Generate

		#endregion method Generate

		#region method AddEvent

		public void AddEvent(UiEvent oEvent) {
			foreach (UiItemGroups nItemType in UiItemGroupsSequence.Get())
				m_oData[nItemType].AddEvent(oEvent);	
		} // AddEvent

		#endregion method AddEvent

		#region method ToRow

		public void ToRow(DataTable tbl) {
			var oRow = new List<object>(new object[] { CustomerInfo.ID, CustomerInfo.FirstName, CustomerInfo.Surname });

			foreach (UiItemGroups nItemType in UiItemGroupsSequence.Get())
				m_oData[nItemType].ToRow(oRow);

			tbl.Rows.Add(oRow.ToArray());
		} // ToRow

		#endregion method ToRow

		#region method ToString

		public override string ToString() {
			var os = new StringBuilder();

			foreach (UiItemGroups nItemType in UiItemGroupsSequence.Get())
				os.AppendFormat("{0} ", m_oData[nItemType].ToString());

			os.AppendFormat("{0}", CustomerInfo);

			return os.ToString();
		} // ToString

		#endregion method ToString

		#endregion public

		#region private

		private readonly SortedDictionary<UiItemGroups, UiReportItemGroupData> m_oData; 

		#endregion private
	} // class UiReportItem

	#endregion class UiReportItem
} // namespace Reports
