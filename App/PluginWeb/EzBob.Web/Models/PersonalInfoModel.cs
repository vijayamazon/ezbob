namespace EzBob.Web.Areas.Underwriter.Models 
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using EzBob.Models;
	using Ezbob.Backend.Models;
	using Ezbob.Utils;
	using Infrastructure;
	using Infrastructure.Email;
	using NHibernate;
	using CommonLib;
	using Ezbob.Database;
	using Ezbob.Logger;
	using ServiceClientProxy;
	using StructureMap;
	using log4net;

	public class PersonalInfoModel {
		private static readonly ILog Log = LogManager.GetLogger(typeof(PersonalInfoModel));
		private readonly ServiceClient serviceClient;
		private readonly CustomerPhoneRepository customerPhoneRepository = ObjectFactory.GetInstance <CustomerPhoneRepository>();

		public int Id { get; set; }
		public string Name { get; set; }
		public string FirstName { get; set; }
		public string Surname { get; set; }
		public string Email { get; set; }
		public string EmailState { get; set; }
		public string Medal { get; set; }
		public string MobilePhone { get; set; }
		public string DaytimePhone { get; set; }
		public bool MobilePhoneVerified { get; set; }
		public bool DaytimePhoneVerified { get; set; }
		public string MobileTooltip { get; set; }
		public string DaytimeTooltip { get; set; }
		public string RegistrationDate { get; set; }
		public List<string> IndustryFields { get; set; }
		public string UserStatus { get; set; }
		public string CreditResult { get; set; }
		public int CustomerStatusId { get; set; }
		public string CustomerStatusName { get; set; }
		public List<string> TopCategories { get; set; }
		public decimal? WebSiteTurnOver { get; set; }
		public decimal? OverallTurnOver { get; set; }
		public string ReferenceSource { get; set; }
		public string ABTesting { get; set; }
		public bool IsMainStratFinished { get; set; }
		public string StrategyError { get; set; }
		public string FraudCheckStatus { get; set; }
		public int FraudCheckStatusId { get; set; }
		public bool IsCustomerInEnabledStatus { get; set; }
		public bool IsFraudInAlertMode { get; set; }
		public bool IsTestInAlertMode { get; set; }
		public bool IsCciMarkInAlertMode { get; set; }
		public bool IsCustomerStatusInAlertMode { get; set; }
		public bool IsAmlInAlertMode { get; set; }
		public string AmlResult { get; set; }
		public int Age { get; set; }
		public DateTime DateOfBirth { get; set; }
		public string Gender { get; set; }
		public string FullGender { get; set; }
		public string FamilyStatus { get; set; }
		public PropertyStatusModel PropertyStatus { get; set; }
		public string CompanyName { get; set; }
		public string CompanyType { get; set; }
		public string CompanySeniority { get; set; }
		public bool IsYoungCompany { get; set; }
		public string CompanyExperianRefNum { get; set; }
		public int NumOfDirectors { get; set; }
		public int NumOfShareholders { get; set; }
		public string Website { get; set; }
		public bool IsWarning { get; set; }
		public string PromoCode { get; set; }
		public string PromoCodeCss { get; set; }
		public CompanyEmployeeCountInfo CompanyEmployeeCountInfo { get; set; }
		public string ActiveCampaign { get; set; }
		public string PostCode { get; set; }
		public DirectorModel[] Directors { get; set; }
		public bool IsWizardComplete { get; set; }
		public bool IsAlibaba { get; set; }
		public int BoardResolutionTemplateID { get; set; }
		public int PersonalGuaranteeTemplateID { get; set; }

		public PersonalInfoModel() {
			IndustryFields = new List<string>();
			StrategyError = "";
			CompanyEmployeeCountInfo = null;
			serviceClient = new ServiceClient();
		} // constructor

		public void InitFromCustomer(Customer customer, ISession session) {
			if (customer == null)
				return;

			IsWizardComplete = (customer.WizardStep != null) && customer.WizardStep.TheLastOne;

			Id = customer.Id;
			SegmentType = customer.SegmentType();
			Origin = customer.CustomerOrigin.Name;
			IsAvoid = customer.IsAvoid;
			IsAlibaba = customer.IsAlibaba;
			FraudCheckStatus = customer.FraudStatus.Description();
			FraudCheckStatusId = (int)customer.FraudStatus;

			Website = "www." + customer.Name.Substring(customer.Name.IndexOf('@') + 1);

			if (customer.PersonalInfo != null)
			{
				if (customer.PersonalInfo.DateOfBirth.HasValue) {
					DateOfBirth = customer.PersonalInfo.DateOfBirth.Value;
					Age = MiscUtils.GetFullYears(customer.PersonalInfo.DateOfBirth.Value);

					Gender = customer.PersonalInfo.Gender.ToString();
					FullGender = Gender == "M" ? "Male" : "Female";
					FamilyStatus = customer.PersonalInfo.MaritalStatus.ToString();
					if (customer.PropertyStatus != null)
					{
						PropertyStatus = new PropertyStatusModel
							{
								Description = customer.PropertyStatus.Description,
								Id = customer.PropertyStatus.Id,
								IsOwnerOfMainAddress = customer.PropertyStatus.IsOwnerOfMainAddress,
								IsOwnerOfOtherProperties = customer.PropertyStatus.IsOwnerOfOtherProperties
							};
					}
				}
			}

			if (customer.Company != null)
			{
				CompanyName = customer.Company.CompanyName;
				CompanyType = customer.Company.TypeOfBusiness.ToString();
				CompanyExperianRefNum = customer.Company.ExperianRefNum;

				if (customer.Company.Directors != null) {
					List<Director> oDirList = customer.Company.Directors.ToList();

					if (oDirList.Count > 0)
						Directors = customer.Company.Directors.Select(d => DirectorModel.FromDirector(d, oDirList)).ToArray();
				} // if
			} // if

			if (Directors == null)
				Directors = new DirectorModel[0];
			
			if (customer.FraudStatus != FraudStatus.Ok)
			{
				IsFraudInAlertMode = true;
			}

			IsTestInAlertMode = customer.IsTest;

			AmlResult = customer.AMLResult;
			IsAmlInAlertMode = AmlResult != "Passed";

			PromoCode = customer.PromoCode;
			if (!string.IsNullOrEmpty(PromoCode))
				PromoCodeCss = "promo_code";

			if (customer.PersonalInfo != null) {
				Name = customer.PersonalInfo.Fullname;
				FirstName = customer.PersonalInfo.FirstName;
				Surname = customer.PersonalInfo.Surname;
				MobilePhone = customer.PersonalInfo.MobilePhone;
				DaytimePhone = customer.PersonalInfo.DaytimePhone;
			} // if

			List<CustomerPhone> customerPhones =  customerPhoneRepository.GetAll().Where(x => x.CustomerId == customer.Id && x.IsCurrent).ToList();
			MobileTooltip = "Click to verify";
			DaytimeTooltip = "Click to verify";
			foreach (CustomerPhone customerPhone in customerPhones)
			{
				if (customerPhone.PhoneType == "Mobile")
				{
					MobilePhoneVerified = customerPhone.IsVerified;
					if (MobilePhoneVerified)
					{
						if (!string.IsNullOrEmpty(customerPhone.VerifiedBy) && customerPhone.VerificationDate.HasValue)
						{
							MobileTooltip = string.Format("Verified by {0} at {1}", customerPhone.VerifiedBy, customerPhone.VerificationDate.Value.ToShortDateString());
						}
						else
						{
							MobileTooltip = "Verified";
						}
					}
				}
				else if (customerPhone.PhoneType == "Daytime")
				{
					DaytimePhoneVerified = customerPhone.IsVerified;
					if (DaytimePhoneVerified)
					{
						if (!string.IsNullOrEmpty(customerPhone.VerifiedBy) && customerPhone.VerificationDate.HasValue)
						{
							DaytimeTooltip = string.Format("Verified by {0} at {1}", customerPhone.VerifiedBy, customerPhone.VerificationDate.Value.ToShortDateString());
						}
						else
						{
							DaytimeTooltip = "Verified";
						}
					}
				}
			}

			Medal = customer.Medal.HasValue ? customer.Medal.ToString() : "";
			Email = customer.Name;
			EmailState = EmailConfirmationState.Get(customer);

			if (customer.GreetingMailSentDate != null)
			{
				DateTime registrationDate = customer.GreetingMailSentDate.Value;
				int registrationTimeYears, registrationTimeMonths;
				MiscUtils.GetFullYearsAndMonths(registrationDate, out registrationTimeYears, out registrationTimeMonths);
				
				RegistrationDate = customer.GreetingMailSentDate.Value.ToString("MMM dd, yyyy") +
				                   string.Format(" [{0}y {1}m]", registrationTimeYears, registrationTimeMonths);
			}

			IndustryFields.Add(string.Empty);
			UserStatus = customer.Status.ToString();
			CreditResult = customer.CreditResult.ToString();

			CustomerStatusId = customer.CollectionStatus.CurrentStatus.Id;
			IsCustomerInEnabledStatus = customer.CollectionStatus.CurrentStatus.IsEnabled;
			IsCustomerStatusInAlertMode = customer.CollectionStatus.CurrentStatus.Name != "Enabled";
			CustomerStatusName = customer.CollectionStatus.CurrentStatus.Name;

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

			IsCciMarkInAlertMode = customer.CciMark;

			TrustPilotStatusDescription = customer.TrustPilotStatus.Description;
			TrustPilotStatusName = customer.TrustPilotStatus.Name;

			string lastMainStrategyStatus = (string)session.CreateSQLQuery("EXEC GetLastMainStrategyStatus " + customer.Id).UniqueResult();

			IsMainStratFinished = lastMainStrategyStatus != "BG launch" && lastMainStrategyStatus != "In progress";
			if (lastMainStrategyStatus == "Finished" || lastMainStrategyStatus == "Failed" ||
			    lastMainStrategyStatus == "Terminated")
			{
				StrategyError = string.Format("Error occurred in main strategy, its status is:{0}", lastMainStrategyStatus);
			}

			BrokerID = customer.Broker == null ? 0 : customer.Broker.ID;
			BrokerName = customer.Broker == null ? "" : customer.Broker.ContactName;
			BrokerFirmName = customer.Broker == null ? "" : customer.Broker.FirmName;
			BrokerContactEmail = customer.Broker == null ? "" : customer.Broker.ContactEmail;
			BrokerContactMobile = customer.Broker == null ? "" : customer.Broker.ContactMobile;

			CustomerAddress oAddress = customer.AddressInfo.PersonalAddress.FirstOrDefault();
			if (oAddress != null)
				PostCode = oAddress.Rawpostcode;

			DbConnectionGenerator.Get(new SafeILog(this)).ForEachRowSafe(
				sr => {
					switch ((string)sr["DocumentName"]) {
					case "Board resolution":
						BoardResolutionTemplateID = sr["EsignTemplateID"];
						break;

					case "Personal guarantee":
						PersonalGuaranteeTemplateID = sr["EsignTemplateID"];
						break;
					} // switch
				},
				"LoadEsignTemplatesByCustomer",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", customer.Id)
			);
		} // InitFromCustomer

		public bool IsAvoid { get; set; }
		public string SegmentType { get; set; }
		public string Origin { get; set; }

		public string TrustPilotStatusDescription { get; set; }
		public string TrustPilotStatusName { get; set; }

		public int BrokerID { get; set; }
		public string BrokerName { get; set; }
		public string BrokerFirmName { get; set; }
		public string BrokerContactEmail { get; set; }
		public string BrokerContactMobile { get; set; }

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
