namespace Ezbob.Integration.LogicalGlue.Exceptions.Keeper {
	using System;
	using Ezbob.Logger;

	public class SetIsTryOutStatusAlert : LogicalGlueAlert {
		public SetIsTryOutStatusAlert(Guid requestID, bool newStatus, Exception inner, ASafeLog log) : base(
			log,
			inner,
			"Failed to change request {0} 'is try out' status to {1}.",
			requestID,
			newStatus
		) { } // constructor
	} // class SetIsTryOutStatusAlert
} // namespace
