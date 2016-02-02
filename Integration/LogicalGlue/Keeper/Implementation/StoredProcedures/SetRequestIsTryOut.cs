namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation.StoredProcedures {
	using System;
	using System.Data;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	class SetRequestIsTryOut : ALogicalGlueStoredProc {
		public SetRequestIsTryOut(AConnection db, ASafeLog log) : base(db, log) {
		} // constructor

		public override bool HasValidParameters() {
			return RequestUniqueID != Guid.Empty;
		} // HasValidParameters

		[UsedImplicitly]
		public Guid RequestUniqueID { get; set; }

		[UsedImplicitly]
		public bool NewIsTryOutStatus { get; set; }

		[UsedImplicitly]
		public DateTime Now {
			get { return DateTime.UtcNow; }
			// ReSharper disable once ValueParameterNotUsed
			set { }
		} // Now

		[UsedImplicitly]
		[Direction(ParameterDirection.Output)]
		public long RequestID { get; set; }
	} // class SetRequestIsTryOut
} // namespace
