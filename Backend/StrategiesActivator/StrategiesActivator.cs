namespace StrategiesActivator {
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.ServiceModel;
	using EzServiceConfigurationLoader;
	using System;
	using EzBob.Backend.Strategies;
	using EzServiceReference;
	using Ezbob.Database;
	using Ezbob.Logger;
	using log4net;

	public class StrategiesActivator {
		private readonly string[] args;
		private readonly EzServiceClient serviceClient;

		public StrategiesActivator(string[] args) {
			this.args = new string[args.Length - 1];
			Array.Copy(args, 1, this.args, 0, args.Length - 1);

			string sInstanceName = args[0];

			ASafeLog log = new SafeILog(LogManager.GetLogger(typeof(StrategiesActivator)));

			var env = new Ezbob.Context.Environment(log);
			AConnection db = new SqlConnection(env, log);

			var cfg = new Configuration(sInstanceName, db, log);
			cfg.Init();

			var oTcpBinding = new NetTcpBinding();
			oTcpBinding.Security.Mode = SecurityMode.Transport;
			oTcpBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;

			serviceClient = new EzServiceClient(
				oTcpBinding,
				new EndpointAddress(cfg.AdminEndpointAddress)
			);
		}

		public void Execute() {
			string strategyName = args[0];

			MethodInfo[] aryMethods = GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);

			var oMethods = new SortedDictionary<string, MethodInfo>();

			foreach (MethodInfo mi in aryMethods) {
				IEnumerable<StrategyActivatorAttribute> oAttrList = mi.GetCustomAttributes<StrategyActivatorAttribute>();

				if (oAttrList.Any()) {
					oMethods[mi.Name] = mi;
				}
			} // foreach

			string sKey = "Activate" + strategyName;

			if (oMethods.ContainsKey(sKey)) {
				oMethods[sKey].Invoke(this, new object[] { });
			}
			else {
				Console.WriteLine("Strategy {0} is not supported", strategyName);
				Console.WriteLine("Supported stratefies are: {0}", string.Join(", ", oMethods.Keys.Select(k => k.Substring(8))));
			}
		} // Execute

		[StrategyActivator]
		private void ActivateGreeting() {
			int customerId;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> Greeting <CustomerId> <ConfirmEmailAddress>");
				return;
			}

			serviceClient.GreetingMailStrategy(customerId, args[2]);
		}

		[StrategyActivator]
		private void ActivateApprovedUser() {
			int customerId;
			decimal loanAmount;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId) || !decimal.TryParse(args[2], out loanAmount)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> ApprovedUser <CustomerId> <loanAmount>");
				return;
			}

			serviceClient.ApprovedUser(customerId, loanAmount);
		}

		[StrategyActivator]
		private void ActivateCashTransferred() {
			int customerId;
			decimal amount;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId) || !decimal.TryParse(args[2], out amount)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> CashTransferred <CustomerId> <amount>");
				return;
			}

			serviceClient.CashTransferred(customerId, amount);
		}

		[StrategyActivator]
		private void ActivateEmailRolloverAdded() {
			int customerId;
			decimal amount;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId) || !decimal.TryParse(args[2], out amount)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> EmailRolloverAdded <CustomerId> <amount>");
				return;
			}

			serviceClient.EmailRolloverAdded(customerId, amount);
		}

		[StrategyActivator]
		private void ActivateEmailUnderReview() {
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> EmailUnderReview <CustomerId>");
				return;
			}

			serviceClient.EmailUnderReview(customerId);
		}

		[StrategyActivator]
		private void ActivateEscalated() {
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> Escalated <CustomerId>");
				return;
			}

			serviceClient.Escalated(customerId);
		}

		[StrategyActivator]
		private void ActivateGetCashFailed() {
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> GetCashFailed <CustomerId>");
				return;
			}

			serviceClient.GetCashFailed(customerId);
		}

		[StrategyActivator]
		private void ActivateLoanFullyPaid() {
			int customerId;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> LoanFullyPaid <CustomerId> <loanRefNum>");
				return;
			}

			serviceClient.LoanFullyPaid(customerId, args[2]);
		}

		[StrategyActivator]
		private void ActivateMoreAmlAndBwaInformation() {
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> MoreAmlAndBwaInformation <CustomerId>");
				return;
			}

			serviceClient.MoreAmlAndBwaInformation(customerId);
		}

		[StrategyActivator]
		private void ActivateMoreAmlInformation() {
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> MoreAmlInformation <CustomerId>");
				return;
			}
			serviceClient.MoreAmlInformation(customerId);
		}

		[StrategyActivator]
		private void ActivateMoreBwaInformation() {
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> MoreBwaInformation <CustomerId>");
				return;
			}

			serviceClient.MoreBwaInformation(customerId);
		}

		[StrategyActivator]
		private void ActivatePasswordChanged() {
			int customerId;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> PasswordChanged <CustomerId> <password>");
				return;
			}

			serviceClient.PasswordChanged(customerId, args[2]);
		}

		[StrategyActivator]
		private void ActivatePasswordRestored() {
			int customerId;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> PasswordRestored <CustomerId> <password>");
				return;
			}

			serviceClient.PasswordRestored(customerId, args[2]);
		}

		[StrategyActivator]
		private void ActivatePayEarly() {
			int customerId;
			decimal amount;
			if (args.Length != 4 || !int.TryParse(args[1], out customerId) || !decimal.TryParse(args[2], out amount)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> PayEarly <CustomerId> <amount> <loanRefNumber>");
				return;
			}

			serviceClient.PayEarly(customerId, amount, args[3]);
		}

		[StrategyActivator]
		private void ActivatePayPointAddedByUnderwriter() {
			int customerId, underwriterId;
			if (args.Length != 5 || !int.TryParse(args[1], out customerId) || !int.TryParse(args[4], out underwriterId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> PayPointAddedByUnderwriter <CustomerId> <cardno> <underwriterName> <underwriterId>");
				return;
			}

			serviceClient.PayPointAddedByUnderwriter(customerId, args[2], args[3], underwriterId);
		}

		[StrategyActivator]
		private void ActivatePayPointNameValidationFailed() {
			int customerId;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> PayPointNameValidationFailed <CustomerId> <cardHodlerName>");
				return;
			}

			serviceClient.PayPointNameValidationFailed(customerId, args[2]);
		}

		[StrategyActivator]
		private void ActivateRejectUser() {
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> RejectUser <CustomerId>");
				return;
			}

			serviceClient.RejectUser(customerId);
		}

		[StrategyActivator]
		private void ActivateRenewEbayToken() {
			int customerId;
			if (args.Length != 4 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> RenewEbayToken <CustomerId> <marketplaceName> <eBayAddress>");
				return;
			}

			serviceClient.RenewEbayToken(customerId, args[2], args[3]);
		}

		[StrategyActivator]
		private void ActivateRequestCashWithoutTakenLoan() {
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> RequestCashWithoutTakenLoan <CustomerId>");
				return;
			}

			serviceClient.RequestCashWithoutTakenLoan(customerId);
		}

		[StrategyActivator]
		private void ActivateSendEmailVerification() {
			int customerId;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> SendEmailVerification <CustomerId> <address>");
				return;
			}

			serviceClient.SendEmailVerification(customerId, args[2]);
		}

		[StrategyActivator]
		private void ActivateThreeInvalidAttempts() {
			int customerId;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> ThreeInvalidAttempts <CustomerId> <password>");
				return;
			}

			serviceClient.ThreeInvalidAttempts(customerId, args[2]);
		}

		[StrategyActivator]
		private void ActivateTransferCashFailed() {
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> TransferCashFailed <CustomerId>");
				return;
			}

			serviceClient.TransferCashFailed(customerId);
		}

		[StrategyActivator]
		private void ActivateCaisGenerate() {
			int underwriterId;
			if (args.Length != 2 || !int.TryParse(args[1], out underwriterId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> CaisGenerate <underwriterId>");
				return;
			}

			serviceClient.CaisGenerate(underwriterId);
		}

		[StrategyActivator]
		private void ActivateCaisUpdate() {
			int caisId;
			if (args.Length != 2 || !int.TryParse(args[1], out caisId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> CaisUpdate <caisId>");
				return;
			}

			serviceClient.CaisUpdate(caisId);
		}

		[StrategyActivator]
		private void ActivateFirstOfMonthStatusNotifier() {
			if (args.Length != 1) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> FirstOfMonthStatusNotifier");
				return;
			}

			serviceClient.FirstOfMonthStatusNotifier();
		}

		[StrategyActivator]
		private void ActivateFraudChecker() {
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> FraudChecker <CustomerId>");
				return;
			}

			serviceClient.FraudChecker(customerId);
		}

		[StrategyActivator]
		private void ActivateLateBy14Days() {
			if (args.Length != 1) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> LateBy14Days");
				return;
			}

			serviceClient.LateBy14Days();
		}

		[StrategyActivator]
		private void ActivatePayPointCharger() {
			if (args.Length != 1) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> PayPointCharger");
				return;
			}

			serviceClient.PayPointCharger();
		}

		[StrategyActivator]
		private void ActivateSetLateLoanStatus() {
			if (args.Length != 1) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> SetLateLoanStatus");
				return;
			}

			serviceClient.SetLateLoanStatus();
		}

		[StrategyActivator]
		private void ActivateCustomerMarketPlaceAdded() {
			int customerId, marketplaceId;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId) || !int.TryParse(args[2], out marketplaceId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> CustomerMarketPlaceAdded <CustomerId> <CustomerMarketplaceId>");
				return;
			}

			serviceClient.UpdateMarketplace(customerId, marketplaceId);
		}

		[StrategyActivator]
		private void ActivateUpdateAllMarketplaces() {
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> UpdateAllMarketplaces <CustomerId>");
				return;
			}

			serviceClient.UpdateAllMarketplaces(customerId);
		}

		[StrategyActivator]
		private void ActivateUpdateTransactionStatus() {
			if (args.Length != 1) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> UpdateTransactionStatus");
				return;
			}

			serviceClient.UpdateTransactionStatus();
		}

		[StrategyActivator]
		private void ActivateXDaysDue() {
			if (args.Length != 1) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> XDaysDue");
				return;
			}

			serviceClient.XDaysDue();
		}

		[StrategyActivator]
		private void ActivateMainStrategy() {
			int customerId, avoidAutoDescison;
			NewCreditLineOption newCreditLineOption;
			if (args.Length == 4) {
				if (int.TryParse(args[1], out customerId) && Enum.TryParse(args[2], out newCreditLineOption) && int.TryParse(args[3], out avoidAutoDescison)) {
					serviceClient.MainStrategy1(customerId, newCreditLineOption, avoidAutoDescison);
					return;
				}
			}
			else if (args.Length == 5) {
				bool isUnderwriterForced;
				if (int.TryParse(args[1], out customerId) && Enum.TryParse(args[2], out newCreditLineOption) && int.TryParse(args[3], out avoidAutoDescison) && bool.TryParse(args[4], out isUnderwriterForced)) {
					serviceClient.MainStrategy2(customerId, newCreditLineOption, avoidAutoDescison, isUnderwriterForced);
					return;
				}
			}
			else if (args.Length == 14) {
				int checkType;
				if (int.TryParse(args[1], out customerId) && int.TryParse(args[2], out checkType) && int.TryParse(args[12], out avoidAutoDescison)) {
					serviceClient.MainStrategy3(customerId, checkType, args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], avoidAutoDescison);
					return;
				}
			}

			Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> MainStrategy <customerId> <newCreditLineOption> <avoidAutoDescison>");
			Console.WriteLine("OR");
			Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> MainStrategy <customerId> <newCreditLineOption> <avoidAutoDescison> <isUnderwriterForced(should always be true)>");
			Console.WriteLine("OR");
			Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> MainStrategy <customerId> <checkType> <houseNumber> <houseName> <street> <district> <town> <county> <postcode> <bankAccount> <sortCode> <avoidAutoDescison>");
		}
	}
}
