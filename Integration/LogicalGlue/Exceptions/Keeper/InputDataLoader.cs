namespace Ezbob.Integration.LogicalGlue.Exceptions.Keeper {
	using System;
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Harvester.Interface;
	using Ezbob.Logger;

	class InputDataLoader {
		public InputDataLoader(AConnection db, ASafeLog log, int customerID, DateTime now) {
			this.db = db;
			this.log = log;
			this.customerID = customerID;
			this.now = now;

			Result = new InferenceInput();
		} // constructor

		public InputDataLoader Execute() {
			return this;
		} // Execute

		public InferenceInput Result { get; private set; }

		private readonly AConnection db;
		private readonly ASafeLog log;
		private readonly int customerID;
		private readonly DateTime now;
	} // class InputDataLoader
} // namespace
