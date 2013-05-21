﻿using System;
using System.Collections.Generic;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.PyaPalDetails;
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
            model.BankAccounts.AddRange(customer.BankAccounts.Where(a => a.Id != model.CurrentBankAccount.Id).Select(BankAccountModel.FromCard));

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
        public ActionResult PayPointCallback(bool valid, string trans_id, string code, string auth_code, decimal? amount, string ip, string test_status, string hash, string message, string card_no, string expiry, int customerId)
        {
            if (test_status == "true")
            {
                card_no = "1111";
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

            var customer = _customers.GetChecked(customerId);

            customer.TryAddPayPointCard(trans_id, card_no, expiry, customer.PersonalInfo.Fullname);

            _appCreator.PayPointAddedByUnderwriter(_context.User, customer, card_no);

            return View("PayPointAdded");
        }

        [Transactional]
        [HttpPost]
        public JsonNetResult AddPayPointCard(int customerId, string transactionid, string cardno, DateTime expiredate)
        {
            var customer = _customers.GetChecked(customerId);
            customer.TryAddPayPointCard(transactionid, cardno, expiredate.ToString("MMyy"), customer.PersonalInfo.Fullname);

            _appCreator.PayPointAddedByUnderwriter( _context.User, customer, cardno);

            return this.JsonNet(new {});
        }
        [Ajax]
        [Transactional]
        [HttpPost]
        public void SetPaypointDefaultCard(string transactionid, int customerId)
        {
            var customer = _customers.GetChecked(customerId);
            customer.PayPointTransactionId = transactionid;
        }
    }
}
