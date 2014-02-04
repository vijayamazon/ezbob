﻿using System;
using System.Linq;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Loans;
using EZBob.DatabaseLib.Model.Database.Repository;
using EZBob.DatabaseLib.Model.Loans;
using EZBob.DatabaseLib.Repository;
using EzBob.Models;
using EzBob.Web.Areas.Underwriter.Models;
using PaymentServices.Calculators;

namespace EzBob.Web.Areas.Customer.Models
{
	using System.Collections.Generic;
	using Backend.Models;
	using EZBob.DatabaseLib;

	public class CustomerModelBuilder
	{
		private readonly ISecurityQuestionRepository _questions;
		private readonly ICustomerRepository _customerRepository;
		private readonly IUsersRepository _users;
		private readonly LoanPaymentFacade _facade;
		private readonly PaymentRolloverRepository _paymentRolloverRepository;
		private readonly ChangeLoanDetailsModelBuilder _changeLoanDetailsModelBuilder;
		private readonly ICustomerInviteFriendRepository _customerInviteFriendRepository;
		private readonly PerksRepository _perksRepository;
		public CustomerModelBuilder(
			ISecurityQuestionRepository questions, 
			ICustomerRepository customerRepository, 
			IUsersRepository users, 
			PaymentRolloverRepository paymentRolloverRepository, 
			ICustomerInviteFriendRepository customerInviteFriendRepository, 
			PerksRepository perksRepository)
		{
			_questions = questions;
			_customerRepository = customerRepository;
			_users = users;
			_paymentRolloverRepository = paymentRolloverRepository;
			_customerInviteFriendRepository = customerInviteFriendRepository;
			_perksRepository = perksRepository;
			_facade = new LoanPaymentFacade();
			_changeLoanDetailsModelBuilder = new ChangeLoanDetailsModelBuilder();
		}

		public CustomerModel BuildWizardModel(EZBob.DatabaseLib.Model.Database.Customer cus)
        {
            var customerModel = new CustomerModel() {loggedIn = cus != null, bankAccountAdded = false};

            if (!customerModel.loggedIn) return customerModel;

            var customer = _customerRepository.GetAndInitialize(cus.Id);
            var user = _users.Get(cus.Id);

            if (customer == null) return customerModel;

            customerModel.Id = customer.Id;
            customerModel.userName = user.Name;
            customerModel.Email = customer.Name;
            customerModel.EmailState = customer.EmailState.ToString();

            customerModel.CustomerPersonalInfo = customer.PersonalInfo;
            customerModel.BusinessTypeReduced = customerModel.CustomerPersonalInfo == null ?
                TypeOfBusinessReduced.Personal.ToString() : customer.PersonalInfo.TypeOfBusiness.Reduce().ToString();
			
            customerModel.BirthDateYMD = customerModel.CustomerPersonalInfo == null
				? "" : customerModel.CustomerPersonalInfo.BirthDateYMD();

            customerModel.bankAccountAdded = customer.HasBankAccount;
            if (customer.HasBankAccount)
            {
                customerModel.BankAccountNumber = customer.BankAccount.AccountNumber;
                customerModel.SortCode = customer.BankAccount.SortCode;
            }

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
                                select repayment).FirstOrDefault();
            if (nextPayment != null)
            {
                customerModel.NextPayment = nextPayment.AmountDue;
                customerModel.NextPaymentDate = nextPayment.Date;
				customerModel.IsEarly = nextPayment.Date > DateTime.UtcNow && (nextPayment.Date.Year != DateTime.UtcNow.Year || nextPayment.Date.Month != DateTime.UtcNow.Month || nextPayment.Date.Day != DateTime.UtcNow.Day);
            }

            customerModel.TotalPayEarlySavings = _facade.CalculateSavings(customer, DateTime.UtcNow);

            var account = new AccountSettingsModel();

            account.SecurityQuestions = _questions.GetQuestions();

            account.SecurityQuestionModel = new SecurityQuestionModel
            {
                Question = user.SecurityQuestion == null ? 0 : user.SecurityQuestion.Id,
                Answer = user.SecurityAnswer
            };

            customerModel.AccountSettings = account;

            var payments = from loan in customer.Loans
                           from tran in loan.Transactions
                           where tran is PaypointTransaction
                           orderby tran.PostDate descending
                           select tran;
            var lastPayment = payments.OfType<PaypointTransaction>().FirstOrDefault();

            if(lastPayment != null)
            {
                customerModel.LastPaymentTotal = lastPayment.Amount;
                customerModel.LastPaymentPrincipal = lastPayment.Principal;
                customerModel.LastPaymentInterest = lastPayment.Interest;
                customerModel.LastPaymentFees = lastPayment.Fees;
            }

            customerModel.GreetingMailSentDate = customer.GreetingMailSentDate;

