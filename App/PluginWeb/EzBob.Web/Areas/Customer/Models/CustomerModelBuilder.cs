namespace EzBob.Web.Areas.Customer.Models {
	using System;
	using System.Linq;
	using ConfigManager;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Database.Mapping;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Model.Loans;
	using EZBob.DatabaseLib.Repository;
	using EzBob.Models;
	using Underwriter.Models;
	using PaymentServices.Calculators;
	using System.Collections.Generic;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database.UserManagement;
	using Ezbob.Backend.Models;
	using Infrastructure;
	using System.Web;

	public class CustomerModelBuilder {
		public CustomerModelBuilder(
			ISecurityQuestionRepository questions,
			ICustomerRepository customerRepository,
			IUsersRepository users,
			PaymentRolloverRepository paymentRolloverRepository,
			ICustomerInviteFriendRepository customerInviteFriendRepository,
			PerksRepository perksRepository,
			DatabaseDataHelper oDbHelper
		) {
			m_oQuestions = questions;
			m_oCustomerRepository = customerRepository;
			m_oUsers = users;
			m_oPaymentRolloverRepository = paymentRolloverRepository;
			m_oCustomerInviteFriendRepository = customerInviteFriendRepository;
			m_oPerksRepository = perksRepository;
			m_oChangeLoanDetailsModelBuilder = new ChangeLoanDetailsModelBuilder();
			m_oExperianDirectors = oDbHelper.ExperianDirectorRepository;
		} // constructor

		public CustomerModel BuildWizardModel(Customer cus, HttpSessionStateBase session, bool isProfile = false) {
			var customerModel = new CustomerModel {
				loggedIn = cus != null,
				bankAccountAdded = false,
			};

			if (!customerModel.loggedIn) {
				customerModel.IsBrokerFill = (session[Constant.Broker.FillsForCustomer] ?? Constant.No).ToString() == Constant.Yes;
				return customerModel;
			} // if

			if (cus == null)
				return customerModel;

			var customer = m_oCustomerRepository.GetAndInitialize(cus.Id);

			if (customer == null)
				return customerModel;

			var user = m_oUsers.Get(cus.Id);

			customerModel.Id = customer.Id;
			customerModel.userName = user.Name;
			customerModel.Email = customer.Name;
			customerModel.EmailState = customer.EmailState.ToString();

			customerModel.CustomerPersonalInfo = customer.PersonalInfo;
			customerModel.BusinessTypeReduced = customerModel.CustomerPersonalInfo == null ?
				TypeOfBusinessReduced.Personal.ToString() : customer.PersonalInfo.TypeOfBusiness.Reduce().ToString();

			customerModel.BirthDateYMD = customerModel.CustomerPersonalInfo == null
				? ""
				: customerModel.CustomerPersonalInfo.BirthDateYMD();

			customerModel.bankAccountAdded = customer.HasBankAccount;

			if (customer.HasBankAccount) {
				customerModel.BankAccountNumber = customer.BankAccount.AccountNumber;
				customerModel.SortCode = customer.BankAccount.SortCode;
			} // if

			customerModel.mpAccounts = customer.GetMarketPlaces();

			customerModel.LastApprovedLoanTypeID = customer.LastCashRequest != null ? customer.LastCashRequest.LoanType.Id : 0;
			customerModel.LastApprovedRepaymentPeriod = customer.LastCashRequest != null ? customer.LastCashRequest.RepaymentPeriod : 0;
			customerModel.IsLastApprovedLoanSourceEu = customer.LastCashRequest != null && customer.LastCashRequest.LoanSource.Name == "EU";
			customerModel.Medal = customer.Medal.HasValue ? customer.Medal.ToString() : "";

			customerModel.CreditSum = customer.CreditSum;
			customerModel.IsLoanTypeSelectionAllowed = customer.IsLoanTypeSelectionAllowed;
			customerModel.CreditResult = customer.CreditResult.ToString();
			customerModel.OfferStart = customer.OfferStart;
			customerModel.OfferValidUntil = customer.OfferValidUntil;
			customerModel.Status = customer.Status.ToString();

			customerModel.Loans = customer.Loans
				.OrderBy(l => l.Status)
				.ThenByDescending(l => l.Date)
				.Select(l => LoanModel.FromLoan(l, new LoanRepaymentScheduleCalculator(l, null), new LoanRepaymentScheduleCalculator(l, DateTime.UtcNow)))
				.ToList();

			customerModel.TotalBalance = customerModel.Loans.Sum(l => l.Balance);
			customerModel.PrincipalBalance = customer.ActiveLoans.Sum(l => l.LoanAmount);
			customerModel.TotalEarlyPayment = customerModel.Loans.Sum(l => l.TotalEarlyPayment);
			customerModel.TotalLatePayment = customerModel.Loans.Where(l => l.Status == LoanStatus.Late.ToString()).Sum(l => l.Late);

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
				customerModel.IsEarly = nextPayment.Date > DateTime.UtcNow && (nextPayment.Date.Year != DateTime.UtcNow.Year || nextPayment.Date.Month != DateTime.UtcNow.Month || nextPayment.Date.Day != DateTime.UtcNow.Day);
			} // if

			customerModel.TotalPayEarlySavings = new LoanPaymentFacade().CalculateSavings(customer, DateTime.UtcNow);

			var account = new AccountSettingsModel {
				SecurityQuestions = m_oQuestions.GetQuestions(),

				SecurityQuestionModel = new SecurityQuestionModel {
					Question = user.SecurityQuestion == null ? 0 : user.SecurityQuestion.Id,
					Answer = user.SecurityAnswer
				},
			};

			customerModel.AccountSettings = account;

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

			customerModel.GreetingMailSentDate = customer.GreetingMailSentDate;

			var company = customer.Company;

			customerModel.CanHaveDirectors = false;

			if (company != null) {
				customerModel.CanHaveDirectors = company.TypeOfBusiness != TypeOfBusiness.SoleTrader;

				customerModel.CompanyInfo = CompanyInfoMap.FromCompany(company);
				customerModel.CompanyAddress = company.CompanyAddress.ToArray();

				customerModel.CompanyInfo.Directors.AddRange(
					m_oExperianDirectors.Find(customer.Id).Select(ed => DirectorModel.FromExperianDirector(ed, company.TypeOfBusiness.Reduce()))
				);
			} // if

			if (customer.AddressInfo != null) {
				customerModel.PersonalAddress = customer.AddressInfo.PersonalAddress.ToArray();
				customerModel.PrevPersonAddresses = customer.AddressInfo.PrevPersonAddresses.ToArray();
				customerModel.OtherPropertyAddress = customer.AddressInfo.OtherPropertyAddress.ToArray();
			} // if

			customerModel.CompanyEmployeeCountInfo = new CompanyEmployeeCountInfo(customer.Company);

			customerModel.ApplyCount = customer.ApplyCount;
			customerModel.CreditCardNo = customer.CreditCardNo;
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


			customerModel.IsDisabled = !customer.CollectionStatus.CurrentStatus.IsEnabled;
			customerModel.CustomerStatusName = customer.CollectionStatus.CurrentStatus.Name;
			customerModel.IsDefaultCustomerStatus = customer.CollectionStatus.CurrentStatus.IsDefault;
			customerModel.HasRollovers = customerModel.ActiveRollovers.Any();

			var cr = cus.LastCashRequest;
			customerModel.IsLoanDetailsFixed = !m_oChangeLoanDetailsModelBuilder.IsAmountChangingAllowed(cr);

			customerModel.IsCurrentCashRequestFromQuickOffer = !ReferenceEquals(cr, null) && !ReferenceEquals(cr.QuickOffer, null);

			//customerModel.LoyaltyPoints = customer.LoyaltyPoints();
			customerModel.IsOffline = customer.IsOffline;

			var inviteFriend = customer.CustomerInviteFriend.FirstOrDefault();
			if (inviteFriend == null) {
				customer.CustomerInviteFriend = new List<CustomerInviteFriend>();
				var customerInviteFriend = new CustomerInviteFriend(customer);
				customer.CustomerInviteFriend.Add(customerInviteFriend);
			} // if

			customerModel.InviteFriendSource = customer.CustomerInviteFriend.First().InviteFriendSource;

			customerModel.InvitedFriends = m_oCustomerInviteFriendRepository
				.GetAll()
				.Where(c => c.InvitedByFriendSource == customer.CustomerInviteFriend.First().InviteFriendSource)
				.Select(i => new InvitedFriend {
					FriendName = string.IsNullOrEmpty(i.Customer.PersonalInfo.Fullname) ? i.Customer.Name : i.Customer.PersonalInfo.Fullname,
					FriendTookALoan = (i.Customer.Loans.Any() ? "Yes" : "No")
				});

			customerModel.LastSavedWizardStep = ((customer.WizardStep == null) || customer.WizardStep.TheLastOne) ? "" : customer.WizardStep.Name;

			var isDefault =
				customer.CollectionStatus != null &&
				customer.CollectionStatus.CurrentStatus != null &&
				customer.CollectionStatus.CurrentStatus.IsDefault;

			customerModel.Perks = isDefault ? null : m_oPerksRepository.GetActivePerk();

			customerModel.TrustPilotStatusID = customer.TrustPilotStatus.ID;
			customerModel.TrustPilotReviewEnabled = CurrentValues.Instance.TrustPilotReviewEnabled;

			customerModel.QuickOffer = BuildQuickOfferModel(customer);

			var oRequestedAmount = customer.CustomerRequestedLoan.OrderBy(x => x.Created).LastOrDefault();
			customerModel.RequestedAmount = ReferenceEquals(oRequestedAmount, null) || !oRequestedAmount.Amount.HasValue ? 0 : (decimal)oRequestedAmount.Amount;

			customerModel.IsBrokerFill = customer.FilledByBroker;
			customerModel.DefaultCardSelectionAllowed = customer.DefaultCardSelectionAllowed;

			return customerModel;
		} // BuildWizardModel

		private QuickOfferModel BuildQuickOfferModel(Customer c) {
			if (ReferenceEquals(c, null) || ReferenceEquals(c.QuickOffer, null) || (c.QuickOffer.ExpirationDate < DateTime.UtcNow))
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
			var payEarlyCalc = new LoanRepaymentScheduleCalculator(loan, DateTime.UtcNow);
			var state = payEarlyCalc.GetState();

			return state.Fees + state.Interest;
		} // GetRolloverPayValue

		private PayPointCardModel[] FillPayPointCards(Customer customer) {
			//Add paypoint cards for old customers
			if (!string.IsNullOrEmpty(customer.PayPointTransactionId) && !customer.PayPointCards.Any())
				customer.TryAddPayPointCard(customer.PayPointTransactionId, customer.CreditCardNo, null, customer.PersonalInfo.Fullname);

			return customer.PayPointCards.Select(PayPointCardModel.FromCard).OrderByDescending(x => x.IsDefault).ToArray();
		} // FillPayPointCards

		private readonly ISecurityQuestionRepository m_oQuestions;
		private readonly ICustomerRepository m_oCustomerRepository;
		private readonly IUsersRepository m_oUsers;
		private readonly PaymentRolloverRepository m_oPaymentRolloverRepository;
		private readonly ChangeLoanDetailsModelBuilder m_oChangeLoanDetailsModelBuilder;
		private readonly ICustomerInviteFriendRepository m_oCustomerInviteFriendRepository;
		private readonly PerksRepository m_oPerksRepository;
		private readonly ExperianDirectorRepository m_oExperianDirectors;
	} // CustomerModelBuilder
} // namespace