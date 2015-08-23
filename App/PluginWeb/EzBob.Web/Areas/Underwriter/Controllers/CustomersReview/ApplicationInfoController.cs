namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview {
	using System;
	using System.Globalization;
	using System.Linq;
	using System.Web.Mvc;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Logger;
	using EzBob.Models.Agreements;
	using EzBob.Web.Areas.Underwriter.Models;
	using EzBob.Web.Code;
	using EzBob.Web.Infrastructure;
	using EzBob.Web.Infrastructure.Attributes;
	using EzBob.Web.Infrastructure.csrf;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Model.Database.UserManagement;
	using EZBob.DatabaseLib.Model.Loans;
	using EZBob.DatabaseLib.Repository;
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
		private readonly ApplicationInfoModelBuilder _infoModelBuilder;
		private readonly IApprovalsWithoutAMLRepository _approvalsWithoutAmlRepository;
		private readonly LoanOptionsRepository loanOptionsRepository;
		private readonly ILoanSourceRepository _loanSources;
		private readonly IUsersRepository _users;
		private readonly IEzbobWorkplaceContext _context;
		private readonly ISuggestedAmountRepository _suggestedAmountRepository;
		private readonly CustomerPhoneRepository customerPhoneRepository;
		private readonly IExternalCollectionStatusesRepository externalCollectionStatusesRepository;
		private readonly ILoanRepository loanRepository;

		private static readonly ASafeLog log = new SafeILog(typeof(ApplicationInfoController));

		public ApplicationInfoController(
			ICustomerRepository customerRepository,
			ICashRequestsRepository cashRequestsRepository,
			ILoanTypeRepository loanTypes,
			LoanLimit limit,
			IDiscountPlanRepository discounts,
			ApplicationInfoModelBuilder infoModelBuilder,
			IApprovalsWithoutAMLRepository approvalsWithoutAMLRepository,
			ILoanSourceRepository loanSources,
			IUsersRepository users,
			IEzbobWorkplaceContext context,
			ISuggestedAmountRepository suggestedAmountRepository,
			CustomerPhoneRepository customerPhoneRepository,
			IExternalCollectionStatusesRepository externalCollectionStatusesRepository,
			LoanOptionsRepository loanOptionsRepository,
			ILoanRepository loanRepository
		) {
			_customerRepository = customerRepository;
			_cashRequestsRepository = cashRequestsRepository;
			_loanTypes = loanTypes;
			_limit = limit;
			_discounts = discounts;
			_infoModelBuilder = infoModelBuilder;
			_approvalsWithoutAmlRepository = approvalsWithoutAMLRepository;
			_loanSources = loanSources;
			_users = users;
			_context = context;
			_suggestedAmountRepository = suggestedAmountRepository;
			serviceClient = new ServiceClient();
			this.customerPhoneRepository = customerPhoneRepository;
			this.externalCollectionStatusesRepository = externalCollectionStatusesRepository;
			this.loanOptionsRepository = loanOptionsRepository;
			this.loanRepository = loanRepository;
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
		[ValidateJsonAntiForgeryToken]
		[HttpPost]
		public JsonResult UpdateBrokerCommissionDefaults(long id, decimal amount) {
			var cr = this._cashRequestsRepository.Get(id);
			if (cr == null) {
				return Json(new { brokerCommission = 0, setupFeePercent = 0 }, JsonRequestBehavior.AllowGet);
			}
			var brokerCommissionPercent = cr.BrokerSetupFeePercent;
			var setupFeePercent = cr.ManualSetupFeePercent;

			if (cr.Customer.Broker != null) {
				BrokerCommissionDefaultCalculator brokerCommissionDefaultCalculator = new BrokerCommissionDefaultCalculator();
				bool hasLoans = cr.Customer.Loans.Any();
				DateTime? firstLoanDate = hasLoans ? cr.Customer.Loans.Min(x => x.Date) : (DateTime?)null;
				Tuple<decimal, decimal> commission = brokerCommissionDefaultCalculator.Calculate(amount, hasLoans, firstLoanDate);
				brokerCommissionPercent = commission.Item1;
				setupFeePercent = commission.Item2;
			}
			return Json(new { brokerCommission = brokerCommissionPercent, setupFeePercent = setupFeePercent }, JsonRequestBehavior.AllowGet);
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
			NewCreditLineOption typedNewCreditLineOption = (NewCreditLineOption)newCreditLineOption;
			User underwriter = this._users.GetUserByLogin(User.Identity.Name);

			log.Debug("RunNewCreditLine({0}, {1}) start", Id, typedNewCreditLineOption);

			ActionMetaData amd = ExecuteNewCreditLine(underwriter.Id, Id, typedNewCreditLineOption);

			// Reload from DB
			Customer customer = this._customerRepository.Load(Id);

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

				cr.DiscountPlan = _discounts.Get(discountPlan);

				Customer c = cr.Customer;
				c.OfferStart = cr.OfferStart;
				c.OfferValidUntil = cr.OfferValidUntil;
				c.ManagerApprovedSum = sum;
				this._cashRequestsRepository.SaveOrUpdate(cr);
				this._customerRepository.SaveOrUpdate(c);
			}).Execute();

			DateTime now = DateTime.UtcNow;

			var decision = this.serviceClient.Instance.AddDecision(this._context.UserId, cr.Customer.Id, new NL_Decisions {
				UserID = this._context.UserId,
				DecisionTime = now,
				DecisionNameID = (int)DecisionActions.Waiting,
				Notes = "Waiting; oldCashRequest: " + cr.Id
				//todo Position =
			}, cr.Id, null);

			log.Info("NL decisionID: {0}, oldCashRequestID: {1}, Error: {2}", decision.Value, cr.Id, decision.Error);

			NL_OfferFees offerFee = new NL_OfferFees() {
				LoanFeeTypeID = (int)NLFeeTypes.SetupFee,
				Percent = manualSetupFeePercent ?? 0,
				OneTimePartPercent = 1,
				DistributedPartPercent = 0
			};
			if (cr.SpreadSetupFee != null && cr.SpreadSetupFee == true) {
				offerFee.LoanFeeTypeID = (int)NLFeeTypes.ServicingFee;
				offerFee.OneTimePartPercent = 0;
				offerFee.DistributedPartPercent = 1;
			}
			NL_OfferFees[] ofeerFees = { offerFee };

			var offer = this.serviceClient.Instance.AddOffer(this._context.UserId, cr.Customer.Id, new NL_Offers {
				DecisionID = decision.Value,
				LoanTypeID = loanType,
				RepaymentIntervalTypeID = (int)RepaymentIntervalTypesId.Month,
				LoanSourceID = loanSource,
				StartTime = FormattingUtils.ParseDateWithCurrentTime(offerStart),
				EndTime = FormattingUtils.ParseDateWithCurrentTime(offerValidUntil),
				RepaymentCount = repaymentPeriod,
				Amount = (decimal)amount,
				MonthlyInterestRate = interestRate,
				CreatedTime = now,
				BrokerSetupFeePercent = brokerSetupFeePercent ?? 0,
				Notes = "offer from ChangeCreditLine, ApplicationInfoController",
				DiscountPlanID = discountPlan,
				IsLoanTypeSelectionAllowed = isLoanTypeSelectionAllowed == 1,
				IsRepaymentPeriodSelectionAllowed = isCustomerRepaymentPeriodSelectionAllowed,
				SendEmailNotification = allowSendingEmail,
				// SetupFeeAddedToLoan = 0 // default 0 TODO EZ-3515
				// InterestOnlyRepaymentCount = 
				//IsAmountSelectionAllowed = 1 default 1 always allowed
			}, ofeerFees);

			log.Info("NL--- offerID: {0}, decisionID: {1} oldCashRequestID: {2}, Error: {3}", offer.Value, decision.Value, cr.Id, offer.Error);
	
			log.Debug("update offer for customer {0} all the offer is changed", cr.Customer.Id);

			return Json(true);
		} // ChangeCreditLine

		[HttpPost, Ajax, ValidateJsonAntiForgeryToken]
		public JsonResult ActivateMainStrategy(int customerId) {
			int underwriterId = _context.User.Id;

			Customer customer = _customerRepository.Get(customerId);

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
			int underwriterId = _context.User.Id;

			_customerRepository.Get(customerId).AddAlibabaDefaultBankAccount();

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

		private ActionMetaData ExecuteNewCreditLine(
			int underwriterID,
			int customerID,
			NewCreditLineOption newCreditLineOption
		) {
			Customer customer = this._customerRepository.Get(customerID);

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
