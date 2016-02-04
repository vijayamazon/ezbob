namespace Ezbob.Integration.LogicalGlue.Engine.Interface {
	using System.Collections.Generic;
	using System.Runtime.Serialization;

	[DataContract]
	public class Grade {
		[DataMember]
		public decimal? Score { get; set; }

		[DataMember]
		public long? EncodedResult { get; set; }

		[DataMember]
		public string DecodedResult { get; set; }

		[DataMember]
		public Dictionary<string, decimal> OutputRatios {
			get {
				if (this.outputRatios == null)
					this.outputRatios = new Dictionary<string, decimal>();

				return this.outputRatios;
			} // get
			set {
				if (this.outputRatios == null)
					this.outputRatios = new Dictionary<string, decimal>();
				else
					this.outputRatios.Clear();

				if (value != null)
					foreach (KeyValuePair<string, decimal> pair in value)
						this.outputRatios[pair.Key] = pair.Value;
			} // set
		} // OutputRatios

		private Dictionary<string, decimal> outputRatios;
	} // class Grade

	public static class GradeExt {
		public static Grade CloneFrom(this Grade target, Grade source) {
			if (source == null)
				return new Grade();

			target = target ?? new Grade();

			target.Score = source.Score;
			target.EncodedResult = source.EncodedResult;
			target.DecodedResult = source.DecodedResult;
			target.OutputRatios = source.OutputRatios;

			return target;
		} // CloneFrom
	} // class GradeExt
} // namespace
