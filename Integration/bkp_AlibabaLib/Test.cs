namespace AlibabaLib {
	using NUnit.Framework;

	[TestFixture]
	public class Test {

		/*[SetUp]
		public void Init() {

			NHibernateManager.FluentAssemblies.Add(typeof(AlibabaBuyer).Assembly);
			NHibernateManager.FluentAssemblies.Add(typeof(AlibabaSentData).Assembly);

			Scanner.Register();

			ObjectFactory.Configure(x => {
				x.For<ISession>()
				 .LifecycleIs(new ThreadLocalStorageLifecycle())
				 .Use(ctx => NHibernateManager.SessionFactory.OpenSession());

				x.For<ISessionFactory>().Use(() => NHibernateManager.SessionFactory);
				//x.For<AlibabaBuyerRepository>().Use<AlibabaBuyerRepository>();
				//x.For<AlibabaSentDataRepository>().Use<AlibabaSentDataRepository>();

			});		

		}*/

		/*[Test]
		public void Test_1() {
			/*string c = CurrentValues.Instance.AlibabaAppSecret_Sandbox.Value;
			Console.WriteLine(c);#1#
			var appSettings = WebConfigurationManager.AppSettings;
			Console.WriteLine(appSettings.Get("Environment"));
			/*foreach (var k in appSettings.AllKeys) {
			//	if (k.Equals("Environment")) {
					Console.WriteLine("Key: {0} ", k);
					Console.WriteLine(appSettings.Get("Environment"));
			//	}
			}#1#
		}


		[Test]
		public void Test_2() {
			var c = new AlibabaApiClient();
			//c.SetConnection();
		}


		[Test]
		public void Test_3() {

			//	Console.WriteLine(new Random().);
			//Console.WriteLine(new Random().Next(Int32.MaxValue) );
			//	Console.WriteLine(BizType.APPLICATION.DescriptionAttr());
			var c = new AlibabaApiClient();
			CustomerDataSharing data = new CustomerDataSharing();
			data.aId = 12345;
			data.aId = 18234;
			data.compCity = "Kent";

			c.SendDecision(data, 0);
		}*/
	}


}