namespace Ezbob.Integration.LogicalGlue.Engine.Interface {
	using System;
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using Ezbob.Utils;

	[DataContract]
	public class Inference {
		[DataMember]
		public decimal MonthlyRepayment { get; set; }

		[DataMember]
		public DateTime ReceivedTime { get; set; }

		[DataMember]
		public Bucket Bucket { get; set; }

		[DataMember]
		[NonTraversable]
		public SortedDictionary<RequestType, ModelOutput> ModelOutputs {
			get {
				if (this.modelOutputs == null)
					this.modelOutputs = new SortedDictionary<RequestType, ModelOutput>();

				return this.modelOutputs;
			} // get
			set {
				this.modelOutputs = value ?? new SortedDictionary<RequestType, ModelOutput>();
			} // set
		} // ModelOutputs

		private SortedDictionary<RequestType, ModelOutput> modelOutputs;
	} // class Inference
} // namespace
