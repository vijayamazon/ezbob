namespace EzBob.Backend.Strategies
{
	using System;
	using System.Data;
	using System.Globalization;
	using System.Threading;
	using EzBob;
	using FraudChecker;
	using log4net;
	using System.Collections.Generic;
	using DbConnection;

	public class Strategies
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(Strategies));
		private readonly StrategiesMailer mailer = new StrategiesMailer();

		public class CustomerData
		{
			public string FirstName { get; set; }
			public string Surname { get; set; }
			public string FullName { get; set; }
			public string Mail { get; set; }
			public bool IsOffline { get; set; }
			public int NumOfLoans { get; set; }

			public CustomerData(int customerId)
			{
				DataTable dt = DbConnection.ExecuteSpReader("GetBasicCustomerData", DbConnection.CreateParam("CustomerId", customerId));
				DataRow results = dt.Rows[0];

				FirstName = results["FirstName"].ToString();
				Surname = results["Surname"].ToString();
				FullName = results["FullName"].ToString();
				Mail = results["Mail"].ToString();
				IsOffline = bool.Parse(results["IsOffline"].ToString());
				NumOfLoans = int.Parse(results["NumOfLoans"].ToString());
			}
		}

		public void Greeting(int customerId, string confirmEmailAddress)
		{
			var customerData = new CustomerData(customerId);
			const string emailSubject = "Thank you for registering with EZBOB!";
			const string templateName = "Greeting";
			var variables = new Dictionary<string, string>
				{
					{"Email", customerData.Mail},
					{"ConfirmEmailAddress", confirmEmailAddress}
				};

			mailer.SendToCustomerAndEzbob(variables, customerData.Mail, templateName, emailSubject);

			DbConnection.ExecuteSpNonQuery("Greeting_Mail_Sent", 
				DbConnection.CreateParam("UserId", customerId), 
				DbConnection.CreateParam("GreetingMailSent", 1));
		}

		public void CashTransferred( int customerId, int amount)
		{
			var customerData = new CustomerData(customerId);

			var variables = new Dictionary<string, string>
				{
					{"FirstName", customerData.FirstName},
					{"Amount", amount.ToString(CultureInfo.InvariantCulture)}
				};

			string emailSubject, templateName;
			if (customerData.NumOfLoans == 1)
			{
				emailSubject = "Welcome to the EZBOB family";
				templateName = customerData.IsOffline ? "Mandrill - Took Offline Loan (1st loan)" : "Mandrill - Took Loan (1st loan)";
			}
			else
			{
				emailSubject = "Thanks for choosing EZBOB as your funding partner";
				templateName = customerData.IsOffline ? "Mandrill - Took Offline Loan (not 1st loan)" : "Mandrill - Took Loan (not 1st loan)";
			}

			mailer.SendToCustomerAndEzbob(variables, customerData.Mail, templateName, emailSubject);
		}

		public void ThreeInvalidAttempts(int customerId, string firstName, string password)
		{
			var customerData = new CustomerData(customerId);
			const string emailSubject = "Three unsuccessful login attempts to your account have been made.";
			const string templateName = "Mandrill - Temporary password";

			var variables = new Dictionary<string, string>
				{
					{"Password", password},
					{"FirstName", firstName},
					{"ProfilePage", "https://app.ezbob.com/Customer/Profile"},
					{"NIMRODTELEPHONENUMBER", "+44 800 011 4787"} // TODO: change name of variable here and in mandrill\mailchimp
				};

			mailer.SendToCustomerAndEzbob(variables, customerData.Mail, templateName, emailSubject);
		}

		public void PasswordChanged(int customerId, string password)
		{
			var customerData = new CustomerData(customerId);
			const string emailSubject = "Your new EZBOB password has been registered.";
			const string templateName = "Mandrill - New password";
			
			var variables = new Dictionary<string, string>
				{
					{"Password", password},
					{"FirstName", customerData.FirstName}
				};

			mailer.SendToCustomerAndEzbob(variables, customerData.Mail, templateName, emailSubject);
		}

		// TODO: passing the emailTo is ugly 
		// According to the code it could be based on User.EMail or User.Name
		// It should not be passed in and the mail should go to customerData.Mail
		public void PasswordRestored(int customerId, string emailTo, string password)
		{
			var customerData = new CustomerData(customerId);
			const string emailSubject = "RestorePassword";
			const string templateName = "Mandrill - EZBOB password was restored";
			
			var variables = new Dictionary<string, string>
				{
					{"ProfilePage", "https://app.ezbob.com/Account/LogOn"},
					{"Password", password},
					{"FirstName", customerData.FirstName}
				};

			mailer.SendToCustomerAndEzbob(variables, emailTo, templateName, emailSubject);
		}

		public void GetCashFailed(int customerId, string firstName)
		{
			var customerData = new CustomerData(customerId);
			const string emailSubject = "Get cash - problem with the card";
			const string templateName = "Mandrill - Debit card authorization problem";
			
			var variables = new Dictionary<string, string>
				{
					{"DashboardPage", "https://app.ezbob.com/Customer/Profile"},
					{"FirstName", customerData.FirstName}
				};

			mailer.SendToCustomerAndEzbob(variables, customerData.Mail, templateName, emailSubject);
		}

		public void TransferCashFailed(int customerId, string firstName)
		{
			var customerData = new CustomerData(customerId);
			const string emailSubject = "Bank account couldn’t be verified";
			const string templateName = "Mandrill - Problem with bank account";
			
			var variables = new Dictionary<string, string>
				{
					{"FirstName", customerData.FirstName}
				};

			mailer.SendToCustomerAndEzbob(variables, customerData.Mail, templateName, emailSubject);
		}

		public void PayEarly(int customerId, decimal amount, string loanRefNumber)
		{
			var customerData = new CustomerData(customerId);
			string subject = string.Format("Dear {0}, your payment of £{1} has been credited to your EZBOB account.", customerData.FirstName, amount);
			const string templateName = "Mandrill - Repayment confirmation";
			
			var variables = new Dictionary<string, string>
				{
					{"FirstName", customerData.FirstName},
					{"AMOUNT", amount.ToString(CultureInfo.InvariantCulture)},
					{"DATE", DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss").Replace('-','/')},
					{"RefNum", loanRefNumber}
				};

			mailer.SendToCustomerAndEzbob(variables, customerData.Mail, templateName, subject);
		}

		public void PayPointNameValidationFailed(string cardHodlerName, int customerId)
		{
			var customerData = new CustomerData(customerId);
			const string subject = "PayPoint personal data differs from EZBOB application";
			const string templateName = "Mandrill - PayPoint data differs";

			var variables = new Dictionary<string, string>
				{
					{"E-mail", customerData.Mail},
					{"UserId", customerId.ToString(CultureInfo.InvariantCulture)},
					{"Name", customerData.FirstName},
					{"Surname", customerData.Surname},
					{"PayPointName", cardHodlerName}
				};
			mailer.SendToEzbob(variables, templateName, subject);
		}

		public void ApprovedUser(int customerId, decimal loanAmount)
		{
			var customerData = new CustomerData(customerId);
			string subject = string.Format("Congratulations {0}, £{1} is available to fund your business today", customerData.FirstName, loanAmount);


			bool isFirstApproval = false; // TODO: get according to customerId (if DecisionHistory table for this customer contains at least one approval)
			int validFor = 10; // TODO: get according to customerId (customer.OfferValidUntil - customer.OfferStart).Value.TotalHours)

			// TODO: get customerEmail from customerId

			var variables = new Dictionary<string, string>
				{
					{"FirstName", customerData.FirstName},
					{"LoanAmount", loanAmount.ToString(CultureInfo.InvariantCulture)},
					{"ValidFor", validFor.ToString(CultureInfo.InvariantCulture)}
				};

			if (customerData.IsOffline)
			{
				mailer.SendToCustomerAndEzbob(variables, customerData.Mail,
				                       isFirstApproval
					                       ? "Mandrill - Approval Offline (1st time)"
					                       : "Mandrill - Approval Offline (not 1st time)", subject);
			}
			else
			{
				mailer.SendToCustomerAndEzbob(variables, customerData.Mail,
									   isFirstApproval
										   ? "Mandrill - Approval (1st time)"
										   : "Mandrill - Approval (not 1st time)", subject);
			}
		}

		public void RejectUser(int customerId)
		{
			var customerData = new CustomerData(customerId);
			string subject = "Sorry, EZBOB cannot make you a loan offer at this time";
			const string templateName = "Mandrill - Rejection email";
			
			var variables = new Dictionary<string, string>
				{
					{"FirstName", customerData.FirstName},
					{"EzbobAccount", "https://app.ezbob.com/Customer/Profile"}
				};

			mailer.SendToCustomerAndEzbob(variables, customerData.Mail, templateName, subject);
		}

		public void MoreAMLInformation(int customerId)
		{
			var customerData = new CustomerData(customerId);
			string subject = "Proof of ID required to make you a loan offer";
			const string templateName = "Mandrill - Application incompleted AML";
			
			var variables = new Dictionary<string, string>
				{
					{"FirstName", customerData.FirstName}
				};

			mailer.SendToCustomerAndEzbob(variables, customerData.Mail, templateName, subject);
		}

		public void MoreAMLandBWAInformation(int customerId)
		{
			var customerData = new CustomerData(customerId);
			string subject = "We require a proof of bank account ownership and proof of ID to make you a loan offer";
			const string templateName = "Mandrill - Application incompleted AML & Bank";
			
			var variables = new Dictionary<string, string>
				{
					{"FirstName", customerData.FirstName}
				};

			mailer.SendToCustomerAndEzbob(variables, customerData.Mail, templateName, subject);
		}

		public void MoreBWAInformation(int customerId)
		{
			var customerData = new CustomerData(customerId);
			string subject = "We require a proof of bank account ownership to make you a loan offer";
			const string templateName = "Mandrill - Application incompleted Bank";
			
			var variables = new Dictionary<string, string>
				{
					{"FirstName", customerData.FirstName}
				};

			mailer.SendToCustomerAndEzbob(variables, customerData.Mail, templateName, subject);
		}

		public void SendEmailVerification(int customerId, string address)
		{
			var customerData = new CustomerData(customerId);
			string subject = "Please verify your email";
			const string templateName = "Mandrill - Confirm your email";
			
			var variables = new Dictionary<string, string>
				{
					{"FirstName", customerData.FirstName},
					{"Email", customerData.Mail},
					{"ConfirmEmailAddress", address}
				};

			mailer.SendToCustomerAndEzbob(variables, customerData.Mail, templateName, subject);
		}

		public void PayPointAddedByUnderwriter(int customerId, string cardno, string underwriterName, int underwriterId)
		{
			var customerData = new CustomerData(customerId);
			string subject = "Underwriter has added a debit card to the client account";
			const string templateName = "Mandrill - Underwriter added a debit card";
			
			var variables = new Dictionary<string, string>
				{
					{"UWName", underwriterName},
					{"UWID", underwriterId.ToString(CultureInfo.InvariantCulture)},
					{"Email", customerData.Mail},
					{"ClientId", customerId.ToString(CultureInfo.InvariantCulture)},
					{"ClientName", customerData.FullName},
					{"CardNo", cardno}
				};
			mailer.SendToEzbob(variables, templateName, subject);
		}

		public void EmailRolloverAdded(int customerId, decimal amount)
		{
			var customerData = new CustomerData(customerId);
			string subject = "Rollover added";
			const string templateName = "Mandrill - Rollover added";
			
			var variables = new Dictionary<string, string>
				{
					{"FirstName", customerData.FirstName},
					{"RolloverAmount", amount.ToString(CultureInfo.InvariantCulture)}
				};

			mailer.SendToCustomerAndEzbob(variables, customerData.Mail, templateName, subject);
		}

		public void RenewEbayToken(int customerId, string marketplaceName, string eBayAddress)
		{
			var customerData = new CustomerData(customerId);
			string subject = "Please renew your eBay token";
			const string templateName = "Mandrill - Renew your eBay token";

			var variables = new Dictionary<string, string>
				{
					{"FirstName", customerData.FirstName},
					{"eBayName", marketplaceName},
					{"eBayAddress", eBayAddress}
				};

			mailer.SendToCustomerAndEzbob(variables, customerData.Mail, templateName, subject);
		}

		public void Escalated(int customerId)
		{
			var customerData = new CustomerData(customerId);
			string subject = "User was Escalated";
			const string templateName = "Mandrill - User was escalated";



			string escalationReason = null; // TODO: customer.EscalationReason
			string underwriterName = null; // TODO: customer.UnderwriterName
			DateTime registrationDate = DateTime.Now; // TODO: customer.GreetingMailSentDate
			string medal = null; // customer.Medal.HasValue ? customer.Medal.ToString() : ""
			string systemDecision = null;// customer.SystemDecision;

			var variables = new Dictionary<string, string>
				{
					{"userID", customerId.ToString(CultureInfo.InvariantCulture)},
					{"Name", customerData.Mail},
					{"EscalationReason", escalationReason},
					{"UWName", underwriterName},
					{"RegistrationDate", registrationDate.ToString(CultureInfo.InvariantCulture)},
					{"FirstName", customerData.FirstName},
					{"Surname", customerData.Surname},
					{"MedalType", medal},
					{"SystemDecision", systemDecision}
				};
			mailer.SendToEzbob(variables, templateName, subject);
		}


		public void EmailUnderReview(int customerId, string email)
		{
			var customerData = new CustomerData(customerId);
			string subject = "Your completed application is currently under review";
			const string templateName = "Mandrill - Application completed under review";

			var variables = new Dictionary<string, string>
				{
					{"FirstName", customerData.FirstName}
				};

			mailer.SendToCustomerAndEzbob(variables, email/*Why should we use an input param mail???*/, templateName, subject);
		}



		// Small but not mail
		public void FraudChecker(int customerId)
		{
			var checker = new FraudDetectionChecker();
			checker.Check(customerId);
		}














		// Large strategies:
		private object caisGenerationLock = new object();
		private int caisGenerationTriggerer = -1;
		public void CAISGenerate(int underwriterId)
		{
			lock (caisGenerationLock)
			{
				if (caisGenerationTriggerer != -1)
				{
					log.WarnFormat("A CAIS generation is already in progress. Triggered by Underwriter:{0}", caisGenerationTriggerer);
					return;
				}
				caisGenerationTriggerer = underwriterId;
			}
			
			// TODO: complete implementation - CAIS_NO_Upload

			lock (caisGenerationLock)
			{
				caisGenerationTriggerer = -1;
			}
		}


		public void CAISUpdate(int customerId, int caisId)
		{
			// TODO: complete implementation - CAIS_NO_Upload
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


		// main strategy - 1
		public void Evaluate(int customerId, NewCreditLineOption newCreditLineOption, int avoidAutomaticDescison, bool isUnderwriterForced = false)
		{
			var mainStrategy = new MainStrategy();
			mainStrategy.Evaluate(customerId, newCreditLineOption, avoidAutomaticDescison, isUnderwriterForced);
		}
	}
}
