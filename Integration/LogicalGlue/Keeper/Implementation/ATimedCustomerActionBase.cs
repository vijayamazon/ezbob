namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation {
	using System;
	using System.Globalization;
	using Ezbob.Database;
	using Ezbob.Logger;

	internal abstract class ATimedCustomerActionBase : ACustomerActionBase {
		protected ATimedCustomerActionBase(
			AConnection db,
			ASafeLog log,
			int customerID,
			DateTime now
		) : base(db, log, customerID) {
			Now = now;
			NowStr = Now.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture);
		} // constructor

		protected DateTime Now { get; private set; }
		protected string NowStr { get; private set; }
	} // class ATimedCustomerActionBase
} // namespace
