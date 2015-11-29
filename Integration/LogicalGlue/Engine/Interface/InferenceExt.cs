namespace Ezbob.Integration.LogicalGlue.Engine.Interface {
	using System;
	using System.Runtime.Serialization;

	[DataContract]
	public static class InferenceExt {
		public static bool IsUpToDate(this Inference inference, DateTime now, int acceptenceDays) {
			if (inference == null)
				return false;

			return inference.ReceivedTime.AddDays(acceptenceDays) <= now;
		} // IsUpToDate
	} // class InferenceExt
} // namespace
