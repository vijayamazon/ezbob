namespace EzBob.Web.Models {
	using System;
	using System.Collections.Generic;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.ModelsWithDB.CompaniesHouse;
	using Ezbob.Backend.ModelsWithDB.CompanyScore;

	public class CompanyHistory {
		public long ServiceLogId { get; set; }
		public int? Score { get; set; }
		public decimal? Balance { get; set; }
		public DateTime Date { get; set; }
	} // class NonLimScoreHistory

	public class CompanyDetails {
		public int CustomerId { get; set; }
		public string TypeOfBusiness { get; set; }
		public string CompanyRefNum { get; set; }
		public string CompanyName { get; set; }
		public List<CustomerAddress> CompanyAddress { get; set; }
		public string CustomerFirstName { get; set; }
		public string CustomerSurname { get; set; }
	}

	public class FinDataModel {
		public decimal TangibleEquity { get; set; }
		public decimal AdjustedProfit { get; set; }
	} // class FinDataModel

	public class CompanyScoreModel {
		public CompanyDetails CompanyDetails { get;set; }
		public CompanyData Data { get; set; }

		public const string Ok = "ok";

		public string result { get; set; }

		public SortedDictionary<string, CompanyScoreModelItem> dataset { get; set; }

		public List<string> dataset_display_order { get; set; }

		public string company_name { get; set; }

		public string company_ref_num { get; set; }

		public ComapanyDashboardModel DashboardModel { get; set; }

		public CompanyScoreModel[] Owners { get { return ReferenceEquals(m_oOwners, null) ? null : m_oOwners.ToArray(); } }
		public CompaniesHouseOfficerOrder CompaniesHouseModel { get; set; }

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
		public int CcjValue { get; set; }
		public int DefaultAccounts { get; set; }
		public decimal DefaultAmount { get; set; }
		public int LateAccounts { get; set; }
		public string LateStatus { get; set; }
		public List<FinDataModel> FinDataHistories { get; set; }
		public List<CompanyHistory> CompanyHistories { get; set; }
		public string Error { get; set; }
		public DateTime? OriginationDate { get; set; }
	} // class ComapanyDashboardModel

} // namespace
