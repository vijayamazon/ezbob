using System;
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
        private readonly IPayPalDetailsRepository _payPalDetails;
        private readonly ISortCodeChecker _sortCodeChecker;
        private readonly IPayPointFacade _payPointFacade;
        private readonly IWorkplaceContext _context;
        private readonly IAppCreator _appCreator;

        public PaymentAccountsController(CustomerRepository customers,
                                            IAppCreator appCreator,
                                            ICustomerMarketPlaceRepository customerMarketplaces,
                                            IPayPalDetailsRepository payPalDetails,
                                            ISortCodeChecker sortCodeChecker,
                                            IPayPointFacade payPointFacade,
                                            IWorkplaceContext context
                                            )
        {
            _customers = customers;
            _appCreator = appCreator;
            _customerMarketplaces = customerMarketplaces;
            _payPalDetails = payPalDetails;
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
                customer.TryAddPayPointCard(customer.PayPointTransactionId, customer.CreditCardNo, null);
            }
            
            model.PayPalAccounts.AddRange(customer.GetPayPalAccounts());

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
        [HttpGet]
        [Transactional]
        public JsonNetResult Details(int id)
        {
            var payments = new PayPalAccountGeneralPaymentsInfoModel();
            var total = new List<PayPalGeneralDataRowModel>();

            var details = _payPalDetails.GetDetails(id);

            var detailIncome = details.DetailIncome.Select(row => new PayPalGeneralDataRowModel(row)).ToList();
            var detailExpenses = details.DetailExpenses.Select(row => new PayPalGeneralDataRowModel(row)).ToList();

            total.Add(new PayPalGeneralDataRowModel(details.TotalIncome));
            total.Add(new PayPalGeneralDataRowModel(details.TotalExpenses));
            total.Add(new PayPalGeneralDataRowModel(details.TotalTransactions) { Pounds = false });

            payments.Data = total;

            var detailPayments = new PayPalAccountDetailPaymentsInfoModel
                {
                    Income = ProcessPayments(detailIncome, details), 
                    Expenses = ProcessPayments(detailExpenses, details)
                };

            var generalInfo = details.Marketplace.Customer.GetPayPalAccounts().FirstOrDefault(x => x.id == id);
            var model = new PayPalAccountModel { GeneralInfo = generalInfo, PersonalInfo = new PayPalAccountInfoModel(details.Marketplace.PersonalInfo), DetailPayments = detailPayments, Payments = payments };
            return this.JsonNet(model);
        }

        private static IEnumerable<PayPalGeneralDataRowModel> ProcessPayments(ICollection<PayPalGeneralDataRowModel> payments, PayPalDetailsModel details)
        {
            var percents = new List<double?>();
            foreach (var payPalGeneralDataRowModel in payments)
            {
                var v1 = payPalGeneralDataRowModel.FirstNotNull();
                var v2 = details.TotalIncome.FirstNotNull();
                var percent = (v1 / v2) * 100;
                payPalGeneralDataRowModel.Type = string.Format("{0} ({1:0.00}%)", payPalGeneralDataRowModel.Type, percent ?? 0);
                payPalGeneralDataRowModel.Percent = percent;
                percents.Add(percent);
            }

            var otherIncomePercent = 100 - percents.Sum();
            var otherIncomeRow = new PayPalGeneralDataRowModel
                {
                    Type = string.Format("Other ({0:0.00}%)", otherIncomePercent),
                    M1 = details.TotalIncome.M1 - payments.Sum(x => x.M1),
                    M3 = details.TotalIncome.M3 - payments.Sum(x => x.M3),
                    M6 = details.TotalIncome.M6 - payments.Sum(x => x.M6),
                    M12 = details.TotalIncome.M12 - payments.Sum(x => x.M12),
                    M15 = details.TotalIncome.M15 - payments.Sum(x => x.M15),
                    M18 = details.TotalIncome.M18 - payments.Sum(x => x.M18),
                    M24 = details.TotalIncome.M24 - payments.Sum(x => x.M24),
                    M24Plus = details.TotalIncome.M24Plus - payments.Sum(x => x.M24Plus)
                };
            payments.Add(otherIncomeRow);
            return payments.OrderByDescending(x => x.Percent).ToList();
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

            customer.TryAddPayPointCard(trans_id, card_no, expiry);

            _appCreator.PayPointAddedByUnderwriter(_context.User, customer, card_no);

            return View("PayPointAdded");
        }

        [Transactional]
        [HttpPost]
        public JsonNetResult AddPayPointCard(int customerId, string transactionid, string cardno, DateTime expiredate)
        {
            var customer = _customers.GetChecked(customerId);
            customer.TryAddPayPointCard(transactionid, cardno, expiredate.ToString("MMyy"));

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
