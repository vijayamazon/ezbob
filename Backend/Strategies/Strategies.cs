﻿namespace Strategies
{
	using System;
	using System.Globalization;
	using System.Threading;
	using ExperianLib;
	using ExperianLib.Ebusiness;
	using EzBob;
	using EzBob.Models;
	using EzBobIntegration.Web_References.Consumer;
	using MailApi;
	using log4net;
	using System.Collections.Generic;

	public class Strategies
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(Strategies));
		private readonly Mail mail = new Mail();

		public void Greeting(string customerEmail, string confirmEmailAddress, int custumerId)
		{
			const string emailSubject = "Thank you for registering with EZBOB!";
			const string templateName = "Greeting";
			var variables = new Dictionary<string, string>
				{
					{"Email", customerEmail},
					{"ConfirmEmailAddress", confirmEmailAddress}
				};

			SendToCustomerAndEzbob(variables, customerEmail, templateName, emailSubject);

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

			SendToCustomerAndEzbob(variables, customerEmail, templateName, emailSubject);
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

			SendToCustomerAndEzbob(variables, customerEmail, templateName, emailSubject);
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

			SendToCustomerAndEzbob(variables, customerEmail, templateName, emailSubject);
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

			SendToCustomerAndEzbob(variables, customerEmail, templateName, emailSubject);
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
				string ezbobTo = null, ezbobCc = null;
				// TODO: add addresses to ConfigurationVariables
				// TODO: load addresses from ConfigurationVariables

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

					SendMailViaMandrill(variables, ezbobTo, ezbobCc, templateName, emailSubject);
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

					SendMailViaMandrill(variables, ezbobTo, ezbobCc, templateName, emailSubject);
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

			SendToCustomerAndEzbob(variables, customerEmail, templateName, emailSubject);
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

			SendToCustomerAndEzbob(variables, customerEmail, templateName, emailSubject);
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

			SendToCustomerAndEzbob(variables, customerEmail, templateName, subject);
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
			SendToEzbob(variables, templateName, subject);
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
				SendToCustomerAndEzbob(variables, customerEmail,
				                       isFirstApproval
					                       ? "Mandrill - Approval Offline (1st time)"
					                       : "Mandrill - Approval Offline (not 1st time)", subject);
			}
			else
			{
				SendToCustomerAndEzbob(variables, customerEmail,
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

			SendToCustomerAndEzbob(variables, customerEmail, templateName, subject);
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

			SendToCustomerAndEzbob(variables, customerEmail, templateName, subject);
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

			SendToCustomerAndEzbob(variables, customerEmail, templateName, subject);
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

			SendToCustomerAndEzbob(variables, customerEmail, templateName, subject);
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

			SendToCustomerAndEzbob(variables, customerEmail, templateName, subject);
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
			SendToEzbob(variables, templateName, subject);
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

			SendToCustomerAndEzbob(variables, customerEmail, templateName, subject);
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

			SendToCustomerAndEzbob(variables, customerEmail, templateName, subject);
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
			SendToEzbob(variables, templateName, subject);
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




		public enum NewCreditLineOption
		{
			SkipEverything = 1,
			UpdateEverythingExceptMp = 2,
			UpdateEverythingAndApplyAutoRules = 3,
			UpdateEverythingAndGoToManualDecision = 4,
		}


		// main strategy - 1
		public void Evaluate(int customerId, NewCreditLineOption newCreditLineOption, int avoidAutomaticDescison, bool isUnderwriterForced = false)
		{
			StrategyHelper strategyHelper = new StrategyHelper();
			// TODO: remove column Customer.LastStartedMainStrategy

			/*new StrategyParameter("userId", user.Id),
            new StrategyParameter("Underwriter_Check", isUnderwriterForced ? 1 : 0),
            new StrategyParameter("NewCreditLineOption", (int)newCreditLineOption),
            new StrategyParameter("AvoidAutomaticDescison", avoidAutomaticDescison)*/

			// TODO: Read from ConfigurationVariables (ConfigurationVariables_MainStrat)
			string BWABusinessCheck = string.Empty;
			bool EnableAutomaticRejection = false;
			bool EnableAutomaticReApproval = false;
			bool EnableAutomaticApproval = false;
			int LowCreditScore = 1;
			int LowTotalAnnualTurnover = 1;
			int LowTotalThreeMonthTurnover = 1;
			int MaxCapHomeOwner = 1;
			int MaxCapNotHomeOwner = 1;
			int Reject_Defaults_CreditScore = 1;
			int Reject_Defaults_AccountsNum = 1;
			int Reject_Defaults_Amount = 1;
			int Reject_Defaults_MonthsNum = 1;
			bool EnableAutomaticReRejection = false;
			int AutoRejectionException_CreditScore = 1;
			int AutoRejectionException_AnualTurnover = 1;
			int Reject_Minimal_Seniority = 1;
			int AutoReApproveMaxNumOfOutstandingLoans = 1;
			bool AutoApproveIsSilent = false;
			string AutoApproveSilentTemplateName = string.Empty;
			string AutoApproveSilentToAddress = string.Empty;
			//string ScortoInternalErrorMessage = string.Empty;

			strategyHelper.GetZooplaData(customerId);




			// TODO: get personal info (GetPersonalInfo)
			string App_email = string.Empty;
			string App_FirstName = string.Empty;
			string App_Surname = string.Empty;
			DateTime App_DateOfBirth = DateTime.Now;
			string App_HomeOwner = string.Empty;
			string App_MaritalStatus = string.Empty;
			int App_TimeAtAddress = 1;
			string App_Gender = string.Empty;
			string CompanyType = string.Empty;
			string App_AccountNumber = string.Empty;
			string App_SortCode = string.Empty;
			string App_LimitedRefNum = string.Empty;
			string App_NonLimitedRefNum = string.Empty;
			decimal App_OverallTurnOver = 1;
			decimal App_WebSiteTurnOver = 1;
			DateTime App_ApplyForLoan = DateTime.Now;
			DateTime App_ValidFor = DateTime.Now;
			DateTime App_RegistrationDate = DateTime.Now;
			string App_RefNumber = string.Empty;
			string App_BankAccountType = string.Empty;
			int Prev_ExperianConsumerScore = 1;
			bool CustomerStatusIsEnabled = false;
			bool CustomerStatusIsWarning = false;
			bool IsOffline = false;
			bool HasAccountingAccounts = false;
			string ScortoInternalErrorMessage = string.Empty;



			string ExperianLimitedError = null;
			string ExperianNonLimitedError = null;
			decimal ExperianBureauScoreLimited = 0;
			decimal ExperianExistingBusinessLoans = 0;
			decimal ExperianBureauScoreNonLimited = 0;
			bool ExperianCompanyNotFoundOnBureau = false;
			string ExperianConsumerError = string.Empty;
			string ExperianConsumerErrorPrev = string.Empty;
			double ExperianConsumerScore = 0;
			double MinExperianScore = 0;
			double Inintial_ExperianConsumerScore = 0;
			double ExperianScoreConsumer = 0;
			DateTime ExperianBirthDate = DateTime.Now;



			if (!CustomerStatusIsEnabled || CustomerStatusIsWarning)
			{
				EnableAutomaticReApproval = false;
				EnableAutomaticApproval = false;
			}


			if (IsOffline)
			{
				EnableAutomaticReApproval = false;
				EnableAutomaticApproval = false;
				EnableAutomaticReRejection = false;
				EnableAutomaticRejection = false;
			}

			if (newCreditLineOption != NewCreditLineOption.SkipEverything && newCreditLineOption != NewCreditLineOption.UpdateEverythingExceptMp)
			{
				if (!WaitForMarketplacesToFinishUpdates(customerId))
				{
					string customerEmail = null;

					// TODO: get customerEmail from customerId

					var variables = new Dictionary<string, string>
						{
							{"UserEmail", customerEmail},
							{"CustomerID", customerId.ToString(CultureInfo.InvariantCulture)},
							{"ApplicationID", customerEmail}
						};
					SendToEzbob(variables, "Mandrill - No Information about shops", "No information about customer marketplace");
					return;
				}
			}

			if (newCreditLineOption != NewCreditLineOption.SkipEverything)
			{
				if (CompanyType == "Limited" || CompanyType == "LLP")
				{
					if (string.IsNullOrEmpty(App_LimitedRefNum))
					{
						ExperianLimitedError = "RefNumber is empty";
					}
					else
					{
						var service = new EBusinessService();
						LimitedResults limitedData = service.GetLimitedBusinessData(App_LimitedRefNum, customerId);

						if (!limitedData.IsError)
						{
							ExperianBureauScoreLimited = limitedData.BureauScore;
							ExperianExistingBusinessLoans = limitedData.ExistingBusinessLoans;
						}
						else
						{
							ExperianLimitedError = limitedData.Error;
						}
					}

					// TODO: call UpdateExperianBusiness (App_LimitedRefNum,ExperianLimitedError,ExperianBureauScoreLimited)
				}
				else if (CompanyType == "PShip3P" || CompanyType == "PShip" || CompanyType == "SoleTrader")
				{
					if (string.IsNullOrEmpty(App_NonLimitedRefNum))
					{
						ExperianNonLimitedError = "RefNumber is empty";
					}
					else
					{
						var service = new EBusinessService();
						var nonlimitedData = service.GetNotLimitedBusinessData(App_NonLimitedRefNum, customerId);
						if (!nonlimitedData.IsError)
						{
							ExperianBureauScoreNonLimited = nonlimitedData.BureauScore;
							ExperianCompanyNotFoundOnBureau = nonlimitedData.CompanyNotFoundOnBureau;
						}
						else
						{
							ExperianNonLimitedError = nonlimitedData.Error;
						}
					}

					// TODO: call UpdateExperianBusiness (App_NonLimitedRefNum,ExperianNonLimitedError,ExperianBureauScoreNonLimited)
				}

				// TODO: call Get_Customer_Address_and_Previous_Address
				string App_Line1 = null;
				string App_Line2 = null;
				string App_Line3 = null;
				string App_Line4 = null;
				string App_Line5 = null;
				string App_Line6 = null;
				string App_Line1Prev = null;
				string App_Line2Prev = null;
				string App_Line3Prev = null;
				string App_Line4Prev = null;
				string App_Line5Prev = null;
				string App_Line6Prev = null;



				var consumerService = new ConsumerService();
				var location = new InputLocationDetailsMultiLineLocation();
				location.LocationLine1 = App_Line1;
				location.LocationLine2 = App_Line2;
				location.LocationLine3 = App_Line3;
				location.LocationLine4 = App_Line4;
				location.LocationLine5 = App_Line5;
				location.LocationLine6 = App_Line6;
				var result = consumerService.GetConsumerInfo(App_FirstName, App_Surname, App_Gender, App_DateOfBirth, null, location,"PL" , customerId, 0); // TODO: verify the null in 5th parameter is ok

				if (result.IsError)
				{
					ExperianConsumerError = result.Error;
				}
				else
				{
					ExperianConsumerScore = result.BureauScore;
					ExperianBirthDate = result.BirthDate;
				}

				if (!string.IsNullOrEmpty(ExperianConsumerError) && App_TimeAtAddress == 1 && !string.IsNullOrEmpty(App_Line6Prev)))
				{
					var consumerServiceForPrev = new ConsumerService();
					var prevLocation = new InputLocationDetailsMultiLineLocation();
					prevLocation.LocationLine1 = App_Line1Prev;
					prevLocation.LocationLine2 = App_Line2Prev;
					prevLocation.LocationLine3 = App_Line3Prev;
					prevLocation.LocationLine4 = App_Line4Prev;
					prevLocation.LocationLine5 = App_Line5Prev;
					prevLocation.LocationLine6 = App_Line6Prev;

					result = consumerServiceForPrev.GetConsumerInfo(App_FirstName, App_Surname, App_Gender, App_DateOfBirth, null, location, "PL", customerId, 0); // TODO: verify the null in 5th parameter is ok

					if (result.IsError)
					{
						ExperianConsumerErrorPrev = result.Error;
					}
					else
					{
						ExperianConsumerScore = result.BureauScore;
						ExperianBirthDate = result.BirthDate;
					}
				}

				if (ExperianBirthDate.Year == 1900 && ExperianBirthDate.Month == 1 && ExperianBirthDate.Day == 1)
				{
					ExperianBirthDate = App_DateOfBirth;
				}

				MinExperianScore = ExperianConsumerScore;
				Inintial_ExperianConsumerScore = ExperianConsumerScore;
				ExperianScoreConsumer = ExperianConsumerScore;

				// Continue with 2 calls to UpdateExperianConsumer
			}

			// Continue with MP_GetScoreCardData...
		}

		private bool WaitForMarketplacesToFinishUpdates(int customerId)
		{
			bool isUpdated = false; // TODO: get from MP_IsCustomerMarketPlacesUpdated
			int totalTimeToWaitInSeconds = 43200; //TODO put in ConfigurationVariables
			int intervalInMilliseconds = 300000; //TODO put in ConfigurationVariables
			DateTime startWaitingTime = DateTime.UtcNow;

			while (!isUpdated)
			{
				if ((DateTime.UtcNow - startWaitingTime).TotalSeconds > totalTimeToWaitInSeconds)
				{
					return false;
				}

				Thread.Sleep(intervalInMilliseconds);
				isUpdated = false; // TODO: get from MP_IsCustomerMarketPlacesUpdated
			}

			return true;
		}


		private void SendMailViaMandrill(Dictionary<string, string> variables, string toAddress, string ccAddress, string templateName, string subject)
		{
			var sendStatus = mail.Send(variables, toAddress, templateName, subject, ccAddress);
			var renderedHtml = mail.GetRenderedTemplate(variables, templateName);

			if (sendStatus == null || renderedHtml == null)
			{
				log.ErrorFormat("Failed sending mail. template:{0} to:{1} cc:{2}", templateName, toAddress, ccAddress);
				return;
			}

			// TODO: change column FileName to TemplateName, remove unused columns and other export tables
			string sp =
				"INSERT INTO Export_Results (FileName, BinaryBody, FileType, CreationDate, SourceTemplateId, ApplicationId, Status, StatusMode, NodeName, SignedDocumentId) VALUES ('Thank you for registering with EZBOB!(2013-11-07_11-23-53).docx', 0x504B0304140000000800FB5A6743982A25FA4C020000EE04000011000000776F72642F73657474696E67732E786D6C9D54DB6ED34010FD95C8EFADD322018A9A56695044D50B112EF479B21EDBABECEEAC76C7B9F06B3CF049FC42C737E207842A9E9278E69C39172BBF7FFEBABA395833D961889ADC3CB9389F2613748A72EDCA79527371F63199440697832187F3E48831B9B9BEDACF2232CB529C08818B3392EDE066515568219E59AD02452AF84C919D51516885FD47D223C23CA998FD2C4D7BD0397974322B285860F919CAB4837C22555B749C5E4EA7EFD3800658E4C64AFB38B0D9FF6593613590ECFE656267CDB0B7BF98BEC1EE9E42FE07F116790DC0075218A3246BCD2050BB81269AB7F074A307BD09108E2392A6B61F4476B29F790C4A2290C6A7D278DA4CD06E30CF8E91D1AEC871EC9E8A1E2A320646419501AC05294E1904D7E3221F0DAEC1E1AA95B0D28631C8F20E44ECBBD5F4A2DFCBE989F83980DA3ED20E7BFA1C0BA80D3FC32663F203EAC3E520CAD1BA768AEBB6F07B0C4E82E926AA02E192539907254F97A239901928DA6B4BB23E489A3D9974E3819B6F75C417095BA2B994D306B3C6C3D7DA0CB2CA40FB45CD5468EE1E08E0A9B167DA4D71FAA0238F7C7D8B78E7724974119F6A093288A2C1D440B0307CAF5DA46DFDA01DDE0684EDE8241843FBC60B7E2932B09DA43BD7CA1B1DCA6ADF5AEAAEB5B18CA6BD66C9227290CA316FF1C3896EFA4C2B1DA21475C0FC45E75C2DD1989E44476FE0F8195C599BD3425FB4379AD7652B7CE1F2B5E4FF08613B3AFF1D032F8C2E5DC3F8A2B9CAFC68DA0247D22445D50BFC1B87783F6C0EA7F45CD4FD0BB006D1DF570AF2AEE74B32B760C0A996283D359D9EFEA7AE5F01504B0304140000000800FB5A67431F7AB12F5D0100001F050000130000005B436F6E74656E745F54797065735D2E786D6CB5944D4EC3301085AF62798B12171608A1A65D005BA8442FE0DA93D4E03FD9EEDFD9587024AEC024A5015569D32258C66FDE7BDF38513EDEDE87E3B5D16409212A670B7A990F28012B9C54B62AE82295D90D2531712BB976160ABA8148C7A3E174E32112F4DA58D0794AFE96B128E66078CC9D078B4AE982E1091F43C53C17AFBC027635185C33E16C029BB25467D0D1F01E4ABED0893CACF178CBF1E2A1A2E46E3B58771554993A602BB04E93B7DD9EE6BCDB1240C73D0FF75E2BC113EA6C69E5DE36D9D726393A9B9938573E5EE0C0A18A463AD8B0333EE12B084A0299F0901EB9C131269D9804E72343437E3CA603D495A51280190B83961C6A220932F318092129F8A63E5A2E5C80F3DB77D754BB4FAD5CB920590B7C76E5DEC2751A160B8811BF65A3F356315CD97E9012ABA77CA67FB17C1F491BDD4F11212534C5BF87D8259FC090361AFE83A0C9EDEF5FC1ECF9DFAEE147784BC29ABFDBE813504B0304140000000000FB5A6743EF09D5CFC60B0000C60B000015000000776F72642F6D656469612F696D616765322E706E6789504E470D0A1A0A0000000D49484452000000700000006408060000005EA7B4C60000001974455874536F6674776172650041646F626520496D616765526561647971C9653C0000032269545874584D4C3A636F6D2E61646F62652E786D7000000000003C3F787061636B657420626567696E3D22EFBBBF222069643D2257354D304D7043656869487A7265537A4E54637A6B633964223F3E203C783A786D706D65746120786D6C6E733A783D2261646F62653A6E733A6D6574612F2220783A786D70746B3D2241646F626520584D5020436F726520352E332D633031312036362E3134353636312C20323031322F30322F30362D31343A35363A32372020202020202020223E203C7264663A52444620786D6C6E733A7264663D22687474703A2F2F7777772E77332E6F72672F313939392F30322F32322D7264662D73796E7461782D6E7323223E203C7264663A4465736372697074696F6E207264663A61626F75743D222220786D6C6E733A786D703D22687474703A2F2F6E732E61646F62652E636F6D2F7861702F312E302F2220786D6C6E733A786D704D4D3D22687474703A2F2F6E732E61646F62652E636F6D2F7861702F312E302F6D6D2F2220786D6C6E733A73745265663D22687474703A2F2F6E732E61646F62652E636F6D2F7861702F312E302F73547970652F5265736F75726365526566232220786D703A43726561746F72546F6F6C3D2241646F62652050686F746F73686F7020435336202857696E646F7773292220786D704D4D3A496E7374616E636549443D22786D702E6969643A42413546394635324237454631314532384441434441384531383742333541312220786D704D4D3A446F63756D656E7449443D22786D702E6469643A4241354639463533423745463131453238444143444138453138374233354131223E203C786D704D4D3A4465726976656446726F6D2073745265663A696E7374616E636549443D22786D702E6969643A4241354639463530423745463131453238444143444138453138374233354131222073745265663A646F63756D656E7449443D22786D702E6469643A4241354639463531423745463131453238444143444138453138374233354131222F3E203C2F7264663A4465736372697074696F6E3E203C2F7264663A5244463E203C2F783A786D706D6574613E203C3F787061636B657420656E643D2272223F3EF5F661CE0000083A4944415478DAEC5D096C1645147E40697F410E4120C58B434B234A206915094754A0D62322823128D1A84824A8048347240615BCE21D4582178A092A08448245F10802C65240914B45014528022A0295C3D0FA3EF7991462E7BF666766FF9D2FF992B6FB777776BF7D33EFCD7B337FA3BABA3AF2882E1AFB47E005F4F0027A7801BD801E5E400F2FA087173056C8CBF604D515A5B65FC08ECC2ECC22E619F27B7B660B663326662A1A310F336B98BB993B983F3337317F646E631EB1710385E5557605348C53980398A5221684EBCA3C318B731E626E65FEC0FC89F90DF353F93DF72DD000FA304B981733FB314FD27CFE04B358585FD44AE622264C64A92D0B8DAA80AD9843983731FB5BB87E422C7D80FCBE8E395358ED9D98868171EC15E65AE60C4BE2FD1FCE613E2E42CE675EE0053C16E789602B9837334F73B46768C3BC92B98CB9407E8EB580F01827CB187383749D5109BF2E176B7C93D93D8E02C2D2BE66DECFCC8F70283692B9525EC4441C0484FBBF58C6BA7639124F27E4455C219E72CE0A780D733973608E4E8C9CCBFC9879B7290FDF948079D2C5BC23C1782E235F3C56841C6D7341405C638E743171C2B5CCCF2898D68BAC801D98152EB8DB16BBD425CC5E51141021C25BCCC1146F14CBD0D12B4A02E6CB1830903C80B3987399A74645C059CC8BBC6EC7A0137321B3B96EEF50371E660EB5F4906A991F313FA120C7B787B99F829C20EEB5A5C49E5DA477E867B87D3D98EF4A3855E3A2807056265A100EA99E79CC67284803A5828728C83620662BA720E96B029732C731A7B8D6859ECE9C6A413CA4770689DB5E99E6FFC243BC4C1CAD4D06DBFCA02EE74EA7804F88E76912DF337B333FCFF23C983DE92F5DAF0934613EC92C704540740B571B16EF17E6080A6A5B7460A79CCF94252246BCCF05019B88F599CEEEE3E657693EE72EE670E64143F770477545693BDB020E23F3F930CCFABF1DD2B9D78478EEE381FA9E49D99CA05136EB03F9ED815B8EA29F22C3025E454132550564F9AF97F80BE99E7DD23DBE2E63A70AA874FB8A82D2C4B08102AA92C2F2AAF536C288320BE2FD49414A4A05248B9F6B20681ECD1C93C4CA502BBA40C6C4B09110FF212301B3ED428758081B102AEC4EE21C4C57CC78A0DB9A252F9F0A4B0DDED348EECD124605E40B76B624E0E224F7F36C8AF7757B0AF1A5299C99E9B3CCC60261F6CD2C08B85271AC33A55E8A8862E1D68AE3FB0DDFD760D3029658100F03FE16C5F18E698CEB2727997830BDFB430FEED5D2D6232327862FD454C61AD3407C8639CB560D3CF074DA84F953D5847281690199DD981B4D78A1DD84A6D14A9C98DA068EA7B3C805F1DE36C5F1B686EF0D46D1D794803D6506C634D0C5E8AA3199A9781188EC14EB22767DD9C418D893A20D947ABCA4380EE76CB88576159B7262BA46583CACFDBB25899332565C7BD368614AC00E11156FBEC45B87159F416831CE52FB9A9912B06504C5C3D8322C497C8771FD5566A1A536E69B12306A9B23A07CE256E6D1249F4379C5508BED4CBBAC238F721FA8087F2485CFDD99E2E79C42A696743422F7373F4551B06EE36907DA5B67CA02F74540BC3D293A23E3C99D751B874C59607504049C46C1B6212ADCC67CCAA136D79812F0BB0808B82CC971CCFEBFE0589B7F3725E05AC7C53BC0DCA0388EE4E92407BDE90DA6045C4D8E6E7C2340C9E14EC5F14BC8A1AD42D2E835B409889A918D0E0B88BA99BF15C72F74B4D7586E44C0C2F22A8411EB1D1610A999360ABAB8B9C22A7EAE3B4C06F2A8CD1CE1A880488EAA2AAC9BBB28A0C9401EC06AA0BF1C15302F89051638D8E6F78D0AC8E68E350973C94307965286658CD9BAD1F322F0705061FD06737612CFD42666B241D4DA1010359AAE6E8C0AB14651503D772305AB62B1D1C06B8EB5F337E60799FE735602F25B83DCDA4407C5C39C22D64F603BAFDAE34445D9FD5487DA3A999FE3762B020A663B18523CC6FC5271FC2E47E2D8ADA4AECD312220DEF007C8AD14D3C2142C748103ED9CC2D677D8B68024DEE81C47C4434E2D9587B2DB723B1749174F2E08084C70C4CB435942EB143E5764B18DC8A78ED771229D02A2CA79AC2356986CE511CA226DD6BE4CD03506EB4EA7BCC77CD10101872B448475CE20F3A5F3FF014ECB745D270B231FF6852356F83C052582D8A70C6B26500A3948663CFA5A6A139ECD189D270CA32AAD2DB9037CEFC408715870AF8516DBB25A6253725D40D7BE3220E1409B56CA8BB44BF789C3E8428BC8A33EBE655E47216D20A4DB02511ADEDD6B76CC9887EF97F823AC0B340EC1FA3A79DDFE0582F4B230C50B43C0628A47B9BE0AA8EDBC97824CC881B02F9617828071063CCDD1A4DE49C3690B2C8BA970987BC5847E6F93E2E9B6C0B329F866CDB8610605F9C52A1B17D72920B65B2E888968286A46CA0A53628B6C3644A7807D62201CAAF050FEF0A88C77D6A14B40EC973D20878543C53476B6C0D6CC4ED500E912102E73987B6B227F867503D85CC8C40E19DBE57AD88B1B05B795AEBE593A04440235ACDC1AC61A6C447E0F052BA24EA0E0FB6CB130E57C0ABEFF01DB472245D4528EA7031465EDA56059D74EF120978843B2370A5D830E014B48FFB65B186B502C85EF815853EFEF07E5E1D6F7F8F245386441B0FD09D63DB41751EB6FBD552B968C852FBF5290A1D82E7F43F05D4711840E0131EBA06BDB2D54B74D134761731A567A4484D94C314356025657946277C02BB26CC366E9B6160A0F9187310BC4C6394D33140D25EFB3248EAAF152181690AD0F1BA68E4EE35FB0533C6A31E78A7807FDE3B76B81A348BD67DA1671C3E182AF13C7C3778F2E08C8D6D7FA38EBC364EE46E12619D3B000F4807FC46E5A60B9B8E11F5250E55529331475FE919A4556DFDCE2611F8DFD23F0027A78013DBC805E400F2FA08717D0C30BE805F4880EFE116000855990C5141725860000000049454E44AE426082504B0304140000000800FB5A674317E459D6E60E00003213000016000000776F72642F6D656469612F696D616765362E6A7065679D57075453D9D63EF7A6110825041010E412A49B4028018248092D14A982608D49800829A4D01415077104461111046CA0A2888A15101B562C0CF60A3A28837D1447511147FE1B1C9537FF73DEBCF7ADACACEFEE72EE3EBB9C75EEF0ADE1FBC030204B980400876307D4C0370C3F07647F252F351B40008B3EF351D17D8C7D8A422165393888E5742E5F324740E749440E595CA90383EEE800267A6749B9BC548102992348168ABDA82F0F1DA32242BE1735DE35DC315CCA16A40883736482989CC9B1BC9C549E079FEA3D099998C5CA124945020517C912A589E5AC2C2FEAC8E22C94ABC40E5464C44491EA45F5552990A9E191085B221320AE74671ACF91C140984C3AC3C595C9644C409C1C194E0E8EE88F4963B8B05C992C2737E44F50D1B7C9F849AC68FFC03FDF853E7951FFDC546666263DD3992E91253B303C3C3C546B3839D1500B9A3C5BACE066D1C4728B2F2BF80BE43C9950AA104AC488EA993B47A2547851A95FB620927E5DF6DFE66A946178F8DF9B8A445FADE58A6841D2DF5BCB63B3A5028768815CA294F104A8B985CA59CA62CB045C8544162B91A47DC962648A442191A748A4083B8689D8C40BC57C49A6DC76C43E3C9CC511CB155C314FC0F1F7A2A212BA50C867B906B8B8383B39FBBAB938BB3018014EBE01BECE7EBE014C470F06C3C3D7D5F58BAFBF84A71409C48A2FBEFC6FBE7EDFF555B5C2676F814C9821E007CA24226464CB2CE1F76371FF7E2C9F7DF9DF8FC5E3BBBE0E68300E7F29F41711DA3D2AFAB56DD187AF8D2F10A3DD2E43DB7AF801692A87CD9E15191D11C8090B0010040029384C2896C03A0088C40A5974901F323521112174021810011E3000E0F2E4D2F098C058D5047202D8881C3502FF82B737D081447195161C8920E0BF039927952900802251EECC477786F27C94A7652AA42A793FCAF5E6A4AA38AC1A7A3D191A20CA0D553CF9339F3062F399FBA8385F24460F075815B3942FE2ABF82994FF98A114A01C1386F2820CA12013E5D7503E3E4D2912A2FCBDCA5724E0CA01C0925472858097827247949364B1D16C944F04408D943C8ACF19C515822C856A536C89345B264C4E5120363C5B049D5C772458909926502868916851B8323E7A5688A45C71367A8A8DEC7904BAAADC226892990C0F2693E644677CCBD3DF2BFF2154B5FDCC5E478DD40C32E8F826FB7776925A00DC07D0DC947C93CDA904A07909008677BEC9C66F04401BAD5BD3C551FB3150F5CBA8934C28E0D15509FD8AFF68F00F30EA7D74D5725FD383F80B92B8CA3405A2CA1B4F92869E3E881C9D090142FB6B13FFCF8EA3302A8E09E88C0B6402F47040E2D02E138A93D1728BF9C291A35928FE5E11FF47B7BFE0735FA3A06CFE04F466D181CE453D80F9AD0360291A00337D3DAA81BED62D8C18075493176FF6E873DF8F00FAFFABC22B547F7261F2881F3B3A16E12965199F75AAB10438A00EB4811E3002E38005B00134E004DC8027F001012004448058900066021E480122200399603EC80305A0089480D5A00A6C009B413DD805F6826670181C0767C00570095C075DA007F48167A01FBC0543100411204D88021941669025640F3941EED02428000A83A2A1046836940C892125341FFA012A824AA12AA806AA87F64007A1E3D039E832741BBA0F3D817E873EC0189804EBC1A6B015EC00BBC3BE70281C0BCF8093E1743807CE8797C36BE05A7807DC041F872FC0D7E11EF8193C8001180D8C01C61C43C3B863D898084C22260923C32CC01462CA31B5985D98564C3BE62AA607F31C3388C5632958044BC37A6283B153B03C6C3A7601B6185B85DD866DC29EC25EC5DEC7F6633FE1347126387B1C0BC7C14DC525E3327105B8725C1DEE00EE34EE3AAE0FF7168FC71BE0ADF16EF8607C027E2E7E1EBE18BF0EDF886FC35FC6F7E20708048211C19EE045882070090A4201A192B083708C7085D04778AFA6A166A6E6A416A896A826565BAC56AEB65DEDA8DA15B5476A43441DA22591458C20F289D9C415C4CDC456E245621F71489DAC6EADEEA51EAB3E573D4F7D8DFA2EF5D3EADDEAAF353434A81A1E1A511A428D451A6B34766B9CD5B8AF3148D225D991D8A4E924256939692BA98D749BF45A5353D34AD347335153A1B95CB35EF3A4E63DCDF75A142DBA16478BAFB550AB5AAB49EB8AD60B6DA2B6A5B6AFF64CED1CED72ED7DDA17B59FEB1075AC74D83A5C9D053AD53A07756EEA0C9029640639822C221793B793CF911FEB1274AD740374F9BAF9BA9B744FEAF65230140B0A9BC2A3FC40D94C394DE9D3C3EB59EB71F4E6EA15E9EDD4EBD4EBD7D7D577D18FD3CFD2AFD63FA2DF638031B032E018A419AC30D86B70C3E0C318D331BE630463968DD935E6CA987786630D7D0C058685868D86D70D3F1821460146A9462B8D9A8DEE1A638DED8CA38C338DD71B9F367E3E566FACE758DED8C2B17BC7DE31814DEC4CA24DE6996C32E93019301D671A642A35AD343D69FA7C9CC1389F7173C7958D3B3AEE8919C56C9299D0ACCCEC98D953441FF145D29035C829A4DFDCC43CD85C695E63DE693E44B5A64EA12EA63652EF5AA85BB85B245994599CB0E81F6F363E7CFCFCF10DE3EF58122DDD2D532C2B2CDB2DDF59595BC55B2DB56AB67A6C6D68CDB1CEB16EB0EEB6D1B4F1B649B7A9B5B9668BB775B74DB55D677BC90EB673B54BB1ABB6BB680FDB33ED85F6EBEC2F4FC04DF098209E503BE1268D44F3A565D01A68F7E906F430FA627A33FD85C378874487950EED0E9F1C5D1DD31C373B76317419218CC58C56C6EF4E764E3CA76AA76BCE9ACE81CE0B9D5B9C5FB9D8BB085CD6BBDC72A5B886BB2E753DE1FA07D38D2963EE623E711BEF36DB6DADDB4D773DF748F762F7B31E380F3F8F851E873D06594C9682B597F5D293E699EAB9DDF3F144EB8982899B27F67A51BDB85E355E3D939049B3276D9CD4E36DEECDF5AEF57EE063E1C3F7A9F379E46BEB3BD77787EF0B3F473F99DF01BF776C163B97DDE68FF10FF22FF4EF0CD00D98125015702F901A981CD810D81FE41A342FA82D18171C1ABC32F826C794C3E3D473FA43DC4272434E8592426342AB421F84D985C9C25AC3E1F090F055E1DD932D278B27374780084EC4AA88BB91D691E99187A2F0519151D5510FA319D1F3A3DB632831B362B6C7BC8DF58B5D11DB35C5668A72CA8938EDB8E971F571EFE2FDE34BE37BA63A4CCD9D7A21C1384198D09248488C4BAC4B1C9816306DF5B4BEE9AED30BA6DF98613D236BC6B999C633D3661E99A53D8B3B6BDF6CDCECF8D9DB677FE446706BB903733873D6CEE9E7B17915BC677C1F7E19FF89C04B502A7894E495549AF438D92B7955F29314EF94F294E742B6B04AF86A6EF0DC0D73DFA546A46E4D1D4E8B4F6B14A989668B0E8A75C5A9E2539271922CC965A9BDB440DA93CE4A5F9DDE2F0B95D5C921F90C798B420FBD4C75286D944B94F73326655467BCCF8CCBDC9745CE12677564DB652FCB7E941398B3651E761E6FDE89F9E6F3F3E6DFCFF5CDAD59002D98B3E0C4428B85F90BFB16052DDA96A79E979AF7F362C7C5A58BDFFC10FF436BBE69FEA2FCDE25414B1A0AB40A640537977A2EDDF023F647E18F9DCB9C97552EFB54C82F3C5FE458545EF4B198577CFE27C64F6B7E1A5E9EB4BC730573C5FA127C89B8E4C64AEF95DB4AC9A539A5BDABC2573595216585656F56CF5A7DAEDCA57C43857A85B2A2674DD89A96CAF19525951FAB52AAAE57FB5537AE3559BB6CEDBB75FC7557D6FBACDFB5C17443D1860F1B851B6FD504D534D55AD5966FC26FCAD8F47073DCE6F62DEE5BEAEB8CEB8AEAFED82ADEDAB32D7ADBA97AB7FAFAED26DB5734C00DCA86273BA6EFB8B4D37F67CB2EDAAE9A4683C6A2DD60B772F7D33DB3F7DCD81BBAF7C43EF77DBBF65BEE5F7B8072A0B0096ACA6EEA6F4E69EE694968B97C30E4E08956CFD60387E887B61E363F5C7D44FFC88AA3EA47F38F0E1FCB3936D0266D7B7E3CF978EF895927BA4E4E3D79ED54D4A9CED3A1A7CF9E093C73B2DDB7FDD859AFB387CFB1CE1D3CEF7EBEF902F34253876BC7819F5D7F3ED0C9EC6CBAE876B1E592C7A5D6CB132F1FBDE27DE5F855FFAB67AE71AE5DB83EF9FAE51B536EDCBA39FD66CF2DFEADC7B7D36EBFBA937167A86B5137AEBBF0AECEDDF27B26F76A7FB1FDA5B187D973E4BEFFFD8E07310FBA7A79BDCF7E95FFFAB12FFFA1E6C3F247668FEA1F3B3D3EFC24F0C9A5A7D39EF63D933E1B7A5EF01BF9B7B52F6C5EEC7FE9F3B2A37F6A7FDF2BD9ABE1DF8B5F1BBDDEFAC6E5CD8981C8817B6F456F87DE15BE377ABF6DD07DB0FD43FC874743991F091FD7FC61FB47EBA7D04FDDC3A2E1E1E117406BE4030EF0DB54F787E12E908F7E5EFC63C0DF80F98A2FBA1135345AF30F31DC06C86A000108062203980C61C8D0F0292045AF37987FBDE1E0D5880448258546CB096A443C561DFD32FAACFB2286600C1687270035B2AEBE391940D891A8A1D15A40D6C55308E67A5C2703FDE13B80846A6132860CBCC183DCD583037797F34CCE7814CB8D9AD7E0BDCBDA9656BCA039875544BD0EDF645CB67660F9BAF464CDA48A0E857F282F3D857890D070307BAD416D6C6D97EF7E8B580E931BB9C5BE59A3ECD7DDEB920ACBDDDEBFBEAC4BEAAC7B83A39F67D7D0ED3C7B2BE555CB2C57185487DA12A1ABBBE16B1B035387E26F993EBC3A34F308F5E18755DD2587286903858F8B323CE31CB56F8649D475CA6A4FFE260E5D3523A6C1B11DDEE2785560573338B0F0E7D70956BF78880F74ADB97D29373AA1E4CC43BDF60BBE6EF372560650A2F3CE54C447870E981F37BB14BCA37715BF323FDF2EDFD69F57D872243AC0B278D9C5499579E12BCF78FA6F1C53462892345F285576A4FBF8BC1F8BA111529BDEE132739F4D29739CED1952B705373861CBC6499BFDD2361C3CD31A2C39F5D21B2E6AACEF035BF46C83622FB64CD863F8D05930F7ADDD27F6C61842478599F59ED7B977F2C28BAFFDAA9CB6139EFC242D2F772C692866F1ED2763880714B2B3BAB987DA8446330AA97EDDDB303EACA5DE1DAFB562133DDD87C1D96321EBEBA9CE9C927AADBD5BA7FB8690F24384D72B06E5EA0FBA34F3DA576896CA278A9F8A0E0D83D83DE31E35BE140F2D6EBA316DF54F37D86BBBDD1B6BDA57260EF89F2D2E1C0646DCF031FC88D2291DADB36C436276ACBB5C5ED772A951EC5CFE32E1D185EE1DCFA212830B0B4A5227C7895E3DA3FD62C4338DCA2AB62DB1DB904EC96D5BE01F347469E7F68D765742B62F5E35759674AFFE10FEE00643CFC15B81CD83176E5F1BFB3C5CC96A3A24EB923759B53A56D50DF4C6EE3E8F5D629168A2D17D5E016EEC772DE9EC4F0EF8D242C3B7FF0F504B0304140000000800FB5A674300EC3F86551200008516000016000000776F72642F6D656469612F696D616765372E6A7065679D57075453D9BADE2795126A000129219450A4044820A186504504A9764A1220948021A15A1171042922A2D214AC14415410B12B8805412CA0800A2236505107051BEF04C772E75DEF9B77BF9595F5EDBFECF297BDF699BE3B3D04945D92B9E1007878180231F013D36340CE59C88A4E011040C163362C1A421A450A04F17453535E8249283B2E8C63C28A8B354D0E8D37259B9899025B87E4F8505634474008E344707976C4D727CF12095CB61D3188E265E615CFE44472DD53F91CBFD4F9FEACD468168D4D74B027D826D39363E36339825042726C0C2F819E6C479C999C0E7391D8944898311144DB1119220561A1970F8119C7E710282616C62C33329940A59A902D29542A790EC1DC8C6C6E6A06FFA8C6644B3A854A37B722FC0522BC1A9F1D4EF77576FD6B2D786447FCEB504949492649162671FC0853328D4613CD616E6E0C5B1827A4F004A1C9C6BC04EDEF33387312587C6EBC801BC72388C6A1617142811D91F8FD08B1F13FA6FDB7B1FAC5D0CBEB3F9BC6C6FEB04E10F872C2FFB375827F4A3CC7D497931027E4B338B0B9B6C8399ECEE4734205717CFFB8B898EF51F4898C13C42544C6C513987E54827E1097C78E4B4A3098B1F7F2A27BF01204A13C16C7C3D98E084B4CB85C369D426330AC2C694E5696169664B28BB913D3D9DCDA9566666EC9A031AD688CEFBECE712C612C8727F8EECBFEE9CBFCADAFA814BE7973F8DC440EDB951F174B9839329DFBFBBDD07EBF976FBEECDFEF85F15B5F537833A67F4BF477115C3D22FAA36CE1C18FC2E7F0E06AE7C3653DFD08B7D083C90CF6F1F576F598E7022008009CFB3C2E2F0E210B402C4FC0F77573222C5CB49880ED0408200E30800C40282B21DECBCFD55FD4811E2E4C42026C04FE05EF7BE0868471CBD8DD874000FF3FC8B1E2F90200201F985BB0E193C13C03E631498278917C1CE60A61D1228E1035BD021FDE20CC95453CE21B9F3363F38D3B8A383B96075F0E08D19EE3D9B16C116F85F91F89420ECC91F3609E99C8E524C1FC36CC756284B15C984F8A7C6339A10900A07022B980C38A84B919CC717C7F5F26CC6D0110C345FCC2C37EE1024EB2407428665C7C0A9F1B112920E8B30C0870E75A13DC3949311C81C0D8074E4A289F0DDF15B1F1A1BC14F8169B39F30CE445B125C041A6926954AAB1B909F9679CFEB3F21F4294DB6FECDD82999C414A1D3F65FFCE2EAE1200EB093836F93F65613B0138BE0100E5BE9F329DDD00C8C0796BEAFAE53C4AA27AF9E526E3725826A280FEC0FF69F00FF0CB7A26A2E97E8487E0CC090F15C60808A2B8B1E262E0DB879000F7048760FCF722FEAF1D7FC12FFB9803F73887CF812F0742205C655C5E049C6E1E9B3B73357379BF4BE27FE9F6377CAB6B18F8BD5F8142B00990ED5200C8971D00859704C8A5E5B006FA91B779E28140D479419A4FBFD5FD0CA0FF3D2B224FF497C08D98F163FAFA1358427EE2379DA82D011A480019A000548006D006FAC01898032B60031C810B980BBC813F58049603168804B1800F92C04AB00E64826C900FB68162B00BEC0555A00E1C01C7C129701E5C02D7C00D7007F48341300246C138780F3E41108485A4203CA4026942BA9011640E5943F6900B340FF2851641215004C48384D04A683D940D1540C550055405354027A0F3D015A81BBA070D41CFA1B7D047041281432820D4117A085384358281F044F823962122102B10A9880C442E6207A212518B68429C475C43DC410C22461113488094442A21B590C6486B2413E98D5C8C0C47F291AB9159C8226425B20ED9826C47DE420E22C79053280C0A8F22A08C5136287754008A855A815A8DDA8C2A461D4435A15A51B75043A871D457B4145A0D6D84A6A33DD00BD111E8247426BA08BD1FDD886E43DF418FA0DF633018250C09638571C72CC24461D2309B3165987ACC394C3766183381C56255B046583BAC3736142BC0666277626BB167B137B123D8493149314D31733157B1C5623CB174B122B16AB1336237C59E8A7D129715D715A78B7B8BB3C553C4F3C4F78AB78877898F887F9290932049D849F84B4449AC93D8215127D1263120F14E5252922849935C20C9955C2BB943F2B0E465C921C9299C3CCE10C7C42DC50971B9B803B873B87BB8775252527A528E528BA50452B952555217A51E484D4AE3A54DA43DA4D9D26BA44BA49BA46F4ABF921197D19561C82C9749952992392AD32533262B2EAB27CB940D955D2D5B227B42B65776420E2F4796F3968B95DB2C572D7745EE993C565E4FDE459E2D9F21BF47FEA2FC301E89D7C633F12CFC7AFC5E7C1B7E4401A34052F0508852C85638A4D0A930AE28AF68A918A898AC58A2785A715009A9A4A7E4A114A394A77444A947E9E32CF5598C599C599B66D5CDBA39EB83F26C6547658E729672BDF21DE58F2A0415179568952D2AC755EEABA2540D5517A826A996ABB6A98ECD56986D339B353B6BF691D97D6A083543355FB534B53D6A1D6A13EA1AEA6EEAF1EA3BD52FAA8F692869386A4469146A9CD178AE89D7B4D7E46A166A9ED57C415024300831841D8456C2B8969A96BB9650AB42AB53EB1391440C20A613EB89F7B525B4ADB5C3B50BB52F688FEB68EA78E9ACD4A9D1E9D315D7B5D68DD4DDAEDBAEFB418FA417A4B751EFB8DE339232C983944AAA210DE84BE93BE8AFD0AFD4BF6D8031B03688362833B8618830A418461A96187619218CA8465CA332A3EE39E839B439BC3995737A8D71C60CE344E31AE3211325937926E926C74D5E99EA982E36DD62DA6EFAD58C621663B6D7AC9F2C4F9E4B4E27B790DF9A1B9AB3CC4BCC6F5B4859B85AACB168B678636964C9B12CB7BC4BC153BC281B2917285FA856543EB58EFADC4AC72AC4AAD4AAD75AC1DAC77AB3F5651A9AE6445B433B459BA253E902FA11FA6B1B639B689B6A9B67B6245B8EED5EDB613BA25DA85D85DDA03DC13EC47EB7FDA0839643A843A5C323476D47B6E37EC7A70C034614A396F1CAC9CC89EFD4E8F4814967AE629E73463ABB39673977BAC8BB04B814BB3C7025BA46B8D6B88EBB51DCD2DCCEB9A3DD3DDDB7B8F77AA87BB03CAA3CC6E75ACD5D35B7D513E7E9E759ECF9689EE13CFEBC162F84D75CAFAD5E03F375E7F3E61FF706DE1EDE5BBDEFFB907C56F89C5C8059E0B3A064C1135FB2EF4ADF763FBC5FB05FB5DF7B7F27FF3CFFFE00FD0061C0854099C0A58155811F829C830A8206179A2E5CB5F0DA22D545DC45CD8BB18B0317EF5F3CB1C465C9B625234B294B3397F62C232D4B5E7665B9EAF298E5A783658243838F86A0438242AA433E877A8756864E84798495868DB398ACEDAC51B623BB90FD9C63C729E03C0DB70B2F087F166117B135E279A4436451E41897C92DE6BE89728FDA15F521DA3BFA40F4744C504C7DAC586C48EC099E3C2F9AD71AA711971CD71D6F149F193FB882BE62DB8A71BE277F7F0294B02CA159A0003FA63A84FAC20DC2A144FBC492C4C9A4C0A4A3C972C9BCE48E14C3944D294F535D53F7A5A1D2586917566AAD5CB77268156355C56A6875D8EA0B6BB4D764AC1959EBB6F6E03A8975D1EBAEA79BA517A4FFB93E687D4B867AC6DA8CE10D6E1B6A32A533F999BD1B6D36EEFA03F507F78FCE4D169B766EFA9AC5CEBA9A6D965D94FD79336BF3D51C72CE8E9CE9DCF0DCCE3C6A5E793E269F97DFB3C561CBC102B982D482E1AD5E5B9B0A098559857F6E0BDE76A5C8B268D77689EDC2ED833BE6ED68DEA9B3337FE7E7E2C8E23B254E25F5A56AA59B4A3F94B1CB6E963B96D7ED52DF95BDEBE36EEEEEBB156E154D957A95457B307B12F73CD91BB8B77D9FF5BEAAFDAAFBB3F77F39C03B3078D0F7606B95555555B55A755E0DA24658F3BC7669ED8D43CE879AEB8CEB2AEA95EAB30F83C3C2C32F1A421A7A8E781EB970D4FA68DD31DD63A58DF8C6AC26A829A569FC78E4F1C1E645CDDD27E69EB8D062D3D278D2E4E481535AA74A4E2B9ECE3B237126E3CCF4D9D4B313E7E2CF8D9D8F383F7C21F842FFC585176FB72E68ED6CF36CBB7CC9F5D2C57646FBD9CB76974F5DA15F3971D5FAEAF16BD46B4D1D948EC6EB94EB8D9DD4CEA62EABAEE61BB41B2DDDB6DD676E3ADC3C7FCBF9D6A5DB1EB7AFDD997FA7BB27A0E76EEFD2DEC1BBECBBCFEEC5DC7BD397D8F7A97FED007A20EBBEECFDA2076A0F2A1F1A3CAC1FA40E9E1E721EEA78E4F7A87F98353CFA38E1F1E7918C27524F8A9E6A3EAD7A66FEECD473D7E7375E2C7931321A3FFA692CF3A5DCCBD257FAAF8EBD767CDD31BE707CE40DFFCDF4DBCDEF54DE1DF8D3F2CF0B133E130FDEC7BEFFF4216B5265F2E094F554FBC7A08F4F3F257DC67EDEF1C5E04BCB57CFAF03D3B1D3D3D3AF80F4CC071C609F13BD1FA6FB4106FC79F18F81F809E40F7CD7CDA8A15F35FF10D3E7809C1820000212920308390829074DB782D5F0F30681FCF58983159380501871588CF817051A0B0BC5104894842C80D0DFD47FA950100289C66081989C16112FAFA004CF8F80E02551DF2D603D0A0DE4305AF2583203BF600551A1422CD4C962BA0FE0E02510724839E00072CF04F1CE5C9F301C3D4D1B9BB5B23AFBF053EDCA5B23138A7F4A77A0CB8A3FF67A54DC7E3026549EA25D77D19F9A283CFD123BF92E3606E5A623EB47D9404D91776DE490DFE04E4D1EBBD63B395263581F32FF4952E296E1C90027E1B6D5D4A460864687F22B4CFE71878633F44B09FDCBE91D25FE09C7C24B17A521C46A7A6BD0F49E4BAE8B8A2F5D715AD39F792825F175DA86DD91F97BCCF0CD23EA0EB6BA6D1B3ED8F4FBAE2F2FBE9C366C6CFEC151D2D38E725D29B1CFC8E3AAB4FEBA82A64F0B162B3C376C0C333C1759363EB5C70D5DA2B8B52E4F67E289F3E6FCB62D6B7D6B0FDE6FCD2DB1A82CFBA25298BA653FCF66577C4C5BD61DF2A2CD9C922025EA3AE2515C8EF975DC7B22D9F084894E7AE3CEFC8376DD0C8B9C1E92F6DA77E7AB784D4EB81A3CAA1E95E269FD2804470E09BB42D355340A514B664519ED2DE296E744FBBD0C586DFBF88F9BD63990492BC0569176235FD79550430E2CF74C2C63D5C85B9C24DF0BF0570A27DD05E2A4BB52696F461DAACF0CDC0C799E7C2760D6EC9B725D1F56D5B36A3A73C6EAB82D86B4B459D236EBCA1842467165BFDEF26175B26BD195271E7DC3C1FAAF369C2D19981C5E195CC474317BA34B533DF55E47300B7F7A4D584375C036A5F997F212633BA741D1E8E11742DFAF31EFBA265E1B5C1D9B227EA9F398AB76C6486AFEA1CCE144AF944BDAD49D8FCB55C6BA3CDF87362B29679142653376B83FCCDA507EF145FD89D3D15B05C512D294EB268487018B21BF3545EF0D8B3E2F3462DF4BFD5255A4DFD67031A2FDC6920A8D538EA457CCF31BD847B675E6282935B6F13F07EF1869080C492F8BC0EF410B9A6AC194E1D98A4DC8A609C4EB813D4924DEDA2868B8DE761A2C59B9E631CE6489F1150BE1486B9742C1F9E83C2FA4657AF9EA83BA7B4E87D6B4F55FD6587E11F5BA7F3945DA79C386181970F5684E6D836A48AD51B60E72BD56B60A4259A74BADE240CEC3C204862D2FBAF86CE463FF774BF7BA6DEAA341F20684F52FC49E313EA92FA37806D407644D582D32F4526DA197DDE93EC7E1D80F15A6CB34D35D1E61830213CB991C5E43D3FEB30743A48F09AA6F6F6ABC10F52282C20932A46833F624BD0DF838399EAB389C6DE1EBB1E67153A5F17BE9BDD3403573CAFED0B507968FFD1BFA4B33EE1A3D577A7056FBCE36BFD896D703AE0F9F065E1E53997F2F77543D8979957C5ABA5AFD61E23E41AFDA3B3A3BEEEC81ADBD0F1C51813DAEFEB957489D1BEBEB1AD96D7347D28D30F37283FBA6C1B528FE44428079D358D8D0E16CBB91FB95CC29D317BDEAADF51FC591E9EDBB79AF137AD67E16EE7AB35D7B502D4DA6EBC8C234F9B0E6DA7E66FEE10C8659D7C8409BEADC56E38D55BA81EE8549C730F6DD6FEA9DEC393BDF1F27733A8F1E4B4FEAD8685FFB52F10F61D40DEE35653EBBF1DCF64835CACDFE9BB85986D496B4C61DEB354DA214536462CC777D099F24CD5E52D853743AD96572C5E88A24F53A47BB6EA5D949FAB2B2EC8106B3BCCB63EC97B7244B9E7C162FDDB7B383FDF8518AC63207D5ED47BCCB8F58E8677D747EC250189B72DEC245E5CA9E20F5299AEB7A97BE385A3676F593C6F3BE18434D856D6EC2F2CDB1EC7C9A2672F4EADDA5F3F14369ABCBF0995BAAEF78EFF3CEA39CA4BFB9527A4267C94A2F5ED8C9D418769843CEB2039B46971C78D449F126280767E70D1738DE57766FD40A42A5C99E7B65DFA2371E78E023FFD0FB35F7A242B1885D77573FF517CBBF6456D13371689C88B39D13139F6BFDE8193153392CE7604FC3C19C282291D49E39FC78A040515CDAE92D6136201502FBB5D867967527075F4BB06BED6AF3641EF518BBB7ED7CDCE693144EE2775C5F5AD05AE0A153905A16DEDE27C379852E358A0BB329DB0D7FAF3FDEC4EAF97029CBF16D43F735D4F0D6DD450615DB72034FD4B9446F0950B4ED282D2998B74FF7F272B67618EE6DBEDB8DF48F6D0E98496F757A5FFBF857637461C8D5378F22B58D90CD1C4E9D1821E73C367E95A5CAA09D65E3F4BDFF01504B0304140000000800FB5A67437F6BD934D10E00001F13000016000000776F72642F6D656469612F696D616765352E6A7065679D57075453D9D63EF7A6D3430001412EA183014209218894D042912A4D144272810829A448B12BA282238A8A28565414B13B826243C4AED8B0379041451D07C701BBBC1B1CCB9BF79C7FFEF7ADACACEFECB3F729BBAD73076F0E7601E3E0425116003C9E2320836F187C0EA8412A416E1180001E1B0B315117CE2947A994715C5C240A67BE509A893A0BA4629742BECC85E9ECEA0246F915CAF8825C548964A2D922892FFDB78347E98848E84B4FF28C728D9271D11C5158B11C8D2F1E932028CE15780BE97EA39151859C42B14C8C2AF948A1384FA2E014FAD28716E7605C2D76A123432ACA5C5F7A807A02498E8A41B852398A783ABB3304AE4C26C26239333D3C592CE648C4CD95E9E6E28AFD580CA607C793C571F342FE041DDB4D2ECCE2C40585FCB91736F2A5FF79A9828202E7027767A93CDB85E9EDEDAD5EC3CD8D81693014451225BF902151587D5921085508E4229952249520EA313F53AA52FAD2E95FAE20967D5DF6BFFAEA3BC5A8A8BF57158BBF6A2B947168D6DF6B2B128A64A84B1CAA90AAE4021453B7521BCB385C39CA574AE5095269DE172FC6E4489552458E548670E359887D9248229416281C86F4A3A2383C8942C99708505E902F1D93388B44428E0737C095E5C50EF4F270F7603283DDD8C16C6F6FA6BB27D73D048B826BC017DB20A940254625CA2FB6C26FB6DC1FDAAA53E1B3352A174D42852172A91819BA3247F4E3B378FFF82C9F6D853F3E4BC00F6D5DB0C3B8FC25D05F4458F6A8E9D7B4C5065F131F9560D92EC7D27AF0A156328FCB4D8F898B0EE14506030802402B2C522491C27A0088254A795C6820929C928A90DA010C2880089800F0050A59547C4882BA0279C15C448129817FC3C075AC20315C6584C52008F8FF812A90C9950040311877176237C37809C6F30A9432B5BC0FE30699B96A0EAB8BDE408E1D10E3C66A9EFD998F1CD2F9CCFDD55C289660CD01569F5926140BD5BC0DE3F326A9508CE322315E3A49841660BC03E3D6792AB108E36FD4B66294AF0000AFA5962B51410EC65D31AE254F88E3627C140064ADECEF78E6775C89162AD597E24A65457251768E12B117382058E5B29130B4200F552A19315850F87221D62BC432BEA408EB6243771E82BEDAB708E66416D39BC562B83933BFF9E9EF27FF21D4B1FDCC5EC50EC50C323AFF4DF6DFF4A4B500B0FB31DF2CFA26CB5C0140D31C008C6F7F9359AF0340178B5BE3C5EFEE63A4CE97EF3A99081538AB1DFA15FFA7C23FC077FB39AB97FBEA1E2408CDE2ABF29488DA6F02691ED67D1005561328C2F86B12FFCF86DFE1BB738CC46A1C95A358734012B12C1349B2B1704B84A2A1D62C92FC2888FFA3D95FF039AF31D0367E0206E9CE40EFA201C0FD7A1EE0699A0097B6069B81BEC62D929208D4959764F1F873DE0F01FACF55E10AF59F42943D64C78D4B40042AF9A4CF73EAB20404A0017481013001238015B0070CE006BC800FF007C1201C4483049002260001C80162200705600A98094A41395804968195602DD808EAC10EB007348143E0183809CE814BE01AB8033A410F7806FAC000780F411009D2866890096401D9404E901BC4864643C150241407A540195036248154D0146836540E55422BA1F5503DB41B3A001D83CE4097A15B5017D40BFD0EBD8371B0166C009BC3B6B00BCC8603E00838011E0F67C3F970315C022F84ABE15A781BDC081F83CFC1D7E04EF819DC8F03384D9C11CE12C7C0B1715C5C342E15978593E3A6E1CA7055B85ADC0E5C33EE14EE2AAE13F71CF7164FC4D3F0089E81F7C187E1C7E205F87CFC34FC02FC4AFC167C23BE0D7F15DF85EFC37F226813CC084E040E8147482664130A08A5842A421D611FE104E11AA187304024128D8876442F6218318538913899B880B89AB893D842BC4CEC26F6934824139213C997144DE29394A452D20AD236D251D215520FE90D59936C417623879053C912F22C7215792BF908F90AF931F93D458F6243E150A229424A11A582B291D24CB948E9A1BCD7A06AD869F86A24684CD498A951ADB143E384C65D8D579A9A9A744D6FCD584D91E60CCD6ACD5D9AA735BB34DF6AE96B396A71B5D2B4545A0BB5366BB568DDD27AA5ADAD6DABEDAF9DAAADD45EA85DAF7D5CFBBEF61B1D9A8EB30E4F47A8335DA746A751E78ACE0B5D8AAE8D6E80EE04DD62DD2ADDBDBA17759FEB51F46CF5B87A7CBD697A357A07F46EE8F553695426359A2AA62EA06EA59EA13ED127E9DBEA07EB0BF54BF437E81FD7EFA6E16856342E4D409B4DDB483B41EB31201AD819F00C261A941B6C376837E833D437F4304C342C34AC313C6CD8698433B235E219E5195518ED31BA6EF46E98F9B08061E8B0F9C3760CBB32ECB5F170637F63D4B8CC78A7F135E377268849B049AEC9629326937BA6785347D358D302D335A6274C9F0F3718EE335C30BC6CF89EE1B7CD603347B338B3C9661BCCCE9BF59B8F300F359799AF303F6EFE7C84D108FF1113472C1D716444AF05CD62B485C862A9C5518BA788211280E421D5481BD26769661966A9B25C6FD96EF99E6E471F4B9F45DF49BF67A561C5B6CAB25A6AD56AD5676D611D653DC5BAC1FAB60DC5866D9363B3DCE694CD6B5B3BDB24DBB9B64DB64FEC8CED7876C5760D7677EDB5EDFDECF3ED6BED3B1C880E6C875C87D50E971C61474FC71CC71AC78B4EB013CB49E4B4DAE9F248C248EF919291B5236F30B418018C498C064697B39173A4F32CE726E7172ED62EA92E8B5D4EB97C72F574CD73DDE87A87A9CF0C67CE6236337F77737413B8D5B875B86BBB87B84F77DFEFFED2C3C903F558E371D393E619E539D7B3D5F323CB8B2567ED60F57A597B6578ADF2BAC13660C7B017B04F7B13BC03BDA77B1FF27ECB6171949C3D9CDF7C183EB93E5B7D9E8CB21B858EDA38AADB97EECBF75DEFDB391A199D317ADDE84E3F4B3FBE5FADDF437F2B7FA17F9DFFE30087808901DB025E04BA06CA03F705BEE672B853B92D41B8A0D0A0B2A0F660FDE0B1C12B83EF87D043B2431A42FA423D432787B68411C222C21687DDE099F304BC7A5E5FB857F8D4F0B608AD88F88895110F231D23E591CD51705478D492A8BB636CC648C6344583685EF492E87B317631F931076389B131B135B18FE2987153E24EC5D3E2D3E3B7C60F2404265424DC196B3F5635B6355137312DB13EF175525052655267B24BF2D4E47329A629A294FDA9A4D4C4D4BAD4FE71C1E3968DEB49F34C2B4DBB3EDE6E7CE1F833134C27E44D389CAE9BCE4FDF9B41C848CAD89AF1811FCDAFE5F767F2325765F609B882E58267427FE152612FEA8B56A28FB37CB32AB39E64FB662FC9EECDF1CBA9CA792EE28A568A5E4E0C9BB876E2EBDCE8DCCDB9837949793BC5647186F880445F922B69938E90164A2FCB9C64A5B2CE7C4EFEB2FC3E7984BC4E0129C62BF62B0DB0C7D47995BD6A8EAA6BD2E8493593DE142416EC2DA4164A0ACF173916CD2F7A5C1C52BC69327EB26072EB14CB2933A7744D0D98BA7E1A342D735AEB74ABE925D37B6684CED832536366EECC0BB35C6755CEFA6376D2ECE612F3921925DD7342E73494EA94CA4B6FCCF599BB761E7E9E685EFB7CF7F92BE67F2A13969D2D772DAF2AFFB040B0E0EC4FCC9FAA7F1A5C98B5B0BD8255B16611719164D1F5C57E8BB754522B8B2BBB97442D695C8A2C2D5BFAC7B2F46567AA3CAAD62ED758AE5ADE591D59BD7F85F58A452B3EACCC5979AD26B066E72AB355F357BD5E2D5C7D658DFF9A1D6BCDD796AF7DB74EB4EEE6FAD0F58DB5B6B5551B881B266D78B43171E3A94DEC4DF575A675E5751F374B36776E89DBD256EF555FBFD56C6B4503DCA06AE8DD96B6EDD2F6A0EDFB773076ACDF69B4B37C17D8A5DAF57477C6EEEB7B22F6B4EE65EFDDF1B3CDCFABF6D1F69535428D458D7D4D394D9DFB53F65F3E107EA0B5D9A779DF41E7839B0F591EAA396C78B8E288C691922383478B8FF6B7C85A9E1FCB3ED6DD9ADE7AE778F2F18EB6D8B6F61311274E9F0C3979FC54C0A9A3A77D4F1F3AC33973E02CFB6CD339D6B9C6F39EE7F75DF0BCB0AF9DD5DE78D1EBE2FE4BDE979A2F8FBA7CE48ADF95635783AE9EECE0759CBB36E6DAE5EB63AFDFBC9176A3F3A6F0E6935B79B75EDE9E74FBFD9D19770977CBEEE9DDABBA6F76BFF681C3839D9DACCEC35D415DE71FC63FBCD32DE87EF68BE2970F3D258FB41F553DB6785CFFC4EDC9A1DE90DE4B4FC73DED79267BF6FE79E9AFD45F57BDB07FF1F36FFEBF9DEF4BEEEB79297F39F8FB825726AF36FFE1F1476B7F4CFFFD01F1C0FBD7656F4CDE6C79CB7E7BEA5DD2BBC7EF0B3E903E547F74F8D8FC29E2D3DD41F1E0E0E00BA033F40107842DEAF7C3E01D50827D5EFC63C0DF80FB8A2F7343D3D0F733FF10832D804A060840701015C05408478506DB403EF6BCC1FDFB0B874CD12041FF212592C814ECBB888803D8DE5F84300E4F20920044A6EA1B502C69565400E1F0F0F70AD81850F531254B269D16C0371ABC0DB4B095612A8E0AFC40C3DDE787658ED59B98E1994B4F3876FC72B7E48C57CF98C4F1C10A5258C5BA5F0FB895F6BF5ADE23D6707272F0A6F286BFFB3926E89D0204E6940B98A97E0F57A156410DE1211903496599169CC53B1ED5D4BDB5ED8EFF70C82866CEA2401EF55A5A1E7542DAD4299722CED0579492E7171F7DAC6A1BBF39FDE5EBC235730B3E2E3CD1AF3FF504A1DC86BAD92C96EFF6BA2F5E17DE021E9A5676C7EE296A99BA3C396967E3A699EDD7231349C671F222CD570DAC32C62A6B062EBF61F906E8115BEF8AE186CBD3DDD129E917D7DE7AD699C2B83E2085EE8A875FCD65ED1E716BD4BC7570C1D9DE658BC565CB0E754F399AB425E4C1161F776E5DC1F4FD57DA2BEA339699C95A6F26EF8EB36EAE244E706FF4ABB97381FB61F5586964DCBEED17A6DAB42F48B0EBB8EAFEEBFB48B783D7AB04C75797753C438F5F62B75CAA091BB71D75AAAE4D781137EFB6C03C8D66432987C358EB79C1CFA61DD7699A77404AEB892BF2622D1E9EB22B7FFEA29664CB85BD9A672B1F566E089F9D3D8C4ADF752FDF309D375EAFCBECC19CB687BE45A613BA933BAADD176C9C7946C89B99EC0E1976C5078B0C4E073E169E2E99BA3549BF5475C8B7FFC18B8F754557F2BD724ECC4C4CD1EE7057565DCE8E3BE7291FABBD2F2555AFA33ECD7CE0A7B701BDCADBD595A981190B06A2B79326DF74E3EEEA7CD5CDBC217DA6722939BD37D9A1E4AE9C9B137BF364844D90845C3EA0DF50D574F1627379AECFB071CED11343EF48BA0F10B38E84B50C43E1D9BF6C3B078CE785661BC63F3508D93979D6B8B389E8DE0989C6750E9171EA9419BCF52F504B0304140000000800FB5A67434ED38CD74C0700006C3900000F000000776F72642F7374796C65732E786D6CED9BDD52DB3814C75FC593FB3621810498D20E9F5B6628A584CE5EEE38B682B5D8565692CBC7ABEDC53ED2BEC2CAB2EC243E96D13181E9EE2CBD2096747E928ECEFF48C4EADF7FFEF5E1D343127B3F081794A507BDADF7839E47D2808534BD3DE86572FE6EB7E709E9A7A11FB3941CF41E89E87DFAF8E17E5FC8C798084F99A7629F1FF4222917FBFDBE082292F8E23D5B9054D5CD194F7CA91EF96D9FCDE73420272CC81292CAFE703018F739897DA9BA16115D889EA1DDBBD0EE190F179C05440835D6242E78894FD35E3EBC90052764EE67B114F923BFE2E6D13CE95F672C95C2BBDFF74540E941EF86266A4697E4DEBB66899FF6540DF1853C14D46FAC8C0E53D16C168886E2FEC70F7DD377BF3EA245F5649AD5C6AFBCA47C362D7CAE6AC9FC820577249C4A5571D01BF48AC2EFE7579C324EE5E3B26C4A12FA9986214957DAA5110DC9AF1149BF0B122ECBBF9D690F9B828065A9FA3C1C4FB44B63119E3E046491AF97AA4DFD44F57C991BC479EB3F4ADB2D3D554BFB88F8796C795B7893616E225666A319596D2A1DC0A3D7026FBF1678E7B5C0E3D7024F5E0BBCFB5AE0BD4D83035F3F6F1A7B43654CDC9B4FB399445A48CED25BF7F6A7C922F20515EE1657B11F9088C521E1DE0D7990CD2EA2CBDCB6B7D786BB64DE74E107B418F4AA1DC2AD17F43692DE34D2C150E78C07CF9B5E5021819D4397BF701A02BB619BDD1712D22C29C7EA016F8D4708EB21B0DE76B0CE27DBD0F18EAB29EC75EC609ABBAAA1D789AB29EC75D7D574044C5B43F2C4E7778D1131698DA46316333ECF625B1C4E5AE3A9B26EECB835A42AD3A6689CB4C6D39A70BCC32050079786457254901DE028253B00A5293B06252E3BC65D657646ABDCAEC90F2AACFB8F7372D563B8F2B97FCBFD4554B71D6DBB67D86F1993A40E18EEB903CE53752816C46B048D06EEA0B54C6477AF7B4AB233DC73939DE19EA4EC0CB76C65B5C7A52D3BC63D7FD919EE89CCCEC06734B87720331A0420331A0474CA6810D329A3BDE4DC6067B81F20EC0CBC6C21032FDB979C2DEC0C9C6C817D37D9420C5EB69081972D64E0650BCF6D48D9420052B610D049B610D349B61083972D64E0650B1978D942065EB69081976DD7BF0DACF6DD640B3178D942065EB6908197EDF64B650B0148D9424027D9424C27D9420C5EB69081972D64E0650B1978D942065EB69081932DB0EF265B88C1CB1632F0B2850CBC6C775E2A5B0840CA16023AC916623AC91662F0B2850CBC6C21032F5BC8C0CB1632F0B2850C9C6C817D37D9420C5EB69081972D64E0653B7EA96C2100295B08E8245B88E9245B88C1CB1632F0B2850CBC6C21032F5BC8C0CB163270B205F6DD640B3178D942065EB690D11AA9F95BBC9878ABAFDA568DB73A7C8B6A630DB7DC596658D7644E384903F8A52C82558ECB0E1BBAC38E18BBF3AA37A56B94118242673165FA8BEFC7DCAAF56BF4917EE5DDF062D7CEBFF97AEC7D26D59B9E76FC9E050F26D3BF5FBB3E92F7AE2FEFA896F271A1BA5DAC7E9B1F16B74E0C4A373C0FAB7B1EB9713E5655F5C38F97D73FF49C4CBFC5C3A2B863B3380AF56FC916A54D4A4B8398CC252C9D31295902CBB9DE256AC5FD6517A278E7AB5ACCC89C71626EB1140F879964A68129F7E792F0D5CFF526BA539A865E3150D3548FA2ACEF57F334578AC45339C061B94588A763010A635F0F541792F4DDF7698E5E5E36AA8A6634A4073D9FBF9B1E961D961788F4D23CB398D5F299A8DA020BB8721FA770BEAFE2E86BDAB8BE2979908D156B0BCF050DCB5683C1E9D9E468AFDCA58CAFEE08595CE6AC7EF3B20DB75717A83A7BB04CC63425173FE20ADFBC08F57B5D879C165794CC85ADEA39BFA6651E8AC99B5F6AC1FAC54079E5895199D3974BBC52B45C6253F8C285DAB62E541941FFAA851A352F54DDDF2BEA290F146BEAD9DD846B77ACAE2D0F646FE1DAB7CC8B513516C1E2E2757E909F41D421E3E86838D47FE8CF69ACEA7D95024BC777CAA65B3BF67CAAE645D365D4E40FD7594CD67A6D4FB7F7FBBF07E554F236D678DB46268665E6ED9C2BA8F9553E6B0F97E339D33F208134EF1165A47459B7E7B511444A1C815A9E96CDDEDC3CADDEF2E76E03AA29EFAB56AD3CDDCC8CBD3ABA9851594623FD597124B38CE426AF6F3D7B783705A2A15BD5C92CBEE2ED2348B36446B83971590F3FFA060D1C80B959839AF25A720A32A134AD0F66F54EC9D38CCDBE6544E44744013AAF553F9FB98C145AA24A85E8F8E4B839AA6AC79C6E104CDA6EF1CCB5CAD75FC4ED110B1F815B56EB9C7C6272E87361E23AB6A8617B71D957FEDF1636BC2DBC45F2C7E5F80EFEDE9C66CE935B109879D94F1999558C3D17193031494EEFAA0996E191559DB1B4DC2A66216F18C286BC9DFB43CF72C678A8B6DABAE7EBF5E855587578FEFF5F8A4D543CA953B8FEA03C584AD3C4D8A9FE695A01B47DB554F040BF746A17EA86BC1FA9D9C5F90C8BA341DDF9B56A67DFB708B89CC68B76CE36C8A63CA37647C28F59FED59ABCCE7DD0F8C750AD89B3877ECA1CB1BA3BF06A3E6FBE3D0C07F93FB83D4C36BCC4A70F6A8F4DFDF838F6053C35AED73A2DAC598F4D1D90B4E89E39DBEA36BFFD074EB8E527F1F11F504B0304140000000800FB5A6743E07DACCD6F0100001404000012000000776F72642F666F6E745461626C652E786D6CBD924D4EC3301085AF62794F63A73F89AAA65529EA9205700137751A4BB11D79DC869E8D0547E20A0C71828A28A895101E45F2BC791E4F3EF9EDE575B678D615394807CA9A8CF201A3449ADC6E95D96574EF8B9B9412F0C26C45658DCCE851025DCC67CDB4B0C603C1D306A62EA3A5F7F5348A202FA51630B0B534582BACD3C263EA76912D0A95CB3B9BEFB5343E8A199B444E56C2E3CD50AA1A68D7ADB9A45B63DDB67636970038AAAE423F2D94A1FD74A4991AA171E627A525907BD99007AB4530D4C258901C3D07516594C5181336646336C22FC6DD8892E8C39997C281F4BD73B5EAF44268551D7BD9B59D43A5563E2FFBC24138253695EC6AA07658D9C38665144766C9324D68507846531656A7C438585869A70C3F95D693B77DDA94AFD79DC24F3D786914707CC3F278D41B5B9DA531C6E04881B304A9C49825E769B0F8CF689CFE57A0C17FA2C17EA3917E552EA5B1C4C1CEC388D92D4218B54F24C4754F031A0570258CFF7E1ADD06E6EF504B0304140000000800FB5A6743B472424BC3010000C503000010000000646F6350726F70732F6170702E786D6C9D53416EDB3010FC8AC07B4CDB288AC2A01414CE2187B6316035396FA9954D942209722DC4FD5A0F7D52BFD0A59428B2DB53759A1D0E47C35DF2F7CF5FEAF6B9B3458F3119EF4AB15A2C45814EFBC6B843294ED4DE7C104522700D58EFB014674CE2B652BBE8034632980A3670A91447A2B09132E923769016BCEC78A5F5B103E2321EA46F5BA3F1CEEB53878EE47AB97C2FF199D035D8DC84C9508C8E9B9EFED7B4F13AE74B8FF539B05FA56AEC8205C2EA4BDE69178D27252752D59EC0D6A6C36AC9F454A81D1C30552B2547A09E7C6C52D68C406D8F104113372F93B34A7D0CC11A0DC44DAD3E1B1D7DF22D150F43D622EF56722E519C7F8FFA140D9DB3D5BC549F8C1B538C805345384408C7976853A5F61A2C6EF9E4550B36A1926F84BA47C833DD81C9F97ADAF4A8C9C722991F3CD5B528BE41C2DCAF52F4100D3812A36C2C066C43A258D5862C7B4FF500E7B23936EF72C8115C0AE59481F165BAE10FE9A1E5B3D13FC2AEE661870CE22D5E21AFACAFCCB6BE0BE0CE59C6FDFC9EBE86DADFE55BF0D2B24B7236E22743C77D008D57C39EF16ACF2C363CBD690013A1EE396EB4D99DF7BA0336AF9ABF17F2F5791C9F64B55A2F96FC0DF7E595E3A94FAFA5FA03504B0304140000000800FB5A6743BD957539AF0F0000EC13000016000000776F72642F6D656469612F696D616765332E6A7065679D57075853D7DB3FF766913043C214E4129069800009108C8C00121199B21C1092001132C860891B6905DC8A0A2215298A5A0716445454D4226A7154C555051111B516ABB850F96EB08EF6FBDBAFDFFF973CF7F9DDF7BCEF39EF79C779CE1DB936D2034C83F3C46900F0784E400B7CC6C863400E520B32F30104B0E8BB1015F5609C33542A39DBD555AA74E10B65A92217814CE29AC797BB325CDC5CC104BF3C395F90295221A9A274B19443FBFDD0511A22167268F1CC70B770395794210E2D5088620AA6C60A0A32053E429ADF4464421E3B4F229788547C244F922555B2F338B4D1C9D928D7885D69C8A88A2A93430BD00C2009E1910857A610214C170FBAC08DC140582C17862793C5628C47DCDD18EEAE6EE89F456778B2992CB6BB17F22768E86A0A611A3B3A28E4CFB5D0370EEDCF4DE5E6E6BAE47AB8C814E9AE0C1F1F1FCD1CEEEE745483AECC97AAF87974A9D2E6E30C4122A5402196ABC43229A279E7A7CAD42A0E8DF6710B12F9A769FF63ACBE500C0FFF675589E493B652152D4AFB676D656CBE5CE41A2D52CAD40A810855B7D118CBD95C8588AF92296265B2AC8F518CCC90A964CA0C991CE1C6B0108778B15428CB553A8EEA8787B37952A58A2F158878411C1A2A71118B856C4F1637C483C50DF2F2F4F0643082DD0319C121413E41EE1E5C6E0897C9647EB40D9209D4129154F5D156F8D936F8ABB69A52F8602D52887344C210854C828C6E992DFEBA2F815FF7E583ADF0EBBE70BF6AEB8A3AE3FAB7447F14A1D5A3A19FCA167DF954F822295AED0AB4AC47EEEA24F0B8DCE4C8E88810DE9460004100E8844E114B65B0010012A94A113D291049484C42089D00064480070C00F802A53C3C262456D381BC602EA24495C05FF0E22ADA90287EA187462208F8FF812C902B5400409128F710A23B437911CAB37255728D7C10E5D4D44C0D87354D4F55A00EA2DC54C3D33FF0F1A33A1FB8BF860B2552F47080353ECB8512A1869F44F9B7396A11CA3153505E9C2316E5A2FC32CA6DB3D41231CA5F696C2522BE1200AC8E46AE12093250EE86721D456C3417E51300D0D249FF82A77EC155A23C9566535C993C5F214ECF50210E024704ED5C6F2454949B2552A9E8916852F80A217A5648E47C693E7A8A8DEE7914869AD8226890590C1F168BEEEEC2F81CA77F1EFC97D0E4F6037B16359A33C8F8DC67D97FD293D500E03D84C666E56759EA46000E2C01C0F4C66799ED7700E8A3796B3AFFC57E8C35F5F2C5492616095C3401FD84FF53E15FE08BF55C34D37D0A0F12244AE3ABB35488266E0259167AFA204AB4274408FDEF45FC5F1B7E812FFC188FF6B84821420F07240EAD32B1341D4DB754281E3D9AC5D2AF25F1BF34FB1B3ED4350A4AED7B404D760106E7A900F3DB3980A56803CC8CCDE808F4296F53887140D379F156FD1FEA7E14D0FF9E155EA17928C5E9A376DCE85844A056E47C18D3B425C00112D007546006C6021BE000E8C01D78015FE00F82C16410016241229805042003488002E48242B010148352B012AC0315A01AD4827AB01B348003E030380E7E0267C1057005DC04DDA00F3C0283E0051886208800E94214C80CB282C641CE903BE40D4D8482A12950349408A540E99014524385D062A8145A0D55405BA07A681F74103A0E754017A1EB500F3400FD01BD8131B00E4C852D613BD815F68603E03038169E09A7C3D970015C042F8737C035F02EB8093E0E9F85AFC0DDF02378080330DA18638C35868EF1C670311198244C1A4681998729C194636A30BB312D9876CC2F986ECC63CC6B2C1E4BC122583AD6171B8A9D861560B3B1F3B065D80AEC766C13F624F6176C0F7610FB1EA78BB3C039E3D8381E2E01978ECBC515E3CA7175B846DC29DC155C1FEE051E8F37C6DBE3BDF0A1F844FC6CFC1C7C19BE0ABF077F0C7F11DF8B1F22100866046702871041E013548462C246C22EC251C225421FE19596B6969596BB5688569296546B9156B9D60EAD56AD4B5AFD5AC34403E238229B18411412F3892B88B5C416E279621F71984426D99338A458D26CD242D206D26ED229D22DD2336D6D6D9AB68F7694B6587B81F606EDBDDAA7B57BB45FEB18EA38E9707566E8A87596EB6CD339A6735DE799AEAEAE9DAEBF6E92AE4A77B96EBDEE09DDDBBAAFF4287A2E7A3C3DA1DE7CBD4ABD26BD4B7A4FF489FAE3F403F467E917E897EBEFD73FAFFFD880686067C035E01BCC33A8343868D0653044A69019E408B2845C46DE41EE203F302418DA19061B0A0D8B0CB71A9E30ECA5602836142E4540594CA9A59CA2F451F1547B2A8F3A9B5A4AFD81DA491D343234F2348A33CA33AA343A62D46D8C31B633E6196719AF306E30BE6AFCC6C4D224C04464B2D464B7C9259397A6634CFD4D45A625A67B4CAF98BE3143CC82CD32CD56991D30FBD51C6BEE641E659E6BBED9FC94F9E331D431BE6304634AC6348CB961015B3859445BCCB1D86A71CE62C872ACE5244BB9E546CB13968FC71A8FF51F3B7BECDAB1AD6307AC285613ADC4566BAD8E5A3D448C9000240BD9809C4406AD2DAC43ADD5D65BAC3BAD8769F6B469B445B43DB45F6D4836DE3669366B6DDA6C066DAD6CC36D0B6D77DADE18471CE73D2E63DCFA71EDE35EDAD9DBC5DB7D6377C0EE81BDA93DCFBEC07EA7FD2D075D073F876C871A87CB8E78476FC74CC72AC70B4EB013D329C3A9D2E9BC33ECCC72163B57395F1C8F1BEF335E3ABE667C175D871E40CFA1EFA4F7B818BB4C7159E472C0E589ABAD6B92EB2AD776D7F76E4CB72CB75AB79B0C43C664C622460BE30F772777817BA5FB650F5D8F108FF91ECD1E4F3D9D3D459E9B3DAF3129CC70E637CC36E63B96174BC1DACD1AF0B2F54AF1DAE4D5E54DF58EF42EF33EED83F309F499EF73D8E7359BC556B11BD8BFFBD27D337D77F83E98603F4134A176422F87C6E173B670BA27221353267E37B1DBCFDA8FEF57E377D7DFC65FE85FE7DF1FE018303B6057C09340B740456063E04B2E9B3B977B2C08133429A824A833D830785A7045F0ED105A487AC8CE90C149CC4973261D0BC5858685AE0AEDE259F204BC7ADEE064AFC973279F0CD3098B09AB08BB3BC5698A624A4B381C3E397C4DF8ADA9E3A64AA71E880011BC883511BF46DA4766471E8AC24745465546DD8F66441746B7C75062926376C4BC880D8C5D117B739AC334F5B4B638FDB81971F5712FE383E257C77727B826CC4D389B689E284E6C4E2224C525D5250D4D0F9EBE6E7ADF0CE68CE2195767DACFCC9BD931CB7C56D6AC23C9FAC9FCE4FD29B894F8941D296FF911FC1AFE502A2F7553EAA0802B582F7824F417AE150E8838A2D5A2FE344EDAEAB407E99CF435E903197E19E5198FC55C7185F8E9ECD0D9D5B35F6646646ECB1CC98ACFDA23D192A4480E4A0DA599D293B2B1B23CD945B9B3BC58DE9DCDCE5E973DA80853D42921E54C65B38A8A5EA6CEA91DD44BD43D3913732A735EE5C6E5EECF23E749F3CEE53BE52FCDEF2F0829F87E0E768E604E5BA175E1C2C29EB90173B7CC83E6A5CE6B9B6F33BF687EDF82490BB62F242DCC5CF8F322B745AB173D5F1CBFB8A5C8B2684151EF92494B7616EB152B8ABBBEF1FDA6FA5BECB7E26F3B977A2CDDB8F47D89B0E44CA95B6979E9DB3241D999658C651B968D2C4F5BDEB982B562F34AFC4AE9CAABABFC566D5F4D5E5DB0BA774DF89AA6B5C8DA92B5CFD725AFEB28F72CAF5E4F5AAF5EDFBD61CA86E68DB61B576E7C5B915171A532B072CF268B4D4B37BDAC12565DDAECBF7977B5657569F59BEFC4DF5DDB32694B538D5D4DF956FCD69CADF76BE36ADBBFF7FEBEBECEBCAEB4EEDD36E9B6EEEDD1DB4FD67BD5D7EFB0D8B16227BC53BD7360D78C5D177E08FAA179377DF7963DC67B4AF782BDEABD0FF7A5ECBBDA10D6D0B6DF7BFFEE1FC7FDB8A991D258D20435E5370D1EC838D0DD9CD87CF1E0E4836D2DBE2D8D875C0E6D3B6C7DB8F288D19115ADA4D6A2D691A30547878EC98F3D3E9E7EBCB72DB9EDE6898413974F469DEC3C1576EAF44F213F9D680F683F7A9A73FA7007BBE3E019EF3307CEB2CE369D639E6BFC99F9736327ABB3E9BCD7F9E60B3E175A2E4EB8D87AC9EFD2F15F827EF9E932EFF2D92B53AF5CBC3AEDEAB5AE195DDDD784D71E5CCFBAFEF446CE8DE19B0B6EE16E95FC6AF06BF96D8BDB35771CEFECE966751FE909EA397737E6EECD5E41EFA37BCA7B6FFB8AEEEBDE2FEFB7EAAF7FE0FEE0F040C8C08587D31FF63D923F1A7E5CFC1BF9B74D4F1C9EFCF8BBFFEFE7061306FB9E2A9E8EFC51F6CCECD9B6E79ECFDB8622876EBF90BC187E59F2CAECD5F6D7DEAFDBDFC4BFE91FCE7D4B78BBE19DE3BB96F761EF6F8D484646469E00BDD10F38203CA6B93F8CDC0445E8E7C5BF06FC19984FF838363A0C7D39F22F31720C90B50002100C44063019C290A1919360117ABD81317FB9DCE049180211FD1A82FF2AC711612D3C816400201C401DC012FE94C310068BC303821699616D48A3B8538D8CED4C88648D1C83451FB80F6A103A1B196B88C35B332804AA162D8A9FED1EE8616B347203E860D0C5C81832F00337DE36273D3B3FD551F34B34A7DA2719065BDB83D8A0A2D0553C726468EBAA6911A2E4454A635B479B233B6D413361BB6DBC6878CFDB6D17A7BF8F50F70AAF8E006906C2FDF1FDFEC78EEEDB1B130F887E32A9DABD4C9CD2D1BE41879DECD4E8BD7542DB3BA7DE3B2B2A769E6C3D9D72F7D1F5D70FADAA310D8587536C8B23EB8E9738B6DC521FE65F393430F5CA91C2E181CE81AADE7B1377AC9C7FB66399F095D9AE0BF7FC13A55142F9B4051CD299A8354DF5BE5DA16EAD20EF6CC4656A023D39FABE94BEDE6F564555ECFD11B0E1F45107898F8C35B185F2DBF4B8A672D2EC9B51B65EA66382A84656F38A6F73BA1E5BEC8E623D6B7A924F16E4DF3C411A010B266D2F8AC1C065AA1B7D970F727847247E63DF5C7F983596BCA2DDAB6ABD61E1F9B26A1BEBF6613A6BA5307B7551E8E299CF7749F72D4C812C1FBEBEAE6CB83AFD5DE1B960DF8D6F4BBBAE12ABEAD42FEEF9C0DF6F5BB7216BCDCF856DB37F602D79EAC5E3CC2576855D69DB7BDB4FAF774543EAA94367EC6F9D74EE5B09637C4C0482E1EA0BFA17631383431487976D755EDD1FB1D73FBB29A4633E36A8D8D19415957DE7BBA3399C8243950797B0AEB7F44A58DFD60D94A694043AD4091739D4CEA639EC687DB87D69475ACA8E8417F01028E93C6EF266E9DEF711CFE29A07BD926F1A73AC2BCEE7BCB78FA9AADEB86B19BC55B0FDC285EBFBF7D7E32277E5B4FA1A8E80E492953B4F3575883C7BA7225BB08B5FAF6B7AA4BC7528D97CF1FA7BE9D98587C53DA757EE3FAE136DBD70041C6638F7B22BFA0791E5A7708DCCD3EF94B75F6F8EDFF394D5295BB8B174CD52526DBE0F858B9C295395D5143F7359DBB086D6503953983990E05AF53EABE74DFC1ECFE6F169252DC3CD8D6A65464A7CCC9434C5DDAD090DF2FE0CD99D122973269B9ACEE30733879F744432F51BEC666C7C7EA7DE6B5E62ABE0F7E9BB0CB9A7F6D5269A554D7E7DA62A8C9A72AD611FE674EF53EDECB941A4DE3BA97387DB9B4D4CCFF656EAC5264EE87537596268A7FF74DAF1F88AD4DA2275746154F5CFD56146174CAF241FE4AF8BFAF1EEDAD1DFBA91EBFF03504B0304140000000800FB5A6743E717FE47A61100009418000016000000776F72642F6D656469612F696D616765312E6A706567CD97793894FFBBC79F590C868C651A5B8CDD08912DCB142A846F69B14BF625FB924C76296BF6B267084591254214D9254BB28E7D5F47440C86F9A9DF39DFF3C7397F7CBFE73A5DE77B3F7F3CF7F5BA3FCFF5DCEFEB7E5FF7F53C9421CA24C0A8A5AEA90E804000003ABA00CA08F0008041A9605450188C0A464D0DA38133C2E1B4B4702403829E9115C9C6C68A44A138B804D11C9CFC2750281E311E7E2161919322EC687149718CA420460403A2A6A686D3C059E070160C078A03F3B783520F30D100C66043088809003381204C204A13E8D9518F54A05F01FC4780C090A35EA96968E1748C0018048180A1102A2A28F4A8E27F5403A04C54CCBCA755612CD72CA8F9DC9152C1F1D934FCE74B3F1EBFFE655D40DAD2E33E2D1CC5CAC6CE2128248C113929232B77465E41F1C24535758D4B9A5A3774F5F40D0C8D8CADAC6D6CEDEC6F3B78DEF1BAEB8DBBE713F2E061685878446442E2E32749C929A969CF7272F39EBFC82F7859F6A6BCE26D6555F5BB86C6A6E696D6B6F64FBD5FFBFA070687860953D333B373F30B8B4BCB1BDF37B77E6CEF9076F7400004F49FF1DFF41C090781A1500894FAA71E10D8FB6791094AC57B1AC6AC7A8DDAC29D854F2A9806793E3EBBF4232DBFF4F5F5E3961E5FE028019929C18D9F927E29FA6B82EEFFAF14FD29E84F3D1402400F011D0D0CC20428037B2F6400DB20FA1E501A20AD02B3073A01F62CC83F1B41FEC733DC6AD313318A7661581FBF3372C99E526A7C6A67096DB0867A6E16B42428CC7D3C78E8C7680FCF9F8F6123256214ADF1CC8153480ED3B658A26DD3ADDD90546FE7FD94FD9AAA2EFF0D95559F357BC6BFDED56F4610CF7F037765C643219C6F7D78C1CBAE4DB34B35396F2A41B9FC7DDFC1B11E01A95908511D824BDFD814595ABEA27B9CD5BC6E90D37DAF4789BB7312CF613F2EFDE54ADEB39BACD10C42112DC3BC4E76F47583D94495D853D507795825AEA60286F897DA1CA574C26D3561AF69A78387A20E39CC54284016770105F0D73894A18AD521B9CE3C556A7DC7B2EC636DC02876834D20D2ED8C2880B87A899F1EC3F4DDB5E52EDDC61B235B5E5DD3264D61E058844AB7CD063591F7D99083C7C86D63B5AE3C574EE77DCD948BC531CB929AE89B575167A9A78A9B8A585AB77B29802DBBF9872097B45E3FEA6B075331ECFEAD3391502352C806DDF975C1C71E2B376A6E6A21121D69C6C452D6602A90D1380CA4BD9578C6626192E4B0D2DE3DB17F7DAD75BFE697DC379308F4E22DC592047FE6A9DAC5A0CDA2E79C365167A5A4DB76D89134BB98B191213F5CC2DA77BD50028251A6AC6DE1815E4682CE24867DD527962891DB18B031B47826592E7288A5768553F2075DC85E1AB8D53797ACFFA5A28EF672917DA98EA700437CA72D4993CF76A926FFE3AABAEAD24030576648C131138D8A3AC65AF01DC45C9D1ED937F6C2E8552AF25F9CF06F45FC6D5910BE20664DB07A970FA613F03B3B4534D1DC606D1691C5738FCE5C4E303A6547CF1E32C9153FA52ABF9FC31D820CF5B18BF9204C126DE2C012606153B43FA416AFC362A8423E39B97D215255ED21B4CCCF6CBBACC7466D49A0CE1E1CA6BB5BE4743B7DE9DCC6C7552FA75CD8B4E6262D5E06BA75378AC3CE71E37CB5DE0FCA1E506B5DEA4BB2DAF61A13C54F5D1B8BA0001C50BFC6DC450333A4DF3D8173E85EE29D57423C651FB9F92A1E04B204DEAB4BDDE202E1AD70D28D8ADE0FFC949D73979856EFF326D9EEF72A2CAFFA26528081C523CB215A1CF53F90FA0B09EAE76B77DC2FE66E8B852B1741CEE556902C2649589977D2F649D72FCCADDE4EE35D38DD2A31EF3F39B69906F5AB5C5F33AA456FBC6DE962370CF792FB9AAB3CA1EEC9DE468E9D5269250D18913D375AA63F2DEBD39E7B2E33AB40010C98DBDFC4D6B3FC100A3A571408E0AE9523563DB30DE924246CA1A2B5EE3B9507C409A29D0CCDEE8DA9CEA6433869C2D10AFAF9FA59B5F2761DF25634BE6361CB351F49DF5DF765D08C34E89BDB36C75F3B30F770C80B6EABC03FC19E8578255BF7F090A50F2B712A9E21C2CA09FBACA41BC9A3CD39133AB113D17D3331738B66F7CDF6DDBEE7BD01A2042366BFF4FEA4DE3B27421B92D0A4EF91B0EEC2DA0BD3D1E84667FB77CD13C30F3B56A31F39E97AC08C8278D50DE94B2169503B33DCBE7A659F4BA1784E5F05E788FE30561F25D53D57791345578E6E09E2FF4AF63A3E117EE780CF1F615C9AB66B8DB3AE896064E2988B007580DAD8F90BC0418AE448400A03FCB210FD3F2505A5FE74364B9C1AE07766C61944A6DA1032F4E284D694C42C10199B25794F998BC7496FEDEA64C3E2BAEF7CFD20EB87FD22A3B277EAA97879B3F2CCA7F14E4BA5F77488E48DF6E728F9EED071D90DBB88737CB77069DFB797110C461A623B5F6056AEF4BC6C85163EA8A535B7CB801B1A19C0334E56D9807778101D731B469C8BF6C2FB324D83969BBCC80F019F6F8DDF229519B0882C8DE74BF1CB8F4665768E39642F04AC06B86486949017E9B81B27CD38EC0FE5BE0A7EE8997B9213A973F2E213394539D0AC3FE29A4A6CC2A1743BD68CFD47813384F3C1BB881CB89437F4BCE4B972FA1DD140024BC38BC6575E8AA6D1BA6A9B86851AA9A77B94C746320FDC472A4056CD87C796BC3F48916A9C0B34B9674389154AD5C0EED53531EA6912E0EF8F019D3BB954790825D5C93D8FE78AF59155D2C08AA5FACF79AE0E5353806215EE650A80C672C820DE16EE08ED74740F619336CFCCACADB58CDA7DB03F1E7B8CA4906786FB3A3DAB5818CF8264B5BB5193883EC9D5DCE7AF6D0E5AC26BC12C567C9EB6446FF8DEE8179BD9B42B1B10AAA68DAD65D6B744BC05755700BEDB95FBAA381B3F59B13D1DC26CFF6C7878D12DE60F26F11F6DF7ABA87B327A50D89A6DDBF51F5D451BB45EC9EA72690E4AB5B96E365E275FE16F699231307F9BFCE111077DE9E189F38984AE50054BF7275E6755B3341E4C2C12EA1ED61AA19EBC583E768CB8BC8B628D0A3C21656A92BF6B99CA7DA6A375475F05241A4C54E97A35A5D82E1CE57751562E61C5A054B73C69DE36491F48184169C06B3CA41FAA755367629896ABFFCE1EFD0D8869FEDF59F386C1C32DB3E38651977D7542BF0B0A643B855E582E94D498B48784BF21B14E3A3AC68B1ADEEC02B5C598248BEE99E39B7A9432431C736E27A569F6129DC4B3BF46B27596BB3F7C54C0C6133CC85F04967F4E3A3D29641958A26BBD009E3757DE52044F073624AFB31E001A8754972940730FABE2049C64A7B1FE3A73C0982AD268A426955D37496C0822502AB085044507AECF349E193F1DD5B423274628744970D9A451F76A1910ED890FDEBD0A3B57B3EE5B4F789D23486C0E1B9E8BAFBCF476072660354A5C54A6D7AD6E76F9A4A5637DE790EB6BADEC94B8F2AAB1949161FD1543FEFB5DFE8609DAE8220BAC2D371AD9BB9BA316F64AFCD9E1349A05012785D60FAF9412264C16784D694FFAEBC6A8EEC0AAF3D22767DE5F7428D19B18C62E924F916C85C6C5BF0D072EA4645EAE9BD91B60EE2EB9B66FB02BAD966C89D669FA2643D77D8CE4A98332E6C66F6B9B7B9661EA47FD358505F6330674C2D71D1D0F26D6B58C6C5CF9F115EB0B21591B770E054334DEF96BE97B3BB7DEB1C860EC7C23DFDEBE3DFFE2C0AAF12045935C3719B6ED7CA54CDDF45B031E663ACE994F962EE0F1AC6A4476C5E3B3D8EBB7D95EDDEB542FA6C74E741C9E781212D9ABC734BA18F90EF6384951382CA0D9917DD7EAD109EB1DAC35DED3B05BBE053366911788DF4596DC712DD2BD9DB75DB2825F25B5F89BA03F7F0D55964D7BD7589C5EB92C72B5A17817A72A5E44EC1F65F5E670491B74B36BCFBC48BF61F77AA04BDDFF65BFD3B436C9CD4C99245065C14AB07D9A3CD24801F2D22DF15BDD0AAF47D183BB75846AE739DF94FE6DD4C1D8B7F95B9D3E1AF27FD38EFFB7088CFD99B50E60355F2C5D2F97AEA99DB4EB88F6083C77BFEAB4D1137AD1B9F4C13225491247D5C3CEE32F3CD948CBC289CE6FF1D80C6F73868010D985DABD630446D7BDDDA751C2090619BD5C1480F6DCEB11BFF7708B9AB504B685ADECF9A16417638499D977C42A5971FC2559F973DCF22D4CF2817E32BDD541DD610D3CD0E4F38BF55EEDC1C313F8579356EFD58F969EAA1CAEECBA61A72BCC4DEE68C40ECEE93A26793B811FB9BE93966C0F290009128A9F36D8DFCBA5CBAC22EE8AA3F53A05DFEBD615ABFAFAC109E9871F2C9F42F4920F022AE5A4BF78078667CFE186C9B830120548F1F745A04313C8D4810D42C58775D7AAE32800FEFAED40F34D3FAFDCE68302D7505A446C03870F3E4D3B940204830E4EE63456B9568828D7901F6C1A8F318B6B4C15892BD6771F6E451F88E2660D31DF38C9C38FDE885D5821B144D3FDC12536431E213B9AADFAB192F7CD6277E37F1C6DEECFC6310D5DB22071E77AF4D667833FDC0CED45D5D3F6E3A38AF54DDA0F669FB125AFB5B2ED618AF4381CED8BAB62EE9DCB4D2DBA2BF06B503384BF3BC8DF83D87F02A4ACF2EA88A1F5873EABB7BA8FC09B9A6C02774FCCC83ECEBF9E519D879CC8CE75D48D9C7B94762B17A57818AF0871BCC7D07D9A978D537D7BFFA5FC890CBDFCFCD5700F574F13E505D9F3471F06BF4CC7A95D25C1BBE96CABF96CB4BD9A415D374E6352358BF6CD7FBD1A04DD89533D5B2ED586D24E16619E247F4696A213124957A1BB7FA7FBDF8E7EFDE942F01B2367C87C83BE80C43B31B01BEDA6C17B4023DF6C797B7B5BE8BB92F63DC1BA503EF139A9D0E8589E35B58F7B57617F78FBA4D9973926387823040C8A192B09F7F919AB4F13F95D345B2F64CAEFB4417EF81B2F737EAB181A75F06CE49A755318662660A40A5D32A686EE5F1A956EDBCA828D8D3B683F774C6BDDCE985C1A64A171EEBFAADE16B656D791181F9FB1D1A2301F24BBFAA1BFD0ECD615C2C3FE42B9CACBD0ABAA25601676892702B30B3129AD01F6909303FBD7FA18129D24662B4D52BF0DD845B0DC339C37CF62966400D85B8B03EC41EB6613DD3819E5CA3FF25C6D828BE58AD32C2F25A8285A797F03C662227D337A602FC2726BA28CFA575D5C6D6A4387CE5BDCD6BB906FDE745F977E6B8502AC64D11719AF5C9B987ED55621326D2ACC6C521F994E8B5AE245C4274113271EE19D0043ED9AA7428743660E091D54FD8F76A4B19F6A2C70E4BA54004217A62CD2ADFA2D8EFE55B4EFB55EDBD4D6EDC7A5ED36329FBC042354E3E9CDA51ECC192CCC322A59BD076EF6135FCB1439AA54A55F7B5707BE3D9693C2D34177FB8A8143744E7E2BFF8B69107770BE63B20187E988EA4BA9316747BCBC8AF803A7DCF9B38586E6C4169F98DA2DA06FC325CF9B6A2DDDC2EEF88A78CF93394709894E7830EEFA2C286146F83535771638D26C193DFCCA318050CE8AF0587189CF155AC6E9A976303DBE89FAE81413927DE456C5D0579EB92D36FEAACEC4D70BF7E788FD1C8B7CF8B8B86A6A74F31B81C99F8D269BA6754AA0448BDF1E3C238894B7B39DB2B66DC2C903E88C1C6EE5B9176F319097E5467C15A10A79D25AE12FBF33823CA8C5835BCFDA836FFE452BFE2E04FB09A87C572E49108DC797CE34343157F3CBF3C4685421815BC6AA383E56DA5AB5939F33E2C36EC4E1B26894CB9CEADE18D737057CEA65D772C196267EC62E28E3E683443AA69ECA998426A86564A7DCA4C1EE30896557BAD265807594237EC8803C2E0B7DD2A61B110A4FF76DB99BEA8D4EA91B5FC0062041C83DC1C24AE2DD045355FE927BFA7461D1363CE628F83D79C0AE0A381109248D6896BD4D189B0A0F0DCEC3754DDF4D35F1DC5910384482F0454B6E51B7AD2B4AAAAA334DF0E5A72BAB98B4624B4E6DD9F23020C15220E88FF9209686773793A4CCC6BA5954054778BFCF2677C651DDB4991016D33C4B0B98DFCD3A2672A746C1B22F07D76E7A91C973138DC7B0ACEFAA9CB80AF2CE1BBE25C59972319D3761147C055B3952C534A1A406DCA8581F7F4ADF6204DD91BAA7035B494D331B7B2B9562F5C89D7C600F72C62A59387B3B7BA6173CAFB09594D941083F863FE1F5F031A70012AA2A88D21EA7C2D7D74DEE56EB3E4A44C2BC255BAEA81252EB97718B4A964162CB3E5E1F9EC547909B12F8DF29587A24A8BBDFD70BE38FA159F4EDA152F03B6B2320F922F56E59D3EA8ADB29B3AE0E8073A1CB4D05C225CFB68801B136263199E4CA15793B44F06BEE471530963AB98C7A76B9296EAA1CA700651709E3695491234B7BE7252B2D5537D9F221F6D0B897A0AE55E6F333CFED21CFFEBA477E173AF617CEFCC31001C00059E020F8FFE30D4119FE17504B0304140000000800FB5A674370868C44A0000000E000000014000000776F72642F77656253657474696E67732E786D6C8DCECD0DC2300C05E05522DF690A0784AAFE5CD8800942EAB6911ABB8A5D0AB37160245620120CC0D17A4FDFF3FBF9AABB7B9CCD0D9304A606F6450906C9731F686C60D561770223EAA877331336F04081AEADB76AC3EB0555734F4C3648AAD4C0A4BA54D68A9F303A297841CAD9C0293ACD671A2D0F43F07866BF4624B587B23CDA84B3D3BC2F5358047EDAF68FB671EA97C41E45F22371FE7AD1050263DB0F504B0304140000000800FB5A674310F2D32E240100005D0600001C000000776F72642F5F72656C732F646F63756D656E742E786D6C2E72656C73BDD54F5283301406F0AB64DE5E02B4A5D569DA8D9B6EB51748E141A3E4CF24A9CAD95C7824AF6046474B47C9B86058E6317CF9F12533BCBFBEADB72FB2254F689DD08A4196A4405095BA12AA6170F2F5D50A88F35C55BCD50A1974E860BB59DF61CB7D78C51D8571246428C7E0E8BDB9A1D4954794DC25DAA00A4F6A6D25F761691B6A78F9C81BA4799A16D4F633E03293EC2A0676576540F69DC1FF64EBBA1625DEEAF22451F93FB6A00EBD0FDFE54226B70D7A06DF93246401A10386744C8490A180B3406225F8D770993C186C8619E376E1BB16FB4D7CAEA33DE463EEFF8C87FB5FC7D11B4625B33125B5567ECF0F6DEF507E4651C57C4CC5F0B5C8E2D762318D224F8C1A4614D32066F12A96D328E671C56A1AC522AEB89E46519C15F4E29FB0F900504B0304140000000800FB5A6743E3E63BC2AB0B00000A69000011000000776F72642F646F63756D656E742E786D6CED5DE972DB38127E158CB6F6CF6C241E3AAD1D6BECDC9978626F8E72EDD656A540121231A6080E009A562A0FB37FF719F6C756CD0BED2B6C03047524A24C39B22D394AAA481168341ADD5FA31B2024FFEF3FFFFDE9E7AB71842E091794C58735A761D710897D16D07874584BE5B0DEAB2121711CE088C5E4B03621A2F6F3E0A7AC1F303F1D93582260108BFE25D48552267DCB127E48C65834584262A81C323EC6121EF9C81A637E9126759F8D132CA947232A27966BDB9D9A61C3A0531EF70D8BFA98FA9C093694AA499F0D87D427E656B4E055FACD9B3C3522EB1E2D4E229081C522A48928B88D6FCA0D2AC382C9E5AA415C8EA3822E4BAAF416709C8135C651DE51C6789070E61321A0F4695E39E5E8D81514A8584C5B541161B1CF429231A6F1944DFCB5FDA77D37A06FA334CD6A3610D0C5E0A71FEA75F482C484634902E44DD0B14898208D73201008E4408D37CFDEA35E03D0D9B051BDAEE0E7B160A2EE89BA707D39D3370F59EAE6B3081A66FD4B1C1DD69E3FB7E15F2DAF119F8A62B7A58BAC695B39780615A986053A8DA349033DE1448B9551192E0806552C99703A0A2582F134EB70393014E84C4ED0890C1A8AB7CC7BD0572DADF4227333BD7AD139880496686B29A16492803A135F1A917FF30B917D001CE1A6181A3E2151F42BCED9B024E732C722B8C286362243B9A2DA6352B2F10A827CA065F5D6A230D6E2E85E701AA88F23B83F6151CEC5E938C624D602912C5A9DF167570F5B3D667C067C4B4761CDEAA59F5FF327110653F28860AE3AD5983FACE15432F538A451543C1956FED42625083C5850F16C7CEBA9D8695FA3E4128279359790CC297A29C50D907860AF0BC487AAA52D046499CFEF8C4E5721AFDD6E175169A1BCE9B4D744E4CEABC90C687390522385AC36780B33B27DDCED3E6E3EABE972C33BC13EE432408487806D336DEBCFC7C0CD54EBE22F5C5B29A9E8CC30CB953B781FE2F8024D58AA93164E4654003BDD8BCA1E9EFDE3F1E9E3251981351BF7FCE8A56F6CEBF6966B7A47D4B330482B47716EF58DCB51ADCBDB8E7ED7B8D6B58E759D5B55772A23E4A6F2AF87A69E2D0C770F4DC5AB10785F79D7F668E76E0108D4C7111DC525BD540B0CC93B398948C1E2D5786458AF37515789A7C328781262B586379FDE6B957A1056E3827E464D6321F97B72A5F7A3B4342A0FE244107E496A03F4F2EF67CFDE9EBC7AF31A15DB14599635C8278F796A7BA28614BB29932F989788224882D5A6C5D7D2188B2D6C42A82D88E7CFCDF0D3A2546DA844330E46C9D407112EC1C638210A49880687B58F578AC547D96DD790CFD41E04FD0452B84EC7B61FE96B0D3118BA3CAC2912D687E10F09CFF7B9D4FE1CA839C1323CAC8D8F5A47ED082E8E7374905FDA572084020C819E866ACF8FB38BFCB396443FA2DF985211000078D0023C977DB557944658E8CF88FC1E1FD6E8104534266A7F2A4609BD22D1093C9ED3001221BB6896938A748C8E6CE42C2BB7E1FF91B3589E7016A023171AB8CB2A9A48EB22EF5477782DD54BA2BC77A95420D7B2E69DB2DEBBD7F4AED9F60C917D2D8752C91C7B8187B56804656600002099A76A67975D68A38E380E2878BDC6952A930A263E8B63E2CB7CC6E2C4AC1D583F62FE05BA544C0E6B24A0508C4502D55CED8FE9B679C753944E213B0F57EAD82EA031E7FEA7790C1B20857A88FD4EB7D14EE45F33A5B27EAB6D2752038F8EF188045862C4FB8A2B7F15B494D0924AD57841867C0AD1BE53C57D491CCC79EEFDE68C4A1FAB82D2F2FAF9A8B49C622E2C2D23B841D4760FDC9E615EA9BC69B7BA4BCB1DB5EBFF7579CB3E70E764BBB3AC60BB2CF02D99C174E1E8946C0ADC53EA70F31461B1BB7B4B12AC9065752AEB19E317C2DAAE94410E5EB20C5189CE9570DA2AD5245A310BEE817557C0CAC2493D15A4AE4BB60E59E7E164C9E6D51E565B0F2BECB15402B0B60E51C74A30F4613F4FED18A0BC888DB60E4C8F41A83D90B61A4802908493640E49C7BECFD2585A276C741A6F1BA240281A6F0652EA9697C93B7EE191DDFE9AE97BDD499DF3D4DBDE18D81E151B21F75BF9BB02C035CF0434971B64A6CD1282797D9690CC697429C59D206EE7D571D74724165FF5BC24581D0A76CC18BE8A536EAF2452152DAE7FE3639C63EE8066B7209B86E61B9EB1B876B0EBC5DD2AA39183976041F668411A73F17831AE554525C9D5398920832248B27CB8280B09276848631CEB315081C6382088603169A07382305487907F4D541B1AAB37055A8532C4527F5004CBE45C3C537B5DDA34A6712AC9D2455D599E88333C41430E0E3322522AE1718CD87048782E9C0F8F2189122DE588B34C7DE0C84B4104224463595FCB3439F8E35F95494F639FE408A331152108021A4C9288FAF9C16055958153210FCABD481B4210D03048374EFD100DD338D058225AFE7C38D0AA818E81CDC49808D6147F86A143431C07532B4053208E19621E2C13F20E81BFC4178A6EA2798BC61AA35983F41746633D5A19B25480540264477E2A6042235C20ACCE730B4DF1E135A08E01A82E41AC88C3DC30C9754402ED985ACC399704891510D5EB2839839DD622683940CAA801C111E8468022C1B6F21150E31820C321843ED24A4A0519A6D16D187D393CCF604A15447D3D820E27B9D8D779C9BC3F4CD26822FE623B47D365CF52D70031010E1AD7648C69A48EC02B199497696583562E006C11CBFA15C75326D1F2B598CFE221E563CB39C01EEEB5DCFAB0D70CEA2D4248BD77E07875EC7686B6ED7981EBB62A0AB08EEE07F92C46736445799C416A3588633937AD259C5D52E51D06A58041053531C6CA17CD8C4044E14E2A3A44543BE81C0FCC654C785508552B92837714E60CE86EF22DB33DC4366266F4F7042F62C55AB9A6FC4E734057E71CD657E59DAEBBCF0DEF3237BCCD63400B6761BE3C58D0A970B0A03B3B56D06B959F2A68AF7BAAA02CA529F7DBFB339049DE5BB798BCCBC10B22F5D4CC540EB47DD9F853A6738E3C65519914218187FD0B0431E4F714120EF55D40A45F999859182A5418590C3028629095FCFC2DF3BC492B7CC8282FBE08F02AD783702CB12F97F55D6496EA1C0E856C4948A1F3309539EA6F69AA4A4FA7F92A6BFEE1E652DEEA76F0769FBE938327C602DFF6D2F3C6F38495ADB767BCE8EBEF55CAA355FBB702D5379A9C0B07FA26D136D765B62A7EEF7713B3CD4495F24C72332ADEBE2F1E196137B4C9F850B5B485805C3367DF3E9DAE42DEFE9B815F426F0152E1AF848FA6110F62AA8475BDE9667DB8659B78277F772BA2B2CD9DE72C521964BA7403763BB23E09CB0218B8DE75229F3EE687DDFE2951EDA317E1F862BBF2C0952BCF6E8595A7DB9AAE3CDD152BCFCEBA2BCFB5B2CF928DB8DBB6B4CAEF8740E6317661CCAD927DBD78DA5193F73667F2EE4335B95AC91258859A7DE571022B6DCBEDB43A6EABBDA3663FD89CD97B0FCEECEA654212A5A231620CF4AACD9E5AB6E5D84DDB75DA1D9828BBBD6ECFB13B9D66DB4A9890CAFB23545B4DB0833869DA9BC3C9C183C389991E262C95A9676002A496DE61797172FAF8F86447CDEE6CCEEC8E7D9B76BFE70CF0C7CFCF4FDF9E1FBF7DFAF9C7EDB2AE1C0C19CF300FD4AE2946434E417D6B38C88636FD4A96387A00FB25CD327BDC33A01FDA92A6E96E6E26DB2F6976C3E4CDCD997CBFA4D91DB3B73667F6FD92E6012F69AAFC6E43459CEC9734BB63F62AA76A2A9A7DBFA4B90FEB6EC99246DDF2A6F9F5BB793D35FDDE68DBDDE99F6E5C4423FD1264B31FC0FEE3DFC8B59D667E60E6113A8E22A42B04323809AA9EB82D5B099EA61CA9C3D16A643808D4F1684445D5F3D0E68CF11A67929B07F59683DE302E43F496E1608DA6272C0E58FC08E577F4A68B0E9E9EADD1FE434CD52F8EBF86A1066CC531E052D43DC01F0CCDBEBB39640B5F716FD0C653B0DF71E2F6E3E70F6FDE7D78BC7D013B8D45EA099F538FE4DF87922115489DF05C433F6BFCB4C1166C2FEEE1B4BB7032D3723E816DDCD68559F75DAEDFE5B71CE5CF2501A6C497C68547EFD4B771553C73DD96E61CAAD8D66B155E998C7ED5C881F00A15AD9C46C74178ECE581398F9CB36AD5F1AC362438509277ED9E9E6A1893738FA35416AAB48C4F0894156EA9897479C07C751A4E71A73139A3D207419B9DA9AA8B31594A1CF5B763ACD9DF301AFC1F504B0304140000000800FB5A67439CB70DCC390E00008312000016000000776F72642F6D656469612F696D616765342E6A7065679D57075853D9B6DEE7A411082524749043E82D104A80005202810808D2652C84244084149350ED0EE200164445298AA2A3628151876217D4B180BD60BFA0328A150BF631EF0447E5CEBBCE9B77FF8F8FEFDF6BAFB5CFDAABE51CD555553F300E2F146502C0E53A010DF00DAAC7801C96C7CF290210C0A26B012AEAC738672B9532969B9B4441E709A419423A5F2A762BE4C9DC1874773710105428E3F173844A244398259204D29EED3B44434482405A8A778C7B8C8C2DCC164516CB8509C5E313F9C5397C3F012D682C1250C82A14CBC442250F2914E74A14ACC240DAC8E12C94ABC56E346444459913480B516F20A93171085B2A1722DE744F57BE3B838130997486973793C970413CDC191E6EEEE81FD395E1C5F266B23C7C903F41439F261764B2E2C3387F3E0B5D05D2FEBC54414101BDC0932E9567B931FCFCFCD4677878B8A21AAE8A22899257E82A51587F39214CA8E0CB4532A5482A41D46B5E86344F1948A37DB98258F6F5D8FF18AB518A31317FAF2A167FD55628E385997FAFAD482C9209DDE2850A699E9C2F44D5ADD5C632165B2EE429A5F244A934F74B14E3B2A54AA9225B2A43D8094CC421452411480B148E23FA31312CAE44A1E449F8426E58200D95D0452201CBDB83E1CDE0B887FB78797A3118E11E21616C1F0E23D4831DE2EEE3C9F10EFF621B26E5E7898512E5175BC1375BCE776DD5A5F0D95A2817E50B051CB9548C8C5C9925FABE2FECEFFBF2D956F07D5FC2BE6BEB863AE3F697447F11A1D5A3A65FCB165D7C2D7CA104AD76395AD6AA3BA4542E9B3D352E3E96C38D0E0710040029325A2491C27A0088254A797C4428923A310D219C013020023C6000C0E32B6431099C44750772C3D988025502FF86D757D0864471D135320E41C0FF0F64BE4CAE04008A43B9A700BD19CA4B509E5BA094A9E54328A766E4A839AC6E7AAA1C7510E5C66A9EF599BB8CE87CE6C16A2E104BD0E100AB7D9609C402353F8AF29FF2F38428C744A3BC345F242C40F92594DBE4E68945287FABB6150B790A00B024B55C29E467A3DC1DE52479623C1BE501006890B246F18C515C292C54AA2FC596CA8AE4A2AC6C25E2C07744D0CEF545228505B942A5D2350E4D0A4F2E40678558C69314A1536CE4CE23D057C7164183CC64F83199AE1E74C6B738FDFDE63F843AB79FD9CB092339830C7BBEC9FE939EB41100DF61343695DF64193500B42F00C0F8FA3799CD3A0074D1BCB59D1D751F4375BD8C9A6422219FAE0EE857FC9F0AFF00A39E47571FF7353C489830939797AB44D471E34B73D1E98328D09E1022AE7F2DE2FFDA701446F9E182F6B8502E448703928C5699489285A65B22108D8C6691E47B49FC2FCDFE82CF758D82B2F113A04EA503BDB3548079D203B0142D8099B416DD81BEE62D9A980CD49D976279FF73DD8F00FADFA7C24BD5FF14A2AC113B767C22C2CF93E77FDE53B725C0014DA00BA8C0048C01D6C001B8020FE003FC41300807E3402C480413C114C007D9400CE4A000CC04F34029A800956025A8050D60236802CD60176807FB4127F80D748373E032B801FAC000780486C06BF001822002A40D512013C812B2859C210FC8171A0B8543D1503C34114A87B220099407CD847E842AA02AA8165A0F35413BA13D502774123A0F5D83FAA141E805F41EC6C024980A5BC076B01BEC0B87C05170223C19CE82A7C3C57009BC045E0D37C2DBE136B813EE862FC37DF023781803305A18438C15C615E38B6163623169984C8C1C331B538EA9C634629A317B31C73117317D98C79877583C968245B0AE587F6C243609CBC74EC7CEC62EC2D662B760DBB047B117B1FDD821EC279C36CE1CE78C63E1B8B8545C16AE00578AABC66DC2B5E28EE12EE30670AFF178BC21DE1EEF838FC44FC44FC3CFC02FC2AFC1B7E00FE3CFE3EFE28709048209C19910488825F0084A4229A186B09D70887081304078ABA1A561A9E1A1C1D148D39068CCD7A8D6D8AA7150E382C67D8D0F443DA22D91458C250A8845C4A5C48DC4BDC4B3C401E2074DB2A6BD66A066A2E634CD799AAB359B358F69DED47CA9A5A545D3F2D39AA025D29AABB55AEB17AD135AFD5AEF48FA2427129B348994475A42DA4C3A4CBA467AA9ADAD6DA71DAC9DA6ADD45EA2DDA47D44FBB6F65B1D8A0E5D87AB23D099A353A7D3A67341E7A92E51D7563744778A6EB16EB5EE6EDDB3BA8FF5887A767A6C3D9EDE6CBD3ABD3D7ABD7AC3640A99418E258BC98BC85BC927C90FF409FA76FAE1FA02FD12FD0DFA47F4EF5230146B0A9BC2A7FC48D948394619A0E2A9F6542E751AB582BA837A863A64A06FE065906C5068506770C0A0CF10636867C835CC355C6AB8CBF08AE17B230BA31023A1519951B3D105A337C666C6C1C642E372E316E3CBC6EF4D109370931C936526ED26B74CB1A64EA6134C0B4CD79A1E337D6C4635F337E39B959BED32BB6E0E9B3B99C79BCF30DF60DE633E6C31C622C24266516371C4E2F118C331C163A68D5931E6E098414B8AE5584B91E50ACB43960F11032404C94556234791212B73AB48AB3CABF55667AC3ED0EC6949B4F9B416DA2D6B4D6B5FEB4CEB15D65DD6433696363136336DB6D95CB725DAFADA66DBAEB23D6EFBC6CEDE2EC56EA15DBBDD037B637BAE7DB1FD36FB9B0EDA0E410ED31D1A1D2E39E21D7D1D731CD7389E73829DBC9DB29DEA9CCE3AC3CE4C6791F31AE7F32E38173F17894BA34BAF2BC935C435DF759B6B3FDD901E4D9F4F6FA73F75B3714B735BE676DCED93BBB77BAEFB46F71B0C7DC638C67CC65EC60B0F270FBE479DC7254F6D4F8EE71CCF0ECFE75ECE5E42AFB55E57BD29DE31DE0BBDBBBCFF60FA30E5CC66E6A08F8D4FBA4FBD4FAF2FD537CE7791EF093F9C5FA8DF1CBFFD7EEF584C9692B58BF5CCDFD53FC77FABFF8300FB0061C0C680BB81B4405EE0FAC0BEB1C8D8F4B1EBC6F6055905F1821A83EE045B070B823705DF0F710C9916B23DE469A87BA83CB435F40D9BC59EC53E1C86098B082B0F3B13AE1F9E145E1B7E9B43E36471B6718622BC2366441C8EC44546452E8BECE55A70F9DC26EED0389F71B3C61D8D22452544D546DD89768A9647EF8D8163C6C52C8FB939DE76BC647C7B2C88E5C62E8FBD15671F373D6EDF04FC84B80975137E8F67C4CF8C3F9E4049989AB035E1756268E2D2C41B490E4979495DC9BAC993929B92DFA484A554A5F4A5BAA5CE4AED9E683A5134B1238D90969CB6296DF887F01F56FE3030C97B52E9A42B93ED27174E3E39C5744AEE94035375A7F2A6EE4EC7A5A7A46F4DFFC88BE535F28633B819F519437C367F15FF912058B04230280C145609EF67066656653EC80ACC5A9E35981D945D9DFD58C416D58A9E4F8B9CD630ED4D4E6CCEE61C556E4A6E8B58439C2EDE23D197E4488E4AC7480BA5E765CEB25259DF74D6F495D387E451F24D0A483159D1A1A4A22F533D790E790BF2FAF3C7E6D7E5BF2D482ED85D482E9414F61439159515DD2FE614FF3C033B833FA36BA6D5CC7933FB6785CC5A3F1B9A9D31BB6B8EF59C9239037323E66E99A7392F67DEE9F9EEF3ABE6BFFA31E5C7BD251625734BEE2E8858B0AD54A7545EDABBD07F61C34FD89F443F9D29F32CAB29FB542E283F55E15E515DF171117FD1A9C58CC5AB17AB96642E39B394B9746D25BE5252796559D0B22D55E4AAE2AABBCB6396B7AD405694AF78B572EACA93D55ED50DAB3457E5ADEA5B1DBDBAA3C6A6A6B2E6636D76EDE5BAD0BA967AF3FAB2FA376B046B2EAC0D5EDBDC60D150D1F07E9D68DDD5F511EBDB1AED1AAB37E037E46FF87D63F2C6E33FFBFEDCB4C97453C5A63F364B36F76D89DF72B4C9A7A969ABF9D6A5DBE06D79DB06B74FDA7E6E47D88E8E66D7E6F52D862D15BF805FF27E79B8337DE7955D51BBBA76FBEE6EFED5F6D7FA564A6B791BD456D436D49EDDDED731B1E3FC9E717BBAF6FAEF6DDD47DFB779BFD5FEBA030607961ED43C58725075A8F8D0F061D9E1C79D599D77BBA676DD38927AE4D2D10947CF1C8B3A76E237CE6F478E871C3F7422F0C4FE93AC937B4EF99E6AEF6676B7F578F7B49EF63EDD7A8679A6EDACCFD98E737EE7F69E0F387FF042D085CE8B61177FBBC4BDD47D79FCE5F35792AE5CED9DD4DB775570F5C1B5DC6BCFAFE75FFF7063EE4DDCCDF25B7AB7AA6F9BDF6EFC97E3BF5AFA987D07FAC3FA7BEE24DCB971977FF7D13DC5BD8F0325BF6BFF5E7DDFF27ED3038F07FB073983E71EFEF070E091ECD187C7A54FC84FEA9F3A3CFDF559F0B39EA1D4A181E7F2E7AA178B5E9ABCDCFCCAEB55D770DCF0EDD7E2D71FDE94BF3579BBE59DEFBBE3EF53DEDFFF50F091F071F51F8E7FECFD14F5E9A64AAC52A99E029D910F382038AC7E7F50DD0025E8E7C53F06FC0D98AFF8B237B20D8DDEF987501D06640D8000040391014C863064487514148E38F66F2F37440D022A417FE2470B7178025683887E18E1B06AB7BE88211883C501329EA041D4A7D00C19542332AA82C16061CCD74321328CB1D2C73270F8101E8DE24198EE4935505D072454032663C82008ACF21E441E0EC5C173A9609DE6E0455DEAF27B2DF4759BE53313CC7AF2926B10E13E7D336BF9CD27BDC91CFAAC367DDC7C79559320E6884BFABC7A1FDFE259E1E438D2D4B25DCFDAF6309E7F72793B7DA0CAB5DBADCBDE7D71B6D82FA0E24EBEC9FA877BB5A4DAD70F5DB855FB2EB5F7CE917B2FC6DECBBFD8B24CBF7270CD83DDD79E072C3C92FE7207AF29AFD9DAC8A8D67EF3A11A979B028F358AD70D97CB1A4E5F6B0BD9356177C5B265B9F3D0891A1E4A8DC6BD3056814D0DED513BA398869C644EC21ACB4337B79CCC4D617A7A27F726ED20C98B3A05BB657CFB72873BB482E6D393BAB748E80B63B0413B4AE243E73AA900F7C2C68AC72951163D35D6D6D58BAE85CFCCD22D3F5675CCAEB59B74CB8793945899DC7D35C1F1D9CBD6F9AFB79F7DF2E040BDAE7BED8DB5E3CBF456A8408E016BB08537AD5FDA5ED9B56971C48EB8833A26BB7712C38545C74F5DE670B9CCB2676BEAEBE58DAFCED691575DBFB7A8CCC4F14912ADF5B9D90A8AA5D66CA7CE4E6B878EFCABC9D79FDD59905DDA6678400568FE951DB70E0C9836CDB8BD326D5231292BB993846602071C30AA6BFF03504B0304140000000800FB5A6743CCC5D2B40A010000C401000011000000646F6350726F70732F636F72652E786D6C6D91CF4AC43010875F25E6DECE5641A4B45D58F0A628E841BC8564B60DDBFC21996EB7CFE6C147F2154C8BD615F638FCBE7C4C7EF3F5F1596D4FA667470C513B5BF322DF7086563AA56D5BF381F6D91D67918455A277166B3E61E4DBA692BE942EE073701E03698C2C796C2CA5AF7947E44B80283B3422E689B029DCBB6004A531B4E0853C8816E17AB3B9058324942001B330F3AB91FF28955C957E08FD225012B0478396221479017F2C6130F1E2832539238DA6C9E345F4375CE953D42B388E633EDE2C68DABF80B7C78797E5AB99B67353127953295992A61E9BD74ED8039BDCC052012C60AB635A24B5CB464D1DBB7FDF3DEDAE2A58F9B9D980473DDFA3292A381F97E97FEDCD37504B0304140000000800FB5A6743103E70D1E8000000520200000B0000005F72656C732F2E72656C73AD92CB4A43410C407F65C8BE37B71544A4D36EBAE94EC41F0833B90FBCF36026D5F6DB5CF849FE8211A458A9A50B97799D1C423EDEDE97EB7D98CC0B973AA66861DEB46038BAE4C7D85BD84937BB035385A2A72945B670E00AEBD5F29127121DA9C398AB5146AC1606917C8F58DDC0816A933247AD74A904120D4B8F99DC33F58C8BB6BDC5F29301A74CB3F516CAD6CFC13C1D325FC34E5D373ADE24B70B1CE5CC8A5F1D4AA6D2B358784DC5A3FF4E378A0583E77516FFA9C37BE1E8D9CF72D1F922A39EF6E8A43A0F9AAE48395F54BAB95EE9EFEB6360214F42E852E1CB425F1D47233C7984D527504B01022D00140000000800FB5A6743982A25FA4C020000EE040000110000000000000000000000000000000000776F72642F73657474696E67732E786D6C504B01022D00140000000800FB5A67431F7AB12F5D0100001F05000013000000000000000000000000007B0200005B436F6E74656E745F54797065735D2E786D6C504B01022D00140000000000FB5A6743EF09D5CFC60B0000C60B0000150000000000000000000000000009040000776F72642F6D656469612F696D616765322E706E67504B01022D00140000000800FB5A674317E459D6E60E000032130000160000000000000000000000000002100000776F72642F6D656469612F696D616765362E6A706567504B01022D00140000000800FB5A674300EC3F86551200008516000016000000000000000000000000001C1F0000776F72642F6D656469612F696D616765372E6A706567504B01022D00140000000800FB5A67437F6BD934D10E00001F1300001600000000000000000000000000A5310000776F72642F6D656469612F696D616765352E6A706567504B01022D00140000000800FB5A67434ED38CD74C0700006C3900000F00000000000000000000000000AA400000776F72642F7374796C65732E786D6C504B01022D00140000000800FB5A6743E07DACCD6F01000014040000120000000000000000000000000023480000776F72642F666F6E745461626C652E786D6C504B01022D00140000000800FB5A6743B472424BC3010000C50300001000000000000000000000000000C2490000646F6350726F70732F6170702E786D6C504B01022D00140000000800FB5A6743BD957539AF0F0000EC1300001600000000000000000000000000B34B0000776F72642F6D656469612F696D616765332E6A706567504B01022D00140000000800FB5A6743E717FE47A6110000941800001600000000000000000000000000965B0000776F72642F6D656469612F696D616765312E6A706567504B01022D00140000000800FB5A674370868C44A0000000E00000001400000000000000000000000000706D0000776F72642F77656253657474696E67732E786D6C504B01022D00140000000800FB5A674310F2D32E240100005D0600001C00000000000000000000000000426E0000776F72642F5F72656C732F646F63756D656E742E786D6C2E72656C73504B01022D00140000000800FB5A6743E3E63BC2AB0B00000A6900001100000000000000000000000000A06F0000776F72642F646F63756D656E742E786D6C504B01022D00140000000800FB5A67439CB70DCC390E00008312000016000000000000000000000000007A7B0000776F72642F6D656469612F696D616765342E6A706567504B01022D00140000000800FB5A6743CCC5D2B40A010000C40100001100000000000000000000000000E7890000646F6350726F70732F636F72652E786D6C504B01022D00140000000800FB5A6743103E70D1E8000000520200000B00000000000000000000000000208B00005F72656C732F2E72656C73504B0506000000001100110059040000318C00000000DF, 0, '2013-11-07 11:23:54', 0, 8, NULL, NULL, 'MailNode22613382', 0)";

			// TODO: execute sp
		}

		private void SendToCustomerAndEzbob(Dictionary<string, string> variables, string toAddress, string templateName, string subject)
		{
			SendMailViaMandrill(variables, toAddress, string.Empty, templateName, subject);
			SendToEzbob(variables, templateName, subject);
		}

		private void SendToEzbob(Dictionary<string, string> variables, string templateName, string subject)
		{
			string ezbobCopyTo = null, ezbobCopyCc = null;
			// TODO: add addresses to ConfigurationVariables
			// TODO: load addresses from ConfigurationVariables
			SendMailViaMandrill(variables, ezbobCopyTo, ezbobCopyCc, templateName, subject);
		}
	}
}
