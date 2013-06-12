using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationMng;
using ApplicationMng.Model;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database;
using EzBob.Web.ApplicationCreator;
using EzBob.Web.Infrastructure;
using NHibernate;
using Scorto.Strategy;
using log4net;

namespace EzBob.Web.Code.ApplicationCreator
{
    public class AppCreator : IAppCreator
    {
        private readonly IStrategyRepository _strategies;
        private readonly IEzBobConfiguration _config;
        private readonly IUsersRepository _users;
        private static readonly ILog Log = LogManager.GetLogger(typeof(AppCreator));
        private readonly StrategyManager _sm;
        private readonly ISession _session;
        private readonly ApplicationRepository _applicationRepository;

        public AppCreator(IEzBobConfiguration config, IUsersRepository users, ISession session, ApplicationRepository applicationRepository, IStrategyRepository strategies)
        {
            _config = config;
            _users = users;
            _session = session;
            _applicationRepository = applicationRepository;
            _strategies = strategies;
            _sm = new StrategyManager();
        }

        public void AfterSignup(User user, string address)
        {
            var strategyParameters = new[]
                                             {
                                                 new StrategyParameter("email", user.EMail),
                                                 new StrategyParameter("ConfirmEmailAddress", address),
                                                 new StrategyParameter("userId", user.Id)
                                             };
            CreateApplication(user, strategyParameters, _config.GreetingStrategyName);
        }

        public void CashTransfered(User user, string firstName, decimal? cashAmount, decimal setUpFee)
        {
            var strategyParameters = new[]
                                             {
                                                 new StrategyParameter("email", user.EMail),
                                                 new StrategyParameter("userId", user.Id),
                                                 new StrategyParameter("FirstName", firstName),
                                                 new StrategyParameter("CashAmount", cashAmount ),
                                                 new StrategyParameter("SetUpFee", setUpFee) 
                                             };
            CreateApplication(user, strategyParameters, _config.CashTransferedStrategyName);
        }

        public void ThreeInvalidAttempts(User user, string firstName, string password)
        {
            var strategyParameters = new[]
                                             {
                                                 new StrategyParameter("userId", user.Id),
                                                 new StrategyParameter("email", user.EMail),
                                                 new StrategyParameter("FirstName", firstName),
                                                 new StrategyParameter("Password", password)
                                             };
            CreateApplication(user, strategyParameters, _config.RestorePasswordStrategyName);
        }

        public void PasswordChanged(User user, string firstName, string password)
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
            var strategyParameters = new[]
                                             {
                                                 new StrategyParameter("userId", user.Id),
                                                 new StrategyParameter("email", emailTo),
                                                 new StrategyParameter("FirstName", firstName),
                                                 new StrategyParameter("Password", password)
                                             };
            CreateApplication(user, strategyParameters, _config.RestorePasswordStrategyName);
        }

        public void CustomerMarketPlaceAdded(Customer customer, int umi)
        {
            var strategyParameters = new[]
                                             {
                                                 new StrategyParameter("umi", umi),
                                                 new StrategyParameter("refNum", customer.RefNumber),
                                                 new StrategyParameter("userId", customer.Id)
                                             };
            var user = _users.Get(customer.Id);
            CreateApplication(user, strategyParameters, _config.CustomerMarketPlaceStrategyName);
        }

        public void Evaluate(User user, bool isUnderwriterForced = false)
        {
            var strategyParameters = new[]
                                             {
                                                 new StrategyParameter("userId", user.Id),
                                                 new StrategyParameter("Underwriter_Check", isUnderwriterForced ? 1 : 0)
                                             };
            var application = CreateApplication(user, strategyParameters, _config.ScoringResultStrategyName);
            var customer = _session.Get<Customer>(user.Id);
            customer.LastStartedMainStrategy = application;
            _session.Update(customer);
        }

