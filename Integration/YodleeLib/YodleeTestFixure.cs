namespace YodleeLib
{
	using System;
	using System.IO;
	using System.Linq;
	using System.Xml;
	using EzBob.CommonLib.Security;
	using NUnit.Framework;
	using Scorto.Configuration;
	using Scorto.Configuration.Loader;
	using Scorto.RegistryScanner;
	using log4net.Config;

	[TestFixture]
	class YodleeTestFixure
	{

		[SetUp]
		public void Init()
		{
			var paths = new [] {
				@"c:\alexbo\src\App\clients\Maven\maven.exe",
				@"c:\EzBob\App\clients\Maven\maven.exe"
			};

			foreach (string sPath in paths.Where(File.Exists))
			{
				EnvironmentConfigurationLoader.AppPathDummy = sPath;
				break;
			}

			Scanner.Register();
			
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

			// halifax "EZBOB+2012@ezbob.com", Encryptor.Decrypt("D61Ggo7aGrQTD/ZNsnqUfteq4mlqW00xAG1yL8wpfBA="  10321216 WBkLy450 
			// dag "EZBOB+2014@ezbob.com", Encryptor.Decrypt("sykuYcJcd+ShmykTY/pi0qBpRJK0a9HBiiw9NN0Dgjg="
			// mfa "EZBOB+2013@ezbob.com", Encryptor.Decrypt("qcw25++Ggp8/yi4bNkeB2Q64ABGU5YL+r7DPAZDrkkE="
			var pass = Encryptor.Decrypt("VmLkF2xNXAeuLAv/hWbh5iD2as2PgvyStMNEoUaoD0s=");
			long itemId = m.GetItemId("EZBOB+2012@ezbob.com", Encryptor.Decrypt("D61Ggo7aGrQTD/ZNsnqUfteq4mlqW00xAG1yL8wpfBA="),null, out dName, out csId);
			Console.WriteLine("{0} {1}", dName, itemId);
			Assert.That(itemId != -1);
			return itemId;
		}

		[Test]
		[Ignore]
		public void test_refresh_not_mfa()
		{
			var m = new YodleeMain();
			m.LoginUser("EZBOB+2014@ezbob.com", Encryptor.Decrypt("sykuYcJcd+ShmykTY/pi0qBpRJK0a9HBiiw9NN0Dgjg="));
			m.RefreshNotMFAItem(10334329, true);
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
