namespace EzBob.Web.Code {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Logger;
	using Ezbob.Utils.Extensions;
	using EzBob.Models.Agreements;
	using EzBob.Web.Areas.Customer.Controllers;
	using EzBob.Web.Areas.Customer.Controllers.Exceptions;
	using EzBob.Web.Infrastructure;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Loans;
	using NHibernate;
	using PaymentServices.Calculators;
	using PaymentServices.PacNet;
	using SalesForceLib.Models;
	using ServiceClientProxy;
	using StructureMap;

	public interface ILoanCreator {
		Loan CreateLoan(Customer cus, decimal loanAmount, PayPointCard card, DateTime now, NL_Model nlModel = null);
	} // interface ILoanCreator

	public class LoanCreator : ILoanCreator {
		public LoanCreator(
			IPacnetService pacnetService,
			IAgreementsGenerator agreementsGenerator,
			IEzbobWorkplaceContext context,
			LoanBuilder loanBuilder,
			ISession session
		) {
			this.pacnetService = pacnetService;
			this.serviceClient = new ServiceClient();
			this.agreementsGenerator = agreementsGenerator;
			this.context = context;
			this.loanBuilder = loanBuilder;
			this.session = session;
			this.tranMethodRepo = ObjectFactory.GetInstance<DatabaseDataHelper>().LoanTransactionMethodRepository;
		} // constructor