        public void EvaluateWithIdHubCustomAddress(User user, int checkType, string houseNumber, string houseName, string street,
                                            string district, string town, string county, string postcode, string bankAccount, string sortCode)
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
                                             new StrategyParameter("idhubBranchCode", sortCode)
                                         };
            CreateApplication(user, strategyParameters, _config.ScoringResultStrategyName);
        }

        public void GetCashFailed(User user, string firstName)
        {
            var strategyParameters = new[]
                                             {
                                                 new StrategyParameter("email", user.EMail),
                                                 new StrategyParameter("userId", user.Id),
                                                 new StrategyParameter("FirstName", firstName)
                                             };
            CreateApplication(user, strategyParameters, _config.GetCashFailedStrategyName);
        }

        

        public void PayEarly(User user, DateTime date, decimal? amount, string firstName)
        {
            var strategyParameters = new[]
                                             {
                                                 new StrategyParameter("email", user.EMail),
                                                 new StrategyParameter("userId", user.Id),
                                                 new StrategyParameter("Date", date),
                                                 new StrategyParameter("Amount", amount),
                                                 new StrategyParameter("FirstName", firstName)
                                             };
            CreateApplication(user, strategyParameters, _config.PayEarlyStrategyName);
        }

        public void PayPointNameValidationFailed(string cardHodlerName, User user, Customer customer)
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

        public void ApprovedUser(User user, Customer customer, decimal? loanAmount)
        {
            var strategyParameters = new[]
                                             {
                                                 new StrategyParameter("email", customer.Name ),
                                                 new StrategyParameter("userId", customer.Id),
                                                 new StrategyParameter("FirstName", customer.PersonalInfo.FirstName ), 
                                                 new StrategyParameter("LoanAmount", loanAmount),
                                                 new StrategyParameter("ValidFor", (customer.OfferValidUntil - customer.OfferStart).Value.TotalHours),
                                                 new StrategyParameter("LoanType", customer.LastCashRequest.LoanType.Type),
                                                 new StrategyParameter("InterestNumberOfMonths", Math.Floor(customer.LastCashRequest.RepaymentPeriod / 2.0))
                                             };
            CreateApplication(user, strategyParameters, _config.ApprovedUserStrategyName);
        }

        public void RejectUser(User user, string email, int userId, string firstName)
        {
            var strategyParameters = new[]
                                             {
                                                 new StrategyParameter("email", email),
                                                 new StrategyParameter("userId", userId),
                                                 new StrategyParameter("FirstName", firstName  )
                                             };
            CreateApplication(user, strategyParameters, _config.RejectUserStrategyName);
        }

        public void MoreAMLInformation(User user, string email, int userId, string firstName)
        {
            var strategyParameters = new[]
                                             {
                                                 new StrategyParameter("email",  email),
                                                 new StrategyParameter("userId", userId),
                                                 new StrategyParameter("FirstName", firstName  )
                                             };
            CreateApplication(user, strategyParameters, _config.MoreAMLInformationStrategyName);
        }

        public void MoreAMLandBWAInformation(User user, string email, int userId, string firstName)
        {
            var strategyParameters = new[]
                                             {
                                                 new StrategyParameter("email", email ),
                                                 new StrategyParameter("userId", userId),
                                                 new StrategyParameter("FirstName", firstName  )
                                             };
            CreateApplication(user, strategyParameters, _config.MoreAMLandBWAStrategyName);
        }

        public void MoreBWAInformation(User user, string email, int userId, string firstName)
        {
            var strategyParameters = new[]
                                             {
                                                 new StrategyParameter("email", email),
                                                 new StrategyParameter("userId", userId),
                                                 new StrategyParameter("FirstName", firstName  )
                                             };
            CreateApplication(user, strategyParameters, _config.MoreBWAInformationStrategyName);
        }

        public void SendEmailVerification(User user, Customer customer, string address)
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

        public void PayPointAddedByUnderwriter(User user, Customer customer, string cardno)
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

        public void UpdateAllMarketplaces(Customer customer)
        {
            var strategyParameters = new[]
                                         {
                                             new StrategyParameter("customerId", customer.Id),
                                             new StrategyParameter("userId", customer.Id)
                                         };
            var user = _users.Get(customer.Id);
            CreateApplication(user, strategyParameters, _config.UpdateMarketplacesStrategyName);
        }

        public void FeeAdded(Customer customer, decimal? feeAmount)
        {
            var strategyParameters = new[]
                                         {
                                             new StrategyParameter("Email", customer.Name),
                                             new StrategyParameter("FirstName", customer.PersonalInfo.FirstName),
                                             new StrategyParameter("FeeAmount", feeAmount)
                                         };
            var user = _users.Get(customer.Id);
            CreateApplication(user, strategyParameters, _config.FeeAddedStrategyName);
        }

        public void EmailRolloverAdded(Customer customer, decimal amount, DateTime expireDate)
        {
            var strategyParameters = new[]
                                         {
                                             new StrategyParameter("email", customer.Name),
                                             new StrategyParameter("Amount", amount),
                                             new StrategyParameter("Firstname", customer.PersonalInfo.FirstName),
                                             new StrategyParameter("ExpiryDate", expireDate)
                                         };
            var user = _users.Get(customer.Id);
            CreateApplication(user, strategyParameters, _config.EmailRolloverAddedStrategyName);
        }

        public void RenewEbayToken(Customer customer, string marketplaceName, string url)
        {
            var strategyParameters = new[]
                                         {
                                             new StrategyParameter("email", customer.Name),
                                             new StrategyParameter("Firstname", customer.PersonalInfo.FirstName),
                                             new StrategyParameter("eBayName", marketplaceName),
                                             new StrategyParameter("url", url)
                                         };
            var user = _users.Get(customer.Id);
            CreateApplication(user, strategyParameters, _config.ReneweBayTokenStrategyName);
        }

        public void Escalated(Customer customer)
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
            var user = _users.Get(customer.Id);
            CreateApplication(user, strategyParameters, _config.CustomerEscalatedStrategyName);
        }

        public void CAISGenerate(User user)
        {
            var caisStrategies = _strategies.GetAll().Where(x => x.DisplayName == _config.CAISNoUploadStrategyName);
            var caisStrat = caisStrategies.FirstOrDefault(x => x.Id == caisStrategies.Max(y => y.Id));
            var caisStratStatus = _applicationRepository.GetAll().Where(x => x.Strategy == caisStrat).Select(x=>x.State);
            if (caisStratStatus.Any(x => 
                   x != ApplicationStrategyState.SecurityViolation &&
                   x != ApplicationStrategyState.StrategyFinishedWithoutErrors &&
                   x != ApplicationStrategyState.StrategyFinishedWithErrors &&
                   x != ApplicationStrategyState.Error 
                ))
            {
                throw new Exception("Strategy already started");
            }
            CreateApplication(user, new StrategyParameter[]{}, _config.CAISNoUploadStrategyName);
        }

        public void CAISUpdate(User user, int caisId)
        {
            var strategyParameters = new[]
                {
                    new StrategyParameter("caisId", caisId)
                };
            CreateApplication(user, strategyParameters, _config.CAISNoUploadStrategyName);
        }
    }
}