namespace EzBob.Web.Areas.Customer.Models {
	using System;
	using System.Linq;
	using System.Web;
	using ConfigManager;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using EzBob.Models;
	using EzBob.Web.Areas.Underwriter.Models;
	using EzBob.Web.Code;
	using EzBob.Web.Infrastructure;
	using EzBob.Web.Infrastructure.Email;
	using EzBob.Web.Models;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Broker;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Database.Mapping;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Model.Database.UserManagement;
	using EZBob.DatabaseLib.Model.Loans;
	using EZBob.DatabaseLib.Repository;
	using log4net;
	using PaymentServices.Calculators;
	using ServiceClientProxy;

	public class CustomerModelBuilder {
		public CustomerModelBuilder(
			ISecurityQuestionRepository questions,
			ICustomerRepository customerRepository,
			IUsersRepository users,
			PaymentRolloverRepository paymentRolloverRepository,
			DatabaseDataHelper oDbHelper,
			WhiteLabelProviderRepository whiteLabelProviderRepository
		) {
			m_oQuestions = questions;
			m_oCustomerRepository = customerRepository;
			m_oUsers = users;
			m_oPaymentRolloverRepository = paymentRolloverRepository;
			m_oChangeLoanDetailsModelBuilder = new ChangeLoanDetailsModelBuilder();
			m_oExperianDirectors = oDbHelper.ExperianDirectorRepository;
			_whiteLabelProviderRepository = whiteLabelProviderRepository;
		} // constructor

