namespace EzBob.Web.Models {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.Models;

	#region class NonLimScoreHistory

	public class NonLimScoreHistory {
		public int Score { get; set; }
		public DateTime ScoreDate { get; set; }
	} // class NonLimScoreHistory

	#endregion class NonLimScoreHistory

	#region class FinDataModel

	public class FinDataModel {
		public decimal TangibleEquity { get; set; }
		public decimal AdjustedProfit { get; set; }
	} // class FinDataModel

	#endregion class FinDataModel

	#region class CompanyScoreModelItemValues

	public class CompanyScoreModelItemValues {
		#region constructor

		public CompanyScoreModelItemValues() {
			Values = new SortedDictionary<string, string>();
			Children = new Dictionary<string, CompanyScoreModelItem>();
		} // constructor

		#endregion constructor

		public SortedDictionary<string, string> Values { get; private set; }
		public Dictionary<string, CompanyScoreModelItem> Children { get; private set; }
	} // class CompanyScoreModelItemValues

	#endregion class ParsedDataItem

	#region class CompanyScoreModelItem

	public class CompanyScoreModelItem {
		public CompanyScoreModelItem(string sGroupName, SortedDictionary<string, string> oMetaData) {
			GroupName = sGroupName;
			MetaData = oMetaData ?? new SortedDictionary<string, string>();
			Data = new List<CompanyScoreModelItemValues>();
		} // constructor

		public string GroupName { get; private set; }

		public SortedDictionary<string, string> MetaData { get; private set; }

		public List<CompanyScoreModelItemValues> Data { get; private set; }
	} // CompanyScoreModelItem

	#endregion class CompanyScoreModelItem

	#region class CompanyScoreModel

	public class CompanyScoreModel {
		public CompanyData Data { get; set; }

		public const string Ok = "ok";

		public string result { get; set; }

		public Dictionary<string, CompanyScoreModelItem> dataset { get; set; }

		public Dictionary<string, CompanyScoreModelItem> newDataset { get; set; }

		public string company_name { get; set; }

		public string company_ref_num { get; set; }

		public ComapanyDashboardModel DashboardModel { get; set; }

		public CompanyScoreModel[] Owners { get { return ReferenceEquals(m_oOwners, null) ? null : m_oOwners.ToArray(); } }

		public void AddOwner(CompanyScoreModel oOwner) {
			if (ReferenceEquals(m_oSavedOwners, null)) {
				m_oSavedOwners = new SortedSet<string>();
				m_oOwners = new List<CompanyScoreModel>();
			} // if

			if (m_oSavedOwners.Contains(oOwner.company_ref_num))
				return;

			m_oSavedOwners.Add(oOwner.company_ref_num);
			m_oOwners.Add(oOwner);
		} // AddOwner

		private SortedSet<string> m_oSavedOwners;
		private List<CompanyScoreModel> m_oOwners;
	} // class CompanyScoreModel

	#endregion class CompanyScoreModel

	#region class ComapanyDashboardModel

	public class ComapanyDashboardModel {
		public bool IsLimited { get; set; }
		public string CompanyName { get; set; }
		public string CompanyRefNum { get; set; }
		public int Score { get; set; }
		public string ScoreColor { get; set; }
		public decimal CaisBalance { get; set; }
		public int CaisAccounts { get; set; }
		public FinDataModel LastFinData { get; set; }
		public int Ccjs { get; set; }
		public int CcjMonths { get; set; }
		public int DefaultAccounts { get; set; }
		public decimal DefaultAmount { get; set; }
		public int LateAccounts { get; set; }
		public string LateStatus { get; set; }
		public List<FinDataModel> FinDataHistories { get; set; }
		public List<NonLimScoreHistory> NonLimScoreHistories { get; set; }
		public string Error { get; set; }
	} // class ComapanyDashboardModel

	#endregion class ComapanyDashboardModel
} // namespace