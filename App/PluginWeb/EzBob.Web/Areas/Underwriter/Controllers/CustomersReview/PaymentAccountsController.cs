using System;
using System.Collections.Generic;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.PayPal;
using EzBob.Models;
using EzBob.Web.ApplicationCreator;
using PostcodeAnywhere;
using Scorto.Web;
using System.Linq;
using System.Web.Mvc;
using EzBob.Web.Areas.Underwriter.Models;
using EZBob.DatabaseLib.Model.Database.Repository;


using EzBob.Web.Areas.Customer.Models;

namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
    public class PaymentAccountsController : Controller
    {
        private readonly CustomerRepository _customers;
        private readonly ICustomerMarketPlaceRepository _customerMarketplaces;
        private readonly ISortCodeChecker _sortCodeChecker;
        private readonly IPayPointFacade _payPointFacade;
        private readonly IWorkplaceContext _context;
        private readonly IAppCreator _appCreator;

        public PaymentAccountsController(CustomerRepository customers,
                                            IAppCreator appCreator,
                                            ICustomerMarketPlaceRepository customerMarketplaces,
                                            ISortCodeChecker sortCodeChecker,
                                            IPayPointFacade payPointFacade,
                                            IWorkplaceContext context
                                            )
        {
            _customers = customers;
            _appCreator = appCreator;
            _customerMarketplaces = customerMarketplaces;
            _sortCodeChecker = sortCodeChecker;
            _payPointFacade = payPointFacade;
            _context = context;
        }

        [Ajax]
        [HttpGet]
        [Transactional]
        public JsonNetResult Index(int id)
        {
            var customer = _customers.Get(id);
            var model = new PaymentsAccountModel();
            
            if (!string.IsNullOrEmpty(customer.PayPointTransactionId) && !customer.PayPointCards.Any())
            {
                customer.TryAddPayPointCard(customer.PayPointTransactionId, customer.CreditCardNo, null, customer.PersonalInfo.Fullname);
            }
            
            model.PayPointCards.AddRange(customer.PayPointCards.Select(PayPointCardModel.FromCard));

            model.CurrentBankAccount = BankAccountModel.FromCard(customer.CurrentCard);
            
            int currentBankAccountId = 0;
            if (model.CurrentBankAccount != null) currentBankAccountId = model.CurrentBankAccount.Id;

            model.BankAccounts.AddRange(customer.BankAccounts.Where(a => a.Id != currentBankAccountId).Select(BankAccountModel.FromCard));

            return this.JsonNet(model);
        }

        [Ajax]
        [Transactional]
        public JsonNetResult SetDefaultCard(int customerId, int cardId)
        {
            var customer = _customers.Get(customerId);
            var card = customer.BankAccounts.SingleOrDefault(c => c.Id == cardId);
            customer.SetDefaultCard(card);
            return this.JsonNet(new {});
        }

        [Ajax]
        [Transactional]
        public JsonNetResult PerformCheckBankAccount(int id, int cardid)
        {
            var customer = _customers.Get(id);
            var card = customer.BankAccounts.Single(b => b.Id == cardid);
            return CheckBankAccount(card);
        }

        [Ajax]
        [Transactional]
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
            catch (UnknownSortCodeException e)
            {
                error = "Sortcode was not found.";
            }
            catch (SortCodeNotFoundException e)
            {
                error = "Sortcode was not found.";
            }
            catch (InvalidAccountNumberException e)
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
        [Transactional]
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
            catch (Exception e)
            {
            }
            customer.BankAccounts.Add(card);
            customer.SetDefaultCard(card);

            return this.JsonNet(new {r = card.Id});
        }



        [Ajax]
        public void ReCheckPaypal(int customerId, int umi)
        {
            if (_customerMarketplaces.Get(umi).UpdatingEnd == null)
            {
                throw new Exception("Strategy already started");
            }
            var customer = _customers.Get(customerId);
            _customerMarketplaces.ClearUpdatingEnd(umi);
			_appCreator.CustomerMarketPlaceAdded(customer, umi);
        }

        [Ajax]
        [HttpGet]
        public JsonNetResult CheckForUpdatedStatus(int mpId)
        {
            return this.JsonNet(new { status = _customerMarketplaces.GetUpdatedStatus(mpId) });
        }

        [Transactional]
        public RedirectResult AddPayPoint(int id)
        {
            var callback = Url.Action("PayPointCallback", "PaymentAccounts", new { Area = "Underwriter", customerId = id }, "https");
            var url = _payPointFacade.GeneratePaymentUrl(5m, callback);
            
            return Redirect(url);
        }

        [Transactional]
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

        [Transactional]
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

            _appCreator.PayPointAddedByUnderwriter(_context.User, customer, cardno);
        }

        [Ajax]
        [Transactional]
        [HttpPost]
        public void SetPaypointDefaultCard(string transactionid, int customerId, string cardNo)
        {
            var customer = _customers.GetChecked(customerId);
            customer.PayPointTransactionId = transactionid;
            customer.CreditCardNo = cardNo;
        }
    }
}
