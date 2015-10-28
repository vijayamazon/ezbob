namespace Ezbob.Integration.LogicalGlue.Interface {
	using System.Collections.Generic;
	using System.Linq;

	public class Grade {
		public decimal? Score { get; set; }

		public long EncodedResult { get; set; }

		public string DecodedResult { get; set; }

		public List<string> ListRangeErrors { get; set; }

		public Dictionary<string, decimal> MapOutputRatios { get; set; }
	} // class Grade

	public static class GradeExt {
		public static Grade CloneFrom(this Grade target, Grade source) {
			if (source == null)
				return new Grade();

			target = target ?? new Grade();

			target.Score = source.Score;
			target.EncodedResult = source.EncodedResult;
			target.DecodedResult = source.DecodedResult;

			if (target.MapOutputRatios == null)
				target.MapOutputRatios = new Dictionary<string, decimal>();
			else
				target.MapOutputRatios.Clear();

			if (source.MapOutputRatios != null)
				foreach (var pair in source.MapOutputRatios)
					target.MapOutputRatios[pair.Key] = pair.Value;

			if (target.ListRangeErrors == null)
				target.ListRangeErrors = new List<string>();
			else
				target.ListRangeErrors.Clear();

			if (source.ListRangeErrors != null)
				target.ListRangeErrors.AddRange(source.ListRangeErrors.Where(s => !string.IsNullOrWhiteSpace(s)));

			return target;
		} // CloneFrom
	} // class GradeExt
} // namespace
