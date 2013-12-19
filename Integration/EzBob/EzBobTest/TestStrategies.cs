namespace EzBobTest
{
	using System.Xml;
	using EKM;
	using EzBob.Backend.Strategies;
	using EzBob.Backend.Strategies.MailStrategies;
	using FreeAgent;
	using Sage;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using EzBob.AmazonLib;
	using EzBob.CommonLib;
	using EzBob.PayPal;
	using EzBob.eBayLib;
	using NHibernate;
	using NUnit.Framework;
	using Scorto.Configuration;
	using Scorto.Configuration.Loader;
	using Scorto.NHibernate;
	using Scorto.RegistryScanner;
	using StructureMap;
	using StructureMap.Pipeline;
	using log4net.Config;
    using PayPoint;
    using YodleeLib.connector;
	using System.IO;
	using Integration.ChannelGrabberFrontend;

	[TestFixture]
	public class TestStrategies
	{
		private DatabaseDataHelper _Helper;

		[SetUp]
		public void Init()
		{
			var paths = new string[]
				{
					@"c:\alexbo\src\App\clients\Maven\maven.exe",
					@"c:\EzBob\App\clients\Maven\maven.exe"
				};

			foreach (string sPath in paths)
			{
				if (File.Exists(sPath))
				{
					EnvironmentConfigurationLoader.AppPathDummy = sPath;
					break;
				} // if
			} // foreach

			NHibernateManager.FluentAssemblies.Add(typeof (ApplicationMng.Model.Application).Assembly);
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

			var cfg = ConfigurationRoot.GetConfiguration();

			XmlElement configurationElement = cfg.XmlElementLog;
			XmlConfigurator.Configure(configurationElement);

			_Helper = ObjectFactory.GetInstance<IDatabaseDataHelper>() as DatabaseDataHelper;
		}

		[Test]
		public void UpdateCustomerMarketplace()
		{
			var s = new UpdateMarketplaces();
			//s.CustomerMarketPlaceAddedAlternative(3055, 3040);
		}

		[Test]
		public void Greeting()
		{
			var s = new Greeting(3060, "dfgdfsg");
			s.Execute();
		}

		[Test]
		public void ApprovedUser()
		{
			var s = new ApprovedUser(3060, 2500);
			s.Execute();
		}

		[Test]
		public void CashTransferred()
		{
			var s = new CashTransferred(3060, 2500);
			s.Execute();
		}

		[Test]
		public void EmailRolloverAdded()
		{
			var s = new EmailRolloverAdded(3060, 2500);
			s.Execute();
		}

		[Test]
		public void EmailUnderReview()
		{
			var s = new EmailUnderReview(3060);
			s.Execute();
		}

		[Test]
		public void Escalated()
		{
			var s = new Escalated(3060);
			s.Execute();
		}

		[Test]
		public void GetCashFailed()
		{
			var s = new GetCashFailed(3060);
			s.Execute();
		}

		[Test]
		public void LoanFullyPaid()
		{
			var s = new LoanFullyPaid(3060, "fdsfdf");
			s.Execute();
		}

		[Test]
		public void MoreAMLandBWAInformation()
		{
			var s = new MoreAMLandBWAInformation(3060);
			s.Execute();
		}

		[Test]
		public void MoreAMLInformation()
		{
			var s = new MoreAMLInformation(3060);
			s.Execute();
		}

		[Test]
		public void MoreBWAInformation()
		{
			var s = new MoreBWAInformation(3060);
			s.Execute();
		}

		[Test]
		public void PasswordChanged()
		{
			var s = new PasswordChanged(3060, "dfsgfsdg");
			s.Execute();
		}

		[Test]
		public void PasswordRestored()
		{
			var s = new PasswordRestored(3060, "dfsgfsdg");
			s.Execute();
		}

		[Test]
		public void PayEarly()
		{
			var s = new PayEarly(3060, 2500, "dfsgfsdg");
			s.Execute();
		}

		[Test]
		public void PayPointAddedByUnderwriter()
		{
			var s = new PayPointAddedByUnderwriter(3060, "dfgsdf", "dfsgfsdg", 5);
			s.Execute();
		}

		[Test]
		public void PayPointNameValidationFailed()
		{
			var s = new PayPointNameValidationFailed(3060, "dfgsdf");
			s.Execute();
		}

		[Test]
		public void RejectUser()
		{
			var s = new RejectUser(3060);
			s.Execute();
		}

		[Test]
		public void RenewEbayToken()
		{
			var s = new RenewEbayToken(3060, "sdfgfgg", "dsfg");
			s.Execute();
		}

		[Test]
		public void RequestCashWithoutTakenLoan()
		{
			var s = new RequestCashWithoutTakenLoan(3060);
			s.Execute();
		}

		[Test]
		public void SendEmailVerification()
		{
			var s = new SendEmailVerification(3060, "dfg");
			s.Execute();
		}

		[Test]
		public void ThreeInvalidAttempts()
		{
			var s = new ThreeInvalidAttempts(3060, "dfg");
			s.Execute();
		}

		[Test]
		public void TransferCashFailed()
		{
			var s = new TransferCashFailed(3060);
			s.Execute();
		}
	}
}