		/// <exception cref="Exception">PacnetSafeGuard stopped money transfer</exception>
		/// <exception cref="OverflowException"><paramref /> is less than <see cref="F:System.TimeSpan.MinValue" /> or greater than <see cref="F:System.TimeSpan.MaxValue" />.-or-<paramref /> is <see cref="F:System.Double.PositiveInfinity" />.-or-<paramref name="value" /> is <see cref="F:System.Double.NegativeInfinity" />. </exception>
		/// <exception cref="LoanDelayViolationException">Condition. </exception>
		public Loan CreateLoan(Customer cus, decimal loanAmount, PayPointCard card, DateTime now, Ezbob.Backend.ModelsWithDB.NewLoan.NL_Model nlModel = null) {
			ValidateCustomer(cus); // continue (customer's data/status, finish wizard, bank account data)
			ValidateAmount(loanAmount, cus); // continue (loanAmount > customer.CreditSum)
			ValidateOffer(cus); // check offer validity dates - in AddLoan strategy
			ValidateLoanDelay(cus, now, TimeSpan.FromMinutes(1)); // checks if last loan was taken a minute before "now" - ?? to prevent multiple clicking on "create loan" button?
			ValidateRepaymentPeriodAndInterestRate(cus);

			bool isFakeLoanCreate = (card == null);
			bool isEverlineRefinance = ValidateEverlineRefinance(cus); // NL should also treat it???
			var cr = cus.LastCashRequest;

			Loan loan = this.loanBuilder.CreateLoan(cr, loanAmount, now);

			var transfered = loan.LoanAmount - loan.SetupFee;

			PacnetReturnData ret;

			if (PacnetSafeGuard(cus, transfered)) {
				if (!isFakeLoanCreate && !cus.IsAlibaba && !isEverlineRefinance) {
					ret = SendMoney(cus, transfered);
					VerifyAvailableFunds(transfered);

				} else {
					log.Debug(
						"Not sending money via Pacnet. isFake: {0}, isAlibaba: {1}, isEverlineRefinance: {2}.",
						isFakeLoanCreate,
						cus.IsAlibaba,
						isEverlineRefinance
					);

					ret = new PacnetReturnData {
						Status = "Done",
						TrackingNumber = "fake"
					};

					if (isEverlineRefinance) {
						this.serviceClient.Instance.SendEverlineRefinanceMails(
							cus.Id,
							cus.Name,
							now,
							loanAmount,
							transfered
						);
					} // if
				} // if
			} else {
				log.Error("PacnetSafeGuard stopped money transfer");
				// ReSharper disable once ThrowingSystemException
				throw new Exception("PacnetSafeGuard stopped money transfer");
			} // if

			cr.HasLoans = true;

			loan.Customer = cus;
			loan.Status = LoanStatus.Live;
			loan.CashRequest = cr;
			loan.LoanType = cr.LoanType;

			loan.GenerateRefNumber(cus.RefNumber, cus.Loans.Count);

			if (nlModel == null)
				nlModel = new NL_Model(cus.Id);

			nlModel.UserID = this.context.UserId;

			// NL fund transfer
			nlModel.FundTransfer = new NL_FundTransfers() {
				Amount = loanAmount, // logic transaction - full amount
				TransferTime = now,
				FundTransferStatusID = (int)NLFundTransferStatuses.Pending, //  (int)NLPacnetTransactionStatuses.InProgress,
				LoanTransactionMethodID = (int)NLLoanTransactionMethods.Pacnet,
				PacnetTransactions = new List<NL_PacnetTransactions>()
			};

			PacnetTransaction loanTransaction;
			if (!cus.IsAlibaba) {

				loanTransaction = new PacnetTransaction {
					Amount = loan.LoanAmount,
					Description = "Ezbob " + FormattingUtils.FormatDateToString(DateTime.Now),
					PostDate = now,
					Status = (isFakeLoanCreate || isEverlineRefinance) ? LoanTransactionStatus.Done : LoanTransactionStatus.InProgress,
					TrackingNumber = ret.TrackingNumber,
					PacnetStatus = ret.Status,
					Fees = loan.SetupFee,
					LoanTransactionMethod = this.tranMethodRepo.FindOrDefault("Pacnet"),
				};

				// NL pacnet transaction
				nlModel.FundTransfer.PacnetTransactions.Add(new NL_PacnetTransactions() {
					TransactionTime = now,
					Amount = loanAmount,
					Notes = "Ezbob " + FormattingUtils.FormatDateToString(DateTime.Now) + " Status: " + ret.Status + " Err:" + ret.Error,
					StatusUpdatedTime = DateTime.UtcNow,
					TrackingNumber = ret.TrackingNumber,
					PacnetTransactionStatusID = (isFakeLoanCreate || isEverlineRefinance) ? (int)NLPacnetTransactionStatuses.Done : (int)NLPacnetTransactionStatuses.InProgress,
				});

			} else {
				// alibaba 
				loanTransaction = new PacnetTransaction {
					Amount = loan.LoanAmount,
					Description = "Ezbob " + FormattingUtils.FormatDateToString(DateTime.Now),
					PostDate = now,
					Status = LoanTransactionStatus.Done,
					TrackingNumber = "alibaba deal. CustomerID: " + cus.Id, // TODO save who got the money
					PacnetStatus = ret.Status,
					Fees = loan.SetupFee,
					LoanTransactionMethod = this.tranMethodRepo.FindOrDefault("Manual")
				};

				log.Debug("Alibaba loan, adding manual pacnet transaction to loan schedule");

				// NL: only logic transaction created in "alibaba" case; real money transfer will be done later, not transferred to customer (alibaba buyer) directly, but to seller (3rd party)
				nlModel.FundTransfer.LoanTransactionMethodID = (int)NLLoanTransactionMethods.Manual;

			} // if

			//  This is the place where the funds transferred to customer saved to DB
			log.Info(
				"Save transferred funds to customer {0} amount {1}, isFake {2} , isAlibaba {3}, isEverlineRefinance {4}",
				cus.Id,
				transfered,
				isFakeLoanCreate,
				cus.IsAlibaba,
				isEverlineRefinance
			);

			loan.AddTransaction(loanTransaction);

			var aprCalc = new APRCalculator();
			loan.APR = (decimal)aprCalc.Calculate(loanAmount, loan.Schedule, loan.SetupFee, now);

			cus.AddLoan(loan);
			cus.FirstLoanDate = cus.Loans.Min(x => x.Date);
			cus.LastLoanDate = cus.Loans.Max(x => x.Date);
			cus.LastLoanAmount = cus.Loans.First(x => x.Date == cus.LastLoanDate).LoanAmount;
			cus.AmountTaken = cus.Loans.Sum(x => x.LoanAmount);
			cus.CreditSum = cus.CreditSum - loanAmount;

			if (loan.SetupFee > 0)
				cus.SetupFee = loan.SetupFee;

			/**
			1. Build/ReBuild agreement model - private AgreementModel GenerateAgreementModel(Customer customer, Loan loan, DateTime now, double apr); in \App\PluginWeb\EzBob.Web\Code\AgreementsModelBuilder.cs
			2. RenderAgreements: loan.Agreements.Add
			3. RenderAgreements: SaveAgreement (file?) \backend\Strategies\Misc\Agreement.cs strategy
			*/

			// NL model - loan. Create history here for agreements processing 
			nlModel.Loan = new NL_Loans();
			nlModel.Loan.Histories.Clear();
			nlModel.Loan.Histories.Add(new NL_LoanHistory() {
				Amount = loanAmount,
				EventTime = now
			});

			// populate nlModel by agreements data also
			this.agreementsGenerator.RenderAgreements(loan, true, nlModel);

			var loanHistoryRepository = new LoanHistoryRepository(this.session);
			loanHistoryRepository.SaveOrUpdate(new LoanHistory(loan, now));


			// This is the place where the loan is created and saved to DB
			log.Info(
				"Create loan for customer {0} cash request {1} amount {2}",
				cus.Id,
				loan.CashRequest.Id,
				loan.LoanAmount
			);

			// actually this is the place where the loan saved to DB
			this.session.Flush();

			Loan oldloan = cus.Loans.First(s => s.RefNumber.Equals(loan.RefNumber));
			nlModel.Loan.OldLoanID = oldloan.Id;
			nlModel.Loan.Refnum = loan.RefNumber; // TODO generate another refnum with new algorithm in addloan strategy

			try {
				// copy newly created agreementtemplateID (for new templeates)
				foreach (NL_LoanAgreements ag in nlModel.Loan.LastHistory().Agreements) {
					if (ag.LoanAgreementTemplateID == 0)
						ag.LoanAgreementTemplateID = oldloan.Agreements.FirstOrDefault(a => a.FilePath.Equals(ag.FilePath)).TemplateRef.Id;
				}
				this.serviceClient.Instance.AddLoan(null, cus.Id, nlModel);
				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
				log.Debug("Failed to save new loan {0}", ex);
			} // try


			if (!isFakeLoanCreate)
				this.serviceClient.Instance.CashTransferred(cus.Id, transfered, loan.RefNumber, cus.Loans.Count() == 1);

			this.serviceClient.Instance.LinkLoanToInvestor(this.context.UserId, cus.Id, loan.Id);
			// verify see above line 45-48
			// 
			// ++++++++
			// \App\PluginWeb\EzBob.Web\Code\LoanBuilder.cs , method public Loan CreateNewLoan(CashRequest cr, decimal amount, DateTime now, int term, int interestOnlyTerm = 0)
			//
			// calculate setupFee
			// calculate CalculateBrokerFee
			// var loanLegal = cr.LoanLegals.LastOrDefault();
			//
			// ---------------------
			// file \Integration\PaymentServices\Calculators\LoanScheduleCalculator.cs, method 
			// public IList<LoanScheduleItem> Calculate(decimal total, Loan loan = null, DateTime? startDate = null, int interestOnlyTerm = 0)
			// 
			// calculate Schedules, but use it only for "fill in loan with total data"
			// GetDiscounts
			// loanType => GetBalances => fill in loan with total data: Interest, LoanAmount; Principal, Balance (loan.LoanAmount loan.Interest); InterestRate; startDate.Value
			// ---------------------
			// 
			// LoanSource
			// brokerFee
			// +++++++++++++++++++++
			//
			// line 85
			// prepare pacnet transaction => add to loan => loan.arp => add loan to cutomer
			// agreement
			// save loan via new LoanHistory (139)
			// SF
			// flush

			this.serviceClient.Instance.SalesForceUpdateOpportunity(
				cus.Id,
				cus.Id,
				new ServiceClientProxy.EzServiceReference.OpportunityModel {
					Email = cus.Name,
					CloseDate = now,
					TookAmount = (int)loan.LoanAmount,
					ApprovedAmount = (int)(cus.CreditSum ?? 0) + (int)loanAmount,
					DealCloseType = OpportunityDealCloseReason.Won.ToString(),
					Origin = cus.CustomerOrigin.Name
				}
			);

			this.serviceClient.Instance.SalesForceAddUpdateLeadAccount(cus.Id, cus.Name, cus.Id, false, false); //update account with new number of loans
			HandleSalesForceTopup(cus, now); //EZ-3908

			return loan;
		}// CreateLoan

