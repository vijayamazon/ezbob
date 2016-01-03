namespace Ezbob.Integration.LogicalGlue.Engine.Interface {
	using System;
	using System.Runtime.Serialization;

	[DataContract]
	public static class InferenceExt {
		public static bool IsUpToDate(this Inference inference, DateTime now, int acceptenceDays) {
			if ((inference == null) || (inference.ResponseID <= 0))
				return false;

			return inference.ReceivedTime.AddDays(acceptenceDays).Date >= now.Date;
		} // IsUpToDate
	} // class InferenceExt
} // namespace
