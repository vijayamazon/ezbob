namespace Ezbob.Backend.Strategies.LogicalGlue {
	using System;
	using Ezbob.Integration.LogicalGlue;

	public class SetRequestIsTryOut : AStrategy {
		public SetRequestIsTryOut(Guid requestID, bool newIsTryOutStatus) {
			this.requestID = requestID;
			this.newIsTryOutStatus = newIsTryOutStatus;
			Success = false;
		} // constructor

		public override string Name {
			get { return "SetRequestIsTryOut"; }
		} // Name

		public override void Execute() {
			Success = InjectorStub.GetEngine().SetRequestIsTryOut(this.requestID, this.newIsTryOutStatus);
		} // Execute

		public bool Success { get; private set; }

		private readonly Guid requestID;
		private readonly bool newIsTryOutStatus;
	} // class SetRequestIsTryOut
} // namespace

