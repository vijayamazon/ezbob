namespace EzBob.Web.Areas.Underwriter.Models {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ApplicationMng.Model;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using NHibernate;
	using CommonLib;
	using StructureMap;

	public class PersonalInfoModel {
		public int Id { get; set; }
		public string RefNumber { get; set; }
		public string Name { get; set; }
		public string Medal { get; set; }
		public string Email { get; set; }
		public string EmailState { get; set; }
		public string MobilePhone { get; set; }
		public string DaytimePhone { get; set; }
		public DateTime RegistrationDate { get; set; }
		public List<string> IndustryFields { get; set; }
		public string UserStatus { get; set; }
		public string CreditResult { get; set; }
		public double CreditScore { get; set; }
		public int Disabled { get; set; }
		public bool Editable { get; set; }
		public List<string> TopCategories { get; set; }
		public decimal? WebSiteTurnOver { get; set; }
		public decimal? OverallTurnOver { get; set; }
		public string ReferenceSource { get; set; }
		public string ABTesting { get; set; }
		public bool IsMainStratFinished { get; set; }
		public string StrategyError { get; set; }
		public string FraudCheckStatus { get; set; }
		public int FraudCheckStatusId { get; set; }
		public string FraudHighlightCss { get; set; }
		public bool IsWarning { get; set; }
		public string PromoCode { get; set; }
		public string PromoCodeCss { get; set; }
		public CompanyEmployeeCountInfo CompanyEmployeeCountInfo { get; set; }
		public string ActiveCampaign { get; set; }
		public PersonalInfoModel() {
			IndustryFields = new List<string>();
			StrategyError = "";
			CompanyEmployeeCountInfo = null;
		} // constructor

		public void InitFromCustomer(Customer customer, ISession session) {
			if (customer == null)
				return;

			Id = customer.Id;
			IsTest = customer.IsTest;
			SegmentType = customer.SegmentType();
			IsAvoid = customer.IsAvoid;

			FraudCheckStatus = customer.FraudStatus.Description();
			FraudCheckStatusId = (int)customer.FraudStatus;
			if (customer.FraudStatus == FraudStatus.FraudSuspect)
				FraudHighlightCss = "red_cell";

			PromoCode = customer.PromoCode;
			if (!string.IsNullOrEmpty(PromoCode))
				PromoCodeCss = "promo_code";

			if (customer.PersonalInfo != null) {
				Name = customer.PersonalInfo.Fullname;
				MobilePhone = customer.PersonalInfo.MobilePhone;
				DaytimePhone = customer.PersonalInfo.DaytimePhone;
			} // if

			Medal = customer.Medal.HasValue ? customer.Medal.ToString() : "";
			Email = customer.Name;
			EmailState = customer.EmailState.ToString();
			RefNumber = customer.RefNumber;

			if (customer.GreetingMailSentDate != null)
				RegistrationDate = customer.GreetingMailSentDate.Value;

			IndustryFields.Add(string.Empty);
			UserStatus = customer.Status.ToString();
			CreditResult = customer.CreditResult.ToString();
			CreditScore = customer.ScoringResults.Any() ? customer.ScoringResults.Last().ScoreResult : 0.00;
			Disabled = customer.CollectionStatus.CurrentStatus.Id;
			Editable = customer.CreditResult == CreditResultStatus.WaitingForDecision && customer.CollectionStatus.CurrentStatus.IsEnabled;
			IsWarning = customer.CollectionStatus.CurrentStatus.IsWarning;

			if (customer.PersonalInfo != null) {
				OverallTurnOver = customer.PersonalInfo.OverallTurnOver;
				WebSiteTurnOver = customer.PersonalInfo.WebSiteTurnOver;
			} // if

			ReferenceSource = customer.ReferenceSource;
			ABTesting = customer.ABTesting;
			var app = customer.LastStartedMainStrategy;

			IsMainStratFinished = app != null && (
				app.State == ApplicationStrategyState.SecurityViolation ||
				app.State == ApplicationStrategyState.StrategyFinishedWithoutErrors ||
				app.State == ApplicationStrategyState.StrategyFinishedWithErrors ||
				app.State == ApplicationStrategyState.Error
			) || app == null;

			if (app != null) {
				var sql = string.Format("SELECT top 1 ErrorMsg FROM Application_Application where ApplicationId = {0}", app.Id);
				StrategyError = session.CreateSQLQuery(sql).UniqueResult<string>();
			} // if

			CompanyEmployeeCountInfo = new CompanyEmployeeCountInfo(customer.Company);

			ActiveCampaign = "";
			var activeCampaigns = customer.ActiveCampaigns
				.Where(cc => 
					cc.Campaign.EndDate >= DateTime.Today && 
					cc.Campaign.StartDate <= DateTime.Today
				)
				.Select(cc => cc.Campaign.Name)
				.ToList();

			if (activeCampaigns.Any())
				ActiveCampaign = activeCampaigns.Aggregate((i, j) => i + ", " + j);

			CciMark = customer.CciMark;

			TrustPilotStatusDescription = customer.TrustPilotStatus.Description;
			TrustPilotStatusName = customer.TrustPilotStatus.Name;

			LastMainStrategyStatus = (string)session.CreateSQLQuery("EXEC GetLastMainStrategyStatus " + customer.Id).UniqueResult();

			BrokerID = customer.Broker == null ? 0 : customer.Broker.ID;
			BrokerName = customer.Broker == null ? "" : customer.Broker.FirmName;
		} // InitFromCustomer

		public bool IsTest { get; set; }
		public bool IsAvoid { get; set; }
		public string SegmentType { get; set; }
		public bool CciMark { get; set; }

		public string TrustPilotStatusDescription { get; set; }
		public string TrustPilotStatusName { get; set; }
		public string LastMainStrategyStatus { get; set; }

		public int BrokerID { get; set; }
		public string BrokerName { get; set; }

		public List<object> TrustPilotStatusList {
			get {
				if (m_oTrustPilotStatusList != null)
					return m_oTrustPilotStatusList;

				m_oTrustPilotStatusList = new List<object>();

				var oHelper = ObjectFactory.GetInstance<DatabaseDataHelper>();

				foreach (TrustPilotStatus tsp in oHelper.TrustPilotStatusRepository.GetAll())
					m_oTrustPilotStatusList.Add(new { value = tsp.Name, text = tsp.Description });

				return m_oTrustPilotStatusList;
			} // get
		} // TrustPilotSatusList

		private List<object> m_oTrustPilotStatusList;
	} // class PersonalInfoModel
} // namespace
