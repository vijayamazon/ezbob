namespace StrategiesActivator {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.Linq;
	using System.Reflection;
	using System.ServiceModel;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Models.NewLoan;
	using Ezbob.Database;
	using Ezbob.Database.Pool;
	using Ezbob.Logger;
	using Ezbob.Utils.Security;
	using EzServiceConfigurationLoader;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using log4net;
	using Newtonsoft.Json;
	using ServiceClientProxy.EzServiceReference;

	[SuppressMessage("ReSharper", "UnusedMember.Local")]
	public class ServiceClientActivation {
		public ServiceClientActivation(string[] args) {
			this.log = new ConsoleLog(new SafeILog(LogManager.GetLogger(typeof(ServiceClientActivation))));
			this.log.NotifyStart();

			InitMethods();

			string sInstanceName = args[0].ToLower();

			if (sInstanceName.ToLower() == "list") {
				ListSupported();
				this.log.NotifyStop();
				throw new ExitException();
			} // if

			var env = new Ezbob.Context.Environment(this.log);
			this.DB = new SqlConnection(env, this.log);

			CurrentValues.Init(this.DB, this.log);
			DbConnectionPool.ReuseCount = CurrentValues.Instance.ConnectionPoolReuseCount;
			AConnection.UpdateConnectionPoolMaxSize(CurrentValues.Instance.ConnectionPoolMaxSize);

			Configuration cfg;

			if (this.methodList.ContainsKey(sInstanceName)) {
				cfg = new DefaultConfiguration(System.Environment.MachineName, this.DB, this.log);
				this.cmdLineArgs = args;
			} else {
				this.cmdLineArgs = new string[args.Length - 1];
				Array.Copy(args, 1, this.cmdLineArgs, 0, args.Length - 1);

				cfg = new Configuration(sInstanceName, this.DB, this.log);
			} // if

			cfg.Init();

			this.log.Info("Running against instance {0} - {1}.", cfg.InstanceID, cfg.InstanceName);
			this.log.Info("Arguments:\n\t{0}", string.Join("\n\t", this.cmdLineArgs));

			var oTcpBinding = new NetTcpBinding();

			this.serviceClient = new EzServiceClient(
				oTcpBinding,
				new EndpointAddress(cfg.AdminEndpointAddress)
				);

			this.adminClient = new EzServiceAdminClient(
				oTcpBinding,
				new EndpointAddress(cfg.AdminEndpointAddress)
				);
		}// constructor

		protected readonly SqlConnection DB;



		public void Execute() {
			string strategyName = this.cmdLineArgs[0];

			string sKey = strategyName.ToLower();

			if (this.methodList.ContainsKey(sKey))
				this.methodList[sKey].Invoke(this, new object[] { });
			else {
				this.log.Msg("Strategy {0} is not supported", strategyName);
				ListSupported();
			} // if

			this.log.NotifyStop();
		} // Execute

		[Activation]
		private void AmlChecker() {
			int customerId;

			if (this.cmdLineArgs.Length == 2 && int.TryParse(this.cmdLineArgs[1], out customerId)) {
				this.serviceClient.CheckAml(customerId, 1);
				return;
			}

			if (this.cmdLineArgs.Length == 9 && int.TryParse(this.cmdLineArgs[1], out customerId)) {
				this.serviceClient.CheckAmlCustom(1, customerId, this.cmdLineArgs[2], this.cmdLineArgs[3], this.cmdLineArgs[4], this.cmdLineArgs[5], this.cmdLineArgs[6], this.cmdLineArgs[7], this.cmdLineArgs[8]);
				return;
			}

			this.log.Msg("Usage: AmlChecker <CustomerId>");
			this.log.Msg("OR");
			this.log.Msg("Usage: AmlChecker <CustomerId> <idhubHouseNumber> <idhubHouseName> <idhubStreet> <idhubDistrict> <idhubTown> <idhubCounty> <idhubPostCode>");
		}

		[Activation]
		private void AndRecalculateVatReturnSummaryForAll() {
			this.serviceClient.AndRecalculateVatReturnSummaryForAll();
		}

		[Activation]
		private void ApprovedUser() {
			int underwriterId;
			int customerId;
			decimal loanAmount;
			int validFor;
			bool isFirst;
			if (this.cmdLineArgs.Length != 6 || !int.TryParse(this.cmdLineArgs[1], out underwriterId) || !int.TryParse(this.cmdLineArgs[2], out customerId) || !decimal.TryParse(this.cmdLineArgs[3], out loanAmount) || !int.TryParse(this.cmdLineArgs[4], out validFor) || bool.TryParse(this.cmdLineArgs[5], out isFirst)) {
				this.log.Msg("Usage: ApprovedUser <Underwriter ID> <CustomerId> <loanAmount> <ValidFor> <isFirst>");
				return;
			}

			this.serviceClient.ApprovedUser(underwriterId, customerId, loanAmount, validFor, isFirst);
		}

		[Activation]
		private void BackfillAml() {
			if ((this.cmdLineArgs.Length != 1)) {
				this.log.Msg("Usage: BackfillAml");
				return;
			} // if

			this.serviceClient.BackfillAml();
		}

		[Activation]
		private void BackfillLinkedHmrc() {
			this.serviceClient.BackfillLinkedHmrc();
		} // BackfillLinkedHmrc

		[Activation]
		private void BackfillExperianConsumer() {
			this.serviceClient.BackfillExperianConsumer();
		}

		[Activation]
		private void BackFillExperianNonLtdScoreText() {
			this.serviceClient.BackFillExperianNonLtdScoreText();
		}

		[Activation]
		private void BackfillExperianDirectors() {
			if (this.cmdLineArgs.Length == 1) {
				this.serviceClient.BackfillExperianDirectors(null);
				return;
			} // if

			int nCustomerID;

			if ((this.cmdLineArgs.Length != 2) || !int.TryParse(this.cmdLineArgs[1], out nCustomerID)) {
				this.log.Msg("Usage: BackfillExperianDirectors [<Customer ID>]");
				return;
			} // if

			this.serviceClient.BackfillExperianDirectors(nCustomerID);
		}

		[Activation]
		private void BackfillExperianLtd() {
			this.serviceClient.BackfillExperianLtd();
		}

        [Activation]
        private void BackfillExperianLtdScoreText()
        {
	        this.serviceClient.BackfillExperianLtdScoreText();
        }

		[Activation]
		private void BackfillHmrcBusinessRelevance() {
			this.serviceClient.BackfillHmrcBusinessRelevance();
		}

		[Activation]
		private void BackfillLandRegistry2PropertyLink() {
			if ((this.cmdLineArgs.Length != 1)) {
				this.log.Msg("Usage: BackfillLandRegistry2PropertyLink");
				return;
			} // if

			this.serviceClient.BackfillLandRegistry2PropertyLink();
		}

		[Activation]
		private void BackfillNonLimitedCompanies() {
			if ((this.cmdLineArgs.Length != 1)) {
				this.log.Msg("Usage: BackfillNonLimitedCompanies");
				return;
			} // if

			this.serviceClient.BackfillNonLimitedCompanies();
		}

		[Activation]
		private void BackfillTurnover() {
			this.serviceClient.BackfillTurnover();
		}

		[Activation]
		private void BackfillZooplaValue() {
			if ((this.cmdLineArgs.Length != 1)) {
				this.log.Msg("Usage: BackfillZooplaValue");
				return;
			} // if

			this.serviceClient.BackfillZooplaValue();
		}

		[Activation]
		private void BackfillPostcodeNuts() {
			if ((this.cmdLineArgs.Length != 1)) {
				this.log.Msg("Usage: BackfillPostcodeNuts");
				return;
			} // if

			this.DB.ForEachRow((sr, bRowsetStart) => {
				this.serviceClient.PostcodeNuts(1, sr["Postcode"].ToString());
				return Ezbob.Database.ActionResult.Continue;
			}, "SELECT DISTINCT Postcode FROM CustomerAddress WHERE Postcode IS NOT NULL AND Postcode <> ''", CommandSpecies.Text);
			
		}

		[Activation]
		private void BrokerApproveAndResetCustomerPassword() {
			int nUnderwriterID;
			int nCustomerID;
			decimal nLoanAmount;
			int nValidHours;
			bool isFirst;

			if (this.cmdLineArgs.Length != 6 ||
					!int.TryParse(this.cmdLineArgs[1], out nUnderwriterID) ||
					!int.TryParse(this.cmdLineArgs[2], out nCustomerID) ||
					!decimal.TryParse(this.cmdLineArgs[3], out nLoanAmount) ||
					!int.TryParse(this.cmdLineArgs[4], out nValidHours) ||
					!bool.TryParse(this.cmdLineArgs[5], out isFirst)
				) {
				this.log.Msg("Usage: BrokerApproveAndResetCustomerPassword <underwriter id> <customer id> <loan amount> <valid hours> <is first approval>");
				return;
			} // if

			this.serviceClient.BrokerApproveAndResetCustomerPassword(nUnderwriterID, nCustomerID, nLoanAmount, nValidHours, isFirst);
		}

		[Activation]
		private void BrokerLoadCustomerList() {
			if (this.cmdLineArgs.Length != 3) {
				this.log.Msg("Usage: BrokerLoadCustomerList <Contact person email> <broker origin>");
				return;
			} // if

			CustomerOriginEnum origin;

			if (!Enum.TryParse(this.cmdLineArgs[2], true, out origin)) {
				this.log.Msg("Usage: BrokerLoadCustomerList <Contact person email> <broker origin>");
				return;
			} // if

			BrokerCustomersActionResult res = this.serviceClient.BrokerLoadCustomerList(this.cmdLineArgs[1], origin);

			foreach (var oEntry in res.Customers)
				this.log.Msg("Customer ID: {0} Name: {1} {2}", oEntry.CustomerID, oEntry.FirstName, oEntry.LastName);
		}

		[Activation]
		private void BwaChecker() {
			int customerId;

			if (this.cmdLineArgs.Length == 2 && int.TryParse(this.cmdLineArgs[1], out customerId)) {
				this.serviceClient.CheckBwa(customerId, 1);
				return;
			}

			if (this.cmdLineArgs.Length == 11 && int.TryParse(this.cmdLineArgs[1], out customerId)) {
				this.serviceClient.CheckBwaCustom(1, customerId, this.cmdLineArgs[2], this.cmdLineArgs[3], this.cmdLineArgs[4], this.cmdLineArgs[5], this.cmdLineArgs[6], this.cmdLineArgs[7], this.cmdLineArgs[8], this.cmdLineArgs[9], this.cmdLineArgs[10]);
				return;
			}

			this.log.Msg("Usage: BwaChecker <CustomerId>");
			this.log.Msg("OR");
			this.log.Msg("Usage: BwaChecker <CustomerId> <idhubHouseNumber> <idhubHouseName> <idhubStreet> <idhubDistrict> <idhubTown> <idhubCounty> <idhubPostCode> <idhubBranchCode> <idhubAccountNumber>");
		}

		[Activation]
		private void CaisGenerate() {
			int underwriterId;
			if (this.cmdLineArgs.Length != 2 || !int.TryParse(this.cmdLineArgs[1], out underwriterId)) {
				this.log.Msg("Usage: CaisGenerate <underwriterId>");
				return;
			}

			this.serviceClient.CaisGenerate(underwriterId);
		}

		[Activation]
		private void CaisUpdate() {
			int underwriterId;
			int caisId;
			if (this.cmdLineArgs.Length != 3 || !int.TryParse(this.cmdLineArgs[1], out underwriterId) || !int.TryParse(this.cmdLineArgs[2], out caisId)) {
				this.log.Msg("Usage: CaisUpdate <Underwriter ID> <caisId>");
				return;
			}

			this.serviceClient.CaisUpdate(underwriterId, caisId);
		}

		[Activation]
		private void CalculateVatReturnSummary() {
			int nCustomerMarketplaceID;

			if ((this.cmdLineArgs.Length != 2) || !int.TryParse(this.cmdLineArgs[1], out nCustomerMarketplaceID)) {
				this.log.Msg("Usage: CalculateVatReturnSummary <Customer Marketplace ID>");
				return;
			} // if

			this.serviceClient.CalculateVatReturnSummary(nCustomerMarketplaceID);
		}

		[Activation]
		private void CashTransferred() {
			int customerId;
			decimal amount;
			bool isFirst;
			if (this.cmdLineArgs.Length != 5 || !int.TryParse(this.cmdLineArgs[1], out customerId) || !decimal.TryParse(this.cmdLineArgs[2], out amount) || !bool.TryParse(this.cmdLineArgs[3], out isFirst)) {
				this.log.Msg("Usage: CashTransferred <CustomerId> <amount> <loanRefNum>");
				return;
			}
			string loanRefNum = this.cmdLineArgs[3];
			this.serviceClient.CashTransferred(customerId, amount, loanRefNum, isFirst);
		}

		[Activation]
		private void CreateUnderwriter() {
			if (this.cmdLineArgs.Length != 4) {
				this.log.Msg("Usage: CreateUnderwriter <Name> <Password> <RoleName>");
				this.log.Msg(@"Available roles (underwriter is usually one of the last three):
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

			this.serviceClient.SignupUnderwriterMultiOrigin(
				this.cmdLineArgs[1],
				new DasKennwort(this.cmdLineArgs[2]),
				this.cmdLineArgs[3]
			);
		}

		[Activation]
		private void CustomerMarketPlaceAdded() {
			int customerId, marketplaceId;
			bool bUpdateWizardStep;
			if (this.cmdLineArgs.Length != 4 || !int.TryParse(this.cmdLineArgs[1], out customerId) || !int.TryParse(this.cmdLineArgs[2], out marketplaceId) || !bool.TryParse(this.cmdLineArgs[3], out bUpdateWizardStep)) {
				this.log.Msg("Usage: CustomerMarketPlaceAdded <CustomerId> <CustomerMarketplaceId> <Update customer wizard step>");
				this.log.Msg("<Update customer wizard step>: is boolean and means whether to set customer wizard step to Marketplace or not. If wizard step is 'TheLastOne' it is never changed.");
				return;
			}

			this.serviceClient.UpdateMarketplace(customerId, marketplaceId, bUpdateWizardStep, 1);
		}

		[Activation]
		private void Decrypt() {
			if (this.cmdLineArgs.Length < 2) {
				this.log.Msg("Usage: Decrypt <what to decrypt>");
				this.log.Msg("Concatenates all the passed arguments, separating them with a space, and decrypts the result.");
				return;
			} // if

			var sInput = string.Join(" ", this.cmdLineArgs, 1, this.cmdLineArgs.Length - 1);
			string sOutput;

			try {
				sOutput = Encrypted.Decrypt(sInput);
			} catch (Exception e) {
				this.log.Warn(e, "Failed to decrypt.");
				sOutput = string.Empty;
			} // try

			this.log.Msg("\n\nInput: {0}\nOutput: {1}\n", sInput, sOutput);
		}

		[Activation]
		private void DisplayMarketplaceSecurityData() {
			int nCustomerID;

			if ((this.cmdLineArgs.Length != 2) || !int.TryParse(this.cmdLineArgs[1], out nCustomerID)) {
				this.log.Msg("Usage: DisplayMarketplaceSecurityData <Customer ID>");
				return;
			} // if

			this.serviceClient.DisplayMarketplaceSecurityData(nCustomerID);
		}

		[Activation]
		private void EmailRolloverAdded() {
			int customerId;
			decimal amount;
			if (this.cmdLineArgs.Length != 3 || !int.TryParse(this.cmdLineArgs[1], out customerId) || !decimal.TryParse(this.cmdLineArgs[2], out amount)) {
				this.log.Msg("Usage: EmailRolloverAdded <CustomerId> <amount>");
				return;
			}

			this.serviceClient.EmailRolloverAdded(1, customerId, amount);
		}

		[Activation]
		private void Encrypt() {
			if (this.cmdLineArgs.Length < 2) {
				this.log.Msg("Usage: Encrypt <what to encrypt>");
				this.log.Msg("Concatenates all the passed arguments, separating them with a space, and encrypts the result.");
				return;
			} // if

			var sInput = string.Join(" ", this.cmdLineArgs, 1, this.cmdLineArgs.Length - 1);
			var oOutput = new Encrypted(sInput);

			this.log.Msg("\n\nInput: {0}\nOutput: {1}\n", sInput, oOutput);
		}

		[Activation]
		private void EncryptChannelGrabberMarketplaces() {
			this.serviceClient.EncryptChannelGrabberMarketplaces();
		}

		[Activation]
		private void Escalated() {
			int customerId;
			if (this.cmdLineArgs.Length != 2 || !int.TryParse(this.cmdLineArgs[1], out customerId)) {
				this.log.Msg("Usage: Escalated <CustomerId>");
				return;
			}

			this.serviceClient.Escalated(customerId, 1);
		}

		[Activation]
		private void EsignProcessPending() {
			if (this.cmdLineArgs.Length == 1) {
				this.serviceClient.EsignProcessPending(null);
				return;
			} // if

			int nCustomerID;

			if ((this.cmdLineArgs.Length != 2) || !int.TryParse(this.cmdLineArgs[1], out nCustomerID)) {
				this.log.Msg("Usage: EsignProcessPending [<Customer ID>]");
				return;
			} // if

			this.serviceClient.EsignProcessPending(nCustomerID);
		}

		[Activation]
		private void ExperianCompanyCheck() {
			int customerId;
			bool forceCheck;
			if (this.cmdLineArgs.Length != 3 || !int.TryParse(this.cmdLineArgs[1], out customerId) || !bool.TryParse(this.cmdLineArgs[2], out forceCheck)) {
				this.log.Msg("Usage: ExperianCompanyCheck <CustomerId> <ForceCheck>");
				return;
			}

			this.serviceClient.ExperianCompanyCheck(1, customerId, forceCheck);
		}

		[Activation]
		private void ExperianConsumerCheck() {
			int customerId, directorId;
			bool forceCheck;

			if (this.cmdLineArgs.Length != 4 || !int.TryParse(this.cmdLineArgs[1], out customerId) || !int.TryParse(this.cmdLineArgs[2], out directorId) || !bool.TryParse(this.cmdLineArgs[3], out forceCheck)) {
				this.log.Msg("Usage: ExperianConsumerCheck <CustomerId> <DirectorId> <ForceCheck>");
				return;
			}

			this.serviceClient.ExperianConsumerCheck(1, customerId, directorId, forceCheck);
		}

		[Activation]
		private void FinishWizard() {
			int customerId, underwriterId;
			if (this.cmdLineArgs.Length < 3 || !int.TryParse(this.cmdLineArgs[1], out customerId) || !int.TryParse(this.cmdLineArgs[2], out underwriterId)) {
				this.log.Msg("Usage: FinishWizard <CustomerId> <UnderwriterId> [<DoSendEmail(true/false)>] [<AvoidAutoDecision(0/1)>]");
				return;
			}

			var oArgs = JsonConvert.DeserializeObject<FinishWizardArgs>(CurrentValues.Instance.FinishWizardForApproved);

			oArgs.CustomerID = customerId;

			if (this.cmdLineArgs.Length >= 4)
				oArgs.DoSendEmail = bool.Parse(this.cmdLineArgs[3]);

			if (this.cmdLineArgs.Length >= 5)
				oArgs.AvoidAutoDecision = int.Parse(this.cmdLineArgs[4]);

			this.serviceClient.FinishWizard(oArgs, underwriterId);
		}

		[Activation]
		private void FirstOfMonthStatusNotifier() {
			if (this.cmdLineArgs.Length != 1) {
				this.log.Msg("Usage: FirstOfMonthStatusNotifier");
				return;
			}

			this.serviceClient.FirstOfMonthStatusNotifier();
		}

		[Activation]
		private void FraudChecker() {
			int customerId;
			FraudMode mode;

			if (this.cmdLineArgs.Length != 3 || !int.TryParse(this.cmdLineArgs[1], out customerId) || !Enum.TryParse(this.cmdLineArgs[2], true, out mode)) {
				this.log.Msg("Usage: FraudChecker <CustomerId> <Fraud mode>");
				this.log.Msg("Fraud mode values: {0}", string.Join(", ", Enum.GetValues(typeof(FraudMode))));
				return;
			}

			this.serviceClient.FraudChecker(customerId, mode);
		}

		[Activation]
		private void GenerateMobileCode() {
			if (this.cmdLineArgs.Length != 2) {
				this.log.Msg("Usage: GenerateMobileCode <phone number>");
				return;
			}

			this.serviceClient.GenerateMobileCode(this.cmdLineArgs[1]);
		}

		[Activation]
		private void GeneratePassword() {
			if (this.cmdLineArgs.Length != 3) {
				this.log.Msg(@"Usage: GeneratePassword <user name> <new password>

Generates new password hash from given user name and password.
Neither user name nor password can contain a white space character!

I.e. to generate hash for underwriter 'manager' with password '123456' call
GeneratePassword manager 123456

");
				return;
			} // if

			string userName = this.cmdLineArgs[1];
			string rawPassword = this.cmdLineArgs[2];

			var pu = new PasswordUtility(CurrentValues.Instance.PasswordHashCycleCount);

			var pass = pu.Generate(userName, rawPassword);

			this.log.Msg("\n\n" +
				"Raw user name:password:\n" +
				"\t{0}:{1}\n\n" +
				"generated hash:\n" +
				"\t{2}\n\n" +
				"generated salt:\n" +
				"\t{3}\n\n" +
				"generated cycle count:\n" +
				"\t{4}\n\n" +
				"query:\n" +
				"\tUPDATE Security_User SET\n" +
					"\t\tEzPassword = '{2}',\n" +
					"\t\tSalt = '{3}',\n" +
					"\t\tCycleCount = '{4}'\n" +
				"\tWHERE\n" +
					"\t\tUserName = '{0}'\n",
				userName,
				rawPassword,
				pass.Password,
				pass.Salt,
				pass.CycleCount
			);
		} // GeneratePassword

		[Activation]
		private void GenerateUwPasswords() {
			var uwList = new List<string> {
				"se", "vitasd", "tomerg", "sharonep", "farleyk", "alexbo", "rosb", "shirik", "stasd", "ezbobpartners",
				"travism", "galitg", "clareh", "yarons", "sivanc", "assafb", "sarahd", "gadif", "songulo", "bateli",
				"everline", "darrenh", "russellb", "lirang", "andrea", "sashaf", "mishas", "elinar", "shlomim", "masha",
				"russell", "scotth", "sailishr", "hanryc", "marius", "stuartd", "tanyag", "nataliem", "jamiem", "guest",
				"lauren", "hagayj", "dora", "romanp", "inie", "jackiew", "inas", "sofiad", "normanc", "igaell", "louip",
				"amiyc",
			};

			int maxLen = uwList.Select(s => s.Length).Max();

			var output = new List<string>();

			var pu = new PasswordUtility(CurrentValues.Instance.PasswordHashCycleCount);

			foreach (string userName in uwList) {
				var pwd = new DasKennwort();
				pwd.GenerateSimplePassword(16);

				var pass = pu.Generate(userName, pwd.Data);

				output.Add(string.Format(
					"User name: '{0}' Password: '{1}' Query: " +
					"UPDATE Security_User SET Password = NULL, " +
						"EzPassword = '{2}', " +
						"Salt = '{3}', " +
						"CycleCount = '{4}' " +
					"WHERE " +
						"UserName = '{5}'",
					userName.PadRight(maxLen),
					pwd.Data,
					pass.Password,
					pass.Salt,
					pass.CycleCount,
					userName
				));
			} // for each user name

			this.log.Debug("\n\n{0}\n\n", string.Join("\n", output));
		} // GenerateUwPasswords

		[Activation]
		private void GetCashFailed() {
			int customerId;
			if (this.cmdLineArgs.Length != 2 || !int.TryParse(this.cmdLineArgs[1], out customerId)) {
				this.log.Msg("Usage: GetCashFailed <CustomerId>");
				return;
			}

			this.serviceClient.GetCashFailed(customerId);
		}

		[Activation]
		private void GetWizardConfigs() {
			this.serviceClient.GetWizardConfigs();
		}

		private void InitMethods() {
			MethodInfo[] aryMethods = GetType()
				.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);

			this.methodList = new SortedDictionary<string, MethodInfo>();

			foreach (MethodInfo mi in aryMethods) {
				IEnumerable<ActivationAttribute> oAttrList = mi.GetCustomAttributes<ActivationAttribute>();

				if (oAttrList.Any())
					this.methodList[mi.Name.ToLower()] = mi;
			} // foreach
		}

		[Activation]
		private void LateBy14Days() {
			if (this.cmdLineArgs.Length != 1) {
				this.log.Msg("Usage: LateBy14Days");
				return;
			}

			this.serviceClient.LateBy14Days();
		}

		[Activation]
		private void ListActiveActions() {
			StringListActionResult res = this.adminClient.ListActiveActions();

			this.log.Msg(
				"\nRetriever (i.e. this action):\n\t{{ {0}: {4} [{1}sync] {2}: {3} }}",
				res.MetaData.ActionID,
				res.MetaData.IsSynchronous ? "" : "a",
				res.MetaData.Status,
				res.MetaData.Comment ?? "-- no comments --",
				res.MetaData.Name
				);

			string sKey = "{ " + res.MetaData.ActionID;

			this.log.Msg("\nList of active actions - begin:\n");

			foreach (string s in res.Records) {
				if (!s.StartsWith(sKey))
					this.log.Msg("\t{0}\n", s);
			}

			this.log.Msg("\nList of active actions - end.");
		}

		private void ListSupported() {
			this.log.Msg("Supported strategies are (case insensitive):\n\t{0}", string.Join("\n\t", this.methodList.Keys));
		} // ListSupported

		// InitMethods

		[Activation]
		private void LoadEsignatures() {
			int zu;
			int? nCustomerID = null;
			bool bReady = false;
			bool bPollStatus = false;

			if (this.cmdLineArgs.Length == 1)
				bReady = true;
			else if ((this.cmdLineArgs.Length == 2) && int.TryParse(this.cmdLineArgs[1], out zu)) {
				nCustomerID = zu;
				bReady = true;
			} else if ((this.cmdLineArgs.Length == 3) && int.TryParse(this.cmdLineArgs[1], out zu) && bool.TryParse(this.cmdLineArgs[2], out bPollStatus)) {
				nCustomerID = zu;
				bReady = true;
			}

			if (!bReady) {
				this.log.Msg("Usage: LoadEsignatures [<Customer ID> [Poll Status (true/false)]]");
				return;
			} // if

			var elar = this.serviceClient.LoadEsignatures(1, nCustomerID, bPollStatus);

			foreach (var e in elar.Data)
				this.log.Msg("{0}", e);
		}

		[Activation]
		private void LoadExperianConsumer() {
			long nServiceLogID;

			if ((this.cmdLineArgs.Length != 2) || !long.TryParse(this.cmdLineArgs[1], out nServiceLogID)) {
				this.log.Msg("Usage: LoadExperianLtd <MP_ServiceLog entry ID>");
				return;
			} // if

			var res = this.serviceClient.LoadExperianConsumer(1, 1, null, nServiceLogID);

			this.log.Msg("Result:\n{0}", res.Value);
		}

		[Activation]
		private void LoadExperianLtd() {
			long nServiceLogID;

			if ((this.cmdLineArgs.Length != 2) || !long.TryParse(this.cmdLineArgs[1], out nServiceLogID)) {
				this.log.Msg("Usage: LoadExperianLtd <MP_ServiceLog entry ID>");
				return;
			} // if

			ExperianLtdActionResult lear = this.serviceClient.LoadExperianLtd(nServiceLogID);

			this.log.Msg("Result:\n{0}", lear.Value.StringifyAll());
		}

		[Activation]
		private void LoadManualVatReturnPeriods() {
			int nCustomerMarketplaceID;

			if ((this.cmdLineArgs.Length != 2) || !int.TryParse(this.cmdLineArgs[1], out nCustomerMarketplaceID)) {
				this.log.Msg("Usage: LoadVatReturnSummary <Customer Marketplace ID>");
				return;
			} // if

			VatReturnPeriodsActionResult oResult = this.serviceClient.LoadManualVatReturnPeriods(nCustomerMarketplaceID);

			this.log.Msg("Result is:\n{0}", string.Join("\n", oResult.Periods.Select(x => x.ToString())));
		}

		[Activation]
		private void LoadVatReturnFullData() {
			int customerId;
			int nCustomerMarketplaceID;

			if ((this.cmdLineArgs.Length != 3) || !int.TryParse(this.cmdLineArgs[1], out nCustomerMarketplaceID) || !int.TryParse(this.cmdLineArgs[2], out customerId)) {
				this.log.Msg("Usage: LoadVatReturnFullData <Customer Marketplace ID> <Customer ID>");
				return;
			} // if

			VatReturnDataActionResult oResult = this.serviceClient.LoadVatReturnFullData(customerId, nCustomerMarketplaceID);

			this.log.Msg("VAT return - begin:");

			foreach (var v in oResult.VatReturnRawData)
				this.log.Msg(v.ToString());

			this.log.Msg("VAT return - end.");

			this.log.Msg("RTI months - begin:");

			foreach (var v in oResult.RtiTaxMonthRawData)
				this.log.Msg(v.ToString());

			this.log.Msg("RTI months - end.");

			this.log.Msg("Summary - begin:");

			foreach (var v in oResult.Summary)
				this.log.Msg(v.ToString());

			this.log.Msg("Summary - end.");
		}

		[Activation]
		private void LoadVatReturnRawData() {
			int nCustomerMarketplaceID;

			if ((this.cmdLineArgs.Length != 2) || !int.TryParse(this.cmdLineArgs[1], out nCustomerMarketplaceID)) {
				this.log.Msg("Usage: LoadVatReturnRawData <Customer Marketplace ID>");
				return;
			} // if

			VatReturnDataActionResult oResult = this.serviceClient.LoadVatReturnRawData(nCustomerMarketplaceID);

			this.log.Msg("VAT return - begin:");

			foreach (var v in oResult.VatReturnRawData)
				this.log.Msg(v.ToString());

			this.log.Msg("VAT return - end.");

			this.log.Msg("RTI months - begin:");

			foreach (var v in oResult.RtiTaxMonthRawData)
				this.log.Msg(v.ToString());

			this.log.Msg("RTI months - end.");
		}

		[Activation]
		private void LoadVatReturnSummary() {
			int customerId;
			int nCustomerMarketplaceID;

			if ((this.cmdLineArgs.Length != 3) || !int.TryParse(this.cmdLineArgs[1], out nCustomerMarketplaceID) || !int.TryParse(this.cmdLineArgs[2], out customerId)) {
				this.log.Msg("Usage: LoadVatReturnSummary <Customer Marketplace ID> <Customer ID>");
				return;
			} // if

			VatReturnDataActionResult oResult = this.serviceClient.LoadVatReturnSummary(customerId, nCustomerMarketplaceID);

			this.log.Msg("Result is:\n{0}", string.Join("\n", oResult.Summary.Select(x => x.ToString())));
		}

		[Activation]
		private void LoanFullyPaid() {
			int customerId;
			if (this.cmdLineArgs.Length != 3 || !int.TryParse(this.cmdLineArgs[1], out customerId)) {
				this.log.Msg("Usage: LoanFullyPaid <CustomerId> <loanRefNum>");
				return;
			}

			this.serviceClient.LoanFullyPaid(customerId, this.cmdLineArgs[2]);
		}

		[Activation]
		private void MainStrategy() {
			int underwriterId;
			int customerId;
			NewCreditLineOption newCreditLineOption;
			int avoidAutoDescison;
			long cashRequestID;

			if (
				int.TryParse(this.cmdLineArgs[1], out underwriterId) &&
				int.TryParse(this.cmdLineArgs[2], out customerId) &&
				Enum.TryParse(this.cmdLineArgs[3], out newCreditLineOption) &&
				int.TryParse(this.cmdLineArgs[4], out avoidAutoDescison) &&
				long.TryParse(this.cmdLineArgs[5], out cashRequestID)
			) {
				this.serviceClient.MainStrategyAsync(
					underwriterId,
					customerId,
					newCreditLineOption,
					avoidAutoDescison,
					cashRequestID <= 0 ? (long?)null : cashRequestID,
					CashRequestOriginator.Manual
				);
				return;
			}

			// NewCreditLineOption.UpdateEverythingAndApplyAutoRules 3
			// avoidAutoDescison 0

			this.log.Msg("Usage: MainStrategy <Underwriter ID> <customerId> <newCreditLineOption> <avoidAutoDescison> <cash request id>");
			this.log.Msg("Cash request id: 0 (or negative) to create a new one, or id to update existing one.");
		} // MainStrategy

		[Activation]
		private void MarketplaceInstantUpdate() {
			int nCustomerMarketplaceID;

			if ((this.cmdLineArgs.Length != 2) || !int.TryParse(this.cmdLineArgs[1], out nCustomerMarketplaceID)) {
				this.log.Msg("Usage: MarketplaceInstantUpdate <Customer Marketplace ID>");
				return;
			} // if

			this.serviceClient.MarketplaceInstantUpdate(nCustomerMarketplaceID);
		}

		[Activation]
		private void MoreAmlAndBwaInformation() {
			int underwriterId;
			int customerId;
			if (this.cmdLineArgs.Length != 3 || !int.TryParse(this.cmdLineArgs[1], out underwriterId) || !int.TryParse(this.cmdLineArgs[2], out customerId)) {
				this.log.Msg("Usage: MoreAmlAndBwaInformation <Underwriter ID> <CustomerId>");
				return;
			}

			this.serviceClient.MoreAmlAndBwaInformation(underwriterId, customerId);
		}

		[Activation]
		private void MoreAmlInformation() {
			int underwriterId;
			int customerId;
			if (this.cmdLineArgs.Length != 3 || !int.TryParse(this.cmdLineArgs[1], out underwriterId) || !int.TryParse(this.cmdLineArgs[2], out customerId)) {
				this.log.Msg("Usage: MoreAmlInformation <Underwriter ID> <CustomerId>");
				return;
			}
			this.serviceClient.MoreAmlInformation(underwriterId, customerId);
		}

		[Activation]
		private void MoreBwaInformation() {
			int underwriterId;
			int customerId;
			if (this.cmdLineArgs.Length != 3 || !int.TryParse(this.cmdLineArgs[1], out underwriterId) || !int.TryParse(this.cmdLineArgs[2], out customerId)) {
				this.log.Msg("Usage: MoreBwaInformation <Underwriter ID> <CustomerId>");
				return;
			}

			this.serviceClient.MoreBwaInformation(underwriterId, customerId);
		}

		[Activation]
		private void Noop() {
			this.adminClient.Noop();
		}

		[Activation]
		private void Nop() {
			int nLength;

			var oUsage = new Action(() => {
				this.log.Msg("Usage: NOP <length> <message>");
				this.log.Msg("Where");
				this.log.Msg("\tlength - for how many seconds continue executing");
				this.log.Msg("\tmessage - some message to print to log file");
			});

			if (this.cmdLineArgs.Length != 3 || !int.TryParse(this.cmdLineArgs[1], out nLength)) {
				oUsage();
				return;
			} // if

			string sMsg = this.cmdLineArgs[2];

			if (nLength < 1) {
				oUsage();
				return;
			} // if

			this.adminClient.Nop(nLength, sMsg);
		}

		[Activation]
		private void NotifySalesOnNewCustomer() {
			int nCustomerID;

			if ((this.cmdLineArgs.Length != 2) || !int.TryParse(this.cmdLineArgs[1], out nCustomerID)) {
				this.log.Msg("Usage: NotifySalesOnNewCustomer <customer id>");
				return;
			} // if

			this.serviceClient.NotifySalesOnNewCustomer(nCustomerID);
		}

		[Activation]
		private void ParseExperianConsumer() {
			long nServiceLogID;

			if ((this.cmdLineArgs.Length != 2) || !long.TryParse(this.cmdLineArgs[1], out nServiceLogID)) {
				this.log.Msg("Usage: LoadExperianConsumer <MP_ServiceLog entry ID>");
				return;
			} // if

			var res = this.serviceClient.ParseExperianConsumer(nServiceLogID);

			this.log.Msg("Result:\n{0}", res.Value);
		}

		[Activation]
		private void ParseExperianLtd() {
			long nServiceLogID;

			if ((this.cmdLineArgs.Length != 2) || !long.TryParse(this.cmdLineArgs[1], out nServiceLogID)) {
				this.log.Msg("Usage: ParseExperianLtd <MP_ServiceLog entry ID>");
				return;
			} // if
			this.serviceClient.ParseExperianLtd(nServiceLogID);
		}

		[Activation]
		private void PasswordRestored() {
			int customerId;
			if (this.cmdLineArgs.Length != 2 || !int.TryParse(this.cmdLineArgs[1], out customerId)) {
				this.log.Msg("Usage: PasswordRestored <CustomerId>");
				return;
			}

			this.serviceClient.PasswordRestored(customerId);
		}

		[Activation]
		private void PayEarly() {
			int customerId;
			decimal amount;
			if (this.cmdLineArgs.Length != 4 || !int.TryParse(this.cmdLineArgs[1], out customerId) || !decimal.TryParse(this.cmdLineArgs[2], out amount)) {
				this.log.Msg("Usage: PayEarly <CustomerId> <amount> <loanRefNumber>");
				return;
			}

			this.serviceClient.PayEarly(customerId, amount, this.cmdLineArgs[3]);
		}

		[Activation]
		private void PayPointAddedByUnderwriter() {
			int customerId, underwriterId;
			if (this.cmdLineArgs.Length != 5 || !int.TryParse(this.cmdLineArgs[1], out customerId) || !int.TryParse(this.cmdLineArgs[4], out underwriterId)) {
				this.log.Msg("Usage: PayPointAddedByUnderwriter <CustomerId> <cardno> <underwriterName> <underwriterId>");
				return;
			}

			this.serviceClient.PayPointAddedByUnderwriter(customerId, this.cmdLineArgs[2], this.cmdLineArgs[3], underwriterId);
		}

		[Activation]
		private void PayPointCharger() {
			if (this.cmdLineArgs.Length != 1) {
				this.log.Msg("Usage: PayPointCharger");
				return;
			}

			this.serviceClient.PayPointCharger();
		}

		[Activation]
		private void PayPointNameValidationFailed() {
			int underwriterId;
			int customerId;
			if (this.cmdLineArgs.Length != 4 || !int.TryParse(this.cmdLineArgs[1], out underwriterId) || !int.TryParse(this.cmdLineArgs[2], out customerId)) {
				this.log.Msg("Usage: PayPointNameValidationFailed <Underwriter ID> <CustomerId> <cardHodlerName>");
				return;
			}

			this.serviceClient.PayPointNameValidationFailed(underwriterId, customerId, this.cmdLineArgs[2]);
		}

		[Activation]
		private void QuickOffer() {
			int customerId;
			bool bSaveOfferToDB;

			if (this.cmdLineArgs.Length != 3 || !int.TryParse(this.cmdLineArgs[1], out customerId) || !bool.TryParse(this.cmdLineArgs[2], out bSaveOfferToDB)) {
				this.log.Msg("Usage: QuickOffer <CustomerId> <Save offer to DB>");
				return;
			}

			this.serviceClient.QuickOffer(customerId, bSaveOfferToDB);
		} // QuickOffer

		[Activation]
		private void QuickOfferWithPrerequisites() {
			int customerId;
			bool bSaveOfferToDB;

			if (this.cmdLineArgs.Length != 3 || !int.TryParse(this.cmdLineArgs[1], out customerId) || !bool.TryParse(this.cmdLineArgs[2], out bSaveOfferToDB)) {
				this.log.Msg("Usage: QuickOfferWithPrerequisites <CustomerId> <Save offer to DB>");
				return;
			}

			this.serviceClient.QuickOfferWithPrerequisites(customerId, bSaveOfferToDB);
		} // QuickOffer

	    [Activation]
		private void RescheduleLoan() {
	        int loanId;
	        int userId;
	        int customerId;
            if (this.cmdLineArgs.Length != 4 || !int.TryParse(this.cmdLineArgs[1], out loanId) || !int.TryParse(this.cmdLineArgs[2], out customerId) || !int.TryParse(this.cmdLineArgs[3], out userId))
	        {
				this.log.Msg("Usage: RescheduleLoan <Loan Id> <Customer Id> <User Id>");
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
			this.log.Msg(res.Value);
	    }
        
        [Activation]
		private void RejectUser() {
			int underwriterId;
			int customerId;
			bool bSendToCustomer;

			if (this.cmdLineArgs.Length != 4 || !int.TryParse(this.cmdLineArgs[1], out underwriterId) || !int.TryParse(this.cmdLineArgs[2], out customerId) || !bool.TryParse(this.cmdLineArgs[3], out bSendToCustomer)) {
				this.log.Msg("Usage: RejectUser <Underwriter ID> <CustomerId> <send to customer>");
				return;
			}

			this.serviceClient.RejectUser(underwriterId, customerId, bSendToCustomer);
		}

		[Activation]
		private void RenewEbayToken() {
			int customerId;
			if (this.cmdLineArgs.Length != 4 || !int.TryParse(this.cmdLineArgs[1], out customerId)) {
				this.log.Msg("Usage: RenewEbayToken <CustomerId> <marketplaceName> <eBayAddress>");
				return;
			}

			this.serviceClient.RenewEbayToken(1, customerId, this.cmdLineArgs[2], this.cmdLineArgs[3]);
		}

		[Activation]
		private void RequestCashWithoutTakenLoan() {
			int customerId;
			if (this.cmdLineArgs.Length != 2 || !int.TryParse(this.cmdLineArgs[1], out customerId)) {
				this.log.Msg("Usage: RequestCashWithoutTakenLoan <CustomerId>");
				return;
			}

			this.serviceClient.RequestCashWithoutTakenLoan(customerId);
		}

		[Activation]
		private void SetLateLoanStatus() {
			if (this.cmdLineArgs.Length != 1) {
				this.log.Msg("Usage: SetLateLoanStatus");
				return;
			}

			this.serviceClient.SetLateLoanStatus();
		}

		[Activation]
		private void Shutdown() {
			ActionMetaData res = this.adminClient.Shutdown();
			this.log.Msg("Shutdown request result: status = {0}, comment = '{1}'", res.Status, res.Comment);
		}

		[Activation]
		private void StressTest() {
			int nLength;
			int nCount;

			var oUsage = new Action(() => {
				this.log.Msg("Usage: StressTest <count> <length>");
				this.log.Msg("Where");
				this.log.Msg("\tcount - how many requests to execute");
				this.log.Msg("\tlength - length in seconds of each request (1..100 seconds).");
			});

			if (this.cmdLineArgs.Length != 3 || !int.TryParse(this.cmdLineArgs[1], out nCount) || !int.TryParse(this.cmdLineArgs[2], out nLength)) {
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

			this.log.Debug("Stress test with {0} requests of {1} seconds each...", nCount, nLength);

			for (int i = 0; i < nCount; i++)
				this.adminClient.StressTestAction(nLength, i.ToString(CultureInfo.InvariantCulture));

			this.log.Debug("Stress test with {0} requests of {1} seconds each is complete.", nCount, nLength);
		}

		[Activation]
		private void StressTestSync() {
			int nLength;

			var oUsage = new Action(() => {
				this.log.Msg("Usage: StressTestSync <count> <message>");
				this.log.Msg("Where");
				this.log.Msg("\tcount - how many requests to execute");
			});

			if (this.cmdLineArgs.Length != 3 || !int.TryParse(this.cmdLineArgs[1], out nLength)) {
				oUsage();
				return;
			} // if

			string sMsg = this.cmdLineArgs[2];

			if ((nLength < 1) || (nLength > 100)) {
				oUsage();
				return;
			} // if

			this.adminClient.StressTestSync(nLength, sMsg);
		}

		// GeneratePassword
		// BackfillCustomerAnalyticsCompany
		[Activation]
		private void Temp_BackFillMedals() {
			if ((this.cmdLineArgs.Length != 1)) {
				this.log.Msg("Usage: Temp_BackFillMedals");
				return;
			} // if

			this.serviceClient.Temp_BackFillMedals();
		}

		[Activation]
		private void TerminateAction() {
			if (this.cmdLineArgs.Length != 2) {
				this.log.Msg("Usage: TerminateAction <action guid>");

				this.log.Msg(@"
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

			this.adminClient.Terminate(new Guid(this.cmdLineArgs[1]));
		}

		[Activation]
		private void TransferCashFailed() {
			int customerId;
			if (this.cmdLineArgs.Length != 2 || !int.TryParse(this.cmdLineArgs[1], out customerId)) {
				this.log.Msg("Usage: TransferCashFailed <CustomerId>");
				return;
			}

			this.serviceClient.TransferCashFailed(customerId);
		}

		[Activation]
		private void UpdateCurrencyRates() {
			if (this.cmdLineArgs.Length != 1) {
				this.log.Msg("Usage: UpdateCurrencyRates");
				return;
			}

			this.serviceClient.UpdateCurrencyRates();
		}

		[Activation]
		private void UpdateGoogleAnalytics() {
			if (this.cmdLineArgs.Length < 2) {
				this.serviceClient.UpdateGoogleAnalytics(null, null);
				return;
			} // if

			DateTime oFrom;
			DateTime oTo;

			if (
				(this.cmdLineArgs.Length != 3) ||
					!DateTime.TryParse(this.cmdLineArgs[1], out oFrom) ||
					!DateTime.TryParse(this.cmdLineArgs[2], out oTo)
				) {
				this.log.Msg("Usage: UpdateGoogleAnalytics [<backfill from date> <backfill to date>]");
				return;
			} // if

			this.serviceClient.UpdateGoogleAnalytics(oFrom, oTo);
		}

		[Activation]
		private void UpdateLinkedHmrcPassword() {
			int nCustomerID;

			if ((this.cmdLineArgs.Length != 4) || !int.TryParse(this.cmdLineArgs[1], out nCustomerID)) {
				this.log.Msg("Usage: UpdateLinkedHmrcPassword <Customer ID> <Display name> <Password>");
				return;
			} // if

			string sDisplayName = this.cmdLineArgs[2];
			string sPassword = this.cmdLineArgs[3];

			this.serviceClient.UpdateLinkedHmrcPassword(
				new Encrypted(nCustomerID.ToString(CultureInfo.InvariantCulture)),
				new Encrypted(sDisplayName),
				new Encrypted(sPassword)
			);
		}

		[Activation]
		private void UpdateTransactionStatus() {
			if (this.cmdLineArgs.Length != 1) {
				this.log.Msg("Usage: UpdateTransactionStatus");
				return;
			}

			this.serviceClient.UpdateTransactionStatus();
		}

		[Activation]
		private void VerifyApproval() {
			var i = new VerificationInput("VerifyApproval", this.cmdLineArgs, this.log);

			i.Init();

			if (i.IsGood)
				this.serviceClient.VerifyApproval(i.CustomerCount, i.LastCheckedCustomerID);
		}

		[Activation]
		private void VerifyReapproval() {
			var i = new VerificationInput("VerifyReapproval", this.cmdLineArgs, this.log);

			i.Init();

			if (i.IsGood)
				this.serviceClient.VerifyReapproval(i.CustomerCount, i.LastCheckedCustomerID);
		}

		[Activation]
		private void VerifyReject() {
			var i = new VerificationInput("VerifyReject", this.cmdLineArgs, this.log);

			i.Init();

			if (i.IsGood)
				this.serviceClient.VerifyReject(i.CustomerCount, i.LastCheckedCustomerID);
		}

		[Activation]
		private void VerifyRerejection() {
			var i = new VerificationInput("VerifyRerejection", this.cmdLineArgs, this.log);

			i.Init();

			if (i.IsGood)
				this.serviceClient.VerifyRerejection(i.CustomerCount, i.LastCheckedCustomerID);
		}

		[Activation]
		private void MaamMedalAndPricing() {
			var i = new VerificationInput("MaamMedalAndPricing", this.cmdLineArgs, this.log);

			i.Init();

			if (i.IsGood)
				this.serviceClient.MaamMedalAndPricing(i.CustomerCount, i.LastCheckedCustomerID);
		} // MaamMedalAndPricing

		[Activation]
		private void VerifyMedal() {
			var i = new MedalVerificationInput("VerifyMedal", this.cmdLineArgs, this.log);

			i.Init();

			this.log.Debug("Arguments: {0}", i);

			if (i.IsGood)
				this.serviceClient.VerifyMedal(i.CustomerCount, i.LastCheckedCustomerID, i.IncludeTest, i.CalculationTime);
		} // VerifyMedal

		[Activation]
		private void WriteToLog() {
			if (this.cmdLineArgs.Length < 3) {
				this.log.Msg("Usage: WriteToLog <severity> <arg1> <arg2> ... <argN>");
				this.log.Msg("All the args are merged into a message which is written to service log with requested severity.");
				return;
			} // if

			this.adminClient.WriteToLog(this.cmdLineArgs[1], string.Join(" ", this.cmdLineArgs.Skip(2)));
		}

		[Activation]
		private void XDaysDue() {
			if (this.cmdLineArgs.Length != 1) {
				this.log.Msg("Usage: XDaysDue");
				return;
			}

			this.serviceClient.XDaysDue();
		}

		[Activation]
		private void TotalMaamMedalAndPricing() {
			bool testMode = false;

			if (this.cmdLineArgs.Length > 1)
				testMode = this.cmdLineArgs[1].Equals("test", StringComparison.CurrentCultureIgnoreCase);

			this.serviceClient.TotalMaamMedalAndPricing(testMode);
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

			if ((this.cmdLineArgs.Length != 3) || !int.TryParse(this.cmdLineArgs[1], out customerID) || !long.TryParse(this.cmdLineArgs[2], out aliMemberID)) {
				this.log.Msg("Usage: CustomerAvaliableCredit <Customer ID> <Alibaba MemberID>");
				return;
			}

			this.log.Debug("activator: customerID: {0}, aliMemberID: {1}", customerID, aliMemberID);

			//AlibabaAvailableCreditActionResult result = serviceClient.CustomerAvaliableCredit(customerID, aliMemberID);
			//this.log.Debug("blablabla: {0}",  JsonConvert.SerializeObject(result)); //json
		}

		[Activation]
		private void DataSharing() { // DataSharing 18241
			int customerID;

			if ((this.cmdLineArgs.Length != 2) || !int.TryParse(this.cmdLineArgs[1], out customerID)) {
				this.log.Msg("Usage: DataSharing <Customer ID>");
				return;
			}

			this.log.Debug("activator: customerID: {0}", customerID);

			// ActionMetaData result =
			this.serviceClient.DataSharing(customerID, AlibabaBusinessType.APPLICATION_REVIEW, null );
			//this.log.Debug("result: {0}", JsonConvert.SerializeObject(result.Result)); //json
		}

		[Activation]
		private void BravoAutomationReport() {
			Tuple<DateTime?, DateTime?> dates = GetDatesForAutomationReports();

			this.log.Debug(
				"Start date is {0}, end date is {1}", 
				dates.Item1.HasValue ? dates.Item1.Value.ToString("MMM d yyyy", CultureInfo.InvariantCulture) : string.Empty,
				dates.Item2.HasValue ? dates.Item2.Value.ToString("MMM d yyyy", CultureInfo.InvariantCulture) : string.Empty
			);

			this.serviceClient.BravoAutomationReport(dates.Item1, dates.Item2);
		} // BravoAutomationReport

		private Tuple<DateTime?, DateTime?> GetDatesForAutomationReports() {
			bool hasStart = false;
			DateTime startTime = new DateTime(2015, 5, 11, 0, 0, 0, DateTimeKind.Utc);

			bool hasEnd = false;
			DateTime endTime = DateTime.UtcNow;

			if (this.cmdLineArgs.Length > 1)
				hasStart = DateTime.TryParse(this.cmdLineArgs[1], out startTime);

			if (hasStart && (this.cmdLineArgs.Length > 2))
				hasEnd = DateTime.TryParse(this.cmdLineArgs[2], out endTime);

			return new Tuple<DateTime?, DateTime?>(
				hasStart ? startTime : (DateTime?)null,
				hasEnd ? endTime : (DateTime?)null
			);
		} // GetDatesForAutomationReports

        [Activation]
        private void BrokerTransferCommission()
        {
            ActionMetaData result = this.serviceClient.BrokerTransferCommission();
            this.log.Debug("{0}", result.Status.ToString());
        }

		[Activation]
		private void BackFillBrokerCommissionInvoice() {
			ActionMetaData result = this.serviceClient.BackfillBrokerCommissionInvoice();
			this.log.Debug("BackFillBrokerCommissionInvoice {0}", result.Status.ToString());
		}

		[Activation]
		private void BackfillMedalForAll() {
			this.serviceClient.BackfillMedalForAll();
		} // BackfillMedalForAll

		[Activation]
		private void BackfillDailyLoanStats() {
			this.serviceClient.BackfillDailyLoanStats();
		} // BackfillDailyLoanStats

		[Activation]
		private void RecalculateAutoRejectOnFirstDecision() {
			this.serviceClient.RecalculateAutoRejectOnFirstDecision();
		} // RecalculateAutoRejectOnFirstDecision

		[Activation]
		private void GetIncomeSms() {
			DateTime? date = null;

			if (this.cmdLineArgs.Length == 2) {
				DateTime day;
				if (!DateTime.TryParse(this.cmdLineArgs[1], out day)) {
					this.log.Msg("Usage: GetIncomeSms <date yyyy-mm-dd> OR GetIncomeSms without parameters for backfill everything");
					return;
				} else { //if
					date = day;
				}
			} // if

			this.serviceClient.GetIncomeSms(date, false);
		} // GetIncomeSms

		[Activation]
		private void ManualLegalDocsSyncTemplatesFiles() {

			const string pathExample = @"C:\ezbob\App\PluginWeb\EzBob.Web\Areas\Customer\Views\Agreement";
				string agreementsPath = null;

			if (this.cmdLineArgs.Length == 2) {
				agreementsPath = this.cmdLineArgs[1];
			} else {
				this.log.Msg("Usage: ManualLegalDocsSyncTemplatesFiles path to agreements folder like: {0}", pathExample);
				return;
			} // if
			this.serviceClient.ManualLegalDocsSyncTemplatesFiles(agreementsPath);
		} // ManualLegalDocsSyncTemplatesFiles


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
