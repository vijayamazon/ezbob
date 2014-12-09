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

		[Test]
		[Ignore]
		public void testSendEmail()
		{
			Mailer.Mailer.SendMail(m_oConf.LoginAddress, m_oConf.LoginPassword, "PacNet Balance Report Error", (new Exception("Test Exception")).ToString(), "alexbo@ezbob.com");
		}
	}
}
