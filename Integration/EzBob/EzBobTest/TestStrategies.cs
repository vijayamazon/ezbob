namespace EzBobTest
{
	using System;
	using EzBob.Backend.Strategies.Experian;
	using EzBob.Backend.Strategies.MailStrategies;
	using EzBob.Backend.Strategies.Misc;
	using EzServiceAccessor;
	using EzServiceShortcut;
	using Ezbob.Backend.Models;
	using NUnit.Framework;
	using Newtonsoft.Json;
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
			var s = new UpdateMarketplace(3055, 3040, false, m_oDB, m_oLog);
			s.Execute();
		}

		[Test]
		public void Greeting()
		{
			var s = new Greeting(3060, "dfgdfsg", m_oDB, m_oLog);
			s.Execute();
		}

		[Test]
		public void ApprovedUser()
		{
			var s = new ApprovedUser(3060, 2500, m_oDB, m_oLog);
			s.Execute();
		}

		[Test]
		public void CashTransferred()
		{
			var s = new CashTransferred(21340, 2500, "01971847001", m_oDB, m_oLog);
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
			var s = new RejectUser(21370, m_oDB, m_oLog);
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
			var s = new SendEmailVerification(3060, "fakeemail", "dfg", m_oDB, m_oLog);
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
			new CalculateModelsAndAffordability(234, null, m_oDB, m_oLog).Execute();
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
	}
}