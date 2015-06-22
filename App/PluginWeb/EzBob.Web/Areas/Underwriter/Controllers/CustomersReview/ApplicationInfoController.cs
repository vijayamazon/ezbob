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
	using Models;
	using Code;
	using Infrastructure;
	using Infrastructure.csrf;
	using NHibernate;
	using PaymentServices.Calculators;
	using PaymentServices.PacNet;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;
	using StructureMap;

	public class ApplicationInfoController : Controller {
		private readonly ServiceClient serviceClient;
		private readonly ICustomerRepository _customerRepository;
		private readonly ICashRequestsRepository _cashRequestsRepository;
		private readonly ILoanTypeRepository _loanTypes;
		private readonly LoanLimit _limit;
		private readonly IDiscountPlanRepository _discounts;
		private readonly CashRequestBuilder _crBuilder;
		private readonly ApplicationInfoModelBuilder _infoModelBuilder;
		private readonly IApprovalsWithoutAMLRepository _approvalsWithoutAmlRepository;

		private readonly ILoanSourceRepository _loanSources;
		private readonly IUsersRepository _users;
		private readonly IEzbobWorkplaceContext _context;
		private readonly ISuggestedAmountRepository _suggestedAmountRepository;
		private readonly CustomerPhoneRepository customerPhoneRepository;
		private readonly IExternalCollectionStatusesRepository externalCollectionStatusesRepository;

		private static readonly ASafeLog log = new SafeILog(typeof(ApplicationInfoController));

		public ApplicationInfoController(
			ICustomerRepository customerRepository,
			ICashRequestsRepository cashRequestsRepository,
			ILoanTypeRepository loanTypes,
			LoanLimit limit,
			IDiscountPlanRepository discounts,
			CashRequestBuilder crBuilder,
			ApplicationInfoModelBuilder infoModelBuilder,
			IApprovalsWithoutAMLRepository approvalsWithoutAMLRepository,
			ILoanSourceRepository loanSources,
			IUsersRepository users,
			IEzbobWorkplaceContext context,
			ISuggestedAmountRepository suggestedAmountRepository,
			CustomerPhoneRepository customerPhoneRepository, 
			IExternalCollectionStatusesRepository externalCollectionStatusesRepository) {
			_customerRepository = customerRepository;
			_cashRequestsRepository = cashRequestsRepository;
			_loanTypes = loanTypes;
			_limit = limit;
			_discounts = discounts;
			_crBuilder = crBuilder;
			_infoModelBuilder = infoModelBuilder;
			_approvalsWithoutAmlRepository = approvalsWithoutAMLRepository;
			_loanSources = loanSources;
			_users = users;
			_context = context;
			_suggestedAmountRepository = suggestedAmountRepository;
			serviceClient = new ServiceClient();
			this.customerPhoneRepository = customerPhoneRepository;
			this.externalCollectionStatusesRepository = externalCollectionStatusesRepository;
		}

		// Here we get VA\FCF\Turnover
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpGet]
		public JsonResult Index(int id) {
			var customer = _customerRepository.Get(id);
			var m = new ApplicationInfoModel();
			var cr = customer.LastCashRequest;
			_infoModelBuilder.InitApplicationInfo(m, customer, cr);
			return Json(m, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpPost]
		public JsonResult VerifyPhone(int customerId, string phoneType, bool verifiedPreviousState) {
			CustomerPhone customerPhone = customerPhoneRepository.GetAll().FirstOrDefault(x => x.CustomerId == customerId && x.PhoneType == phoneType && x.IsCurrent);
			if (customerPhone == null) {
				return Json(new { });
			}

			customerPhone.IsCurrent = false;
			customerPhoneRepository.SaveOrUpdate(customerPhone);

			var newCustomerPhoneEntry = new CustomerPhone {
				CustomerId = customerPhone.CustomerId,
				IsCurrent = true,
				Phone = customerPhone.Phone,
				PhoneType = customerPhone.PhoneType,
				IsVerified = !verifiedPreviousState,
				VerificationDate = DateTime.UtcNow,
				VerifiedBy = User.Identity.Name
			};
			customerPhoneRepository.SaveOrUpdate(newCustomerPhoneEntry);
			return Json(new { });
		}

		[HttpPost]
		[Transactional]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[Permission(Name = "CreditLineFields")]
		public JsonResult ChangeCashRequestOpenCreditLine(long id, double amount, string method, string medal, decimal? value, decimal? percent) {
			_limit.Check(amount);
			var cr = _cashRequestsRepository.Get(id);
			int step = CurrentValues.Instance.GetCashSliderStep;
			int sum = (int)Math.Round(amount / step, MidpointRounding.AwayFromZero) * step;
			cr.ManagerApprovedSum = sum;
			cr.Customer.ManagerApprovedSum = sum;
			cr.LoanTemplate = null;

			if (cr.Customer.Broker != null) {
				BrokerCommissionDefaultCalculator brokerCommissionDefaultCalculator = new BrokerCommissionDefaultCalculator();
				bool hasLoans = cr.Customer.Loans.Any();
				DateTime? firstLoanDate = hasLoans ? cr.Customer.Loans.Min(x => x.Date) : (DateTime?)null;
				Tuple<decimal, decimal> commission = brokerCommissionDefaultCalculator.Calculate(sum, hasLoans, firstLoanDate);
				cr.BrokerSetupFeePercent = commission.Item1;
				cr.ManualSetupFeePercent = commission.Item2;
			}

			_cashRequestsRepository.SaveOrUpdate(cr);

			//TODO update new offer table
			log.Debug("update offer for customer {0} amount {1}", cr.Customer.Id, amount);

			log.Debug("CashRequest({0}).ManagerApprovedSum = {1}", id, cr.ManagerApprovedSum);

			if (value.HasValue && value.Value > 0) {
				var underwriter = _context.User;
				var sa = new SuggestedAmount {
					InsertDate = DateTime.UtcNow,
					Customer = cr.Customer,
					Underwriter = underwriter,
					CashRequest = cr,
					Amount = value.Value,
					Medal = medal,
					Method = method,
					Percents = percent.HasValue ? percent.Value : 0
				};
				_suggestedAmountRepository.SaveOrUpdate(sa);
			}
			return Json(true);
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

			_approvalsWithoutAmlRepository.SaveOrUpdate(entry);
		}

		[HttpPost]
		[Ajax]
		[Transactional]
		[Permission(Name = "CreditLineFields")]
		public void LoanType(long id, int loanType) {
			var cr = _cashRequestsRepository.Get(id);
			var loanT = _loanTypes.Get(loanType);
			cr.LoanType = loanT;
			cr.RepaymentPeriod = loanT.RepaymentPeriod;
			cr.ApprovedRepaymentPeriod = cr.RepaymentPeriod;
			cr.LoanTemplate = null;
			log.Debug("CashRequest({0}).LoanType = {1}", id, cr.LoanType.Name);

			//TODO update new offer table
			log.Debug("update offer for customer {0} loan type {1}", cr.Customer.Id, loanType);
		}

		[HttpPost]
		[Ajax]
		[Transactional]
		public JsonResult DiscountPlan(long id, int discountPlanId) {
			var cr = _cashRequestsRepository.Get(id);
			var discount = _discounts.Get(discountPlanId);
			cr.DiscountPlan = discount;
			//cr.LoanTemplate = null;
			log.Debug("CashRequest({0}).Discount = {1}", id, cr.DiscountPlan.Name);

			//TODO update new offer table
			log.Debug("update offer for customer {0} discountPlanId {1}", cr.Customer.Id, discountPlanId);

			return Json(new { });
		}

		[HttpPost]
		[Ajax]
		[Transactional]
		public JsonResult LoanSource(long id, int LoanSourceID) {
			var cr = _cashRequestsRepository.Get(id);
			cr.LoanSource = _loanSources.Get(LoanSourceID);

			if (cr.LoanSource == null)
				cr.IsCustomerRepaymentPeriodSelectionAllowed = true;
			else {
				cr.IsCustomerRepaymentPeriodSelectionAllowed = cr.LoanSource.IsCustomerRepaymentPeriodSelectionAllowed;
				cr.IsLoanTypeSelectionAllowed = cr.LoanSource.IsCustomerRepaymentPeriodSelectionAllowed ? 1 : 0;
				if (cr.LoanSource.DefaultRepaymentPeriod.HasValue) {
					cr.RepaymentPeriod = cr.LoanSource.DefaultRepaymentPeriod.Value;
					cr.ApprovedRepaymentPeriod = cr.RepaymentPeriod;
				}
			} // if

			//TODO update new offer table
			log.Debug("update offer for customer {0} loan source {1}", cr.Customer.Id, LoanSourceID);

			return Json(new { });
		} // LoanSource

		[HttpPost]
		[Transactional]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[Permission(Name = "CreditLineFields")]
		public JsonResult ChangeCashRequestInterestRate(long id, decimal interestRate) {
			var cr = _cashRequestsRepository.Get(id);
			cr.InterestRate = interestRate / 100;
			cr.LoanTemplate = null;

			log.Debug("CashRequest({0}).InterestRate = {1}", id, cr.InterestRate);

			//TODO update new offer table
			log.Debug("update offer for customer {0} interest rate {1}", cr.Customer.Id, interestRate);

			return Json(true);
		}

		[HttpPost]
		[Transactional]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[Permission(Name = "CreditLineFields")]
		public JsonResult ChangeCashRequestRepaymentPeriod(long id, int period) {
			var cr = _cashRequestsRepository.Get(id);
			cr.RepaymentPeriod = period;
			cr.ApprovedRepaymentPeriod = cr.RepaymentPeriod;
			cr.LoanTemplate = null;

			log.Debug("CashRequest({0}).RepaymentPeriod = {1}", id, period);

			//TODO update new offer table
			log.Debug("update offer for customer {0} period {1}", cr.Customer.Id, period);


			return Json(true);
		}

		[HttpPost]
		[Transactional]
		[ValidateJsonAntiForgeryToken]
		[Ajax]
		public void SaveDetails(int id, string details) {
			var cust = _customerRepository.Get(id);
			if (cust == null)
				return;

			cust.Details = details;
		}

		[Transactional]
		[HttpPost, ValidateJsonAntiForgeryToken, Ajax, Permission(Name = "CreditLineFields")]
		public JsonResult ChangeManualSetupFeePercent(long id, decimal? manualPercent) {
			var cr = _cashRequestsRepository.Get(id);
			if (manualPercent.HasValue && manualPercent > 0) {
				cr.ManualSetupFeePercent = manualPercent.Value * 0.01M;
			} else {
				cr.ManualSetupFeePercent = null;
			}
			cr.LoanTemplate = null;

			//TODO update new offer table
			log.Debug("update offer for customer {0} setup fee percent {1}", cr.Customer.Id, manualPercent);

			log.Debug("CashRequest({0}).ManualSetupFee percent: {1}", id, cr.ManualSetupFeePercent);
			return Json(new { });
		}

		[Transactional]
		[HttpPost, ValidateJsonAntiForgeryToken, Ajax, Permission(Name = "CreditLineFields")]
		public JsonResult ChangeBrokerSetupFeePercent(long id, decimal? brokerPercent) {
			var cr = _cashRequestsRepository.Get(id);
			if (brokerPercent.HasValue && brokerPercent > 0) {
				cr.BrokerSetupFeePercent = brokerPercent.Value * 0.01M;
			} else {
				cr.BrokerSetupFeePercent = null;
			}
			cr.LoanTemplate = null;
			log.Debug("CashRequest({0}).BrokerSetupFeePercent percent: {1}", id, cr.BrokerSetupFeePercent);

			//TODO update new offer table
			log.Debug("update offer for customer {0} broker setup fee percent {1}", cr.Customer.Id, brokerPercent);

			return Json(new { });
		}

		[HttpPost]
		[Transactional]
		[ValidateJsonAntiForgeryToken]
		[Ajax]
		public JsonResult ToggleCciMark(int id) {
			Customer oCustomer = _customerRepository.Get(id);

			if (oCustomer == null) {
				log.Debug("Customer({0}) not found", id);
				return Json(new { error = "Customer not found.", id = id });
			} // if

			oCustomer.CciMark = !oCustomer.CciMark;

			serviceClient.Instance.AddCciHistory(id, _context.UserId, oCustomer.CciMark);

			log.Debug("Customer({0}).CciMark set to {1}", id, oCustomer.CciMark);

			return Json(new { error = (string)null, id = id, mark = oCustomer.CciMark });
		} // ToggleCciMark

		[HttpPost]
		[Transactional]
		[ValidateJsonAntiForgeryToken]
		[Ajax]
		public JsonResult ToggleIsTest(int id) {
			Customer oCustomer = _customerRepository.Get(id);

			if (oCustomer == null) {
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
		public JsonResult UpdateTrustPilotStatus(int id, string status) {
			Customer oCustomer = _customerRepository.Get(id);

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
			Customer oCustomer = _customerRepository.Get(id);

			if (oCustomer == null) {
				log.Debug("Customer({0}) not found", id);
				return Json(new { error = "Customer not found.", id = id, status = externalStatusID });
			} // if

			var oTsp = (externalStatusID == null) ? null : this.externalCollectionStatusesRepository.Get(externalStatusID);

			if (oTsp == null && externalStatusID != null) {
				log.Debug("Status({0}) not found in the DB repository.", externalStatusID);
				return Json(new { error = "Status not found in the DB repository.", id = id, status = externalStatusID });
			} // if

			oCustomer.ExternalCollectionStatus = oTsp;
			log.Debug("Customer({0}).ExternalCollectionStatus set to {1}", id, externalStatusID);

			return Json(new { error = (string)null, id = id, status = externalStatusID });
		} // ChangeExternalCollectionStatus

		[HttpPost]
		[Transactional]
		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[Permission(Name = "CreditLineFields")]
		public JsonResult AvoidAutomaticDecision(int id, bool enabled) {
			var cust = _customerRepository.Get(id);
			cust.IsAvoid = enabled;
			log.Debug("Customer({0}).IsAvoided = {1}", id, enabled);

			//TODO update new offer table
			log.Debug("update offer for customer {0} avoid auto decision {1}", cust.Id, enabled);

			return Json(new { error = (string)null, id = id, status = cust.IsAvoid });
		}

		[HttpPost]
		[Transactional]
		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[Permission(Name = "CreditLineFields")]
		public JsonResult AllowSendingEmails(long id, bool enabled) {
			var cr = _cashRequestsRepository.Get(id);
			cr.EmailSendingBanned = !enabled;
			cr.LoanTemplate = null;
			log.Debug("CashRequest({0}).EmailSendingBanned = {1}", id, cr.EmailSendingBanned);

			//TODO update new offer table
			log.Debug("update offer for customer {0} EmailSendingBanned {1}", cr.Customer.Id, cr.EmailSendingBanned);
			return Json(new { error = (string)null, id = id, status = enabled });
		}

		[HttpPost]
		[Transactional]
		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[Permission(Name = "CreditLineFields")]
		public JsonResult SpreadSetupFee(long id, bool enabled) {
			var cr = _cashRequestsRepository.Get(id);
			cr.SpreadSetupFee = enabled;
			cr.LoanTemplate = null;
			log.Debug("CashRequest({0}).SpreadSetupFee = {1}", id, cr.SpreadSetupFee);

			//TODO update new offer table
			log.Debug("update offer for customer {0} SpreadSetupFee {1}", cr.Customer.Id, cr.SpreadSetupFee);
			return Json(new { error = (string)null, id = id, status = enabled });
		}

		[HttpPost]
		[Transactional]
		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[Permission(Name = "CreditLineFields")]
		public void IsLoanTypeSelectionAllowed(long id, int loanTypeSelection) {
			var cr = _cashRequestsRepository.Get(id);
			cr.IsLoanTypeSelectionAllowed = loanTypeSelection;
			log.Debug("CashRequest({0}).IsLoanTypeSelectionAllowed = {1}", id, cr.IsLoanTypeSelectionAllowed);

			//TODO update new offer table
			log.Debug("update offer for customer {0} IsLoanTypeSelectionAllowed {1}", cr.Customer.Id, cr.IsLoanTypeSelectionAllowed);
		}

		[HttpPost]
		[Transactional]
		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[Permission(Name = "CreditLineFields")]
		public void ChangeOferValid(int id, string date) {
			var cust = _customerRepository.Get(id);
			if (cust == null)
				return;

			log.Debug("CashRequest({0}).OfferValidUntil = {1}", id, date);

			var dt = FormattingUtils.ParseDateWithCurrentTime(date);
			cust.OfferValidUntil = dt;
			var cr = cust.LastCashRequest;
			cr.LoanTemplate = null;
			cr.OfferValidUntil = dt;

			//TODO update new offer table
			log.Debug("update offer for customer {0} OfferValidUntil {1}", cr.Customer.Id, cr.OfferValidUntil);
		}

		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		[Transactional]
		[Ajax]
		[Permission(Name = "CreditLineFields")]
		public void ChangeStartingDate(int id, string date) {
			var cust = _customerRepository.Get(id);
			if (cust == null)
				return;

			log.Debug("CashRequest({0}).OfferStart = {1}", id, date);
			log.Debug("CashRequest({0}).OfferValidUntil = {1}", id, date);

			var dt = FormattingUtils.ParseDateWithCurrentTime(date);

			int offerValidForHours = (int)Math.Truncate((decimal)CurrentValues.Instance.OfferValidForHours);

			var cr = cust.LastCashRequest;
			cust.OfferStart = dt;
			var offerValidUntil = dt.AddHours(offerValidForHours);
			cust.OfferValidUntil = offerValidUntil;
			cr.LoanTemplate = null;
			cr.OfferStart = dt;
			cr.OfferValidUntil = offerValidUntil;

			//TODO update new offer table
			log.Debug("update offer for customer {0} OfferStart {1} OfferValidUntil {2}", cr.Customer.Id, cr.OfferStart, cr.OfferValidUntil);
		}

		[HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[Permission(Name = "NewCreditLineButton")]
		public JsonResult RunNewCreditLine(int Id, int newCreditLineOption) {
			log.Debug("RunNewCreditLine({0}, {1}) start", Id, newCreditLineOption);

			var customer = _customerRepository.Get(Id);

			new Transactional(() => {
				var cashRequest = _crBuilder.CreateCashRequest(customer, CashRequestOriginator.NewCreditLineBtn);
				cashRequest.LoanType = _loanTypes.GetDefault();

				customer.CreditResult = null;
				customer.OfferStart = cashRequest.OfferStart;
				customer.OfferValidUntil = cashRequest.OfferValidUntil;

				_customerRepository.SaveOrUpdate(customer);
			}).Execute();

			CreditResultStatus? status;
			string strategyError;

			var typedNewCreditLineOption = (NewCreditLineOption)newCreditLineOption;

			if (typedNewCreditLineOption == NewCreditLineOption.SkipEverything) {
				customer.CreditResult = CreditResultStatus.WaitingForDecision;
				_customerRepository.SaveOrUpdate(customer);

				strategyError = null;
				status = customer.CreditResult;
			} else {
				var underwriter = _users.GetUserByLogin(User.Identity.Name);

				ActionMetaData amd = _crBuilder.ForceEvaluate(underwriter.Id, customer, typedNewCreditLineOption, true);

				// Reload from DB
				var updatedCustomer = _customerRepository.Load(customer.Id);

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
		[Transactional]
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
			CashRequest cr = _cashRequestsRepository.Get(id);

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

				cr.DiscountPlan = _discounts.Get(discountPlan);

				Customer c = cr.Customer;
				c.OfferStart = cr.OfferStart;
				c.OfferValidUntil = cr.OfferValidUntil;
				c.ManagerApprovedSum = sum;
				_cashRequestsRepository.SaveOrUpdate(cr);
				_customerRepository.SaveOrUpdate(c);
			}).Execute();

			DateTime now = DateTime.UtcNow;
			/*
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
			 */ 
			//TODO update new offer table
			log.Debug("update offer for customer {0} all the offer is changed", cr.Customer.Id);


			return Json(true);
		} // ChangeCreditLine

		[HttpPost, Ajax, ValidateJsonAntiForgeryToken]
		public JsonResult ActivateMainStrategy(int customerId) {
			int underwriterId = _context.User.Id;

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
		public JsonResult ActivateFinishWizard(int customerId) {
			int underwriterId = _context.User.Id;

			_customerRepository.Get(customerId).AddAlibabaDefaultBankAccount();

			var oArgs = new FinishWizardArgs { CustomerID = customerId, };

			new ServiceClient().Instance.FinishWizard(oArgs, underwriterId);

			return Json(true);
		}

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

				Customer oCustomer = _customerRepository.Get(nCustomerID);

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
			new ServiceClient().Instance.ResetPassword123456(_context.User.Id, nCustomerID, PasswordResetTarget.Customer);
			return Json(true);
		} // ResetPassword123456
	} // class ApplicationInfoController
} // namespace
