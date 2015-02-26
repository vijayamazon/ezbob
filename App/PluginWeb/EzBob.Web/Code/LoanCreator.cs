namespace EzBob.Web.Code {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Loans;
	using Ezbob.Backend.Models;
	using NHibernate;
	using Areas.Customer.Controllers;
	using Areas.Customer.Controllers.Exceptions;
	using Agreements;
	using Ezbob.Logger;
	using Infrastructure;
	using PaymentServices.Calculators;
	using PaymentServices.PacNet;
	using SalesForceLib.Models;
	using ServiceClientProxy;
	using StructureMap;

	public interface ILoanCreator {
		Loan CreateLoan(Customer cus, decimal loanAmount, PayPointCard card, DateTime now);
	} // interface ILoanCreator

	public class LoanCreator : ILoanCreator {
		public LoanCreator(
			IPacnetService pacnetService,
			IAgreementsGenerator agreementsGenerator,
			IEzbobWorkplaceContext context,
			LoanBuilder loanBuilder,
			ISession session
		) {
			_pacnetService = pacnetService;
			m_oServiceClient = new ServiceClient();
			_agreementsGenerator = agreementsGenerator;
			_context = context;
			_loanBuilder = loanBuilder;
			_session = session;
			m_oTranMethodRepo = ObjectFactory.GetInstance<DatabaseDataHelper>().LoanTransactionMethodRepository;
		} // constructor

		public Loan CreateLoan(Customer cus, decimal loanAmount, PayPointCard card, DateTime now) {
			ValidateCustomer(cus);
			ValidateAmount(loanAmount, cus);
			ValidateOffer(cus);
			ValidateLoanDelay(cus, now, TimeSpan.FromMinutes(1));

			bool isFakeLoanCreate = (card == null);
			bool isEverlineRefinance = ValidateEverlineRefinance(cus);
			var cr = cus.LastCashRequest;

			var calculator = new SetupFeeCalculator(
				cr.UseSetupFee,
				cr.UseBrokerSetupFee,
				cr.ManualSetupFeeAmount,
				cr.ManualSetupFeePercent
			);

			var fee = calculator.Calculate(loanAmount);

			var transfered = loanAmount - fee;

			PacnetReturnData ret;

			if (PacnetSafeGuard(cus, transfered)) {
				if (!isFakeLoanCreate && !cus.IsAlibaba && !isEverlineRefinance) {
					ret = SendMoney(cus, transfered);
					VerifyAvailableFunds(transfered);
				} else {
					Log.Debug("Not sending money via pacnet. isFake: {0}, isAlibaba: {1}, isEverlineRefinance: {2}", isFakeLoanCreate, cus.IsAlibaba, isEverlineRefinance);
					ret = new PacnetReturnData {
						Status = "Done",
						TrackingNumber = "fake"
					};

					if (isEverlineRefinance) {
						SendEverlineRefinanceMails(cus.Id, cus.Name, now, transfered);
					}
				}
			} else {
				Log.Error("PacnetSafeGuard stopped money transfer");
				throw new Exception("PacnetSafeGuard stopped money transfer");
			} // if

			cr.HasLoans = true;

			var loan = _loanBuilder.CreateLoan(cr, loanAmount, now);

			loan.Customer = cus;
			loan.Status = LoanStatus.Live;
			loan.CashRequest = cr;
			loan.LoanType = cr.LoanType;

			loan.GenerateRefNumber(cus.RefNumber, cus.Loans.Count);

			PacnetTransaction loanTransaction;
			if (!cus.IsAlibaba) {
				loanTransaction = new PacnetTransaction {
					Amount = loan.LoanAmount,
					Description = "Ezbob " + FormattingUtils.FormatDateToString(DateTime.Now),
					PostDate = now,
					Status = isFakeLoanCreate ? LoanTransactionStatus.Done : LoanTransactionStatus.InProgress,
					TrackingNumber = ret.TrackingNumber,
					PacnetStatus = ret.Status,
					Fees = fee,
					LoanTransactionMethod = m_oTranMethodRepo.FindOrDefault("Pacnet"),
				};
			} else {
				loanTransaction = new PacnetTransaction {
					Amount = loan.LoanAmount,
					Description = "Ezbob " + FormattingUtils.FormatDateToString(DateTime.Now),
					PostDate = now,
					Status = LoanTransactionStatus.Done,
					TrackingNumber = "alibaba", //TODO save who got the money
					PacnetStatus = ret.Status,
					Fees = fee,
					LoanTransactionMethod = m_oTranMethodRepo.FindOrDefault("Manual"),
				};
				Log.Debug("Alibaba loan, adding manual pacnet transaction to loan schedule");
			} // if

			loan.AddTransaction(loanTransaction);

			var aprCalc = new APRCalculator();
			loan.APR = (decimal)aprCalc.Calculate(loanAmount, loan.Schedule, fee, now);

			cus.AddLoan(loan);
			cus.FirstLoanDate = cus.Loans.Min(x => x.Date);
			cus.LastLoanDate = cus.Loans.Max(x => x.Date);
			cus.LastLoanAmount = cus.Loans.First(x => x.Date == cus.LastLoanDate).LoanAmount;
			cus.AmountTaken = cus.Loans.Sum(x => x.LoanAmount);
			cus.CreditSum = cus.CreditSum - loanAmount;

			if (fee > 0) cus.SetupFee = fee;

			_agreementsGenerator.RenderAgreements(loan, true);

			var loanHistoryRepository = new LoanHistoryRepository(_session);
			loanHistoryRepository.SaveOrUpdate(new LoanHistory(loan, now));

			m_oServiceClient.Instance.SalesForceUpdateOpportunity(cus.Id, cus.Id, new ServiceClientProxy.EzServiceReference.OpportunityModel {
				Email = cus.Name,
				CloseDate = now,
				TookAmount = (int)loan.LoanAmount,
				DealCloseType = OpportunityDealCloseReason.Won.ToString()
			});

			_session.Flush();

			if (!isFakeLoanCreate)
				m_oServiceClient.Instance.CashTransferred(cus.Id, transfered, loan.RefNumber, cus.Loans.Count() == 1);

			return loan;
		}

		private void SendEverlineRefinanceMails(int customerId, string customerName, DateTime now, decimal amount) {
			m_oServiceClient.Instance.SendEverlineRefinanceMails(customerId, customerName, now, amount);
		}

		private bool ValidateEverlineRefinance(Customer cus) {
			if (cus.CustomerOrigin.Name == "everline" && !cus.Loans.Any()) {
				EverlineLoginLoanChecker checker = new EverlineLoginLoanChecker();
				var status = checker.GetLoginStatus(cus.Name);
				return (status.status == EverlineLoanStatus.ExistsWithCurrentLiveLoan);
			}
			return false;
		}

// CreateLoan

		/// <summary>
		/// not yet implemented function that is
		/// going to check last minute details of money transfer to prevent some 
		/// hackers’ attacks and fraud.
		/// </summary>
		/// <returns>
		/// true if every thing is ok with this money transfer, else false 
		/// currently always true
		/// </returns>
		[SuppressMessage("ReSharper", "UnusedParameter.Local")]
		private bool PacnetSafeGuard(Customer cus, decimal transfered) {
			return true;
		} // PacnetSafeGuard

		private PacnetReturnData SendMoney(Customer cus, decimal transfered) {
			var name = GetCustomerNameForPacNet(cus);

			if (!cus.HasBankAccount)
				throw new Exception("bank account should be added in order to send money");

			Log.Debug(
				"SendMoney customerId: {0}, amount: {1}, sortcode: {2}, account number: {3}, name: {4}, ezbob GBP EZBOB",
				cus.Id,
				transfered,
				cus.BankAccount.SortCode,
				cus.BankAccount.AccountNumber,
				name
			);

			var sendResponse = _pacnetService.SendMoney(
				cus.Id,
				transfered,
				cus.BankAccount.SortCode,
				cus.BankAccount.AccountNumber,
				name,
				"ezbob",
				"GBP",
				"EZBOB" //TODO rename to orange money
			);

			Log.Debug(
				"SendMoney response: tracking: {0}, status {1} {2} ",
				sendResponse.TrackingNumber,
				sendResponse.Status,
				sendResponse.HasError ? ",Error: " + sendResponse.Error : ""
			);

			var closeResponse = _pacnetService.CloseFile(cus.Id, "ezbob");

			Log.Debug(
				"CloseFile response: tracking: {0}, status {1} {2} ",
				closeResponse.TrackingNumber,
				closeResponse.Status,
				closeResponse.HasError ? ",Error: " + closeResponse.Error : ""
			);

			return sendResponse;
		} // SendMoney

		public virtual void ValidateLoanDelay(Customer customer, DateTime now, TimeSpan period) {
			var lastLoan = customer.Loans.OrderByDescending(l => l.Date).FirstOrDefault();

			if (lastLoan == null)
				return;

			var diff = now.Subtract(lastLoan.Date);

			if (diff < period) {
				var msg = string.Format("Please try again in {0} seconds.", (int)(period.TotalSeconds - diff.TotalSeconds));
				Log.Error(msg);
				throw new LoanDelayViolationException(msg);
			} // if
		} // ValidateLoanDelay

		public virtual void ValidateOffer(Customer cus) {
			cus.ValidateOfferDate();
		} // ValidateOffer

		public virtual void VerifyAvailableFunds(decimal transfered) {
			m_oServiceClient.Instance.VerifyEnoughAvailableFunds(_context.UserId, transfered);
		} // VerifyAvailableFunds

		public virtual void ValidateCustomer(Customer cus) {
			if (cus == null || cus.PersonalInfo == null || cus.BankAccount == null)
				throw new CustomerIsNotFullyRegisteredException();

			if (cus.Status != Status.Approved)
				throw new CustomerIsNotApprovedException();

			if (!cus.WizardStep.TheLastOne)
				throw new CustomerIsNotFullyRegisteredException();

			if (!cus.CreditSum.HasValue || !cus.CollectionStatus.CurrentStatus.IsEnabled)
				throw new Exception("Invalid customer state");
		} // ValidateCustomer

		public virtual void ValidateAmount(decimal loanAmount, Customer customer) {
			if (loanAmount <= 0)
				throw new ArgumentException();

			if (customer.CreditSum < loanAmount)
				throw new ArgumentException();
		} // ValidateAmount

		public virtual string GetCustomerNameForPacNet(Customer customer) {
			string name = string.Format("{0} {1}", customer.PersonalInfo.FirstName, customer.PersonalInfo.Surname);

			if (name.Length > 18)
				name = customer.PersonalInfo.Surname;

			if (name.Length > 18)
				name = name.Substring(0, 17);

			return name;
		} // GetCustomerNameForPacNet

		private readonly IPacnetService _pacnetService;
		private readonly ServiceClient m_oServiceClient;
		private readonly IAgreementsGenerator _agreementsGenerator;
		private readonly IEzbobWorkplaceContext _context;
		private readonly LoanBuilder _loanBuilder;
		private readonly ISession _session;
		private readonly LoanTransactionMethodRepository m_oTranMethodRepo;

		private static readonly ASafeLog Log = new SafeILog(typeof(LoanCreator));
	} // class LoanCreator
} // namespace
