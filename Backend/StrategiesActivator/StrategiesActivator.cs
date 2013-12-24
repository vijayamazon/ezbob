namespace StrategiesActivator
{
	using System.ServiceModel;
	using EzService;
	using System;
	using EzBob.Backend.Strategies;
	using EzServiceReference;
	using Ezbob.Database;
	using Ezbob.Logger;
	using log4net;

	public class StrategiesActivator
	{
		private readonly string[] args;
		private readonly EzServiceClient serviceClient;

		public StrategiesActivator(string[] args)
		{
			this.args = new string[args.Length - 1];
			Array.Copy(args, 1, this.args, 0, args.Length - 1);

			string sInstanceName = args[0];

			m_oLog = new SafeILog(LogManager.GetLogger(typeof(StrategiesActivator)));

			var env = new Ezbob.Context.Environment(m_oLog);
			m_oDB = new SqlConnection(env, m_oLog);

			var cfg = new Configuration(sInstanceName, m_oDB, m_oLog);

			serviceClient = new EzServiceClient(
				new NetHttpBinding(),
				new EndpointAddress(cfg.GetClientEndpointAddress())
			);
		}

		public void Execute()
		{
			string strategyName = args[0];
			switch (strategyName)
			{
				case "Greeting":
					ActivateGreeting();
					break;
				case "ApprovedUser":
					ActivateApprovedUser();
					break;
				case "CashTransferred":
					ActivateCashTransferred();
					break;
				case "EmailRolloverAdded":
					ActivateEmailRolloverAdded();
					break;
				case "EmailUnderReview":
					ActivateEmailUnderReview();
					break;
				case "Escalated":
					ActivateEscalated();
					break;
				case "GetCashFailed":
					ActivateGetCashFailed();
					break;
				case "LoanFullyPaid":
					ActivateLoanFullyPaid();
					break;
				case "MoreAMLandBWAInformation":
					ActivateMoreAmLandBwaInformation();
					break;
				case "MoreAMLInformation":
					ActivateMoreAmlInformation();
					break;
				case "MoreBWAInformation":
					ActivateMoreBwaInformation();
					break;
				case "PasswordChanged":
					ActivatePasswordChanged();
					break;
				case "PasswordRestored":
					ActivatePasswordRestored();
					break;
				case "PayEarly":
					ActivatePayEarly();
					break;
				case "PayPointAddedByUnderwriter":
					ActivatePayPointAddedByUnderwriter();
					break;
				case "PayPointNameValidationFailed":
					ActivatePayPointNameValidationFailed();
					break;
				case "RejectUser":
					ActivateRejectUser();
					break;
				case "RenewEbayToken":
					ActivateRenewEbayToken();
					break;
				case "RequestCashWithoutTakenLoan":
					ActivateRequestCashWithoutTakenLoan();
					break;
				case "SendEmailVerification":
					ActivateSendEmailVerification();
					break;
				case "ThreeInvalidAttempts":
					ActivateThreeInvalidAttempts();
					break;
				case "TransferCashFailed":
					ActivateTransferCashFailed();
					break;
				case "CaisGenerate":
					ActivateCaisGenerate();
					break;
				case "CaisUpdate":
					ActivateCaisUpdate();
					break;
				case "FirstOfMonthStatusNotifier":
					ActivateFirstOfMonthStatusNotifier();
					break;
				case "FraudChecker":
					ActivateFraudChecker();
					break;
				case "LateBy14Days":
					ActivateLateBy14Days();
					break;
				case "PayPointCharger":
					ActivatePayPointCharger();
					break;
				case "SetLateLoanStatus":
					ActivateSetLateLoanStatus();
					break;
				case "CustomerMarketPlaceAdded":
					ActivateCustomerMarketPlaceAdded();
					break;
				case "UpdateAllMarketplaces":
					ActivateUpdateAllMarketplaces();
					break;
				case "UpdateTransactionStatus":
					ActivateUpdateTransactionStatus();
					break;
				case "XDaysDue":
					ActivateXDaysDue();
					break;

				default:
					Console.WriteLine("Strategy {0} is not supported", strategyName);
					Console.WriteLine("Supported stratefies are:Greeting, ApprovedUser, CashTransferred, EmailRolloverAdded, EmailUnderReview, Escalated, GetCashFailed, LoanFullyPaid, MoreAMLandBWAInformation, MoreAMLInformation, MoreBWAInformation, PasswordChanged, PasswordRestored, PayEarly, PayPointAddedByUnderwriter, PayPointNameValidationFailed, RejectUser, RenewEbayToken, RequestCashWithoutTakenLoan, SendEmailVerification, ThreeInvalidAttempts, TransferCashFailed, CaisGenerate, CaisUpdate, FirstOfMonthStatusNotifier, FraudChecker, LateBy14Days, PayPointCharger, SetLateLoanStatus, CustomerMarketPlaceAdded, UpdateAllMarketplaces, UpdateTransactionStatus, XDaysDue");
					break;
			}
		}

		private void ActivateGreeting()
		{
			int customerId;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe Greeting <CustomerId> <ConfirmEmailAddress>");
				return;
			}

			serviceClient.GreetingMailStrategy(customerId, args[2]);
		}

		private void ActivateApprovedUser()
		{
			int customerId;
			decimal loanAmount;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId) || !decimal.TryParse(args[2], out loanAmount))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe ApprovedUser <CustomerId> <loanAmount>");
				return;
			}

			serviceClient.ApprovedUser(customerId, loanAmount);
		}

		private void ActivateCashTransferred()
		{
			int customerId;
			decimal amount;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId) || !decimal.TryParse(args[2], out amount))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe CashTransferred <CustomerId> <amount>");
				return;
			}

			serviceClient.CashTransferred(customerId, amount);
		}

		private void ActivateEmailRolloverAdded()
		{
			int customerId;
			decimal amount;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId) || !decimal.TryParse(args[2], out amount))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe EmailRolloverAdded <CustomerId> <amount>");
				return;
			}

			serviceClient.EmailRolloverAdded(customerId, amount);
		}

		private void ActivateEmailUnderReview()
		{
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe EmailUnderReview <CustomerId>");
				return;
			}

			serviceClient.EmailUnderReview(customerId);
		}

		private void ActivateEscalated()
		{
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe Escalated <CustomerId>");
				return;
			}

			serviceClient.Escalated(customerId);
		}

		private void ActivateGetCashFailed()
		{
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe GetCashFailed <CustomerId>");
				return;
			}

			serviceClient.GetCashFailed(customerId);
		}

		private void ActivateLoanFullyPaid()
		{
			int customerId;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe LoanFullyPaid <CustomerId> <loanRefNum>");
				return;
			}

			serviceClient.LoanFullyPaid(customerId, args[2]);
		}

		private void ActivateMoreAmLandBwaInformation()
		{
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe MoreAMLandBWAInformation <CustomerId>");
				return;
			}

			serviceClient.MoreAmLandBwaInformation(customerId);
		}

		private void ActivateMoreAmlInformation()
		{
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe MoreAMLInformation <CustomerId>");
				return;
			}
			serviceClient.MoreAmlInformation(customerId);
		}

		private void ActivateMoreBwaInformation()
		{
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe MoreBWAInformation <CustomerId>");
				return;
			}

			serviceClient.MoreBwaInformation(customerId);
		}

		private void ActivatePasswordChanged()
		{
			int customerId;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe PasswordChanged <CustomerId> <password>");
				return;
			}

			serviceClient.PasswordChanged(customerId, args[2]);
		}

		private void ActivatePasswordRestored()
		{
			int customerId;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe PasswordRestored <CustomerId> <password>");
				return;
			}

			serviceClient.PasswordRestored(customerId, args[2]);
		}

		private void ActivatePayEarly()
		{
			int customerId;
			decimal amount;
			if (args.Length != 4 || !int.TryParse(args[1], out customerId) || !decimal.TryParse(args[2], out amount))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe PayEarly <CustomerId> <amount> <loanRefNumber>");
				return;
			}

			serviceClient.PayEarly(customerId, amount, args[3]);
		}

		private void ActivatePayPointAddedByUnderwriter()
		{
			int customerId, underwriterId;
			if (args.Length != 5 || !int.TryParse(args[1], out customerId) || !int.TryParse(args[4], out underwriterId))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe PayPointAddedByUnderwriter <CustomerId> <cardno> <underwriterName> <underwriterId>");
				return;
			}

			serviceClient.PayPointAddedByUnderwriter(customerId, args[2], args[3], underwriterId);
		}

		private void ActivatePayPointNameValidationFailed()
		{
			int customerId;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe PayPointNameValidationFailed <CustomerId> <cardHodlerName>");
				return;
			}

			serviceClient.PayPointNameValidationFailed(customerId, args[2]);
		}

		private void ActivateRejectUser()
		{
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe RejectUser <CustomerId>");
				return;
			}

			serviceClient.RejectUser(customerId);
		}

		private void ActivateRenewEbayToken()
		{
			int customerId;
			if (args.Length != 4 || !int.TryParse(args[1], out customerId))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe RenewEbayToken <CustomerId> <marketplaceName> <eBayAddress>");
				return;
			}

			serviceClient.RenewEbayToken(customerId, args[2], args[3]);
		}

		private void ActivateRequestCashWithoutTakenLoan()
		{
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe RequestCashWithoutTakenLoan <CustomerId>");
				return;
			}

			serviceClient.RequestCashWithoutTakenLoan(customerId);
		}

		private void ActivateSendEmailVerification()
		{
			int customerId;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe SendEmailVerification <CustomerId> <address>");
				return;
			}

			serviceClient.SendEmailVerification(customerId, args[2]);
		}

		private void ActivateThreeInvalidAttempts()
		{
			int customerId;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe ThreeInvalidAttempts <CustomerId> <password>");
				return;
			}

			serviceClient.ThreeInvalidAttempts(customerId, args[2]);
		}

		private void ActivateTransferCashFailed()
		{
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe TransferCashFailed <CustomerId>");
				return;
			}

			serviceClient.TransferCashFailed(customerId);
		}

		private void ActivateCaisGenerate()
		{
			int underwriterId;
			if (args.Length != 2 || !int.TryParse(args[1], out underwriterId))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe CaisGenerate <underwriterId>");
				return;
			}

			serviceClient.CaisGenerate(underwriterId);
		}

		private void ActivateCaisUpdate()
		{
			int caisId;
			if (args.Length != 2 || !int.TryParse(args[1], out caisId))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe CaisUpdate <caisId>");
				return;
			}

			serviceClient.CaisUpdate(caisId);
		}

		private void ActivateFirstOfMonthStatusNotifier()
		{
			if (args.Length != 1)
			{
				Console.WriteLine("Usage: StrategiesActivator.exe FirstOfMonthStatusNotifier");
				return;
			}
			new FirstOfMonthStatusNotifier(m_oDB, m_oLog).Execute();
		}

		private void ActivateFraudChecker()
		{
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe FraudChecker <CustomerId>");
				return;
			}

			serviceClient.FraudChecker(customerId);
		}

		private void ActivateLateBy14Days()
		{
			if (args.Length != 1)
			{
				Console.WriteLine("Usage: StrategiesActivator.exe LateBy14Days");
				return;
			}
			new LateBy14Days(m_oDB, m_oLog).Execute();
		}

		private void ActivatePayPointCharger()
		{
			if (args.Length != 1)
			{
				Console.WriteLine("Usage: StrategiesActivator.exe PayPointCharger");
				return;
			}
			new PayPointCharger(m_oDB, m_oLog).Execute();
		}

		private void ActivateSetLateLoanStatus()
		{
			if (args.Length != 1)
			{
				Console.WriteLine("Usage: StrategiesActivator.exe SetLateLoanStatus");
				return;
			}
			new SetLateLoanStatus(m_oDB, m_oLog).Execute();
		}

		private void ActivateCustomerMarketPlaceAdded()
		{
			int customerId, marketplaceId;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId) || !int.TryParse(args[2], out marketplaceId))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe CustomerMarketPlaceAdded <CustomerId> <marketplaceId>");
				return;
			}

			serviceClient.UpdateMarketplace(customerId, marketplaceId);
		}

		private void ActivateUpdateAllMarketplaces()
		{
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe UpdateAllMarketplaces <CustomerId>");
				return;
			}

			serviceClient.UpdateAllMarketplaces(customerId);
		}

		private void ActivateUpdateTransactionStatus()
		{
			if (args.Length != 1)
			{
				Console.WriteLine("Usage: StrategiesActivator.exe UpdateTransactionStatus");
				return;
			}
			new UpdateTransactionStatus(m_oDB, m_oLog).Execute();
		}

		private void ActivateXDaysDue()
		{
			if (args.Length != 1)
			{
				Console.WriteLine("Usage: StrategiesActivator.exe XDaysDue");
				return;
			}
			new XDaysDue(m_oDB, m_oLog).Execute();
		}

		private readonly AConnection m_oDB;
		private readonly ASafeLog m_oLog;
	}
}
