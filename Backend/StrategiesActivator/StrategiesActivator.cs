﻿namespace StrategiesActivator {
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

		[StrategyActivator]
		private void Greeting() {
			int customerId;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> Greeting <CustomerId> <ConfirmEmailAddress>");
				return;
			}

			serviceClient.GreetingMailStrategy(customerId, args[2]);
		}

		[StrategyActivator]
		private void ApprovedUser() {
			int customerId;
			decimal loanAmount;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId) || !decimal.TryParse(args[2], out loanAmount)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> ApprovedUser <CustomerId> <loanAmount>");
				return;
			}

			serviceClient.ApprovedUser(customerId, loanAmount);
		}

		[StrategyActivator]
		private void CashTransferred() {
			int customerId;
			decimal amount;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId) || !decimal.TryParse(args[2], out amount)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> CashTransferred <CustomerId> <amount>");
				return;
			}

			serviceClient.CashTransferred(customerId, amount);
		}

		[StrategyActivator]
		private void EmailRolloverAdded() {
			int customerId;
			decimal amount;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId) || !decimal.TryParse(args[2], out amount)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> EmailRolloverAdded <CustomerId> <amount>");
				return;
			}

			serviceClient.EmailRolloverAdded(customerId, amount);
		}

		[StrategyActivator]
		private void EmailUnderReview() {
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> EmailUnderReview <CustomerId>");
				return;
			}

			serviceClient.EmailUnderReview(customerId);
		}

		[StrategyActivator]
		private void Escalated() {
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> Escalated <CustomerId>");
				return;
			}

			serviceClient.Escalated(customerId);
		}

		[StrategyActivator]
		private void GetCashFailed() {
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> GetCashFailed <CustomerId>");
				return;
			}

			serviceClient.GetCashFailed(customerId);
		}

		[StrategyActivator]
		private void LoanFullyPaid() {
			int customerId;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> LoanFullyPaid <CustomerId> <loanRefNum>");
				return;
			}

			serviceClient.LoanFullyPaid(customerId, args[2]);
		}

		[StrategyActivator]
		private void MoreAmlAndBwaInformation() {
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> MoreAmlAndBwaInformation <CustomerId>");
				return;
			}

			serviceClient.MoreAmlAndBwaInformation(customerId);
		}

		[StrategyActivator]
		private void MoreAmlInformation() {
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> MoreAmlInformation <CustomerId>");
				return;
			}
			serviceClient.MoreAmlInformation(customerId);
		}

		[StrategyActivator]
		private void MoreBwaInformation() {
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> MoreBwaInformation <CustomerId>");
				return;
			}

			serviceClient.MoreBwaInformation(customerId);
		}

		[StrategyActivator]
		private void PasswordChanged() {
			int customerId;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> PasswordChanged <CustomerId> <password>");
				return;
			}

			serviceClient.PasswordChanged(customerId, args[2]);
		}

		[StrategyActivator]
		private void PasswordRestored() {
			int customerId;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> PasswordRestored <CustomerId> <password>");
				return;
			}

			serviceClient.PasswordRestored(customerId, args[2]);
		}

		[StrategyActivator]
		private void PayEarly() {
			int customerId;
			decimal amount;
			if (args.Length != 4 || !int.TryParse(args[1], out customerId) || !decimal.TryParse(args[2], out amount)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> PayEarly <CustomerId> <amount> <loanRefNumber>");
				return;
			}

			serviceClient.PayEarly(customerId, amount, args[3]);
		}

		[StrategyActivator]
		private void PayPointAddedByUnderwriter() {
			int customerId, underwriterId;
			if (args.Length != 5 || !int.TryParse(args[1], out customerId) || !int.TryParse(args[4], out underwriterId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> PayPointAddedByUnderwriter <CustomerId> <cardno> <underwriterName> <underwriterId>");
				return;
			}

			serviceClient.PayPointAddedByUnderwriter(customerId, args[2], args[3], underwriterId);
		}

		[StrategyActivator]
		private void PayPointNameValidationFailed() {
			int customerId;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> PayPointNameValidationFailed <CustomerId> <cardHodlerName>");
				return;
			}

			serviceClient.PayPointNameValidationFailed(customerId, args[2]);
		}

		[StrategyActivator]
		private void RejectUser() {
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> RejectUser <CustomerId>");
				return;
			}

			serviceClient.RejectUser(customerId);
		}

		[StrategyActivator]
		private void RenewEbayToken() {
			int customerId;
			if (args.Length != 4 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> RenewEbayToken <CustomerId> <marketplaceName> <eBayAddress>");
				return;
			}

			serviceClient.RenewEbayToken(customerId, args[2], args[3]);
		}

		[StrategyActivator]
		private void RequestCashWithoutTakenLoan() {
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> RequestCashWithoutTakenLoan <CustomerId>");
				return;
			}

			serviceClient.RequestCashWithoutTakenLoan(customerId);
		}

		[StrategyActivator]
		private void SendEmailVerification() {
			int customerId;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> SendEmailVerification <CustomerId> <address>");
				return;
			}

			serviceClient.SendEmailVerification(customerId, args[2]);
		}

		[StrategyActivator]
		private void ThreeInvalidAttempts() {
			int customerId;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> ThreeInvalidAttempts <CustomerId> <password>");
				return;
			}

			serviceClient.ThreeInvalidAttempts(customerId, args[2]);
		}

		[StrategyActivator]
		private void TransferCashFailed() {
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> TransferCashFailed <CustomerId>");
				return;
			}

			serviceClient.TransferCashFailed(customerId);
		}

		[StrategyActivator]
		private void CaisGenerate() {
			int underwriterId;
			if (args.Length != 2 || !int.TryParse(args[1], out underwriterId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> CaisGenerate <underwriterId>");
				return;
			}

			serviceClient.CaisGenerate(underwriterId);
		}

		[StrategyActivator]
		private void CaisUpdate() {
			int caisId;
			if (args.Length != 2 || !int.TryParse(args[1], out caisId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> CaisUpdate <caisId>");
				return;
			}

			serviceClient.CaisUpdate(caisId);
		}

		[StrategyActivator]
		private void FirstOfMonthStatusNotifier() {
			if (args.Length != 1) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> FirstOfMonthStatusNotifier");
				return;
			}

			serviceClient.FirstOfMonthStatusNotifier();
		}

		[StrategyActivator]
		private void FraudChecker() {
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> FraudChecker <CustomerId>");
				return;
			}

			serviceClient.FraudChecker(customerId);
		}

		[StrategyActivator]
		private void LateBy14Days() {
			if (args.Length != 1) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> LateBy14Days");
				return;
			}

			serviceClient.LateBy14Days();
		}

		[StrategyActivator]
		private void PayPointCharger() {
			if (args.Length != 1) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> PayPointCharger");
				return;
			}

			serviceClient.PayPointCharger();
		}

		[StrategyActivator]
		private void SetLateLoanStatus() {
			if (args.Length != 1) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> SetLateLoanStatus");
				return;
			}

			serviceClient.SetLateLoanStatus();
		}

		[StrategyActivator]
		private void CustomerMarketPlaceAdded() {
			int customerId, marketplaceId;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId) || !int.TryParse(args[2], out marketplaceId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> CustomerMarketPlaceAdded <CustomerId> <CustomerMarketplaceId>");
				return;
			}

			serviceClient.UpdateMarketplace(customerId, marketplaceId);
		}

		[StrategyActivator]
		private void UpdateAllMarketplaces() {
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> UpdateAllMarketplaces <CustomerId>");
				return;
			}

			serviceClient.UpdateAllMarketplaces(customerId);
		}

		[StrategyActivator]
		private void UpdateTransactionStatus() {
			if (args.Length != 1) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> UpdateTransactionStatus");
				return;
			}

			serviceClient.UpdateTransactionStatus();
		}

		[StrategyActivator]
		private void XDaysDue() {
			if (args.Length != 1) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> XDaysDue");
				return;
			}

			serviceClient.XDaysDue();
		}

		[StrategyActivator]
		private void MainStrategy() {
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

		#endregion strategy activators
	}
}
