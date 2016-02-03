namespace PacnetBalance
{
	using System;
	using Ezbob.Logger;
	using NUnit.Framework;

	[TestFixture]
	class PacNetTestFixture
	{
		private Conf m_oConf;

		[SetUp]
		public void Init()
		{
			var pacnetcfg = new Conf();
			pacnetcfg.Init();
			m_oConf = pacnetcfg;
		}

		//[Test]
		//[Ignore]
		//public void testSendEmail()
		//{
		//	Mailer.Mailer.SendMail(m_oConf.LoginAddress, m_oConf.LoginPassword, "PacNet Balance Report Error", (new Exception("Test Exception")).ToString(), "alexbo@ezbob.com");
		//}

		[Test]
		public void TestParsing1() {
			string path = @"c:\ezbob\test-data\pacnet\pacnet20150113.pdf";
			byte[] data = System.IO.File.ReadAllBytes(path);
			ParsePacNetText p = new ParsePacNetText();
			ParsePacNetText.Logger = new ConsoleLog();
			p.ParsePdf(data);
			PacNetBalance.SavePacNetBalanceToDb();
		}

		[Test]
		public void TestParsing2() {
			string path = @"c:\ezbob\test-data\pacnet\pacnet20150825.pdf";
			byte[] data = System.IO.File.ReadAllBytes(path);
			ParsePacNetText p = new ParsePacNetText();
			ParsePacNetText.Logger = new ConsoleLog();
			p.ParsePdf(data);
			PacNetBalance.SavePacNetBalanceToDb();
		}

		[Test]
		public void TestParsing3() {
			string path = @"c:\ezbob\test-data\pacnet\pacnet20150908.pdf";
			byte[] data = System.IO.File.ReadAllBytes(path);
			ParsePacNetText p = new ParsePacNetText();
			ParsePacNetText.Logger = new ConsoleLog();
			p.ParsePdf(data);
			PacNetBalance.SavePacNetBalanceToDb();
		}

		[Test]
		public void TestParsing4() {
			string path = @"c:\ezbob\test-data\pacnet\pacnet20150908.pdf";
			byte[] data = System.IO.File.ReadAllBytes(path);
			ParsePacNetText p = new ParsePacNetText();
			ParsePacNetText.Logger = new ConsoleLog();
			p.ParsePdf(data);
			PacNetBalance.SavePacNetBalanceToDb();
		}

		[Test]
		public void TestParsing5() {
			string path = @"c:\ezbob\test-data\pacnet\pacnet20160120.pdf";
			byte[] data = System.IO.File.ReadAllBytes(path);
			ParsePacNetText p = new ParsePacNetText();
			ParsePacNetText.Logger = new ConsoleLog();
			p.ParsePdf(data);
			PacNetBalance.SavePacNetBalanceToDb();
		}

		[Test]
		public void TestParsing6() {
			string path = @"c:\ezbob\test-data\pacnet\pacnet20160121.pdf";
			byte[] data = System.IO.File.ReadAllBytes(path);
			ParsePacNetText p = new ParsePacNetText();
			ParsePacNetText.Logger = new ConsoleLog();
			p.ParsePdf(data);
			PacNetBalance.SavePacNetBalanceToDb();
		}

		[Test]
		public void TestParsing7() {
			string path = @"c:\ezbob\test-data\pacnet\pacnet20160122.pdf";
			byte[] data = System.IO.File.ReadAllBytes(path);
			ParsePacNetText p = new ParsePacNetText();
			ParsePacNetText.Logger = new ConsoleLog();
			p.ParsePdf(data);
			PacNetBalance.SavePacNetBalanceToDb();
		}

		[Test]
		public void TestParsing8() {
			string path = @"c:\ezbob\test-data\pacnet\pacnet20160123.pdf";
			byte[] data = System.IO.File.ReadAllBytes(path);
			ParsePacNetText p = new ParsePacNetText();
			ParsePacNetText.Logger = new ConsoleLog();
			p.ParsePdf(data);
			PacNetBalance.SavePacNetBalanceToDb();
		}

		[Test]
		public void TestParsing9() {
			string path = @"c:\ezbob\test-data\pacnet\pacnet20160125.pdf";
			byte[] data = System.IO.File.ReadAllBytes(path);
			ParsePacNetText p = new ParsePacNetText();
			ParsePacNetText.Logger = new ConsoleLog();
			p.ParsePdf(data);
			PacNetBalance.SavePacNetBalanceToDb();
		}

		[Test]
		public void TestParsing10() {
			string path = @"c:\ezbob\test-data\pacnet\pacnet20160202.pdf";
			byte[] data = System.IO.File.ReadAllBytes(path);
			ParsePacNetText p = new ParsePacNetText();
			ParsePacNetText.Logger = new ConsoleLog();
			p.ParsePdf(data);
			PacNetBalance.SavePacNetBalanceToDb();
		}
	}
}
