namespace EzBob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;
	using EchoSignLib;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class NotifyDocumentSigned : AMailStrategyBase {
		#region public

		#region constructor

		public NotifyDocumentSigned(EsignatureStatus oStatus, AConnection oDB, ASafeLog oLog) : base(oStatus.CustomerID, false, oDB, oLog) {
			m_oStatus = oStatus;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Notify Document Signed"; }
		} // Name

		#endregion property Name

		#endregion public

		#region protected

		protected override void SetTemplateAndVariables() {
			TemplateName = "Esign - Notify document status";

			Variables = new Dictionary<string, string> {
				{ "FirstName", CustomerData.FirstName },
				{ "LastName", CustomerData.Surname },
				{ "Status", m_oStatus.Status.ToString() },
			};
		} // SetTemplateAndVariables

		#endregion protected

		#region private

		private readonly EsignatureStatus m_oStatus;

		#endregion private
	} // class NotifyDocumentSigned
} // namespace
