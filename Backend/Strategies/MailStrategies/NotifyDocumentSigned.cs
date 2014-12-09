namespace Ezbob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;
	using EchoSignLib;

	public class NotifyDocumentSigned : AMailStrategyBase {

		public NotifyDocumentSigned(EsignatureStatus oStatus) : base(oStatus.CustomerID, false) {
			m_oStatus = oStatus;
		} // constructor

		public override string Name {
			get { return "Notify Document Signed"; }
		} // Name

		protected override void SetTemplateAndVariables() {
			TemplateName = "Esign - Notify document status";

			Variables = new Dictionary<string, string> {
				{ "FirstName", CustomerData.FirstName },
				{ "LastName", CustomerData.Surname },
				{ "Status", m_oStatus.Status.ToString() },
			};
		} // SetTemplateAndVariables

		private readonly EsignatureStatus m_oStatus;

	} // class NotifyDocumentSigned
} // namespace
