﻿namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation {
	using System;
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Keeper.Implementation.StoredProcedures;
	using Ezbob.Logger;

	internal class InferenceLoadAttempter : InferenceLoader {
		internal InferenceLoadAttempter (
			AConnection db,
			ASafeLog log,
			int customerID,
			DateTime now,
			bool includeTryOutData,
			decimal monthlyPayment,
			BucketRepository bucketRepo,
			TimeoutSourceRepository timeoutSourceRepo
		) : base(db, log, customerID, now, includeTryOutData, monthlyPayment, bucketRepo, timeoutSourceRepo) {
		} // constructor

		protected override LoadInference CreateLoadInferenceProcedure() {
			return new LoadInferenceIfExists(DB, Log);
		} // CreateLoadInferenceProcedure
	} // class InferenceLoader
} // namespace
