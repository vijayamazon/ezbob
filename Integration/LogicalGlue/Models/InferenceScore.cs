namespace Ezbob.Integration.LogicalGlue.Models {
	using System.Collections.Generic;

	public class InferenceScore {
		public InferenceResult Result { get; set; }
		public decimal? Score { get; set; }
		public List<OutputRatio> MapOutputRatios { get; set; }
	} // class InferenceScore
} // namespace
