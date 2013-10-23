using System.IO;
using Ezbob.HmrcHarvester;
using Ezbob.Logger;
using Integration.ChannelGrabberFrontend;

namespace EzBobTest
{
	using System;
	using System.Collections.Concurrent;
	using System.Linq;
	using System.Threading;
	using System.Xml;
	using EKM;
	using EzBob.Configuration;
	using EzBob.Models;
	using FreeAgent;
	using Sage;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EzBob;
	using EzBob.AmazonLib;
	using EzBob.AmazonServiceLib;
	using EzBob.CommonLib;
	using EzBob.PayPal;
	using EzBob.PayPalDbLib.Models;
	using EzBob.PayPalServiceLib;
	using EzBob.eBayLib;
	using EzBob.eBayLib.Config;
	using EzBob.eBayServiceLib;
	using NHibernate;
	using NUnit.Framework;
	using Scorto.Configuration;
	using Scorto.Configuration.Loader;
	using Scorto.NHibernate;
	using Scorto.RegistryScanner;
	using StructureMap;
	using StructureMap.Pipeline;
	using log4net;
	using log4net.Config;
    using PayPoint;
    using YodleeLib.connector;
	using MailApi;

	[TestFixture]
	public class TestStrategyHelper
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
				x.For<IMail>().Use<Mail>();
				x.For<IMandrillConfig>().Use<MandrillConfig>();
			});

			var cfg = ConfigurationRoot.GetConfiguration();

			XmlElement configurationElement = cfg.XmlElementLog;
			XmlConfigurator.Configure(configurationElement);

			_Helper = ObjectFactory.GetInstance<IDatabaseDataHelper>() as DatabaseDataHelper;
		}

		// This test is meant only for DEV tests, do not run it as a unit-test
		// Returning hardcoded values from MandrillConfig.cs will be required
		//[Test]
		public void testFirstOfMonthMail()
		{
			//StrategyHelper sa = new StrategyHelper();
			//sa.SendFirstOfMonthStatusMail();
		}
	}
}