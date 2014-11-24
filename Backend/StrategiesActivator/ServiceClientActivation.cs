namespace StrategiesActivator {
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Reflection;
	using System.ServiceModel;
	using System.Text;
	using ConfigManager;
	using EzServiceAccessor;
	using EzServiceConfigurationLoader;
	using System;
	using Ezbob.Backend.Models;
	using Ezbob.Database.Pool;
	using ServiceClientProxy.EzServiceReference;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Security;
	using Newtonsoft.Json;
	using StructureMap;
	using log4net;
	using Configuration = EzServiceConfigurationLoader.Configuration;

	public class ServiceClientActivation {
		#region public

		#region constructor

		public ServiceClientActivation(string[] args) {
			m_oLog = new ConsoleLog(new SafeILog(LogManager.GetLogger(typeof(ServiceClientActivation))));
			m_oLog.NotifyStart();

			InitMethods();

			string sInstanceName = args[0].ToLower();

			if (sInstanceName.ToLower() == "list") {
				ListSupported();
				m_oLog.NotifyStop();
				throw new ExitException();
			} // if

			var env = new Ezbob.Context.Environment(m_oLog);
			m_oDB = new SqlConnection(env, m_oLog);

			ConfigManager.CurrentValues.Init(m_oDB, m_oLog);
			DbConnectionPool.ReuseCount = CurrentValues.Instance.ConnectionPoolReuseCount;
			AConnection.UpdateConnectionPoolMaxSize(CurrentValues.Instance.ConnectionPoolMaxSize);

			Configuration cfg;

			if (m_oMethods.ContainsKey(sInstanceName)) {
				cfg = new DefaultConfiguration(System.Environment.MachineName, m_oDB, m_oLog);
				m_aryArgs = args;
			}
			else {
				m_aryArgs = new string[args.Length - 1];
				Array.Copy(args, 1, m_aryArgs, 0, args.Length - 1);

				cfg = new Configuration(sInstanceName, m_oDB, m_oLog);
			} // if

			cfg.Init();

			m_oLog.Info("Running against instance {0} - {1}.", cfg.InstanceID, cfg.InstanceName);
			m_oLog.Info("Arguments:\n\t{0}", string.Join("\n\t", m_aryArgs));

			var oTcpBinding = new NetTcpBinding();

			m_oServiceClient = new EzServiceClient(
				oTcpBinding,
				new EndpointAddress(cfg.AdminEndpointAddress)
			);

			m_oAdminClient = new EzServiceAdminClient(
				oTcpBinding,
				new EndpointAddress(cfg.AdminEndpointAddress)
			);
		} // constructor

		#endregion constructor

		#region method Execute

		public void Execute() {
			string strategyName = m_aryArgs[0];

			string sKey = strategyName.ToLower();

			if (m_oMethods.ContainsKey(sKey))
				m_oMethods[sKey].Invoke(this, new object[] { });
			else {
				m_oLog.Msg("Strategy {0} is not supported", strategyName);
				ListSupported();
			} // if

			m_oLog.NotifyStop();
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		#region method ListSupported

		private void ListSupported() {
			m_oLog.Msg("Supported strategies are (case insensitive):\n\t{0}", string.Join("\n\t", m_oMethods.Keys));
		} // ListSupported

		#endregion method ListSupported

		#region method InitMethods

		private void InitMethods() {
			MethodInfo[] aryMethods = GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);

			m_oMethods = new SortedDictionary<string, MethodInfo>();

			foreach (MethodInfo mi in aryMethods) {
				IEnumerable<ActivationAttribute> oAttrList = mi.GetCustomAttributes<ActivationAttribute>();

				if (oAttrList.Any())
					m_oMethods[mi.Name.ToLower()] = mi;
			} // foreach
		} // InitMethods

		#endregion method InitMethods

		#region strategy activators
		// ReSharper disable UnusedMember.Local

		[Activation]
		private void GetWizardConfigs() {
			m_oServiceClient.GetWizardConfigs();
		}

		[Activation]
		private void QuickOffer() {
			int customerId;
			bool bSaveOfferToDB;

			if (m_aryArgs.Length != 3 || !int.TryParse(m_aryArgs[1], out customerId) || !bool.TryParse(m_aryArgs[2], out bSaveOfferToDB)) {
				m_oLog.Msg("Usage: QuickOffer <CustomerId> <Save offer to DB>");
				return;
			}

			m_oServiceClient.QuickOffer(customerId, bSaveOfferToDB);
		} // QuickOffer

		[Activation]
		private void QuickOfferWithPrerequisites() {
			int customerId;
			bool bSaveOfferToDB;

			if (m_aryArgs.Length != 3 || !int.TryParse(m_aryArgs[1], out customerId) || !bool.TryParse(m_aryArgs[2], out bSaveOfferToDB)) {
				m_oLog.Msg("Usage: QuickOfferWithPrerequisites <CustomerId> <Save offer to DB>");
				return;
			}

			m_oServiceClient.QuickOfferWithPrerequisites(customerId, bSaveOfferToDB);
		} // QuickOffer

		[Activation]
		private void ApprovedUser() {
			int underwriterId;
			int customerId;
			decimal loanAmount;
			int validFor;
			bool isFirst;
			if (m_aryArgs.Length != 6 || !int.TryParse(m_aryArgs[1], out underwriterId) || !int.TryParse(m_aryArgs[2], out customerId) || !decimal.TryParse(m_aryArgs[3], out loanAmount) || !int.TryParse(m_aryArgs[4], out validFor) || bool.TryParse(m_aryArgs[5], out isFirst)) {
				m_oLog.Msg("Usage: ApprovedUser <Underwriter ID> <CustomerId> <loanAmount> <ValidFor> <isFirst>");
				return;
			}

			m_oServiceClient.ApprovedUser(underwriterId, customerId, loanAmount, validFor, isFirst);
		}

		[Activation]
		private void CashTransferred() {
			int customerId;
			decimal amount;
			bool isFirst;
			if (m_aryArgs.Length != 5 || !int.TryParse(m_aryArgs[1], out customerId) || !decimal.TryParse(m_aryArgs[2], out amount) || !bool.TryParse(m_aryArgs[3], out isFirst)) {
				m_oLog.Msg("Usage: CashTransferred <CustomerId> <amount> <loanRefNum>");
				return;
			}
			string loanRefNum = m_aryArgs[3];
			m_oServiceClient.CashTransferred(customerId, amount, loanRefNum, isFirst);
		}

		[Activation]
		private void EmailRolloverAdded() {
			int customerId;
			decimal amount;
			if (m_aryArgs.Length != 3 || !int.TryParse(m_aryArgs[1], out customerId) || !decimal.TryParse(m_aryArgs[2], out amount)) {
				m_oLog.Msg("Usage: EmailRolloverAdded <CustomerId> <amount>");
				return;
			}

			m_oServiceClient.EmailRolloverAdded(1, customerId, amount);
		}

		[Activation]
		private void EmailUnderReview() {
			int customerId;
			if (m_aryArgs.Length != 2 || !int.TryParse(m_aryArgs[1], out customerId)) {
				m_oLog.Msg("Usage: EmailUnderReview <CustomerId>");
				return;
			}

			m_oServiceClient.EmailUnderReview(customerId);
		}

		[Activation]
		private void Escalated() {
			int customerId;
			if (m_aryArgs.Length != 2 || !int.TryParse(m_aryArgs[1], out customerId)) {
				m_oLog.Msg("Usage: Escalated <CustomerId>");
				return;
			}

			m_oServiceClient.Escalated(customerId, 1);
		}

		[Activation]
		private void GetCashFailed() {
			int customerId;
			if (m_aryArgs.Length != 2 || !int.TryParse(m_aryArgs[1], out customerId)) {
				m_oLog.Msg("Usage: GetCashFailed <CustomerId>");
				return;
			}

			m_oServiceClient.GetCashFailed(customerId);
		}

		[Activation]
		private void LoanFullyPaid() {
			int customerId;
			if (m_aryArgs.Length != 3 || !int.TryParse(m_aryArgs[1], out customerId)) {
				m_oLog.Msg("Usage: LoanFullyPaid <CustomerId> <loanRefNum>");
				return;
			}

			m_oServiceClient.LoanFullyPaid(customerId, m_aryArgs[2]);
		}

		[Activation]
		private void MoreAmlAndBwaInformation() {
			int underwriterId;
			int customerId;
			if (m_aryArgs.Length != 3 || !int.TryParse(m_aryArgs[1], out underwriterId) || !int.TryParse(m_aryArgs[2], out customerId)) {
				m_oLog.Msg("Usage: MoreAmlAndBwaInformation <Underwriter ID> <CustomerId>");
				return;
			}

			m_oServiceClient.MoreAmlAndBwaInformation(underwriterId, customerId);
		}

		[Activation]
		private void MoreAmlInformation() {
			int underwriterId;
			int customerId;
			if (m_aryArgs.Length != 3 || !int.TryParse(m_aryArgs[1], out underwriterId) || !int.TryParse(m_aryArgs[2], out customerId)) {
				m_oLog.Msg("Usage: MoreAmlInformation <Underwriter ID> <CustomerId>");
				return;
			}
			m_oServiceClient.MoreAmlInformation(underwriterId, customerId);
		}

		[Activation]
		private void MoreBwaInformation() {
			int underwriterId;
			int customerId;
			if (m_aryArgs.Length != 3 || !int.TryParse(m_aryArgs[1], out underwriterId) || !int.TryParse(m_aryArgs[2], out customerId)) {
				m_oLog.Msg("Usage: MoreBwaInformation <Underwriter ID> <CustomerId>");
				return;
			}

			m_oServiceClient.MoreBwaInformation(underwriterId, customerId);
		}

		[Activation]
		private void PasswordRestored() {
			int customerId;
			if (m_aryArgs.Length != 2 || !int.TryParse(m_aryArgs[1], out customerId)) {
				m_oLog.Msg("Usage: PasswordRestored <CustomerId>");
				return;
			}

			m_oServiceClient.PasswordRestored(customerId);
		}

		[Activation]
		private void PayEarly() {
			int customerId;
			decimal amount;
			if (m_aryArgs.Length != 4 || !int.TryParse(m_aryArgs[1], out customerId) || !decimal.TryParse(m_aryArgs[2], out amount)) {
				m_oLog.Msg("Usage: PayEarly <CustomerId> <amount> <loanRefNumber>");
				return;
			}

			m_oServiceClient.PayEarly(customerId, amount, m_aryArgs[3]);
		}

		[Activation]
		private void PayPointAddedByUnderwriter() {
			int customerId, underwriterId;
			if (m_aryArgs.Length != 5 || !int.TryParse(m_aryArgs[1], out customerId) || !int.TryParse(m_aryArgs[4], out underwriterId)) {
				m_oLog.Msg("Usage: PayPointAddedByUnderwriter <CustomerId> <cardno> <underwriterName> <underwriterId>");
				return;
			}

			m_oServiceClient.PayPointAddedByUnderwriter(customerId, m_aryArgs[2], m_aryArgs[3], underwriterId);
		}

		[Activation]
		private void PayPointNameValidationFailed() {
			int underwriterId;
			int customerId;
			if (m_aryArgs.Length != 4 || !int.TryParse(m_aryArgs[1], out underwriterId) || !int.TryParse(m_aryArgs[2], out customerId)) {
				m_oLog.Msg("Usage: PayPointNameValidationFailed <Underwriter ID> <CustomerId> <cardHodlerName>");
				return;
			}

			m_oServiceClient.PayPointNameValidationFailed(underwriterId, customerId, m_aryArgs[2]);
		}

		[Activation]
		private void RejectUser() {
			int underwriterId;
			int customerId;
			bool bSendToCustomer;

			if (m_aryArgs.Length != 4 || !int.TryParse(m_aryArgs[1], out underwriterId) || !int.TryParse(m_aryArgs[2], out customerId) || !bool.TryParse(m_aryArgs[3], out bSendToCustomer)) {
				m_oLog.Msg("Usage: RejectUser <Underwriter ID> <CustomerId> <send to customer>");
				return;
			}

			m_oServiceClient.RejectUser(underwriterId, customerId, bSendToCustomer);
		}

		[Activation]
		private void RenewEbayToken() {
			int customerId;
			if (m_aryArgs.Length != 4 || !int.TryParse(m_aryArgs[1], out customerId)) {
				m_oLog.Msg("Usage: RenewEbayToken <CustomerId> <marketplaceName> <eBayAddress>");
				return;
			}

			m_oServiceClient.RenewEbayToken(1, customerId, m_aryArgs[2], m_aryArgs[3]);
		}

		[Activation]
		private void RequestCashWithoutTakenLoan() {
			int customerId;
			if (m_aryArgs.Length != 2 || !int.TryParse(m_aryArgs[1], out customerId)) {
				m_oLog.Msg("Usage: RequestCashWithoutTakenLoan <CustomerId>");
				return;
			}

			m_oServiceClient.RequestCashWithoutTakenLoan(customerId);
		}

		[Activation]
		private void TransferCashFailed() {
			int customerId;
			if (m_aryArgs.Length != 2 || !int.TryParse(m_aryArgs[1], out customerId)) {
				m_oLog.Msg("Usage: TransferCashFailed <CustomerId>");
				return;
			}

			m_oServiceClient.TransferCashFailed(customerId);
		}

		[Activation]
		private void CaisGenerate() {
			int underwriterId;
			if (m_aryArgs.Length != 2 || !int.TryParse(m_aryArgs[1], out underwriterId)) {
				m_oLog.Msg("Usage: CaisGenerate <underwriterId>");
				return;
			}

			m_oServiceClient.CaisGenerate(underwriterId);
		}

		[Activation]
		private void CaisUpdate() {
			int underwriterId;
			int caisId;
			if (m_aryArgs.Length != 3 || !int.TryParse(m_aryArgs[1], out underwriterId) || !int.TryParse(m_aryArgs[2], out caisId)) {
				m_oLog.Msg("Usage: CaisUpdate <Underwriter ID> <caisId>");
				return;
			}

			m_oServiceClient.CaisUpdate(underwriterId, caisId);
		}

		[Activation]
		private void FirstOfMonthStatusNotifier() {
			if (m_aryArgs.Length != 1) {
				m_oLog.Msg("Usage: FirstOfMonthStatusNotifier");
				return;
			}

			m_oServiceClient.FirstOfMonthStatusNotifier();
		}

		[Activation]
		private void FraudChecker() {
			int customerId;
			FraudMode mode;

			if (m_aryArgs.Length != 3 || !int.TryParse(m_aryArgs[1], out customerId) || !Enum.TryParse(m_aryArgs[2], true, out mode)) {
				m_oLog.Msg("Usage: FraudChecker <CustomerId> <Fraud mode>");
				m_oLog.Msg("Fraud mode values: {0}", string.Join(", ", Enum.GetValues(typeof(FraudMode))));
				return;
			}

			m_oServiceClient.FraudChecker(customerId, mode);
		}

		[Activation]
		private void LateBy14Days() {
			if (m_aryArgs.Length != 1) {
				m_oLog.Msg("Usage: LateBy14Days");
				return;
			}

			m_oServiceClient.LateBy14Days();
		}

		[Activation]
		private void PayPointCharger() {
			if (m_aryArgs.Length != 1) {
				m_oLog.Msg("Usage: PayPointCharger");
				return;
			}

			m_oServiceClient.PayPointCharger();
		}

		[Activation]
		private void SetLateLoanStatus() {
			if (m_aryArgs.Length != 1) {
				m_oLog.Msg("Usage: SetLateLoanStatus");
				return;
			}

			m_oServiceClient.SetLateLoanStatus();
		}

		[Activation]
		private void CustomerMarketPlaceAdded() {
			int customerId, marketplaceId;
			bool bUpdateWizardStep;
			if (m_aryArgs.Length != 4 || !int.TryParse(m_aryArgs[1], out customerId) || !int.TryParse(m_aryArgs[2], out marketplaceId) || !bool.TryParse(m_aryArgs[3], out bUpdateWizardStep)) {
				m_oLog.Msg("Usage: CustomerMarketPlaceAdded <CustomerId> <CustomerMarketplaceId> <Update customer wizard step>");
				m_oLog.Msg("<Update customer wizard step>: is boolean and means whether to set customer wizard step to Marketplace or not. If wizard step is 'TheLastOne' it is never changed.");
				return;
			}

			m_oServiceClient.UpdateMarketplace(customerId, marketplaceId, bUpdateWizardStep, 1);
		}

		[Activation]
		private void UpdateTransactionStatus() {
			if (m_aryArgs.Length != 1) {
				m_oLog.Msg("Usage: UpdateTransactionStatus");
				return;
			}

			m_oServiceClient.UpdateTransactionStatus();
		}

		[Activation]
		private void XDaysDue() {
			if (m_aryArgs.Length != 1) {
				m_oLog.Msg("Usage: XDaysDue");
				return;
			}

			m_oServiceClient.XDaysDue();
		}

		[Activation]
		private void MainStrategy() {
			int underwriterId;
			int customerId, avoidAutoDescison;
			NewCreditLineOption newCreditLineOption;

			if (int.TryParse(m_aryArgs[1], out underwriterId) && int.TryParse(m_aryArgs[2], out customerId) && Enum.TryParse(m_aryArgs[3], out newCreditLineOption) && int.TryParse(m_aryArgs[4], out avoidAutoDescison)) {
				m_oServiceClient.MainStrategy1(underwriterId, customerId, newCreditLineOption, avoidAutoDescison);
				return;
			}

			m_oLog.Msg("Usage: MainStrategy <Underwriter ID> <customerId> <newCreditLineOption> <avoidAutoDescison>");
		}

		[Activation]
		private void MainStrategySync() {
			int underwriterId;
			int customerId, avoidAutoDescison;
			NewCreditLineOption newCreditLineOption;
			if (m_aryArgs.Length == 5 && int.TryParse(m_aryArgs[1], out underwriterId) && int.TryParse(m_aryArgs[2], out customerId) && Enum.TryParse(m_aryArgs[3], out newCreditLineOption) && int.TryParse(m_aryArgs[4], out avoidAutoDescison)) {
				m_oServiceClient.MainStrategySync1(underwriterId, customerId, newCreditLineOption, avoidAutoDescison);
				return;
			}

			m_oLog.Msg("Usage: MainStrategySync <Underwriter ID> <customerId> <newCreditLineOption> <avoidAutoDescison>");
		}

		[Activation]
		private void UpdateCurrencyRates() {
			if (m_aryArgs.Length != 1) {
				m_oLog.Msg("Usage: UpdateCurrencyRates");
				return;
			}

			m_oServiceClient.UpdateCurrencyRates();
		}

		[Activation]
		private void ExperianCompanyCheck() {
			int customerId;
			bool forceCheck;
			if (m_aryArgs.Length != 3 || !int.TryParse(m_aryArgs[1], out customerId) || !bool.TryParse(m_aryArgs[2], out forceCheck)) {
				m_oLog.Msg("Usage: ExperianCompanyCheck <CustomerId> <ForceCheck>");
				return;
			}

			m_oServiceClient.ExperianCompanyCheck(1, customerId, forceCheck);
		}

		[Activation]
		private void ExperianConsumerCheck() {
			int customerId, directorId;
			bool forceCheck;

			if (m_aryArgs.Length != 4 || !int.TryParse(m_aryArgs[1], out customerId) || !int.TryParse(m_aryArgs[2], out directorId) || !bool.TryParse(m_aryArgs[3], out forceCheck)) {
				m_oLog.Msg("Usage: ExperianConsumerCheck <CustomerId> <DirectorId> <ForceCheck>");
				return;
			}

			m_oServiceClient.ExperianConsumerCheck(1, customerId, directorId, forceCheck);
		}

		[Activation]
		private void AmlChecker() {
			int customerId;

			if (m_aryArgs.Length == 2 && int.TryParse(m_aryArgs[1], out customerId)) {
				m_oServiceClient.CheckAml(customerId, 1);
				return;
			}

			if (m_aryArgs.Length == 9 && int.TryParse(m_aryArgs[1], out customerId)) {
				m_oServiceClient.CheckAmlCustom(1, customerId, m_aryArgs[2], m_aryArgs[3], m_aryArgs[4], m_aryArgs[5], m_aryArgs[6], m_aryArgs[7], m_aryArgs[8]);
				return;
			}

			m_oLog.Msg("Usage: AmlChecker <CustomerId>");
			m_oLog.Msg("OR");
			m_oLog.Msg("Usage: AmlChecker <CustomerId> <idhubHouseNumber> <idhubHouseName> <idhubStreet> <idhubDistrict> <idhubTown> <idhubCounty> <idhubPostCode>");

		}

		[Activation]
		private void BwaChecker() {
			int customerId;

			if (m_aryArgs.Length == 2 && int.TryParse(m_aryArgs[1], out customerId)) {
				m_oServiceClient.CheckBwa(customerId, 1);
				return;
			}

			if (m_aryArgs.Length == 11 && int.TryParse(m_aryArgs[1], out customerId)) {
				m_oServiceClient.CheckBwaCustom(1, customerId, m_aryArgs[2], m_aryArgs[3], m_aryArgs[4], m_aryArgs[5], m_aryArgs[6], m_aryArgs[7], m_aryArgs[8], m_aryArgs[9], m_aryArgs[10]);
				return;
			}

			m_oLog.Msg("Usage: BwaChecker <CustomerId>");
			m_oLog.Msg("OR");
			m_oLog.Msg("Usage: BwaChecker <CustomerId> <idhubHouseNumber> <idhubHouseName> <idhubStreet> <idhubDistrict> <idhubTown> <idhubCounty> <idhubPostCode> <idhubBranchCode> <idhubAccountNumber>");
		}

		[Activation]
		private void FinishWizard() {
			int customerId, underwriterId;
			if (m_aryArgs.Length < 3 || !int.TryParse(m_aryArgs[1], out customerId) || !int.TryParse(m_aryArgs[2], out underwriterId)) {
				m_oLog.Msg("Usage: FinishWizard <CustomerId> <UnderwriterId> [<DoSendEmail(true/false)>] [<AvoidAutoDecision(0/1)>]");
				return;
			}

			var oArgs = JsonConvert.DeserializeObject<FinishWizardArgs>(CurrentValues.Instance.FinishWizardForApproved);

			oArgs.CustomerID = customerId;

			if (m_aryArgs.Length >= 4) {
				oArgs.DoSendEmail = bool.Parse(m_aryArgs[3]);
			}

			if (m_aryArgs.Length >= 5) {
				oArgs.AvoidAutoDecision = int.Parse(m_aryArgs[4]);
			}

			m_oServiceClient.FinishWizard(oArgs, underwriterId);
		}

		[Activation]
		private void GenerateMobileCode() {
			if (m_aryArgs.Length != 2) {
				m_oLog.Msg("Usage: GenerateMobileCode <phone number>");
				return;
			}

			m_oServiceClient.GenerateMobileCode(m_aryArgs[1]);
		}

		[Activation]
		private void CreateUnderwriter() {
			if (m_aryArgs.Length != 4) {
				m_oLog.Msg("Usage: CreateUnderwriter <Name> <Password> <RoleName>");
				m_oLog.Msg(@"Available roles (underwriter is usually one of the last three):
						Admin:             Administrator - Manage users
						SuperUser:         SuperUser - Have rights to all applications
						Underwriter:       Underwriter
						manager:           Manager
						crm:               CRM
						Collector:         Collector - Allow change only Loans
						Sales:             Sales person
						BrokerSales:       Broker Sales person
						JuniorUnderwriter: Junior Underwriter
					");

				return;
			}

			m_oServiceClient.UnderwriterSignup(m_aryArgs[1], new Password(m_aryArgs[2]), m_aryArgs[3]);
		}

		[Activation]
		private void BrokerLoadCustomerList() {
			if (m_aryArgs.Length != 2) {
				m_oLog.Msg("Usage: BrokerLoadCustomerList <Contact person email>");
				return;
			} // if

			BrokerCustomersActionResult res = m_oServiceClient.BrokerLoadCustomerList(m_aryArgs[1]);

			foreach (var oEntry in res.Customers)
				m_oLog.Msg("Customer ID: {0} Name: {1} {2}", oEntry.CustomerID, oEntry.FirstName, oEntry.LastName);
		} // BrokerLoadCustomerList

		[Activation]
		private void NotifySalesOnNewCustomer() {
			int nCustomerID;

			if ((m_aryArgs.Length != 2) || !int.TryParse(m_aryArgs[1], out nCustomerID)) {
				m_oLog.Msg("Usage: NotifySalesOnNewCustomer <customer id>");
				return;
			} // if

			m_oServiceClient.NotifySalesOnNewCustomer(nCustomerID);
		} // NotifySalesOnNewCustomer

		[Activation]
		private void Shutdown() {
			ActionMetaData res = m_oAdminClient.Shutdown();
			m_oLog.Msg("Shutdown request result: status = {0}, comment = '{1}'", res.Status, res.Comment);
		} // Shutdown

		[Activation]
		private void ListActiveActions() {
			StringListActionResult res = m_oAdminClient.ListActiveActions();

			m_oLog.Msg(
				"\nRetriever (i.e. this action):\n\t{{ {0}: {4} [{1}sync] {2}: {3} }}",
				res.MetaData.ActionID,
				res.MetaData.IsSynchronous ? "" : "a",
				res.MetaData.Status,
				res.MetaData.Comment ?? "-- no comments --",
				res.MetaData.Name
			);

			string sKey = "{ " + res.MetaData.ActionID;

			m_oLog.Msg("\nList of active actions - begin:\n");

			foreach (string s in res.Records)
				if (!s.StartsWith(sKey))
					m_oLog.Msg("\t{0}\n", s);

			m_oLog.Msg("\nList of active actions - end.");
		} // ListActiveActions

		[Activation]
		private void TerminateAction() {
			if (m_aryArgs.Length != 2) {
				m_oLog.Msg("Usage: TerminateAction <action guid>");

				m_oLog.Msg(@"
A string that contains a GUID in one of the following formats ('d' represents a hexadecimal digit whose case is ignored):
32 contiguous digits:
dddddddddddddddddddddddddddddddd
-or-
Groups of 8, 4, 4, 4, and 12 digits with hyphens between the groups. The entire GUID can optionally be enclosed in matching braces or parentheses:
dddddddd-dddd-dddd-dddd-dddddddddddd
-or-
{{dddddddd-dddd-dddd-dddd-dddddddddddd}}
-or-
(dddddddd-dddd-dddd-dddd-dddddddddddd)
-or-
Groups of 8, 4, and 4 digits, and a subset of eight groups of 2 digits, with each group prefixed by '0x' or '0X', and separated by commas. The entire GUID, as well as the subset, is enclosed in matching braces:
{{0xdddddddd, 0xdddd, 0xdddd,{{0xdd,0xdd,0xdd,0xdd,0xdd,0xdd,0xdd,0xdd}}}}
All braces, commas, and '0x' prefixes are required. All embedded spaces are ignored. All leading zeros in a group are ignored.
The digits shown in a group are the maximum number of meaningful digits that can appear in that group. You can specify from 1 to the number of digits shown for a group. The specified digits are assumed to be the low-order digits of the group.
");
				return;
			} // if

			m_oAdminClient.Terminate(new Guid(m_aryArgs[1]));
		} // TerminateAction

		[Activation]
		private void GeneratePassword() {
			if (m_aryArgs.Length < 2) {
				m_oLog.Msg(@"Usage: GeneratePassword <arg 1> <arg 2> ... <arg N>

Generates password hash by concatenating all the arguments in the order of appearance.

I.e. to generate broker password call
GeneratePassword broker-contact-email@example.com password-itself

");
				return;
			} // if

			var os = new StringBuilder();
			for (int i = 1; i < m_aryArgs.Length; i++)
				os.Append(m_aryArgs[i]);

			string sOriginalPassword = os.ToString();

			string sHash = SecurityUtils.HashPassword(sOriginalPassword);

			m_oLog.Msg(
				"\n\nOriginal string:\n\t{0}\n\ngenerated hash:\n\t{1}\n\nquery:\n" +
				"\tUPDATE Broker SET Password = '{1}' WHERE ContactEmail = '{2}'\n" +
				"\n\tUPDATE Security_User SET EzPassword = '{1}' WHERE UserName = '{2}'\n",
				sOriginalPassword, sHash, m_aryArgs[1]
			);
		} // GeneratePassword

		[Activation]
		private void ChangeBrokerEmail()
		{
			if ((m_aryArgs.Length != 4))
			{
				m_oLog.Msg("Usage: ChangeBrokerEmail <OldEmail> <NewEmail> <NewPassword>");
				return;
			}

			m_oServiceClient.ChangeBrokerEmail(m_aryArgs[1], m_aryArgs[2], m_aryArgs[3]);
		}

		[Activation]
		private void BackfillCustomerAnalyticsCompany() {
			m_oServiceClient.BackfillCustomerAnalyticsCompany();
		} // BackfillCustomerAnalyticsCompany

		[Activation]
		private void BackfillAml()
		{
			if ((m_aryArgs.Length != 1))
			{
				m_oLog.Msg("Usage: BackfillAml");
				return;
			} // if

			m_oServiceClient.BackfillAml();
		}

		[Activation]
		private void Temp_BackFillMedals()
		{
			if ((m_aryArgs.Length != 1))
			{
				m_oLog.Msg("Usage: Temp_BackFillMedals");
				return;
			} // if

			m_oServiceClient.Temp_BackFillMedals();
		}

		[Activation]
		private void BackfillZooplaValue() {
			if ((m_aryArgs.Length != 1)) {
				m_oLog.Msg("Usage: BackfillZooplaValue");
				return;
			} // if

			m_oServiceClient.BackfillZooplaValue();
		}

		[Activation]
		private void BackfillLandRegistry2PropertyLink() {
			if ((m_aryArgs.Length != 1)) {
				m_oLog.Msg("Usage: BackfillLandRegistry2PropertyLink");
				return;
			} // if

			m_oServiceClient.BackfillLandRegistry2PropertyLink();
		}

		[Activation]
		private void BackfillNonLimitedCompanies() {
			if ((m_aryArgs.Length != 1)) {
				m_oLog.Msg("Usage: BackfillNonLimitedCompanies");
				return;
			} // if

			m_oServiceClient.BackfillNonLimitedCompanies();
		}

		[Activation]
		private void CalculateVatReturnSummary() {
			int nCustomerMarketplaceID;

			if ((m_aryArgs.Length != 2) || !int.TryParse(m_aryArgs[1], out nCustomerMarketplaceID)) {
				m_oLog.Msg("Usage: CalculateVatReturnSummary <Customer Marketplace ID>");
				return;
			} // if

			var serviceAccessor = ObjectFactory.GetInstance<IEzServiceAccessor>();

			serviceAccessor.CalculateVatReturnSummary(nCustomerMarketplaceID);
		} // CalculateVatReturnSummary

		[Activation]
		private void MarketplaceInstantUpdate() {
			int nCustomerMarketplaceID;

			if ((m_aryArgs.Length != 2) || !int.TryParse(m_aryArgs[1], out nCustomerMarketplaceID)) {
				m_oLog.Msg("Usage: MarketplaceInstantUpdate <Customer Marketplace ID>");
				return;
			} // if

			m_oServiceClient.MarketplaceInstantUpdate(nCustomerMarketplaceID);
		} // MarketplaceInstantUpdate

		[Activation]
		private void LoadVatReturnSummary() {
			int customerId;
			int nCustomerMarketplaceID;

			if ((m_aryArgs.Length != 3) || !int.TryParse(m_aryArgs[1], out nCustomerMarketplaceID) || !int.TryParse(m_aryArgs[2], out customerId)) {
				m_oLog.Msg("Usage: LoadVatReturnSummary <Customer Marketplace ID> <Customer ID>");
				return;
			} // if

			VatReturnDataActionResult oResult = m_oServiceClient.LoadVatReturnSummary(customerId, nCustomerMarketplaceID);

			m_oLog.Msg("Result is:\n{0}", string.Join("\n", oResult.Summary.Select(x => x.ToString())));
		} // LoadVatReturnSummary

		[Activation]
		private void LoadVatReturnFullData() {
			int customerId;
			int nCustomerMarketplaceID;

			if ((m_aryArgs.Length != 3) || !int.TryParse(m_aryArgs[1], out nCustomerMarketplaceID) || !int.TryParse(m_aryArgs[2], out customerId)) {
				m_oLog.Msg("Usage: LoadVatReturnFullData <Customer Marketplace ID> <Customer ID>");
				return;
			} // if

			VatReturnDataActionResult oResult = m_oServiceClient.LoadVatReturnFullData(customerId, nCustomerMarketplaceID);

			m_oLog.Msg("VAT return - begin:");

			foreach (var v in oResult.VatReturnRawData)
				m_oLog.Msg(v.ToString());

			m_oLog.Msg("VAT return - end.");

			m_oLog.Msg("RTI months - begin:");

			foreach (var v in oResult.RtiTaxMonthRawData)
				m_oLog.Msg(v.ToString());

			m_oLog.Msg("RTI months - end.");

			m_oLog.Msg("Summary - begin:");

			foreach (var v in oResult.Summary)
				m_oLog.Msg(v.ToString());

			m_oLog.Msg("Summary - end.");
		} // LoadVatReturnFullData

		[Activation]
		private void AndRecalculateVatReturnSummaryForAll() {
			m_oServiceClient.AndRecalculateVatReturnSummaryForAll();
		} // AndRecalculateVatReturnSummaryForAll

		[Activation]
		private void EncryptChannelGrabberMarketplaces() {
			m_oServiceClient.EncryptChannelGrabberMarketplaces();
		} // EncryptChannelGrabberMarketplaces

		[Activation]
		private void LoadVatReturnRawData() {
			int nCustomerMarketplaceID;

			if ((m_aryArgs.Length != 2) || !int.TryParse(m_aryArgs[1], out nCustomerMarketplaceID)) {
				m_oLog.Msg("Usage: LoadVatReturnRawData <Customer Marketplace ID>");
				return;
			} // if

			VatReturnDataActionResult oResult = m_oServiceClient.LoadVatReturnRawData(nCustomerMarketplaceID);

			m_oLog.Msg("VAT return - begin:");

			foreach (var v in oResult.VatReturnRawData)
				m_oLog.Msg(v.ToString());

			m_oLog.Msg("VAT return - end.");

			m_oLog.Msg("RTI months - begin:");

			foreach (var v in oResult.RtiTaxMonthRawData)
				m_oLog.Msg(v.ToString());

			m_oLog.Msg("RTI months - end.");
		} // LoadVatReturnRawData

		[Activation]
		private void LoadManualVatReturnPeriods() {
			int nCustomerMarketplaceID;

			if ((m_aryArgs.Length != 2) || !int.TryParse(m_aryArgs[1], out nCustomerMarketplaceID)) {
				m_oLog.Msg("Usage: LoadVatReturnSummary <Customer Marketplace ID>");
				return;
			} // if

			VatReturnPeriodsActionResult oResult = m_oServiceClient.LoadManualVatReturnPeriods(nCustomerMarketplaceID);

			m_oLog.Msg("Result is:\n{0}", string.Join("\n", oResult.Periods.Select(x => x.ToString())));
		} // LoadManualVatReturnPeriods

		[Activation]
		private void DisplayMarketplaceSecurityData() {
			int nCustomerID;

			if ((m_aryArgs.Length != 2) || !int.TryParse(m_aryArgs[1], out nCustomerID)) {
				m_oLog.Msg("Usage: DisplayMarketplaceSecurityData <Customer ID>");
				return;
			} // if

			m_oServiceClient.DisplayMarketplaceSecurityData(nCustomerID);
		} // DisplayMarketplaceSecurityData

		[Activation]
		private void UpdateLinkedHmrcPassword() {
			int nCustomerID;

			if ((m_aryArgs.Length != 4) || !int.TryParse(m_aryArgs[1], out nCustomerID)) {
				m_oLog.Msg("Usage: UpdateLinkedHmrcPassword <Customer ID> <Display name> <Password>");
				return;
			} // if

			string sDisplayName = m_aryArgs[2];
			string sPassword = m_aryArgs[3];
			string sHash = SecurityUtils.Hash(nCustomerID + sPassword + sDisplayName);

			m_oServiceClient.UpdateLinkedHmrcPassword(
				new Encrypted(nCustomerID.ToString(CultureInfo.InvariantCulture)),
				new Encrypted(sDisplayName),
				new Encrypted(sPassword), sHash
			);
		} // UpdateLinkedHmrcPassword

		[Activation]
		private void Encrypt() {
			if (m_aryArgs.Length < 2) {
				m_oLog.Msg("Usage: Encrypt <what to encrypt>");
				m_oLog.Msg("Concatenates all the passed arguments, separating them with a space, and encrypts the result.");
				return;
			} // if

			var sInput = string.Join(" ", m_aryArgs, 1, m_aryArgs.Length - 1);
			var oOutput = new Encrypted(sInput);

			m_oLog.Msg("\n\nInput: {0}\nOutput: {1}\n", sInput, oOutput);
		} // Encrypt

		[Activation]
		private void Decrypt() {
			if (m_aryArgs.Length < 2) {
				m_oLog.Msg("Usage: Decrypt <what to decrypt>");
				m_oLog.Msg("Concatenates all the passed arguments, separating them with a space, and decrypts the result.");
				return;
			} // if

			var sInput = string.Join(" ", m_aryArgs, 1, m_aryArgs.Length - 1);
			string sOutput;

			try {
				sOutput = Encrypted.Decrypt(sInput);
			}
			catch (Exception e) {
				m_oLog.Warn(e, "Failed to decrypt.");
				sOutput = string.Empty;
			} // try

			m_oLog.Msg("\n\nInput: {0}\nOutput: {1}\n", sInput, sOutput);
		} // Decrypt

		[Activation]
		private void EsignProcessPending() {
			if (m_aryArgs.Length == 1) {
				m_oServiceClient.EsignProcessPending(null);
				return;
			} // if

			int nCustomerID;

			if ((m_aryArgs.Length != 2) || !int.TryParse(m_aryArgs[1], out nCustomerID)) {
				m_oLog.Msg("Usage: EsignProcessPending [<Customer ID>]");
				return;
			} // if

			m_oServiceClient.EsignProcessPending(nCustomerID);
		} // EsignProcessPending

		[Activation]
		private void LoadEsignatures() {
			int zu;
			int? nCustomerID = null;
			bool bReady = false;
			bool bPollStatus = false;

			if (m_aryArgs.Length == 1)
				bReady = true;
			else if ((m_aryArgs.Length == 2) && int.TryParse(m_aryArgs[1], out zu)) {
				nCustomerID = zu;
				bReady = true;
			}
			else if ((m_aryArgs.Length == 3) && int.TryParse(m_aryArgs[1], out zu) && bool.TryParse(m_aryArgs[2], out bPollStatus)) {
				nCustomerID = zu;
				bReady = true;
			}

			if (!bReady) {
				m_oLog.Msg("Usage: LoadEsignatures [<Customer ID> [Poll Status (true/false)]]");
				return;
			} // if

			var elar = m_oServiceClient.LoadEsignatures(1, nCustomerID, bPollStatus);

			foreach (var e in elar.Data)
				m_oLog.Msg("{0}", e);
		} // LoadEsignatures

		[Activation]
		private void BackfillExperianDirectors() {
			if (m_aryArgs.Length == 1) {
				m_oServiceClient.BackfillExperianDirectors(null);
				return;
			} // if

			int nCustomerID;

			if ((m_aryArgs.Length != 2) || !int.TryParse(m_aryArgs[1], out nCustomerID)) {
				m_oLog.Msg("Usage: BackfillExperianDirectors [<Customer ID>]");
				return;
			} // if

			m_oServiceClient.BackfillExperianDirectors(nCustomerID);
		} // BackfillExperianDirectors

		[Activation]
		private void ParseExperianLtd() {
			long nServiceLogID;

			if ((m_aryArgs.Length != 2) || !long.TryParse(m_aryArgs[1], out nServiceLogID)) {
				m_oLog.Msg("Usage: ParseExperianLtd <MP_ServiceLog entry ID>");
				return;
			} // if
			m_oServiceClient.ParseExperianLtd(nServiceLogID);
		} // ParseExperianLtd

		[Activation]
		private void BackfillExperianLtd() {
			m_oServiceClient.BackfillExperianLtd();
		} // BackfillExperianLtd

		[Activation]
		private void BackfillExperianConsumer() {
			m_oServiceClient.BackfillExperianConsumer();
		} // BackfillExperianConsumer

		[Activation]
		private void LoadExperianLtd() {
			long nServiceLogID;

			if ((m_aryArgs.Length != 2) || !long.TryParse(m_aryArgs[1], out nServiceLogID)) {
				m_oLog.Msg("Usage: LoadExperianLtd <MP_ServiceLog entry ID>");
				return;
			} // if

			ExperianLtdActionResult lear = m_oServiceClient.LoadExperianLtd(nServiceLogID);

			m_oLog.Msg("Result:\n{0}", lear.Value.StringifyAll());
		} // LoadExperianLtd

		[Activation]
		private void LoadExperianConsumer() {
			long nServiceLogID;

			if ((m_aryArgs.Length != 2) || !long.TryParse(m_aryArgs[1], out nServiceLogID)) {
				m_oLog.Msg("Usage: LoadExperianLtd <MP_ServiceLog entry ID>");
				return;
			} // if

			var res = m_oServiceClient.LoadExperianConsumer(1, 1, null, nServiceLogID);

			m_oLog.Msg("Result:\n{0}", res.Value);
		} // LoadExperianLtd

		[Activation]
		private void ParseExperianConsumer() {
			long nServiceLogID;

			if ((m_aryArgs.Length != 2) || !long.TryParse(m_aryArgs[1], out nServiceLogID)) {
				m_oLog.Msg("Usage: LoadExperianConsumer <MP_ServiceLog entry ID>");
				return;
			} // if

			var res = m_oServiceClient.ParseExperianConsumer(nServiceLogID);

			m_oLog.Msg("Result:\n{0}", res.Value);
		} // LoadExperianLtd

		[Activation]
		private void StressTest() {
			int nLength;
			int nCount;

			var oUsage = new Action(() => {
				m_oLog.Msg("Usage: StressTest <count> <length>");
				m_oLog.Msg("Where");
				m_oLog.Msg("\tcount - how many requests to execute");
				m_oLog.Msg("\tlength - length in seconds of each request (1..100 seconds).");
			});

			if (m_aryArgs.Length != 3 || !int.TryParse(m_aryArgs[1], out nCount) || !int.TryParse(m_aryArgs[2], out nLength)) {
				oUsage();
				return;
			} // if

			if (nCount < 1) {
				oUsage();
				return;
			} // if

			if ((nLength < 1) || (nLength > 100)) {
				oUsage();
				return;
			} // if

			m_oLog.Debug("Stress test with {0} requests of {1} seconds each...", nCount, nLength);

			for (int i = 0; i < nCount; i++)
				m_oAdminClient.StressTestAction(nLength, i.ToString(CultureInfo.InvariantCulture));

			m_oLog.Debug("Stress test with {0} requests of {1} seconds each is complete.", nCount, nLength);
		} // StressTest

		[Activation]
		private void StressTestSync() {
			int nLength;

			var oUsage = new Action(() => {
				m_oLog.Msg("Usage: StressTestSync <count> <message>");
				m_oLog.Msg("Where");
				m_oLog.Msg("\tcount - how many requests to execute");
			});

			if (m_aryArgs.Length != 3 || !int.TryParse(m_aryArgs[1], out nLength)) {
				oUsage();
				return;
			} // if

			string sMsg = m_aryArgs[2];

			if ((nLength < 1) || (nLength > 100)) {
				oUsage();
				return;
			} // if

			m_oAdminClient.StressTestSync(nLength, sMsg);
		} // StressTestSync

		[Activation]
		private void Nop() {
			int nLength;

			var oUsage = new Action(() => {
				m_oLog.Msg("Usage: NOP <length> <message>");
				m_oLog.Msg("Where");
				m_oLog.Msg("\tlength - for how many seconds continue executing");
				m_oLog.Msg("\tmessage - some message to print to log file");
			});

			if (m_aryArgs.Length != 3 || !int.TryParse(m_aryArgs[1], out nLength)) {
				oUsage();
				return;
			} // if

			string sMsg = m_aryArgs[2];

			if (nLength < 1) {
				oUsage();
				return;
			} // if

			m_oAdminClient.Nop(nLength, sMsg);
		} // Nop

		[Activation]
		private void Noop() {
			m_oAdminClient.Noop();
		} // Noop

		[Activation]
		private void BrokerApproveAndResetCustomerPassword() {
			int nUnderwriterID;
			int nCustomerID;
			decimal nLoanAmount;
			int nValidHours;
			bool isFirst;

			if (
				m_aryArgs.Length != 6 ||
				!int.TryParse(m_aryArgs[1], out nUnderwriterID) ||
				!int.TryParse(m_aryArgs[2], out nCustomerID) ||
				!decimal.TryParse(m_aryArgs[3], out nLoanAmount) ||
				!int.TryParse(m_aryArgs[4], out nValidHours) ||
				!bool.TryParse(m_aryArgs[5], out isFirst)
			) {
				m_oLog.Msg("Usage: BrokerApproveAndResetCustomerPassword <underwriter id> <customer id> <loan amount> <valid hours> <is first approval>");
				return;
			} // if

			m_oServiceClient.BrokerApproveAndResetCustomerPassword(nUnderwriterID, nCustomerID, nLoanAmount, nValidHours, isFirst);
		} // BrokerApproveAndResetCustomerPassword

		[Activation]
		private void WriteToLog() {
			if (m_aryArgs.Length < 3) {
				m_oLog.Msg("Usage: WriteToLog <severity> <arg1> <arg2> ... <argN>");
				m_oLog.Msg("All the args are merged into a message which is written to service log with requested severity.");
				return;
			} // if

			m_oAdminClient.WriteToLog(m_aryArgs[1], string.Join(" ", m_aryArgs.Skip(2)));
		} // Noop

		[Activation]
		private void UpdateGoogleAnalytics() {
			if (m_aryArgs.Length < 2) {
				m_oServiceClient.UpdateGoogleAnalytics(null, null);
				return;
			} // if

			DateTime oFrom;
			DateTime oTo;

			if (
				(m_aryArgs.Length != 3) ||
				!DateTime.TryParse(m_aryArgs[1], out oFrom) ||
				!DateTime.TryParse(m_aryArgs[2], out oTo)
			) {
				m_oLog.Msg("Usage: UpdateGoogleAnalytics [<backfill from date> <backfill to date>]");
				return;
			} // if

			m_oServiceClient.UpdateGoogleAnalytics(oFrom, oTo);
		} // UpdateGoogleAnalytics

		[Activation]
		private void VerifyReapproval() {
			int nCustomerCount = -1;
			int nLastCheckedCustomerID = -1;

			bool bGood = false;

			if (m_aryArgs.Length == 1) {
				nCustomerCount = -1;
				nLastCheckedCustomerID = -1;
				bGood = true;
			}
			else if (m_aryArgs.Length == 2) {
				bGood = int.TryParse(m_aryArgs[1], out nCustomerCount);
				nLastCheckedCustomerID = -1;
			}
			else if (m_aryArgs.Length == 3) {
				bGood = int.TryParse(m_aryArgs[1], out nCustomerCount);
				bGood = bGood && int.TryParse(m_aryArgs[2], out nLastCheckedCustomerID);
			} // if

			if (!bGood) {
				m_oLog.Msg("Usage: VerifyReapproval [ <customer count>  [ <last checked customer id> ] ]");
				m_oLog.Msg("Specify customer count 0 or negative to run on all the customers.");
				m_oLog.Msg("Specify customer count 0 or negative to start from the beginning.");
				return;
			} // if

			m_oServiceClient.VerifyReapproval(nCustomerCount, nLastCheckedCustomerID);
		} // VerifyReapproval

		[Activation]
		private void VerifyApproval() {
			int nCustomerCount = -1;
			int nLastCheckedCustomerID = -1;

			bool bGood = false;

			if (m_aryArgs.Length == 1) {
				nCustomerCount = -1;
				nLastCheckedCustomerID = -1;
				bGood = true;
			}
			else if (m_aryArgs.Length == 2) {
				bGood = int.TryParse(m_aryArgs[1], out nCustomerCount);
				nLastCheckedCustomerID = -1;
			}
			else if (m_aryArgs.Length == 3) {
				bGood = int.TryParse(m_aryArgs[1], out nCustomerCount);
				bGood = bGood && int.TryParse(m_aryArgs[2], out nLastCheckedCustomerID);
			} // if

			if (!bGood) {
				m_oLog.Msg("Usage: VerifyApproval [ <customer count>  [ <last checked customer id> ] ]");
				m_oLog.Msg("Specify customer count 0 or negative to run on all the customers.");
				m_oLog.Msg("Specify customer count 0 or negative to start from the beginning.");
				return;
			} // if

			m_oServiceClient.VerifyApproval(nCustomerCount, nLastCheckedCustomerID);
		} // VerifyApproval

		// ReSharper restore UnusedMember.Local
		#endregion strategy activators

		#region fields

		private readonly string[] m_aryArgs;
		private readonly EzServiceClient m_oServiceClient;
		private readonly EzServiceAdminClient m_oAdminClient;
		private SortedDictionary<string, MethodInfo> m_oMethods;
		private readonly ASafeLog m_oLog;
		private readonly AConnection m_oDB;

		#endregion fields

		#endregion private
	} // class ServiceClientActivation
} // namespace
