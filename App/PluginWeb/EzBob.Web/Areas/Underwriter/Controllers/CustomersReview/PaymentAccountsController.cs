namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
	using System.Data;
	using Code;
	using EzServiceReference;
	using NHibernate;
	using System;
	using EZBob.DatabaseLib.Model.Database;
	using PostcodeAnywhere;
	using Scorto.Web;
	using System.Linq;
	using System.Web.Mvc;
	using Models;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Customer.Models;
	using ActionResult = System.Web.Mvc.ActionResult;

	public class PaymentAccountsController : Controller
    {
        private readonly CustomerRepository _customers;
        private readonly ICustomerMarketPlaceRepository _customerMarketplaces;
        private readonly ISortCodeChecker _sortCodeChecker;
        private readonly IPayPointFacade _payPointFacade;
        private readonly IWorkplaceContext _context;
		private readonly EzServiceClient m_oServiceClient;
		private readonly ISession session;

        public PaymentAccountsController(
			CustomerRepository customers,
			ICustomerMarketPlaceRepository customerMarketplaces,
			ISortCodeChecker sortCodeChecker,
			IPayPointFacade payPointFacade,
			IWorkplaceContext context, 
			ISession session
		) {
            _customers = customers;
	        m_oServiceClient = ServiceClient.Instance;
            _customerMarketplaces = customerMarketplaces;
            _sortCodeChecker = sortCodeChecker;
            _payPointFacade = payPointFacade;
			_context = context;
			this.session = session;
        }

        [Ajax]
        [HttpGet]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
        public JsonNetResult Index(int id)
        {
            var customer = _customers.Get(id);
            var model = new PaymentsAccountModel(customer);
            return this.JsonNet(model);
        }

        [Ajax]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
        public JsonNetResult SetDefaultCard(int customerId, int cardId)
        {
            var customer = _customers.Get(customerId);
            var card = customer.BankAccounts.SingleOrDefault(c => c.Id == cardId);
            customer.SetDefaultCard(card);
            return this.JsonNet(new {});
        }

        [Ajax]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
        public JsonNetResult PerformCheckBankAccount(int id, int cardid)
        {
            var customer = _customers.Get(id);
            var card = customer.BankAccounts.Single(b => b.Id == cardid);
            return CheckBankAccount(card);
        }

        [Ajax]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
        public JsonNetResult CheckBankAccount(string bankAccount, string sortCode)
        {
            var card = new CardInfo(bankAccount, sortCode);
            return CheckBankAccount(card);
        }

        private JsonNetResult CheckBankAccount(CardInfo card)
        {
            string error = null;
            try
            {
                _sortCodeChecker.Check(card);
            }
            catch (UnknownSortCodeException )
            {
                error = "Sortcode was not found.";
            }
            catch (SortCodeNotFoundException )
            {
                error = "Sortcode was not found.";
            }
            catch (InvalidAccountNumberException )
            {
                error = "Invalid account number.";
            }

            if (!string.IsNullOrEmpty(error))
            {
                card.StatusInformation = error;
                return this.JsonNet(new { error = error });
            }

            return this.JsonNet(BankAccountModel.FromCard(card));
        }

        [Ajax]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
        public JsonNetResult TryAddBankAccount(int customerId, string bankAccount, string sortCode, BankAccountType accountType)
        {
            var customer = _customers.Get(customerId);

            if (customer.BankAccounts.Any(a => a.BankAccount == bankAccount && a.SortCode == sortCode))
            {
                return this.JsonNet(new {error = "This bank account is already added."});
            }

            var card = new CardInfo() { BankAccount = bankAccount, SortCode = sortCode, Customer = customer, Type = accountType };
            try
            {
                _sortCodeChecker.Check(card);
            }
            catch (Exception )
            {
            }
            customer.BankAccounts.Add(card);
            customer.SetDefaultCard(card);

            return this.JsonNet(new {r = card.Id});
        }

        [Ajax]
        [HttpGet]
        public JsonNetResult CheckForUpdatedStatus(int mpId)
        {
            return this.JsonNet(new { status = _customerMarketplaces.Get(mpId).GetUpdatingStatus() });
        }

		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
        public RedirectResult AddPayPoint(int id)
        {
            var customer = _customers.Get(id);

            var callback = Url.Action("PayPointCallback", "PaymentAccounts", new { Area = "Underwriter", customerId = id }, "https");
            var url = _payPointFacade.GeneratePaymentUrl(customer != null && customer.IsOffline.Value, 5m, callback);
            
            return Redirect(url);
        }

		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
        [HttpGet]
        public ActionResult PayPointCallback(bool valid, string trans_id, string code, string auth_code, decimal? amount, string ip, string test_status, string hash, string message, string card_no, string customer, string expiry, int customerId)
        {
            if (test_status == "true")
            {
				// Use last 4 random digits as card number (to enable useful tests)
	            string random4Digits = string.Format("{0}{1}", DateTime.UtcNow.Second, DateTime.UtcNow.Millisecond);
				if (random4Digits.Length > 4)
				{
					random4Digits = random4Digits.Substring(random4Digits.Length - 4);
				}
				card_no = random4Digits;
                expiry = string.Format("{0}{1}", "01", DateTime.Now.AddYears(2).Year.ToString().Substring(2, 2));
            }
            if (!valid || code != "A")
            {
                TempData["code"] = code;
                TempData["message"] = message;
                return View("Error");
            }

            if (!_payPointFacade.CheckHash(hash, Request.Url))
            {
                throw new Exception("check hash failed");
            }

            var cus = _customers.GetChecked(customerId);

            AddPayPointCardToCustomer(trans_id, card_no, cus, expiry);

            return View("PayPointAdded", amount ?? 0);
        }

		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
        [HttpPost]
        public JsonNetResult AddPayPointCard(int customerId, string transactionid, string cardno, DateTime expiredate)
        {
            var customer = _customers.GetChecked(customerId);
            var expiry = expiredate.ToString("MMyy");

            AddPayPointCardToCustomer(transactionid, cardno, customer, expiry);

            return this.JsonNet(new {});
        }

        private void AddPayPointCardToCustomer(string transactionid, string cardno, EZBob.DatabaseLib.Model.Database.Customer customer, string expiry)
        {
            customer.TryAddPayPointCard(transactionid, cardno, expiry, customer.PersonalInfo.Fullname);

            if (string.IsNullOrEmpty(customer.PayPointTransactionId))
            {
                SetPaypointDefaultCard(transactionid, customer.Id, cardno);
            }

	        m_oServiceClient.PayPointAddedByUnderwriter(customer.Id, cardno, _context.User.FullName, _context.User.Id);
        }

        [Ajax]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
        [HttpPost]
        public void SetPaypointDefaultCard(string transactionid, int customerId, string cardNo)
        {
            var customer = _customers.GetChecked(customerId);
            customer.PayPointTransactionId = transactionid;
            customer.CreditCardNo = cardNo;
        }
    }
}
