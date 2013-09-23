namespace YodleeLib
{
	using System;
	using System.IO;
	using System.Xml;
	using EzBob.CommonLib.Security;
	using NUnit.Framework;
	using Scorto.Configuration;
	using Scorto.Configuration.Loader;
	using Scorto.NHibernate;
	using Scorto.RegistryScanner;
	using StructureMap;
	using StructureMap.Pipeline;
	using config;
	using log4net.Config;

	[TestFixture]
	class YodleeTestFixure
	{

		[SetUp]
		public void Init()
		{
			var paths = new string[] {
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

			Scanner.Register();
			
			//ObjectFactory.Configure(x =>
			//{
			//	x.For<IYodleeMarketPlaceConfig>().Singleton().Use(ezBobConfigRoot.YodleeConfig);
			//});
			var cfg = ConfigurationRoot.GetConfiguration();

			XmlElement configurationElement = cfg.XmlElementLog;
			XmlConfigurator.Configure(configurationElement);
		}

		[Test]
		[Ignore]
		public long test_get_itemId()
		{
			var m = new YodleeMain();
			string dName;
			long csId;
			long itemId = m.GetItemId("EZBOB+2013@ezbob.com", Encryptor.Decrypt("qcw25++Ggp8/yi4bNkeB2Q64ABGU5YL+r7DPAZDrkkE="), out dName, out csId);
			Console.WriteLine("{0} {1}", dName, itemId);
			Assert.That(itemId != -1);
			return itemId;
		}

		[Test]
		[Ignore]
		public void test_remove_item()
		{
			var m = new YodleeMain();
			m.RemoveItem(test_get_itemId());
		}
	}
}
