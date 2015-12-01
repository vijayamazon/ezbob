namespace Ezbob.Integration.LogicalGlue.Engine.Interface {
	using System;
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using Ezbob.Utils;

	[DataContract]
	public class Inference {
		// TODO : request? [DataMember] public ...

		[DataMember]
		public DateTime ReceivedTime { get; set; }

		[DataMember]
		public Bucket Bucket { get; set; }

		[DataMember]
		[NonTraversable]
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
