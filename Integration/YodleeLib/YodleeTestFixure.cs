namespace YodleeLib
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Xml;
	using EzBob.CommonLib.Security;
	using NUnit.Framework;
	using Scorto.Configuration;
	using Scorto.Configuration.Loader;
	using Scorto.RegistryScanner;
	using log4net.Config;

	[TestFixture]
	internal class YodleeTestFixure
	{

		[SetUp]
		public void Init()
		{
			var paths = new[]
				{
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
			var pass = Encryptor.Decrypt("SH24xKutqoIefQ9oz2gjD9rITakU29ZXQpGA9SY8UMw=");
			long itemId = m.GetItemId("EZBOB+311@ezbob.com", pass, null, out dName, out csId);
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

		[Test]
		[Ignore]
		public void test_get_data_for_item()
		{
			var m = new YodleeMain();
			var pass = Encryptor.Decrypt("SH24xKutqoIefQ9oz2gjD9rITakU29ZXQpGA9SY8UMw=");
			m.LoginUser("EZBOB+311@ezbob.com", pass);
			GetBankData g = new GetBankData();
			string s1;
			string err;
			Dictionary<BankData, List<BankTransactionData>> data;
			//var itemId = test_get_itemId();
			long itemId = 10403741;
			g.GetBankDataForItem(m.UserContext, itemId, out s1, out err, out data);

			Console.WriteLine("info {0}, errors:{1}, count of data:{2}", s1, err, data.Keys.Count);

		}

		[Test]
		public void test_regex()
		{
			string literal = @"\w{3}\d{10}[a-zA-Z]\d{3}";
			string description = "abc1122334455x123";
			bool containsLiteral = false;
			if (literal.Contains("\\")) //is regex
			{
				containsLiteral = Regex.IsMatch(description.ToLowerInvariant(), literal);
			}
			else if (description.ToLowerInvariant().Contains(literal))
			{
				containsLiteral = true;
			}

			Assert.AreEqual(true, containsLiteral);
		}
	}
}
