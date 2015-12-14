﻿namespace EzBobTest {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using AutomationCalculator.AutoDecision.AutoApproval;
	using AutomationCalculator.Turnover;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Backend.CalculateLoan.LoanCalculator;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Models.ExternalAPI;
	using Ezbob.Backend.Models.NewLoan;
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Backend.Strategies.Alibaba;
	using Ezbob.Backend.Strategies.AutomationVerification;
	using Ezbob.Backend.Strategies.Broker;
	using Ezbob.Backend.Strategies.CreditSafe;
	using Ezbob.Backend.Strategies.Experian;
	using Ezbob.Backend.Strategies.ExternalAPI;
	using Ezbob.Backend.Strategies.ExternalAPI.Alibaba;
	using Ezbob.Backend.Strategies.Lottery;
	using Ezbob.Backend.Strategies.MailStrategies;
	using Ezbob.Backend.Strategies.MainStrategy;
	using Ezbob.Backend.Strategies.MedalCalculations;
	using Ezbob.Backend.Strategies.Misc;
	using Ezbob.Backend.Strategies.NewLoan;
	using Ezbob.Backend.Strategies.OfferCalculation;
	using Ezbob.Backend.Strategies.Postcode;
	using Ezbob.Backend.Strategies.Reports;
	using Ezbob.Backend.Strategies.SalesForce;
	using Ezbob.Backend.Strategies.UserManagement;
	using Ezbob.Database;
	using Ezbob.Utils.Extensions;
	using Ezbob.Utils.Security;
	using Ezbob.Utils.Serialization;
	using EzBob.Models;
	using EzServiceAccessor;
	using EzServiceShortcut;
	using EZBob.DatabaseLib.Model.Alibaba;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Fraud;
	using EZBob.DatabaseLib.Model.Loans;
	using FraudChecker;
	using NHibernate.Util;
	using NUnit.Framework;
	using PaymentServices.Calculators;
	using SalesForceLib;
	using SalesForceLib.Models;
	using StructureMap;
	using Twilio;

	[TestFixture]
	public class TestStrategies : BaseTestFixtue {
		[Test]
		public void ApprovedUser() {
			var s = new ApprovedUser(182, 1000, 24, true);
			s.Execute();
		}

		[Test]
		public void CashTransferred() {
			var s = new CashTransferred(21340, 2500, "01971847001", true);
			s.Execute();
		}

		[Test]
		public void EmailRolloverAdded() {
			var s = new EmailRolloverAdded(3060, 2500);
			s.Execute();
		}

		[Test]
		public void EmailUnderReview() {
			var s = new EmailUnderReview(3060);
			s.Execute();
		}

		[Test]
		public void Escalated() {
			var s = new Escalated(3060);
			s.Execute();
		}

		[Test]
		public void GetCashFailed() {
			var s = new GetCashFailed(3060);
			s.Execute();
		}

		[Test]
		public void Greeting() {
			var s = new Greeting(21401, "torke");
			s.Execute();
		}


		[SetUp]
		public new void Init() {
			base.Init();

			ObjectFactory.Configure(x => {
				x.For<IEzServiceAccessor>()
					.Use<EzServiceAccessorShort>();
				x.For<ILoanRepository>()
					.Use<LoanRepository>();
				x.For<ILoanScheduleRepository>()
					.Use<LoanScheduleRepository>();
				x.For<ILoanHistoryRepository>()
					.Use<LoanHistoryRepository>();
				x.For<ISalesForceAppClient>()
					.Use<FakeApiClient>();
				x.For<ILoanTransactionMethodRepository>()
					.Use<LoanTransactionMethodRepository>();

			});

			Ezbob.Backend.Strategies.Library.Initialize(this.m_oEnv, this.m_oDB, this.m_oLog);
		} // Init

		[Test]
		public void LoanFullyPaid() {
			var s = new LoanFullyPaid(3060, "fdsfdf");
			s.Execute();
		}

		[Test]
		public void MoreAmlAndBwaInformation() {
			var s = new MoreAmlAndBwaInformation(3060);
			s.Execute();
		}

		[Test]
		public void MoreAmlInformation() {
			var s = new MoreAmlInformation(3060);
			s.Execute();
		}

		[Test]
		public void MoreBwaInformation() {
			var s = new MoreBwaInformation(3060);
			s.Execute();
		}

		[Test]
		public void PasswordChanged() {
			var s = new PasswordChanged(3060, new Password("dfsgfsdg"));
			s.Execute();
		}

		[Test]
		public void PasswordRestored() {
			var s = new PasswordRestored(3060);
			s.Execute();
		}

		[Test]
		public void PayEarly() {
			var s = new PayEarly(3060, 2500, "dfsgfsdg");
			s.Execute();
		}

		[Test]
		public void PayPointAddedByUnderwriter() {
			var s = new PayPointAddedByUnderwriter(3060, "dfgsdf", "dfsgfsdg", 5);
			s.Execute();
		}

		[Test]
		public void PayPointNameValidationFailed() {
			var s = new PayPointNameValidationFailed(3060, "dfgsdf");
			s.Execute();
		}

		[Test]
		public void RejectUser() {
			var s = new RejectUser(21370, true);
			s.Execute();
		}

		[Test]
		public void RenewEbayToken() {
			var s = new RenewEbayToken(3060, "sdfgfgg", "dsfg");
			s.Execute();
		}

		[Test]
		public void RequestCashWithoutTakenLoan() {
			var s = new RequestCashWithoutTakenLoan(3060);
			s.Execute();
		}

		[Test]
		public void SendEmailVerification() {
			var s = new SendEmailVerification(3060, "dfg");
			s.Execute();
		}

		[Test]
		public void test_mainstrat() {
			var ms = new MainStrategy(
				1,
				14036,
				NewCreditLineOption.UpdateEverythingAndApplyAutoRules,
				0,
				null,
				null,
				CashRequestOriginator.Other
			);
			ms.Execute();
		}

		[Test]
		public void TestBackfillExperianConsumer() {
			var s = new BackfillExperianConsumer();
			s.Execute();
		}

		[Test]
		public void TestBrokerInstantOffer() {
			var s = new BrokerInstantOffer(new BrokerInstantOfferRequest() {
				BrokerId = 21348,
				AnnualProfit = 5000,
				AnnualTurnover = 60000,
				CompanyNameNumber = "hren",
				NumOfEmployees = 4,
				IsHomeOwner = true,
				MainApplicantCreditScore = "ok",
				ExperianRefNum = "01234567",
				ExperianCompanyLegalStatus = "L",
				ExperianCompanyName = "Hren Limited",
				ExperianCompanyPostcode = "AB10 1BA",
			});
			s.Execute();
		}

		[Test]
		public void TestCalculateModelsAndAffordability() {
			var stra = new CalculateModelsAndAffordability(14166, null);
			stra.Execute();
			Assert.IsNotNull(stra.MpModel.Affordability);
			Assert.IsNotNull(stra.MpModel.MarketPlaces);
		}


		[Test]
		public void TestUpdateCurrencyRate() {
			var ucr = new UpdateCurrencyRates();
			ucr.Execute();
		}

		[Test]
		public void TestDecrypt() {
			string sIn = @"<?xml version=""1.0""?>
<AccountModel xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <id>2022</id>  
  <name>xero1</name> 
  <apikey>DFECQTB5KOK7IHIGGW5S2KXBNLLAV8</apikey>  
  <secret>MWOWLLN0LOGVUPF9BLBUGBM0EOCLWR</sercet> 
  <limitDays>0</limitDays>  
  <accountTypeName>XERO</accountTypeName> 
  <displayName>stasd+rt9@ezbob.com</displayName>  
  <publicCert>-----BEGIN CERTIFICATE-----MIICaDCCAdGgAwIBAgIJAI88EON8XhSeMA0GCSqGSIb3DQEBBQUAME0xCzAJBgNVBAYTAlVLMQ4wDAYDVQQKDAVlemJvYjEOMAwGA1UEAwwFZXpib2IxHjAcBgkqhkiG9w0BCQEWD2V6Ym9iQGV6Ym9iLmNvbTAeFw0xNDA3MjExMTQ2NDdaFw0xOTA3MjAxMTQ2NDdaME0xCzAJBgNVBAYTAlVLMQ4wDAYDVQQKDAVlemJvYjEOMAwGA1UEAwwFZXpib2IxHjAcBgkqhkiG9w0BCQEWD2V6Ym9iQGV6Ym9iLmNvbTCBnzANBgkqhkiG9w0BAQEFAAOBjQAwgYkCgYEAx85lHKJsulizOsWLLVW/NdF5fh4WJwXwwbmXRweTlvITHkfoJn7Kh6WeFj23QMspLZuN88L55dJdM7PUFgMS8PRO8ul2MegJ3oTOceIke0Ks49RH2kaeXvHt7xY+nAvgKZuqVlz9sjijkcQLUJ/AuNWsMvjkAooxns4kDB4gS48CAwEAAaNQME4wHQYDVR0OBBYEFDAtGGKmWyhFxidHpPO0VzWtRi2uMB8GA1UdIwQYMBaAFDAtGGKmWyhFxidHpPO0VzWtRi2uMAwGA1UdEwQFMAMBAf8wDQYJKoZIhvcNAQEFBQADgYEARHwqcwcDhiRNrVPWJZmlhMnOmftcfNcP3ak0lyBIe8SrSxLvHmWRonZa84G1/S/u5aaa/Kh2LX3LN1ypj1DEQJfsi8lwj7Lk9NiJQLi66azs73Z6gdVNAJ0KapVt6S81yV9Hf1xs5qRZU/+Ex/OZ+12QMGpe4X7bF8tOXarBROQ=-----END CERTIFICATE-----</publicCert>  <privateKey>-----BEGIN RSA PRIVATE KEY-----MIICXwIBAAKBgQDHzmUcomy6WLM6xYstVb810Xl+HhYnBfDBuZdHB5OW8hMeR+gmfsqHpZ4WPbdAyyktm43zwvnl0l0zs9QWAxLw9E7y6XYx6AnehM5x4iR7Qqzj1EfaRp5e8e3vFj6cC+Apm6pWXP2yOKORxAtQn8C41awy+OQCijGeziQMHiBLjwIDAQABAoGBAJiVhaHYaC/mjPjU4vQ8B0mSLrWhREmIv9MxZ9VWc99R/kehoifDq+brE07o0okonMm3gTAmNbDMdWCGc/BbaJo/8amH3pgbCtEfLUKN7bokiREUQG8rSOxXkjUXAesrLUI+5ZVKQzNPKB20xAEF9vmj/+Ew4q/aaOKSKiw8WHgBAkEA79Jah/WpFWkau8gSGLsoSu7NHbNPNMxy/zR4Sa8uCfX4jdht3hgVpIeDRfqr3gCyVdmC3c1MvT8mVLDinAxOGQJBANVI/JZ+aB5l8L82UwtK1nmwuEgrHS2vwTlxYPtraIn5WHB7DZIWEv21XJXKT+sKAxZxwtf1eo/uOvR5Y+jYy+cCQQC8H2VWu5TsL6uB3Bqd/cpIpXSPNMYQI8SdfrpRCrLxq1pTYpAkpP3tN0P5k/5XwnIiN+KZPN9SoIPf8XWBdYGZAkEArDvbVcBQXjPd/NtmpljR58Uwm37NekZSmAuZ0I5FBh5oL7T/GXhf76dUU9XjQZ99LNPDD1g5mB8+VovshW0FAwJBAKEbR4WNRWgoc0qV2rJ8gUpCXns9yjVEPTkxyDUGIMn/qTHAicTdl0G4NVJ6H9AYrIvFa4ICwOninM+6k/AiRg4=-----END RSA PRIVATE KEY-----</privateKey>
</AccountModel>";
			var sOutEn = new Encrypted(new Serialized(sIn));
			string sOutDec = Encrypted.Decrypt(sOutEn);

			//string sOutput = null;
			////xero
			//string sInput  = "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";
			////not xero
			//string sInput2 = "C0/PLr+re1WYbSklttMpN43371tjwPz5G/zRdZgtika0HJyXaOvvimQX/Y+8jYj5iAWuST9WIh6vRp6ZDC6UVL2ahn27SVIM94HkYcTf6bcAMlwGlXVgp7gAzSM0EY2kUv/JJ3loK5KEZnMGdOCkn62c7Gj1Uj829SqUstX1aBgwxlpnYzPKKRFQ1SCXXvb40MKvVAe68p69Uq33wQJYXECV3DjpYD7U60WkLeHKClz8ZrwS7YqpcQLcfgnTgmLyPeGxClc0nu8aHB7SwvGWEXU0kbS8Vo0qBBeOCVusFH9v5ns8l5XqFd38fUZzxIkxhVDS4wNOFBpnwO1eNl6vhHxkFQQFSXXlw2HMi3P86LXTDrLh95ozcdt6taSP4BxQALRWUQD8T9UOXiOyK3i3G6S3+ZYvtPiiG25jWvtjMO4CullL5WrIwxX/vJJXxfpoq2u5H8CtmfDpWCoGBL/wlrlrpfbpId8FkMtzEzElQ1wu00DAEfYRUsJACuWMcFsR9+cimVXJCo+4ZXqXoKsxa28FiVXZbkpi7tLoEV4FmNOZPzSHA+3PhuabCJh176xasjkWmw9c+0jowArfPBiSYY84HykegweBVbUyUeyKIQ5gE0XXyyTROdzqtORPdbkzPcVNpqmvcn7Nz0E5dgThx/04zRC6K2IFjOAsY23WQDRV3mjlgCKnY43sYMh5/TsU3PzvPD/WsGs7GjpFmxeml7+j1hB/o6MYCznKWjirQjIiUuYXE8XpaE/nvIxQLuEA/YAPq/kUI+PuXyhOR9WKE/XoQ2WYv0KOJ3Ct4eZy+43LwmbVHBGk8//Pp0kZwmbOpWT6CqSd32zTIDxhZL6yEbkjt9wWP5h0yZ4aze64JC8t7f4xjLimn90ocYp/lvgtN9JvOTfik5fIazhOCPeWAh/Zm+cFNiPH++3NlmNcwB1qzmhvKJ+RzodAL16D7tOQF49YyVt8VuJG+rlXRlx12AL80OkH5Q1KXjP2AGbRSkc7QLCYzxd0NVX2GGPN30OmZ2nQGy1ze689h86LsZDjpUCXR01anflqBOYblRenIGqIwEXJXtbqVujriANlZhhSK6SAByjK7WfdTkgbGM2hFl9pjZPPcopjdPCgJdLJ6C7Oo3gyjkH3vL8qe4idcmi0";
			//try
			//{
			//	sOutput = Encrypted.Decrypt(sInput2);
			//	sOutput = Encrypted.Decrypt(sInput);
			//}
			//catch (Exception e)
			//{
			//	m_oLog.Warn(e, "Failed to decrypt.");
			//	sOutput = string.Empty;
			//} // try
			Console.WriteLine(sOutDec);
			Assert.IsNotNullOrEmpty(sOutDec);
		}

		[Test]
		public void TestExperianConsumerCheck() {
			var s = new ExperianConsumerCheck(85, null, false);
			s.Execute();

			s = new ExperianConsumerCheck(85, 6, false);
			s.Execute();
		}

		[Test]
		public void testFraud() {
			// var s = new FraudChecker(21394, FraudMode.FullCheck);
			// s.Execute();

			var chk = new InternalChecker(126, FraudMode.FullCheck);
			List<FraudDetection> lst = chk.Decide();

			m_oLog.Debug("{0} collisions found.", lst.Count);

			foreach (FraudDetection fd in lst) {
				m_oLog.Debug(
					"\n\nCollision:" +
					"\n\tCurrent customer: {0}" +
					"\n\tOther customer: {1}" +
					"\n\tExternal user: {2}" +
					"\n\tCurrent field: {3}" +
					"\n\tCompare field: {4}" +
					"\n\tValue: {5}" +
					"\n\tConcurrence: {6}" +
					"\n",
					fd.CurrentCustomer.Id,
					fd.InternalCustomer == null ? "null" : fd.InternalCustomer.Id.ToString(),
					fd.ExternalUser == null ? "null" : fd.ExternalUser.FirstName,
					fd.CurrentField,
					fd.CompareField,
					fd.Value,
					fd.Concurrence
				);
			} // for each
		}

		[Test]
		public void TestGenerateCode() {
			//http://freesmsreceive.com/+441300452045.php
			var s = new GenerateMobileCode("01300452045");
			s.Execute();
		}

		[Test]
		public void TestGetBankModel() {
			new GetBankModel(234).Execute();
		}

		[Test]
		public void TestLoadExperianConsumer() {
			var s = new LoadExperianConsumerData(20323, null, 110285);
			s.Execute();
			Console.WriteLine(s.Result.ToString());
			Assert.IsNotNull(s.Result);

			s = new LoadExperianConsumerData(17254, null, null);
			s.Execute();
			Console.WriteLine(s.Result.ToString());
			Assert.IsNotNull(s.Result);

			s = new LoadExperianConsumerData(110285, 1014, null);
			s.Execute();
			Console.WriteLine(s.Result.ToString());
			Assert.IsNotNull(s.Result);
		}

		[Test]
		public void TestLoadExperianConsumerMortagageData() {
			var s = new LoadExperianConsumerMortgageData(20323);
			s.Execute();
			Assert.AreEqual(200089, s.Result.MortgageBalance);
			Assert.AreEqual(1, s.Result.NumMortgages);
			Console.WriteLine("{0} {1}", s.Result.NumMortgages, s.Result.MortgageBalance);
		}

		[Test]
		public void TestLoadExperianLtd() {
			var s = new LoadExperianLtd("06357516", 0);
			s.Execute();
			Console.WriteLine(s.Result.ToString());
			Assert.IsNotNull(s.Result);
			Assert.IsNotNull(s.History);
		}

		[Test]
		public void TestLoadExperianNonLtd() {
			var s = new GetCompanyDataForCompanyScore("10732957");
			s.Execute();
			Assert.IsNotNull(s.Data);
		}

		[Test]
		public void TestLREnquiry() {
			var s = new LandRegistryEnquiry(21340, null, "test", null, null, "E12 6AY");
			s.Execute();
			Assert.IsNotNullOrEmpty(s.Result);
		}

		// TestCalculateModelsAndAffordability
		[Test]
		public void TestLRRes() {
			var s = new LandRegistryRes(21378, "SK310937");
			s.Execute();
			Assert.IsNotNullOrEmpty(s.Result);
		}

		[Test]
		public void TestAutoRejectTurnover() {
			var turnover = new AutoRejectTurnover();
			turnover.Init();

			this.m_oDB.ForEachResult<TurnoverDbRow>(
				row => turnover.Add(row),
				"GetCustomerTurnoverForAutoDecision",
				new QueryParameter("@IsForApprove", false),
				new QueryParameter("@CustomerID", 211),
				new QueryParameter("@Now", DateTime.UtcNow)
				);

			this.m_oLog.Info("Turnover for year is {0}, for quarter is {1}.", turnover[12], turnover[3]);
		} // TestAutoRejectTurnover

		[Test]
		public void TestAutoApprovalTurnover() {
			var turnover = new AutoApprovalTurnover();

			this.m_oDB.ForEachResult<TurnoverDbRow>(
				row => turnover.Add(row),
				"GetCustomerTurnoverForAutoDecision",
				new QueryParameter("@IsForApprove", true),
				new QueryParameter("@CustomerID", 211),
				new QueryParameter("@Now", DateTime.UtcNow)
				);

			turnover.TurnoverType = AutomationCalculator.Common.TurnoverType.HMRC;
			turnover.Init();
			this.m_oLog.Info("HMRC turnover for year is {0}.", turnover[12]);

			turnover.TurnoverType = AutomationCalculator.Common.TurnoverType.Bank;
			turnover.Init();
			this.m_oLog.Info("Bank turnover for year is {0}.", turnover[12]);

			turnover.TurnoverType = AutomationCalculator.Common.TurnoverType.Online;
			turnover.Init();
			this.m_oLog.Info("Online turnover for year is {0}.", turnover[12]);
		} // TestAutoApprovalTurnover

		[Test]
		public void TestMedalCalculation() {
			DateTime calculationTime = DateTime.UtcNow;

			var customers = new[] {
				26338
			};

			//foreach (var customerID in customers)
			//	new CalculateMedal(customerID, calculationTime, false, true).Execute();

			this.m_oDB.ForEachRowSafe((sr) => {
				int customerId = sr["Id"];
				new CalculateMedal(customerId, null, DateTime.UtcNow, false, true).Execute();
			}, "select Id from dbo.Customer where IsTest = 0 and WizardStep=4 order by Id desc", CommandSpecies.Text);
		}

		[Test]
		public void TestOfferCalculation() {
			/*
			var calc = new OfferDualCalculator(18040, DateTime.UtcNow, 20000, false, EZBob.DatabaseLib.NLModel.Database.Medal.Gold);
			var offer1 = calc.CalculateOffer();
			Assert.AreEqual(5.5M, offer1.SetupFee);
			Assert.AreEqual(4.5M, offer1.InterestRate);
			//todo uncomment to run once 
			//return;
			*/

			const string query = "SELECT " +
				"CustomerId, Medal, OfferedLoanAmount, NumOfLoans, ZooplaValue " +
				"FROM " +
				"MedalCalculations " +
				"WHERE IsActive = 1 " +
				"AND Medal <> 'NoMedal' " +
				"AND OfferedLoanAmount > 0 " +
				"ORDER BY CustomerId DESC";

			int calculatedOffers = 0;
			int failedVerificationOffers = 0;

			this.m_oDB.ForEachRowSafe(
				sr => {
					int customerId = sr["CustomerId"];

					Medal medal = (Medal)Enum.Parse(
						typeof(Medal),
						sr["Medal"]
						);

					int offeredLoanAmount = sr["OfferedLoanAmount"];
					int numOfLoans = sr["NumOfLoans"];
					int zooplaValue = sr["ZooplaValue"];

					int roundedAmount = (int)Math.Truncate(
						(decimal)offeredLoanAmount / CurrentValues.Instance.GetCashSliderStep
						) * CurrentValues.Instance.GetCashSliderStep;

					int cappedAmount = Math.Min(
						roundedAmount,
						zooplaValue > 0 ? CurrentValues.Instance.MaxCapHomeOwner : CurrentValues.Instance.MaxCapNotHomeOwner
						);

					var calc = new OfferDualCalculator(
						customerId,
						DateTime.UtcNow,
						cappedAmount,
						numOfLoans > 0,
						medal
						);

					OfferResult offer = calc.CalculateOffer();

					calculatedOffers++;

					string offerMsg;

					if (offer == null) {
						failedVerificationOffers++;
						offerMsg = "mismatch - offer is null";
					} else if (offer.IsError) {
						failedVerificationOffers++;
						offerMsg = "mismatch - error encountered, offer is: '" + offer + "'";
					} else if (offer.IsMismatch) {
						failedVerificationOffers++;
						offerMsg = "mismatch - offer is: '" + offer + "'";
					} else
						offerMsg = "match - offer is: '" + offer + "'";

					this.m_oLog.Info(
						"\n\nCustomer #{0} id {1}: {2}.\n\nResult summary:\n{3}\n\n",
						calculatedOffers,
						customerId,
						offerMsg,
						calc.ResultSummary
						);
				},
				query,
				CommandSpecies.Text
				);

			this.m_oLog.Debug("Calculated offer for {0} customers failed {1}", calculatedOffers, failedVerificationOffers);

			Assert.AreEqual(0, failedVerificationOffers);
		} // TestOfferCalculation

		// TestCalculateModelsAndAffordability
		[Test]
		public void TestParseExperianConsumer() {
			var s = new ParseExperianConsumerData(110285);
			s.Execute();
			Console.WriteLine(s.Result.ToString());
			Assert.IsNotNull(s.Result);
		}

		[Test]
		public void TestPayPointCharger() {
			var s = new PayPointCharger();
			s.Execute();
		}

		[Test]
		public void TestSetLateLoanStatus() {
			var stra = new SetLateLoanStatus();
			stra.Execute();
		}

		[Test]
		public void ThreeInvalidAttempts() {
			var s = new ThreeInvalidAttempts(3060);
			s.Execute();
		}

		[Test]
		public void TransferCashFailed() {
			var s = new TransferCashFailed(3060);
			s.Execute();
		}

		[Test]
		public void UpdateCustomerMarketplace() {
			var s = new UpdateMarketplace(21400, 18364, false);
			s.Execute();
		}


		[Test]
		public void Test_AutoReject() {
			int nCustomerCount = 1000;
			int nLastCheckedCustomerID = -1;
			var test = new VerifyReject(nCustomerCount, nLastCheckedCustomerID);
			test.Execute();
		}

		[Test]
		public void Test_RejectTurnover() {
			var turnover = new AutoRejectTurnover();
			turnover.Init();

			this.m_oDB.ForEachResult<TurnoverDbRow>(
				row => turnover.Add(row),
				"GetCustomerTurnoverForAutoDecision",
				new QueryParameter("@IsForApprove", false),
				new QueryParameter("@CustomerID", 18416),
				new QueryParameter("@Now", new DateTime(2015, 2, 2))
				);
		}

		[Test]
		public void TestLotteryEnlistingType() {
			var let0 = new LotteryDataForEnlisting();
			let0.LotteryEnlistingTypeStr = "MinCount";
			let0.LotteryEnlistingTypeStr = "MaxCount";
			let0.LotteryEnlistingTypeStr = "MaxAmount";
			let0.LotteryEnlistingTypeStr = "MinAmount";

			let0.LotteryEnlistingTypeStr = "MinCountOrMinAmount";
			let0.LotteryEnlistingTypeStr = "MinCountAndMinAmount";

			let0.LotteryEnlistingTypeStr = "MaxCountOrMinAmount";
			let0.LotteryEnlistingTypeStr = "MaxCountAndMinAmount";

			let0.LotteryEnlistingTypeStr = "MaxCountOrMaxAmount";
			let0.LotteryEnlistingTypeStr = "MaxCountAndMaxAmount";

			let0.LotteryEnlistingTypeStr = "MinCountOrMaxAmount";
			let0.LotteryEnlistingTypeStr = "MinCountAndMaxAmount";
		} // TestLotteryEnlistingType


		[Test]
		public void RequalifyCustomer() {
			int customerID = 18234; //217; // 18234;
			long aliMemberID = 12345000; //00;
			var s = new RequalifyCustomer(customerID, aliMemberID); //"caroles@ezbob.com.test.test"
			s.Execute();
		}


		[Test]
		public void AvailableCredit() {
			int customerID = 18234; //217; // 18234;
			decimal aliMemberID = 12345000; //00;
			/*var s = new CustomerAvaliableCredit(customerID, aliMemberID); // "caroles@ezbob.com.test.test");
			s.Execute();
			Console.WriteLine(s.Result.ToString());*/
		}

		[Test]
		public void TestGetSmsDetails() {
			//In case we need to retrieve the status of sms need to invoke this method and update sms message table
			string m_sAccountSid = "ACcc682df6341371ee27ada6858025490b";//CurrentValues.Instance.TwilioAccountSid;
			string m_sAuthToken = "fab0b8bd342443ff44497273b4ba2aa1";//CurrentValues.Instance.TwilioAuthToken;

			var twilio = new TwilioRestClient(m_sAccountSid, m_sAuthToken);
			var smsDetails = twilio.GetSmsMessage("SM6b5974acc0054605ab0443c9f38d2349");
			Assert.IsNotNull(smsDetails);
			Assert.IsNotNull(smsDetails.Status);
		}

		[Test]
		public void TestBrokerTransferCommission() {
			var stra = new BrokerTransferCommission();
			stra.Execute();
		}

		[Test]
		public void TestUpdateTransactionStatus() {
			var stra = new UpdateTransactionStatus();
			stra.Execute();
		}

		[Test]
		public void TestAlibabaDataSharing_01() {
			// run "requalify before all
            int customerID = 16134; //  23504; // 24319;  //24321 ; 
			// ad cashe request before
			AlibabaBuyerRepository aliMemberRep = ObjectFactory.GetInstance<AlibabaBuyerRepository>();
			var v = aliMemberRep.ByCustomer(customerID);
			//	Console.WriteLine(v.AliId);
			//new RequalifyCustomer(customerID, v.AliId).Execute(); // only for CashRequest creation!!!
			//new MainStrategy(v.Customer.Id, NewCreditLineOption.SkipEverythingAndApplyAutoRules, 0, null).Execute();
			/*new DataSharing(customerID, AlibabaBusinessType.APPLICATION).Execute();*/
			/* many customers
			 * var aliCustomers = aliMemberRep.ByCustomer(customerID);
			foreach (var v in aliCustomers) {
				Console.WriteLine("AliId: {0}, customerID: {1}, email: {2}", v.AliId, v.Customer.Id, v.Customer.Name);
				//new RequalifyCustomer(v.Customer.Name).Execute();
				//new MainStrategy(v.Customer.Id, NewCreditLineOption.SkipEverythingAndApplyAutoRules, 0, null).Execute();
				new DataSharing(v.Customer.Id, 0).Execute();
			}*/
			//var s = new MainStrategy(customerID, NewCreditLineOption.UpdateEverythingAndApplyAutoRules, 0, null).Execute();
			new DataSharing(customerID, AlibabaBusinessType.APPLICATION).Execute();
			//new DataSharing(customerID, AlibabaBusinessType.APPLICATION_REVIEW).Execute();
		}


		[Test]
		public void TestSF() {
			var sf = new AddUpdateLeadAccount("a@b.c", 1, false, false);
			sf.Execute();
		}

		[Test]
		public void TestSaveApiCall() {
			var sf = new SaveApiCall(new ApiCallData() {
				Request = "requestonbj",
				RequestId = "11111",
				Response = "responseobj",
				Comments = "test strategy",
				StatusCode = "5500",
				ErrorCode = "0",
				ErrorMessage = "no errors",
				Url = "stam"
			});
			sf.Execute();
		}

		[Test]
		public void TestCaisGenerate() {
			CaisGenerate cg = new CaisGenerate(1);
			cg.Execute();
		}

		[Test]
		[Ignore]
		public void TestCreditSfaeServiceLogWriter() {
			ServiceLogCreditSafeLtd saveTest = new ServiceLogCreditSafeLtd("X999999", 1);
			saveTest.Execute();
		}
		[Test]
		public void TestPPNoLoan() {
			PayPointAddedWithoutOpenLoan p = new PayPointAddedWithoutOpenLoan(6548, 5, "safdhdf533f");
			p.Execute();
		}

		[Test]
		[Ignore]
		public void TestBackFillExperianNonLtdScoreText() {
			BackFillExperianNonLtdScoreText test = new BackFillExperianNonLtdScoreText();
			test.Execute();
		}


		[Test]
		public void TestAddDecision() {
			AddDecision addDecision = new AddDecision(new NL_Decisions {
				UserID = 347,
				DecisionTime = DateTime.UtcNow,
				Notes = "Reject",
				DecisionNameID = 2
			}, 22785, new List<NL_DecisionRejectReasons> {
				new NL_DecisionRejectReasons {
					RejectReasonID = 1
				},
				new NL_DecisionRejectReasons {
					RejectReasonID = 3
				}
			});
			addDecision.Execute();
		}



		[Test]
		public void TestUserDisable() {
			UserDisable ud = new UserDisable(1, "a@b.com", true);
			ud.Execute();
		}

		[Test]
		public void TestAlibabaReports() {
			new Alibaba(null, false).Execute();
		}

		[Test]
		public void TestLoanStatusAfterPayment() {
			new LoanStatusAfterPayment(54, "", 27, 1000, 500, false, false).Execute();
		}

		[Test]
		public void TestBrokerLeadSendInvitation() {
			new BrokerLeadSendInvitation(1040, "stasd+evlbrk5@ezbob.com").Execute();
		}



		[Test]
		public void TestNL_AddLoan() {
			int customerID = 369; // 366;
			int oldLoanID = 1049; //1042;

			LoanRepository loanRep = ObjectFactory.GetInstance<LoanRepository>();
			Loan oldLoan = loanRep.Get(oldLoanID);

			NL_Model nlModel = new NL_Model(customerID);
			nlModel.Loan = new NL_Loans();
			nlModel.Loan.Refnum = oldLoan.RefNumber;
			nlModel.Loan.OldLoanID = oldLoanID;
			nlModel.Loan.InitialLoanAmount = oldLoan.LoanAmount;

			nlModel.FundTransfer = new NL_FundTransfers();
			nlModel.FundTransfer.Amount = nlModel.Loan.InitialLoanAmount; // logic transaction - full amount
			nlModel.FundTransfer.TransferTime = DateTime.UtcNow;
			nlModel.FundTransfer.IsActive = true;
			nlModel.FundTransfer.LoanTransactionMethodID = 1; // 'Pacnet'

			nlModel.LoanHistory = new NL_LoanHistory();
			nlModel.LoanHistory.AgreementModel = oldLoan.AgreementModel;

			nlModel.LoanAgreements = new List<NL_LoanAgreements>();
			foreach (var aggr in oldLoan.Agreements) {
				//Console.WriteLine(aggr);
				NL_LoanAgreements agreement = new NL_LoanAgreements();
				agreement.FilePath = aggr.FilePath;
				agreement.LoanAgreementTemplateID = aggr.TemplateRef.Id;
				nlModel.LoanAgreements.Add(agreement);
			}

			PacnetTransaction oldPacnetTransaction = EnumerableExtensions.First(oldLoan.PacnetTransactions) as PacnetTransaction;

			if (oldPacnetTransaction != null) {
				nlModel.PacnetTransaction = new NL_PacnetTransactions();
				nlModel.PacnetTransaction.TransactionTime = oldPacnetTransaction.PostDate; //DateTime.UtcNow;
				nlModel.PacnetTransaction.StatusUpdatedTime = oldPacnetTransaction.PostDate; //DateTime.UtcNow;
				nlModel.PacnetTransaction.Amount = oldPacnetTransaction.Amount; //nlModel.Loan.InitialLoanAmount;
				nlModel.PacnetTransaction.Notes = oldPacnetTransaction.Description;
				nlModel.PacnetTransaction.TrackingNumber = oldPacnetTransaction.TrackingNumber;
				nlModel.PacnetTransactionStatus = "sdfgsdfgsdg"; // oldPacnetTransaction.Status.ToString();
			}

			var s = new AddLoan(nlModel);
			try {
				s.Execute();
			} catch (Exception e) {
				Console.WriteLine(e);
			}
		}


		[Test]
		public void TestNL_AddPayment() {
			int customerID = 369;
			int loanID = 5;
			decimal amount = 5;

			NL_Model nlModel = new NL_Model(customerID);

			nlModel.Loan = new NL_Loans() {
				LoanID = loanID
			};

			nlModel.PaypointTransactionStatus = "Done";

			nlModel.Payment = new NL_Payments() {
				PaymentMethodID = 2,
				PaymentTime = DateTime.UtcNow,
				IsActive = true,
				Amount = amount,
				Notes = "bbbbblala"
			};

			nlModel.PaypointTransaction = new NL_PaypointTransactions() {
				TransactionTime = DateTime.UtcNow,
				Amount = amount,
				Notes = "system-repay",
				PaypointUniqID = "4f0fce47-deb0-4667-bc65-f6edd3c978b5",
				IP = "127.0.0.1",
				PaypointTransactionStatusID = 1
			};

			var s = new AddPayment(nlModel);
			try {
				s.Execute();
			} catch (Exception e) {
				Console.WriteLine(e);
			}
		}

		[Test]
		public void TestMultipleLoanState() {
			var loans = new[] {
				1, 2, 3, 4, 5
			};
			foreach (var lID in loans) {
				try {
					var s = new LoanState<Loan>(new Loan(), lID, DateTime.UtcNow);
					s.Execute();
				} catch (Exception e) {
					Console.WriteLine(e);
				}
			}
		}


		/*[Test]
		public void TestLoanState() {
			int loanID = 2151; // cust 329;   
			var s = new LoanState<Loan>(new Loan(), loanID, DateTime.UtcNow);
			try {
				s.Execute();
				LoanCalculatorModel calculatorModel = s.CalcModel;
				Console.WriteLine(calculatorModel.ToString());
			} catch (Exception e) {
				Console.WriteLine(e);
			}
		}
*/

		[Test]
		public void TestRescheduleOUT() {
			const int loanID = 5211; //4182; // 1718; // 4439; //3534;
			Loan loan = new Loan();
			ReschedulingArgument reModel = new ReschedulingArgument();
			reModel.LoanType = loan.GetType().AssemblyQualifiedName;
			reModel.LoanID = loanID;
			reModel.ReschedulingDate = DateTime.UtcNow.Date.AddDays(8); //new DateTime(2015, 10, 02); 
			reModel.ReschedulingRepaymentIntervalType = RepaymentIntervalTypes.Month;
			reModel.SaveToDB = false;
			reModel.RescheduleIn = false;
			reModel.PaymentPerInterval = 0m;
			reModel.StopFutureInterest = true;
			var s1 = new RescheduleLoan<Loan>(loan, reModel);
			s1.Context.UserID = 357; //25852;
			try {
				s1.Execute();
				this.m_oLog.Debug("RESULT FOR OUT" + s1.Result.ToString());
				// ReSharper disable once CatchAllClause
			} catch (Exception e) {
				Console.WriteLine(e);
			}
		}

		[Test]
		public void TestRescheduleIN() {
			const int loanID = 11;
			Loan loan = new Loan();
			ReschedulingArgument reModel = new ReschedulingArgument();
			reModel.LoanType = loan.GetType().AssemblyQualifiedName;
			reModel.LoanID = loanID;
			reModel.ReschedulingDate = DateTime.UtcNow.Date.AddDays(5);
			reModel.ReschedulingRepaymentIntervalType = RepaymentIntervalTypes.Month;
			reModel.RescheduleIn = true;
			reModel.SaveToDB = false;
			reModel.StopFutureInterest = true;
			try {
				var s = new RescheduleLoan<Loan>(loan, reModel);
				s.Context.UserID =  357; // 25852
				s.Execute();
				this.m_oLog.Debug("RESULT FOR IN" + s.Result.ToString());
				// ReSharper disable once CatchAllClause
			} catch (Exception e) {
				Console.WriteLine(e);
			}
		}

		[Test]
		public void TestMultipleRescheduling() {
			/*var loans = new[] {
				18,19,20,22,29,30,31,32,33,34,36,37,38, 35, 4439, 3534, 1846, 2662,	1721,1758,1718,1764,1781,1795,1808,1810,1825,1831,1838,1841,1847,1856,1895,1903,1904,
				3456,3462,3476,3478,3509,3510,3513,3521,3533,3535,3538,3539,3547,3583,3593,3607,3651,3670,3700,3710,3711,3714,3754,3827,3860,3917,3946,4081,4139,4174,4183,4192,
				2990,2996,3003,3007,3032,3038,3083,3084,3094,3096,3113,3118,3141,3142,3166,3196,3208,3236,3275,3285,3302
			};
			// select top 10 *  from [dbo].[Loan] l left join [dbo].[LoanScheduleDeleted] d on l.Id=d.LoanId where d.Id IS NULL and l.Status <> 'PaidOff' and YEAR(l.Date) = 2015 and DateClosed is null
			foreach (var loanID in loans) {
				try {
					ReschedulingArgument reModel = new ReschedulingArgument();
					Loan loan = new Loan();
					reModel.LoanType = loan.GetType().AssemblyQualifiedName;
					reModel.LoanID = loanID;
					reModel.RescheduleIn = false;
					reModel.SaveToDB = false;
					reModel.ReschedulingDate = DateTime.UtcNow;
					reModel.ReschedulingRepaymentIntervalType = RepaymentIntervalTypes.Month;
					if (reModel.RescheduleIn == false) {
						reModel.PaymentPerInterval = 900m;
					}
					var s = new RescheduleLoan<Loan>(loan, reModel);
					s.Context.UserID = 25852;
					s.Execute();
					m_oLog.Debug(s.Result.ToString());
				} catch (Exception e) {
					Console.WriteLine(e);
				}
			}*/


            Loan loan = new Loan();

			this.m_oDB.ForEachRowSafe((sr) => {
				try {
					int loanid = sr["Id"];

					// IN
				    ReschedulingArgument reModel = new ReschedulingArgument();
					reModel.LoanID = loanid;
					reModel.LoanType = loan.GetType().AssemblyQualifiedName;
					reModel.RescheduleIn = true;
					reModel.SaveToDB = false;
					reModel.ReschedulingDate = DateTime.UtcNow.Date.AddDays(15);
					reModel.ReschedulingRepaymentIntervalType = RepaymentIntervalTypes.Month;
                    var s = new RescheduleLoan<Loan>(loan, reModel);
					s.Context.UserID = 357; //25852;
					s.Execute();
					this.m_oLog.Debug(s.Result.ToString());
					this.m_oLog.Debug("IN_RESULT: {0}", s.Result.ToString());

					/*// OUT
					ReschedulingArgument reModel1 = new ReschedulingArgument();
					Loan loan1 = new Loan();
					reModel1.LoanID = loanid;
					reModel1.LoanType = loan1.GetType().AssemblyQualifiedName;
					reModel1.RescheduleIn = false;
					reModel1.SaveToDB = false;
					reModel1.ReschedulingDate = DateTime.UtcNow.Date.AddDays(44);
					reModel1.ReschedulingRepaymentIntervalType = RepaymentIntervalTypes.Month;
					reModel1.PaymentPerInterval = 90m;
					reModel1.StopFutureInterest = true;
					var s1 = new RescheduleLoan<Loan>(loan1, reModel1);
					s1.Context.UserID = 357; //25852;
					s1.Execute();
					this.m_oLog.Debug("OUT_RESULT: {0}", s1.Result.ToString());*/

				} catch (Exception e) {
					Console.WriteLine(e);
				}
			},
			"select top 10 l.Id from [dbo].[Loan] l left join [dbo].[LoanScheduleDeleted] d on l.Id=d.LoanId where d.Id IS NULL and l.Status <> 'PaidOff' and DateClosed is null order by l.Id desc", 
		//	"select top 10 * from [dbo].[Loan] l left join [dbo].[LoanScheduleDeleted] d on l.Id=d.LoanId where d.Id IS NULL and l.Status <> 'PaidOff' and YEAR(l.Date) = 2015 and DateClosed is null",
			CommandSpecies.Text); //top 100 
		}


		[Test]
		public void TestLoanOldCalculator() {
			int	loanID = 5211;
			LoanRepository loanRep = ObjectFactory.GetInstance<LoanRepository>();
			Loan loan = loanRep.Get(loanID);

			ChangeLoanDetailsModelBuilder loanModelBuilder = new ChangeLoanDetailsModelBuilder();
			EditLoanDetailsModel model = new EditLoanDetailsModel();
			var loaan =  ObjectFactory.GetInstance<LoanRepository>().Get(loanID);

			// 1. build model from DB loan
			model = loanModelBuilder.BuildModel(loaan);
			//m_oLog.Debug("===========================" + model.InterestFreeze.Count);

			// 2. create DB loan from the model
			Loan loan1 = loanModelBuilder.CreateLoan(model);
			//m_oLog.Debug("----------------------" + loan1.InterestFreeze.Count);

			var calc = new LoanRepaymentScheduleCalculator(loan1, DateTime.UtcNow, CurrentValues.Instance.AmountToChargeFrom);
			calc.GetState();
			this.m_oLog.Debug("---------------------------------------Loan recalculated: \n {0}", loan1);
		}

		/*[Test]
		public void TestLoanCalculator() {

			// new instance of loan calculator - for new schedules list
			/*LoanCalculatorModel calculatorModel = new LoanCalculatorModel() {
				LoanIssueTime = DateTime.UtcNow,
				LoanAmount = 6000m,
				RepaymentCount = 7,
				MonthlyInterestRate = 0.06m,
				InterestOnlyRepayments = 0,
				RepaymentIntervalType = RepaymentIntervalTypes.Month
			};

			Console.WriteLine("Calc model for new schedules list: " + calculatorModel);

			ALoanCalculator calculator = new LegacyLoanCalculator(calculatorModel);

			// new schedules
			try {
				//var shedules = calculator.CreateSchedule();
			} catch (InterestOnlyMonthsCountException interestOnlyMonthsCountException) {
				Console.WriteLine(interestOnlyMonthsCountException);
			} catch (NegativeMonthlyInterestRateException negativeMonthlyInterestRateException) {
				Console.WriteLine(negativeMonthlyInterestRateException);
			} catch (NegativeLoanAmountException negativeLoanAmountException) {
				Console.WriteLine(negativeLoanAmountException);
			} catch (NegativeRepaymentCountException negativeRepaymentCountException) {
				Console.WriteLine(negativeRepaymentCountException);
			} catch (NegativeInterestOnlyRepaymentCountException negativeInterestOnlyRepaymentCountException) {
				Console.WriteLine(negativeInterestOnlyRepaymentCountException);
			}

			Console.WriteLine();
			var scheduleswithinterests = calculator.CreateScheduleAndPlan();#1#

			decimal A = 6000m;
			decimal m = 600m;
			decimal r = 0.06m;
			decimal F = 100m;

			decimal n = Math.Ceiling(A / (m - A * r));
			Console.WriteLine("n=" + n);
			decimal total1 = A + A * r * ((n + 1) / 2);
			Console.WriteLine(total1);
			decimal B = (A + F);

			//	decimal total2 = B + B * r * (((n - 1) + 1) / 2);
			//	Console.WriteLine(total2);

			decimal k = Math.Ceiling(n + 2 * F / (A * r));

			/*	LoanCalculatorModel calculatorModel = new LoanCalculatorModel() {
					LoanIssueTime = DateTime.UtcNow,
					LoanAmount = A,
					RepaymentCount = (int)n,
					MonthlyInterestRate = 0.06m,
					InterestOnlyRepayments = 0,
					RepaymentIntervalType = RepaymentIntervalTypes.Month
				};

				Console.WriteLine("Calc model for new schedules list: " + calculatorModel);

				ALoanCalculator calculator = new LegacyLoanCalculator(calculatorModel);

				Console.WriteLine();
				var scheduleswithinterests = calculator.CreateScheduleAndPlan();#1#

			LoanCalculatorModel calculatorModel2 = new LoanCalculatorModel() {
				LoanIssueTime = DateTime.UtcNow,
				LoanAmount = B,
				RepaymentCount = (int)(k),
				MonthlyInterestRate = 0.06m,
				InterestOnlyRepayments = 0,
				RepaymentIntervalType = RepaymentIntervalTypes.Month
			};

			Console.WriteLine("Calc model for new schedules list: " + calculatorModel2);

			ALoanCalculator calculator2 = new LegacyLoanCalculator(calculatorModel2);

			Console.WriteLine();
			List<ScheduledItemWithAmountDue> scheduleswithinterests2 = calculator2.CreateScheduleAndPlan();

			Console.WriteLine(scheduleswithinterests2.Sum(x => x.AccruedInterest));
		}*/

	

		[Test]
		public void TestSFRetrier() {
			DateTime now = DateTime.UtcNow;
			AddOpportunity add = new AddOpportunity(28, new OpportunityModel {
				Name = "NewOpportunity",
				Email = "alexbo+off02@ezbob.com",
				CreateDate = now,
				ExpectedEndDate = now.AddDays(7),
				RequestedAmount = 1000,
				Stage = OpportunityStage.s5.DescriptionAttr(),
				Type = OpportunityType.FinishLoan.DescriptionAttr()
			});

			add.Execute();
		}


		[Test]
		public void TestLoanInterestRate() {
			LoanRepository loanRep = ObjectFactory.GetInstance<LoanRepository>();
			Loan loan = loanRep.Get(5211);

			var firstSchedule = loan.Schedule.OrderBy(s => s.Date).FirstOrDefault();
			var lastSchedule = loan.Schedule.OrderBy(s => s.Date).LastOrDefault();

			Console.WriteLine(firstSchedule);
			Console.WriteLine(lastSchedule);

			var calc = new LoanRepaymentScheduleCalculator(loan, DateTime.UtcNow, CurrentValues.Instance.AmountToChargeFrom);

			decimal r = 5;
			if (firstSchedule != null && lastSchedule!=null)
				//r = calc.GetInterestRate(firstSchedule.Date, lastSchedule.Date);
			r = calc.GetInterestRate(firstSchedule.Date, new DateTime(2099, 01, 01));

			this.m_oLog.Debug("{0}", loan);

			Console.WriteLine(r);			
		}

		[Test]
		public void TestLoanInterestRateBetweenDates() {
			LoanRepository loanRep = ObjectFactory.GetInstance<LoanRepository>();
			Loan loan = loanRep.Get(5211);
			DateTime start = new DateTime(2015, 07, 08);
			DateTime end = new DateTime(2015, 10, 21);
			Console.WriteLine(start);
			Console.WriteLine(end);
			var calc = new LoanRepaymentScheduleCalculator(loan, DateTime.UtcNow, CurrentValues.Instance.AmountToChargeFrom);
			calc.GetState();
			//this.m_oLog.Debug("{0}",loan);
			decimal I = 0m;
			decimal P = 57024.55m;
			TimeSpan ts = end.Date.Subtract(start.Date);
			Console.WriteLine(ts);
			int dcounter = 1;
			while (dcounter < ts.Days) {
				DateTime s = start.Date.AddDays(dcounter);
				DateTime e = s.Date.AddDays(1);
				Console.WriteLine("{0}, {1}", s, e);
				decimal r = calc.GetInterestRate(s, e);
				dcounter++;
				Console.WriteLine("{0}, {1}", dcounter, r);
				I += P * r;
			}
			Console.WriteLine(I);
		}

		[Test]
		public void TestCollectionSms() {
			/*
			SetLateLoanStatus stra = new SetLateLoanStatus();
			stra.LoadSmsTemplates();
			var modelEzbob = new SetLateLoanStatus.CollectionDataModel {
				OriginID = 1,
				FirstName = "John",
				AmountDue = 125,
				DueDate = DateTime.Today.AddDays(-5),
				SmsSendingAllowed = false,
				CustomerID = 199
			};
			var modelEverline = new SetLateLoanStatus.CollectionDataModel {
				OriginID = 2,
				FirstName = "John",
				AmountDue = 125,
				DueDate = DateTime.Today.AddDays(-5),
				SmsSendingAllowed = false,
				CustomerID = 199
			};
			stra.SendCollectionSms(modelEzbob, SetLateLoanStatus.CollectionType.CollectionDay0);
			stra.SendCollectionSms(modelEzbob, SetLateLoanStatus.CollectionType.CollectionDay1to6);
			stra.SendCollectionSms(modelEzbob, SetLateLoanStatus.CollectionType.CollectionDay7);
			stra.SendCollectionSms(modelEzbob, SetLateLoanStatus.CollectionType.CollectionDay8to14);
			stra.SendCollectionSms(modelEzbob, SetLateLoanStatus.CollectionType.CollectionDay15);
			stra.SendCollectionSms(modelEzbob, SetLateLoanStatus.CollectionType.CollectionDay21);
			stra.SendCollectionSms(modelEzbob, SetLateLoanStatus.CollectionType.CollectionDay31);

			stra.SendCollectionSms(modelEverline, SetLateLoanStatus.CollectionType.CollectionDay0);
			stra.SendCollectionSms(modelEverline, SetLateLoanStatus.CollectionType.CollectionDay1to6);
			stra.SendCollectionSms(modelEverline, SetLateLoanStatus.CollectionType.CollectionDay7);
			stra.SendCollectionSms(modelEverline, SetLateLoanStatus.CollectionType.CollectionDay8to14);
			stra.SendCollectionSms(modelEverline, SetLateLoanStatus.CollectionType.CollectionDay15);
			stra.SendCollectionSms(modelEverline, SetLateLoanStatus.CollectionType.CollectionDay21);
			stra.SendCollectionSms(modelEverline, SetLateLoanStatus.CollectionType.CollectionDay31);
        [Test]
        public void TestDataSharing() {
            DataSharing stra = new DataSharing(16142, AlibabaBusinessType.APPLICATION_WS_3);
            stra.Execute();
            Console.WriteLine(stra.Result);

        }
			*/
		}

		[Test]
		public void TestGetIncomeSms() {
			var stra = new GetIncomeSms(null,true);
			stra.Execute();
		}

		[Test]
		public void TestPostcodeNutsStra() {
			var stra = new PostcodeNuts("AB10 1BA");
			stra.Execute();
			if (!stra.ExistsInCash) {
				Assert.IsNotNull(stra.Result);
				Assert.AreEqual(stra.Result.status, 200);
				Assert.IsNotNullOrEmpty(stra.Result.result.postcode);
				Assert.IsNotNullOrEmpty(stra.Result.result.codes.nuts);
			} else {
				this.m_oLog.Info("Exists in cash");
			}
		}

        [Test]
        public void TestBrokerLoadCustomerList()
        {
            
            var s = new BrokerLoadCustomerList("shlomi+naor@ezbob.com",423);
            s.Execute();

        }
	}
}