		private void HandleSalesForceTopup(Customer cus, DateTime now) {
			if (cus.CreditSum > 1000 && cus.Loans.Count(x => x.Status != LoanStatus.PaidOff) < CurrentValues.Instance.NumofAllowedActiveLoans) {
				var requestedLoan = cus.CustomerRequestedLoan.OrderByDescending(x => x.Id).FirstOrDefault();
				int requestedAmount = requestedLoan != null && requestedLoan.Amount.HasValue ? (int)requestedLoan.Amount.Value : 0;
				this.serviceClient.Instance.SalesForceAddOpportunity(cus.Id, cus.Id, new ServiceClientProxy.EzServiceReference.OpportunityModel {
					Name = cus.PersonalInfo.Fullname + " TopUp",
					CreateDate = now,
					Email = cus.Name,
					Origin = cus.CustomerOrigin.Name,
					ExpectedEndDate = cus.OfferValidUntil,
					RequestedAmount = requestedAmount,
					ApprovedAmount = (int?)cus.CreditSum,
					Stage = OpportunityStage.s90.DescriptionAttr(),
					Type = OpportunityType.Topup.DescriptionAttr()
				});
			}
		}//HandleSalesForceTopup


		/// <exception cref="LoanDelayViolationException">Condition. </exception>
		public virtual void ValidateLoanDelay(Customer customer, DateTime now, TimeSpan period) {
			var lastLoan = customer.Loans.OrderByDescending(l => l.Date).FirstOrDefault();

			if (lastLoan == null)
				return;

			var diff = now.Subtract(lastLoan.Date);

			if (diff < period) {
				var msg = string.Format("Please try again in {0} seconds.", (int)(period.TotalSeconds - diff.TotalSeconds));
				log.Error(msg);
				throw new LoanDelayViolationException(msg);
			} // if
		} // ValidateLoanDelay

