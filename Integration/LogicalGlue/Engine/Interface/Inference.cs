namespace Ezbob.Integration.LogicalGlue.Engine.Interface {
	using System;
	using System.Collections.Generic;
	using System.Runtime.Serialization;

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

		private SortedDictionary<ModelNames, ModelOutput> modelOutputs;
	} // class Inference
} // namespace
