namespace Ezbob.Backend.Strategies.Esign {
	using System.Collections.Generic;
	using EchoSignLib;
	using Ezbob.Backend.Strategies.MailStrategies;

	public class EsignProcessPending : AStrategy {
		public EsignProcessPending(int? nCustomerID) {
			this.customerID = nCustomerID;
			this.facade = new EchoSignFacade(DB, Log);
		} // constructor

		public override string Name {
			get { return "EsignProcessPending"; }
		} // Name

		public override void Execute() {
			List<EsignatureStatus> oCompleted = this.facade.ProcessPending(this.customerID);

			foreach (var oStatus in oCompleted)
				new NotifyDocumentSigned(oStatus).Execute();
		} // Execute

		private readonly EchoSignFacade facade;
		private readonly int? customerID;
	} // class EsignProcessPending
} // namespace
