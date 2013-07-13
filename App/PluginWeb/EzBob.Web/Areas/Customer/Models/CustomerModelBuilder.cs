using System;
using System.Linq;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Loans;
using EZBob.DatabaseLib.Model.Database.Repository;
using EZBob.DatabaseLib.Model.Loans;
using EZBob.DatabaseLib.Repository;
using EzBob.Models;
using EzBob.Web.Areas.Underwriter.Models;
using PaymentServices.Calculators;

namespace EzBob.Web.Areas.Customer.Models
{
    public class CustomerModelBuilder
    {
        private readonly ISecurityQuestionRepository _questions;
        private readonly ICustomerRepository _customerRepository;
        private readonly IUsersRepository _users;
        private readonly LoanPaymentFacade _facade;
        private readonly PaymentRolloverRepository _paymentRolloverRepository;
        private ChangeLoanDetailsModelBuilder _changeLoanDetailsModelBuilder;

        public CustomerModelBuilder(ISecurityQuestionRepository questions, ICustomerRepository customerRepository, IUsersRepository users, PaymentRolloverRepository paymentRolloverRepository)
        {
            _questions = questions;
            _customerRepository = customerRepository;
            _users = users;
            _paymentRolloverRepository = paymentRolloverRepository;
            _facade = new LoanPaymentFacade();
            _changeLoanDetailsModelBuilder = new ChangeLoanDetailsModelBuilder();
        }

