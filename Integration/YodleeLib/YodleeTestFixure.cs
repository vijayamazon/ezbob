namespace YodleeLib
{
	using System;
	using System.Collections.Generic;
	using System.Text.RegularExpressions;
	using BankTransactionsParser;
	using Ezbob.Utils.Security;
	using NUnit.Framework;

	[TestFixture]
	internal class YodleeTestFixure
	{
		[Test]
        [Ignore("Ignore this fixture")]
		public long test_get_itemId()
		{
			var m = new YodleeMain();
			string dName;
			long csId;

			// halifax "EZBOB+2012@ezbob.com", Encrypted.Decrypt("D61Ggo7aGrQTD/ZNsnqUfteq4mlqW00xAG1yL8wpfBA="  10321216 WBkLy450 
			// dag "EZBOB+2014@ezbob.com", Encrypted.Decrypt("sykuYcJcd+ShmykTY/pi0qBpRJK0a9HBiiw9NN0Dgjg="
			// mfa "EZBOB+2013@ezbob.com", Encrypted.Decrypt("qcw25++Ggp8/yi4bNkeB2Q64ABGU5YL+r7DPAZDrkkE="
			var pass = Encrypted.Decrypt("SH24xKutqoIefQ9oz2gjD9rITakU29ZXQpGA9SY8UMw=");
			long itemId = m.GetItemId("EZBOB+311@ezbob.com", pass, null, out dName, out csId);
			Console.WriteLine("{0} {1}", dName, itemId);
			Assert.That(itemId != -1);
			return itemId;
		}

		[Test]
        [Ignore("Ignore this fixture")]
		public void test_refresh_not_mfa()
		{
			var m = new YodleeMain();
			m.LoginUser("EZBOB+2014@ezbob.com", Encrypted.Decrypt("sykuYcJcd+ShmykTY/pi0qBpRJK0a9HBiiw9NN0Dgjg="));
			m.RefreshNotMFAItem(10334329, true);
		}

		[Test]
        [Ignore("Ignore this fixture")]
		public void test_remove_item()
		{
			var m = new YodleeMain();
			m.RemoveItem(test_get_itemId());
		}

		[Test]
        [Ignore("Ignore this fixture")]
		public void test_get_data_for_item()
		{
			var m = new YodleeMain();
			var pass = Encrypted.Decrypt("SH24xKutqoIefQ9oz2gjD9rITakU29ZXQpGA9SY8UMw=");
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

		[Test]
		public void test_parse()
		{
			var parser = new TransactionsParser();
			var parsed = parser.ParseFile(@"c:\ezbob\test-data\bank\NatWest.csv");
			Assert.NotNull(parsed);
			Assert.AreEqual(null, parsed.Error);
		}
	}
}
