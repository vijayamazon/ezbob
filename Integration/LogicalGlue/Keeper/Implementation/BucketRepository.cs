﻿namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation {
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Ezbob.Integration.LogicalGlue.Keeper.Implementation.StoredProcedures;
	using Ezbob.Logger;

	internal class BucketRepository : AEnum<Bucket, int> {
		public BucketRepository(AConnection db, ASafeLog log) : base(db, log) {
		} // constructor

		protected override ALogicalGlueStoredProc StoredProc {
			get { return new LoadBuckets(DB, Log); }
		} // get
	} // class BucketRepository
} // namespace
