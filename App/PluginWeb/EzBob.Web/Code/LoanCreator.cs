using System;
using System.Globalization;
using EZBob.DatabaseLib.Model;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Loans;
using EZBob.DatabaseLib.Model.Loans;
using EzBob.Web.ApplicationCreator;
using EzBob.Web.Areas.Customer.Controllers.Exceptions;
using EzBob.Web.Areas.Underwriter.Controllers;
using EzBob.Web.Code;
using EzBob.Web.Code.Agreements;
using EzBob.Web.Infrastructure;
using PaymentServices.Calculators;
using PaymentServices.PacNet;
using ZohoCRM;

namespace EzBob.Web.Areas.Customer.Controllers
{
    public interface ILoanCreator
    {
        Loan CreateLoan(EZBob.DatabaseLib.Model.Database.Customer cus, decimal loan_amount, PayPointCard card, DateTime now);
    }

    public class LoanCreator : ILoanCreator
    {
        private readonly IPacnetPaypointServiceLogRepository _logRepository;
        private readonly ILoanHistoryRepository _loanHistoryRepository;
        private readonly IPacnetService _pacnetService;
        private readonly IAppCreator _appCreator;
        private readonly IZohoFacade _crm;
        private readonly IAgreementsGenerator _agreementsGenerator;
        private readonly IEzbobWorkplaceContext _context;
        private readonly LoanBuilder _loanBuilder;

        public LoanCreator(IPacnetPaypointServiceLogRepository logRepository, ILoanHistoryRepository loanHistoryRepository, IPacnetService pacnetService, IAppCreator appCreator, IZohoFacade crm, IAgreementsGenerator agreementsGenerator, IEzbobWorkplaceContext context, LoanBuilder loanBuilder)
        {
            _logRepository = logRepository;
            _loanHistoryRepository = loanHistoryRepository;
            _pacnetService = pacnetService;
            _appCreator = appCreator;
            _crm = crm;
            _agreementsGenerator = agreementsGenerator;
            _context = context;
            _loanBuilder = loanBuilder;
        }

        public Loan CreateLoan(EZBob.DatabaseLib.Model.Database.Customer cus, decimal loan_amount, PayPointCard card, DateTime now)
        {
            ValidateCustomer(cus);

            ValidateAmount(loan_amount, cus);

            ValidateOffer(cus);

            var fee = 0M;

            var cr = cus.LastCashRequest;

            if (!cr.HasLoans && cr.UseSetupFee)
            {
                var calculator = new SetupFeeCalculator();
                fee = calculator.Calculate(loan_amount);
            }

            var transfered = loan_amount - fee;

            var name = GetCustomerNameForPacNet(cus);
            string description = string.Format("EZBOB");

            var ret = _pacnetService.SendMoney(cus.Id, transfered, cus.BankAccount.SortCode,
                                                                  cus.BankAccount.AccountNumber, name, "ezbob", "GBP", description);
            _pacnetService.CloseFile(cus.Id, "ezbob");

            cr.HasLoans = true;

            var loan = _loanBuilder.CreateLoan(cr, loan_amount, now);

            loan.Customer = cus;
            loan.Status = LoanStatus.Live;
            loan.CashRequest = cr;
            loan.PayPointCard = card;
            loan.LoanType = cr.LoanType;

            loan.GenerateRefNumber(cus.RefNumber, cus.Loans.Count);

            var loanTransaction = new PacnetTransaction
                                      {
                                          Amount = loan.LoanAmount,
                                          Description = "Ezbob " + FormattingUtils.FormatDateToString(DateTime.Now),
                                          PostDate = now,
                                          Status = LoanTransactionStatus.InProgress,
                                          TrackingNumber = ret.TrackingNumber,
                                          PacnetStatus = ret.Status,
                                          Fees = fee
                                      };
            loan.AddTransaction(loanTransaction);

            try
            {
                var aprCalc = new APRCalculator();
                loan.APR = (decimal)aprCalc.Calculate(loan_amount, loan.Schedule, fee, now);
            }
            catch (Exception e)
            {
                //apr sometimes throws overflow exceptions
            }

            cus.AddLoan(loan);
            cus.CreditSum = cus.CreditSum - loan_amount;
            if (fee > 0) cus.SetupFee = fee;

            _loanHistoryRepository.Save(new LoanHistory(loan, now));

            _appCreator.CashTransfered(_context.User, cus.PersonalInfo.FirstName, transfered, fee);

            _agreementsGenerator.RenderAgreements(loan, true);

            _crm.CreateLoan(cus, loan);

            return loan;
        }

        public virtual void ValidateOffer(EZBob.DatabaseLib.Model.Database.Customer cus)
        {
            cus.ValidateOfferDate();
        }

        public virtual void ValidateCustomer(EZBob.DatabaseLib.Model.Database.Customer cus)
        {
            if (cus == null || cus.PersonalInfo == null || cus.BankAccount == null)
            {
                throw new CustomerIsNotFullyRegisteredException();
            }

            if (cus.Status != Status.Approved)
            {
                throw new CustomerIsNotApprovedException();
            }

            if(!cus.IsSuccessfullyRegistered)
            {
                throw  new CustomerIsNotFullyRegisteredException();
            }

            if (
                !cus.CreditSum.HasValue ||
                !cus.Status.HasValue ||
                cus.Status.Value != Status.Approved ||
                cus.CollectionStatus.CurrentStatus != CollectionStatusType.Enabled)
            {
                throw new Exception("Invalid customer state");
            }

        }

        public virtual void ValidateAmount(decimal loanAmount, EZBob.DatabaseLib.Model.Database.Customer cus)
        {
            if (loanAmount <= 0)
            {
                throw new ArgumentException();
            }

            if (cus.CreditSum < loanAmount)
            {
                throw new ArgumentException();
            }
        }


        public virtual string GetCustomerNameForPacNet(EZBob.DatabaseLib.Model.Database.Customer cus)
        {
            string name = string.Format("{0} {1}", cus.PersonalInfo.FirstName, cus.PersonalInfo.Surname);

            if (name.Length > 18)
            {
                name = cus.PersonalInfo.Surname;
            }

            if (name.Length > 18)
            {
                name = name.Substring(0, 17);
            }

            return name;
        }

    }
}