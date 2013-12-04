namespace EzBob.Backend.Strategies
{
	using System;
	using System.Globalization;
	using System.Threading;
	using EzBob;
	using log4net;
	using System.Collections.Generic;

	public class Strategies
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(Strategies));
		private readonly StrategiesMailer mailer = new StrategiesMailer();

		public void Greeting(string customerEmail, string confirmEmailAddress, int custumerId)
		{
			const string emailSubject = "Thank you for registering with EZBOB!";
			const string templateName = "Greeting";
			var variables = new Dictionary<string, string>
				{
					{"Email", customerEmail},
					{"ConfirmEmailAddress", confirmEmailAddress}
				};

			mailer.SendToCustomerAndEzbob(variables, customerEmail, templateName, emailSubject);

			// TODO: execute sp Greeting_Mail_Sent(custumerId, 1)
		}

		public void CashTransferred(string customerEmail, int custumerId, int amount, decimal setUpFee, int loanId)
		{
			string firstName = null;
			bool isOffline = false;
			bool isFirstLoan = false;

			// TODO: get firstName, isOffline, isFirstLoan from DB (bool isFirstLoan = customer.Loans.Count == 1;)

			// TODO: remove redundant variables - IsFirstLoan, IsOffline
			var variables = new Dictionary<string, string>
				{
					{"FirstName", firstName},
					{"Amount", amount.ToString(CultureInfo.InvariantCulture)}
				};

			string emailSubject, templateName;
			if (isFirstLoan)
			{
				emailSubject = "Welcome to the EZBOB family";
				templateName = isOffline ? "Mandrill - Took Offline Loan (1st loan)" : "Mandrill - Took Loan (1st loan)";
			}
			else
			{
				emailSubject = "Thanks for choosing EZBOB as your funding partner";
				templateName = isOffline ? "Mandrill - Took Offline Loan (not 1st loan)" : "Mandrill - Took Loan (not 1st loan)";
			}

			mailer.SendToCustomerAndEzbob(variables, customerEmail, templateName, emailSubject);
		}

		public void ThreeInvalidAttempts(int customerId, string firstName, string password)
		{
			const string emailSubject = "Three unsuccessful login attempts to your account have been made.";
			const string templateName = "Mandrill - Temporary password";

			string customerEmail = null;

			// TODO: get customerEmail from customerId

			var variables = new Dictionary<string, string>
				{
					{"Password", password},
					{"FirstName", firstName},
					{"ProfilePage", "https://app.ezbob.com/Customer/Profile"},
					{"NIMRODTELEPHONENUMBER", "+44 800 011 4787"} // TODO: change name of variable here and in mandrill\mailchimp
				};

			mailer.SendToCustomerAndEzbob(variables, customerEmail, templateName, emailSubject);
		}

		public void PasswordChanged(int customerId, string firstName, string password)
		{
			const string emailSubject = "Your new EZBOB password has been registered.";
			const string templateName = "Mandrill - New password";

			string customerEmail = null;

			// TODO: get customerEmail from customerId

			var variables = new Dictionary<string, string>
				{
					{"Password", password},
					{"FirstName", firstName}
				};

			mailer.SendToCustomerAndEzbob(variables, customerEmail, templateName, emailSubject);
		}

		public void PasswordRestored(int customerId, string emailTo, string firstName, string password)
		{
			const string emailSubject = "RestorePassword";
			const string templateName = "Mandrill - EZBOB password was restored";

			string customerEmail = null;

			// TODO: get customerEmail from customerId

			var variables = new Dictionary<string, string>
				{
					{"ProfilePage", "https://app.ezbob.com/Account/LogOn"},
					{"Password", password},
					{"FirstName", firstName}
				};

			mailer.SendToCustomerAndEzbob(variables, customerEmail, templateName, emailSubject);
		}

		public void CustomerMarketPlaceAdded(int customerId, int marketplaceId)
		{
			var requestId = RetrieveDataHelper.UpdateCustomerMarketplaceData(marketplaceId);

			while (!RetrieveDataHelper.IsRequestDone(requestId))
			{
				Thread.Sleep(1000); // TODO: make this configurable
			}
			var requestState = RetrieveDataHelper.GetRequestState(requestId);
			string errorCode = null;

			if (requestState == null || requestState.HasError())
			{
				string errorMessage = null;
				string marketplaceType = null;
				bool isEbay = false;
				bool tokenExpired = false;
				// TODO: fill errorCode, errorMessage, marketplaceType from requestState.ErorrInfo
				// TODO: fill isEbay from marketplaceId

				string emailSubject, templateName;

				if (isEbay &&
					(errorCode == "16110" || errorCode == "931" || errorCode == "932" || errorCode == "16118" ||
					 errorCode == "16119" || errorCode == "17470"))
				{
					tokenExpired = true;
					emailSubject = "eBay token has expired";
					templateName = "Mandrill - Update MP Error Code";

					var variables = new Dictionary<string, string>
						{
							{"userID", customerId.ToString(CultureInfo.InvariantCulture)},
							{"MPType", marketplaceType},
							{"CustomerMarketPlaceId", marketplaceId.ToString(CultureInfo.InvariantCulture)},
							{"ErrorMessage", errorMessage},
							{"ErrorCode", errorCode}
						};

					mailer.SendToEzbob(variables, templateName, emailSubject);
				}
				else
				{
					emailSubject = "eBay token has expired";
					templateName = "Mandrill - UpdateCMP Error";

					// TODO: Remove ApplicationID from mandrill\mailchimp templates
					var variables = new Dictionary<string, string>
						{
							{"userID", customerId.ToString(CultureInfo.InvariantCulture)},
							{"CustomerMarketPlaceId", marketplaceId.ToString(CultureInfo.InvariantCulture)},
							{"UpdateCMP_Error", errorMessage}
						};

					mailer.SendToEzbob(variables, templateName, emailSubject);
				}
			}

			// TODO: update end time & (errorCode, tokenExpired)
		}

		public void GetCashFailed(int customerId, string firstName)
		{
			const string emailSubject = "Get cash - problem with the card";
			const string templateName = "Mandrill - Debit card authorization problem";

			string customerEmail = null;

			// TODO: get customerEmail from customerId

			var variables = new Dictionary<string, string>
				{
					{"DashboardPage", "https://app.ezbob.com/Customer/Profile"},
					{"FirstName", firstName}
				};

			mailer.SendToCustomerAndEzbob(variables, customerEmail, templateName, emailSubject);
		}

		public void TransferCashFailed(int customerId, string firstName)
		{
			const string emailSubject = "Bank account couldn’t be verified";
			const string templateName = "Mandrill - Problem with bank account";

			string customerEmail = null;

			// TODO: get customerEmail from customerId

			var variables = new Dictionary<string, string>
				{
					{"FirstName", firstName}
				};

			mailer.SendToCustomerAndEzbob(variables, customerEmail, templateName, emailSubject);
		}

		public void PayEarly(int customerId, decimal amount, string firstName, string refNumber)
		{
			string subject = string.Format("Dear {0}, your payment of £{1} has been credited to your EZBOB account.", firstName, amount);
			const string templateName = "Mandrill - Repayment confirmation";

			string customerEmail = null;

			// TODO: get customerEmail from customerId

			var variables = new Dictionary<string, string>
				{
					{"FirstName", firstName},
					{"AMOUNT", amount.ToString(CultureInfo.InvariantCulture)},
					{"DATE", DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss").Replace('-','/')},
					{"RefNum", refNumber}
				};

			mailer.SendToCustomerAndEzbob(variables, customerEmail, templateName, subject);
		}

		public void PayPointNameValidationFailed(string cardHodlerName, int customerId)
		{
			string subject = "PayPoint personal data differs from EZBOB application";
			const string templateName = "Mandrill - PayPoint data differs";

			string customerEmail = null;
			string customerFirstName = null;
			string customerSurName = null;

			// TODO: get customerEmail from customerId

			var variables = new Dictionary<string, string>
				{
					{"E-mail", customerEmail},
					{"UserId", customerId.ToString(CultureInfo.InvariantCulture)},
					{"Name", customerFirstName},
					{"Surname", customerSurName},
					{"PayPointName", cardHodlerName}
				};
			mailer.SendToEzbob(variables, templateName, subject);
		}

		public void ApprovedUser(int customerId, decimal loanAmount)
		{
			string firstName = null; // TODO: get from customerId
			string subject = string.Format("Congratulations {0}, £{1} is available to fund your business today", firstName, loanAmount);
			string customerEmail = null;
			bool isOffline = false; // TODO: get according to customerId
			bool isFirstApproval = false; // TODO: get according to customerId (if DecisionHistory table for this customer contains at least one approval)
			int validFor = 10; // TODO: get according to customerId (customer.OfferValidUntil - customer.OfferStart).Value.TotalHours)

			// TODO: get customerEmail from customerId

			var variables = new Dictionary<string, string>
				{
					{"FirstName", firstName},
					{"LoanAmount", loanAmount.ToString(CultureInfo.InvariantCulture)},
					{"ValidFor", validFor.ToString(CultureInfo.InvariantCulture)}
				};

			if (isOffline)
			{
				mailer.SendToCustomerAndEzbob(variables, customerEmail,
				                       isFirstApproval
					                       ? "Mandrill - Approval Offline (1st time)"
					                       : "Mandrill - Approval Offline (not 1st time)", subject);
			}
			else
			{
				mailer.SendToCustomerAndEzbob(variables, customerEmail,
									   isFirstApproval
										   ? "Mandrill - Approval (1st time)"
										   : "Mandrill - Approval (not 1st time)", subject);
			}
		}

		public void RejectUser(int customerId)
		{
			string subject = "Sorry, EZBOB cannot make you a loan offer at this time";
			const string templateName = "Mandrill - Rejection email";

			string customerEmail = null;
			string firstName = null;
			// TODO: get customerEmail from customerId

			var variables = new Dictionary<string, string>
				{
					{"FirstName", firstName},
					{"EzbobAccount", "https://app.ezbob.com/Customer/Profile"}
				};

			mailer.SendToCustomerAndEzbob(variables, customerEmail, templateName, subject);
		}

		public void MoreAMLInformation(int customerId)
		{
			string subject = "Proof of ID required to make you a loan offer";
			const string templateName = "Mandrill - Application incompleted AML";

			string customerEmail = null;
			string firstName = null;
			// TODO: get customerEmail from customerId

			var variables = new Dictionary<string, string>
				{
					{"FirstName", firstName}
				};

			mailer.SendToCustomerAndEzbob(variables, customerEmail, templateName, subject);
		}

		public void MoreAMLandBWAInformation(int customerId)
		{
			string subject = "We require a proof of bank account ownership and proof of ID to make you a loan offer";
			const string templateName = "Mandrill - Application incompleted AML & Bank";

			string customerEmail = null;
			string firstName = null;
			// TODO: get customerEmail from customerId

			var variables = new Dictionary<string, string>
				{
					{"FirstName", firstName}
				};

			mailer.SendToCustomerAndEzbob(variables, customerEmail, templateName, subject);
		}

		public void MoreBWAInformation(int customerId)
		{
			string subject = "We require a proof of bank account ownership to make you a loan offer";
			const string templateName = "Mandrill - Application incompleted Bank";

			string customerEmail = null;
			string firstName = null;
			// TODO: get customerEmail from customerId

			var variables = new Dictionary<string, string>
				{
					{"FirstName", firstName}
				};

			mailer.SendToCustomerAndEzbob(variables, customerEmail, templateName, subject);
		}

		public void SendEmailVerification(int customerId, string address)
		{
			string subject = "Please verify your email";
			const string templateName = "Mandrill - Confirm your email";

			string customerEmail = null;
			string firstName = null;
			// TODO: get customerEmail from customerId

			var variables = new Dictionary<string, string>
				{
					{"FirstName", firstName},
					{"Email", customerEmail},
					{"ConfirmEmailAddress", address}
				};

			mailer.SendToCustomerAndEzbob(variables, customerEmail, templateName, subject);
		}

		public void PayPointAddedByUnderwriter(int customerId, string cardno, string underwriterName, int underwriterId)
		{
			string subject = "Underwriter has added a debit card to the client account";
			const string templateName = "Mandrill - Underwriter added a debit card";

			string customerEmail = null;
			string customerFullName = null;

			// TODO: get customerEmail from customerId

			var variables = new Dictionary<string, string>
				{
					{"UWName", underwriterName},
					{"UWID", underwriterId.ToString(CultureInfo.InvariantCulture)},
					{"Email", customerEmail},
					{"ClientId", customerId.ToString(CultureInfo.InvariantCulture)},
					{"ClientName", customerFullName},
					{"CardNo", cardno}
				};
			mailer.SendToEzbob(variables, templateName, subject);
		}

		public void EmailRolloverAdded(int customerId, decimal amount, DateTime expireDate)
		{
			string subject = "Rollover added";
			const string templateName = "Mandrill - Rollover added";

			string customerEmail = null;
			string firstName = null;
			// TODO: get customerEmail from customerId

			var variables = new Dictionary<string, string>
				{
					{"FirstName", firstName},
					{"RolloverAmount", amount.ToString(CultureInfo.InvariantCulture)}
				};

			mailer.SendToCustomerAndEzbob(variables, customerEmail, templateName, subject);
		}

		public void RenewEbayToken(int customerId, string marketplaceName, string eBayAddress)
		{
			string subject = "Please renew your eBay token";
			const string templateName = "Mandrill - Renew your eBay token";

			string customerEmail = null;
			string firstName = null;
			// TODO: get customerEmail from customerId

			var variables = new Dictionary<string, string>
				{
					{"FirstName", firstName},
					{"eBayName", marketplaceName},
					{"eBayAddress", eBayAddress}
				};

			mailer.SendToCustomerAndEzbob(variables, customerEmail, templateName, subject);
		}

		public void Escalated(int customerId)
		{
			string subject = "User was Escalated";
			const string templateName = "Mandrill - User was escalated";

			string customerEmail = null;
			string customerFirstName = null;
			string customerSurname = null;
			string escalationReason = null; // TODO: customer.EscalationReason
			string underwriterName = null; // TODO: customer.UnderwriterName
			DateTime registrationDate = DateTime.Now; // TODO: customer.GreetingMailSentDate
			string medal = null; // customer.Medal.HasValue ? customer.Medal.ToString() : ""
			string systemDecision = null;// customer.SystemDecision;
			// TODO: get customerEmail from customerId

			var variables = new Dictionary<string, string>
				{
					{"userID", customerId.ToString(CultureInfo.InvariantCulture)},
					{"Name", customerEmail},
					{"EscalationReason", escalationReason},
					{"UWName", underwriterName},
					{"RegistrationDate", registrationDate.ToString(CultureInfo.InvariantCulture)},
					{"FirstName", customerFirstName},
					{"Surname", customerSurname},
					{"MedalType", medal},
					{"SystemDecision", systemDecision}
				};
			mailer.SendToEzbob(variables, templateName, subject);
		}

		private object caisGenerationLock = new object();
		private bool isCaisGenerationInProgress = false;
		public void CAISGenerate(int underwriterId)
		{
			lock (caisGenerationLock)
			{
				if (isCaisGenerationInProgress)
				{
					//log
					return;
				}
				isCaisGenerationInProgress = true;
			}
			
			// TODO: complete implementation

			lock (caisGenerationLock)
			{
				isCaisGenerationInProgress = false;
			}
		}


		// main strategy - 1
		public void Evaluate(int customerId, NewCreditLineOption newCreditLineOption, int avoidAutomaticDescison, bool isUnderwriterForced = false)
		{
			var mainStrategy = new MainStrategy();
			mainStrategy.Evaluate(customerId, newCreditLineOption, avoidAutomaticDescison, isUnderwriterForced);
		}
	}
}
