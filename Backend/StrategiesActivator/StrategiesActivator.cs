namespace StrategiesActivator
{
	using System;
	using EzBob.Backend.Strategies;
	using EzBob.Backend.Strategies.MailStrategies;

	public class StrategiesActivator
	{
		private readonly string[] args;

		public StrategiesActivator(string[] args)
		{
			this.args = args;
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
			new Greeting(customerId, args[2]).Execute();
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
			new ApprovedUser(customerId, loanAmount).Execute();
		}

		private void ActivateCashTransferred()
		{
			int customerId;
			int amount;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId) || !int.TryParse(args[2], out amount))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe CashTransferred <CustomerId> <amount>");
				return;
			}
			new CashTransferred(customerId, amount).Execute();
		}

		private void ActivateEmailRolloverAdded()
		{
			int customerId;
			int amount;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId) || !int.TryParse(args[2], out amount))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe EmailRolloverAdded <CustomerId> <amount>");
				return;
			}
			new EmailRolloverAdded(customerId, amount).Execute();
		}

		private void ActivateEmailUnderReview()
		{
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe EmailUnderReview <CustomerId>");
				return;
			}
			new EmailUnderReview(customerId).Execute();
		}

		private void ActivateEscalated()
		{
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe Escalated <CustomerId>");
				return;
			}
			new Escalated(customerId).Execute();
		}

		private void ActivateGetCashFailed()
		{
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe GetCashFailed <CustomerId>");
				return;
			}
			new GetCashFailed(customerId).Execute();
		}

		private void ActivateLoanFullyPaid()
		{
			int customerId;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe LoanFullyPaid <CustomerId> <loanRefNum>");
				return;
			}
			new LoanFullyPaid(customerId, args[2]).Execute();
		}

		private void ActivateMoreAmLandBwaInformation()
		{
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe MoreAMLandBWAInformation <CustomerId>");
				return;
			}
			new MoreAMLandBWAInformation(customerId).Execute();
		}

		private void ActivateMoreAmlInformation()
		{
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe MoreAMLInformation <CustomerId>");
				return;
			}
			new MoreAMLInformation(customerId).Execute();
		}

		private void ActivateMoreBwaInformation()
		{
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe MoreBWAInformation <CustomerId>");
				return;
			}
			new MoreBWAInformation(customerId).Execute();
		}

		private void ActivatePasswordChanged()
		{
			int customerId;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe PasswordChanged <CustomerId> <password>");
				return;
			}
			new PasswordChanged(customerId, args[2]).Execute();
		}

		private void ActivatePasswordRestored()
		{
			int customerId;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe PasswordRestored <CustomerId> <password>");
				return;
			}
			new PasswordRestored(customerId, args[2]).Execute();
		}

		private void ActivatePayEarly()
		{
			int customerId, amount;
			if (args.Length != 4 || !int.TryParse(args[1], out customerId) || !int.TryParse(args[2], out amount))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe PayEarly <CustomerId> <amount> <loanRefNumber>");
				return;
			}
			new PayEarly(customerId, amount, args[3]).Execute();
		}

		private void ActivatePayPointAddedByUnderwriter()
		{
			int customerId, underwriterId;
			if (args.Length != 5 || !int.TryParse(args[1], out customerId) || !int.TryParse(args[4], out underwriterId))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe PayPointAddedByUnderwriter <CustomerId> <cardno> <underwriterName> <underwriterId>");
				return;
			}
			new PayPointAddedByUnderwriter(customerId, args[2], args[3], underwriterId).Execute();
		}

		private void ActivatePayPointNameValidationFailed()
		{
			int customerId;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe PayPointNameValidationFailed <CustomerId> <cardHodlerName>");
				return;
			}
			new PayPointNameValidationFailed(customerId, args[2]).Execute();
		}

		private void ActivateRejectUser()
		{
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe RejectUser <CustomerId>");
				return;
			}
			new RejectUser(customerId).Execute();
		}

		private void ActivateRenewEbayToken()
		{
			int customerId;
			if (args.Length != 4 || !int.TryParse(args[1], out customerId))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe RenewEbayToken <CustomerId> <marketplaceName> <eBayAddress>");
				return;
			}
			new RenewEbayToken(customerId, args[2], args[3]).Execute();
		}

		private void ActivateRequestCashWithoutTakenLoan()
		{
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe RequestCashWithoutTakenLoan <CustomerId>");
				return;
			}
			new RequestCashWithoutTakenLoan(customerId).Execute();
		}

		private void ActivateSendEmailVerification()
		{
			int customerId;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe SendEmailVerification <CustomerId> <address>");
				return;
			}
			new SendEmailVerification(customerId, args[2]).Execute();
		}

		private void ActivateThreeInvalidAttempts()
		{
			int customerId;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe ThreeInvalidAttempts <CustomerId> <password>");
				return;
			}
			new ThreeInvalidAttempts(customerId, args[2]).Execute();
		}

		private void ActivateTransferCashFailed()
		{
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe TransferCashFailed <CustomerId>");
				return;
			}
			new TransferCashFailed(customerId).Execute();
		}

		private void ActivateCaisGenerate()
		{
			int underwriterId;
			if (args.Length != 2 || !int.TryParse(args[1], out underwriterId))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe CaisGenerate <underwriterId>");
				return;
			}
			new CaisGenerator().CaisGenerate(underwriterId);
		}

		private void ActivateCaisUpdate()
		{
			int caisId;
			if (args.Length != 2 || !int.TryParse(args[1], out caisId))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe CaisUpdate <caisId>");
				return;
			}
			new CaisGenerator().CaisUpdate(caisId);
		}

		private void ActivateFirstOfMonthStatusNotifier()
		{
			if (args.Length != 1)
			{
				Console.WriteLine("Usage: StrategiesActivator.exe FirstOfMonthStatusNotifier");
				return;
			}
			new FirstOfMonthStatusNotifier().Execute();
		}

		private void ActivateFraudChecker()
		{
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe FraudChecker <CustomerId>");
				return;
			}
			new FraudChecker(customerId).Execute();
		}

		private void ActivateLateBy14Days()
		{
			if (args.Length != 1)
			{
				Console.WriteLine("Usage: StrategiesActivator.exe LateBy14Days");
				return;
			}
			new LateBy14Days().Execute();
		}

		private void ActivatePayPointCharger()
		{
			if (args.Length != 1)
			{
				Console.WriteLine("Usage: StrategiesActivator.exe PayPointCharger");
				return;
			}
			new PayPointCharger().Execute();
		}

		private void ActivateSetLateLoanStatus()
		{
			if (args.Length != 1)
			{
				Console.WriteLine("Usage: StrategiesActivator.exe SetLateLoanStatus");
				return;
			}
			new SetLateLoanStatus().Execute();
		}

		private void ActivateCustomerMarketPlaceAdded()
		{
			int customerId, marketplaceId;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId) || !int.TryParse(args[2], out marketplaceId))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe CustomerMarketPlaceAdded <CustomerId> <marketplaceId>");
				return;
			}
			new UpdateMarketplaces().CustomerMarketPlaceAdded(customerId, marketplaceId);
		}

		private void ActivateUpdateAllMarketplaces()
		{
			int customerId;
			if (args.Length != 2 || !int.TryParse(args[1], out customerId))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe UpdateAllMarketplaces <CustomerId>");
				return;
			}
			new UpdateMarketplaces().UpdateAllMarketplaces(customerId);
		}

		private void ActivateUpdateTransactionStatus()
		{
			if (args.Length != 1)
			{
				Console.WriteLine("Usage: StrategiesActivator.exe UpdateTransactionStatus");
				return;
			}
			new UpdateTransactionStatus().Execute();
		}

		private void ActivateXDaysDue()
		{
			if (args.Length != 1)
			{
				Console.WriteLine("Usage: StrategiesActivator.exe XDaysDue");
				return;
			}
			new XDaysDue().Execute();
		}
	}
}
