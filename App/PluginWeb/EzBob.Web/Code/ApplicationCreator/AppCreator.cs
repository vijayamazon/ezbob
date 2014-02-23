namespace EzBob.Web.Code.ApplicationCreator {
	using System.ServiceModel;
	using Backend.Models;
	using EZBob.DatabaseLib;
	using EzServiceReference;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ApplicationMng;
	using ApplicationMng.Model;
	using ApplicationMng.Repository;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using Ezbob.Backend.Models;
	using FraudChecker;
	using Infrastructure;
	using Scorto.Strategy;
	using log4net;
	using EZBob.DatabaseLib.Model;
	using ISession = NHibernate.ISession;

	public class AppCreator : IAppCreator {
		public AppCreator(IEzBobConfiguration config, IUsersRepository users, ISession session, ApplicationRepository applicationRepository,
			DatabaseDataHelper helper, IStrategyRepository strategies, ConfigurationVariablesRepository configurationVariablesRepository) {
			_config = config;
			_users = users;
			_session = session;
			_helper = helper;
			_applicationRepository = applicationRepository;
			_strategies = strategies;
			_sm = new StrategyManager();

			if (!readConfig)
			{
				readConfig = true;
				useNewMailStrategies = configurationVariablesRepository.GetByNameAsBool("UseNewMailStrategies");
				useNewUpdateMpStrategy = configurationVariablesRepository.GetByNameAsBool("UseNewUpdateMpStrategy");
				useNewUpdateCustomerMpsStrategy = configurationVariablesRepository.GetByNameAsBool("UseNewUpdateCustomerMpsStrategy");
				useNewCaisStrategies = configurationVariablesRepository.GetByNameAsBool("UseNewCaisStrategies");
				useNewFraudCheckerStrategy = configurationVariablesRepository.GetByNameAsBool("UseNewFraudCheckerStrategy");
				useNewMainStrategy = configurationVariablesRepository.GetByNameAsBool("UseNewMainStrategy");
			}
		}

		public void AfterSignup(User user, string address) {
			if (useNewMailStrategies) {
				ServiceClient.GreetingMailStrategy(user.Id, address);
			}
			else {
				var strategyParameters = new[] {
					new StrategyParameter("email", user.EMail),
					new StrategyParameter("ConfirmEmailAddress", address),
					new StrategyParameter("userId", user.Id)
				};
				CreateApplication(user, strategyParameters, _config.GreetingStrategyName);
			}
		}

		public bool GenerateMobileCode(string mobilePhone)
		{
			return ServiceClient.GenerateMobileCode(mobilePhone).Value;
		}
		
		public bool ValidateMobileCode(string mobilePhone, string mobileCode)
		{
			var result = ServiceClient.ValidateMobileCode(mobilePhone, mobileCode);
			return result.Value;
		}

		public WizardConfigsActionResult GetWizardConfigs()
		{
			return WizardConfigs();
		}

		public void CashTransfered(User user, string firstName, decimal cashAmount, decimal setUpFee, int loanId) {
			if (useNewMailStrategies) {
				ServiceClient.CashTransferred(user.Id, cashAmount);
			}
			else {
				var customer = _session.Get<Customer>(user.Id);
				bool isFirstLoan = customer.Loans.Count == 1;

				var strategyParameters = new[] {
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

		public void ThreeInvalidAttempts(User user, string firstName, string password) {
			if (useNewMailStrategies) {
				ServiceClient.ThreeInvalidAttempts(user.Id, password);
			}
			else {
				var customer = _session.Get<Customer>(user.Id);

				var strategyParameters = new[] {
					new StrategyParameter("userId", user.Id),
					new StrategyParameter("email", user.EMail),
					new StrategyParameter("FirstName", customer.PersonalInfo.FirstName),
					new StrategyParameter("Password", password)
				};
				CreateApplication(user, strategyParameters, _config.ThreeInvalidAttemptsStrategyName);
			}
		}

		public void PasswordChanged(User user, string firstName, string password) {
			if (useNewMailStrategies) {
				ServiceClient.PasswordChanged(user.Id, password);
			}
			else {
				var strategyParameters = new[] {
					new StrategyParameter("userId", user.Id),
					new StrategyParameter("email", user.EMail),
					new StrategyParameter("FirstName", firstName),
					new StrategyParameter("Password", password)
				};
				CreateApplication(user, strategyParameters, _config.ChangePasswordStrategyName);
			}
		}

		public void PasswordRestored(User user, string emailTo, string firstName, string password) {
			if (useNewMailStrategies) {
				ServiceClient.PasswordRestored(user.Id, password);
			}
			else {
				var strategyParameters = new[] {
					new StrategyParameter("userId", user.Id),
					new StrategyParameter("email", emailTo),
					new StrategyParameter("FirstName", firstName),
					new StrategyParameter("Password", password)
				};
				CreateApplication(user, strategyParameters, _config.RestorePasswordStrategyName);
			}
		}

		public void CustomerMarketPlaceAdded(Customer customer, int umi) {
			if (!customer.WizardStep.TheLastOne)
			{
				customer.WizardStep = _helper.WizardSteps.GetAll().FirstOrDefault(x => x.ID == (int)WizardStepType.Marketplace);
				Log.DebugFormat("Customer {1} ({0}): wizard step has been updated to :{2}", customer.Id, customer.PersonalInfo.Fullname, (int)WizardStepType.Marketplace);
			}

			if (useNewUpdateMpStrategy) {
				ServiceClient.UpdateMarketplace(customer.Id, umi);
			}
			else {
				var strategyParameters = new[] {
					new StrategyParameter("umi", umi),
					new StrategyParameter("refNum", customer.RefNumber),
					new StrategyParameter("userId", customer.Id)
				};
				CreateApplication(customer, strategyParameters, _config.CustomerMarketPlaceStrategyName);
			}
		}

		public void Evaluate(int underwriterId, User user, NewCreditLineOption newCreditLineOption, int avoidAutomaticDescison, bool isUnderwriterForced, bool isSync)
		{
			if (useNewMainStrategy) {
				if (!isUnderwriterForced) {
					if (isSync)
					{
						ServiceClient.MainStrategySync1(underwriterId, user.Id, newCreditLineOption, avoidAutomaticDescison);
					}
					else
					{
						ServiceClient.MainStrategy1(underwriterId, user.Id, newCreditLineOption, avoidAutomaticDescison);
					}
				}
				else {
					ServiceClient.MainStrategy2(underwriterId, user.Id, newCreditLineOption, avoidAutomaticDescison, true);
				}
			}
			else {
				var strategyParameters = new[] {
					new StrategyParameter("userId", user.Id),
					new StrategyParameter("Underwriter_Check", isUnderwriterForced ? 1 : 0),
					new StrategyParameter("NewCreditLineOption", (int)newCreditLineOption),
					new StrategyParameter("AvoidAutomaticDescison", avoidAutomaticDescison)
				};
				var application = CreateApplication(user, strategyParameters, _config.ScoringResultStrategyName);
				var customer = _session.Get<Customer>(user.Id);
				customer.LastStartedMainStrategy = application;
				_session.Update(customer);
			}
		}

		public void EvaluateWithIdHubCustomAddress(int underwriterId, User user, int checkType, string houseNumber, string houseName, string street,
												   string district, string town, string county, string postcode, string bankAccount, string sortCode, int avoidAutomaticDescison) {
			if (useNewMainStrategy) {
				ServiceClient.MainStrategy3(underwriterId, user.Id, checkType, houseNumber, houseName, street, district, town, county, postcode, bankAccount, sortCode, avoidAutomaticDescison);
			}
			else {
				var strategyParameters = new[] {
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
		}

		public void GetCashFailed(User user, string firstName) {
			if (useNewMailStrategies) {
				ServiceClient.GetCashFailed(user.Id);
			}
			else {
				var strategyParameters = new[] {
					new StrategyParameter("email", user.EMail),
					new StrategyParameter("userId", user.Id),
					new StrategyParameter("FirstName", firstName)
				};
				CreateApplication(user, strategyParameters, _config.GetCashFailedStrategyName);
			}
		}

		public void TransferCashFailed(User user, string firstName) {
			if (useNewMailStrategies) {
				ServiceClient.TransferCashFailed(user.Id);
			}
			else {
				var strategyParameters = new[] {
					new StrategyParameter("email", user.EMail),
					new StrategyParameter("FirstName", firstName)
				};
				CreateApplication(user, strategyParameters, _config.TransferCashFailedStrategyName);
			}
		}

		public void PayEarly(User user, DateTime date, decimal? amount, string firstName, string refNumber) {
			if (useNewMailStrategies) {
				ServiceClient.PayEarly(user.Id, amount.HasValue ? amount.Value : 0, refNumber);
			}
			else {
				var strategyParameters = new[] {
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

		public void PayPointNameValidationFailed(string cardHodlerName, User user, Customer customer) {
			if (useNewMailStrategies) {
				ServiceClient.PayPointNameValidationFailed(user.Id, customer.Id, cardHodlerName);
			}
			else {
				var strategyParameters = new[] {
					new StrategyParameter("email", customer.Name),
					new StrategyParameter("userId", customer.Id),
					new StrategyParameter("name", customer.PersonalInfo.FirstName),
					new StrategyParameter("surname", customer.PersonalInfo.Surname),
					new StrategyParameter("cardHodlerName", cardHodlerName)
				};
				CreateApplication(user, strategyParameters, _config.PayPointNameValidationFailedStrategyName);
			}
		}

		public void ApprovedUser(User user, Customer customer, decimal? loanAmount) {
			if (useNewMailStrategies) {
				ServiceClient.ApprovedUser(user.Id, customer.Id, loanAmount.HasValue ? loanAmount.Value : 0);
			}
			else {
				bool isNotFirstApproval = customer.DecisionHistory.Any(x => x.Action == DecisionActions.Approve);

				var strategyParameters = new[] {
					new StrategyParameter("email", customer.Name),
					new StrategyParameter("userId", customer.Id),
					new StrategyParameter("FirstName", customer.PersonalInfo.FirstName),
					new StrategyParameter("LoanAmount", loanAmount),
					new StrategyParameter("ValidFor", (customer.OfferValidUntil - customer.OfferStart).Value.TotalHours),
					new StrategyParameter("LoanType", customer.LastCashRequest.LoanType.Type),
					new StrategyParameter("IsFirstApproval", !isNotFirstApproval),
					new StrategyParameter("InterestNumberOfMonths", Math.Floor(customer.LastCashRequest.RepaymentPeriod / 2.0))
				};

				CreateApplication(user, strategyParameters, _config.ApprovedUserStrategyName);
			}
		}

		public void RejectUser(User user, string email, int userId, string firstName) {
			if (useNewMailStrategies) {
				ServiceClient.RejectUser(user.Id, userId);
			}
			else {
				var strategyParameters = new[] {
					new StrategyParameter("email", email),
					new StrategyParameter("userId", userId),
					new StrategyParameter("FirstName", firstName)
				};
				CreateApplication(user, strategyParameters, _config.RejectUserStrategyName);
			}
		}

		public void MoreAMLInformation(User user, string email, int userId, string firstName) {
			if (useNewMailStrategies) {
				ServiceClient.MoreAmlInformation(user.Id, userId);
			}
			else {
				var strategyParameters = new[] {
					new StrategyParameter("email", email),
					new StrategyParameter("userId", userId),
					new StrategyParameter("FirstName", firstName)
				};
				CreateApplication(user, strategyParameters, _config.MoreAMLInformationStrategyName);
			}
		}

		public void MoreAMLandBWAInformation(User user, string email, int userId, string firstName) {
			if (useNewMailStrategies) {
				ServiceClient.MoreAmlAndBwaInformation(user.Id, userId);
			}
			else {
				var strategyParameters = new[] {
					new StrategyParameter("email", email),
					new StrategyParameter("userId", userId),
					new StrategyParameter("FirstName", firstName)
				};
				CreateApplication(user, strategyParameters, _config.MoreAMLandBWAStrategyName);
			}
		}

		public void MoreBWAInformation(User user, string email, int userId, string firstName) {
			if (useNewMailStrategies) {
				ServiceClient.MoreBwaInformation(user.Id, userId);
			}
			else {
				var strategyParameters = new[] {
					new StrategyParameter("email", email),
					new StrategyParameter("userId", userId),
					new StrategyParameter("FirstName", firstName)
				};
				CreateApplication(user, strategyParameters, _config.MoreBWAInformationStrategyName);
			}
		}

		public void SendEmailVerification(User user, Customer customer, string address) {
			if (useNewMailStrategies) {
				ServiceClient.SendEmailVerification(user.Id, customer.Name, address);
			}
			else {
				var strategyParameters = new[] {
					new StrategyParameter("email", user.EMail),
					new StrategyParameter("ConfirmEmailAddress", address),
					new StrategyParameter("userId", user.Id),
					new StrategyParameter("FirstName", customer.PersonalInfo.FirstName),
					new StrategyParameter("LastName", customer.PersonalInfo.Surname)
				};
				CreateApplication(user, strategyParameters, _config.ConfirmationEmailStrategyName);
			}
		}

		public void PayPointAddedByUnderwriter(User user, Customer customer, string cardno) {
			if (useNewMailStrategies) {
				ServiceClient.PayPointAddedByUnderwriter(customer.Id, cardno, user.FullName, user.Id);
			}
			else {
				var strategyParameters = new[] {
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
				ServiceClient.UpdateAllMarketplaces(customer.Id);
			}
			else
			{
				var strategyParameters = new[] {
					new StrategyParameter("customerId", customer.Id),
					new StrategyParameter("userId", customer.Id)
				};
				CreateApplication(customer, strategyParameters, _config.UpdateMarketplacesStrategyName);
			}
		}

		public void PerformCompanyCheck(int customerId)
		{
			ServiceClient.CheckExperianCompany(customerId);
		}

		public void PerformConsumerCheck(int customerId, int directorId)
		{
			ServiceClient.CheckExperianConsumer(customerId, directorId);
		}

		public void PerformAmlCheck(int customerId)
		{
			ServiceClient.CheckAml(customerId);
		}

		public void EmailRolloverAdded(Customer customer, decimal amount, DateTime expireDate) {
			if (useNewMailStrategies) {
				ServiceClient.EmailRolloverAdded(customer.Id, amount);
			}
			else {
				var strategyParameters = new[] {
					new StrategyParameter("email", customer.Name),
					new StrategyParameter("Amount", amount),
					new StrategyParameter("Firstname", customer.PersonalInfo.FirstName),
					new StrategyParameter("ExpiryDate", expireDate)
				};
				CreateApplication(customer, strategyParameters, _config.EmailRolloverAddedStrategyName);
			}
		}

		public void RenewEbayToken(Customer customer, string marketplaceName, string url) {
			if (useNewMailStrategies) {
				ServiceClient.RenewEbayToken(customer.Id, marketplaceName, url);
			}
			else {
				var strategyParameters = new[] {
					new StrategyParameter("email", customer.Name),
					new StrategyParameter("Firstname", customer.PersonalInfo.FirstName),
					new StrategyParameter("eBayName", marketplaceName),
					new StrategyParameter("url", url)
				};
				CreateApplication(customer, strategyParameters, _config.ReneweBayTokenStrategyName);
			}
		}

		public void Escalated(Customer customer) {
			if (useNewMailStrategies) {
				ServiceClient.Escalated(customer.Id);
			}
			else {
				var strategyParameters = new[] {
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

		public void CAISGenerate(User user) {
			if (useNewCaisStrategies) {
				ServiceClient.CaisGenerate(user.Id);
			}
			else {
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

				CreateApplication(user, new StrategyParameter[] { }, _config.CAISNoUploadStrategyName);
			}
		}

		public void CAISUpdate(User user, int caisId) {
			if (useNewCaisStrategies) {
				ServiceClient.CaisUpdate(user.Id, caisId);
			}
			else {
				var strategyParameters = new[] {
					new StrategyParameter("caisId", caisId)
				};
				CreateApplication(user, strategyParameters, _config.CAISNoUploadStrategyName);
			}
		}

		public void FraudChecker(User user, FraudMode mode) {
			if (useNewFraudCheckerStrategy) {
				ServiceClient.FraudChecker(user.Id, mode);
			}
			else {
				var strategyParameters = new[] {
					new StrategyParameter("CustomerId", user.Id),
				};
				CreateApplication(user, strategyParameters, _config.FraudCheckerStrategyName);
			}
		}

		public void EmailUnderReview(User user, string firstName, string email) {
			if (useNewMailStrategies) {
				ServiceClient.EmailUnderReview(user.Id);
			}
			else {
				var strategyParameters = new[] {
					new StrategyParameter("FirstName", firstName),
					new StrategyParameter("email", email)
				};
				CreateApplication(user, strategyParameters, _config.EmailUnderReviewStrategyName);
			}
		}

		public void RequestCashWithoutTakenLoan(Customer customer, string dashboard) {
			if (useNewMailStrategies) {
				ServiceClient.RequestCashWithoutTakenLoan(customer.Id);
			}
			else {
				var strategyParameters = new[] {
					new StrategyParameter("userId", customer.Id),
					new StrategyParameter("FirstName", customer.PersonalInfo.FirstName),
					new StrategyParameter("DashboardPage", dashboard),
					new StrategyParameter("email", customer.Name)
				};
				CreateApplication(customer, strategyParameters, "Email Didnt take offer and reapplies");
			}
		}

		public void LoanFullyPaid(Loan loan) {
			var customer = loan.Customer;

			if (useNewMailStrategies) {
				ServiceClient.LoanFullyPaid(customer.Id, loan.RefNumber);
			}
			else {
				var strategyParameters = new[] {
					new StrategyParameter("userId", customer.Id),
					new StrategyParameter("FirstName", customer.PersonalInfo.FirstName),
					new StrategyParameter("email", customer.Name),
					new StrategyParameter("RefNum", loan.RefNumber)
				};
				CreateApplication(customer, strategyParameters, "Email Loan Paid Fully");
			}
		}

		public QuickOfferModel QuickOffer(Customer oCustomer, bool saveOfferToDB) {
			try {
				QuickOfferActionResult oResult = ServiceClient.QuickOffer(oCustomer.Id, saveOfferToDB);

				return oResult.HasValue ? oResult.Value : null;
			}
			catch (Exception e) {
				Log.Error("Failed to receive a quick offer from service.", e);
				return null;
			} // try
		} // QuickOffer

		public QuickOfferModel QuickOfferWithPrerequisites(Customer oCustomer, bool saveOfferToDB) {
			try {
				QuickOfferActionResult oResult = ServiceClient.QuickOfferWithPrerequisites(oCustomer.Id, saveOfferToDB);

				return oResult.HasValue ? oResult.Value : null;
			}
			catch (Exception e) {
				Log.Error("Failed to receive a quick offer from service.", e);
				return null;
			} // try
		} // QuickOfferWithPrerequisites

		public void BrokerSignup(
			string FirmName,
			string FirmRegNum,
			string ContactName,
			string ContactEmail,
			string ContactMobile,
			string MobileCode,
			string ContactOtherPhone,
			decimal EstimatedMonthlyClientAmount,
			string Password,
			string Password2
		) {
			ServiceClient.BrokerSignup(
				FirmName,
				FirmRegNum,
				ContactName,
				ContactEmail,
				ContactMobile,
				MobileCode,
				ContactOtherPhone,
				EstimatedMonthlyClientAmount,
				Password,
				Password2
			);
		} // BrokerSignup

		public void BrokerLogin(string Email, string Password) {
			ServiceClient.BrokerLogin(Email, Password);
		} // BrokerLogin

		public void BrokerRestorePassword(string sMobile, string sCode) {
			ServiceClient.BrokerRestorePassword(sMobile, sCode);
		} // BrokerRestorePassword

		public BrokerCustomerEntry[] BrokerLoadCustomerList(string sContactEmail) {
			return ServiceClient.BrokerLoadCustomerList(sContactEmail).Records;
		}

		#region property ServiceClient
		
		private EzServiceClient ServiceClient {
			get {
				if (ReferenceEquals(m_oServiceClient, null) || (m_oServiceClient.State != CommunicationState.Opened)) {
					try {
						var cfg = new EzSrvCfgLoader(_session, Log);
						cfg.Init();

						var oTcpBinding = new NetTcpBinding();

						m_oServiceClient = new EzServiceClient(
							oTcpBinding, // TODO: HTTPS...
							new EndpointAddress(cfg.AdminEndpointAddress) // TODO: when HTTPS is ready make it ClientAdminEndpoint
						);

						m_oServiceClient.InnerChannel.OperationTimeout = TimeSpan.FromSeconds(cfg.ClientTimeoutSeconds);
					}
					catch (Exception e) {
						Log.Debug("Failed to connect to EzService", e);

						// TODO: save to DB failed request to run it later...

						throw; // TODO: remove this after the previous TODO is implemented
					} // try
				} // if

				return m_oServiceClient;
			} // get
		} // ServiceClient

		private EzServiceClient m_oServiceClient;

		#endregion property ServiceClient

		private Application CreateApplication(Customer customer, IEnumerable<StrategyParameter> strategyParameters, string strategyName) {
			var user = _users.Get(customer.Id);
			return CreateApplication(user, strategyParameters, strategyName);
		}

		private Application CreateApplication(User user, IEnumerable<StrategyParameter> strategyParameters, string strategyName) {
			try {
				var strategy = _strategies.GetStrategyByDisplayName(strategyName);
				var createApplicationParameters = new CreateApplicationParameters(strategy.Id, user.Id, strategyParameters);
				var appId = _sm.CreateApplication(createApplicationParameters);
				return _session.Get<Application>(appId);
			}
			catch (Exception ex) {
				Log.Error(ex);
			}
			return null;
		}

		private WizardConfigsActionResult WizardConfigs()
		{
			return ServiceClient.GetWizardConfigs();
		}

		private readonly IStrategyRepository _strategies;
		private readonly IEzBobConfiguration _config;
		private readonly IUsersRepository _users;
		private static readonly ILog Log = LogManager.GetLogger(typeof(AppCreator));
		private readonly StrategyManager _sm;
		private readonly ISession _session;
		private readonly ApplicationRepository _applicationRepository;
		private static bool useNewMailStrategies;
		private static bool useNewUpdateMpStrategy;
		private static bool useNewUpdateCustomerMpsStrategy;
		private static bool useNewFraudCheckerStrategy;
		private static bool useNewMainStrategy;
		private static bool useNewCaisStrategies;
		private static bool readConfig = false;
		private readonly DatabaseDataHelper _helper;
	} // class AppCreator
} // namespace
