namespace StrategiesActivator
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.ServiceModel;
	using ConfigManager;
	using EzServiceConfigurationLoader;
	using System;
	using EzServiceReference;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Newtonsoft.Json;
	using log4net;

	public class ServiceClientActivation
	{
		private readonly string[] args;
		private readonly EzServiceClient serviceClient;
		private readonly EzServiceAdminClient adminClient;

		public ServiceClientActivation(string[] args)
		{
			m_oLog = new ConsoleLog(new SafeILog(LogManager.GetLogger(typeof(ServiceClientActivation))));
			m_oLog.NotifyStart();

			InitMethods();

			string sInstanceName = args[0].ToLower();

			if (sInstanceName.ToLower() == "list")
			{
				ListSupported();
				m_oLog.NotifyStop();
				throw new ExitException();
			} // if

			var env = new Ezbob.Context.Environment(m_oLog);
			AConnection db = new SqlConnection(env, m_oLog);

			ConfigManager.CurrentValues.Init(db, m_oLog);

			Configuration cfg = null;

			if (m_oMethods.ContainsKey(sInstanceName))
			{
				cfg = new DefaultConfiguration(System.Environment.MachineName, db, m_oLog);
				this.args = args;
			}
			else
			{
				this.args = new string[args.Length - 1];
				Array.Copy(args, 1, this.args, 0, args.Length - 1);

				cfg = new Configuration(sInstanceName, db, m_oLog);
			} // if

			cfg.Init();

			m_oLog.Info("Running against instance {0} - {1}.", cfg.InstanceID, cfg.InstanceName);
			m_oLog.Info("Arguments:\n\t{0}", string.Join("\n\t", this.args));

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

		public void Execute()
		{
			string strategyName = args[0];

			string sKey = strategyName.ToLower();

			if (m_oMethods.ContainsKey(sKey))
				m_oMethods[sKey].Invoke(this, new object[] { });
			else
			{
				m_oLog.Msg("Strategy {0} is not supported", strategyName);
				ListSupported();
			} // if

			m_oLog.NotifyStop();
		} // Execute

		private void ListSupported()
		{
			m_oLog.Msg("Supported strategies are (case insensitive):\n\t{0}", string.Join("\n\t", m_oMethods.Keys));
		} // ListSupported

		private void InitMethods()
		{
			MethodInfo[] aryMethods = GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);

			m_oMethods = new SortedDictionary<string, MethodInfo>();

			foreach (MethodInfo mi in aryMethods)
			{
				IEnumerable<ActivationAttribute> oAttrList = mi.GetCustomAttributes<ActivationAttribute>();

				if (oAttrList.Any())
					m_oMethods[mi.Name.ToLower()] = mi;
			} // foreach

		} // InitMethods

		private SortedDictionary<string, MethodInfo> m_oMethods;
		private readonly ASafeLog m_oLog;

		#region strategy activators
		// ReSharper disable UnusedMember.Local

		[Activation]
		private void Greeting()
		{
			int customerId;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId))
			{
				m_oLog.Msg("Usage: Greeting <CustomerId> <ConfirmEmailAddress>");
				return;
			}

			serviceClient.GreetingMailStrategy(customerId, args[2]);
		}

		[Activation]
		private void GetWizardConfigs()
		{
			serviceClient.GetWizardConfigs();
		}

		[Activation]
		private void QuickOffer()
		{
			int customerId;
			bool bSaveOfferToDB;

			if (args.Length != 3 || !int.TryParse(args[1], out customerId) || !bool.TryParse(args[2], out bSaveOfferToDB))
			{
				m_oLog.Msg("Usage: QuickOffer <CustomerId> <Save offer to DB>");
				return;
			}

			serviceClient.QuickOffer(customerId, bSaveOfferToDB);
		} // QuickOffer

		[Activation]
		private void QuickOfferWithPrerequisites()
		{
			int customerId;
			bool bSaveOfferToDB;

			if (args.Length != 3 || !int.TryParse(args[1], out customerId) || !bool.TryParse(args[2], out bSaveOfferToDB))
			{
				m_oLog.Msg("Usage: QuickOfferWithPrerequisites <CustomerId> <Save offer to DB>");
				return;
			}

			serviceClient.QuickOfferWithPrerequisites(customerId, bSaveOfferToDB);
		} // QuickOffer

		[Activation]
		private void ApprovedUser()
		{
			int underwriterId;
			int customerId;
			decimal loanAmount;

			if (args.Length != 4 || !int.TryParse(args[1], out underwriterId) || !int.TryParse(args[2], out customerId) || !decimal.TryParse(args[3], out loanAmount))
			{
				m_oLog.Msg("Usage: ApprovedUser <Underwriter ID> <CustomerId> <loanAmount>");
				return;
			}

			serviceClient.ApprovedUser(underwriterId, customerId, loanAmount);
		}

		[Activation]
		private void CashTransferred()
		{
			int customerId;
			decimal amount;
			if (args.Length != 4 || !int.TryParse(args[1], out customerId) || !decimal.TryParse(args[2], out amount))
			{
				m_oLog.Msg("Usage: CashTransferred <CustomerId> <amount> <loanRefNum>");
				return;
			}
			string loanRefNum = args[3];
			serviceClient.CashTransferred(customerId, amount, loanRefNum);
		}

		[Activation]
		private void EmailRolloverAdded()
		{
			int customerId;
			decimal amount;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId) || !decimal.TryParse(args[2], out amount))
			{
				m_oLog.Msg("Usage: EmailRolloverAdded <CustomerId> <amount>");
				return;
			}

			serviceClient.EmailRolloverAdded(customerId, amount);
		}

		[Activation]
		private void EmailUnderReview()
		{
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId))
			{
				m_oLog.Msg("Usage: EmailUnderReview <CustomerId>");
				return;
			}

			serviceClient.EmailUnderReview(customerId);
		}

		[Activation]
		private void Escalated()
		{
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId))
			{
				m_oLog.Msg("Usage: Escalated <CustomerId>");
				return;
			}

			serviceClient.Escalated(customerId);
		}

		[Activation]
		private void GetCashFailed()
		{
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId))
			{
				m_oLog.Msg("Usage: GetCashFailed <CustomerId>");
				return;
			}

			serviceClient.GetCashFailed(customerId);
		}

		[Activation]
		private void LoanFullyPaid()
		{
			int customerId;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId))
			{
				m_oLog.Msg("Usage: LoanFullyPaid <CustomerId> <loanRefNum>");
				return;
			}

			serviceClient.LoanFullyPaid(customerId, args[2]);
		}

		[Activation]
		private void MoreAmlAndBwaInformation()
		{
			int underwriterId;
			int customerId;
			if (args.Length != 3 || !int.TryParse(args[1], out underwriterId) || !int.TryParse(args[2], out customerId))
			{
				m_oLog.Msg("Usage: MoreAmlAndBwaInformation <Underwriter ID> <CustomerId>");
				return;
			}

			serviceClient.MoreAmlAndBwaInformation(underwriterId, customerId);
		}

		[Activation]
		private void MoreAmlInformation()
		{
			int underwriterId;
			int customerId;
			if (args.Length != 3 || !int.TryParse(args[1], out underwriterId) || !int.TryParse(args[2], out customerId))
			{
				m_oLog.Msg("Usage: MoreAmlInformation <Underwriter ID> <CustomerId>");
				return;
			}
			serviceClient.MoreAmlInformation(underwriterId, customerId);
		}

		[Activation]
		private void MoreBwaInformation()
		{
			int underwriterId;
			int customerId;
			if (args.Length != 3 || !int.TryParse(args[1], out underwriterId) || !int.TryParse(args[2], out customerId))
			{
				m_oLog.Msg("Usage: MoreBwaInformation <Underwriter ID> <CustomerId>");
				return;
			}

			serviceClient.MoreBwaInformation(underwriterId, customerId);
		}

		[Activation]
		private void PasswordChanged()
		{
			int customerId;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId))
			{
				m_oLog.Msg("Usage: PasswordChanged <CustomerId> <password>");
				return;
			}

			serviceClient.PasswordChanged(customerId, args[2]);
		}

		[Activation]
		private void PasswordRestored()
		{
			int customerId;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId))
			{
				m_oLog.Msg("Usage: PasswordRestored <CustomerId> <password>");
				return;
			}

			serviceClient.PasswordRestored(customerId, args[2]);
		}

		[Activation]
		private void PayEarly()
		{
			int customerId;
			decimal amount;
			if (args.Length != 4 || !int.TryParse(args[1], out customerId) || !decimal.TryParse(args[2], out amount))
			{
				m_oLog.Msg("Usage: PayEarly <CustomerId> <amount> <loanRefNumber>");
				return;
			}

			serviceClient.PayEarly(customerId, amount, args[3]);
		}

		[Activation]
		private void PayPointAddedByUnderwriter()
		{
			int customerId, underwriterId;
			if (args.Length != 5 || !int.TryParse(args[1], out customerId) || !int.TryParse(args[4], out underwriterId))
			{
				m_oLog.Msg("Usage: PayPointAddedByUnderwriter <CustomerId> <cardno> <underwriterName> <underwriterId>");
				return;
			}

			serviceClient.PayPointAddedByUnderwriter(customerId, args[2], args[3], underwriterId);
		}

		[Activation]
		private void PayPointNameValidationFailed()
		{
			int underwriterId;
			int customerId;
			if (args.Length != 4 || !int.TryParse(args[1], out underwriterId) || !int.TryParse(args[2], out customerId))
			{
				m_oLog.Msg("Usage: PayPointNameValidationFailed <Underwriter ID> <CustomerId> <cardHodlerName>");
				return;
			}

			serviceClient.PayPointNameValidationFailed(underwriterId, customerId, args[2]);
		}

		[Activation]
		private void RejectUser()
		{
			int underwriterId;
			int customerId;
			if (args.Length != 3 || !int.TryParse(args[1], out underwriterId) || !int.TryParse(args[2], out customerId))
			{
				m_oLog.Msg("Usage: RejectUser <Underwriter ID> <CustomerId>");
				return;
			}

			serviceClient.RejectUser(underwriterId, customerId);
		}

		[Activation]
		private void RenewEbayToken()
		{
			int customerId;
			if (args.Length != 4 || !int.TryParse(args[1], out customerId))
			{
				m_oLog.Msg("Usage: RenewEbayToken <CustomerId> <marketplaceName> <eBayAddress>");
				return;
			}

			serviceClient.RenewEbayToken(customerId, args[2], args[3]);
		}

		[Activation]
		private void RequestCashWithoutTakenLoan()
		{
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId))
			{
				m_oLog.Msg("Usage: RequestCashWithoutTakenLoan <CustomerId>");
				return;
			}

			serviceClient.RequestCashWithoutTakenLoan(customerId);
		}

		[Activation]
		private void SendEmailVerification()
		{
			int customerId;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId))
			{
				m_oLog.Msg("Usage: SendEmailVerification <CustomerId> <address>");
				return;
			}

			serviceClient.SendEmailVerification(customerId, "", args[2]);
		}

		[Activation]
		private void ThreeInvalidAttempts()
		{
			int customerId;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId))
			{
				m_oLog.Msg("Usage: ThreeInvalidAttempts <CustomerId> <password>");
				return;
			}

			serviceClient.ThreeInvalidAttempts(customerId, args[2]);
		}

		[Activation]
		private void TransferCashFailed()
		{
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId))
			{
				m_oLog.Msg("Usage: TransferCashFailed <CustomerId>");
				return;
			}

			serviceClient.TransferCashFailed(customerId);
		}

		[Activation]
		private void CaisGenerate()
		{
			int underwriterId;
			if (args.Length != 2 || !int.TryParse(args[1], out underwriterId))
			{
				m_oLog.Msg("Usage: CaisGenerate <underwriterId>");
				return;
			}

			serviceClient.CaisGenerate(underwriterId);
		}

		[Activation]
		private void CaisUpdate()
		{
			int underwriterId;
			int caisId;
			if (args.Length != 3 || !int.TryParse(args[1], out underwriterId) || !int.TryParse(args[2], out caisId))
			{
				m_oLog.Msg("Usage: CaisUpdate <Underwriter ID> <caisId>");
				return;
			}

			serviceClient.CaisUpdate(underwriterId, caisId);
		}

		[Activation]
		private void FirstOfMonthStatusNotifier()
		{
			if (args.Length != 1)
			{
				m_oLog.Msg("Usage: FirstOfMonthStatusNotifier");
				return;
			}

			serviceClient.FirstOfMonthStatusNotifier();
		}

		[Activation]
		private void FraudChecker()
		{
			int customerId;
			FraudMode mode;

			if (args.Length != 3 || !int.TryParse(args[1], out customerId) || !FraudMode.TryParse(args[2], true, out mode))
			{
				m_oLog.Msg("Usage: FraudChecker <CustomerId> <Fraud mode>");
				m_oLog.Msg("Fraud mode values: {0}", string.Join(", ", Enum.GetValues(typeof(FraudMode))));
				return;
			}

			serviceClient.FraudChecker(customerId, mode);
		}

		[Activation]
		private void LateBy14Days()
		{
			if (args.Length != 1)
			{
				m_oLog.Msg("Usage: LateBy14Days");
				return;
			}

			serviceClient.LateBy14Days();
		}

		[Activation]
		private void PayPointCharger()
		{
			if (args.Length != 1)
			{
				m_oLog.Msg("Usage: PayPointCharger");
				return;
			}

			serviceClient.PayPointCharger();
		}

		[Activation]
		private void SetLateLoanStatus()
		{
			if (args.Length != 1)
			{
				m_oLog.Msg("Usage: SetLateLoanStatus");
				return;
			}

			serviceClient.SetLateLoanStatus();
		}

		[Activation]
		private void CustomerMarketPlaceAdded()
		{
			int customerId, marketplaceId;
			bool bUpdateWizardStep;
			if (args.Length != 4 || !int.TryParse(args[1], out customerId) || !int.TryParse(args[2], out marketplaceId) || !bool.TryParse(args[3], out bUpdateWizardStep))
			{
				m_oLog.Msg("Usage: CustomerMarketPlaceAdded <CustomerId> <CustomerMarketplaceId> <Update customer wizard step>");
				m_oLog.Msg("<Update customer wizard step>: is boolean and means whether to set customer wizard step to Marketplace or not. If wizard step is 'TheLastOne' it is never changed.");
				return;
			}

			serviceClient.UpdateMarketplace(customerId, marketplaceId, bUpdateWizardStep);
		}

		[Activation]
		private void UpdateTransactionStatus()
		{
			if (args.Length != 1)
			{
				m_oLog.Msg("Usage: UpdateTransactionStatus");
				return;
			}

			serviceClient.UpdateTransactionStatus();
		}

		[Activation]
		private void XDaysDue()
		{
			if (args.Length != 1)
			{
				m_oLog.Msg("Usage: XDaysDue");
				return;
			}

			serviceClient.XDaysDue();
		}

		[Activation]
		private void MainStrategy()
		{
			int underwriterId;
			int customerId, avoidAutoDescison;
			NewCreditLineOption newCreditLineOption;

			switch (args.Length)
			{
				case 5:
					if (int.TryParse(args[1], out underwriterId) && int.TryParse(args[2], out customerId) && Enum.TryParse(args[3], out newCreditLineOption) && int.TryParse(args[4], out avoidAutoDescison))
					{
						serviceClient.MainStrategy1(underwriterId, customerId, newCreditLineOption, avoidAutoDescison);
						return;
					}
					break;

				case 6:
					bool isUnderwriterForced;
					if (int.TryParse(args[1], out underwriterId) && int.TryParse(args[2], out customerId) && Enum.TryParse(args[3], out newCreditLineOption) && int.TryParse(args[4], out avoidAutoDescison) && bool.TryParse(args[5], out isUnderwriterForced))
					{
						serviceClient.MainStrategy2(underwriterId, customerId, newCreditLineOption, avoidAutoDescison, isUnderwriterForced);
						return;
					}
					break;
			} // switch

			m_oLog.Msg("Usage: MainStrategy <Underwriter ID> <customerId> <newCreditLineOption> <avoidAutoDescison>");
			m_oLog.Msg("OR");
			m_oLog.Msg("Usage: MainStrategy <Underwriter ID> <customerId> <newCreditLineOption> <avoidAutoDescison> <isUnderwriterForced(should always be true)>");
		}

		[Activation]
		private void MainStrategySync()
		{
			int underwriterId;
			int customerId, avoidAutoDescison;
			NewCreditLineOption newCreditLineOption;
			if (args.Length == 5 && int.TryParse(args[1], out underwriterId) && int.TryParse(args[2], out customerId) && Enum.TryParse(args[3], out newCreditLineOption) && int.TryParse(args[4], out avoidAutoDescison))
			{
				serviceClient.MainStrategySync1(underwriterId, customerId, newCreditLineOption, avoidAutoDescison);
				return;
			}

			m_oLog.Msg("Usage: MainStrategySync <Underwriter ID> <customerId> <newCreditLineOption> <avoidAutoDescison>");
		}

		[Activation]
		private void UpdateCurrencyRates()
		{
			if (args.Length != 1)
			{
				m_oLog.Msg("Usage: UpdateCurrencyRates");
				return;
			}

			serviceClient.UpdateCurrencyRates();
		}

		[Activation]
		private void CheckExperianCompany()
		{
			int customerId;
			bool forceCheck;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId) || !bool.TryParse(args[2], out forceCheck))
			{
				m_oLog.Msg("Usage: CheckExperianCompany <CustomerId> <ForceCheck>");
				return;
			}

			serviceClient.CheckExperianCompany(customerId, forceCheck);
		}

		[Activation]
		private void CheckExperianConsumer()
		{
			int customerId, directorId;
			bool forceCheck;

			if (args.Length != 4 || !int.TryParse(args[1], out customerId) || !int.TryParse(args[2], out directorId) || !bool.TryParse(args[2], out forceCheck))
			{
				m_oLog.Msg("Usage: CheckExperianConsumer <CustomerId> <DirectorId> <ForceCheck>");
				return;
			}

			serviceClient.CheckExperianConsumer(customerId, directorId, forceCheck);
		}

		[Activation]
		private void AmlChecker()
		{
			int customerId;

			if (args.Length == 2 && int.TryParse(args[1], out customerId))
			{
				serviceClient.CheckAml(customerId);
				return;
			}

			if (args.Length == 9 && int.TryParse(args[1], out customerId))
			{
				serviceClient.CheckAmlCustom(customerId, args[2], args[3], args[4], args[5], args[6], args[7], args[8]);
				return;
			}

			m_oLog.Msg("Usage: AmlChecker <CustomerId>");
			m_oLog.Msg("OR");
			m_oLog.Msg("Usage: AmlChecker <CustomerId> <idhubHouseNumber> <idhubHouseName> <idhubStreet> <idhubDistrict> <idhubTown> <idhubCounty> <idhubPostCode>");

		}

		[Activation]
		private void BwaChecker()
		{
			int customerId;

			if (args.Length == 2 && int.TryParse(args[1], out customerId))
			{
				serviceClient.CheckBwa(customerId);
				return;
			}

			if (args.Length == 11 && int.TryParse(args[1], out customerId))
			{
				serviceClient.CheckBwaCustom(customerId, args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10]);
				return;
			}

			m_oLog.Msg("Usage: BwaChecker <CustomerId>");
			m_oLog.Msg("OR");
			m_oLog.Msg("Usage: BwaChecker <CustomerId> <idhubHouseNumber> <idhubHouseName> <idhubStreet> <idhubDistrict> <idhubTown> <idhubCounty> <idhubPostCode> <idhubBranchCode> <idhubAccountNumber>");
		}

		[Activation]
		private void FinishWizard()
		{
			int customerId, underwriterId;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId) || !int.TryParse(args[2], out underwriterId))
			{
				m_oLog.Msg("Usage: FinishWizard <CustomerId> <UnderwriterId>");
				return;
			}

			var oArgs = JsonConvert.DeserializeObject<FinishWizardArgs>(CurrentValues.Instance.FinishWizardForApproved);

			oArgs.CustomerID = customerId;

			serviceClient.FinishWizard(oArgs, underwriterId);
		}

		[Activation]
		private void GenerateMobileCode()
		{
			if (args.Length != 2)
			{
				m_oLog.Msg("Usage: GenerateMobileCode <phone number>");
				return;
			}

			serviceClient.GenerateMobileCode(args[1]);
		}

		[Activation]
		private void GetSpResultTable()
		{
			if (args.Length < 2 || args.Length % 2 != 0)
			{
				m_oLog.Msg("Usage: GetSpResultTable <spName> <parameters - should come in couples>");
				return;
			}

			string spName = args[1];
			var parameterList = new List<string>();
			for (int i = 2; i < args.Length; i++)
			{
				parameterList.Add(args[i]);
			}
			string[] parameterArgs = parameterList.ToArray();

			serviceClient.GetSpResultTable(spName, parameterArgs);
		}

		[Activation]
		private void CreateUnderwriter()
		{
			if (args.Length != 4)
			{
				m_oLog.Msg("Usage: CreateUnderwriter <Name> <Password> <RoleName>");
				m_oLog.Msg(@"Available roles (underwriter is usually one of the last three):
Admin:         Administrator - Manage users
SuperUser:     SuperUser - Have rights to all applications
Nova:          Nova
Inspector:     Inspector - Web working place
CreditAnalyst: Credit Analyst - Manage strategies and workflow, uses Maven and Patron
Maven:         Maven - Creates workflow for strategies
Patron:        Patron - Manage nodes and strategies
Auditor:       Auditor - Monitoring system
FormsDesigner: FormsDesigner - Develop node interface
Web:           Web
Underwriter:   Underwriter
manager:       Manager
crm:           CRM");

				return;
			}

			serviceClient.CreateUnderwriter(args[1], args[2], args[3]);
		}

		[Activation]
		private void BrokerLoadCustomerList()
		{
			if (args.Length != 2)
			{
				m_oLog.Msg("Usage: BrokerLoadCustomerList <Contact person email>");
				return;
			} // if

			BrokerCustomersActionResult res = serviceClient.BrokerLoadCustomerList(args[1]);

			foreach (var oEntry in res.Customers)
				m_oLog.Msg("Customer ID: {0} Name: {1} {2}", oEntry.CustomerID, oEntry.FirstName, oEntry.LastName);
		} // BrokerLoadCustomerList

		[Activation]
		private void NotifySalesOnNewCustomer()
		{
			int nCustomerID = 0;

			if ((args.Length != 2) || !int.TryParse(args[1], out nCustomerID))
			{
				m_oLog.Msg("Usage: NotifySalesOnNewCustomer <customer id>");
				return;
			} // if

			serviceClient.NotifySalesOnNewCustomer(nCustomerID);
		} // NotifySalesOnNewCustomer

		[Activation]
		private void ListActiveActions()
		{
			StringListActionResult res = adminClient.ListActiveActions();

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
		private void TerminateAction()
		{
			if (args.Length != 2)
			{
				m_oLog.Msg("Usage: TerminateAction <action guid>");

				m_oLog.Msg(@"
A string that contains a GUID in one of the following formats ('d' represents a hexadecimal digit whose case is ignored):
32 contiguous digits:
dddddddddddddddddddddddddddddddd
-or-
Groups of 8, 4, 4, 4, and 12 digits with hyphens between the groups. The entire GUID can optionally be enclosed in matching braces or parentheses:
dddddddd-dddd-dddd-dddd-dddddddddddd
-or-
{dddddddd-dddd-dddd-dddd-dddddddddddd}
-or-
(dddddddd-dddd-dddd-dddd-dddddddddddd)
-or-
Groups of 8, 4, and 4 digits, and a subset of eight groups of 2 digits, with each group prefixed by '0x' or '0X', and separated by commas. The entire GUID, as well as the subset, is enclosed in matching braces:
{0xdddddddd, 0xdddd, 0xdddd,{0xdd,0xdd,0xdd,0xdd,0xdd,0xdd,0xdd,0xdd}}
All braces, commas, and '0x' prefixes are required. All embedded spaces are ignored. All leading zeros in a group are ignored.
The digits shown in a group are the maximum number of meaningful digits that can appear in that group. You can specify from 1 to the number of digits shown for a group. The specified digits are assumed to be the low-order digits of the group.
");
				return;
			} // if

			adminClient.Terminate(new Guid(args[1]));
		} // TerminateAction

		// ReSharper restore UnusedMember.Local

		#endregion strategy activators
	} // class ServiceClientActivation
} // namespace