		public WizardModel BuildWizardModel(
			Customer cus,
			HttpSessionStateBase session,
			string profile,
			string requestUrl,
			bool isProfile
		) {
			CustomerOrigin uiOrigin = cus == null ? UiCustomerOrigin.Get() : cus.CustomerOrigin;

			var wizardModel = new WizardModel();

			Log.InfoFormat(
				"BuildWizardModel URL: {0} origin {1} customer {2}",
				requestUrl,
				uiOrigin.Name,
				cus == null ? "null" : cus.Id.ToString()
			);

			var customerModel = new CustomerModel {
				loggedIn = cus != null,
				bankAccountAdded = false,
				Origin = uiOrigin.Name,
			};

			if (!string.IsNullOrEmpty(profile)) {
				wizardModel.WhiteLabel = _whiteLabelProviderRepository.GetByName(profile);
				customerModel.IsWhiteLabel = wizardModel.WhiteLabel != null;
				customerModel.WhiteLabelId = wizardModel.WhiteLabel != null ? wizardModel.WhiteLabel.Id : 0;
			}

			wizardModel.Customer = customerModel;

			if (!customerModel.loggedIn) {
				customerModel.IsBrokerFill =
					(session[Constant.Broker.FillsForCustomer] ?? Constant.No).ToString() == Constant.Yes;

				return wizardModel;
			} // if

			if (cus == null)
				return wizardModel;

			var customer = m_oCustomerRepository.GetAndInitialize(cus.Id);

			if (customer == null)
				return wizardModel;

			var user = m_oUsers.Get(cus.Id);

			customerModel.Origin = customer.CustomerOrigin.Name;
			customerModel.IsTest = customer.IsTest;

			if (customer.WhiteLabel != null) {
				wizardModel.WhiteLabel = customer.WhiteLabel;
				customerModel.IsWhiteLabel = wizardModel.WhiteLabel != null;
				customerModel.WhiteLabelId = customer.WhiteLabel.Id;
			} // if

			customerModel.Id = customer.Id;
			customerModel.RefNumber = customer.RefNumber;
			customerModel.userName = user.Name;
			customerModel.Email = customer.Name;
			customerModel.EmailState = EmailConfirmationState.Get(customer);

			customerModel.CustomerPersonalInfo = customer.PersonalInfo;

			customerModel.IsAlibaba = customer.IsAlibaba;

			if (customer.PropertyStatus != null) {
				customerModel.PropertyStatus = new PropertyStatusModel {
					Id = customer.PropertyStatus.Id,
					Description = customer.PropertyStatus.Description,
					IsOwnerOfOtherProperties = customer.PropertyStatus.IsOwnerOfOtherProperties,
					IsOwnerOfMainAddress = customer.PropertyStatus.IsOwnerOfMainAddress
				};
			} else {
				customerModel.PropertyStatus = new PropertyStatusModel();
			} // if

			customerModel.BusinessTypeReduced = customerModel.CustomerPersonalInfo == null ?
				TypeOfBusinessReduced.Personal.ToString() : customer.PersonalInfo.TypeOfBusiness.Reduce().ToString();

			customerModel.mpAccounts = customer.GetMarketPlaces();
			customerModel.CreditSum = customer.CreditSum;
			customerModel.CreditResult = customer.CreditResult.ToString();
			customerModel.Status = customer.Status.ToString();

			var account = new AccountSettingsModel {
				SecurityQuestions = m_oQuestions.GetQuestions(),

				SecurityQuestionModel = new SecurityQuestionModel {
					Question = user.SecurityQuestion == null ? 0 : user.SecurityQuestion.Id,
					Answer = user.SecurityAnswer
				},
			};

			customerModel.AccountSettings = account;

			customerModel.GreetingMailSentDate = customer.GreetingMailSentDate;

			var company = customer.Company;

			customerModel.CanHaveDirectors = false;

			if (company != null) {
				customerModel.CanHaveDirectors = company.TypeOfBusiness != TypeOfBusiness.SoleTrader;

				customerModel.CompanyInfo = CompanyInfoMap.FromCompany(company);
				customerModel.CompanyAddress = company.CompanyAddress.ToArray();

				customerModel.CompanyInfo.Directors.AddRange(
					m_oExperianDirectors.Find(customer.Id)
					.Select(ed => DirectorModel.FromExperianDirector(ed, company.TypeOfBusiness.Reduce()))
				);
			} // if

			if (customer.AddressInfo != null) {
				customerModel.PersonalAddress = customer.AddressInfo.PersonalAddress.ToArray();
				customerModel.PrevPersonAddresses = customer.AddressInfo.PrevPersonAddresses.ToArray();
				customerModel.OtherPropertiesAddresses = customer.AddressInfo.OtherPropertiesAddresses.ToArray();
			} // if

			customerModel.CompanyEmployeeCountInfo = new CompanyEmployeeCountInfo(customer.Company);

			customerModel.CustomerStatusName = customer.CollectionStatus.Name;

			// customerModel.LoyaltyPoints = customer.LoyaltyPoints();
			customerModel.IsOffline = customer.IsOffline;
			customerModel.IsDisabled = !customer.CollectionStatus.IsEnabled;

			customerModel.LastSavedWizardStep = ((customer.WizardStep == null) || customer.WizardStep.TheLastOne)
				? string.Empty
				: customer.WizardStep.Name;

			customerModel.QuickOffer = BuildQuickOfferModel(customer);

			CustomerRequestedLoan ra = customer.CustomerRequestedLoan.OrderByDescending(x => x.Created).FirstOrDefault();

			customerModel.RequestedLoan = ra ?? new CustomerRequestedLoan();

			customerModel.IsBrokerFill = customer.FilledByBroker;
			customerModel.DefaultCardSelectionAllowed = customer.DefaultCardSelectionAllowed;

			var cr = customer.LastCashRequest;

			customerModel.IsCurrentCashRequestFromQuickOffer = (cr != null) && (cr.QuickOffer != null);

			customerModel.IsLoanDetailsFixed = !m_oChangeLoanDetailsModelBuilder.IsAmountChangingAllowed(cr);

			customerModel.LastCashRequestID = (cr == null) ? 0 : cr.Id;

			if (isProfile)
				BuildProfileModel(customerModel, customer);

			return wizardModel;
		} // BuildWizardModel

