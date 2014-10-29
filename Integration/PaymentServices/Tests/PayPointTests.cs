namespace PaymentServices.Tests
{
	using System;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Database.UserManagement;
	using NHibernate;
	using NHibernateWrapper.NHibernate;
	using NUnit.Framework;
	using PayPoint;
	using Ezbob.RegistryScanner;
	using StructureMap;
	using StructureMap.Pipeline;
	using global::PayPoint;

	// return money
	class PayPointTests
	{
		[SetUp]
		public void Setup()
		{
			NHibernateManager.FluentAssemblies.Add(typeof(User).Assembly);
			NHibernateManager.FluentAssemblies.Add(typeof(Customer).Assembly);
			NHibernateManager.FluentAssemblies.Add(typeof(PayPointDatabaseMarketPlace).Assembly);
			Scanner.Register();
			ObjectFactory.Configure(x =>
			{
				x.For<ISession>().LifecycleIs(new ThreadLocalStorageLifecycle()).Use(ctx => NHibernateManager.SessionFactory.OpenSession());
				x.For<ISessionFactory>().Use(() => NHibernateManager.SessionFactory);
				x.For<ILoanRepository>().Use<LoanRepository>();
			});
		}

		[Test]
		public void Test1()
		{
			PayPointApi papi = new PayPointApi();
			papi.RefundCard("Mr Cardholder", "4444333322221111", 50.99M, new DateTime(2015, 1, 1), "1", new DateTime(2009, 1, 1), "product=ezbob", "123", true);

			string repo = papi.GetReport("CSV", "Date", DateTime.Now.ToString("yyyyMMdd"), "GBP");
			/*SECVPNService service = new SECVPNService();
			service.

			string r = service.validateCardFull("secpay", "secpay", "TRAN00001_SC", "127.0.0.1", "Mr Cardholder",
									 "4444333322221111", "50.99", "0115", "1", "0109",
									 "prod=funny_book,item_amount=25.00x1;prod=sad_book,item_amount=12.50x2", "", "",
									 "test_status=true,dups=false,card_type=Visa,cv2=123");
			string res = service.refundCardFull("secpay", "secpay", "TRAN00001_SC", "50.99", "secpay", "TRAN00001_SC_refund");
			var report = service.getReport("secpay", "secpay", "secpay", "CSV", "Date", "201204", "GBP", "", false, false);*/
		}

		#region PayPointPayPal

		[Test]
		public void PayPointPayPal()
		{
			var papi = new PayPointApi();
			papi.PayPointPayPal("www.google.com", "www.google.com", "www.google.com", 5.0M, "GBP", true);
		}

		#endregion End PayPointPayPal
	}
}
