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
			oTcpBinding.Security.Mode = SecurityMode.None;
			oTcpBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.None;

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
			if (args.Length != 4 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> SendEmailVerification <CustomerId> <email> <address>");
				return;
			}

			serviceClient.SendEmailVerification(customerId, args[2], args[3]);
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
			if (args.Length != 2 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> FraudChecker <CustomerId>");
				return;
			}

			serviceClient.FraudChecker(customerId);
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
			if (args.Length != 3 || !int.TryParse(args[1], out customerId) || !int.TryParse(args[2], out marketplaceId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> CustomerMarketPlaceAdded <CustomerId> <CustomerMarketplaceId>");
				return;
			}

			serviceClient.UpdateMarketplace(customerId, marketplaceId);
		}

		[Activation]
		private void UpdateAllMarketplaces() {
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId)) {
				Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> UpdateAllMarketplaces <CustomerId>");
				return;
			}

			serviceClient.UpdateAllMarketplaces(customerId);
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

			case  6:
				bool isUnderwriterForced;
				if (int.TryParse(args[1], out underwriterId) && int.TryParse(args[2], out customerId) && Enum.TryParse(args[3], out newCreditLineOption) && int.TryParse(args[4], out avoidAutoDescison) && bool.TryParse(args[5], out isUnderwriterForced)) {
					serviceClient.MainStrategy2(underwriterId, customerId, newCreditLineOption, avoidAutoDescison, isUnderwriterForced);
					return;
				}
				break;

			case 15:
				int checkType;
				if (int.TryParse(args[1], out underwriterId) && int.TryParse(args[2], out customerId) && int.TryParse(args[3], out checkType) && int.TryParse(args[13], out avoidAutoDescison)) {
					serviceClient.MainStrategy3(underwriterId, customerId, checkType, args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], avoidAutoDescison);
					return;
				}
				break;
			} // switch

			Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> MainStrategy <Underwriter ID> <customerId> <newCreditLineOption> <avoidAutoDescison>");
			Console.WriteLine("OR");
			Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> MainStrategy <Underwriter ID> <customerId> <newCreditLineOption> <avoidAutoDescison> <isUnderwriterForced(should always be true)>");
			Console.WriteLine("OR");
			Console.WriteLine("Usage: StrategiesActivator.exe <Service Instance Name> MainStrategy <Underwriter ID> <customerId> <checkType> <houseNumber> <houseName> <street> <district> <town> <county> <postcode> <bankAccount> <sortCode> <avoidAutoDescison>");
		}

// ReSharper restore UnusedMember.Local
		#endregion strategy activators
	}
}
