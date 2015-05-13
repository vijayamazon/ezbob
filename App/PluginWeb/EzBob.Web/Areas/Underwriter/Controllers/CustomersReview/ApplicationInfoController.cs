namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
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
	using Models;
	using Code;
	using DbConstants;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Infrastructure;
	using Infrastructure.csrf;
	using NHibernate;
	using PaymentServices.PacNet;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;
	using StructureMap;

	public class ApplicationInfoController : Controller
	{
		private readonly ServiceClient serviceClient;
		private readonly ICustomerRepository _customerRepository;
		private readonly ICashRequestsRepository _cashRequestsRepository;
		private readonly ILoanTypeRepository _loanTypes;
		private readonly IDiscountPlanRepository _discounts;
		private readonly CashRequestBuilder _crBuilder;
		private readonly ApplicationInfoModelBuilder _infoModelBuilder;
		private readonly IApprovalsWithoutAMLRepository _approvalsWithoutAmlRepository;
		
		private readonly ILoanSourceRepository _loanSources;
		private readonly IUsersRepository _users;
		private readonly IEzbobWorkplaceContext _context;
		private readonly CustomerPhoneRepository customerPhoneRepository;
		
		private static readonly ASafeLog log = new SafeILog(typeof(ApplicationInfoController));

		public ApplicationInfoController(
			ICustomerRepository customerRepository,
			ICashRequestsRepository cashRequestsRepository,
			ILoanTypeRepository loanTypes,
			IDiscountPlanRepository discounts,
			CashRequestBuilder crBuilder,
			ApplicationInfoModelBuilder infoModelBuilder,
			IApprovalsWithoutAMLRepository approvalsWithoutAMLRepository,
			ILoanSourceRepository loanSources,
			IUsersRepository users,
			IEzbobWorkplaceContext context,
			CustomerPhoneRepository customerPhoneRepository)
		{
			this._customerRepository = customerRepository;
            this._cashRequestsRepository = cashRequestsRepository;
            this._loanTypes = loanTypes;
            this._discounts = discounts;
            this._crBuilder = crBuilder;
            this._infoModelBuilder = infoModelBuilder;
            this._approvalsWithoutAmlRepository = approvalsWithoutAMLRepository;
            this._loanSources = loanSources;
            this._users = users;
            this._context = context;
            this.serviceClient = new ServiceClient();
			this.customerPhoneRepository = customerPhoneRepository;
			
		}

		// Here we get VA\FCF\Turnover
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpGet]
		public JsonResult Index(int id)
		{
            var customer = this._customerRepository.Get(id);
			var m = new ApplicationInfoModel();
			var cr = customer.LastCashRequest;
            this._infoModelBuilder.InitApplicationInfo(m, customer, cr);
			return Json(m, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpPost]
		public JsonResult VerifyPhone(int customerId,string phoneType, bool verifiedPreviousState)
		{
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
			return Json(new {});
		}
	
		[HttpPost]
		[Transactional]
		[Ajax]
		public void SaveApproveWithoutAML(int customerId, bool doNotShowAgain)
		{
			log.Debug("Saving approve without AML. Customer:{0} doNotShowAgain = {1}", customerId, doNotShowAgain);

			var entry = new ApprovalsWithoutAML
			{
				CustomerId = customerId,
				DoNotShowAgain = doNotShowAgain,
				Timestamp = DateTime.UtcNow,
				Username = User.Identity.Name
			};

            this._approvalsWithoutAmlRepository.SaveOrUpdate(entry);
		}

		[HttpPost]
		[Transactional]
		[ValidateJsonAntiForgeryToken]
		[Ajax]
		public JsonResult ToggleCciMark(int id) {
            Customer oCustomer = this._customerRepository.Get(id);

			if (oCustomer == null) {
				log.Debug("Customer({0}) not found", id);
				return Json(new { error = "Customer not found.", id = id });
			} // if

			oCustomer.CciMark = !oCustomer.CciMark;

            this.serviceClient.Instance.AddCciHistory(id, this._context.UserId, oCustomer.CciMark);

			log.Debug("Customer({0}).CciMark set to {1}", id, oCustomer.CciMark);

			return Json(new { error = (string)null, id = id, mark = oCustomer.CciMark });
		} // ToggleCciMark

		[HttpPost]
		[Transactional]
		[ValidateJsonAntiForgeryToken]
		[Ajax]
		public JsonResult ToggleIsTest(int id)
		{
            Customer oCustomer = this._customerRepository.Get(id);

			if (oCustomer == null)
			{
				log.Debug("Customer({0}) not found", id);
				return Json(new { error = "Customer not found.", id = id });
			} // if

			oCustomer.IsTest = !oCustomer.IsTest;
		    this.serviceClient.Instance.SalesForceAddUpdateLeadAccount(this._context.UserId, null, oCustomer.Id, false, false);
			log.Debug("Customer({0}).IsTest set to {1}", id, oCustomer.IsTest);

			return Json(new { error = (string)null, id = id, isTest = oCustomer.IsTest });
		} // ToggleIsTest

		[HttpPost]
		[Transactional]
		[ValidateJsonAntiForgeryToken]
		[Ajax]
		public JsonResult UpdateTrustPilotStatus(int id, string status)
		{
            Customer oCustomer = this._customerRepository.Get(id);

			if (oCustomer == null)
			{
				log.Debug("Customer({0}) not found", id);
				return Json(new { error = "Customer not found.", id = id, status = status });
			} // if

			var oHelper = ObjectFactory.GetInstance<DatabaseDataHelper>();

			TrustPilotStauses nStatus;

			if (!Enum.TryParse<TrustPilotStauses>(status, true, out nStatus))
			{
				log.Debug("Status({0}) not found", status);
				return Json(new { error = "Failed to parse status.", id = id, status = status });
			} // if

			var oTsp = oHelper.TrustPilotStatusRepository.Find(nStatus);

			if (oTsp == null)
			{
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
		[Permission(Name = "CreditLineFields")]
		public JsonResult AvoidAutomaticDecision(int id, bool enabled)
		{
            var cust = this._customerRepository.Get(id);
			cust.IsAvoid = enabled;
			log.Debug("Customer({0}).IsAvoided = {1}", id, enabled);

			return Json(new { error = (string)null, id = id, status = cust.IsAvoid });
		}

        [HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[Permission(Name = "NewCreditLineButton")]
		public JsonResult RunNewCreditLine(int Id, int newCreditLineOption) {
			log.Debug("RunNewCreditLine({0}, {1}) start", Id, newCreditLineOption);

            var customer = this._customerRepository.Get(Id);

			new Transactional(() => {
                var cashRequest = this._crBuilder.CreateCashRequest(customer, CashRequestOriginator.NewCreditLineBtn);
                cashRequest.LoanType = this._loanTypes.GetDefault();

				customer.CreditResult = null;
				customer.OfferStart = cashRequest.OfferStart;
				customer.OfferValidUntil = cashRequest.OfferValidUntil;

                this._customerRepository.SaveOrUpdate(customer);
			}).Execute();

			CreditResultStatus? status;
			string strategyError;

			var typedNewCreditLineOption = (NewCreditLineOption)newCreditLineOption;

			if (typedNewCreditLineOption == NewCreditLineOption.SkipEverything) {
				customer.CreditResult = CreditResultStatus.WaitingForDecision;
                this._customerRepository.SaveOrUpdate(customer);

				strategyError = null;
				status = customer.CreditResult;
			} else {
                var underwriter = this._users.GetUserByLogin(User.Identity.Name);

                ActionMetaData amd = this._crBuilder.ForceEvaluate(underwriter.Id, customer, typedNewCreditLineOption, true);

				// Reload from DB
                var updatedCustomer = this._customerRepository.Load(customer.Id);

				strategyError = amd.Status == ActionStatus.Done ? null : "Error: " + amd.Comment;
				status = updatedCustomer.CreditResult;
			} // if

			log.Debug("RunNewCreditLine({0}, {1}) ended; status = {2}, error = '{3}'", Id, newCreditLineOption, status, strategyError);

			return Json(new {
				status = (status ?? CreditResultStatus.WaitingForDecision).ToString(),
				strategyError = strategyError,
			});
		} // RunNewCreditLine

		[HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult ChangeCreditLine(
			long id,
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
            int isLoanTypeSelectionAllowed
		) {

            CashRequest cr = this._cashRequestsRepository.Get(id);
		    new Transactional(() => {
                
                LoanType loanT = this._loanTypes.Get(loanType);
                LoanSource source = this._loanSources.Get(loanSource);

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

                cr.DiscountPlan = this._discounts.Get(discountPlan);

		        Customer c = cr.Customer;
		        c.OfferStart = cr.OfferStart;
		        c.OfferValidUntil = cr.OfferValidUntil;
		        c.ManagerApprovedSum = sum;
		        this._cashRequestsRepository.SaveOrUpdate(cr);
                this._customerRepository.SaveOrUpdate(c);
		    }).Execute();

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
            //TODO update new offer table
            log.Debug("update offer for customer {0} all the offer is changed", cr.Customer.Id);

			return Json(true);
		} // ChangeCreditLine

		[HttpPost, Ajax, ValidateJsonAntiForgeryToken]
		public JsonResult ActivateMainStrategy(int customerId) {
            int underwriterId = this._context.User.Id;

            new ServiceClient().Instance.MainStrategy1(
                underwriterId,
                customerId,
                NewCreditLineOption.SkipEverythingAndApplyAutoRules,
                0,
                null,
                MainStrategyDoAction.Yes,
                MainStrategyDoAction.Yes
            );

			return Json(true);
		}

		[HttpPost, Ajax, ValidateJsonAntiForgeryToken]
		public JsonResult ActivateFinishWizard(int customerId)
		{
            int underwriterId = this._context.User.Id;

            this._customerRepository.Get(customerId).AddAlibabaDefaultBankAccount();

			var oArgs = new FinishWizardArgs { CustomerID = customerId, };

			new ServiceClient().Instance.FinishWizard(oArgs, underwriterId);

			return Json(true);
		}

		[HttpPost, Ajax, ValidateJsonAntiForgeryToken]
		public JsonResult CreateLoanHidden(int nCustomerID, decimal nAmount, string sDate)
		{
			try
			{
				var lc = new LoanCreatorNoChecks(
					ObjectFactory.GetInstance<IPacnetService>(),
					ObjectFactory.GetInstance<IAgreementsGenerator>(),
					ObjectFactory.GetInstance<IEzbobWorkplaceContext>(),
					ObjectFactory.GetInstance<LoanBuilder>(),
					ObjectFactory.GetInstance<ISession>()
				);

                Customer oCustomer = this._customerRepository.Get(nCustomerID);

				DateTime oDate = DateTime.ParseExact(sDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);

				lc.CreateLoan(oCustomer, nAmount, null, oDate);
				
				return Json(new { success = true, error = false, });
			}
			catch (Exception e)
			{
				log.Warn(e, "Could not create a hidden loan.");
				return Json(new { success = false, error = e.Message, });
			} // try
		}

		[HttpPost, Ajax, ValidateJsonAntiForgeryToken]
		public JsonResult ResetPassword123456(int nCustomerID) {
            new ServiceClient().Instance.ResetPassword123456(this._context.User.Id, nCustomerID, PasswordResetTarget.Customer);
			return Json(true);
		} // ResetPassword123456
	} // class ApplicationInfoController
} // namespace
