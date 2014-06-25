namespace EzBobTest
{
	using EzBob.Backend.Strategies.MailStrategies;
	using EzBob.Backend.Strategies.Misc;
	using Ezbob.Backend.Models;
	using NUnit.Framework;

	[TestFixture]
	public class TestStrategies : BaseTestFixtue
	{

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
			var s = new PasswordChanged(3060, "dfsgfsdg", m_oDB, m_oLog);
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
		}
	}
}