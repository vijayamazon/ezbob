namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation {
	using System;
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Harvester.Interface;
	using Ezbob.Logger;

	class InputDataLoader : AActionBase {
		public InputDataLoader(AConnection db, ASafeLog log, int customerID, DateTime now) : base(db, log, customerID) {
			this.now = now;

			Result = new InferenceInput();
		} // constructor

		public InputDataLoader Execute() {
			// TODO
			return this;
		} // Execute

		public InferenceInput Result { get; private set; }

		private readonly DateTime now;
	} // class InputDataLoader
} // namespace
