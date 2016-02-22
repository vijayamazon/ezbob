namespace EzBobTest {
	using System;
	using System.Text.RegularExpressions;
	using EchoSignLib;
	using Newtonsoft.Json;
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

		[Test]
		public void TestMailRegex() {
			Regex ms_reEmail = new Regex(@"^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))$", RegexOptions.IgnoreCase);

			Assert.IsTrue(ms_reEmail.IsMatch("asd@sfsdf.fsf"));
			Assert.IsTrue(ms_reEmail.IsMatch("asd@sfsdf-sdfsdf.fsf"));
			Assert.IsFalse(ms_reEmail.IsMatch("as!@d@sfsdf-sdfsdf.f"));
		}

		[Test]
		public void HackForSendingESignatures() {
			var e = new EchoSignEnvelope[1];
			e[0] = new EchoSignEnvelope {
				CustomerID = 22436,
				//Directors = new[] {
				//	1138
				//},
				ExperianDirectors = new []{20817},
				SendToCustomer = false,
				TemplateID = 2
			};

			Console.WriteLine();
			Console.WriteLine("var oRequest = $.post(window.gRootPath + 'Underwriter/Esignatures/Send', {{ sPackage: '{0}', }});",
				JsonConvert.SerializeObject(e));

		}
	} // class TestEchoSign
} // namespace
