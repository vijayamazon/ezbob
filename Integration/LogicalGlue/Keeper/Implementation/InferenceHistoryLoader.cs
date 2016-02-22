namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	internal class InferenceHistoryLoader : AInferenceLoaderBase {
		public InferenceHistoryLoader(
			AConnection db,
			ASafeLog log,
			int customerID,
			DateTime now,
			bool includeTryOutData,
			int historyLength
		) : base(db, log, 0, customerID, now, historyLength, includeTryOutData, 0) {
		} // constructor

		public InferenceHistoryLoader Execute() {
			Load();
			return this;
		} // Execute
	} // class InferenceHistoryLoader
} // namespace