		public virtual void ValidateOffer(Customer cus) {
			try {
				cus.ValidateOfferDate();
			} catch {
				log.Warn("ValidateOffer wrong offer date OfferStart {0} OfferValidUntil {1} Now {2} customerid:{3}",
					cus.OfferStart, cus.OfferValidUntil, DateTime.UtcNow, cus.Id);
				throw;
			}
		} // ValidateOffer

		public virtual void VerifyAvailableFunds(decimal transfered) {
			this.serviceClient.Instance.VerifyEnoughAvailableFunds(this.context.UserId, transfered);
		} // VerifyAvailableFunds

		public virtual void ValidateCustomer(Customer cus) {
			if (cus == null || cus.PersonalInfo == null || cus.BankAccount == null) {
				log.Warn("ValidateCustomer CustomerIsNotFullyRegisteredException cus == null {0} || cus.PersonalInfo == null {1} || cus.BankAccount == null {2} customerid {3}",
					cus == null, cus == null || cus.PersonalInfo == null, cus == null || cus.BankAccount == null, cus == null ? 0 : cus.Id);
				throw new CustomerIsNotFullyRegisteredException();
			}

			if (cus.Status != Status.Approved) {
				log.Warn("ValidateCustomer CustomerIsNotApprovedException cus.Status != Status.Approved {0} customerid: {1}", cus.Status, cus.Id);
				throw new CustomerIsNotApprovedException();
			}

			if (!cus.WizardStep.TheLastOne) {
				log.Warn("ValidateCustomer CustomerIsNotFullyRegisteredException !cus.WizardStep.TheLastOne {0} customerid: {1}", cus.WizardStep.TheLastOne, cus.Id);
				throw new CustomerIsNotFullyRegisteredException();
			}

			if (!cus.CreditSum.HasValue || !cus.CollectionStatus.IsEnabled || cus.BlockTakingLoan) {
				log.Warn("ValidateCustomer Invalid customer state !cus.CreditSum.HasValue {0} || !cus.CollectionStatus.IsEnabled {1} || cus.BlockTakingLoan {2} customerid: {3}",
					 cus.CreditSum.HasValue, cus.CollectionStatus.IsEnabled, cus.BlockTakingLoan, cus.Id);
                throw new Exception("Funding is not available. <br>Please contact support " + cus.CustomerOrigin.PhoneNumber);
			}
		} // ValidateCustomer

		public virtual void ValidateAmount(decimal loanAmount, Customer cus) {
			if (loanAmount <= 0) {
				log.Warn("ValidateAmount loanAmount <= 0 {0} customerid: {1} ", loanAmount, cus.Id);
				throw new ArgumentException();
			}

			if (cus.CreditSum < loanAmount) {
				log.Warn("ValidateAmount cus.CreditSum < loanAmount {0} {1} customerid: {2}", cus.CreditSum, loanAmount, cus.Id);
				throw new ArgumentException();
			}
		} // ValidateAmount

		public virtual string GetCustomerNameForPacNet(Customer customer) {
			string name = string.Format("{0} {1}", customer.PersonalInfo.FirstName, customer.PersonalInfo.Surname);

			if (name.Length > 18)
				name = customer.PersonalInfo.Surname;

			if (name.Length > 18)
				name = name.Substring(0, 17);

			return name;
		} // GetCustomerNameForPacNet

