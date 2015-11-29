namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation {
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Ezbob.Integration.LogicalGlue.Harvester.Interface;
	using Ezbob.Logger;

	internal class InferenceSaver {
		public InferenceSaver(AConnection db, ASafeLog log, int customerID, long requestID, Response<Reply> response) {
			this.db = db;
			this.log = log;
			this.customerID = customerID;
			this.requestID = requestID;
			this.response = response;

			Result = new Inference();
		} // constructor

		public InferenceSaver Execute() {
			return this;
		} // Execute

		public Inference Result { get; private set; }

		private readonly AConnection db;
		private readonly ASafeLog log;
		private readonly int customerID;
		private readonly long requestID;
		private readonly Response<Reply> response;
	} // class InferenceSaver
} // namespace
