namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
	using System;
	using System.Data;
	using System.Linq;
	using System.Web.Mvc;
	using ApplicationMng.Repository;
	using ExperianLib.IdIdentityHub;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Models;
	using Code;
	using Infrastructure;
	using Scorto.Web;
	using EzServiceReference;
	using ActionResult = System.Web.Mvc.ActionResult;

	public class CreditBureauController : Controller
    {
        private readonly CustomerRepository _customers;
		private readonly ServiceClient m_oServiceClient;
        private readonly IUsersRepository _users;
        private readonly IApplicationRepository _applications;
        private readonly IEzBobConfiguration _config;
        private readonly CreditBureauModelBuilder _creditBureauModelBuilder;
        private readonly ConcentAgreementHelper _concentAgreementHelper;

        public CreditBureauController(
			CustomerRepository customers,
			IUsersRepository users,
			IApplicationRepository applications,
			IEzBobConfiguration config,
			CreditBureauModelBuilder creditBureauModelBuilder
		) {
            _customers = customers;
	        m_oServiceClient = new ServiceClient();
            _users = users;
            _applications = applications;
            _config = config;
            _creditBureauModelBuilder = creditBureauModelBuilder;
            _concentAgreementHelper = new ConcentAgreementHelper();
        }


        [HttpPost]
        [Transactional]
        public JsonNetResult RunCheck(int id)
        {
            var customer = _customers.Get(id);
			var anyApps = StrategyChecker.IsStrategyRunning(id, true);
            if (anyApps)
                return this.JsonNet(new { Message = "The evaluation strategy is already running. Please wait..." });

	        var underwriter = _users.GetUserByLogin(User.Identity.Name);

			m_oServiceClient.Instance.MainStrategy2(underwriter.Id, _users.Get(id).Id, NewCreditLineOption.UpdateEverythingExceptMp, Convert.ToInt32(customer.IsAvoid), true);

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
            var customer = _customers.Get(id);
            var isRunning = StrategyChecker.IsStrategyRunning(id, true);

            if (isRunning)
                return this.JsonNet(new { Message = "The evaluation strategy is already running. Please wait..." });

	        var underwriter = _users.GetUserByLogin(User.Identity.Name);

			m_oServiceClient.Instance.MainStrategy3(underwriter.Id, _users.Get(id).Id, checkType, houseNumber, houseName, street, district, town, county, postcode, bankAccount, sortCode, Convert.ToInt32(customer.IsAvoid));

            return this.JsonNet(new { Message = "The evaluation has been started. Please refresh this application after a while..." });
        }

        public ActionResult DownloadConsentAgreement(int id)
        {
            var customer = _customers.Get(id);
            var pdf = _concentAgreementHelper.GenerateWidhDataBase(customer);
            var fileName = _concentAgreementHelper.GetFileName(customer);
            return File(pdf, "application/pdf", fileName);
        }

    }
}