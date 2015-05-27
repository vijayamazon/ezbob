namespace Ezbob.Backend.Strategies.Esign {
	using System;
	using System.Linq;
	using EchoSignLib;

	public class EsignSend : AStrategy {
		public override string Name {
			get { return "EsignSend"; }
		} // Name

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
			EchoSignSendResult result = esf.Send(oPackage);

			switch (result.Code) {
			case EchoSignSendResultCode.Success:
				Result = string.Empty;
				break;

			case EchoSignSendResultCode.Partial:
				Result = "Some envelopes were not sent.";
				break;

			case EchoSignSendResultCode.Fail:
				Result = "Sending failed.";
				break;

			default:
				throw new ArgumentOutOfRangeException();
			} // switch

			if (result.ErrorList.Count > 0)
				Result += "\n" + string.Join("\n", result.ErrorList);
		} // Execute

		public string Result { get; private set; }

		public EchoSignEnvelope[] Package;
	} // class EsignSend
} // namespace
