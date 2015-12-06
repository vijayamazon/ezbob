namespace Ezbob.Integration.LogicalGlue.Engine.Interface {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Runtime.Serialization;
	using Ezbob.Utils.Lingvo;

	[DataContract]
	public class Inference {
		[DataMember]
		public long ResponseID { get; set; }

		[DataMember]
		public DateTime ReceivedTime { get; set; }

		[DataMember]
		public Bucket? Bucket { get; set; }

		[DataMember]
		public SortedDictionary<ModelNames, ModelOutput> ModelOutputs {
			get {
				if (this.modelOutputs == null)
					this.modelOutputs = new SortedDictionary<ModelNames, ModelOutput>();

				return this.modelOutputs;
			} // get
			set {
				this.modelOutputs = value ?? new SortedDictionary<ModelNames, ModelOutput>();
			} // set
		} // ModelOutputs

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString() {
			return string.Format(
				"Response #{0} received at {1}: bucket '{2}' with {3}.",
				ResponseID,
				ReceivedTime.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture),
				Bucket,
				Grammar.Number(ModelOutputs.Count, "model")
			);
		} // ToString

		private SortedDictionary<ModelNames, ModelOutput> modelOutputs;
	} // class Inference
} // namespace
