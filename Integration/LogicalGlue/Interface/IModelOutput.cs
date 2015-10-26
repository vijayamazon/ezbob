namespace Ezbob.Integration.LogicalGlue.Interface {
	using System.Collections.Generic;

	public interface IModelOutput {
		string Status { get; set; }
		decimal? Score { get; set; }
		long EncodedResult { get; set; }
		string DecodedResult { get; set; }
		IReadOnlyDictionary<string, decimal> MapOutputRatios { get; set; }
		IReadOnlyCollection<string> ListRangeErrors { get; set; }
	} // interface IModelOutput
} // namespace
