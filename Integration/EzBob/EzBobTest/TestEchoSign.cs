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
		public void TestSendBoardResolutionCustomerOnly() {
			var esf = new EchoSignFacade(m_oDB, m_oLog);
			esf.Send(new [] { new EchoSignEnvelope {
				CustomerID = 28, Directors = null, TemplateID = 1, SendToCustomer = true,
			}});
		} // TestSendBoardResolutionCustomerOnly

		[Test]
		public void TestSendBoardResolution() {
			var esf = new EchoSignFacade(m_oDB, m_oLog);
			esf.Send(new [] { new EchoSignEnvelope {
				CustomerID = 28, Directors = new [] { 2, 3, }, TemplateID = 1, SendToCustomer = true,
			}});
		} // TestSendBoardResolution

		[Test]
		public void TestSendPersonalGuarantee() {
			var esf = new EchoSignFacade(m_oDB, m_oLog);

			esf.Send(new [] { new EchoSignEnvelope {
				CustomerID = 28, Directors = new [] { 2, 3, }, TemplateID = 2, SendToCustomer = false,
			}});
		} // TestSendPersonalGuarantee

		[Test]
		public void TestProcessPending() {
			var esf = new EchoSignFacade(m_oDB, m_oLog);
			esf.ProcessPending();
		} // TestProcessPending

		/*

		[Test]
		public void TestGetDocuments() {
			var esf = new EchoSignFacade(m_oDB, m_oLog);

			esf.GetDocuments("2AAABLblqZhCLzXHaKf9HRvMQro37ZP6p_tCJjO_hleBEIWOn6NnAGoa64wJ1_nUv3HonlaN1FxQ*");
			esf.GetDocuments("2AAABLblqZhDm9gTJPe2IQjuTHvLioVbvr2raDTZBx-Yqsummg2hkqycHR0eOD6Tr9yVDjjBzqYo*");
			esf.GetDocuments("2AAABLblqZhAin59yVaeGKEMrRFv67nQdeMmdOD7vzQ-FQTusV0dCKFo7XtenbUtBNCY1MW1TfC8*");
		} // TestGetDocuments

		[Test]
		public void TestGetDocumentInfo() {
			var esf = new EchoSignFacade(m_oDB, m_oLog);

			esf.GetDocumentInfo("2AAABLblqZhCLzXHaKf9HRvMQro37ZP6p_tCJjO_hleBEIWOn6NnAGoa64wJ1_nUv3HonlaN1FxQ*");
			esf.GetDocumentInfo("2AAABLblqZhDm9gTJPe2IQjuTHvLioVbvr2raDTZBx-Yqsummg2hkqycHR0eOD6Tr9yVDjjBzqYo*");
			esf.GetDocumentInfo("2AAABLblqZhAin59yVaeGKEMrRFv67nQdeMmdOD7vzQ-FQTusV0dCKFo7XtenbUtBNCY1MW1TfC8*");
			esf.GetDocumentInfo("AAABLblqZhDijFbJ2Nof3ybm-K9vYa1PKO74QBavqbOr3-eyjNUfQ8rNBQ7TwLkUSQO-4QJrlYM*");
			esf.GetDocumentInfo("2AAABLblqZhDZnbD0l9bDKx_EcHnJxIcfDBiL0dbFEKg-3qhRy2il0mnf7moJVty83cjdtBReFKs*");
		} // TestGetDocumentInfo

		 */
	} // class TestEchoSign
} // namespace
