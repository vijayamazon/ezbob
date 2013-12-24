using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationMng;
using ApplicationMng.Model;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Loans;
using EzBob.Web.ApplicationCreator;
using EzBob.Web.Infrastructure;
using NHibernate;
using Scorto.Strategy;
using log4net;

namespace EzBob.Web.Code.ApplicationCreator
{
	using EZBob.DatabaseLib.Model;
	using EzServiceReference;

	public class AppCreator : IAppCreator
    {
        private readonly IStrategyRepository _strategies;
        private readonly IEzBobConfiguration _config;
        private readonly IUsersRepository _users;
        private static readonly ILog Log = LogManager.GetLogger(typeof(AppCreator));
        private readonly StrategyManager _sm;
        private readonly ISession _session;
		private readonly ApplicationRepository _applicationRepository;
		private readonly bool useNewMailStrategies;
		private readonly bool useNewUpdateMpStrategy;
		private readonly bool useNewUpdateCustomerMpsStrategy;
		private readonly bool useNewFraudCheckerStrategy;
		private readonly bool useNewCaisStrategies;
		private readonly EzServiceClient serviceClient = new EzServiceClient();

	    public AppCreator(IEzBobConfiguration config, IUsersRepository users, ISession session, ApplicationRepository applicationRepository,
			IStrategyRepository strategies, ConfigurationVariablesRepository configurationVariablesRepository)
        {
            _config = config;
            _users = users;
            _session = session;
            _applicationRepository = applicationRepository;
            _strategies = strategies;
            _sm = new StrategyManager();

			useNewMailStrategies = configurationVariablesRepository.GetByNameAsBool("UseNewMailStrategies");
			useNewUpdateMpStrategy = configurationVariablesRepository.GetByNameAsBool("UseNewUpdateMpStrategy");
			useNewUpdateCustomerMpsStrategy = configurationVariablesRepository.GetByNameAsBool("UseNewUpdateCustomerMpsStrategy");
			useNewCaisStrategies = configurationVariablesRepository.GetByNameAsBool("UseNewCaisStrategies");
			useNewFraudCheckerStrategy = configurationVariablesRepository.GetByNameAsBool("UseNewFraudCheckerStrategy");
        }

        public void AfterSignup(User user, string address)
        {
	        if (useNewMailStrategies)
			{
				serviceClient.GreetingMailStrategy(user.Id, address);
	        }
	        else
	        {
		        var strategyParameters = new[]
			        {
				        new StrategyParameter("email", user.EMail),
				        new StrategyParameter("ConfirmEmailAddress", address),
				        new StrategyParameter("userId", user.Id)
			        };
		        CreateApplication(user, strategyParameters, _config.GreetingStrategyName);
	        }
        }

        public void CashTransfered(User user, string firstName, decimal cashAmount, decimal setUpFee, int loanId)
        {
	        if (useNewMailStrategies)
	        {
				serviceClient.CashTransferred(user.Id, cashAmount);
	        }
	        else
			{
				var customer = _session.Get<Customer>(user.Id);
				bool isFirstLoan = customer.Loans.Count == 1;

		        var strategyParameters = new[]
			        {
				        new StrategyParameter("email", user.EMail),
				        new StrategyParameter("userId", user.Id),
				        new StrategyParameter("FirstName", firstName),
				        new StrategyParameter("CashAmount", cashAmount),
				        new StrategyParameter("IsFirstLoan", isFirstLoan),
				        new StrategyParameter("IsOffline", customer.IsOffline),
				        new StrategyParameter("SetUpFee", setUpFee),
				        new StrategyParameter("loanId", loanId)
			        };
		        CreateApplication(user, strategyParameters, _config.CashTransferedStrategyName);
	        }
        }

        public void ThreeInvalidAttempts(User user, string firstName, string password)
        {
	        if (useNewMailStrategies)
	        {
				serviceClient.ThreeInvalidAttempts(user.Id, password);
	        }
	        else
	        {
		        var customer = _session.Get<Customer>(user.Id);

		        var strategyParameters = new[]
			        {
				        new StrategyParameter("userId", user.Id),
				        new StrategyParameter("email", user.EMail),
				        new StrategyParameter("FirstName", customer.PersonalInfo.FirstName),
				        new StrategyParameter("Password", password)
			        };
		        CreateApplication(user, strategyParameters, _config.ThreeInvalidAttemptsStrategyName);
	        }
        }