		private bool ValidateEverlineRefinance(Customer cus) {
			if (cus.CustomerOrigin.Name == "everline" && !cus.Loans.Any()) {
				EverlineLoginLoanChecker checker = new EverlineLoginLoanChecker();
				var status = checker.GetLoginStatus(cus.Name);
				return (status.status == EverlineLoanStatus.ExistsWithCurrentLiveLoan);
			} // if
			return false;
		} // ValidateEverlineRefinance

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

			log.Debug(
				"SendMoney customerId: {0}, amount: {1}, sortcode: {2}, account number: {3}, name: {4}, ezbob GBP EZBOB",
				cus.Id,
				transfered,
				cus.BankAccount.SortCode,
				cus.BankAccount.AccountNumber,
				name
			);

			var sendResponse = this.pacnetService.SendMoney(
				cus.Id,
				transfered,
				cus.BankAccount.SortCode,
				cus.BankAccount.AccountNumber,
				name,
				"ezbob",
				"GBP",
				"EZBOB" //TODO rename to orange money
			);

			log.Debug(
				"SendMoney response: tracking: {0}, status {1} {2} ",
				sendResponse.TrackingNumber,
				sendResponse.Status,
				sendResponse.HasError ? ",Error: " + sendResponse.Error : ""
			);

			var closeResponse = this.pacnetService.CloseFile(cus.Id, "ezbob");

			log.Debug(
				"CloseFile response: tracking: {0}, status {1} {2} ",
				closeResponse.TrackingNumber,
				closeResponse.Status,
				closeResponse.HasError ? ",Error: " + closeResponse.Error : ""
			);

			return sendResponse;
		} // SendMoney

		private void ValidateRepaymentPeriodAndInterestRate(Customer cus) {
			var cr = cus.LastCashRequest;

			//validate that CR exsists.
			if (cr == null) {
				log.Warn("ValidateRepaymentPeriodAndInterestRate No offer exists customerid: {0}", cus.Id);
				throw new ArgumentException("No offer exists");
			}
			//validate in case customer not allowed to change period that request equal to approved.
			if (!cr.IsCustomerRepaymentPeriodSelectionAllowed && cr.RepaymentPeriod != cr.ApprovedRepaymentPeriod) {
				log.Warn("ValidateRepaymentPeriodAndInterestRate Wrong repayment period !cr.IsCustomerRepaymentPeriodSelectionAllowed {0} && cr.RepaymentPeriod {1} != cr.ApprovedRepaymentPeriod {2} customerid: {3}",
					cr.IsCustomerRepaymentPeriodSelectionAllowed, cr.RepaymentPeriod, cr.ApprovedRepaymentPeriod, cus.Id);
				throw new ArgumentException("Wrong repayment period");
			}
			//validate that loan period is in the range of max period allowed by loan source
			if (cr.LoanSource.DefaultRepaymentPeriod.HasValue && cr.LoanSource.DefaultRepaymentPeriod > cr.RepaymentPeriod) {
				log.Warn("ValidateRepaymentPeriodAndInterestRate Wrong repayment period2 cr.LoanSource.DefaultRepaymentPeriod.HasValue true && cr.LoanSource.DefaultRepaymentPeriod {0} > cr.RepaymentPeriod {1} customerid: {2}",
					 cr.LoanSource.DefaultRepaymentPeriod, cr.RepaymentPeriod, cus.Id);
				throw new ArgumentException("Wrong repayment period");
			}
			//validate that loan interest is in the range of max interest allowed by loan source
			if (cr.LoanSource.MaxInterest.HasValue && cr.InterestRate > cr.LoanSource.MaxInterest.Value) {
				log.Warn("ValidateRepaymentPeriodAndInterestRate Wrong interest rate cr.LoanSource.MaxInterest.HasValue true && cr.InterestRate {0} > cr.LoanSource.MaxInterest.Value {1} customerid: {2}",
					cr.InterestRate, cr.LoanSource.MaxInterest.Value, cus.Id);
				throw new ArgumentException("Wrong interest rate");
			}
		} // ValidateRepaymentPeriodAndInterestRate


		private readonly IPacnetService pacnetService;
		private readonly ServiceClient serviceClient;
		private readonly IAgreementsGenerator agreementsGenerator;
		private readonly IEzbobWorkplaceContext context;
		private readonly LoanBuilder loanBuilder;
		private readonly ISession session;
		private readonly LoanTransactionMethodRepository tranMethodRepo;

		private static readonly ASafeLog log = new SafeILog(typeof(LoanCreator));
	} // class LoanCreator
} // namespace
