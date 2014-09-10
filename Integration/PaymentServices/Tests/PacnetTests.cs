namespace PaymentServices.Tests
{
	using System;
	using ConfigManager;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.UserManagement;
	using Ezbob.Database;
	using Ezbob.Database.Pool;
	using Ezbob.Logger;
	using Ezbob.RegistryScanner;
	using NHibernate;
	using NHibernateWrapper.NHibernate;
	using NUnit.Framework;
	using PacNet;
	using StructureMap;
	using StructureMap.Pipeline;
	using log4net;

	//send money
	[TestFixture]
	class PacnetTests
	{
		[SetUp]
		public void Start()
		{
			NHibernateManager.FluentAssemblies.Add(typeof(User).Assembly);
			NHibernateManager.FluentAssemblies.Add(typeof(Customer).Assembly);
			Scanner.Register();
			ObjectFactory.Configure(x =>
			{
				x.For<ISession>().LifecycleIs(new ThreadLocalStorageLifecycle()).Use(ctx => NHibernateManager.SessionFactory.OpenSession());
				x.For<ISessionFactory>().Use(() => NHibernateManager.SessionFactory);
			});

			CurrentValues.Init(DbConnectionGenerator.Get(Log), Log);
			DbConnectionPool.ReuseCount = CurrentValues.Instance.ConnectionPoolReuseCount;
			AConnection.UpdateConnectionPoolMaxSize(CurrentValues.Instance.ConnectionPoolMaxSize);
		}

		[Test]
		public void SendMoney()
		{
			var service = new PacnetService();
			var ret = service.SendMoney(1, 5, "11-13-90", "00355973", "Test");
			ret = service.CheckStatus(1, ret.TrackingNumber);
		}

		[Test]
		public void CloseFile()
		{
			var service = new PacnetService();
			var ret = service.CloseFile(1, "orangemoney.wf-RT2012-04-25");
		}

		[Test]
		public void CloseTodayAndYesterdayFiles()
		{
			var service = new PacnetService();
			var ret = service.CloseTodayAndYesterdayFiles(1);
		}

		[Test]
		public void CheckStatus()
		{
			var service = new PacnetService();
			var ret = service.CheckStatus(1, "860270115");
		}

		[Test]
		public void GetReport()
		{
			//Debugger.Launch();
			var service = new PacnetService();
			// Default is past 30 days
			var endTime = DateTime.Now;
			var startTime = endTime.AddDays(-30);
			var ret = service.GetReport(endTime, startTime);
		}

		private static SafeILog Log
		{
			get
			{
				if (ms_oLog == null)
					ms_oLog = new SafeILog(LogManager.GetLogger(typeof(PacnetTests)));

				return ms_oLog;
			} // get
		} // Log

		private static SafeILog ms_oLog;
	}
}
