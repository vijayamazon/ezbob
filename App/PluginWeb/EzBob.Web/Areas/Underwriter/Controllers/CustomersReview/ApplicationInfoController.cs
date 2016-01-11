namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview {
	using System.Globalization;
	using System.Linq;
	using Code.Agreements;
	using ConfigManager;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using System;
	using System.Web.Mvc;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Model.Database.UserManagement;
	using EZBob.DatabaseLib.Model.Loans;
	using Ezbob.Backend.Models;
	using Ezbob.Logger;
	using Infrastructure.Attributes;
	using Code;
	using Infrastructure;
	using Infrastructure.csrf;
	using NHibernate;
	using PaymentServices.PacNet;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;
	using StructureMap;
	using EZBob.DatabaseLib.Repository;

	using CreditResultStatus = EZBob.DatabaseLib.Model.Database.CreditResultStatus;

	public class ApplicationInfoController : Controller {
		private readonly ServiceClient serviceClient;
		private readonly ICustomerRepository customerRepository;
		private readonly ICashRequestsRepository cashRequestsRepository;
		private readonly ILoanTypeRepository loanTypes;
		private readonly IDiscountPlanRepository discounts;
		private readonly IApprovalsWithoutAMLRepository approvalsWithoutAmlRepository;
		private readonly LoanOptionsRepository loanOptionsRepository;
		private readonly ILoanSourceRepository loanSources;
		private readonly IUsersRepository users;
		private readonly IEzbobWorkplaceContext context;
		private readonly CustomerPhoneRepository customerPhoneRepository;
		private readonly IExternalCollectionStatusesRepository externalCollectionStatusesRepository;
		private readonly ILoanRepository loanRepository;

		private static readonly ASafeLog log = new SafeILog(typeof(ApplicationInfoController));

		public ApplicationInfoController(
			ICustomerRepository customerRepository,
			ICashRequestsRepository cashRequestsRepository,
			ILoanTypeRepository loanTypes,
			IDiscountPlanRepository discounts,
			IApprovalsWithoutAMLRepository approvalsWithoutAMLRepository,
			ILoanSourceRepository loanSources,
			IUsersRepository users,
			IEzbobWorkplaceContext context,
			CustomerPhoneRepository customerPhoneRepository, 
			IExternalCollectionStatusesRepository externalCollectionStatusesRepository,
			LoanOptionsRepository loanOptionsRepository,
			ILoanRepository loanRepository, 
			ServiceClient serviceClient) {
			this.customerRepository = customerRepository;
			this.cashRequestsRepository = cashRequestsRepository;
			this.loanTypes = loanTypes;
			this.discounts = discounts;
			this.approvalsWithoutAmlRepository = approvalsWithoutAMLRepository;
			this.loanSources = loanSources;
			this.users = users;
			this.context = context;
			this.customerPhoneRepository = customerPhoneRepository;
			this.externalCollectionStatusesRepository = externalCollectionStatusesRepository;
			this.loanOptionsRepository = loanOptionsRepository;
			this.loanRepository = loanRepository;
			this.serviceClient = serviceClient;
		}

		// Here we get VA\FCF\Turnover
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpGet]
		public JsonResult Index(int id) {
			var customer = this.customerRepository.Get(id);
			var cr = customer.LastCashRequest;

			var aiar = this.serviceClient.Instance.LoadApplicationInfo(
				this.context.UserId,
				customer.Id,
				cr == null ? (long?)null : cr.Id,
				DateTime.UtcNow
			);

			return Json(aiar.Model, JsonRequestBehavior.AllowGet);
		} // Index

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpPost]
		public JsonResult UpdateBrokerCommissionDefaults(long id, decimal amount) {
			decimal brokerCommission;
			decimal setupFeePercent;

			try {
				LoanCommissionDefaultsActionResult lcdar = this.serviceClient.Instance.GetLoanCommissionDefaults(
					this.context.User.Id,
					id,
					amount
				);

				brokerCommission = lcdar.BrokerCommission;
				setupFeePercent = lcdar.ManualSetupFee;
			} catch (Exception e) {
				log.Alert(
					e,
					"Failed to calculate loan default commission for cash request {0} and loan amount {1}.",
					id,
					amount
				);

				brokerCommission = 0;
				setupFeePercent = 0;
			} // try

			return Json(new { brokerCommission, setupFeePercent }, JsonRequestBehavior.AllowGet);
		} // UpdateBrokerCommissionDefaults

		[Ajax]
		[HttpPost]
		public JsonResult VerifyPhone(int customerId, string phoneType, bool verifiedPreviousState) {
			CustomerPhone customerPhone = this.customerPhoneRepository.GetAll().FirstOrDefault(x => x.CustomerId == customerId && x.PhoneType == phoneType && x.IsCurrent);
			if (customerPhone == null) {
				return Json(new { });
			}

			customerPhone.IsCurrent = false;
			this.customerPhoneRepository.SaveOrUpdate(customerPhone);

			var newCustomerPhoneEntry = new CustomerPhone {
				CustomerId = customerPhone.CustomerId,
				IsCurrent = true,
				Phone = customerPhone.Phone,
				PhoneType = customerPhone.PhoneType,
				IsVerified = !verifiedPreviousState,
				VerificationDate = DateTime.UtcNow,
				VerifiedBy = User.Identity.Name
			};
			this.customerPhoneRepository.SaveOrUpdate(newCustomerPhoneEntry);
			return Json(new { });
		}

		[HttpPost]
		[Transactional]
		[Ajax]
		public void SaveApproveWithoutAML(int customerId, bool doNotShowAgain) {
			log.Debug("Saving approve without AML. Customer:{0} doNotShowAgain = {1}", customerId, doNotShowAgain);

			var entry = new ApprovalsWithoutAML {
				CustomerId = customerId,
				DoNotShowAgain = doNotShowAgain,
				Timestamp = DateTime.UtcNow,
				Username = User.Identity.Name
			};

			this.approvalsWithoutAmlRepository.SaveOrUpdate(entry);
		}

		[HttpPost]
		[Transactional]
		[ValidateJsonAntiForgeryToken]
		[Ajax]
		public JsonResult ToggleCciMark(int id) {
			Customer oCustomer = this.customerRepository.Get(id);

			if (oCustomer == null) {
				log.Debug("Customer({0}) not found", id);
				return Json(new { error = "Customer not found.", id = id });
			} // if

			oCustomer.CciMark = !oCustomer.CciMark;

			this.serviceClient.Instance.AddCciHistory(id, this.context.UserId, oCustomer.CciMark);

			log.Debug("Customer({0}).CciMark set to {1}", id, oCustomer.CciMark);

			return Json(new { error = (string)null, id = id, mark = oCustomer.CciMark });
		} // ToggleCciMark

		[HttpPost]
		[Transactional]
		[ValidateJsonAntiForgeryToken]
		[Ajax]
		public JsonResult ToggleBlockTakingLoan(int id) {
			Customer oCustomer = this.customerRepository.Get(id);

			if (oCustomer == null) {
				log.Debug("Customer({0}) not found", id);
				return Json(new { error = "Customer not found.", id = id });
			} // if

			oCustomer.BlockTakingLoan = !oCustomer.BlockTakingLoan;

			log.Debug("Customer({0}).BlockTakingLoan set to {1}", id, oCustomer.BlockTakingLoan);

			return Json(new { error = (string)null, id = id, mark = oCustomer.BlockTakingLoan });
		} // ToggleBlockTakingLoan

		[HttpPost]
		[Transactional]
		[ValidateJsonAntiForgeryToken]
		[Ajax]
		public JsonResult ToggleIsTest(int id) {
			Customer oCustomer = this.customerRepository.Get(id);

			if (oCustomer == null) {
				log.Debug("Customer({0}) not found", id);
				return Json(new { error = "Customer not found.", id = id });
			} // if

			oCustomer.IsTest = !oCustomer.IsTest;
			this.serviceClient.Instance.SalesForceAddUpdateLeadAccount(this.context.UserId, null, oCustomer.Id, false, false);
			log.Debug("Customer({0}).IsTest set to {1}", id, oCustomer.IsTest);

			return Json(new { error = (string)null, id = id, isTest = oCustomer.IsTest });
		} // ToggleIsTest

		[HttpPost]
		[Transactional]
		[ValidateJsonAntiForgeryToken]
		[Ajax]
		public JsonResult UpdateTrustPilotStatus(int id, string status) {
			Customer oCustomer = this.customerRepository.Get(id);

			if (oCustomer == null) {
				log.Debug("Customer({0}) not found", id);
				return Json(new { error = "Customer not found.", id = id, status = status });
			} // if

			var oHelper = ObjectFactory.GetInstance<DatabaseDataHelper>();

			TrustPilotStauses nStatus;

			if (!Enum.TryParse<TrustPilotStauses>(status, true, out nStatus)) {
				log.Debug("Status({0}) not found", status);
				return Json(new { error = "Failed to parse status.", id = id, status = status });
			} // if

			var oTsp = oHelper.TrustPilotStatusRepository.Find(nStatus);

			if (oTsp == null) {
				log.Debug("Status({0}) not found in the DB repository.", status);
				return Json(new { error = "Status not found in the DB repository.", id = id, status = status });
			} // if

			oCustomer.TrustPilotStatus = oTsp;
			log.Debug("Customer({0}).TrustPilotStatus set to {1}", id, status);

			return Json(new { error = (string)null, id = id, status = status });
		} // UpdateTrustPilotStatus

		[HttpPost]
		[Transactional]
		[ValidateJsonAntiForgeryToken]
		[Ajax]
		public JsonResult ChangeExternalCollectionStatus(int id, int? externalStatusID) {
			Customer oCustomer = this.customerRepository.Get(id);

			if (oCustomer == null) {
				log.Debug("Customer({0}) not found", id);
				return Json(new { error = "Customer not found.", id = id, status = externalStatusID });
			} // if

			var prevExternalCollectionStatus = oCustomer.ExternalCollectionStatus;

			var newExternalCollectionStatus = (externalStatusID == null) ? null : this.externalCollectionStatusesRepository.Get(externalStatusID);

			if (newExternalCollectionStatus == null && externalStatusID != null) {
				log.Debug("Status({0}) not found in the DB repository.", externalStatusID);
				return Json(new { error = "Status not found in the DB repository.", id = id, status = externalStatusID });
			} // if

			oCustomer.ExternalCollectionStatus = newExternalCollectionStatus;
			log.Debug("Customer({0}).ExternalCollectionStatus set to {1}", id, externalStatusID);

			DateTime now = DateTime.UtcNow;

			if (newExternalCollectionStatus != prevExternalCollectionStatus && (newExternalCollectionStatus == null || prevExternalCollectionStatus == null)) {
				foreach (Loan loan in oCustomer.Loans.Where(l => l.Status != LoanStatus.PaidOff && l.Balance >= CurrentValues.Instance.MinDectForDefault)) {
					bool customerInGoodStatus = newExternalCollectionStatus == null && oCustomer.CollectionStatus.IsEnabled;
					LoanOptions options = this.loanOptionsRepository.GetByLoanId(loan.Id) ?? LoanOptions.GetDefault(loan.Id);
					options.AutoLateFees = customerInGoodStatus;

					options.AutoPayment = customerInGoodStatus;
					options.StopAutoChargeDate = customerInGoodStatus ? (DateTime?)null : now;

					this.loanOptionsRepository.SaveOrUpdate(options);

					if (!customerInGoodStatus) {
						loan.InterestFreeze.Add(new LoanInterestFreeze {
							Loan = loan,
							StartDate = now.Date,
							EndDate = (DateTime?)null,
							InterestRate = 0,
							ActivationDate = now,
							DeactivationDate = null
						});
					} else if (loan.InterestFreeze.Any(f => f.EndDate == null && f.DeactivationDate == null)) {
						foreach (var interestFreeze in loan.InterestFreeze.Where(f => f.EndDate == null && f.DeactivationDate == null)) {
							interestFreeze.DeactivationDate = now;
						}
					}

					this.loanRepository.SaveOrUpdate(loan);
				}
			} 

			return Json(new { error = (string)null, id = id, status = externalStatusID });
		} // ChangeExternalCollectionStatus

		[HttpPost]
		[Transactional]
		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[Permission(Name = "CreditLineFields")]
		public JsonResult AvoidAutomaticDecision(int id, bool enabled) {
			var cust = this.customerRepository.Get(id);
			cust.IsAvoid = enabled;
			log.Debug("Customer({0}).IsAvoided = {1}", id, enabled);

			//TODO update new offer table
			log.Debug("update offer for customer {0} avoid auto decision {1}", cust.Id, enabled);

			return Json(new { error = (string)null, id = id, status = cust.IsAvoid });
		}
	
		[HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[Permission(Name = "NewCreditLineButton")]
		public JsonResult RunNewCreditLine(int Id, int newCreditLineOption) {
			NewCreditLineOption typedNewCreditLineOption = (NewCreditLineOption)newCreditLineOption;
			User underwriter = this.users.GetUserByLogin(User.Identity.Name);

			log.Debug("RunNewCreditLine({0}, {1}) start", Id, typedNewCreditLineOption);

			ActionMetaData amd = ExecuteNewCreditLine(underwriter.Id, Id, typedNewCreditLineOption);

			// Reload from DB
			Customer customer = this.customerRepository.Load(Id);

			string strategyError = amd.Status == ActionStatus.Done ? null : "Error: " + amd.Comment;
			CreditResultStatus? status = customer.CreditResult;

			log.Debug(
				"RunNewCreditLine({0}, {1}) ended; status = {2}, error = '{3}'",
				Id,
				typedNewCreditLineOption,
				status,
				strategyError
			);

			return Json(new {
				status = (status ?? CreditResultStatus.WaitingForDecision).ToString(),
				strategyError = strategyError,
			});
		} // RunNewCreditLine

		[HttpPost]
		[Transactional]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult ChangeCreditLine(
			long id,
			int productID,
			int productTypeID,
			int productSubTypeID,
			int loanType,
			int loanSource,
			double amount,
			decimal interestRate,
			int repaymentPeriod,
			string offerStart,
			string offerValidUntil,
			bool allowSendingEmail,
			int discountPlan,
			decimal? brokerSetupFeePercent,
			decimal? manualSetupFeePercent,
			bool isCustomerRepaymentPeriodSelectionAllowed,
			int isLoanTypeSelectionAllowed,
			bool spreadSetupFee
		) {
			CashRequest cr = this.cashRequestsRepository.Get(id);

			new Transactional(() => {

				LoanType loanT = this.loanTypes.Get(loanType);
				LoanSource source = this.loanSources.Get(loanSource);

				cr.LoanType = loanT;

				int step = CurrentValues.Instance.GetCashSliderStep;
				int sum = (int)Math.Round(amount / step, MidpointRounding.AwayFromZero) * step;
				cr.ManagerApprovedSum = sum;
				cr.LoanSource = source;
				cr.InterestRate = interestRate;
				cr.RepaymentPeriod = repaymentPeriod;
				cr.ApprovedRepaymentPeriod = cr.RepaymentPeriod;
				cr.OfferStart = FormattingUtils.ParseDateWithCurrentTime(offerStart);
				cr.OfferValidUntil = FormattingUtils.ParseDateWithCurrentTime(offerValidUntil);

				cr.BrokerSetupFeePercent = brokerSetupFeePercent;
				cr.ManualSetupFeePercent = manualSetupFeePercent;

				cr.EmailSendingBanned = !allowSendingEmail;
				cr.LoanTemplate = null;

				cr.IsLoanTypeSelectionAllowed = isLoanTypeSelectionAllowed;
				cr.IsCustomerRepaymentPeriodSelectionAllowed = isCustomerRepaymentPeriodSelectionAllowed;

				cr.DiscountPlan = this.discounts.Get(discountPlan);
				cr.SpreadSetupFee = spreadSetupFee;
				cr.ProductSubTypeID = productSubTypeID;
				Customer c = cr.Customer;
				c.OfferStart = cr.OfferStart;
				c.OfferValidUntil = cr.OfferValidUntil;
				c.ManagerApprovedSum = sum;
				
				this.cashRequestsRepository.SaveOrUpdate(cr);
				this.customerRepository.SaveOrUpdate(c);
			}).Execute();


			/*
			DateTime now = DateTime.UtcNow;
			var decisionId = this.serviceClient.Instance.AddDecision(this._context.UserId, cr.Customer.Id, new NL_Decisions {
				UserID = this._context.UserId,
				SendEmailNotification = allowSendingEmail,
				DecisionTime = now,
				IsRepaymentPeriodSelectionAllowed = isCustomerRepaymentPeriodSelectionAllowed,
				DecisionNameID = (int)DecisionActions.Waiting
				//todo IsAmountSelectionAllowed = 
				//todo InterestOnlyRepaymentCount = 
				//todo Notes = 
				//todo Position = 
			}, cr.Id, null);

			this.serviceClient.Instance.AddOffer(this._context.UserId, cr.Customer.Id, new NL_Offers {
				Amount = (decimal)amount,
				BrokerSetupFeePercent = brokerSetupFeePercent ?? 0,
				CreatedTime = now,
				DiscountPlanID = discountPlan,
				EmailSendingBanned = !allowSendingEmail,
				EndTime = FormattingUtils.ParseDateWithCurrentTime(offerValidUntil),
				SetupFeePercent = manualSetupFeePercent ?? 0,
				// DistributedSetupFeePercent TODO EZ-3515
				IsLoanTypeSelectionAllowed = isLoanTypeSelectionAllowed == 1,
				LoanSourceID = loanSource,
				LoanTypeID = loanType,
				MonthlyInterestRate = interestRate,
				RepaymentCount = repaymentPeriod,
				RepaymentIntervalTypeID = (int)RepaymentIntervalTypesId.Month,
				StartTime = FormattingUtils.ParseDateWithCurrentTime(offerStart),
				DecisionID = decisionId.Value,
				//todo Notes = 
				//todo InterestOnlyRepaymentCount = 
			});
			 */
			//TODO update new offer table
			log.Debug("update offer for customer {0} all the offer is changed", cr.Customer.Id);


			return Json(true);
		} // ChangeCreditLine

		[HttpPost, Ajax, ValidateJsonAntiForgeryToken]
		public JsonResult CheckLogicalGlue(int customerID, long cashRequestID, decimal amount, int repaymentPeriod) {
			log.Info("CheckLogicalGlue {0} {1} {2} {3}", customerID, cashRequestID, amount, repaymentPeriod);
			//TODO run lg check service
			return Json(new LogicalGlueResult{
				BucketStr = "B",
				Error = "This is fake data",
				Date = DateTime.UtcNow,
				Bucket = Bucket.B,
				BucketPercent = 0.42M,
				FLScore = 0.160M,
				NNScore = 0.160M,
				MonthlyRepayment = amount / repaymentPeriod,
				UniqueID = new Guid()
			}, JsonRequestBehavior.AllowGet);
		}

		[HttpPost, Ajax, ValidateJsonAntiForgeryToken]
		public JsonResult ActivateMainStrategy(int customerId) {
			int underwriterId = this.context.User.Id;

			Customer customer = this.customerRepository.Get(customerId);

			CashRequest cr = customer.LastCashRequest;

			if (cr != null) {
				new MainStrategyClient(
					underwriterId,
					customer.Id,
					customer.IsAvoid,
					NewCreditLineOption.SkipEverythingAndApplyAutoRules,
					cr.Id,
					null
				).ExecuteAsync();
			} // if

			return Json(true);
		} // ActivateMainStrategy

		[HttpPost, Ajax, ValidateJsonAntiForgeryToken]
		public JsonResult ActivateFinishWizard(int customerId) {
			int underwriterId = this.context.User.Id;

			this.customerRepository.Get(customerId).AddAlibabaDefaultBankAccount();

			var oArgs = new FinishWizardArgs {
				CustomerID = customerId,
				CashRequestOriginator = EZBob.DatabaseLib.Model.Database.CashRequestOriginator.ForcedWizardCompletion,
			};

			new ServiceClient().Instance.FinishWizard(oArgs, underwriterId);

			return Json(true);
		} // ActivateFinishWizard

		[HttpPost, Ajax, ValidateJsonAntiForgeryToken]
		public JsonResult CreateLoanHidden(int nCustomerID, decimal nAmount, string sDate) {
			try {
				var lc = new LoanCreatorNoChecks(
					ObjectFactory.GetInstance<IPacnetService>(),
					ObjectFactory.GetInstance<IAgreementsGenerator>(),
					ObjectFactory.GetInstance<IEzbobWorkplaceContext>(),
					ObjectFactory.GetInstance<LoanBuilder>(),
					ObjectFactory.GetInstance<ISession>()
				);

				Customer oCustomer = this.customerRepository.Get(nCustomerID);

				DateTime oDate = DateTime.ParseExact(sDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);

				lc.CreateLoan(oCustomer, nAmount, null, oDate);

				return Json(new { success = true, error = false, });
			} catch (Exception e) {
				log.Warn(e, "Could not create a hidden loan.");
				return Json(new { success = false, error = e.Message, });
			} // try
		}

		[HttpPost, Ajax, ValidateJsonAntiForgeryToken]
		public JsonResult ResetPassword123456(int nCustomerID) {
			new ServiceClient().Instance.ResetPassword123456(this.context.User.Id, nCustomerID, PasswordResetTarget.Customer);
			return Json(true);
		} // ResetPassword123456

		private ActionMetaData ExecuteNewCreditLine(
			int underwriterID,
			int customerID,
			NewCreditLineOption newCreditLineOption
		) {
			Customer customer = this.customerRepository.Get(customerID);

			EZBob.DatabaseLib.Model.Database.CashRequestOriginator originator;

			switch (newCreditLineOption) {
			case NewCreditLineOption.SkipEverything:
				originator = EZBob.DatabaseLib.Model.Database.CashRequestOriginator.NewCreditLineSkipAll;
				break;

			case NewCreditLineOption.SkipEverythingAndApplyAutoRules:
				originator = EZBob.DatabaseLib.Model.Database.CashRequestOriginator.NewCreditLineSkipAndGoAuto;
				break;

			case NewCreditLineOption.UpdateEverythingAndApplyAutoRules:
				originator = EZBob.DatabaseLib.Model.Database.CashRequestOriginator.NewCreditLineUpdateAndGoAuto;
				break;

			case NewCreditLineOption.UpdateEverythingAndGoToManualDecision:
				originator = EZBob.DatabaseLib.Model.Database.CashRequestOriginator.NewCreditLineUpdateAndGoManual;
				break;

			default:
				originator = EZBob.DatabaseLib.Model.Database.CashRequestOriginator.NewCreditLineBtn;
				log.Alert(
					"New credit line option not specified for customer {0}, underwriter {1} - defaulting to obsolete value.",
					customerID,
					underwriterID
				);
				break;
			} // switch

			ActionMetaData amd = new MainStrategyClient(
				underwriterID,
				customer.Id,
				customer.IsAvoid,
				newCreditLineOption,
				null,
				originator
			).ExecuteSync();

			ForceNhibernateResync.ForCustomer(customerID);

			return amd;
		} // ExecuteNewCreditLine
	} // class ApplicationInfoController
} // namespace
