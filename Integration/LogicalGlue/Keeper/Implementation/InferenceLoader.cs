namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation {
	using System;
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Ezbob.Logger;

	internal class InferenceLoader : AInferenceLoaderBase {
		internal InferenceLoader(
			AConnection db,
			ASafeLog log,
			int customerID,
			DateTime now,
			bool includeTryOutData,
			decimal monthlyPayment
		) : base(db, log, 0, customerID, now, 1, includeTryOutData, monthlyPayment) {
		} // constructor

		internal InferenceLoader(
			AConnection db,
			ASafeLog log,
			long responseID,
			int customerID
		) : base(db, log, responseID, customerID, DateTime.UtcNow, 1, true, 0) {
		} // constructor

		public virtual Inference Result {
			get { return Results.Count > 0 ? Results[0] : null; }
		} // Result

		public InferenceLoader Execute() {
			Load();
			return this;
		} // Execute
	} // class InferenceLoader
} // namespace
