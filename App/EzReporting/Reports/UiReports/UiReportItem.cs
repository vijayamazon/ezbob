namespace Reports.UiReports {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Text;

	public class UiReportItem : IComparable<UiReportItem> {
		public static DataTable CreateTable() {
			var oOutput = new DataTable();

			oOutput.Columns.Add("Origin", typeof(string));
			oOutput.Columns.Add("UserID", typeof(int));
			oOutput.Columns.Add("FirstName", typeof(string));
			oOutput.Columns.Add("LastName", typeof(string));
			oOutput.Columns.Add("WizardStepName", typeof(string));
			oOutput.Columns.Add("TypeOfBusiness", typeof(string));
			oOutput.Columns.Add("Offline", typeof(string));

			foreach (UiItemGroups nItemType in UiItemGroupsSequence.Get())
				oOutput.Columns.Add(nItemType.ToString(), typeof(string));

			return oOutput;
		} // CreateTable

		public UiReportItem(CustomerInfo oInfo, SortedDictionary<UiItemGroups, SortedDictionary<int, string>> oRelevantControlGroups) {
			CustomerInfo = oInfo;
			m_oData = new SortedDictionary<UiItemGroups, UiReportItemGroupData>();

			foreach (UiItemGroups nItemType in UiItemGroupsSequence.Get())
				m_oData[nItemType] = new UiReportItemGroupData(CustomerInfo, nItemType, oRelevantControlGroups[nItemType]);
		} // constructor

		public CustomerInfo CustomerInfo { get; private set; }

		public UiReportItemGroupData PersonalInfo { get { return m_oData[UiItemGroups.PersonalInfo]; } }
		public UiReportItemGroupData HomeAddress { get { return m_oData[UiItemGroups.HomeAddress]; } }
		public UiReportItemGroupData ContactDetails { get { return m_oData[UiItemGroups.ContactDetails]; } }
		public UiReportItemGroupData CompanyInformation { get { return m_oData[UiItemGroups.CompanyInfo]; } }
		public UiReportItemGroupData CompanyDetails { get { return m_oData[UiItemGroups.CompanyDetails]; } }
		public UiReportItemGroupData AdditionalDirectors { get { return m_oData[UiItemGroups.AdditionalDirectors]; } }
		public UiReportItemGroupData LinkAccounts { get { return m_oData[UiItemGroups.LinkAccounts]; } }

		public void Generate() {
			foreach (UiItemGroups nItemType in UiItemGroupsSequence.Get())
				m_oData[nItemType].Generate();	
		} // Generate

		public void AddEvent(UiEvent oEvent) {
			foreach (UiItemGroups nItemType in UiItemGroupsSequence.Get())
				m_oData[nItemType].AddEvent(oEvent);	
		} // AddEvent

		public void ToRow(DataTable tbl) {
			var oRow = new List<object> {
				CustomerInfo.Origin,
				CustomerInfo.ID, CustomerInfo.FirstName, CustomerInfo.Surname,
				CustomerInfo.WizardStepName, CustomerInfo.TypeOfBusiness,
				CustomerInfo.IsOffline ? "offline" : "online"
			};

			foreach (UiItemGroups nItemType in UiItemGroupsSequence.Get())
				m_oData[nItemType].ToRow(oRow);

			tbl.Rows.Add(oRow.ToArray());
		} // ToRow

		public override string ToString() {
			var os = new StringBuilder();

			foreach (UiItemGroups nItemType in UiItemGroupsSequence.Get())
				os.AppendFormat("{0} ", m_oData[nItemType].ToString());

			os.AppendFormat("{0}", CustomerInfo);

			return os.ToString();
		} // ToString

		public int CompareTo(UiReportItem y) {
			if (ReferenceEquals(y, null))
				return 1;

			if (ReferenceEquals(this, y))
				return 0;

			if (this.CustomerInfo.WizardStepIsLast == y.CustomerInfo.WizardStepIsLast)
				return this.CustomerInfo.WizardStepName.CompareTo(y.CustomerInfo.WizardStepName);

			return this.CustomerInfo.WizardStepIsLast ? -1 : 1;
		} // Compare

		private readonly SortedDictionary<UiItemGroups, UiReportItemGroupData> m_oData; 
	} // class UiReportItem
} // namespace
