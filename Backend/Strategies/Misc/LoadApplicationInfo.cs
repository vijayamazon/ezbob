namespace Ezbob.Backend.Strategies.Misc {
	using Ezbob.Backend.Models.ApplicationInfo;

	public class LoadApplicationInfo : AStrategy {
		public LoadApplicationInfo(long cashRequestID) {
			this.cashRequestID = cashRequestID;
			Result = new ApplicationInfoModel();
		} // constructor

		public override string Name {
			get { return "Load Application Info"; }
		} // Name

		public override void Execute() {
		} // Execute

		public ApplicationInfoModel Result { get; private set; }

		private readonly long cashRequestID;
	} // class LoadApplicationInfo
} // namespace

