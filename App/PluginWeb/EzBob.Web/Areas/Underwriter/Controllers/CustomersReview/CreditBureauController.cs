namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
	using System;
	using System.Globalization;
	using System.Linq;
	using System.Web.Mvc;
	using ExperianLib.IdIdentityHub;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Ezbob.Logger;
	using Infrastructure;
	using Infrastructure.Attributes;
	using Models;
	using Code;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;

	public class CreditBureauController : Controller
	{
		private readonly CustomerRepository _customers;
		private readonly ServiceClient m_oServiceClient;
		private readonly CreditBureauModelBuilder _creditBureauModelBuilder;
		private readonly ConcentAgreementHelper _concentAgreementHelper;
		private readonly IWorkplaceContext _context;

		public CreditBureauController(
			CustomerRepository customers,
			CreditBureauModelBuilder creditBureauModelBuilder, 
			IWorkplaceContext context)
		{
			_customers = customers;
			m_oServiceClient = new ServiceClient();
			_creditBureauModelBuilder = creditBureauModelBuilder;
			_context = context;
			_concentAgreementHelper = new ConcentAgreementHelper();
		}

		[Ajax]
		[HttpPost]
		[Permission(Name = "RunCreditBureauChecks")]
		public JsonResult RunConsumerCheck(int customerId, int? directorId, bool forceCheck)
		{
			m_oServiceClient.Instance.ExperianConsumerCheck(_context.UserId, customerId, directorId, forceCheck);
			return Json(new { Message = "The evaluation has been started. Please refresh this application after a while..." });
		}

		[Ajax]
		[HttpPost]
		[Permission(Name = "RunCreditBureauChecks")]
		public JsonResult RunCompanyCheck(int id, bool forceCheck)
		{
			m_oServiceClient.Instance.ExperianCompanyCheck(_context.UserId, id, forceCheck);
			return Json(new { Message = "The evaluation has been started. Please refresh this application after a while..." });
		}

		[Ajax]
		[HttpGet]
		public JsonResult Index(int id, bool getFromLog = false, long? logId = null) {
			var log = new SafeILog(this);
			log.Debug("Loading a credit bureau model for customer {0}, getFromLog = {1}, logId = {2}.", id, getFromLog, logId);
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
			if (customer.BankAccount != null)
			{
				model.BankAccount = customer.BankAccount.AccountNumber;
				model.SortCode = customer.BankAccount.SortCode;
			}

			var customerMainAddress = customer.AddressInfo.PersonalAddress.FirstOrDefault();
			if (customerMainAddress != null)
			{
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
			if (customerPrevAddress != null)
			{
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

		[Ajax]
		[HttpPost]
		[Permission(Name = "RunCreditBureauChecks")]
		public JsonResult RunAmlbwaCheck(int id, int checkType, string houseNumber, string houseName, string street,
											string district, string town, string county, string postcode, string bankAccount, string sortCode)
		{
			if (checkType == 1)
			{
				m_oServiceClient.Instance.CheckAmlCustom(_context.UserId, id, houseNumber, houseName, street, district, town, county, postcode);
			}
			else
			{
				m_oServiceClient.Instance.CheckBwaCustom(_context.UserId, id, houseNumber, houseName, street, district, town, county, postcode, bankAccount, sortCode);
			}

			return Json(new { Message = "The evaluation has been started. Please refresh this application after a while..." });
		}

		public FileContentResult DownloadConsentAgreement(int id)
		{
			var customer = _customers.Get(id);
			var pdf = _concentAgreementHelper.GenerateWidhDataBase(customer);
			var fileName = _concentAgreementHelper.GetFileName(customer);
			return File(pdf, "application/pdf", fileName);
		}

		[Ajax]
		[HttpPost]
		public JsonResult IsCompanyCacheRelevant(int customerId)
		{
			var customer = _customers.Get(customerId);
			if (customer == null || customer.Company == null || string.IsNullOrEmpty(customer.Company.ExperianRefNum) ||
			    customer.Company.ExperianRefNum.Equals("NotFound"))
			{
				return Json(new { NoCompany = true });
			}

			DateTimeActionResult result = m_oServiceClient.Instance.GetExperianCompanyCacheDate(_context.UserId, customer.Company.ExperianRefNum);

			DateTime cacheDate = result.Value;
			int cacheValidForDays = ConfigManager.CurrentValues.Instance.UpdateCompanyDataPeriodDays;
			string isRelevant = (DateTime.UtcNow - cacheDate).TotalDays > cacheValidForDays ? "False" : "True";
			return Json(new { IsRelevant = isRelevant, LastCheckDate = cacheDate.ToString("dd/MM/yyyy"), CacheValidForDays = cacheValidForDays.ToString(CultureInfo.InvariantCulture) });
		}
	}
}