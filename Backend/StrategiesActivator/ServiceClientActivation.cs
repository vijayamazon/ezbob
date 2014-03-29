namespace StrategiesActivator {
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.ServiceModel;
	using EzServiceConfigurationLoader;
	using System;
	using EzServiceReference;
	using Ezbob.Database;
	using Ezbob.Logger;
	using log4net;

	public class ServiceClientActivation {
		private readonly string[] args;
		private readonly EzServiceClient serviceClient;
		private readonly EzServiceAdminClient adminClient;

		public ServiceClientActivation(string[] args) {
			this.args = new string[args.Length - 1];
			Array.Copy(args, 1, this.args, 0, args.Length - 1);

			string sInstanceName = args[0];

			ASafeLog log = new SafeILog(LogManager.GetLogger(typeof(ServiceClientActivation)));

			var env = new Ezbob.Context.Environment(log);
			AConnection db = new SqlConnection(env, log);

			var cfg = new Configuration(sInstanceName, db, log);
			cfg.Init();

			var oTcpBinding = new NetTcpBinding();

			serviceClient = new EzServiceClient(
				oTcpBinding,
				new EndpointAddress(cfg.AdminEndpointAddress)
			);

			adminClient = new EzServiceAdminClient(
				oTcpBinding,
				new EndpointAddress(cfg.AdminEndpointAddress)
			);
		}

		public void Execute() {
			string strategyName = args[0];

			MethodInfo[] aryMethods = GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);

			var oMethods = new SortedDictionary<string, MethodInfo>();

			foreach (MethodInfo mi in aryMethods) {
				IEnumerable<ActivationAttribute> oAttrList = mi.GetCustomAttributes<ActivationAttribute>();

				if (oAttrList.Any()) {
					oMethods[mi.Name.ToLower()] = mi;
				}
			} // foreach

			string sKey = strategyName.ToLower();

			if (oMethods.ContainsKey(sKey)) {
				oMethods[sKey].Invoke(this, new object[] { });
			}
			else {
				Console.WriteLine("Strategy {0} is not supported", strategyName);
				Console.WriteLine("Supported strategies are (case insensitive):\n\t{0}", string.Join("\n\t", oMethods.Keys));
			}
		} // Execute

		#region strategy activators
		// ReSharper disable UnusedMember.Local

		[Activation]
		private void Greeting() {
			int customerId;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> Greeting <CustomerId> <ConfirmEmailAddress>");
				return;
			}

			serviceClient.GreetingMailStrategy(customerId, args[2]);
		}

		[Activation]
		private void GetWizardConfigs() {
			serviceClient.GetWizardConfigs();
		}

		[Activation]
		private void QuickOffer() {
			int customerId;
			bool bSaveOfferToDB;

			if (args.Length != 3 || !int.TryParse(args[1], out customerId) || !bool.TryParse(args[2], out bSaveOfferToDB)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> QuickOffer <CustomerId> <Save offer to DB>");
				return;
			}

			serviceClient.QuickOffer(customerId, bSaveOfferToDB);
		} // QuickOffer

		[Activation]
		private void QuickOfferWithPrerequisites() {
			int customerId;
			bool bSaveOfferToDB;

			if (args.Length != 3 || !int.TryParse(args[1], out customerId) || !bool.TryParse(args[2], out bSaveOfferToDB)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> QuickOfferWithPrerequisites <CustomerId> <Save offer to DB>");
				return;
			}

			serviceClient.QuickOfferWithPrerequisites(customerId, bSaveOfferToDB);
		} // QuickOffer

		[Activation]
		private void ApprovedUser() {
			int underwriterId;
			int customerId;
			decimal loanAmount;

			if (args.Length != 4 || !int.TryParse(args[1], out underwriterId) || !int.TryParse(args[2], out customerId) || !decimal.TryParse(args[3], out loanAmount)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> ApprovedUser <Underwriter ID> <CustomerId> <loanAmount>");
				return;
			}

			serviceClient.ApprovedUser(underwriterId, customerId, loanAmount);
		}

		[Activation]
		private void CashTransferred() {
			int customerId;
			decimal amount;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId) || !decimal.TryParse(args[2], out amount)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> CashTransferred <CustomerId> <amount>");
				return;
			}

			serviceClient.CashTransferred(customerId, amount);
		}

		[Activation]
		private void EmailRolloverAdded() {
			int customerId;
			decimal amount;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId) || !decimal.TryParse(args[2], out amount)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> EmailRolloverAdded <CustomerId> <amount>");
				return;
			}

			serviceClient.EmailRolloverAdded(customerId, amount);
		}

		[Activation]
		private void EmailUnderReview() {
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> EmailUnderReview <CustomerId>");
				return;
			}

			serviceClient.EmailUnderReview(customerId);
		}

		[Activation]
		private void Escalated() {
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> Escalated <CustomerId>");
				return;
			}

			serviceClient.Escalated(customerId);
		}

		[Activation]
		private void GetCashFailed() {
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> GetCashFailed <CustomerId>");
				return;
			}

			serviceClient.GetCashFailed(customerId);
		}

		[Activation]
		private void LoanFullyPaid() {
			int customerId;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> LoanFullyPaid <CustomerId> <loanRefNum>");
				return;
			}

			serviceClient.LoanFullyPaid(customerId, args[2]);
		}

		[Activation]
		private void MoreAmlAndBwaInformation() {
			int underwriterId;
			int customerId;
			if (args.Length != 3 || !int.TryParse(args[1], out underwriterId) || !int.TryParse(args[2], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> MoreAmlAndBwaInformation <Underwriter ID> <CustomerId>");
				return;
			}

			serviceClient.MoreAmlAndBwaInformation(underwriterId, customerId);
		}

		[Activation]
		private void MoreAmlInformation() {
			int underwriterId;
			int customerId;
			if (args.Length != 3 || !int.TryParse(args[1], out underwriterId) || !int.TryParse(args[2], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> MoreAmlInformation <Underwriter ID> <CustomerId>");
				return;
			}
			serviceClient.MoreAmlInformation(underwriterId, customerId);
		}

		[Activation]
		private void MoreBwaInformation() {
			int underwriterId;
			int customerId;
			if (args.Length != 3 || !int.TryParse(args[1], out underwriterId) || !int.TryParse(args[2], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> MoreBwaInformation <Underwriter ID> <CustomerId>");
				return;
			}

			serviceClient.MoreBwaInformation(underwriterId, customerId);
		}

		[Activation]
		private void PasswordChanged() {
			int customerId;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> PasswordChanged <CustomerId> <password>");
				return;
			}

			serviceClient.PasswordChanged(customerId, args[2]);
		}

		[Activation]
		private void PasswordRestored() {
			int customerId;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> PasswordRestored <CustomerId> <password>");
				return;
			}

			serviceClient.PasswordRestored(customerId, args[2]);
		}

		[Activation]
		private void PayEarly() {
			int customerId;
			decimal amount;
			if (args.Length != 4 || !int.TryParse(args[1], out customerId) || !decimal.TryParse(args[2], out amount)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> PayEarly <CustomerId> <amount> <loanRefNumber>");
				return;
			}

			serviceClient.PayEarly(customerId, amount, args[3]);
		}

		[Activation]
		private void PayPointAddedByUnderwriter() {
			int customerId, underwriterId;
			if (args.Length != 5 || !int.TryParse(args[1], out customerId) || !int.TryParse(args[4], out underwriterId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> PayPointAddedByUnderwriter <CustomerId> <cardno> <underwriterName> <underwriterId>");
				return;
			}

			serviceClient.PayPointAddedByUnderwriter(customerId, args[2], args[3], underwriterId);
		}

		[Activation]
		private void PayPointNameValidationFailed() {
			int underwriterId;
			int customerId;
			if (args.Length != 4 || !int.TryParse(args[1], out underwriterId) || !int.TryParse(args[2], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> PayPointNameValidationFailed <Underwriter ID> <CustomerId> <cardHodlerName>");
				return;
			}

			serviceClient.PayPointNameValidationFailed(underwriterId, customerId, args[2]);
		}

		[Activation]
		private void RejectUser() {
			int underwriterId;
			int customerId;
			if (args.Length != 3 || !int.TryParse(args[1], out underwriterId) || !int.TryParse(args[2], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> RejectUser <Underwriter ID> <CustomerId>");
				return;
			}

			serviceClient.RejectUser(underwriterId, customerId);
		}

		[Activation]
		private void RenewEbayToken() {
			int customerId;
			if (args.Length != 4 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> RenewEbayToken <CustomerId> <marketplaceName> <eBayAddress>");
				return;
			}

			serviceClient.RenewEbayToken(customerId, args[2], args[3]);
		}

		[Activation]
		private void RequestCashWithoutTakenLoan() {
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> RequestCashWithoutTakenLoan <CustomerId>");
				return;
			}

			serviceClient.RequestCashWithoutTakenLoan(customerId);
		}

		[Activation]
		private void SendEmailVerification() {
			int customerId;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> SendEmailVerification <CustomerId> <address>");
				return;
			}

			serviceClient.SendEmailVerification(customerId, "", args[2]);
		}

		[Activation]
		private void ThreeInvalidAttempts() {
			int customerId;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> ThreeInvalidAttempts <CustomerId> <password>");
				return;
			}

			serviceClient.ThreeInvalidAttempts(customerId, args[2]);
		}

		[Activation]
		private void TransferCashFailed() {
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> TransferCashFailed <CustomerId>");
				return;
			}

			serviceClient.TransferCashFailed(customerId);
		}

		[Activation]
		private void CaisGenerate() {
			int underwriterId;
			if (args.Length != 2 || !int.TryParse(args[1], out underwriterId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> CaisGenerate <underwriterId>");
				return;
			}

			serviceClient.CaisGenerate(underwriterId);
		}

		[Activation]
		private void CaisUpdate() {
			int underwriterId;
			int caisId;
			if (args.Length != 3 || !int.TryParse(args[1], out underwriterId) || !int.TryParse(args[2], out caisId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> CaisUpdate <Underwriter ID> <caisId>");
				return;
			}

			serviceClient.CaisUpdate(underwriterId, caisId);
		}

		[Activation]
		private void FirstOfMonthStatusNotifier() {
			if (args.Length != 1) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> FirstOfMonthStatusNotifier");
				return;
			}

			serviceClient.FirstOfMonthStatusNotifier();
		}

		[Activation]
		private void FraudChecker() {
			int customerId;
			FraudMode mode;

			if (args.Length != 3 || !int.TryParse(args[1], out customerId) || !FraudMode.TryParse(args[2], true, out mode)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> FraudChecker <CustomerId> <Fraud mode>");
				Console.WriteLine("Fraud mode values: {0}", string.Join(", ", Enum.GetValues(typeof(FraudMode))));
				return;
			}

			serviceClient.FraudChecker(customerId, mode);
		}

		[Activation]
		private void LateBy14Days() {
			if (args.Length != 1) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> LateBy14Days");
				return;
			}

			serviceClient.LateBy14Days();
		}

		[Activation]
		private void PayPointCharger() {
			if (args.Length != 1) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> PayPointCharger");
				return;
			}

			serviceClient.PayPointCharger();
		}

		[Activation]
		private void SetLateLoanStatus() {
			if (args.Length != 1) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> SetLateLoanStatus");
				return;
			}

			serviceClient.SetLateLoanStatus();
		}

		[Activation]
		private void CustomerMarketPlaceAdded() {
			int customerId, marketplaceId;
			bool bUpdateWizardStep;
			if (args.Length != 4 || !int.TryParse(args[1], out customerId) || !int.TryParse(args[2], out marketplaceId) || !bool.TryParse(args[3], out bUpdateWizardStep)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> CustomerMarketPlaceAdded <CustomerId> <CustomerMarketplaceId> <Update customer wizard step>");
				Console.WriteLine("<Update customer wizard step>: is boolean and means whether to set customer wizard step to Marketplace or not. If wizard step is 'TheLastOne' it is never changed.");
				return;
			}

			serviceClient.UpdateMarketplace(customerId, marketplaceId, bUpdateWizardStep);
		}

		[Activation]
		private void UpdateTransactionStatus() {
			if (args.Length != 1) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> UpdateTransactionStatus");
				return;
			}

			serviceClient.UpdateTransactionStatus();
		}

		[Activation]
		private void XDaysDue() {
			if (args.Length != 1) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> XDaysDue");
				return;
			}

			serviceClient.XDaysDue();
		}

		[Activation]
		private void MainStrategy() {
			int underwriterId;
			int customerId, avoidAutoDescison;
			NewCreditLineOption newCreditLineOption;

			switch (args.Length) {
			case 5:
				if (int.TryParse(args[1], out underwriterId) && int.TryParse(args[2], out customerId) && Enum.TryParse(args[3], out newCreditLineOption) && int.TryParse(args[4], out avoidAutoDescison)) {
					serviceClient.MainStrategy1(underwriterId, customerId, newCreditLineOption, avoidAutoDescison);
					return;
				}
				break;

			case 6:
				bool isUnderwriterForced;
				if (int.TryParse(args[1], out underwriterId) && int.TryParse(args[2], out customerId) && Enum.TryParse(args[3], out newCreditLineOption) && int.TryParse(args[4], out avoidAutoDescison) && bool.TryParse(args[5], out isUnderwriterForced)) {
					serviceClient.MainStrategy2(underwriterId, customerId, newCreditLineOption, avoidAutoDescison, isUnderwriterForced);
					return;
				}
				break;
			} // switch

			Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> MainStrategy <Underwriter ID> <customerId> <newCreditLineOption> <avoidAutoDescison>");
			Console.WriteLine("OR");
			Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> MainStrategy <Underwriter ID> <customerId> <newCreditLineOption> <avoidAutoDescison> <isUnderwriterForced(should always be true)>");
		}

		[Activation]
		private void MainStrategySync() {
			int underwriterId;
			int customerId, avoidAutoDescison;
			NewCreditLineOption newCreditLineOption;
			if (args.Length == 5 && int.TryParse(args[1], out underwriterId) && int.TryParse(args[2], out customerId) && Enum.TryParse(args[3], out newCreditLineOption) && int.TryParse(args[4], out avoidAutoDescison)) {
				serviceClient.MainStrategySync1(underwriterId, customerId, newCreditLineOption, avoidAutoDescison);
				return;
			}

			Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> MainStrategySync <Underwriter ID> <customerId> <newCreditLineOption> <avoidAutoDescison>");
		}

		[Activation]
		private void UpdateCurrencyRates() {
			if (args.Length != 1) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> UpdateCurrencyRates");
				return;
			}

			serviceClient.UpdateCurrencyRates();
		}

		[Activation]
		private void CheckExperianCompany() {
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> CheckExperianCompany <CustomerId>");
				return;
			}

			serviceClient.CheckExperianCompany(customerId);
		}

		[Activation]
		private void CheckExperianConsumer() {
			int customerId, directorId;

			if (args.Length != 3 || !int.TryParse(args[1], out customerId) || !int.TryParse(args[2], out directorId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> CheckExperianConsumer <CustomerId> <DirectorId>");
				return;
			}

			serviceClient.CheckExperianConsumer(customerId, directorId);
		}

		[Activation]
		private void AmlChecker() {
			int customerId;

			if (args.Length == 2 && int.TryParse(args[1], out customerId)) {
				serviceClient.CheckAml(customerId);
				return;
			}

			if (args.Length == 9 && int.TryParse(args[1], out customerId)) {
				serviceClient.CheckAmlCustom(customerId, args[2], args[3], args[4], args[5], args[6], args[7], args[8]);
				return;
			}

			Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> AmlChecker <CustomerId>");
			Console.WriteLine("OR");
			Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> AmlChecker <CustomerId> <idhubHouseNumber> <idhubHouseName> <idhubStreet> <idhubDistrict> <idhubTown> <idhubCounty> <idhubPostCode>");

		}

		[Activation]
		private void BwaChecker() {
			int customerId;

			if (args.Length == 2 && int.TryParse(args[1], out customerId)) {
				serviceClient.CheckBwa(customerId);
				return;
			}

			if (args.Length == 11 && int.TryParse(args[1], out customerId)) {
				serviceClient.CheckBwaCustom(customerId, args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10]);
				return;
			}

			Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> BwaChecker <CustomerId>");
			Console.WriteLine("OR");
			Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> BwaChecker <CustomerId> <idhubHouseNumber> <idhubHouseName> <idhubStreet> <idhubDistrict> <idhubTown> <idhubCounty> <idhubPostCode> <idhubBranchCode> <idhubAccountNumber>");
		}

		[Activation]
		private void FinishWizard() {
			int customerId, underwriterId;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId) || !int.TryParse(args[2], out underwriterId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> FinishWizard <CustomerId> <UnderwriterId>");
				return;
			}

			serviceClient.FinishWizard(customerId, underwriterId);
		}

		[Activation]
		private void GenerateMobileCode() {
			if (args.Length != 2) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> GenerateMobileCode <phone number>");
				return;
			}

			serviceClient.GenerateMobileCode(args[1]);
		}

		[Activation]
		private void GetSpResultTable() {
			if (args.Length < 2 || args.Length % 2 != 0) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> GetSpResultTable <spName> <parameters - should come in couples>");
				return;
			}

			string spName = args[1];
			var parameterList = new List<string>();
			for (int i = 2; i < args.Length; i++) {
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
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> CreateUnderwriter <Name> <Password> <RoleName>");
				return;
			}

			serviceClient.CreateUnderwriter(args[1], args[2], args[3]);
		}

		[Activation]
		private void BrokerLoadCustomerList() {
			if (args.Length != 2) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> BrokerLoadCustomerList <Contact person email>");
				return;
			} // if

			BrokerCustomersActionResult res = serviceClient.BrokerLoadCustomerList(args[1]);

			foreach (var oEntry in res.Customers)
				Console.WriteLine("Customer ID: {0} Name: {1} {2}", oEntry.CustomerID, oEntry.FirstName, oEntry.LastName);
		} // BrokerLoadCustomerList

		[Activation]
		private void ListActiveActions() {
			StringListActionResult res = adminClient.ListActiveActions();

			Console.WriteLine(
				"\nRetriever (i.e. this action):\n\t{{ {0}: {4} [{1}sync] {2}: {3} }}",
				res.MetaData.ActionID,
				res.MetaData.IsSynchronous ? "" : "a",
				res.MetaData.Status,
				res.MetaData.Comment ?? "-- no comments --",
				res.MetaData.Name
			);

			string sKey = "{ " + res.MetaData.ActionID;

			Console.WriteLine("\nList of active actions - begin:\n");

			foreach (string s in res.Records)
				if (!s.StartsWith(sKey))
					Console.WriteLine("\t{0}\n", s);

			Console.WriteLine("\nList of active actions - end.");
		} // ListActiveActions

		[Activation]
		private void TerminateAction() {
			if (args.Length != 2) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> TerminateAction <action guid>");

				Console.WriteLine(@"
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
	}
}
