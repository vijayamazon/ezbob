namespace Ezbob.Backend.Strategies.VatReturn {
	public abstract class AVatReturnStrategy : AStrategy {
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
	} // class AVatReturnStrategy
} // namespace Ezbob.Backend.Strategies.VatReturn
