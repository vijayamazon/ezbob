﻿namespace EzBob.Web.Areas.Customer.Models {
	using System;
	using System.Linq;
	using ConfigManager;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Broker;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Database.Mapping;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Model.Loans;
	using EZBob.DatabaseLib.Repository;
	using EzBob.Models;
	using Infrastructure.Email;
	using Underwriter.Models;
	using PaymentServices.Calculators;
	using System.Collections.Generic;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database.UserManagement;
	using Ezbob.Backend.Models;
	using Infrastructure;
	using System.Web;
	using Web.Models;

	public class CustomerModelBuilder {
		public CustomerModelBuilder(
			ISecurityQuestionRepository questions,
			ICustomerRepository customerRepository,
			IUsersRepository users,
			PaymentRolloverRepository paymentRolloverRepository,
			ICustomerInviteFriendRepository customerInviteFriendRepository,
			PerksRepository perksRepository,
			DatabaseDataHelper oDbHelper, 
			WhiteLabelProviderRepository whiteLabelProviderRepository
		) {
			m_oQuestions = questions;
			m_oCustomerRepository = customerRepository;
			m_oUsers = users;
			m_oPaymentRolloverRepository = paymentRolloverRepository;
			m_oCustomerInviteFriendRepository = customerInviteFriendRepository;
			m_oPerksRepository = perksRepository;
			m_oChangeLoanDetailsModelBuilder = new ChangeLoanDetailsModelBuilder();
			m_oExperianDirectors = oDbHelper.ExperianDirectorRepository;
			_whiteLabelProviderRepository = whiteLabelProviderRepository;
		} // constructor

		public WizardModel BuildWizardModel(Customer cus, HttpSessionStateBase session, string profile, bool isProfile = false) {
			var wizardModel = new WizardModel();

			var customerModel = new CustomerModel {
				loggedIn = cus != null,
				bankAccountAdded = false,
			};

			if (!string.IsNullOrEmpty(profile)) {
				wizardModel.WhiteLabel = _whiteLabelProviderRepository.GetByName(profile);
				customerModel.IsWhiteLabel = wizardModel.WhiteLabel != null;
				customerModel.WhiteLabelId = wizardModel.WhiteLabel != null ? wizardModel.WhiteLabel.Id : 0;
			}

			wizardModel.Customer = customerModel;

			if (!customerModel.loggedIn) {
				customerModel.IsBrokerFill = (session[Constant.Broker.FillsForCustomer] ?? Constant.No).ToString() == Constant.Yes;
				return wizardModel;
			} // if

			if (cus == null)
				return wizardModel;

			var customer = m_oCustomerRepository.GetAndInitialize(cus.Id);

			if (customer == null) {
				return wizardModel;
			}

			var user = m_oUsers.Get(cus.Id);

			if (customer.WhiteLabel != null) {
				wizardModel.WhiteLabel = customer.WhiteLabel;
				customerModel.IsWhiteLabel = wizardModel.WhiteLabel != null;
				customerModel.WhiteLabelId = customer.WhiteLabel.Id;
			}

			customerModel.Id = customer.Id;
			customerModel.userName = user.Name;
			customerModel.Email = customer.Name;
			customerModel.EmailState = EmailConfirmationState.Get(customer);

			customerModel.CustomerPersonalInfo = customer.PersonalInfo;

			customerModel.IsAlibaba = customer.IsAlibaba;

			if (customer.PropertyStatus != null)
			{
				customerModel.PropertyStatus = new PropertyStatusModel
					{
						Id = customer.PropertyStatus.Id,
						Description = customer.PropertyStatus.Description,
						IsOwnerOfOtherProperties = customer.PropertyStatus.IsOwnerOfOtherProperties,
						IsOwnerOfMainAddress = customer.PropertyStatus.IsOwnerOfMainAddress
					};
			}
			customerModel.BusinessTypeReduced = customerModel.CustomerPersonalInfo == null ?
				TypeOfBusinessReduced.Personal.ToString() : customer.PersonalInfo.TypeOfBusiness.Reduce().ToString();
			
			customerModel.bankAccountAdded = customer.HasBankAccount;

			if (customer.HasBankAccount) {
				customerModel.BankAccountNumber = customer.BankAccount.AccountNumber;
				customerModel.SortCode = customer.BankAccount.SortCode;
			} // if

			customerModel.mpAccounts = customer.GetMarketPlaces();

			customerModel.LastApprovedLoanTypeID = customer.LastCashRequest != null ? customer.LastCashRequest.LoanType.Id : 0;
			customerModel.LastApprovedRepaymentPeriod = customer.LastCashRequest != null ? customer.LastCashRequest.RepaymentPeriod : 0;
			customerModel.IsLastApprovedLoanSourceEu = customer.LastCashRequest != null && customer.LastCashRequest.LoanSource.Name == LoanSourceName.EU.ToString();
			customerModel.IsLastApprovedLoanSourceCOSME = customer.LastCashRequest != null && customer.LastCashRequest.LoanSource.Name == LoanSourceName.COSME.ToString();
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

			try {
				customerModel.TotalPayEarlySavings = new LoanPaymentFacade().CalculateSavings(customer, DateTime.UtcNow);
			}
			// ReSharper disable EmptyGeneralCatchClause
			catch (Exception) {
				// Silently ignore for now.
			} // try
			// ReSharper restore EmptyGeneralCatchClause

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
				customerModel.OtherPropertiesAddresses = customer.AddressInfo.OtherPropertiesAddresses.ToArray();
			} // if

			customerModel.CompanyEmployeeCountInfo = new CompanyEmployeeCountInfo(customer.Company);

			customerModel.ApplyCount = customer.ApplyCount;
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

			return wizardModel;
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
		private readonly WhiteLabelProviderRepository _whiteLabelProviderRepository;
	} // CustomerModelBuilder
} // namespace
