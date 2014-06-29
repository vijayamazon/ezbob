namespace EzBob.Backend.Strategies.Esign {
	using System;
	using System.Linq;
	using EchoSignLib;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class EsignSend : AStrategy {
		#region constructor

		public EsignSend(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "EsignSend"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			if (Package == null) {
				Result = "Nothing to send (package is NULL).";
				Log.Debug(Result);
				return;
			} // if

			if (Package.Length == 0) {
				Result = "Empty e-sign package received.";
				Log.Debug(Result);
				return;
			} // if

			var oPackage = Package.Where(x => x.IsValid).ToArray();

			if (oPackage.Length == 0) {
				Result = "No envelopes are ready to be sent in: " + string.Join("\n", (object[])Package);
				Log.Debug(Result);
				return;
			} // if

			Log.Debug("Send for signature request:\n{0}", string.Join("\n", (object[])oPackage));

			EchoSignFacade esf = new EchoSignFacade(DB, Log);
			EchoSignSendResult nResult = esf.Send(oPackage);

			switch (nResult) {
			case EchoSignSendResult.Success:
				Result = string.Empty;
				break;

			case EchoSignSendResult.Partial:
				Result = "Some envelopes were not sent.";
				break;

			case EchoSignSendResult.Fail:
				Result = "Sending failed";
				break;

			default:
				throw new ArgumentOutOfRangeException();
			} // switch
		} // Execute

		#endregion method Execute

		#region property Result

		public string Result { get; private set; }

		#endregion property Result

		public EchoSignEnvelope[] Package;
	} // class EsignSend
} // namespace
