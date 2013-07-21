﻿namespace EzBob.Web.Code
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using Configuration;
	using EZBob.DatabaseLib.Model;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Loans;
	using MailApi;
	using StructureMap;
	using Web.ApplicationCreator;
	using Areas.Customer.Controllers;
	using Areas.Customer.Controllers.Exceptions;
	using Agreements;
	using Infrastructure;
	using PaymentServices.Calculators;
	using PaymentServices.PacNet;
	using ZohoCRM;
	using log4net;
	using EZBob.DatabaseLib;

	public interface ILoanCreator
    {
        Loan CreateLoan(Customer cus, decimal loanAmount, PayPointCard card, DateTime now);
    }

    public class LoanCreator : ILoanCreator
    {
        private readonly ILoanHistoryRepository _loanHistoryRepository;
        private readonly IPacnetService _pacnetService;
        private readonly IAppCreator _appCreator;
        private readonly IZohoFacade _crm;
        private readonly IAgreementsGenerator _agreementsGenerator;
        private readonly IEzbobWorkplaceContext _context;
		private readonly LoanBuilder _loanBuilder;
		private readonly PacNetBalanceRepository _funds;
		private readonly PacNetManualBalanceRepository _manualFunds;
		private static readonly IEzBobConfiguration config = ObjectFactory.GetInstance<IEzBobConfiguration>();
		private Mail _mail;

        private static readonly ILog Log = LogManager.GetLogger(typeof (LoanCreator));

        public LoanCreator(
            ILoanHistoryRepository loanHistoryRepository, 
            IPacnetService pacnetService, 
            IAppCreator appCreator, 
            IZohoFacade crm, 
            IAgreementsGenerator agreementsGenerator, 
            IEzbobWorkplaceContext context, 
            LoanBuilder loanBuilder)
        {
            _loanHistoryRepository = loanHistoryRepository;
            _pacnetService = pacnetService;
            _appCreator = appCreator;
            _crm = crm;
            _agreementsGenerator = agreementsGenerator;
            _context = context;
            _loanBuilder = loanBuilder;

			_funds = ObjectFactory.GetInstance<PacNetBalanceRepository>();
			_manualFunds = ObjectFactory.GetInstance<PacNetManualBalanceRepository>();
			_mail = new Mail(new MandrillConfig());
        }

        public Loan CreateLoan(Customer cus, decimal loanAmount, PayPointCard card, DateTime now)
        {
            ValidateCustomer(cus);

            ValidateAmount(loanAmount, cus);

            ValidateOffer(cus);

            ValidateLoanDelay(cus, now, TimeSpan.FromMinutes(1));

            var fee = 0M;

            var cr = cus.LastCashRequest;

            if (!cr.HasLoans && cr.UseSetupFee)
            {
                var calculator = new SetupFeeCalculator();
                fee = calculator.Calculate(loanAmount);
            }

            var transfered = loanAmount - fee;

            var ret = SendMoney(cus, transfered);

            cr.HasLoans = true;

            var loan = _loanBuilder.CreateLoan(cr, loanAmount, now);

            loan.Customer = cus;
            loan.Status = LoanStatus.Live;
            loan.CashRequest = cr;
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

            var aprCalc = new APRCalculator();
            loan.APR = (decimal)aprCalc.Calculate(loanAmount, loan.Schedule, fee, now);

            cus.AddLoan(loan);
            cus.FirstLoanDate = cus.Loans.Min(x=>x.Date);
            cus.LastLoanDate = cus.Loans.Max(x=>x.Date);
            cus.LastLoanAmount = cus.Loans.First(x => x.Date == cus.LastLoanDate).LoanAmount;
            cus.AmountTaken = cus.Loans.Sum(x => x.LoanAmount);
            cus.CreditSum = cus.CreditSum - loanAmount;

            if (fee > 0) cus.SetupFee = fee;

            _loanHistoryRepository.Save(new LoanHistory(loan, now));

            _appCreator.CashTransfered(_context.User, cus.PersonalInfo.FirstName, transfered, fee);

            _agreementsGenerator.RenderAgreements(loan, true);

            _crm.CreateLoan(cus, loan);

            return loan;
        }

        private PacnetReturnData SendMoney(Customer cus, decimal transfered)
        {
            var name = GetCustomerNameForPacNet(cus);

            if (!cus.HasBankAccount)
            {
                throw new Exception("bank account should be added in order to send money");
            }

            var ret = _pacnetService.SendMoney(cus.Id, transfered, cus.BankAccount.SortCode,
                                               cus.BankAccount.AccountNumber, name, "ezbob", "GBP", "EZBOB");
            _pacnetService.CloseFile(cus.Id, "ezbob");

	        VerifyAvailableFunds();

            return ret;
        }

	    private void VerifyAvailableFunds()
	    {
		    try
		    {
				var balance = _funds.GetBalance();
				var manualBalance = _manualFunds.GetBalance();
				var fundsAvailable = balance.Adjusted + manualBalance;

				DateTime today = DateTime.UtcNow;
				int relevantLimit = (today.DayOfWeek == DayOfWeek.Thursday || today.DayOfWeek == DayOfWeek.Friday) ? config.PacnetBalanceWeekendLimit : config.PacnetBalanceWeekdayLimit;
				if (fundsAvailable < relevantLimit)
				{
					SendMail(fundsAvailable, relevantLimit);
				}
		    }
		    catch (Exception e)
		    {
				Log.ErrorFormat("Failed verifying available funds with error:{0}", e);
		    }
	    }

	    private void SendMail(decimal currentFunds, int requiredFunds)
		{
			var vars = new Dictionary<string, string>
				{
					{"CurrentFunds", currentFunds.ToString("N2", CultureInfo.InvariantCulture)},
					{"RequiredFunds", requiredFunds.ToString("N", CultureInfo.InvariantCulture)} 
				};

			var result = _mail.Send(vars, config.NotEnoughFundsToAddess, config.NotEnoughFundsTemplateName);
			if (result == "OK")
			{
				Log.InfoFormat("Sent mail - not enough funds");
			}
			else
			{
				Log.ErrorFormat("Failed sending alert mail - not enough funds. Result:{0}", result);
			}
		}

        public virtual void ValidateLoanDelay(Customer customer, DateTime now, TimeSpan period)
        {
            var lastLoan = customer.Loans.OrderByDescending(l => l.Date).FirstOrDefault();
            if (lastLoan == null) return;
            var diff = now.Subtract(lastLoan.Date);
            if (diff < period)
            {
                var msg = string.Format("Please try again in {0} seconds.", (int)(period.TotalSeconds - diff.TotalSeconds));
                Log.Error(msg);
                throw new LoanDelayViolationException(msg);
            }
        }

        public virtual void ValidateOffer(Customer cus)
        {
            cus.ValidateOfferDate();
        }

        public virtual void ValidateCustomer(Customer cus)
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

        public virtual void ValidateAmount(decimal loanAmount, Customer customer)
        {
            if (loanAmount <= 0)
            {
                throw new ArgumentException();
            }

            if (customer.CreditSum < loanAmount)
            {
                throw new ArgumentException();
            }
        }


        public virtual string GetCustomerNameForPacNet(Customer customer)
        {
            string name = string.Format("{0} {1}", customer.PersonalInfo.FirstName, customer.PersonalInfo.Surname);

            if (name.Length > 18)
            {
                name = customer.PersonalInfo.Surname;
            }

            if (name.Length > 18)
            {
                name = name.Substring(0, 17);
            }

            return name;
        }
    }
}