namespace EzBob.Backend.Strategies.Esign {
	using System.Collections.Generic;
	using EchoSignLib;
	using Ezbob.Database;
	using Ezbob.Logger;
	using MailStrategies;

	public class EsignProcessPending : AStrategy {

		public EsignProcessPending(int? nCustomerID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_nCustomerID = nCustomerID;
			m_oFacade = new EchoSignFacade(DB, Log);
		} // constructor

		public override string Name {
			get { return "EsignProcessPending"; }
		} // Name

		public override void Execute() {
			List<EsignatureStatus> oCompleted = m_oFacade.ProcessPending(m_nCustomerID);

			foreach (var oStatus in oCompleted)
				new NotifyDocumentSigned(oStatus, DB, Log).Execute();
		} // Execute

		private readonly EchoSignFacade m_oFacade;
		private readonly int? m_nCustomerID;

	} // class EsignProcessPending
} // namespace
