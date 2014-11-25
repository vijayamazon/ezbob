namespace EzBobTest
{
	using System;
	using System.Collections.Generic;
	using ConfigManager;
	using EZBob.DatabaseLib.Model.Database;
	using EzBob.Backend.Strategies.Broker;
	using EzBob.Backend.Strategies.Experian;
	using EzBob.Backend.Strategies.MailStrategies;
	using EzBob.Backend.Strategies.MainStrategy;
	using EzBob.Backend.Strategies.MainStrategy.AutoDecisions;
	using EzBob.Backend.Strategies.MedalCalculations;
	using EzBob.Backend.Strategies.Misc;
	using EzBob.Backend.Strategies.OfferCalculation;
	using EzServiceAccessor;
	using EzServiceShortcut;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Database;
	using Ezbob.Utils.Security;
	using Ezbob.Utils.Serialization;
	using NUnit.Framework;
	using StructureMap;

	[TestFixture]
	public class TestStrategies : BaseTestFixtue
	{
		[SetUp]
		public void Init() {
			base.Init();

			ObjectFactory.Configure(x => {
				x.For<IEzServiceAccessor>().Use<EzServiceAccessorShort>();
			});

			EzServiceAccessorShort.Set(m_oDB, m_oLog);
		} // Init


		[Test]
		public void test_mainstrat()
		{
			var ms = new MainStrategy(21370, NewCreditLineOption.UpdateEverythingAndApplyAutoRules, 0, m_oDB, m_oLog);
			ms.Execute();
		}

		[Test]
		public void UpdateCustomerMarketplace()
		{
			var s = new UpdateMarketplace(21400, 18364, false, m_oDB, m_oLog);
			s.Execute();
		}

		[Test]
		public void Greeting()
		{
			var s = new Greeting(21401, "stasd+confirm@ezbob.com", m_oDB, m_oLog);
			s.Execute();
		}

		[Test]
		public void ApprovedUser()
		{
			var s = new ApprovedUser(3060, 2500, 24, true, m_oDB, m_oLog);
			s.Execute();
		}

		[Test]
		public void CashTransferred()
		{
			var s = new CashTransferred(21340, 2500, "01971847001", true, m_oDB, m_oLog);
			s.Execute();
		}

		[Test]
		public void EmailRolloverAdded()
		{
			var s = new EmailRolloverAdded(3060, 2500, m_oDB, m_oLog);
			s.Execute();
		}

		[Test]
		public void EmailUnderReview()
		{
			var s = new EmailUnderReview(3060, m_oDB, m_oLog);
			s.Execute();
		}

		[Test]
		public void Escalated()
		{
			var s = new Escalated(3060, m_oDB, m_oLog);
			s.Execute();
		}

		[Test]
		public void GetCashFailed()
		{
			var s = new GetCashFailed(3060, m_oDB, m_oLog);
			s.Execute();
		}

		[Test]
		public void LoanFullyPaid()
		{
			var s = new LoanFullyPaid(3060, "fdsfdf", m_oDB, m_oLog);
			s.Execute();
		}

		[Test]
		public void MoreAmlAndBwaInformation()
		{
			var s = new MoreAmlAndBwaInformation(3060, m_oDB, m_oLog);
			s.Execute();
		}

		[Test]
		public void MoreAmlInformation()
		{
			var s = new MoreAmlInformation(3060, m_oDB, m_oLog);
			s.Execute();
		}

		[Test]
		public void MoreBwaInformation()
		{
			var s = new MoreBwaInformation(3060, m_oDB, m_oLog);
			s.Execute();
		}

		[Test]
		public void PasswordChanged()
		{
			var s = new PasswordChanged(3060, new Password("dfsgfsdg"), m_oDB, m_oLog);
			s.Execute();
		}

		[Test]
		public void PasswordRestored()
		{
			var s = new PasswordRestored(3060, m_oDB, m_oLog);
			s.Execute();
		}

		[Test]
		public void PayEarly()
		{
			var s = new PayEarly(3060, 2500, "dfsgfsdg", m_oDB, m_oLog);
			s.Execute();
		}

		[Test]
		public void PayPointAddedByUnderwriter()
		{
			var s = new PayPointAddedByUnderwriter(3060, "dfgsdf", "dfsgfsdg", 5, m_oDB, m_oLog);
			s.Execute();
		}

		[Test]
		public void PayPointNameValidationFailed()
		{
			var s = new PayPointNameValidationFailed(3060, "dfgsdf", m_oDB, m_oLog);
			s.Execute();
		}

		[Test]
		public void RejectUser()
		{
			var s = new RejectUser(21370, true, m_oDB, m_oLog);
			s.Execute();
		}

		[Test]
		public void RenewEbayToken()
		{
			var s = new RenewEbayToken(3060, "sdfgfgg", "dsfg", m_oDB, m_oLog);
			s.Execute();
		}

		[Test]
		public void RequestCashWithoutTakenLoan()
		{
			var s = new RequestCashWithoutTakenLoan(3060, m_oDB, m_oLog);
			s.Execute();
		}

		[Test]
		public void SendEmailVerification()
		{
			var s = new SendEmailVerification(3060, "dfg", m_oDB, m_oLog);
			s.Execute();
		}

		[Test]
		public void ThreeInvalidAttempts()
		{
			var s = new ThreeInvalidAttempts(3060, m_oDB, m_oLog);
			s.Execute();
		}

		[Test]
		public void TransferCashFailed()
		{
			var s = new TransferCashFailed(3060, m_oDB, m_oLog);
			s.Execute();
		}

		[Test]
		public void TestGetBankModel() {
			new GetBankModel(234, m_oDB, m_oLog).Execute();
		} // TestGetBankModel

		[Test]
		public void TestCalculateModelsAndAffordability() {
			new CalculateModelsAndAffordability(15821, null, m_oDB, m_oLog).Execute();
		} // TestCalculateModelsAndAffordability

		[Test]
		public void TestMedalCalculation()
		{
			new CalculateMedal(m_oDB, m_oLog, 18570).Execute();
		} // TestCalculateModelsAndAffordability

		[Test]
		public void TestAutoReRejection()
		{
			var rerejection = new ReRejection(21334, m_oDB, m_oLog);
			var decision = new AutoDecisionRejectionResponse();
			var isrerejected = rerejection.MakeDecision(decision);
			Assert.AreEqual(false, isrerejected);
		} // TestCalculateModelsAndAffordability

		[Test]
		public void TestLREnquiry()
		{
			var s = new LandRegistryEnquiry(21340, null, "test", null, null, "E12 6AY", m_oDB, m_oLog);
			s.Execute();
			Assert.IsNotNullOrEmpty(s.Result);
		}

		[Test]
		public void TestLRRes()
		{
			var s = new LandRegistryRes(21378, "SK310937", m_oDB, m_oLog);
			s.Execute();
			Assert.IsNotNullOrEmpty(s.Result);
		}

		[Test]
		public void TestParseExperianConsumer()
		{
			var s = new ParseExperianConsumerData(110285, m_oDB, m_oLog);
			s.Execute();
			Console.WriteLine(s.Result.ToString());
			Assert.IsNotNull(s.Result);
		}

		[Test]
		public void TestLoadExperianLtd() {
			var s = new LoadExperianLtd("06357516", 0, m_oDB, m_oLog);
			s.Execute();
			Console.WriteLine(s.Result.ToString());
			Assert.IsNotNull(s.Result);
			Assert.IsNotNull(s.History);
		}

		[Test]
		public void TestLoadExperianNonLtd()
		{
			var s = new GetCompanyDataForCompanyScore(m_oDB, m_oLog, "10732957");
			s.Execute();
			Assert.IsNotNull(s.Data);
		}

		[Test]
		public void TestLoadExperianConsumer()
		{
			var s = new LoadExperianConsumerData(20323, null, 110285, m_oDB, m_oLog);
			s.Execute();
			Console.WriteLine(s.Result.ToString());
			Assert.IsNotNull(s.Result);

			s = new LoadExperianConsumerData(17254, null, null, m_oDB, m_oLog);
			s.Execute();
			Console.WriteLine(s.Result.ToString());
			Assert.IsNotNull(s.Result);

			s = new LoadExperianConsumerData(110285, 1014, null, m_oDB, m_oLog);
			s.Execute();
			Console.WriteLine(s.Result.ToString());
			Assert.IsNotNull(s.Result);
		}

		[Test]
		public void TestBackfillExperianConsumer()
		{
			var s = new BackfillExperianConsumer(m_oDB, m_oLog);
			s.Execute();
		}

		[Test]
		public void TestLoadExperianConsumerMortagageData()
		{
			var s = new LoadExperianConsumerMortgageData(20323, m_oDB, m_oLog);
			s.Execute();
			Assert.AreEqual(200089, s.Result.MortgageBalance);
			Assert.AreEqual(1, s.Result.NumMortgages);
			Console.WriteLine("{0} {1}", s.Result.NumMortgages, s.Result.MortgageBalance);
		}


		[Test]
		public void TestExperianConsumerCheck() {
			var s = new ExperianConsumerCheck(85, null, false, m_oDB, m_oLog);
			s.Execute();

			s = new ExperianConsumerCheck(85, 6, false, m_oDB, m_oLog);
			s.Execute();
		}

		[Test]
		public void TestGenerateCode() {
			//http://freesmsreceive.com/+441300452045.php
			var s = new GenerateMobileCode("01300452045", m_oDB, m_oLog);
			s.Execute();
		}


		[Test]
		public void testFraud() {
			var s = new FraudChecker(21340, FraudMode.FullCheck, m_oDB, m_oLog);
			s.Execute();
		}

		[Test]
		public void TestOfferCalculation() {
			var calc = new OfferDualCalculator(m_oDB, m_oLog);
			int calculatedOffers = 0;
			int failedVerificationOffers = 0;
			m_oDB.ForEachRowSafe(sr => {
				int customerId = sr["CustomerId"];
				MedalClassification medal = (MedalClassification)Enum.Parse(typeof(MedalClassification), sr["Medal"]);
				int offeredLoanAmount = sr["OfferedLoanAmount"];
				int numOfLoans = sr["NumOfLoans"];
				int zooplaValue = sr["ZooplaValue"];

				int roundedAmount = (int)Math.Truncate((decimal)offeredLoanAmount / CurrentValues.Instance.GetCashSliderStep) * CurrentValues.Instance.GetCashSliderStep;
				int cappedAmount = 0;

				if (zooplaValue>0)
				{
					cappedAmount = Math.Min(roundedAmount, CurrentValues.Instance.MaxCapHomeOwner);
				}
				else
				{
					cappedAmount = Math.Min(roundedAmount, CurrentValues.Instance.MaxCapNotHomeOwner);
				} // if

				var offer = calc.CalculateOffer(customerId, DateTime.UtcNow, cappedAmount, numOfLoans>0, medal);
				calculatedOffers++;
				if (offer == null || !string.IsNullOrEmpty(offer.Error)) {
					failedVerificationOffers++;
				}
			}, "SELECT CustomerId, Medal, OfferedLoanAmount, NumOfLoans, ZooplaValue FROM MedalCalculations WHERE IsActive=1 AND Medal<>'NoMedal' AND OfferedLoanAmount>0", CommandSpecies.Text);

			m_oLog.Debug("Calculated offer for {0} customers failed {1}", calculatedOffers, failedVerificationOffers);
			Assert.AreEqual(0, failedVerificationOffers);
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
			}, m_oDB, m_oLog);
			s.Execute();
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
	}
}