        public CustomerModel BuildWizardModel(EZBob.DatabaseLib.Model.Database.Customer cus)
        {
            var customerdModel = new CustomerModel() {loggedIn = cus != null, bankAccountAdded = false};

            if (!customerdModel.loggedIn) return customerdModel;

            var customer = _customerRepository.GetAndInitialize(cus.Id);
            var user = _users.Get(cus.Id);

            if (customer == null) return customerdModel;

            customerdModel.Id = customer.Id;
            customerdModel.userName = user.Name;
            customerdModel.Email = customer.Name;
            customerdModel.EmailState = customer.EmailState.ToString();

            customerdModel.CustomerPersonalInfo = customer.PersonalInfo;
            customerdModel.BusinessTypeReduced = customerdModel.CustomerPersonalInfo == null ?
                TypeOfBusinessReduced.Personal.ToString() : customer.PersonalInfo.TypeOfBusiness.Reduce().ToString();

            customerdModel.bankAccountAdded = customer.HasBankAccount;
            if (customer.HasBankAccount)
            {
                customerdModel.BankAccountNumber = customer.BankAccount.AccountNumber;
                customerdModel.SortCode = customer.BankAccount.SortCode;
            }

            customerdModel.ebayMarketPlaces = customer.GetEbayMarketPlaces();
			customerdModel.amazonMarketPlaces = customer.GetAmazonMarketPlaces();
			customerdModel.ekmShops = customer.GetEkmShops();
			customerdModel.freeAgentAccounts = customer.GetFreeAgentAccounts();
			customerdModel.sageAccounts = customer.GetSageAccounts();
			customerdModel.payPointAccounts = customer.GetPayPointAccounts();
			customerdModel.yodleeAccounts = customer.GetYodleeAccounts();
			customerdModel.cgShops = customer.GetChannelGrabberShops();
            customerdModel.paypalAccounts = customer.GetPayPalAccountsSimple();

			customerdModel.LastApprovedLoanTypeID = customer.LastCashRequest != null ? customer.LastCashRequest.LoanType.Id : 0;
			customerdModel.LastApprovedRepaymentPeriod = customer.LastCashRequest != null ? customer.LastCashRequest.RepaymentPeriod : 0;

            customerdModel.Medal = customer.Medal.HasValue ? customer.Medal.ToString() : "";

            customerdModel.CreditSum = customer.CreditSum;
	        customerdModel.IsLoanTypeSelectionAllowed = customer.IsLoanTypeSelectionAllowed;
            customerdModel.CreditResult = customer.CreditResult.ToString();
            customerdModel.OfferStart = customer.OfferStart;
            customerdModel.OfferValidUntil = customer.OfferValidUntil;
            customerdModel.Status = customer.Status.ToString();

            customerdModel.Loans = customer.Loans
                                                .OrderBy(l => l.Status)
                                                .ThenByDescending(l => l.Date)
                                                .Select(l => LoanModel.FromLoan(l, new PayEarlyCalculator2(l, null), new PayEarlyCalculator2(l, DateTime.UtcNow)))
                                                .ToList();

            customerdModel.TotalBalance = customerdModel.Loans.Sum(l => l.Balance);
            customerdModel.PrincipalBalance = customer.ActiveLoans.Sum(l => l.LoanAmount);
            customerdModel.TotalEarlyPayment = customerdModel.Loans.Sum(l => l.TotalEarlyPayment);
            customerdModel.TotalLatePayment = customerdModel.Loans.Where(l => l.Status == LoanStatus.Late.ToString()).Sum(l => l.Late);

            var nextPayment = (
                                from loan in customer.ActiveLoans
                                from repayment in loan.Schedule
                                where repayment.AmountDue > 0
                                where repayment.Status == LoanScheduleStatus.StillToPay || repayment.Status == LoanScheduleStatus.Late
                                orderby repayment.Date
                                select repayment).FirstOrDefault();
            if (nextPayment != null)
            {
                customerdModel.NextPayment = nextPayment.AmountDue;
                customerdModel.NextPaymentDate = nextPayment.Date;
            }

            customerdModel.TotalPayEarlySavings = _facade.CalculateSavings(customer, DateTime.UtcNow);

            var account = new AccountSettingsModel();

            account.SecurityQuestions = _questions.GetQuestions();

            account.SecurityQuestionModel = new SecurityQuestionModel
            {
                Question = user.SecurityQuestion == null ? 0 : user.SecurityQuestion.Id,
                Answer = user.SecurityAnswer
            };

            customerdModel.AccountSettings = account;

            var payments = from loan in customer.Loans
                           from tran in loan.Transactions
                           where tran is PaypointTransaction
                           orderby tran.PostDate descending
                           select tran;
            var lastPayment = payments.OfType<PaypointTransaction>().FirstOrDefault();

            if(lastPayment != null)
            {
                customerdModel.LastPaymentTotal = lastPayment.Amount;
                customerdModel.LastPaymentPrincipal = lastPayment.Principal;
                customerdModel.LastPaymentInterest = lastPayment.Interest;
                customerdModel.LastPaymentFees = lastPayment.Fees;
            }

            customerdModel.GreetingMailSentDate = customer.GreetingMailSentDate;

            customerdModel.LimitedInfo = LimitedInfoMap.FromLimitedInfo(customer.LimitedInfo);

            customerdModel.NonLimitedInfo = NonLimitedInfoMap.FromLimitedInfo(customer.NonLimitedInfo);

            if (customer.AddressInfo != null)
            {
                customerdModel.PersonalAddress = customer.AddressInfo.PersonalAddress.ToArray();
                customerdModel.LimitedAddress = customer.AddressInfo.LimitedCompanyAddress.ToArray();
                customerdModel.NonLimitedAddress = customer.AddressInfo.NonLimitedCompanyAddress.ToArray();
            }               

            customerdModel.ApplyCount = customer.ApplyCount;
            customerdModel.CreditCardNo = customer.CreditCardNo;
            customerdModel.PayPointCards = FillPayPointCards(customer);

            customerdModel.ActiveRollovers = _paymentRolloverRepository
                .GetRolloversForCustomer(customer.Id)
                .Where(x => x.Status == RolloverStatus.New)
                .Select(x => new RolloverModel
                        {
                            Created = x.Created,
                            CreatorName = x.CreatorName,
                            CustomerConfirmationDate = x.CustomerConfirmationDate,
                            ExpiryDate = x.ExpiryDate,
                            Id = x.Id,
                            LoanScheduleId = x.LoanSchedule.Id,
                            PaidPaymentAmount = x.PaidPaymentAmount,
                            Payment = x.Payment,
                            PaymentDueDate = x.PaymentDueDate,
                            PaymentNewDate = x.PaymentNewDate,
                            Status = x.Status,
                            LoanId = x.LoanSchedule.Loan.Id,
                            RolloverPayValue = GetRolloverPayValue(x.LoanSchedule.Loan)
                        });

            customerdModel.CollectionStatus = customer.CollectionStatus;
            customerdModel.HasRollovers = customerdModel.ActiveRollovers.Any();

            var cr = cus.LastCashRequest;
            customerdModel.IsLoanDetailsFixed = !_changeLoanDetailsModelBuilder.IsAmountChangingAllowed(cr);

	        customerdModel.LoyaltyPoints = customer.LoyaltyPoints();

            return customerdModel;
        }

        private decimal GetRolloverPayValue(Loan loan)
        {
            var payEarlyCalc = new PayEarlyCalculator2(loan, DateTime.UtcNow);
            var state = payEarlyCalc.GetState();

            return state.Fees + state.Interest;
        }

        private PayPointCardModel[] FillPayPointCards(EZBob.DatabaseLib.Model.Database.Customer customer)
        {
            //Add paypoint cards for old customers
            if (!string.IsNullOrEmpty(customer.PayPointTransactionId) && !customer.PayPointCards.Any())
            {
                customer.TryAddPayPointCard(customer.PayPointTransactionId, customer.CreditCardNo, null, customer.PersonalInfo.Fullname);
            }

            return customer.PayPointCards.Select(PayPointCardModel.FromCard).OrderByDescending(x=>x.IsDefault).ToArray();
        }
    }
}