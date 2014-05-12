namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
	using System.Data;
	using System.Globalization;
	using Code.Agreements;
	using ConfigManager;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using System;
	using System.Web.Mvc;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Model.Database.UserManagement;
	using EZBob.DatabaseLib.Model.Loans;
	using Ezbob.Backend.Models;
	using Infrastructure.Attributes;
	using Models;
	using Code;
	using Infrastructure;
	using Infrastructure.csrf;
	using NHibernate;
	using PaymentServices.PacNet;
	using ServiceClientProxy;
	using StructureMap;
	using log4net;

	public class ApplicationInfoController : Controller
	{
		private readonly ServiceClient serviceClient;
		private readonly ICustomerRepository _customerRepository;
		private readonly ICashRequestsRepository _cashRequestsRepository;
		private readonly ILoanTypeRepository _loanTypes;
		private readonly LoanLimit _limit;
		private readonly IDiscountPlanRepository _discounts;
		private readonly CashRequestBuilder _crBuilder;
		private readonly ApplicationInfoModelBuilder _infoModelBuilder;
		private readonly IPacNetManualBalanceRepository _pacNetManualBalanceRepository;
		private readonly ICustomerStatusesRepository _customerStatusesRepository;
		private readonly IApprovalsWithoutAMLRepository _approvalsWithoutAmlRepository;
		private readonly IConfigurationVariablesRepository _configurationVariablesRepository;
		private readonly ICustomerStatusHistoryRepository _customerStatusHistoryRepository;
		private readonly ILoanSourceRepository _loanSources;
		private readonly IUsersRepository _users;
		private readonly IEzbobWorkplaceContext _context;
		private readonly ISuggestedAmountRepository _suggestedAmountRepository;

		private static readonly ILog Log = LogManager.GetLogger(typeof(ApplicationInfoController));

		public ApplicationInfoController(
			ICustomerRepository customerRepository,
			ICustomerStatusesRepository customerStatusesRepository,
			ICashRequestsRepository cashRequestsRepository,
			ILoanTypeRepository loanTypes,
			LoanLimit limit,
			IDiscountPlanRepository discounts,
			CashRequestBuilder crBuilder,
			ApplicationInfoModelBuilder infoModelBuilder,
			IPacNetManualBalanceRepository pacNetManualBalanceRepository,
			IApprovalsWithoutAMLRepository approvalsWithoutAMLRepository,
			IConfigurationVariablesRepository configurationVariablesRepository,
			ICustomerStatusHistoryRepository customerStatusHistoryRepository,
			ILoanSourceRepository loanSources,
			IUsersRepository users,
			IEzbobWorkplaceContext context,
			ISuggestedAmountRepository suggestedAmountRepository)
		{
			_customerRepository = customerRepository;
			_cashRequestsRepository = cashRequestsRepository;
			_loanTypes = loanTypes;
			_limit = limit;
			_discounts = discounts;
			_crBuilder = crBuilder;
			_infoModelBuilder = infoModelBuilder;
			_pacNetManualBalanceRepository = pacNetManualBalanceRepository;
			_customerStatusesRepository = customerStatusesRepository;
			_approvalsWithoutAmlRepository = approvalsWithoutAMLRepository;
			_configurationVariablesRepository = configurationVariablesRepository;
			_customerStatusHistoryRepository = customerStatusHistoryRepository;
			_loanSources = loanSources;
			_users = users;
			_context = context;
			_suggestedAmountRepository = suggestedAmountRepository;
			serviceClient = new ServiceClient();
		}

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpGet]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public JsonResult Index(int id)
		{
			var customer = _customerRepository.Get(id);
			var m = new ApplicationInfoModel();
			var cr = customer.LastCashRequest;
			_infoModelBuilder.InitApplicationInfo(m, customer, cr);
			return Json(m, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[OutputCache(VaryByParam = "status", Duration = int.MaxValue)]
		public JsonResult GetIsStatusWarning(int status)
		{
			bool res = _customerStatusesRepository.GetIsWarning(status);
			return Json(res, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public void LogStatusChange(int newStatus, int prevStatus, int customerId)
		{
			var newEntry = new CustomerStatusHistory();
			newEntry.Username = User.Identity.Name;
			newEntry.Timestamp = DateTime.UtcNow;
			newEntry.CustomerId = customerId;
			newEntry.PreviousStatus = prevStatus;
			newEntry.NewStatus = newStatus;
			_customerStatusHistoryRepository.SaveOrUpdate(newEntry);
		}

		[HttpPost]
		[Transactional]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[Permission(Name = "CreditLineFields")]
		public JsonResult ChangeCashRequestOpenCreditLine(long id, double amount, string method, string medal, decimal? value)
		{
			_limit.Check(amount);
			var cr = _cashRequestsRepository.Get(id);
			int step = CurrentValues.Instance.GetCashSliderStep;
			cr.ManagerApprovedSum = Math.Round(amount / step, MidpointRounding.AwayFromZero) * step;
			cr.LoanTemplate = null;
			_cashRequestsRepository.SaveOrUpdate(cr);

			Log.DebugFormat("CashRequest({0}).ManagerApprovedSum = {1}", id, cr.ManagerApprovedSum);

			if (value.HasValue && value.Value > 0)
			{
				var underwriter = _context.User;
				var sa = new SuggestedAmount
					{
						InsertDate = DateTime.UtcNow,
						Customer = cr.Customer,
						Underwriter = underwriter,
						CashRequest = cr,
						Amount = value.Value,
						Medal = medal,
						Method = method
					};
				_suggestedAmountRepository.SaveOrUpdate(sa);
			}
			return Json(true);
		}

		[HttpPost]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		[Ajax]
		public void SaveApproveWithoutAML(int customerId, bool doNotShowAgain)
		{
			Log.DebugFormat("Saving approve without AML. Customer:{0} doNotShowAgain = {1}", customerId, doNotShowAgain);

			var entry = new ApprovalsWithoutAML
			{
				CustomerId = customerId,
				DoNotShowAgain = doNotShowAgain,
				Timestamp = DateTime.UtcNow,
				Username = User.Identity.Name
			};

			_approvalsWithoutAmlRepository.SaveOrUpdate(entry);
		}

		[HttpPost]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[Permission(Name = "PacnetManualButton")]
		public JsonResult SavePacnetManual(int amount, int limit)
		{
			var newEntry = new PacNetManualBalance
			{
				Date = DateTime.UtcNow,
				Enabled = true,
				Amount = amount,
				Username = User.Identity.Name
			};

			_pacNetManualBalanceRepository.SaveOrUpdate(newEntry);
			return Json(true);
		}

		[HttpPost]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[Permission(Name = "PacnetManualButton")]
		public JsonResult DisableTodaysPacnetManual(bool isSure)
		{
			if (isSure)
			{
				_pacNetManualBalanceRepository.DisableCurrents();
			}
			return Json(true);
		}

		[HttpPost]
		[Ajax]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		[Permission(Name = "CreditLineFields")]
		public void LoanType(long id, int loanType)
		{
			var cr = _cashRequestsRepository.Get(id);
			var loanT = _loanTypes.Get(loanType);
			cr.LoanType = loanT;
			cr.RepaymentPeriod = loanT.RepaymentPeriod;
			cr.LoanTemplate = null;
			Log.DebugFormat("CashRequest({0}).LoanType = {1}", id, cr.LoanType.Name);
		}

		[HttpPost]
		[Ajax]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public JsonResult DiscountPlan(long id, int discountPlanId)
		{
			var cr = _cashRequestsRepository.Get(id);
			var discount = _discounts.Get(discountPlanId);
			cr.DiscountPlan = discount;
			//cr.LoanTemplate = null;
			//Log.DebugFormat("CashRequest({0}).Discount = {1}", id, cr.DiscountPlan.Name);
			return Json(new { });
		}

		[HttpPost]
		[Ajax]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public JsonResult LoanSource(long id, int LoanSourceID)
		{
			var cr = _cashRequestsRepository.Get(id);
			cr.LoanSource = _loanSources.Get(LoanSourceID);

			if (cr.LoanSource == null)
				cr.IsCustomerRepaymentPeriodSelectionAllowed = true;
			else
			{
				cr.IsCustomerRepaymentPeriodSelectionAllowed = cr.LoanSource.IsCustomerRepaymentPeriodSelectionAllowed;
				cr.IsLoanTypeSelectionAllowed = cr.LoanSource.IsCustomerRepaymentPeriodSelectionAllowed ? 1 : 0;
				if (cr.LoanSource.DefaultRepaymentPeriod.HasValue)
					cr.RepaymentPeriod = cr.LoanSource.DefaultRepaymentPeriod.Value;
			} // if

			return Json(new { });
		} // LoanSource

		[HttpPost]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[Permission(Name = "CreditLineFields")]
		public JsonResult ChangeCashRequestInterestRate(long id, decimal interestRate)
		{
			var cr = _cashRequestsRepository.Get(id);
			cr.InterestRate = interestRate / 100;
			cr.LoanTemplate = null;

			Log.DebugFormat("CashRequest({0}).InterestRate = {1}", id, cr.InterestRate);

			return Json(true);
		}

		[HttpPost]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[Permission(Name = "CreditLineFields")]
		public JsonResult ChangeCashRequestRepaymentPeriod(long id, int period)
		{
			var cr = _cashRequestsRepository.Get(id);
			cr.RepaymentPeriod = period;
			cr.LoanTemplate = null;

			Log.DebugFormat("CashRequest({0}).RepaymentPeriod = {1}", id, period);

			return Json(true);
		}

		[HttpPost]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		[ValidateJsonAntiForgeryToken]
		[Ajax]
		public void SaveDetails(int id, string details)
		{
			var cust = _customerRepository.Get(id);
			if (cust == null)
				return;

			cust.Details = details;
		}

		[HttpPost]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[Permission(Name = "CreditLineFields")]
		public void ChangeSetupFee(long id, bool enbaled)
		{
			var cr = _cashRequestsRepository.Get(id);
			cr.UseSetupFee = enbaled;
			cr.LoanTemplate = null;
			Log.DebugFormat("CashRequest({0}).UseSetupFee = {1}", id, enbaled);
		}

		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		[HttpPost, ValidateJsonAntiForgeryToken, Ajax, Permission(Name = "CreditLineFields")]
		public void ChangeBrokerSetupFee(long id, bool enbaled)
		{
			var cr = _cashRequestsRepository.Get(id);
			cr.UseBrokerSetupFee = enbaled;
			cr.LoanTemplate = null;
			Log.DebugFormat("CashRequest({0}).UseBrokerSetupFee = {1}", id, enbaled);
		}

		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		[HttpPost, ValidateJsonAntiForgeryToken, Ajax, Permission(Name = "CreditLineFields")]
		public void ChangeManualSetupFeePercent(long id, decimal? manualPercent)
		{
			var cr = _cashRequestsRepository.Get(id);
			if (manualPercent.HasValue && manualPercent > 0)
			{
				cr.ManualSetupFeePercent = manualPercent.Value * 0.01M;
			}
			else
			{
				cr.ManualSetupFeePercent = null;
			}
			cr.LoanTemplate = null;
			Log.DebugFormat("CashRequest({0}).ManualSetupFee percent: {1}", id, cr.ManualSetupFeePercent);
		}

		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		[HttpPost, ValidateJsonAntiForgeryToken, Ajax, Permission(Name = "CreditLineFields")]
		public void ChangeManualSetupFeeAmount(long id, int? manualAmount)
		{
			var cr = _cashRequestsRepository.Get(id);
			cr.ManualSetupFeeAmount = manualAmount.HasValue && manualAmount.Value > 0 ? manualAmount.Value : (int?)null;
			cr.LoanTemplate = null;
			Log.DebugFormat("CashRequest({0}).ManualSetupFee amount: {1}", id, manualAmount);
		}

		[HttpPost]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		[ValidateJsonAntiForgeryToken]
		[Ajax]
		public JsonResult ToggleCciMark(int id)
		{
			Customer oCustomer = _customerRepository.Get(id);

			if (oCustomer == null)
			{
				Log.DebugFormat("Customer({0}) not found", id);
				return Json(new { error = "Customer not found.", id = id });
			} // if

			oCustomer.CciMark = !oCustomer.CciMark;
			serviceClient.Instance.GetSpResultTable("AddCciHistory", new[] { "Username", User.Identity.Name, "CustomerId", id.ToString(CultureInfo.InvariantCulture), "CciMark", oCustomer.CciMark.ToString() });

			Log.DebugFormat("Customer({0}).CciMark set to {1}", id, oCustomer.CciMark);

			return Json(new { error = (string)null, id = id, mark = oCustomer.CciMark });
		} // ToggleCciMark



		[HttpPost]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		[ValidateJsonAntiForgeryToken]
		[Ajax]
		public JsonResult ToggleIsTest(int id)
		{
			Customer oCustomer = _customerRepository.Get(id);

			if (oCustomer == null)
			{
				Log.DebugFormat("Customer({0}) not found", id);
				return Json(new { error = "Customer not found.", id = id });
			} // if

			oCustomer.IsTest = !oCustomer.IsTest;
			Log.DebugFormat("Customer({0}).IsTest set to {1}", id, oCustomer.IsTest);

			return Json(new { error = (string)null, id = id, isTest = oCustomer.IsTest });
		} // ToggleIsTest

		[HttpPost]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		[ValidateJsonAntiForgeryToken]
		[Ajax]
		public JsonResult UpdateTrustPilotStatus(int id, string status)
		{
			Customer oCustomer = _customerRepository.Get(id);

			if (oCustomer == null)
			{
				Log.DebugFormat("Customer({0}) not found", id);
				return Json(new { error = "Customer not found.", id = id, status = status });
			} // if

			var oHelper = ObjectFactory.GetInstance<DatabaseDataHelper>();

			TrustPilotStauses nStatus;

			if (!Enum.TryParse<TrustPilotStauses>(status, true, out nStatus))
			{
				Log.DebugFormat("Status({0}) not found", status);
				return Json(new { error = "Failed to parse status.", id = id, status = status });
			} // if

			var oTsp = oHelper.TrustPilotStatusRepository.Find(nStatus);

			if (oTsp == null)
			{
				Log.DebugFormat("Status({0}) not found in the DB repository.", status);
				return Json(new { error = "Status not found in the DB repository.", id = id, status = status });
			} // if

			oCustomer.TrustPilotStatus = oTsp;
			Log.DebugFormat("Customer({0}).TrustPilotStatus set to {1}", id, status);

			return Json(new { error = (string)null, id = id, status = status });
		} // UpdateTrustPilotStatus

		[HttpPost]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[Permission(Name = "CreditLineFields")]
		public void AvoidAutomaticDecision(int id, bool enbaled)
		{
			var cust = _customerRepository.Get(id);
			cust.IsAvoid = enbaled;
			Log.DebugFormat("Customer({0}).IsAvoided = {1}", id, enbaled);
		}

		[HttpPost]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[Permission(Name = "CreditLineFields")]
		public void AllowSendingEmails(long id, bool enbaled)
		{
			var cr = _cashRequestsRepository.Get(id);
			cr.EmailSendingBanned = !enbaled;
			cr.LoanTemplate = null;
			Log.DebugFormat("CashRequest({0}).EmailSendingBanned = {1}", id, cr.EmailSendingBanned);
		}

		[HttpPost]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[Permission(Name = "CreditLineFields")]
		public void IsLoanTypeSelectionAllowed(long id, int loanTypeSelection)
		{
			var cr = _cashRequestsRepository.Get(id);
			cr.IsLoanTypeSelectionAllowed = loanTypeSelection;
			Log.DebugFormat("CashRequest({0}).IsLoanTypeSelectionAllowed = {1}", id, cr.IsLoanTypeSelectionAllowed);
		}

		[HttpPost]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[Permission(Name = "CreditLineFields")]
		public void ChangeOferValid(int id, string date)
		{
			var cust = _customerRepository.Get(id);
			if (cust == null)
				return;

			Log.DebugFormat("CashRequest({0}).OfferValidUntil = {1}", id, date);

			var dt = FormattingUtils.ParseDateWithCurrentTime(date);
			cust.OfferValidUntil = dt;
			var cr = cust.LastCashRequest;
			cr.LoanTemplate = null;
			cr.OfferValidUntil = dt;
		}

		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		[Ajax]
		[Permission(Name = "CreditLineFields")]
		public void ChangeStartingDate(int id, string date)
		{
			var cust = _customerRepository.Get(id);
			if (cust == null)
				return;

			Log.DebugFormat("CashRequest({0}).OfferStart = {1}", id, date);
			Log.DebugFormat("CashRequest({0}).OfferValidUntil = {1}", id, date);

			var dt = FormattingUtils.ParseDateWithCurrentTime(date);

			int offerValidForHours = (int)_configurationVariablesRepository.GetByNameAsDecimal("OfferValidForHours");

			var cr = cust.LastCashRequest;
			cust.OfferStart = dt;
			var offerValidUntil = dt.AddHours(offerValidForHours);
			cust.OfferValidUntil = offerValidUntil;
			cr.LoanTemplate = null;
			cr.OfferStart = dt;
			cr.OfferValidUntil = offerValidUntil;
		}


		[HttpPost]
		[Transactional]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[Permission(Name = "NewCreditLineButton")]
		public JsonResult RunNewCreditLine(int Id, int newCreditLineOption)
		{
			return Json(new { Message = "Go to new mode" });
		}

		[HttpPost]
		[Transactional]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[Permission(Name = "NewCreditLineButton")]
		public JsonResult RunNewCreditLineNewMode1(int Id, int newCreditLineOption)
		{
			var customer = _customerRepository.Get(Id);

			var cashRequest = _crBuilder.CreateCashRequest(customer, CashRequestOriginator.NewCreditLineBtn);
			cashRequest.LoanType = _loanTypes.GetDefault();

			customer.CreditResult = null;
			customer.OfferStart = cashRequest.OfferStart;
			customer.OfferValidUntil = cashRequest.OfferValidUntil;

			return Json(new { });
		}

		[HttpPost]
		[Transactional]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[Permission(Name = "NewCreditLineButton")]
		public JsonResult RunNewCreditLineNewMode2(int Id, int newCreditLineOption)
		{
			var customer = _customerRepository.Get(Id);
			var underwriter = _users.GetUserByLogin(User.Identity.Name);
			_crBuilder.ForceEvaluate(underwriter.Id, customer, (NewCreditLineOption)newCreditLineOption, false, true);
			return Json(new { });
		}

		[HttpPost, Transactional, Ajax, ValidateJsonAntiForgeryToken]
		public JsonResult ChangeCreditLine(long id, int loanType, double amount, decimal interestRate, int repaymentPeriod, string offerStart, string offerValidUntil, bool useSetupFee, bool useBrokerSetupFee, bool allowSendingEmail, int isLoanTypeSelectionAllowed, int discountPlan, decimal? manualSetupFeeAmount, decimal? manualSetupFeePercent)
		{
			var cr = _cashRequestsRepository.Get(id);
			var loanT = _loanTypes.Get(loanType);
			cr.LoanType = loanT;
			int step = CurrentValues.Instance.GetCashSliderStep;
			cr.ManagerApprovedSum = cr.ManagerApprovedSum = Math.Round(amount / step, MidpointRounding.AwayFromZero) * step; 
			cr.InterestRate = interestRate;
			cr.RepaymentPeriod = repaymentPeriod;
			cr.OfferStart = FormattingUtils.ParseDateWithCurrentTime(offerStart);
			cr.OfferValidUntil = FormattingUtils.ParseDateWithCurrentTime(offerValidUntil);

			cr.UseSetupFee = useSetupFee;
			cr.UseBrokerSetupFee = useBrokerSetupFee;
			cr.ManualSetupFeeAmount = (int?)manualSetupFeeAmount;
			cr.ManualSetupFeePercent = manualSetupFeePercent;

			cr.EmailSendingBanned = !allowSendingEmail;
			cr.LoanTemplate = null;
			cr.IsLoanTypeSelectionAllowed = isLoanTypeSelectionAllowed;
			cr.DiscountPlan = _discounts.Get(discountPlan);

			Customer c = cr.Customer;
			c.OfferStart = cr.OfferStart;
			c.OfferValidUntil = cr.OfferValidUntil;

			return Json(true);
		}

		[HttpPost, Ajax, ValidateJsonAntiForgeryToken]
		public JsonResult ActivateMainStrategy(int customerId)
		{
			int underwriterId = _context.User.Id;

			new ServiceClient().Instance.MainStrategy1(underwriterId, customerId, NewCreditLineOption.UpdateEverythingAndApplyAutoRules, 0);

			return Json(true);
		}

		[HttpPost, Ajax, ValidateJsonAntiForgeryToken]
		public JsonResult ActivateFinishWizard(int customerId)
		{
			int underwriterId = _context.User.Id;

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
					ObjectFactory.GetInstance<ILoanHistoryRepository>(),
					ObjectFactory.GetInstance<IPacnetService>(),
					ObjectFactory.GetInstance<IAgreementsGenerator>(),
					ObjectFactory.GetInstance<IEzbobWorkplaceContext>(),
					ObjectFactory.GetInstance<LoanBuilder>(),
					ObjectFactory.GetInstance<AvailableFundsValidator>(),
					ObjectFactory.GetInstance<ISession>()
				);

				Customer oCustomer = _customerRepository.Get(nCustomerID);

				DateTime oDate = DateTime.ParseExact(sDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);

				lc.CreateLoan(oCustomer, nAmount, null, oDate);

				return Json(new { success = true, error = false, });
			}
			catch (Exception e)
			{
				Log.Warn("Could not create a hidden loan.", e);
				return Json(new { success = false, error = e.Message, });
			} // try
		}
	}
}
