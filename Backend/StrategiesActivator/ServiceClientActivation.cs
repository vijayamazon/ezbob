namespace StrategiesActivator {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.Linq;
	using System.Reflection;
	using System.ServiceModel;
	using System.Text;
	using ConfigManager;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Models.NewLoan;
	using Ezbob.Database;
	using Ezbob.Database.Pool;
	using Ezbob.Logger;
	using Ezbob.Utils.Security;
	using EzServiceConfigurationLoader;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using log4net;
	using Newtonsoft.Json;
	using ServiceClientProxy.EzServiceReference;

	[SuppressMessage("ReSharper", "UnusedMember.Local")]
	public class ServiceClientActivation {
		public ServiceClientActivation(string[] args) {
			log = new ConsoleLog(new SafeILog(LogManager.GetLogger(typeof(ServiceClientActivation))));
			log.NotifyStart();

			InitMethods();

			string sInstanceName = args[0].ToLower();

			if (sInstanceName.ToLower() == "list") {
				ListSupported();
				log.NotifyStop();
				throw new ExitException();
			} // if

			var env = new Ezbob.Context.Environment(log);
			var db = new SqlConnection(env, log);

			CurrentValues.Init(db, log);
			DbConnectionPool.ReuseCount = CurrentValues.Instance.ConnectionPoolReuseCount;
			AConnection.UpdateConnectionPoolMaxSize(CurrentValues.Instance.ConnectionPoolMaxSize);

			Configuration cfg;

			if (methodList.ContainsKey(sInstanceName)) {
				cfg = new DefaultConfiguration(System.Environment.MachineName, db, log);
				cmdLineArgs = args;
			} else {
				cmdLineArgs = new string[args.Length - 1];
				Array.Copy(args, 1, cmdLineArgs, 0, args.Length - 1);

				cfg = new Configuration(sInstanceName, db, log);
			} // if

			cfg.Init();

			log.Info("Running against instance {0} - {1}.", cfg.InstanceID, cfg.InstanceName);
			log.Info("Arguments:\n\t{0}", string.Join("\n\t", cmdLineArgs));

			var oTcpBinding = new NetTcpBinding();

			serviceClient = new EzServiceClient(
				oTcpBinding,
				new EndpointAddress(cfg.AdminEndpointAddress)
				);

			adminClient = new EzServiceAdminClient(
				oTcpBinding,
				new EndpointAddress(cfg.AdminEndpointAddress)
				);
		} // constructor

		public void Execute() {
			string strategyName = cmdLineArgs[0];

			string sKey = strategyName.ToLower();

			if (methodList.ContainsKey(sKey))
				methodList[sKey].Invoke(this, new object[] { });
			else {
				log.Msg("Strategy {0} is not supported", strategyName);
				ListSupported();
			} // if

			log.NotifyStop();
		} // Execute

		[Activation]
		private void AmlChecker() {
			int customerId;

			if (cmdLineArgs.Length == 2 && int.TryParse(cmdLineArgs[1], out customerId)) {
				serviceClient.CheckAml(customerId, 1);
				return;
			}

			if (cmdLineArgs.Length == 9 && int.TryParse(cmdLineArgs[1], out customerId)) {
				serviceClient.CheckAmlCustom(1, customerId, cmdLineArgs[2], cmdLineArgs[3], cmdLineArgs[4], cmdLineArgs[5], cmdLineArgs[6], cmdLineArgs[7], cmdLineArgs[8]);
				return;
			}

			log.Msg("Usage: AmlChecker <CustomerId>");
			log.Msg("OR");
			log.Msg("Usage: AmlChecker <CustomerId> <idhubHouseNumber> <idhubHouseName> <idhubStreet> <idhubDistrict> <idhubTown> <idhubCounty> <idhubPostCode>");
		}

		[Activation]
		private void AndRecalculateVatReturnSummaryForAll() {
			serviceClient.AndRecalculateVatReturnSummaryForAll();
		}

		[Activation]
		private void ApprovedUser() {
			int underwriterId;
			int customerId;
			decimal loanAmount;
			int validFor;
			bool isFirst;
			if (cmdLineArgs.Length != 6 || !int.TryParse(cmdLineArgs[1], out underwriterId) || !int.TryParse(cmdLineArgs[2], out customerId) || !decimal.TryParse(cmdLineArgs[3], out loanAmount) || !int.TryParse(cmdLineArgs[4], out validFor) || bool.TryParse(cmdLineArgs[5], out isFirst)) {
				log.Msg("Usage: ApprovedUser <Underwriter ID> <CustomerId> <loanAmount> <ValidFor> <isFirst>");
				return;
			}

			serviceClient.ApprovedUser(underwriterId, customerId, loanAmount, validFor, isFirst);
		}

		[Activation]
		private void BackfillAml() {
			if ((cmdLineArgs.Length != 1)) {
				log.Msg("Usage: BackfillAml");
				return;
			} // if

			serviceClient.BackfillAml();
		}

		[Activation]
		private void BackfillCustomerAnalyticsCompany() {
			serviceClient.BackfillCustomerAnalyticsCompany();
		}

		[Activation]
		private void BackfillExperianConsumer() {
			serviceClient.BackfillExperianConsumer();
		}

		[Activation]
		private void BackfillExperianDirectors() {
			if (cmdLineArgs.Length == 1) {
				serviceClient.BackfillExperianDirectors(null);
				return;
			} // if

			int nCustomerID;

			if ((cmdLineArgs.Length != 2) || !int.TryParse(cmdLineArgs[1], out nCustomerID)) {
				log.Msg("Usage: BackfillExperianDirectors [<Customer ID>]");
				return;
			} // if

			serviceClient.BackfillExperianDirectors(nCustomerID);
		}

		[Activation]
		private void BackfillExperianLtd() {
			serviceClient.BackfillExperianLtd();
		}

		[Activation]
		private void BackfillHmrcBusinessRelevance() {
			serviceClient.BackfillHmrcBusinessRelevance();
		}

		[Activation]
		private void BackfillLandRegistry2PropertyLink() {
			if ((cmdLineArgs.Length != 1)) {
				log.Msg("Usage: BackfillLandRegistry2PropertyLink");
				return;
			} // if

			serviceClient.BackfillLandRegistry2PropertyLink();
		}

		[Activation]
		private void BackfillNonLimitedCompanies() {
			if ((cmdLineArgs.Length != 1)) {
				log.Msg("Usage: BackfillNonLimitedCompanies");
				return;
			} // if

			serviceClient.BackfillNonLimitedCompanies();
		}

		[Activation]
		private void BackfillTurnover() {
			serviceClient.BackfillTurnover();
		}

		[Activation]
		private void BackfillZooplaValue() {
			if ((cmdLineArgs.Length != 1)) {
				log.Msg("Usage: BackfillZooplaValue");
				return;
			} // if

			serviceClient.BackfillZooplaValue();
		}

		[Activation]
		private void BrokerApproveAndResetCustomerPassword() {
			int nUnderwriterID;
			int nCustomerID;
			decimal nLoanAmount;
			int nValidHours;
			bool isFirst;

			if (
				cmdLineArgs.Length != 6 ||
					!int.TryParse(cmdLineArgs[1], out nUnderwriterID) ||
					!int.TryParse(cmdLineArgs[2], out nCustomerID) ||
					!decimal.TryParse(cmdLineArgs[3], out nLoanAmount) ||
					!int.TryParse(cmdLineArgs[4], out nValidHours) ||
					!bool.TryParse(cmdLineArgs[5], out isFirst)
				) {
				log.Msg("Usage: BrokerApproveAndResetCustomerPassword <underwriter id> <customer id> <loan amount> <valid hours> <is first approval>");
				return;
			} // if

			serviceClient.BrokerApproveAndResetCustomerPassword(nUnderwriterID, nCustomerID, nLoanAmount, nValidHours, isFirst);
		}

		[Activation]
		private void BrokerLoadCustomerList() {
			if (cmdLineArgs.Length != 2) {
				log.Msg("Usage: BrokerLoadCustomerList <Contact person email>");
				return;
			} // if

			BrokerCustomersActionResult res = serviceClient.BrokerLoadCustomerList(cmdLineArgs[1]);

			foreach (var oEntry in res.Customers)
				log.Msg("Customer ID: {0} Name: {1} {2}", oEntry.CustomerID, oEntry.FirstName, oEntry.LastName);
		}

		[Activation]
		private void BwaChecker() {
			int customerId;

			if (cmdLineArgs.Length == 2 && int.TryParse(cmdLineArgs[1], out customerId)) {
				serviceClient.CheckBwa(customerId, 1);
				return;
			}

			if (cmdLineArgs.Length == 11 && int.TryParse(cmdLineArgs[1], out customerId)) {
				serviceClient.CheckBwaCustom(1, customerId, cmdLineArgs[2], cmdLineArgs[3], cmdLineArgs[4], cmdLineArgs[5], cmdLineArgs[6], cmdLineArgs[7], cmdLineArgs[8], cmdLineArgs[9], cmdLineArgs[10]);
				return;
			}

			log.Msg("Usage: BwaChecker <CustomerId>");
			log.Msg("OR");
			log.Msg("Usage: BwaChecker <CustomerId> <idhubHouseNumber> <idhubHouseName> <idhubStreet> <idhubDistrict> <idhubTown> <idhubCounty> <idhubPostCode> <idhubBranchCode> <idhubAccountNumber>");
		}

		[Activation]
		private void CaisGenerate() {
			int underwriterId;
			if (cmdLineArgs.Length != 2 || !int.TryParse(cmdLineArgs[1], out underwriterId)) {
				log.Msg("Usage: CaisGenerate <underwriterId>");
				return;
			}

			serviceClient.CaisGenerate(underwriterId);
		}

		[Activation]
		private void CaisUpdate() {
			int underwriterId;
			int caisId;
			if (cmdLineArgs.Length != 3 || !int.TryParse(cmdLineArgs[1], out underwriterId) || !int.TryParse(cmdLineArgs[2], out caisId)) {
				log.Msg("Usage: CaisUpdate <Underwriter ID> <caisId>");
				return;
			}

			serviceClient.CaisUpdate(underwriterId, caisId);
		}

		[Activation]
		private void CalculateVatReturnSummary() {
			int nCustomerMarketplaceID;

			if ((cmdLineArgs.Length != 2) || !int.TryParse(cmdLineArgs[1], out nCustomerMarketplaceID)) {
				log.Msg("Usage: CalculateVatReturnSummary <Customer Marketplace ID>");
				return;
			} // if

			serviceClient.CalculateVatReturnSummary(nCustomerMarketplaceID);
		}

		[Activation]
		private void CashTransferred() {
			int customerId;
			decimal amount;
			bool isFirst;
			if (cmdLineArgs.Length != 5 || !int.TryParse(cmdLineArgs[1], out customerId) || !decimal.TryParse(cmdLineArgs[2], out amount) || !bool.TryParse(cmdLineArgs[3], out isFirst)) {
				log.Msg("Usage: CashTransferred <CustomerId> <amount> <loanRefNum>");
				return;
			}
			string loanRefNum = cmdLineArgs[3];
			serviceClient.CashTransferred(customerId, amount, loanRefNum, isFirst);
		}

		[Activation]
		private void ChangeBrokerEmail() {
			if ((cmdLineArgs.Length != 4)) {
				log.Msg("Usage: ChangeBrokerEmail <OldEmail> <NewEmail> <NewPassword>");
				return;
			}

			serviceClient.ChangeBrokerEmail(cmdLineArgs[1], cmdLineArgs[2], cmdLineArgs[3]);
		}

		[Activation]
		private void CreateUnderwriter() {
			if (cmdLineArgs.Length != 4) {
				log.Msg("Usage: CreateUnderwriter <Name> <Password> <RoleName>");
				log.Msg(@"Available roles (underwriter is usually one of the last three):
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

			serviceClient.UnderwriterSignup(cmdLineArgs[1], new Password(cmdLineArgs[2]), cmdLineArgs[3]);
		}

		[Activation]
		private void CustomerMarketPlaceAdded() {
			int customerId, marketplaceId;
			bool bUpdateWizardStep;
			if (cmdLineArgs.Length != 4 || !int.TryParse(cmdLineArgs[1], out customerId) || !int.TryParse(cmdLineArgs[2], out marketplaceId) || !bool.TryParse(cmdLineArgs[3], out bUpdateWizardStep)) {
				log.Msg("Usage: CustomerMarketPlaceAdded <CustomerId> <CustomerMarketplaceId> <Update customer wizard step>");
				log.Msg("<Update customer wizard step>: is boolean and means whether to set customer wizard step to Marketplace or not. If wizard step is 'TheLastOne' it is never changed.");
				return;
			}

			serviceClient.UpdateMarketplace(customerId, marketplaceId, bUpdateWizardStep, 1);
		}

		[Activation]
		private void Decrypt() {
			if (cmdLineArgs.Length < 2) {
				log.Msg("Usage: Decrypt <what to decrypt>");
				log.Msg("Concatenates all the passed arguments, separating them with a space, and decrypts the result.");
				return;
			} // if

			var sInput = string.Join(" ", cmdLineArgs, 1, cmdLineArgs.Length - 1);
			string sOutput;

			try {
				sOutput = Encrypted.Decrypt(sInput);
			} catch (Exception e) {
				log.Warn(e, "Failed to decrypt.");
				sOutput = string.Empty;
			} // try

			log.Msg("\n\nInput: {0}\nOutput: {1}\n", sInput, sOutput);
		}

		[Activation]
		private void DisplayMarketplaceSecurityData() {
			int nCustomerID;

			if ((cmdLineArgs.Length != 2) || !int.TryParse(cmdLineArgs[1], out nCustomerID)) {
				log.Msg("Usage: DisplayMarketplaceSecurityData <Customer ID>");
				return;
			} // if

			serviceClient.DisplayMarketplaceSecurityData(nCustomerID);
		}

		[Activation]
		private void EmailRolloverAdded() {
			int customerId;
			decimal amount;
			if (cmdLineArgs.Length != 3 || !int.TryParse(cmdLineArgs[1], out customerId) || !decimal.TryParse(cmdLineArgs[2], out amount)) {
				log.Msg("Usage: EmailRolloverAdded <CustomerId> <amount>");
				return;
			}

			serviceClient.EmailRolloverAdded(1, customerId, amount);
		}

		[Activation]
		private void EmailUnderReview() {
			int customerId;
			if (cmdLineArgs.Length != 2 || !int.TryParse(cmdLineArgs[1], out customerId)) {
				log.Msg("Usage: EmailUnderReview <CustomerId>");
				return;
			}

			serviceClient.EmailUnderReview(customerId);
		}

		[Activation]
		private void Encrypt() {
			if (cmdLineArgs.Length < 2) {
				log.Msg("Usage: Encrypt <what to encrypt>");
				log.Msg("Concatenates all the passed arguments, separating them with a space, and encrypts the result.");
				return;
			} // if

			var sInput = string.Join(" ", cmdLineArgs, 1, cmdLineArgs.Length - 1);
			var oOutput = new Encrypted(sInput);

			log.Msg("\n\nInput: {0}\nOutput: {1}\n", sInput, oOutput);
		}

		[Activation]
		private void EncryptChannelGrabberMarketplaces() {
			serviceClient.EncryptChannelGrabberMarketplaces();
		}

		[Activation]
		private void Escalated() {
			int customerId;
			if (cmdLineArgs.Length != 2 || !int.TryParse(cmdLineArgs[1], out customerId)) {
				log.Msg("Usage: Escalated <CustomerId>");
				return;
			}

			serviceClient.Escalated(customerId, 1);
		}

		[Activation]
		private void EsignProcessPending() {
			if (cmdLineArgs.Length == 1) {
				serviceClient.EsignProcessPending(null);
				return;
			} // if

			int nCustomerID;

			if ((cmdLineArgs.Length != 2) || !int.TryParse(cmdLineArgs[1], out nCustomerID)) {
				log.Msg("Usage: EsignProcessPending [<Customer ID>]");
				return;
			} // if

			serviceClient.EsignProcessPending(nCustomerID);
		}

		[Activation]
		private void ExperianCompanyCheck() {
			int customerId;
			bool forceCheck;
			if (cmdLineArgs.Length != 3 || !int.TryParse(cmdLineArgs[1], out customerId) || !bool.TryParse(cmdLineArgs[2], out forceCheck)) {
				log.Msg("Usage: ExperianCompanyCheck <CustomerId> <ForceCheck>");
				return;
			}

			serviceClient.ExperianCompanyCheck(1, customerId, forceCheck);
		}

		[Activation]
		private void ExperianConsumerCheck() {
			int customerId, directorId;
			bool forceCheck;

			if (cmdLineArgs.Length != 4 || !int.TryParse(cmdLineArgs[1], out customerId) || !int.TryParse(cmdLineArgs[2], out directorId) || !bool.TryParse(cmdLineArgs[3], out forceCheck)) {
				log.Msg("Usage: ExperianConsumerCheck <CustomerId> <DirectorId> <ForceCheck>");
				return;
			}

			serviceClient.ExperianConsumerCheck(1, customerId, directorId, forceCheck);
		}

		[Activation]
		private void FinishWizard() {
			int customerId, underwriterId;
			if (cmdLineArgs.Length < 3 || !int.TryParse(cmdLineArgs[1], out customerId) || !int.TryParse(cmdLineArgs[2], out underwriterId)) {
				log.Msg("Usage: FinishWizard <CustomerId> <UnderwriterId> [<DoSendEmail(true/false)>] [<AvoidAutoDecision(0/1)>]");
				return;
			}

			var oArgs = JsonConvert.DeserializeObject<FinishWizardArgs>(CurrentValues.Instance.FinishWizardForApproved);

			oArgs.CustomerID = customerId;

			if (cmdLineArgs.Length >= 4)
				oArgs.DoSendEmail = bool.Parse(cmdLineArgs[3]);

			if (cmdLineArgs.Length >= 5)
				oArgs.AvoidAutoDecision = int.Parse(cmdLineArgs[4]);

			serviceClient.FinishWizard(oArgs, underwriterId);
		}

		[Activation]
		private void FirstOfMonthStatusNotifier() {
			if (cmdLineArgs.Length != 1) {
				log.Msg("Usage: FirstOfMonthStatusNotifier");
				return;
			}

			serviceClient.FirstOfMonthStatusNotifier();
		}

		[Activation]
		private void FraudChecker() {
			int customerId;
			FraudMode mode;

			if (cmdLineArgs.Length != 3 || !int.TryParse(cmdLineArgs[1], out customerId) || !Enum.TryParse(cmdLineArgs[2], true, out mode)) {
				log.Msg("Usage: FraudChecker <CustomerId> <Fraud mode>");
				log.Msg("Fraud mode values: {0}", string.Join(", ", Enum.GetValues(typeof(FraudMode))));
				return;
			}

			serviceClient.FraudChecker(customerId, mode);
		}

		[Activation]
		private void GenerateMobileCode() {
			if (cmdLineArgs.Length != 2) {
				log.Msg("Usage: GenerateMobileCode <phone number>");
				return;
			}

			serviceClient.GenerateMobileCode(cmdLineArgs[1]);
		}

		[Activation]
		private void GeneratePassword() {
			if (cmdLineArgs.Length < 2) {
				log.Msg(@"Usage: GeneratePassword <arg 1> <arg 2> ... <arg N>

Generates password hash by concatenating all the arguments in the order of appearance.

I.e. to generate broker password call
GeneratePassword broker-contact-email@example.com password-itself

");
				return;
			} // if

			var os = new StringBuilder();
			for (int i = 1; i < cmdLineArgs.Length; i++)
				os.Append(cmdLineArgs[i]);

			string sOriginalPassword = os.ToString();

			string sHash = SecurityUtils.HashPassword(sOriginalPassword);

			log.Msg(
				"\n\nOriginal string:\n\t{0}\n\ngenerated hash:\n\t{1}\n\nquery:\n" +
					"\tUPDATE Broker SET Password = '{1}' WHERE ContactEmail = '{2}'\n" +
					"\n\tUPDATE Security_User SET EzPassword = '{1}' WHERE UserName = '{2}'\n",
				sOriginalPassword, sHash, cmdLineArgs[1]
				);
		}

		[Activation]
		private void GetCashFailed() {
			int customerId;
			if (cmdLineArgs.Length != 2 || !int.TryParse(cmdLineArgs[1], out customerId)) {
				log.Msg("Usage: GetCashFailed <CustomerId>");
				return;
			}

			serviceClient.GetCashFailed(customerId);
		}

		[Activation]
		private void GetWizardConfigs() {
			serviceClient.GetWizardConfigs();
		}

		private void InitMethods() {
			MethodInfo[] aryMethods = GetType()
				.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);

			methodList = new SortedDictionary<string, MethodInfo>();

			foreach (MethodInfo mi in aryMethods) {
				IEnumerable<ActivationAttribute> oAttrList = mi.GetCustomAttributes<ActivationAttribute>();

				if (oAttrList.Any())
					methodList[mi.Name.ToLower()] = mi;
			} // foreach
		}

		[Activation]
		private void LateBy14Days() {
			if (cmdLineArgs.Length != 1) {
				log.Msg("Usage: LateBy14Days");
				return;
			}

			serviceClient.LateBy14Days();
		}

		[Activation]
		private void ListActiveActions() {
			StringListActionResult res = adminClient.ListActiveActions();

			log.Msg(
				"\nRetriever (i.e. this action):\n\t{{ {0}: {4} [{1}sync] {2}: {3} }}",
				res.MetaData.ActionID,
				res.MetaData.IsSynchronous ? "" : "a",
				res.MetaData.Status,
				res.MetaData.Comment ?? "-- no comments --",
				res.MetaData.Name
				);

			string sKey = "{ " + res.MetaData.ActionID;

			log.Msg("\nList of active actions - begin:\n");

			foreach (string s in res.Records) {
				if (!s.StartsWith(sKey))
					log.Msg("\t{0}\n", s);
			}

			log.Msg("\nList of active actions - end.");
		}

		private void ListSupported() {
			log.Msg("Supported strategies are (case insensitive):\n\t{0}", string.Join("\n\t", methodList.Keys));
		} // ListSupported

		// InitMethods

		[Activation]
		private void LoadEsignatures() {
			int zu;
			int? nCustomerID = null;
			bool bReady = false;
			bool bPollStatus = false;

			if (cmdLineArgs.Length == 1)
				bReady = true;
			else if ((cmdLineArgs.Length == 2) && int.TryParse(cmdLineArgs[1], out zu)) {
				nCustomerID = zu;
				bReady = true;
			} else if ((cmdLineArgs.Length == 3) && int.TryParse(cmdLineArgs[1], out zu) && bool.TryParse(cmdLineArgs[2], out bPollStatus)) {
				nCustomerID = zu;
				bReady = true;
			}

			if (!bReady) {
				log.Msg("Usage: LoadEsignatures [<Customer ID> [Poll Status (true/false)]]");
				return;
			} // if

			var elar = serviceClient.LoadEsignatures(1, nCustomerID, bPollStatus);

			foreach (var e in elar.Data)
				log.Msg("{0}", e);
		}

		[Activation]
		private void LoadExperianConsumer() {
			long nServiceLogID;

			if ((cmdLineArgs.Length != 2) || !long.TryParse(cmdLineArgs[1], out nServiceLogID)) {
				log.Msg("Usage: LoadExperianLtd <MP_ServiceLog entry ID>");
				return;
			} // if

			var res = serviceClient.LoadExperianConsumer(1, 1, null, nServiceLogID);

			log.Msg("Result:\n{0}", res.Value);
		}

		[Activation]
		private void LoadExperianLtd() {
			long nServiceLogID;

			if ((cmdLineArgs.Length != 2) || !long.TryParse(cmdLineArgs[1], out nServiceLogID)) {
				log.Msg("Usage: LoadExperianLtd <MP_ServiceLog entry ID>");
				return;
			} // if

			ExperianLtdActionResult lear = serviceClient.LoadExperianLtd(nServiceLogID);

			log.Msg("Result:\n{0}", lear.Value.StringifyAll());
		}

		[Activation]
		private void LoadManualVatReturnPeriods() {
			int nCustomerMarketplaceID;

			if ((cmdLineArgs.Length != 2) || !int.TryParse(cmdLineArgs[1], out nCustomerMarketplaceID)) {
				log.Msg("Usage: LoadVatReturnSummary <Customer Marketplace ID>");
				return;
			} // if

			VatReturnPeriodsActionResult oResult = serviceClient.LoadManualVatReturnPeriods(nCustomerMarketplaceID);

			log.Msg("Result is:\n{0}", string.Join("\n", oResult.Periods.Select(x => x.ToString())));
		}

		[Activation]
		private void LoadVatReturnFullData() {
			int customerId;
			int nCustomerMarketplaceID;

			if ((cmdLineArgs.Length != 3) || !int.TryParse(cmdLineArgs[1], out nCustomerMarketplaceID) || !int.TryParse(cmdLineArgs[2], out customerId)) {
				log.Msg("Usage: LoadVatReturnFullData <Customer Marketplace ID> <Customer ID>");
				return;
			} // if

			VatReturnDataActionResult oResult = serviceClient.LoadVatReturnFullData(customerId, nCustomerMarketplaceID);

			log.Msg("VAT return - begin:");

			foreach (var v in oResult.VatReturnRawData)
				log.Msg(v.ToString());

			log.Msg("VAT return - end.");

			log.Msg("RTI months - begin:");

			foreach (var v in oResult.RtiTaxMonthRawData)
				log.Msg(v.ToString());

			log.Msg("RTI months - end.");

			log.Msg("Summary - begin:");

			foreach (var v in oResult.Summary)
				log.Msg(v.ToString());

			log.Msg("Summary - end.");
		}

		[Activation]
		private void LoadVatReturnRawData() {
			int nCustomerMarketplaceID;

			if ((cmdLineArgs.Length != 2) || !int.TryParse(cmdLineArgs[1], out nCustomerMarketplaceID)) {
				log.Msg("Usage: LoadVatReturnRawData <Customer Marketplace ID>");
				return;
			} // if

			VatReturnDataActionResult oResult = serviceClient.LoadVatReturnRawData(nCustomerMarketplaceID);

			log.Msg("VAT return - begin:");

			foreach (var v in oResult.VatReturnRawData)
				log.Msg(v.ToString());

			log.Msg("VAT return - end.");

			log.Msg("RTI months - begin:");

			foreach (var v in oResult.RtiTaxMonthRawData)
				log.Msg(v.ToString());

			log.Msg("RTI months - end.");
		}

		[Activation]
		private void LoadVatReturnSummary() {
			int customerId;
			int nCustomerMarketplaceID;

			if ((cmdLineArgs.Length != 3) || !int.TryParse(cmdLineArgs[1], out nCustomerMarketplaceID) || !int.TryParse(cmdLineArgs[2], out customerId)) {
				log.Msg("Usage: LoadVatReturnSummary <Customer Marketplace ID> <Customer ID>");
				return;
			} // if

			VatReturnDataActionResult oResult = serviceClient.LoadVatReturnSummary(customerId, nCustomerMarketplaceID);

			log.Msg("Result is:\n{0}", string.Join("\n", oResult.Summary.Select(x => x.ToString())));
		}

		[Activation]
		private void LoanFullyPaid() {
			int customerId;
			if (cmdLineArgs.Length != 3 || !int.TryParse(cmdLineArgs[1], out customerId)) {
				log.Msg("Usage: LoanFullyPaid <CustomerId> <loanRefNum>");
				return;
			}

			serviceClient.LoanFullyPaid(customerId, cmdLineArgs[2]);
		}

		[Activation]
		private void MainStrategy() {
			int underwriterId;
			int customerId;
			NewCreditLineOption newCreditLineOption;
			int avoidAutoDescison;
			bool createCashRequest;
			bool updateCashRequest;

			if (int.TryParse(cmdLineArgs[1], out underwriterId) && int.TryParse(cmdLineArgs[2], out customerId) && Enum.TryParse(cmdLineArgs[3], out newCreditLineOption) && int.TryParse(cmdLineArgs[4], out avoidAutoDescison) && bool.TryParse(cmdLineArgs[5], out createCashRequest) && bool.TryParse(cmdLineArgs[6], out updateCashRequest)) {
				serviceClient.MainStrategy1(
					underwriterId,
					customerId,
					newCreditLineOption,
					avoidAutoDescison,
					null,
					createCashRequest ? MainStrategyDoAction.Yes : MainStrategyDoAction.No,
					updateCashRequest ? MainStrategyDoAction.Yes : MainStrategyDoAction.No
				);
				return;
			}

			// MainStrategy null 18234 3 1

			//	NewCreditLineOption.UpdateEverythingAndApplyAutoRules 3
			// avoidAutoDescison 0

			log.Msg("Usage: MainStrategy <Underwriter ID> <customerId> <newCreditLineOption> <avoidAutoDescison> <create cash request (true/false)> <update cash request (true/false)>");
		}

		[Activation]
		private void MainStrategySync() {
			int underwriterId;
			int customerId, avoidAutoDescison;
			NewCreditLineOption newCreditLineOption;
			bool createCashRequest;
			bool updateCashRequest;

			if (cmdLineArgs.Length == 5 && int.TryParse(cmdLineArgs[1], out underwriterId) && int.TryParse(cmdLineArgs[2], out customerId) && Enum.TryParse(cmdLineArgs[3], out newCreditLineOption) && int.TryParse(cmdLineArgs[4], out avoidAutoDescison) && bool.TryParse(cmdLineArgs[5], out createCashRequest) && bool.TryParse(cmdLineArgs[6], out updateCashRequest)) {
				serviceClient.MainStrategySync1(
					underwriterId,
					customerId,
					newCreditLineOption,
					avoidAutoDescison,
					null,
					createCashRequest ? MainStrategyDoAction.Yes : MainStrategyDoAction.No,
					updateCashRequest ? MainStrategyDoAction.Yes : MainStrategyDoAction.No
				);
				return;
			}

			log.Msg("Usage: MainStrategySync <Underwriter ID> <customerId> <newCreditLineOption> <avoidAutoDescison>");
		}

		[Activation]
		private void MarketplaceInstantUpdate() {
			int nCustomerMarketplaceID;

			if ((cmdLineArgs.Length != 2) || !int.TryParse(cmdLineArgs[1], out nCustomerMarketplaceID)) {
				log.Msg("Usage: MarketplaceInstantUpdate <Customer Marketplace ID>");
				return;
			} // if

			serviceClient.MarketplaceInstantUpdate(nCustomerMarketplaceID);
		}

		[Activation]
		private void MoreAmlAndBwaInformation() {
			int underwriterId;
			int customerId;
			if (cmdLineArgs.Length != 3 || !int.TryParse(cmdLineArgs[1], out underwriterId) || !int.TryParse(cmdLineArgs[2], out customerId)) {
				log.Msg("Usage: MoreAmlAndBwaInformation <Underwriter ID> <CustomerId>");
				return;
			}

			serviceClient.MoreAmlAndBwaInformation(underwriterId, customerId);
		}

		[Activation]
		private void MoreAmlInformation() {
			int underwriterId;
			int customerId;
			if (cmdLineArgs.Length != 3 || !int.TryParse(cmdLineArgs[1], out underwriterId) || !int.TryParse(cmdLineArgs[2], out customerId)) {
				log.Msg("Usage: MoreAmlInformation <Underwriter ID> <CustomerId>");
				return;
			}
			serviceClient.MoreAmlInformation(underwriterId, customerId);
		}

		[Activation]
		private void MoreBwaInformation() {
			int underwriterId;
			int customerId;
			if (cmdLineArgs.Length != 3 || !int.TryParse(cmdLineArgs[1], out underwriterId) || !int.TryParse(cmdLineArgs[2], out customerId)) {
				log.Msg("Usage: MoreBwaInformation <Underwriter ID> <CustomerId>");
				return;
			}

			serviceClient.MoreBwaInformation(underwriterId, customerId);
		}

		[Activation]
		private void Noop() {
			adminClient.Noop();
		}

		[Activation]
		private void Nop() {
			int nLength;

			var oUsage = new Action(() => {
				log.Msg("Usage: NOP <length> <message>");
				log.Msg("Where");
				log.Msg("\tlength - for how many seconds continue executing");
				log.Msg("\tmessage - some message to print to log file");
			});

			if (cmdLineArgs.Length != 3 || !int.TryParse(cmdLineArgs[1], out nLength)) {
				oUsage();
				return;
			} // if

			string sMsg = cmdLineArgs[2];

			if (nLength < 1) {
				oUsage();
				return;
			} // if

			adminClient.Nop(nLength, sMsg);
		}

		[Activation]
		private void NotifySalesOnNewCustomer() {
			int nCustomerID;

			if ((cmdLineArgs.Length != 2) || !int.TryParse(cmdLineArgs[1], out nCustomerID)) {
				log.Msg("Usage: NotifySalesOnNewCustomer <customer id>");
				return;
			} // if

			serviceClient.NotifySalesOnNewCustomer(nCustomerID);
		}

		[Activation]
		private void ParseExperianConsumer() {
			long nServiceLogID;

			if ((cmdLineArgs.Length != 2) || !long.TryParse(cmdLineArgs[1], out nServiceLogID)) {
				log.Msg("Usage: LoadExperianConsumer <MP_ServiceLog entry ID>");
				return;
			} // if

			var res = serviceClient.ParseExperianConsumer(nServiceLogID);

			log.Msg("Result:\n{0}", res.Value);
		}

		[Activation]
		private void ParseExperianLtd() {
			long nServiceLogID;

			if ((cmdLineArgs.Length != 2) || !long.TryParse(cmdLineArgs[1], out nServiceLogID)) {
				log.Msg("Usage: ParseExperianLtd <MP_ServiceLog entry ID>");
				return;
			} // if
			serviceClient.ParseExperianLtd(nServiceLogID);
		}

		[Activation]
		private void PasswordRestored() {
			int customerId;
			if (cmdLineArgs.Length != 2 || !int.TryParse(cmdLineArgs[1], out customerId)) {
				log.Msg("Usage: PasswordRestored <CustomerId>");
				return;
			}

			serviceClient.PasswordRestored(customerId);
		}

		[Activation]
		private void PayEarly() {
			int customerId;
			decimal amount;
			if (cmdLineArgs.Length != 4 || !int.TryParse(cmdLineArgs[1], out customerId) || !decimal.TryParse(cmdLineArgs[2], out amount)) {
				log.Msg("Usage: PayEarly <CustomerId> <amount> <loanRefNumber>");
				return;
			}

			serviceClient.PayEarly(customerId, amount, cmdLineArgs[3]);
		}

		[Activation]
		private void PayPointAddedByUnderwriter() {
			int customerId, underwriterId;
			if (cmdLineArgs.Length != 5 || !int.TryParse(cmdLineArgs[1], out customerId) || !int.TryParse(cmdLineArgs[4], out underwriterId)) {
				log.Msg("Usage: PayPointAddedByUnderwriter <CustomerId> <cardno> <underwriterName> <underwriterId>");
				return;
			}

			serviceClient.PayPointAddedByUnderwriter(customerId, cmdLineArgs[2], cmdLineArgs[3], underwriterId);
		}

		[Activation]
		private void PayPointCharger() {
			if (cmdLineArgs.Length != 1) {
				log.Msg("Usage: PayPointCharger");
				return;
			}

			serviceClient.PayPointCharger();
		}

		[Activation]
		private void PayPointNameValidationFailed() {
			int underwriterId;
			int customerId;
			if (cmdLineArgs.Length != 4 || !int.TryParse(cmdLineArgs[1], out underwriterId) || !int.TryParse(cmdLineArgs[2], out customerId)) {
				log.Msg("Usage: PayPointNameValidationFailed <Underwriter ID> <CustomerId> <cardHodlerName>");
				return;
			}

			serviceClient.PayPointNameValidationFailed(underwriterId, customerId, cmdLineArgs[2]);
		}

		[Activation]
		private void QuickOffer() {
			int customerId;
			bool bSaveOfferToDB;

			if (cmdLineArgs.Length != 3 || !int.TryParse(cmdLineArgs[1], out customerId) || !bool.TryParse(cmdLineArgs[2], out bSaveOfferToDB)) {
				log.Msg("Usage: QuickOffer <CustomerId> <Save offer to DB>");
				return;
			}

			serviceClient.QuickOffer(customerId, bSaveOfferToDB);
		} // QuickOffer

		[Activation]
		private void QuickOfferWithPrerequisites() {
			int customerId;
			bool bSaveOfferToDB;

			if (cmdLineArgs.Length != 3 || !int.TryParse(cmdLineArgs[1], out customerId) || !bool.TryParse(cmdLineArgs[2], out bSaveOfferToDB)) {
				log.Msg("Usage: QuickOfferWithPrerequisites <CustomerId> <Save offer to DB>");
				return;
			}

			serviceClient.QuickOfferWithPrerequisites(customerId, bSaveOfferToDB);
		} // QuickOffer

	    [Activation]
		private void RescheduleLoan() {
	        int loanId;
	        int userId;
	        int customerId;
            if (this.cmdLineArgs.Length != 4 || !int.TryParse(this.cmdLineArgs[1], out loanId) || !int.TryParse(this.cmdLineArgs[2], out customerId) || !int.TryParse(this.cmdLineArgs[3], out userId))
	        {
				log.Msg("Usage: RescheduleLoan <Loan Id> <Customer Id> <User Id>");
	            return;
	        }
  
		    Console.WriteLine("UserID {0}, CustomerID {1}, LoanID {2}", userId, customerId, loanId);

            ReschedulingArgument reModel = new ReschedulingArgument();
            reModel.LoanType = new Loan().GetType().AssemblyQualifiedName;
            reModel.LoanID = loanId;
            reModel.SaveToDB = false;
            reModel.ReschedulingDate = DateTime.UtcNow;
            reModel.ReschedulingRepaymentIntervalType = DbConstants.RepaymentIntervalTypes.Month;
		    reModel.RescheduleIn = false; // true;
		    reModel.PaymentPerInterval = 300;
	
			var res= this.serviceClient.RescheduleLoan(userId, customerId, reModel);
			log.Msg(res.Value);
	    }
        
        [Activation]
		private void RejectUser() {
			int underwriterId;
			int customerId;
			bool bSendToCustomer;

			if (cmdLineArgs.Length != 4 || !int.TryParse(cmdLineArgs[1], out underwriterId) || !int.TryParse(cmdLineArgs[2], out customerId) || !bool.TryParse(cmdLineArgs[3], out bSendToCustomer)) {
				log.Msg("Usage: RejectUser <Underwriter ID> <CustomerId> <send to customer>");
				return;
			}

			serviceClient.RejectUser(underwriterId, customerId, bSendToCustomer);
		}

		[Activation]
		private void RenewEbayToken() {
			int customerId;
			if (cmdLineArgs.Length != 4 || !int.TryParse(cmdLineArgs[1], out customerId)) {
				log.Msg("Usage: RenewEbayToken <CustomerId> <marketplaceName> <eBayAddress>");
				return;
			}

			serviceClient.RenewEbayToken(1, customerId, cmdLineArgs[2], cmdLineArgs[3]);
		}

		[Activation]
		private void RequestCashWithoutTakenLoan() {
			int customerId;
			if (cmdLineArgs.Length != 2 || !int.TryParse(cmdLineArgs[1], out customerId)) {
				log.Msg("Usage: RequestCashWithoutTakenLoan <CustomerId>");
				return;
			}

			serviceClient.RequestCashWithoutTakenLoan(customerId);
		}

		[Activation]
		private void SetLateLoanStatus() {
			if (cmdLineArgs.Length != 1) {
				log.Msg("Usage: SetLateLoanStatus");
				return;
			}

			serviceClient.SetLateLoanStatus();
		}

		[Activation]
		private void Shutdown() {
			ActionMetaData res = adminClient.Shutdown();
			log.Msg("Shutdown request result: status = {0}, comment = '{1}'", res.Status, res.Comment);
		}

		[Activation]
		private void StressTest() {
			int nLength;
			int nCount;

			var oUsage = new Action(() => {
				log.Msg("Usage: StressTest <count> <length>");
				log.Msg("Where");
				log.Msg("\tcount - how many requests to execute");
				log.Msg("\tlength - length in seconds of each request (1..100 seconds).");
			});

			if (cmdLineArgs.Length != 3 || !int.TryParse(cmdLineArgs[1], out nCount) || !int.TryParse(cmdLineArgs[2], out nLength)) {
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

			log.Debug("Stress test with {0} requests of {1} seconds each...", nCount, nLength);

			for (int i = 0; i < nCount; i++)
				adminClient.StressTestAction(nLength, i.ToString(CultureInfo.InvariantCulture));

			log.Debug("Stress test with {0} requests of {1} seconds each is complete.", nCount, nLength);
		}

		[Activation]
		private void StressTestSync() {
			int nLength;

			var oUsage = new Action(() => {
				log.Msg("Usage: StressTestSync <count> <message>");
				log.Msg("Where");
				log.Msg("\tcount - how many requests to execute");
			});

			if (cmdLineArgs.Length != 3 || !int.TryParse(cmdLineArgs[1], out nLength)) {
				oUsage();
				return;
			} // if

			string sMsg = cmdLineArgs[2];

			if ((nLength < 1) || (nLength > 100)) {
				oUsage();
				return;
			} // if

			adminClient.StressTestSync(nLength, sMsg);
		}

		// GeneratePassword
		// BackfillCustomerAnalyticsCompany
		[Activation]
		private void Temp_BackFillMedals() {
			if ((cmdLineArgs.Length != 1)) {
				log.Msg("Usage: Temp_BackFillMedals");
				return;
			} // if

			serviceClient.Temp_BackFillMedals();
		}

		[Activation]
		private void TerminateAction() {
			if (cmdLineArgs.Length != 2) {
				log.Msg("Usage: TerminateAction <action guid>");

				log.Msg(@"
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

			adminClient.Terminate(new Guid(cmdLineArgs[1]));
		}

		[Activation]
		private void TransferCashFailed() {
			int customerId;
			if (cmdLineArgs.Length != 2 || !int.TryParse(cmdLineArgs[1], out customerId)) {
				log.Msg("Usage: TransferCashFailed <CustomerId>");
				return;
			}

			serviceClient.TransferCashFailed(customerId);
		}

		[Activation]
		private void UpdateCurrencyRates() {
			if (cmdLineArgs.Length != 1) {
				log.Msg("Usage: UpdateCurrencyRates");
				return;
			}

			serviceClient.UpdateCurrencyRates();
		}

		[Activation]
		private void UpdateGoogleAnalytics() {
			if (cmdLineArgs.Length < 2) {
				serviceClient.UpdateGoogleAnalytics(null, null);
				return;
			} // if

			DateTime oFrom;
			DateTime oTo;

			if (
				(cmdLineArgs.Length != 3) ||
					!DateTime.TryParse(cmdLineArgs[1], out oFrom) ||
					!DateTime.TryParse(cmdLineArgs[2], out oTo)
				) {
				log.Msg("Usage: UpdateGoogleAnalytics [<backfill from date> <backfill to date>]");
				return;
			} // if

			serviceClient.UpdateGoogleAnalytics(oFrom, oTo);
		}

		[Activation]
		private void UpdateLinkedHmrcPassword() {
			int nCustomerID;

			if ((cmdLineArgs.Length != 4) || !int.TryParse(cmdLineArgs[1], out nCustomerID)) {
				log.Msg("Usage: UpdateLinkedHmrcPassword <Customer ID> <Display name> <Password>");
				return;
			} // if

			string sDisplayName = cmdLineArgs[2];
			string sPassword = cmdLineArgs[3];
			string sHash = SecurityUtils.Hash(nCustomerID + sPassword + sDisplayName);

			serviceClient.UpdateLinkedHmrcPassword(
				new Encrypted(nCustomerID.ToString(CultureInfo.InvariantCulture)),
				new Encrypted(sDisplayName),
				new Encrypted(sPassword), sHash
				);
		}

		[Activation]
		private void UpdateTransactionStatus() {
			if (cmdLineArgs.Length != 1) {
				log.Msg("Usage: UpdateTransactionStatus");
				return;
			}

			serviceClient.UpdateTransactionStatus();
		}

		[Activation]
		private void VerifyApproval() {
			var i = new VerificationInput("VerifyApproval", cmdLineArgs, log);

			i.Init();

			if (i.IsGood)
				serviceClient.VerifyApproval(i.CustomerCount, i.LastCheckedCustomerID);
		}

		[Activation]
		private void VerifyReapproval() {
			var i = new VerificationInput("VerifyReapproval", cmdLineArgs, log);

			i.Init();

			if (i.IsGood)
				serviceClient.VerifyReapproval(i.CustomerCount, i.LastCheckedCustomerID);
		}

		[Activation]
		private void VerifyReject() {
			var i = new VerificationInput("VerifyReject", cmdLineArgs, log);

			i.Init();

			if (i.IsGood)
				serviceClient.VerifyReject(i.CustomerCount, i.LastCheckedCustomerID);
		}

		[Activation]
		private void VerifyRerejection() {
			var i = new VerificationInput("VerifyRerejection", cmdLineArgs, log);

			i.Init();

			if (i.IsGood)
				serviceClient.VerifyRerejection(i.CustomerCount, i.LastCheckedCustomerID);
		}

		[Activation]
		private void MaamMedalAndPricing() {
			var i = new VerificationInput("MaamMedalAndPricing", cmdLineArgs, log);

			i.Init();

			if (i.IsGood)
				serviceClient.MaamMedalAndPricing(i.CustomerCount, i.LastCheckedCustomerID);
		} // MaamMedalAndPricing

		[Activation]
		private void VerifyMedal() {
			var i = new MedalVerificationInput("VerifyMedal", cmdLineArgs, log);

			i.Init();

			this.log.Debug("Arguments: {0}", i);

			if (i.IsGood)
				serviceClient.VerifyMedal(i.CustomerCount, i.LastCheckedCustomerID, i.IncludeTest, i.CalculationTime);
		} // VerifyMedal

		[Activation]
		private void WriteToLog() {
			if (cmdLineArgs.Length < 3) {
				log.Msg("Usage: WriteToLog <severity> <arg1> <arg2> ... <argN>");
				log.Msg("All the args are merged into a message which is written to service log with requested severity.");
				return;
			} // if

			adminClient.WriteToLog(cmdLineArgs[1], string.Join(" ", cmdLineArgs.Skip(2)));
		}

		[Activation]
		private void XDaysDue() {
			if (cmdLineArgs.Length != 1) {
				log.Msg("Usage: XDaysDue");
				return;
			}

			serviceClient.XDaysDue();
		}

		[Activation]
		private void TotalMaamMedalAndPricing() {
			bool testMode = false;

			if (cmdLineArgs.Length > 1)
				testMode = cmdLineArgs[1].Equals("test", StringComparison.CurrentCultureIgnoreCase);

			serviceClient.TotalMaamMedalAndPricing(testMode);
		} // TotalMaamMedalAndPricing

		/*
		[Activation]
		private void RequalifyCustomer() {
			string email;
			if (cmdLineArgs.Length != 2 ) {
				log.Msg("Usage: RequalifyCustomer <CustomerEmail>");
				return;
			}
			email = cmdLineArgs[1];
			var result = serviceClient.RequalifyCustomer(email);
			log.Debug("blablabla: {0}", result.ToString());
		}
		*/

		[Activation]
		private void CustomerAvaliableCredit() { // CustomerAvaliableCredit 18234 12345 
			int customerID;
			long aliMemberID;

			if ((cmdLineArgs.Length != 3) || !int.TryParse(cmdLineArgs[1], out customerID) || !long.TryParse(cmdLineArgs[2], out aliMemberID)) {
				log.Msg("Usage: CustomerAvaliableCredit <Customer ID> <Alibaba MemberID>");
				return;
			}

			log.Debug("activator: customerID: {0}, aliMemberID: {1}", customerID, aliMemberID);

			//AlibabaAvailableCreditActionResult result = serviceClient.CustomerAvaliableCredit(customerID, aliMemberID);
			//this.log.Debug("blablabla: {0}",  JsonConvert.SerializeObject(result)); //json
		}

		[Activation]
		private void DataSharing() { // DataSharing 18241
			int customerID;

			if ((cmdLineArgs.Length != 2) || !int.TryParse(cmdLineArgs[1], out customerID)) {
				log.Msg("Usage: DataSharing <Customer ID>");
				return;
			}

			log.Debug("activator: customerID: {0}", customerID);

			// ActionMetaData result =
				serviceClient.DataSharing(customerID, AlibabaBusinessType.APPLICATION_REVIEW, null );
			//this.log.Debug("result: {0}", JsonConvert.SerializeObject(result.Result)); //json
		}

        [Activation]
        private void BrokerTransferCommission()
        {
            ActionMetaData result = this.serviceClient.BrokerTransferCommission();
            this.log.Debug("{0}", result.Status.ToString());
        }

		//[Activation]
		//private void ExampleMethod() {
		//	int customerID;

		//	if (cmdLineArgs.Length != 2 || !int.TryParse(cmdLineArgs[1], out customerID)) {
		//		log.Msg("Usage: ExampleMethod <customer ID>");
		//		return;
		//	} // if

		//	DateTime now = DateTime.UtcNow;

		//	DateTimeActionResult result = serviceClient.ExampleMethod(1, customerID);

		//	log.Info(
		//		"\nTime before the call: {0}\nCustomer          ID: {1}\nReturned       value: {2}\n",
		//		now.ToString("MMM d yyyy H:mm:ss"),
		//		customerID,
		//		result.Value.ToString("MMM d yyyy H:mm:ss")
		//	);
		//}

		private readonly EzServiceAdminClient adminClient;
		private readonly string[] cmdLineArgs;
		private readonly ASafeLog log;
		private readonly EzServiceClient serviceClient;
		private SortedDictionary<string, MethodInfo> methodList;
	} // class ServiceClientActivation
} // namespace
