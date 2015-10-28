namespace Ezbob.Integration.LogicalGlue.Interface {
	using System.Collections.Generic;

	public class Grade {
		public decimal? Score { get; set; }

		public long EncodedResult { get; set; }

		public string DecodedResult { get; set; }

		public List<Warning> Warnings{
			get {
				if (this.warnings == null)
					this.warnings = new List<Warning>();

				return this.warnings;
			} // get
			set { this.warnings = Utility.SetList(this.warnings, value); }
		} // Warnings

		public Dictionary<string, decimal> MapOutputRatios {
			get {
				if (this.mapOutputRatios == null)
					this.mapOutputRatios = new Dictionary<string, decimal>();

				return this.mapOutputRatios;
			} // get
			set {
				if (this.mapOutputRatios == null)
					this.mapOutputRatios = new Dictionary<string, decimal>();
				else
					this.mapOutputRatios.Clear();

				if (value != null)
					foreach (KeyValuePair<string, decimal> pair in value)
						this.mapOutputRatios[pair.Key] = pair.Value;
			} // set
		} // MapOutputRatios

		private List<Warning> warnings;
		private Dictionary<string, decimal> mapOutputRatios;
	} // class Grade

	public static class GradeExt {
		public static Grade CloneFrom(this Grade target, Grade source) {
			if (source == null)
				return new Grade();

			target = target ?? new Grade();

			target.Score = source.Score;
			target.EncodedResult = source.EncodedResult;
			target.DecodedResult = source.DecodedResult;
			target.MapOutputRatios = source.MapOutputRatios;
			target.Warnings = Utility.SetList(target.Warnings, source.Warnings);

			return target;
		} // CloneFrom
	} // class GradeExt
} // namespace