        public void PasswordChanged(User user, string firstName, string password)
        {
	        if (useNewMailStrategies)
	        {
		        serviceClient.PasswordChanged(user.Id, password);
	        }
	        else
	        {
		        var strategyParameters = new[]
			        {
				        new StrategyParameter("userId", user.Id),
				        new StrategyParameter("email", user.EMail),
				        new StrategyParameter("FirstName", firstName),
				        new StrategyParameter("Password", password)
			        };
		        CreateApplication(user, strategyParameters, _config.ChangePasswordStrategyName);
	        }
        }

        private Application CreateApplication(Customer customer, IEnumerable<StrategyParameter> strategyParameters, string strategyName)
        {
            var user = _users.Get(customer.Id);
            return CreateApplication(user, strategyParameters, strategyName);
        }

        private Application CreateApplication(User user, IEnumerable<StrategyParameter> strategyParameters, string strategyName)
        {
            try
            {
                var strategy = _strategies.GetStrategyByDisplayName(strategyName);
                var createApplicationParameters = new CreateApplicationParameters(strategy.Id, user.Id, strategyParameters);
                var appId = _sm.CreateApplication(createApplicationParameters);
                return _session.Get<Application>(appId);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
            return null;
        }

        public void PasswordRestored(User user, string emailTo, string firstName, string password)
        {
	        if (useNewMailStrategies)
	        {
		        serviceClient.PasswordRestored(user.Id, password);
	        }
	        else
	        {
		        var strategyParameters = new[]
			        {
				        new StrategyParameter("userId", user.Id),
				        new StrategyParameter("email", emailTo),
				        new StrategyParameter("FirstName", firstName),
				        new StrategyParameter("Password", password)
			        };
		        CreateApplication(user, strategyParameters, _config.RestorePasswordStrategyName);
	        }
        }

        public void CustomerMarketPlaceAdded(Customer customer, int umi)
        {
	        if (useNewUpdateMpStrategy)
			{
				serviceClient.UpdateMarketplace(customer.Id, umi);
	        }
	        else
	        {
		        var strategyParameters = new[]
			        {
				        new StrategyParameter("umi", umi),
				        new StrategyParameter("refNum", customer.RefNumber),
				        new StrategyParameter("userId", customer.Id)
			        };
		        CreateApplication(customer, strategyParameters, _config.CustomerMarketPlaceStrategyName);
	        }
        }

        public void Evaluate(User user, NewCreditLineOption newCreditLineOption, int avoidAutomaticDescison, bool isUnderwriterForced = false)
        {
		    var strategyParameters = new[]
			    {
				    new StrategyParameter("userId", user.Id),
				    new StrategyParameter("Underwriter_Check", isUnderwriterForced ? 1 : 0),
				    new StrategyParameter("NewCreditLineOption", (int) newCreditLineOption),
				    new StrategyParameter("AvoidAutomaticDescison", avoidAutomaticDescison)
			    };
		    var application = CreateApplication(user, strategyParameters, _config.ScoringResultStrategyName);
		    var customer = _session.Get<Customer>(user.Id);
		    customer.LastStartedMainStrategy = application;
		    _session.Update(customer);
        }

        public void EvaluateWithIdHubCustomAddress(User user, int checkType, string houseNumber, string houseName, string street,
                                            string district, string town, string county, string postcode, string bankAccount, string sortCode, int avoidAutomaticDescison)
        {
            var strategyParameters = new[]
                                         {
                                             new StrategyParameter("userId", user.Id),
                                             new StrategyParameter("Underwriter_Check", 1),
                                             new StrategyParameter("UseCustomIdHubAddress", checkType),
                                             new StrategyParameter("idhubHouseNumber", houseNumber),
                                             new StrategyParameter("idhubHouseName", houseName),
                                             new StrategyParameter("idhubStreet", street),
                                             new StrategyParameter("idhubDistrict", district),
                                             new StrategyParameter("idhubTown", town),
                                             new StrategyParameter("idhubCounty", county),
                                             new StrategyParameter("idhubPostCode", postcode),
                                             new StrategyParameter("idhubAccountNumber", bankAccount),
                                             new StrategyParameter("idhubBranchCode", sortCode),
                                             new StrategyParameter("AvoidAutomaticDescison", avoidAutomaticDescison)
                                         };
            CreateApplication(user, strategyParameters, _config.ScoringResultStrategyName);
        }

        public void GetCashFailed(User user, string firstName)
        {
	        if (useNewMailStrategies)
	        {
		        serviceClient.GetCashFailed(user.Id);
	        }
	        else
	        {
		        var strategyParameters = new[]
			        {
				        new StrategyParameter("email", user.EMail),
				        new StrategyParameter("userId", user.Id),
				        new StrategyParameter("FirstName", firstName)
			        };
		        CreateApplication(user, strategyParameters, _config.GetCashFailedStrategyName);
	        }
        }

        public void TransferCashFailed(User user, string firstName)
        {
	        if (useNewMailStrategies)
	        {
		        serviceClient.TransferCashFailed(user.Id);
	        }
	        else
	        {
		        var strategyParameters = new[]
			        {
				        new StrategyParameter("email", user.EMail),
				        new StrategyParameter("FirstName", firstName)
			        };
		        CreateApplication(user, strategyParameters, _config.TransferCashFailedStrategyName);
	        }
        }

        public void PayEarly(User user, DateTime date, decimal? amount, string firstName, string refNumber)
        {
	        if (useNewMailStrategies)
	        {
				serviceClient.PayEarly(user.Id, amount.HasValue ? amount.Value : 0, refNumber);
	        }
	        else
	        {
		        var strategyParameters = new[]
			        {
				        new StrategyParameter("email", user.EMail),
				        new StrategyParameter("userId", user.Id),
				        new StrategyParameter("Date", date),
				        new StrategyParameter("Amount", amount),
				        new StrategyParameter("FirstName", firstName),
				        new StrategyParameter("RefNum", refNumber)
			        };
		        CreateApplication(user, strategyParameters, _config.PayEarlyStrategyName);
	        }
        }

        public void PayPointNameValidationFailed(string cardHodlerName, User user, Customer customer)
        {
	        if (useNewMailStrategies)
	        {
				serviceClient.PayPointNameValidationFailed(user.Id, cardHodlerName);
	        }
	        else
	        {
		        var strategyParameters = new[]
			        {
				        new StrategyParameter("email", customer.Name),
				        new StrategyParameter("userId", customer.Id),
				        new StrategyParameter("name", customer.PersonalInfo.FirstName),
				        new StrategyParameter("surname", customer.PersonalInfo.Surname),
				        new StrategyParameter("cardHodlerName", cardHodlerName)
			        };
		        CreateApplication(user, strategyParameters, _config.PayPointNameValidationFailedStrategyName);
	        }
        }

        public void ApprovedUser(User user, Customer customer, decimal? loanAmount)
        {
	        if (useNewMailStrategies)
	        {
				serviceClient.ApprovedUser(user.Id, loanAmount.HasValue ? loanAmount.Value : 0);
	        }
	        else
	        {
		        bool isNotFirstApproval = customer.DecisionHistory.Any(x => x.Action == DecisionActions.Approve);

		        var strategyParameters = new[]
			        {
				        new StrategyParameter("email", customer.Name),
				        new StrategyParameter("userId", customer.Id),
				        new StrategyParameter("FirstName", customer.PersonalInfo.FirstName),
				        new StrategyParameter("LoanAmount", loanAmount),
				        new StrategyParameter("ValidFor", (customer.OfferValidUntil - customer.OfferStart).Value.TotalHours),
				        new StrategyParameter("LoanType", customer.LastCashRequest.LoanType.Type),
				        new StrategyParameter("IsFirstApproval", !isNotFirstApproval),
				        new StrategyParameter("InterestNumberOfMonths", Math.Floor(customer.LastCashRequest.RepaymentPeriod/2.0))
			        };

		        CreateApplication(user, strategyParameters, _config.ApprovedUserStrategyName);
	        }
        }

        public void RejectUser(User user, string email, int userId, string firstName)
        {
	        if (useNewMailStrategies)
	        {
				serviceClient.RejectUser(user.Id);
	        }
	        else
	        {
		        var strategyParameters = new[]
			        {
				        new StrategyParameter("email", email),
				        new StrategyParameter("userId", userId),
				        new StrategyParameter("FirstName", firstName)
			        };
		        CreateApplication(user, strategyParameters, _config.RejectUserStrategyName);
	        }
        }

        public void MoreAMLInformation(User user, string email, int userId, string firstName)
        {
	        if (useNewMailStrategies)
	        {
		        serviceClient.MoreAmlInformation(user.Id);
	        }
	        else
	        {
		        var strategyParameters = new[]
			        {
				        new StrategyParameter("email", email),
				        new StrategyParameter("userId", userId),
				        new StrategyParameter("FirstName", firstName)
			        };
		        CreateApplication(user, strategyParameters, _config.MoreAMLInformationStrategyName);
	        }
        }

        public void MoreAMLandBWAInformation(User user, string email, int userId, string firstName)
        {
	        if (useNewMailStrategies)
	        {
		        serviceClient.MoreAmLandBwaInformation(user.Id);
	        }
	        else
	        {
		        var strategyParameters = new[]
			        {
				        new StrategyParameter("email", email),
				        new StrategyParameter("userId", userId),
				        new StrategyParameter("FirstName", firstName)
			        };
		        CreateApplication(user, strategyParameters, _config.MoreAMLandBWAStrategyName);
	        }
        }

        public void MoreBWAInformation(User user, string email, int userId, string firstName)
        {
	        if (useNewMailStrategies)
	        {
		        serviceClient.MoreBwaInformation(user.Id);
	        }
	        else
	        {
		        var strategyParameters = new[]
			        {
				        new StrategyParameter("email", email),
				        new StrategyParameter("userId", userId),
				        new StrategyParameter("FirstName", firstName)
			        };
		        CreateApplication(user, strategyParameters, _config.MoreBWAInformationStrategyName);
	        }
        }

        public void SendEmailVerification(User user, Customer customer, string address)
        {
	        if (useNewMailStrategies)
	        {
				serviceClient.SendEmailVerification(user.Id, address);
	        }
	        else
	        {
		        var strategyParameters = new[]
			        {
				        new StrategyParameter("email", user.EMail),
				        new StrategyParameter("ConfirmEmailAddress", address),
				        new StrategyParameter("userId", user.Id),
				        new StrategyParameter("FirstName", customer.PersonalInfo.FirstName),
				        new StrategyParameter("LastName", customer.PersonalInfo.Surname)
			        };
		        CreateApplication(user, strategyParameters, _config.ConfirmationEmailStrategyName);
	        }
        }

		public void PayPointAddedByUnderwriter(User user, Customer customer, string cardno)
        {
	        if (useNewMailStrategies)
	        {
				serviceClient.PayPointAddedByUnderwriter(customer.Id, cardno, user.FullName, user.Id);
	        }
	        else
	        {
		        var strategyParameters = new[]
			        {
				        new StrategyParameter("email", customer.Name),
				        new StrategyParameter("userId", user.Id),
				        new StrategyParameter("customerId", customer.Id),
				        new StrategyParameter("cardno", cardno),
				        new StrategyParameter("FirstName", customer.PersonalInfo.FirstName),
				        new StrategyParameter("LastName", customer.PersonalInfo.Surname),
				        new StrategyParameter("UnderwriterName", user.FullName)

			        };
		        CreateApplication(user, strategyParameters, "PayPointAddedByUnderwriter");
	        }
        }

        public void UpdateAllMarketplaces(Customer customer)
        {
	        if (useNewUpdateCustomerMpsStrategy)
	        {
				serviceClient.UpdateAllMarketplaces(customer.Id);
	        }
	        else
	        {
		        var strategyParameters = new[]
			        {
				        new StrategyParameter("customerId", customer.Id),
				        new StrategyParameter("userId", customer.Id)
			        };
		        CreateApplication(customer, strategyParameters, _config.UpdateMarketplacesStrategyName);
	        }
        }

        public void EmailRolloverAdded(Customer customer, decimal amount, DateTime expireDate)
        {
	        if (useNewMailStrategies)
	        {
				serviceClient.EmailRolloverAdded(customer.Id, amount);
	        }
	        else
	        {
		        var strategyParameters = new[]
			        {
				        new StrategyParameter("email", customer.Name),
				        new StrategyParameter("Amount", amount),
				        new StrategyParameter("Firstname", customer.PersonalInfo.FirstName),
				        new StrategyParameter("ExpiryDate", expireDate)
			        };
		        CreateApplication(customer, strategyParameters, _config.EmailRolloverAddedStrategyName);
	        }
        }

        public void RenewEbayToken(Customer customer, string marketplaceName, string url)
        {
	        if (useNewMailStrategies)
	        {
		        serviceClient.RenewEbayToken(customer.Id, marketplaceName, url);
	        }
	        else
	        {
		        var strategyParameters = new[]
			        {
				        new StrategyParameter("email", customer.Name),
				        new StrategyParameter("Firstname", customer.PersonalInfo.FirstName),
				        new StrategyParameter("eBayName", marketplaceName),
				        new StrategyParameter("url", url)
			        };
		        CreateApplication(customer, strategyParameters, _config.ReneweBayTokenStrategyName);
	        }
        }

        public void Escalated(Customer customer)
        {
	        if (useNewMailStrategies)
	        {
		        serviceClient.Escalated(customer.Id);
	        }
	        else
	        {
		        var strategyParameters = new[]
			        {
				        new StrategyParameter("userId", customer.Id),
				        new StrategyParameter("email", customer.Name),
				        new StrategyParameter("FirstName", customer.PersonalInfo.FirstName),
				        new StrategyParameter("LastName", customer.PersonalInfo.Surname),
				        new StrategyParameter("MedalType", customer.Medal.HasValue ? customer.Medal.ToString() : ""),
				        new StrategyParameter("SystemDecision", customer.SystemDecision),
				        new StrategyParameter("UWName", customer.UnderwriterName),
				        new StrategyParameter("RegistrationDate", customer.GreetingMailSentDate),
				        new StrategyParameter("EscalatedReason", customer.EscalationReason)
			        };
		        CreateApplication(customer, strategyParameters, _config.CustomerEscalatedStrategyName);
	        }
        }

        public void CAISGenerate(User user)
        {
	        if (useNewCaisStrategies)
	        {
		        serviceClient.CaisGenerate(user.Id);
	        }
	        else
	        {
		        var caisStrategies = _strategies.GetAll().Where(x => x.DisplayName == _config.CAISNoUploadStrategyName);
		        var caisStrat = caisStrategies.FirstOrDefault(x => x.Id == caisStrategies.Max(y => y.Id));
		        var caisStratStatus = _applicationRepository.GetAll().Where(x => x.Strategy == caisStrat).Select(x => x.State);
		        if (caisStratStatus.Any(x =>
		                                x != ApplicationStrategyState.SecurityViolation &&
		                                x != ApplicationStrategyState.StrategyFinishedWithoutErrors &&
		                                x != ApplicationStrategyState.StrategyFinishedWithErrors &&
		                                x != ApplicationStrategyState.Error
			        ))
		        {
			        throw new Exception("Strategy already started");
		        }
		        CreateApplication(user, new StrategyParameter[] {}, _config.CAISNoUploadStrategyName);
	        }
        }

        public void CAISUpdate(User user, int caisId)
        {
	        if (useNewCaisStrategies)
	        {
				serviceClient.CaisUpdate(caisId);
	        }
	        else
	        {
		        var strategyParameters = new[]
			        {
				        new StrategyParameter("caisId", caisId)
			        };
		        CreateApplication(user, strategyParameters, _config.CAISNoUploadStrategyName);
	        }
        }

        public void FraudChecker(User user)
        {
	        if (useNewFraudCheckerStrategy)
	        {
		        serviceClient.FraudChecker(user.Id);
	        }
	        else
	        {
		        var strategyParameters = new[]
			        {
				        new StrategyParameter("CustomerId", user.Id),
			        };
		        CreateApplication(user, strategyParameters, _config.FraudCheckerStrategyName);
	        }
        }

		public void EmailUnderReview(User user, string firstName, string email)
		{
			if (useNewMailStrategies)
			{
				serviceClient.EmailUnderReview(user.Id);
			}
			else
			{
				var strategyParameters = new[]
					{
						new StrategyParameter("FirstName", firstName),
						new StrategyParameter("email", email)
					};
				CreateApplication(user, strategyParameters, _config.EmailUnderReviewStrategyName);
			}
		}

        public void RequestCashWithoutTakenLoan(Customer customer, string dashboard)
        {
	        if (useNewMailStrategies)
	        {
				serviceClient.RequestCashWithoutTakenLoan(customer.Id);
	        }
	        else
	        {
		        var strategyParameters = new[]
			        {
				        new StrategyParameter("userId", customer.Id),
				        new StrategyParameter("FirstName", customer.PersonalInfo.FirstName),
				        new StrategyParameter("DashboardPage", dashboard),
				        new StrategyParameter("email", customer.Name)
			        };
		        CreateApplication(customer, strategyParameters, "Email Didnt take offer and reapplies");
	        }
        }

        public void LoanFullyPaid(Loan loan)
		{
			var customer = loan.Customer;
	        if (useNewMailStrategies)
	        {
				serviceClient.LoanFullyPaid(customer.Id, loan.RefNumber);
	        }
	        else
	        {
		        var strategyParameters = new[]
			        {
				        new StrategyParameter("userId", customer.Id),
				        new StrategyParameter("FirstName", customer.PersonalInfo.FirstName),
				        new StrategyParameter("email", customer.Name),
				        new StrategyParameter("RefNum", loan.RefNumber)
			        };
		        CreateApplication(loan.Customer, strategyParameters, "Email Loan Paid Fully");
	        }
        }
    }
}