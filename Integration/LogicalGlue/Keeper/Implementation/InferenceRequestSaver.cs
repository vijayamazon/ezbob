namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation {
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Harvester.Interface;
	using Ezbob.Logger;

	internal class InferenceRequestSaver {
		public InferenceRequestSaver(AConnection db, ASafeLog log, int customerID, InferenceInput request) {
			Result = 0;

			this.db = db;
			this.log = log;
			this.customerID = customerID;
			this.request = request;
		} // constructor

		public long Result { get; private set; }

		public InferenceRequestSaver Execute() {
			return this;
		} // Execute

		private readonly AConnection db;
		private readonly ASafeLog log;
		private readonly int customerID;
		private readonly InferenceInput request;
	} // class InferenceRequestSaver
} // namespace
