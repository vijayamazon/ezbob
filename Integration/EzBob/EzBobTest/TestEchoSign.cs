namespace EzBobTest {
	using System.IO;
	using EchoSignLib;
	using Ezbob.Utils;
	using NUnit.Framework;

	[TestFixture]
	class TestEchoSign : BaseTestFixtue {
		[Test]
		public void TestConfiguration() {
			var esf = new EchoSignFacade(m_oDB, m_oLog);
		} // TestConfiguration

		[Test]
		public void TestSendFile() {
			var esf = new EchoSignFacade(m_oDB, m_oLog);

			var aryEmails = new string[] {
				"alexbo+sign-01@ezbob.com",
				"alexbo+sign-02@ezbob.com",
			};

			const string sPath = @"c:\ezbob\test-data\sample_board_resolution.docx";

			esf.Send(aryEmails, "Board resolution", sPath, new MimeTypeResolver()[".docx"], File.ReadAllBytes(sPath));
		} // TestSendFile

		[Test]
		public void TestGetDocuments() {
			var esf = new EchoSignFacade(m_oDB, m_oLog);

			esf.GetDocuments("2AAABLblqZhCLzXHaKf9HRvMQro37ZP6p_tCJjO_hleBEIWOn6NnAGoa64wJ1_nUv3HonlaN1FxQ*");
			esf.GetDocuments("2AAABLblqZhDm9gTJPe2IQjuTHvLioVbvr2raDTZBx-Yqsummg2hkqycHR0eOD6Tr9yVDjjBzqYo*");
		} // TestGetDocuments
	} // class TestEchoSign
} // namespace