		private void BuildProfileModel(CustomerModel customerModel, Customer customer) {
			customerModel.FirstName = string.Empty;
			customerModel.MiddleName = string.Empty;
			customerModel.LastName = string.Empty;

			if (customer.PersonalInfo != null) {
				customerModel.FirstName = customer.PersonalInfo.FirstName;
				customerModel.MiddleName = customer.PersonalInfo.MiddleInitial;
				customerModel.LastName = customer.PersonalInfo.Surname;
			} // if

			customerModel.bankAccountAdded = customer.HasBankAccount;

			if (customer.HasBankAccount) {
				customerModel.BankAccountNumber = customer.BankAccount.AccountNumber;
				customerModel.SortCode = customer.BankAccount.SortCode;
			} // if

			customerModel.LastApprovedLoanTypeID = 0;
			customerModel.LastApprovedRepaymentPeriod = 0;
			customerModel.IsLastApprovedLoanSourceEu = false;
			customerModel.IsLastApprovedLoanSourceCOSME = false;
			customerModel.SignedLegalID = 0;
			customerModel.LastApprovedAmount = 0;
			customerModel.HasApprovalChance = customer.HasApprovalChance;
			customerModel.IsLoanTypeSelectionAllowed = customer.IsLoanTypeSelectionAllowed;
			if (customer.LastCashRequest != null) {
				customerModel.LastApprovedAmount = (int)(customer.LastCashRequest.ManagerApprovedSum ?? 0);

				customerModel.LastApprovedLoanTypeID = customer.LastCashRequest.LoanType.Id;
				customerModel.LastRepaymentPeriod = customer.LastCashRequest.RepaymentPeriod;
				customerModel.LastApprovedRepaymentPeriod = customer.LastCashRequest.ApprovedRepaymentPeriod ?? customer.LastCashRequest.RepaymentPeriod;
				customerModel.IsLastApprovedLoanSourceEu =
					customer.LastCashRequest.LoanSource.Name == LoanSourceName.EU.ToString();
				customerModel.IsLastApprovedLoanSourceCOSME =
					customer.LastCashRequest.LoanSource.Name == LoanSourceName.COSME.ToString();

				LoanLegal lastll = customer.LastCashRequest.LoanLegals.LastOrDefault();

				customerModel.SignedLegalID = (lastll == null) ? 0 : lastll.Id;

			    customerModel.IsCustomerRepaymentPeriodSelectionAllowed = customer.LastCashRequest.IsCustomerRepaymentPeriodSelectionAllowed;
			} // if

			customerModel.Medal = customer.Medal.HasValue ? customer.Medal.ToString() : "";

			customerModel.OfferStart = customer.OfferStart;
			customerModel.OfferValidUntil = customer.OfferValidUntil;


			customerModel.Loans = customer.Loans
				.OrderBy(l => l.Status)
				.ThenByDescending(l => l.Date)
				.Select(l => LoanModel.FromLoan(l,
					new LoanRepaymentScheduleCalculator(l, null, CurrentValues.Instance.AmountToChargeFrom),
					new LoanRepaymentScheduleCalculator(l, DateTime.UtcNow, CurrentValues.Instance.AmountToChargeFrom)))
				.ToList();

			customerModel.TotalBalance = customerModel.Loans.Sum(l => l.Balance);
			customerModel.PrincipalBalance = customer.ActiveLoans.Sum(l => l.LoanAmount);
			customerModel.TotalEarlyPayment = customerModel.Loans.Sum(l => l.TotalEarlyPayment);

			customerModel.TotalLatePayment = customerModel.Loans
				.Where(l => l.Status == LoanStatus.Late.ToString())
				.Sum(l => l.Late);

			var nextPayment = (
				from loan in customer.ActiveLoans
				from repayment in loan.Schedule
				where repayment.AmountDue > 0
				where repayment.Status == LoanScheduleStatus.StillToPay || repayment.Status == LoanScheduleStatus.Late
				orderby repayment.Date
				select repayment
			).FirstOrDefault();

			if (nextPayment != null) {
				customerModel.NextPayment = nextPayment.AmountDue;
				customerModel.NextPaymentDate = nextPayment.Date;
				customerModel.IsEarly =
					nextPayment.Date > DateTime.UtcNow && (
						nextPayment.Date.Year != DateTime.UtcNow.Year ||
						nextPayment.Date.Month != DateTime.UtcNow.Month ||
						nextPayment.Date.Day != DateTime.UtcNow.Day
					);
			} // if

			try {
				customerModel.TotalPayEarlySavings = new LoanPaymentFacade().CalculateSavings(customer, DateTime.UtcNow);
			} catch (Exception) {
				//do nothing
			} // try

			var payments =
				from loan in customer.Loans
				from tran in loan.Transactions
				where tran is PaypointTransaction
				orderby tran.PostDate descending
				select tran;

			var lastPayment = payments.OfType<PaypointTransaction>().FirstOrDefault();

			if (lastPayment != null) {
				customerModel.LastPaymentTotal = lastPayment.Amount;
				customerModel.LastPaymentPrincipal = lastPayment.Principal;
				customerModel.LastPaymentInterest = lastPayment.Interest;
				customerModel.LastPaymentFees = lastPayment.Fees;
			} // if

			var isDefault =
				customer.CollectionStatus != null &&
				customer.CollectionStatus.IsDefault;

			customerModel.BlockTakingLoan = customer.BlockTakingLoan;
			//customerModel.Perks = isDefault ? null : m_oPerksRepository.GetActivePerk();

			customerModel.TrustPilotStatusID = customer.TrustPilotStatus.ID;

			// Currently disabled trust pilot for EVL customers
			customerModel.TrustPilotReviewEnabled =
				CurrentValues.Instance.TrustPilotReviewEnabled &&
				customer.CustomerOrigin.IsEzbob();

			customerModel.PayPointCards = FillPayPointCards(customer);

			customerModel.ActiveRollovers = m_oPaymentRolloverRepository
				.GetRolloversForCustomer(customer.Id)
				.Where(x => x.Status == RolloverStatus.New)
				.Select(x => new RolloverModel {
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

			customerModel.ApplyCount = customer.ApplyCount;
			customerModel.IsDefaultCustomerStatus = customer.CollectionStatus.IsDefault;
			customerModel.HasRollovers = customerModel.ActiveRollovers.Any();

			SafeReader sr = DbConnectionGenerator.Get(new SafeILog(this)).GetFirst(
				"LoadActiveLotteries",
				CommandSpecies.StoredProcedure,
				new QueryParameter("UserID", customer.Id),
				new QueryParameter("Now", DateTime.UtcNow)
			);

			customerModel.LotteryPlayerID = sr.IsEmpty ? string.Empty : ((Guid)sr["UniqueID"]).ToString("N");
			customerModel.LotteryCode = sr["LotteryCode"];
		} // BuildProfileModel

		private QuickOfferModel BuildQuickOfferModel(Customer c) {
			if ((c == null) || (c.QuickOffer == null) || (c.QuickOffer.ExpirationDate < DateTime.UtcNow))
				return null;

			return new QuickOfferModel {
				ID = c.QuickOffer.ID,
				Amount = c.QuickOffer.Amount,
				CreationDate = c.QuickOffer.CreationDate,
				ExpirationDate = c.QuickOffer.ExpirationDate,
				Aml = c.QuickOffer.Aml,
				BusinessScore = c.QuickOffer.BusinessScore,
				IncorporationDate = c.QuickOffer.IncorporationDate,
				TangibleEquity = c.QuickOffer.TangibleEquity,
				TotalCurrentAssets = c.QuickOffer.TotalCurrentAssets,
				ImmediateTerm = c.QuickOffer.ImmediateTerm,
				ImmediateInterestRate = c.QuickOffer.ImmediateInterestRate,
				ImmediateSetupFee = c.QuickOffer.ImmediateSetupFee,
				PotentialAmount = c.QuickOffer.PotentialAmount,
				PotentialTerm = c.QuickOffer.PotentialTerm,
				PotentialInterestRate = c.QuickOffer.PotentialInterestRate,
				PotentialSetupFee = c.QuickOffer.PotentialSetupFee,
			};
		} // BuildQuickOfferModel

		private decimal GetRolloverPayValue(Loan loan) {
			var payEarlyCalc = new LoanRepaymentScheduleCalculator(
				loan,
				DateTime.UtcNow,
				CurrentValues.Instance.AmountToChargeFrom
			);
			var state = payEarlyCalc.GetState();

			try {
				ServiceClient service = new ServiceClient();
				long nlLoanId = service.Instance.GetLoanByOldID(loan.Id, loan.Customer.Id, 1).Value;
				if (nlLoanId > 0) {
					var nlModel = service.Instance.GetLoanState(loan.Customer.Id, nlLoanId, DateTime.UtcNow, 1, true).Value;
					this.Log.InfoFormat("<<< NL_Compare: {0}\n===============loan: {1}  >>>", nlModel, loan);
				}
				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
				this.Log.InfoFormat("<<< NL_Compare fail at: {0}, err: {1}", Environment.StackTrace, ex.Message);
			}

			return state.Fees + state.Interest;
		} // GetRolloverPayValue

		private PayPointCardModel[] FillPayPointCards(Customer customer) {
			return customer.PayPointCards.Select(PayPointCardModel.FromCard).OrderByDescending(x => x.IsDefault).ToArray();
		} // FillPayPointCards

		private readonly ISecurityQuestionRepository m_oQuestions;
		private readonly ICustomerRepository m_oCustomerRepository;
		private readonly IUsersRepository m_oUsers;
		private readonly PaymentRolloverRepository m_oPaymentRolloverRepository;
		private readonly ChangeLoanDetailsModelBuilder m_oChangeLoanDetailsModelBuilder;
		private readonly ExperianDirectorRepository m_oExperianDirectors;
		private readonly WhiteLabelProviderRepository _whiteLabelProviderRepository;
		private readonly ILog Log = LogManager.GetLogger(typeof(CustomerModelBuilder));
	} // CustomerModelBuilder
} // namespace
