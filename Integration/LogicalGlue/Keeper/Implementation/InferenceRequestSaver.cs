namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation {
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Harvester.Interface;
	using Ezbob.Logger;

	internal class InferenceRequestSaver : AActionBase {
		public InferenceRequestSaver(
			AConnection db,
			ASafeLog log,
			int customerID,
			InferenceInput request
		) : base(db, log, customerID) {
			Result = 0;
			this.request = request;
		} // constructor

		public long Result { get; private set; }

		public InferenceRequestSaver Execute() {
			// TODO
			return this;
		} // Execute

		private readonly InferenceInput request;
	} // class InferenceRequestSaver
} // namespace
