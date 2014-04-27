﻿namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
	using System;
	using System.Data;
	using System.Globalization;
	using System.Linq;
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model;
	using ExperianLib.IdIdentityHub;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EzServiceReference;
	using Infrastructure.Attributes;
	using Models;
	using Code;
	using ActionResult = System.Web.Mvc.ActionResult;

	public class CreditBureauController : Controller
    {
        private readonly CustomerRepository _customers;
		private readonly ServiceClient m_oServiceClient;
        private readonly CreditBureauModelBuilder _creditBureauModelBuilder;
        private readonly ConcentAgreementHelper _concentAgreementHelper;
		private readonly ConfigurationVariablesRepository configurationVariablesRepository;

        public CreditBureauController(
			CustomerRepository customers,
			CreditBureauModelBuilder creditBureauModelBuilder,
			ConfigurationVariablesRepository configurationVariablesRepository
		) 
		{
            _customers = customers;
	        m_oServiceClient = new ServiceClient();
            _creditBureauModelBuilder = creditBureauModelBuilder;
            _concentAgreementHelper = new ConcentAgreementHelper();
	        this.configurationVariablesRepository = configurationVariablesRepository;
		}

		[HttpPost]
		[Transactional]
		public JsonResult RunConsumerCheck(int customerId, int directorId, bool forceCheck)
		{
			m_oServiceClient.Instance.ExperianConsumerCheck(customerId, directorId, forceCheck);
			return Json(new { Message = "The evaluation has been started. Please refresh this application after a while..." });
		}

		[HttpPost]
		[Transactional]
		public JsonResult RunCompanyCheck(int id, bool forceCheck)
		{
			m_oServiceClient.Instance.ExperianCompanyCheck(id, forceCheck);
			return Json(new { Message = "The evaluation has been started. Please refresh this application after a while..." });
		}

        [Ajax]
        [HttpGet]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
        public JsonResult Index(int id, bool getFromLog = false, long? logId = null)
        {
            var customer = _customers.Get(id);
            var model = _creditBureauModelBuilder.Create(customer, getFromLog, logId);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [Ajax]
        [HttpGet]
        public JsonResult IdHubCustomAddress(int id)
        {
            var customer = _customers.Get(id);

            var model = new IdHubCustomAddressModel
                            {
                                Id = id,
                                Firstname = customer.PersonalInfo.FirstName ?? string.Empty,
                                MiddleName = customer.PersonalInfo.MiddleInitial ?? string.Empty,
                                Surname = customer.PersonalInfo.Surname ?? string.Empty,
                                FullName = customer.PersonalInfo.Fullname ?? string.Empty,
                                Gender = customer.PersonalInfo.Gender.ToString(),
                                DateOfBirth = customer.PersonalInfo.DateOfBirth.HasValue ? customer.PersonalInfo.DateOfBirth.Value.ToShortDateString() : string.Empty,
                                BankAccount = string.Empty,
                                SortCode = string.Empty
                            };
            if(customer.BankAccount != null)
            {
                model.BankAccount = customer.BankAccount.AccountNumber;
                model.SortCode = customer.BankAccount.SortCode;
            }

            var customerMainAddress = customer.AddressInfo.PersonalAddress.FirstOrDefault();
            if(customerMainAddress != null) {
	            model.CurAddressIsManual = customerMainAddress.Id.StartsWith("MANUAL");
                model.CurAddressLine1 = customerMainAddress.Line1 ?? string.Empty;
                model.CurAddressLine2 = customerMainAddress.Line2 ?? string.Empty;
                model.CurAddressLine3 = customerMainAddress.Line3 ?? string.Empty;
                model.CurAddressTown = customerMainAddress.Town ?? string.Empty;
                model.CurAddressCounty = customerMainAddress.County ?? string.Empty;
                model.CurAddressPostcode = customerMainAddress.Postcode ?? string.Empty;
                model.CurAddressCountry = customerMainAddress.Country ?? string.Empty;
            }

            var customerPrevAddress = customer.AddressInfo.PrevPersonAddresses.FirstOrDefault();
            if (customerPrevAddress != null) {
	            model.PrevAddressIsManual = customerPrevAddress.Id.StartsWith("MANUAL");
                model.PrevAddressLine1 = customerPrevAddress.Line1 ?? string.Empty;
                model.PrevAddressLine2 = customerPrevAddress.Line2 ?? string.Empty;
                model.PrevAddressLine3 = customerPrevAddress.Line3 ?? string.Empty;
                model.PrevAddressTown = customerPrevAddress.Town ?? string.Empty;
                model.PrevAddressCounty = customerPrevAddress.County ?? string.Empty;
                model.PrevAddressPostcode = customerPrevAddress.Postcode ?? string.Empty;
                model.PrevAddressCountry = customerPrevAddress.Country ?? string.Empty;
            }

            var service = new IdHubService();
            var parsedAddress = service.FillAddress(model.CurAddressLine1, model.CurAddressLine2, model.CurAddressLine3,
                                                    model.CurAddressTown, model.CurAddressCounty,
                                                    model.CurAddressPostcode);
            if (parsedAddress != null)
            {
                model.IdHubAddressHouseNumber = parsedAddress.AddressDetail.HouseNumber ?? string.Empty;
                model.IdHubAddressHouseName = parsedAddress.AddressDetail.HouseName ?? string.Empty;
                model.IdHubAddressStreet = parsedAddress.AddressDetail.Address1 ?? string.Empty;
                model.IdHubAddressDistrict = parsedAddress.AddressDetail.Address2 ?? string.Empty;
                model.IdHubAddressTown = parsedAddress.AddressDetail.Address3 ?? string.Empty;
                model.IdHubAddressCounty = parsedAddress.AddressDetail.Address4 ?? string.Empty;
                model.IdHubAddressPostcode = parsedAddress.AddressDetail.PostCode ?? string.Empty;
                model.IdHubAddressCountry = parsedAddress.AddressDetail.Country ?? string.Empty;
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
        public JsonResult RunAmlbwaCheck(int id, int checkType, string houseNumber, string houseName, string street,
                                            string district, string town, string county, string postcode, string bankAccount, string sortCode)
        {
			if (checkType == 1)
			{
				m_oServiceClient.Instance.CheckAmlCustom(id, houseNumber, houseName, street, district, town, county, postcode);
			}
			else
			{
				m_oServiceClient.Instance.CheckBwaCustom(id, houseNumber, houseName, street, district, town, county, postcode, bankAccount, sortCode);
			}
			
            return Json(new { Message = "The evaluation has been started. Please refresh this application after a while..." });
        }

        public ActionResult DownloadConsentAgreement(int id)
        {
            var customer = _customers.Get(id);
            var pdf = _concentAgreementHelper.GenerateWidhDataBase(customer);
            var fileName = _concentAgreementHelper.GetFileName(customer);
            return File(pdf, "application/pdf", fileName);
        }

		[HttpPost]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public JsonResult IsConsumerCacheRelevant(int customerId, int directorId)
		{
			DateTimeActionResult result = m_oServiceClient.Instance.GetExperianConsumerCacheDate(customerId, directorId);
			DateTime cacheDate = result.Value;
			int cacheValidForDays = configurationVariablesRepository.GetByNameAsInt("UpdateConsumerDataPeriodDays");
			string isRelevant = (DateTime.UtcNow - cacheDate).TotalDays > cacheValidForDays ? "False" : "True";
			return Json(new { IsRelevant = isRelevant, LastCheckDate = cacheDate.ToString("dd/MM/yyyy"), CacheValidForDays = cacheValidForDays.ToString(CultureInfo.InvariantCulture) });
		}

		[HttpPost]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public JsonResult IsCompanyCacheRelevant(int customerId)
		{
			DateTimeActionResult result = m_oServiceClient.Instance.GetExperianCompanyCacheDate(customerId);
			DateTime cacheDate = result.Value;
			int cacheValidForDays = configurationVariablesRepository.GetByNameAsInt("UpdateCompanyDataPeriodDays");
			string isRelevant = (DateTime.UtcNow - cacheDate).TotalDays > cacheValidForDays ? "False" : "True";
			return Json(new { IsRelevant = isRelevant, LastCheckDate = cacheDate.ToString("dd/MM/yyyy"), CacheValidForDays = cacheValidForDays.ToString(CultureInfo.InvariantCulture) });
		}
    }
}