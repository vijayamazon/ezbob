namespace Ezbob.Backend.Strategies.Esign {
	using System.Collections.Generic;
	using EchoSignLib;
	using Ezbob.Backend.Strategies.MailStrategies;

	public class EsignProcessPending : AStrategy {

		public EsignProcessPending(int? nCustomerID) {
			m_nCustomerID = nCustomerID;
			m_oFacade = new EchoSignFacade(DB, Log);
		} // constructor

		public override string Name {
			get { return "EsignProcessPending"; }
		} // Name

		public override void Execute() {
			List<EsignatureStatus> oCompleted = m_oFacade.ProcessPending(m_nCustomerID);

			foreach (var oStatus in oCompleted)
				new NotifyDocumentSigned(oStatus).Execute();
		} // Execute

		private readonly EchoSignFacade m_oFacade;
		private readonly int? m_nCustomerID;

	} // class EsignProcessPending
} // namespace
