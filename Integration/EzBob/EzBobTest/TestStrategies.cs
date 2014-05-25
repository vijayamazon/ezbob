using Ezbob.Database;
using Ezbob.Logger;
using log4net;

namespace EzBobTest
{
	using EKM;
	using EZBob.DatabaseLib.Model.Database.UserManagement;
	using EzBob.Backend.Strategies;
	using EzBob.Backend.Strategies.MailStrategies;
	using Ezbob.RegistryScanner;
	using FreeAgent;
	using Sage;
	using EZBob.DatabaseLib.Model.Database;
	using EzBob.AmazonLib;
	using EzBob.PayPal;
	using EzBob.eBayLib;
	using NHibernate;
	using NUnit.Framework;
	using NHibernateWrapper.NHibernate;
	using StructureMap;
	using StructureMap.Pipeline;
    using PayPoint;
    using YodleeLib.connector;
	using Integration.ChannelGrabberFrontend;

	[TestFixture]
	public class TestStrategies
	{
		[SetUp]
		public void Init()
		{
			NHibernateManager.FluentAssemblies.Add(typeof(User).Assembly);
			NHibernateManager.FluentAssemblies.Add(typeof (Customer).Assembly);
			NHibernateManager.FluentAssemblies.Add(typeof (eBayDatabaseMarketPlace).Assembly);
			NHibernateManager.FluentAssemblies.Add(typeof (AmazonDatabaseMarketPlace).Assembly);
			NHibernateManager.FluentAssemblies.Add(typeof (PayPalDatabaseMarketPlace).Assembly);
			NHibernateManager.FluentAssemblies.Add(typeof (EkmDatabaseMarketPlace).Assembly);
			NHibernateManager.FluentAssemblies.Add(typeof (DatabaseMarketPlace).Assembly);
			NHibernateManager.FluentAssemblies.Add(typeof (YodleeDatabaseMarketPlace).Assembly);
			NHibernateManager.FluentAssemblies.Add(typeof (PayPointDatabaseMarketPlace).Assembly);
			NHibernateManager.FluentAssemblies.Add(typeof (FreeAgentDatabaseMarketPlace).Assembly);
			NHibernateManager.FluentAssemblies.Add(typeof (SageDatabaseMarketPlace).Assembly);
			Scanner.Register();
			ObjectFactory.Configure(x =>
				{
					x.For<ISession>()
					 .LifecycleIs(new ThreadLocalStorageLifecycle())
					 .Use(ctx => NHibernateManager.SessionFactory.OpenSession());
					x.For<ISessionFactory>().Use(() => NHibernateManager.SessionFactory);
				});

			m_oLog = new SafeILog(LogManager.GetLogger(typeof(TestStrategies)));

			var env = new Ezbob.Context.Environment(m_oLog);
			m_oDB = new SqlConnection(env, m_oLog);
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
			var s = new RejectUser(3060, m_oDB, m_oLog);
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

		private AConnection m_oDB;
		private ASafeLog m_oLog;
	}
}