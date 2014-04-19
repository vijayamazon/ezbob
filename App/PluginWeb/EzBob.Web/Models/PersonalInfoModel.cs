namespace EzBob.Web.Areas.Underwriter.Models {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Utils;
	using Infrastructure;
	using NHibernate;
	using CommonLib;
	using StructureMap;
	using Code;

	public class PersonalInfoModel {

		private readonly ServiceClient serviceClient;

		public int Id { get; set; }
		public string Name { get; set; }
		public string Email { get; set; }
		public string EmailState { get; set; }
		public string MobilePhone { get; set; }
		public string DaytimePhone { get; set; }
		public string RegistrationDate { get; set; }
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
		public string IsTestHighlightCss { get; set; }
		public string AmlResult { get; set; }
		public int Age { get; set; }
		public string Gender { get; set; }
		public string FamilyStatus { get; set; }
		public string ResidentalStatus { get; set; }
		public string CompanyType { get; set; }
		public string CompanySeniority { get; set; }
		public string NumOfDirectorsAndShareholders { get; set; }
		public string Website { get; set; }
		public bool IsWarning { get; set; }
		public string PromoCode { get; set; }
		public string PromoCodeCss { get; set; }
		public CompanyEmployeeCountInfo CompanyEmployeeCountInfo { get; set; }
		public string ActiveCampaign { get; set; }

		public PersonalInfoModel() {
			IndustryFields = new List<string>();
			StrategyError = "";
			CompanyEmployeeCountInfo = null;
			serviceClient = new ServiceClient();
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

			Website = "www." + customer.Name.Substring(customer.Name.IndexOf('@') + 1);

			if (customer.PersonalInfo != null)
			{
				if (customer.PersonalInfo.DateOfBirth.HasValue)
				{
					Age = MiscUtils.GetFullYears(customer.PersonalInfo.DateOfBirth.Value);

					Gender = customer.PersonalInfo.Gender.ToString();
					FamilyStatus = customer.PersonalInfo.MaritalStatus.ToString();
					ResidentalStatus = customer.PersonalInfo.ResidentialStatus;
				}
			}

			if (customer.Company != null)
			{
				CompanyType = customer.Company.TypeOfBusiness.ToString();
			}

			NumOfDirectorsAndShareholders = "2/5"; // where is the shareholder data?

			var context = ObjectFactory.GetInstance<IWorkplaceContext>();
			DateTime companySeniority = serviceClient.Instance.GetCompanySeniority(customer.Id, context.UserId).Value;
			int companySeniorityYears, companySeniorityMonths;
			MiscUtils.GetFullYearsAndMonths(companySeniority, out companySeniorityYears, out companySeniorityMonths);
			
			CompanySeniority = string.Format("{0}Y{1}M", companySeniorityYears, companySeniorityMonths);

			if (customer.FraudStatus != FraudStatus.Ok)
			{
				FraudHighlightCss = "red_cell";
			}

			if (IsTest)
			{
				IsTestHighlightCss = "red_cell";
			}

			AmlResult = customer.AMLResult;

			PromoCode = customer.PromoCode;
			if (!string.IsNullOrEmpty(PromoCode))
				PromoCodeCss = "promo_code";

			if (customer.PersonalInfo != null) {
				Name = customer.PersonalInfo.Fullname;
				MobilePhone = customer.PersonalInfo.MobilePhone;
				DaytimePhone = customer.PersonalInfo.DaytimePhone;
			} // if

			Email = customer.Name;
			EmailState = customer.EmailState.ToString();

			if (customer.GreetingMailSentDate != null)
			{
				DateTime registrationDate = customer.GreetingMailSentDate.Value;
				int registrationTimeYears, registrationTimeMonths;
				MiscUtils.GetFullYearsAndMonths(registrationDate, out registrationTimeYears, out registrationTimeMonths);
				
				RegistrationDate = customer.GreetingMailSentDate.Value.ToString("MMM dd, yyyy") +
				                   string.Format(" [{0}Y{1}M]", registrationTimeYears, registrationTimeMonths);
			}

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

			string lastMainStrategyStatus = (string)session.CreateSQLQuery("EXEC GetLastMainStrategyStatus " + customer.Id).UniqueResult();

			IsMainStratFinished = lastMainStrategyStatus != "BG launch" && lastMainStrategyStatus != "In progress";
			if (lastMainStrategyStatus == "Finished" || lastMainStrategyStatus == "Failed" ||
			    lastMainStrategyStatus == "Terminated")
			{
				StrategyError = string.Format("Error occured in main strategy, its status is:{0}", lastMainStrategyStatus);
			}

			BrokerID = customer.Broker == null ? 0 : customer.Broker.ID;
			BrokerName = customer.Broker == null ? "" : customer.Broker.FirmName;
		} // InitFromCustomer

		public bool IsTest { get; set; }
		public bool IsAvoid { get; set; }
		public string SegmentType { get; set; }
		public bool CciMark { get; set; }

		public string TrustPilotStatusDescription { get; set; }
		public string TrustPilotStatusName { get; set; }

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
