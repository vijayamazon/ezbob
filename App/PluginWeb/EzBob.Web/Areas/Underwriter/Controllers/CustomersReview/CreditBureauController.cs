namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using System.Linq;
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Mapping;
	using ExperianLib.IdIdentityHub;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EzServiceReference;
	using Models;
	using Code;
	using Scorto.Web;
	using ActionResult = System.Web.Mvc.ActionResult;

	public class CreditBureauController : Controller
    {
        private readonly CustomerRepository _customers;
		private readonly ServiceClient m_oServiceClient;
        private readonly CreditBureauModelBuilder _creditBureauModelBuilder;
        private readonly ConcentAgreementHelper _concentAgreementHelper;
		private readonly DirectorRepository directorRepository;
		private readonly ConfigurationVariablesRepository configurationVariablesRepository;

        public CreditBureauController(
			CustomerRepository customers,
			CreditBureauModelBuilder creditBureauModelBuilder,
			DirectorRepository directorRepository,
			ConfigurationVariablesRepository configurationVariablesRepository
		) 
		{
            _customers = customers;
	        m_oServiceClient = new ServiceClient();
            _creditBureauModelBuilder = creditBureauModelBuilder;
            _concentAgreementHelper = new ConcentAgreementHelper();
	        this.directorRepository = directorRepository;
	        this.configurationVariablesRepository = configurationVariablesRepository;
		}

		[HttpPost]
		[Transactional]
		public JsonNetResult RunConsumerCheck(int customerId, bool forceCheck)
		{
			m_oServiceClient.Instance.CheckExperianConsumer(customerId, 0, forceCheck);
			List<Director> directors = directorRepository.GetAll().Where(x => x.Customer.Id == customerId).ToList();
			foreach (Director director in directors)
			{
				m_oServiceClient.Instance.CheckExperianConsumer(customerId, director.Id, forceCheck);
			}
			return this.JsonNet(new { Message = "The evaluation has been started. Please refresh this application after a while..." });
		}

		[HttpPost]
		[Transactional]
		public JsonNetResult RunCompanyCheck(int id, bool forceCheck)
		{
			m_oServiceClient.Instance.CheckExperianCompany(id, forceCheck);
			return this.JsonNet(new { Message = "The evaluation has been started. Please refresh this application after a while..." });
		}

        [Ajax]
        [HttpGet]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
        public JsonNetResult Index(int id, bool getFromLog = false, long? logId = null)
        {
            var customer = _customers.Get(id);
            var model = _creditBureauModelBuilder.Create(customer, getFromLog, logId);
            return this.JsonNet(model);
        }

        [Ajax]
        [HttpGet]
        public JsonNetResult IdHubCustomAddress(int id)
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
            return this.JsonNet(model);
        }

        [HttpPost]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
        public JsonNetResult RunAmlbwaCheck(int id, int checkType, string houseNumber, string houseName, string street,
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
			
            return this.JsonNet(new { Message = "The evaluation has been started. Please refresh this application after a while..." });
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
		public JsonNetResult IsConsumerCacheRelevant(int customerId)
		{
			var ids = new List<int> { customerId };
			IQueryable<Director> directors = directorRepository.GetAll().Where(x => x.Customer.Id == customerId);
			ids.AddRange(directors.Select(d => d.Id));
			DateTimeActionResult result = m_oServiceClient.Instance.GetExperianConsumerCacheDate(ids.ToArray());
			DateTime cacheDate = result.Value;
			int cacheValidForDays = configurationVariablesRepository.GetByNameAsInt("UpdateConsumerDataPeriodDays");
			string isRelevant = (DateTime.UtcNow - cacheDate).TotalDays > cacheValidForDays ? "False" : "True";
			return this.JsonNet(new { IsRelevant = isRelevant, LastCheckDate = cacheDate.ToString("dd/MM/yyyy"), CacheValidForDays = cacheValidForDays.ToString(CultureInfo.InvariantCulture) });
		}

		[HttpPost]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public JsonNetResult IsCompanyCacheRelevant(int customerId)
		{
			DateTimeActionResult result = m_oServiceClient.Instance.GetExperianCompanyCacheDate(customerId);
			DateTime cacheDate = result.Value;
			int cacheValidForDays = configurationVariablesRepository.GetByNameAsInt("UpdateCompanyDataPeriodDays");
			string isRelevant = (DateTime.UtcNow - cacheDate).TotalDays > cacheValidForDays ? "False" : "True";
			return this.JsonNet(new { IsRelevant = isRelevant, LastCheckDate = cacheDate.ToString("dd/MM/yyyy"), CacheValidForDays = cacheValidForDays.ToString(CultureInfo.InvariantCulture) });
		}
    }
}