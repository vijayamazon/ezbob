namespace Ezbob.Integration.LogicalGlue.Models {
	using System.Collections.Generic;
	using Ezbob.Integration.LogicalGlue.Interface;

	public class ModelOutput : IModelOutput {
		public string Status { get; set; }

		public decimal? Score { get; set; }

		public long EncodedResult { get; set; }

		public string DecodedResult { get; set; }

		IReadOnlyDictionary<string, decimal> IModelOutput.MapOutputRatios {
			get { return MapOutputRatios; }
			set {
				MapOutputRatios = MapOutputRatios ?? new Dictionary<string, decimal>();
				MapOutputRatios.Clear();

				foreach (var pair in value)
					MapOutputRatios[pair.Key] = pair.Value;
			} // set
		} // IModelOutput.MapOutputRatios

		IReadOnlyCollection<string> IModelOutput.ListRangeErrors {
			get { return ListRangeErrors.AsReadOnly(); }
			set {
				if (ListRangeErrors == null)
					ListRangeErrors = new List<string>(value);
				else {
					ListRangeErrors.Clear();
					ListRangeErrors.AddRange(value);
				} // if
			} // set
		} // IModelOutput.ListRangeErrors

		public List<string> ListRangeErrors { get; set; }

		public Dictionary<string, decimal> MapOutputRatios { get; set; }
	} // class ModelOutput
} // namespace