			var company = customer.Companies.FirstOrDefault();
			if (company != null)
			{
				customerModel.CompanyInfo = CompanyInfoMap.FromCompany(company);
				customerModel.CompanyAddress = company.CompanyAddress.ToArray();
			}


            if (customer.AddressInfo != null)
            {
                customerModel.PersonalAddress = customer.AddressInfo.PersonalAddress.ToArray();
	            customerModel.PrevPersonAddresses = customer.AddressInfo.PrevPersonAddresses.ToArray();
                customerModel.OtherPropertyAddress = customer.AddressInfo.OtherPropertyAddress.ToArray();
            }

			customerModel.CompanyEmployeeCountInfo = new CompanyEmployeeCountInfo(customer.Companies.FirstOrDefault());

            customerModel.ApplyCount = customer.ApplyCount;
            customerModel.CreditCardNo = customer.CreditCardNo;
            customerModel.PayPointCards = FillPayPointCards(customer);

            customerModel.ActiveRollovers = _paymentRolloverRepository
                .GetRolloversForCustomer(customer.Id)
                .Where(x => x.Status == RolloverStatus.New)
                .Select(x => new RolloverModel
                        {
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
            customerModel.HasRollovers = customerModel.ActiveRollovers.Any();

            var cr = cus.LastCashRequest;
            customerModel.IsLoanDetailsFixed = !_changeLoanDetailsModelBuilder.IsAmountChangingAllowed(cr);

			customerModel.IsCurrentCashRequestFromQuickOffer = !ReferenceEquals(cr, null) && !ReferenceEquals(cr.QuickOffer, null);

	        customerModel.LoyaltyPoints = customer.LoyaltyPoints();
	        customerModel.IsOffline = customer.IsOffline;

			var inviteFriend = customer.CustomerInviteFriend.FirstOrDefault();
			if (inviteFriend == null)
			{
				customer.CustomerInviteFriend = new List<CustomerInviteFriend>();
				var customerInviteFriend = new CustomerInviteFriend(customer);
				customer.CustomerInviteFriend.Add(customerInviteFriend);
			}
	        customerModel.InviteFriendSource = customer.CustomerInviteFriend.First().InviteFriendSource;
			customerModel.InvitedFriends =
				_customerInviteFriendRepository.GetAll()
											   .Where(c => c.InvitedByFriendSource == customer.CustomerInviteFriend.First().InviteFriendSource)
											   .Select(i => new InvitedFriend
													   {
														   FriendName = string.IsNullOrEmpty(i.Customer.PersonalInfo.Fullname) ? i.Customer.Name : i.Customer.PersonalInfo.Fullname,
														   FriendTookALoan = (i.Customer.Loans.Any() ? "Yes" : "No")
													   });

			customerModel.LastSavedWizardStep = ((customer.WizardStep == null) || customer.WizardStep.TheLastOne) ? "" : customer.WizardStep.Name;

			var isDefault = customer.CollectionStatus != null && customer.CollectionStatus.CurrentStatus != null &&
			                 customer.CollectionStatus.CurrentStatus.Name == "Default";
			customerModel.Perks = isDefault ? null : _perksRepository.GetActivePerk();

			customerModel.TrustPilotStatusID = customer.TrustPilotStatus.ID;
			customerModel.TrustPilotReviewEnabled = DBConfigurationValues.Instance.TrustPilotReviewEnabled;

			customerModel.QuickOffer = BuildQuickOfferModel(customer);

			var oRequestedAmount = customer.CustomerRequestedLoan.OrderBy(x => x.Created).LastOrDefault();
			customerModel.RequestedAmount = ReferenceEquals(oRequestedAmount, null) || !oRequestedAmount.Amount.HasValue ? 0 : (decimal)oRequestedAmount.Amount;

            return customerModel;
        }

		private QuickOfferModel BuildQuickOfferModel(EZBob.DatabaseLib.Model.Database.Customer c) {
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

		private decimal GetRolloverPayValue(Loan loan)
		{
			var payEarlyCalc = new LoanRepaymentScheduleCalculator(loan, DateTime.UtcNow);
			var state = payEarlyCalc.GetState();

			return state.Fees + state.Interest;
		}

		private PayPointCardModel[] FillPayPointCards(EZBob.DatabaseLib.Model.Database.Customer customer)
		{
			//Add paypoint cards for old customers
			if (!string.IsNullOrEmpty(customer.PayPointTransactionId) && !customer.PayPointCards.Any())
			{
				customer.TryAddPayPointCard(customer.PayPointTransactionId, customer.CreditCardNo, null, customer.PersonalInfo.Fullname);
			}

			return customer.PayPointCards.Select(PayPointCardModel.FromCard).OrderByDescending(x => x.IsDefault).ToArray();
		}
	}
}