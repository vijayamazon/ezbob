namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation {
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Ezbob.Integration.LogicalGlue.Harvester.Interface;
	using Ezbob.Logger;

	internal class InferenceSaver : AActionBase {
		public InferenceSaver(
			AConnection db,
			ASafeLog log,
			int customerID,
			long requestID,
			Response<Reply> response
		) : base(db, log, customerID) {
			this.requestID = requestID;
			this.response = response;

			Result = new Inference();
		} // constructor

		public InferenceSaver Execute() {
			// TODO
			return this;
		} // Execute

		public Inference Result { get; private set; }

		private readonly long requestID;
		private readonly Response<Reply> response;
	} // class InferenceSaver
} // namespace
