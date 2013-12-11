using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Reports {
	#region class UiReportItem

	public class UiReportItem {
		#region static constructor

		static UiReportItem() {
			ms_oControlGroups = new SortedDictionary<ItemGroups, SortedSet<int>>();

			foreach (ItemGroups nItemType in (ItemGroups [])Enum.GetValues(typeof (ItemGroups)))
				ms_oControlGroups[nItemType] = new SortedSet<int>();
		} // static constructor

		#endregion static constructor

		#region public

		#region method AddControlToItemGroup

		public static void AddControlToItemGroup(int nControlID, string sControlName) {
			// TODO
		} // AddControlToItemGroup

		#endregion method AddControlToItemGroup

		#region constructor

		public UiReportItem(CustomerInfo oInfo) {
			CustomerInfo = oInfo;
			m_oData = new SortedDictionary<ItemGroups, UiReportItemGroupData>();

			foreach (ItemGroups nItemType in (ItemGroups [])Enum.GetValues(typeof (ItemGroups)))
				m_oData[nItemType] = new UiReportItemGroupData(ms_oControlGroups[nItemType]);
		} // constructor

		#endregion constructor

		#region properties

		public CustomerInfo CustomerInfo { get; private set; }

		public UiReportItemGroupData PersonalInfo { get { return m_oData[ItemGroups.PersonalInfo]; } }
		public UiReportItemGroupData HomeAddress { get { return m_oData[ItemGroups.HomeAddress]; } }
		public UiReportItemGroupData ContactDetails { get { return m_oData[ItemGroups.ContactDetails]; } }
		public UiReportItemGroupData CompanyInformation { get { return m_oData[ItemGroups.CompanyInformation]; } }
		public UiReportItemGroupData CompanyDetails { get { return m_oData[ItemGroups.CompanyDetails]; } }
		public UiReportItemGroupData AdditionalDirectors { get { return m_oData[ItemGroups.AdditionalDirectors]; } }

		#endregion properties

		#region method Generate

		public void Generate() {
			foreach (ItemGroups nItemType in (ItemGroups [])Enum.GetValues(typeof (ItemGroups)))
				m_oData[nItemType].Generate();	
		} // Generate

		#endregion method Generate

		#region method AddEvent

		public void AddEvent(UiEvent oEvent) {
			foreach (ItemGroups nItemType in (ItemGroups [])Enum.GetValues(typeof (ItemGroups)))
				m_oData[nItemType].AddEvent(oEvent);	
		} // AddEvent

		#endregion method AddEvent

		#endregion public

		#region private

		#region enum ItemGroups

		private enum ItemGroups {
			PersonalInfo,
			HomeAddress,
			ContactDetails,
			CompanyInformation,
			CompanyDetails,
			AdditionalDirectors,
		} // enum ItemGroups

		#endregion enum ItemGroups

		private static readonly SortedDictionary<ItemGroups, SortedSet<int>> ms_oControlGroups; 

		private readonly SortedDictionary<ItemGroups, UiReportItemGroupData> m_oData; 

		#endregion private
	} // class UiReportItem

	#endregion class UiReportItem
} // namespace Reports
