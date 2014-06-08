namespace EzBob.Backend.Strategies.VatReturn {
	using Ezbob.Database;
	using Ezbob.Logger;

	public abstract class AVatReturnStrategy : AStrategy {
		#region constructor

		protected AVatReturnStrategy(AConnection oDb, ASafeLog oLog) : base(oDb, oLog) {
		} // constructor

		#endregion constructor

		#region enum DeleteReasons

		protected enum DeleteReasons {
			UploadedEqual = 1,
			UploadedNotEqual = 2,
			OverriddenByUploaded = 3,
			ManualUpdated = 4,
			ManualDeleted = 5,
			ManualRejectedByUploaded = 6,
			ManualRejectedByLinked = 7,
			UploadedRejectedByLinked = 8,
			LinkedUpdated = 9,
		} // enum DeleteReasons

		#endregion enum DeleteReasons
	} // class AVatReturnStrategy
} // namespace EzBob.Backend.Strategies.VatReturn
