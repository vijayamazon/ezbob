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
			decimal monthlyPayment,
			BucketRepository bucketRepo,
			TimeoutSourceRepository timeoutSourceRepo,
			EtlCodeRepository etlCodeRepo
		) : base(
			db,
			log,
			0,
			customerID,
			now,
			1,
			includeTryOutData,
			monthlyPayment,
			bucketRepo,
			timeoutSourceRepo,
			etlCodeRepo
		) {
		} // constructor

		internal InferenceLoader(
			AConnection db,
			ASafeLog log,
			long responseID,
			int customerID,
			BucketRepository bucketRepo,
			TimeoutSourceRepository timeoutSourceRepo,
			EtlCodeRepository etlCodeRepo
		) : base(db, log, responseID, customerID, DateTime.UtcNow, 1, true, 0, bucketRepo, timeoutSourceRepo, etlCodeRepo) {
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
