namespace Ezbob.Integration.LogicalGlue.Harvester.Interface {
	using System;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Newtonsoft.Json;

	public class InferenceOutput {
		public InferenceOutput() {
			this.decision = new BucketDecision(this);
		} // constructor

		[JsonProperty(PropertyName = "FL_response", NullValueHandling = NullValueHandling.Ignore)]
		public string FuzzyLogicResponse {
			get { return FuzzyLogic == null ? string.Empty : JsonConvert.SerializeObject(FuzzyLogic); }

			set {
				FuzzyLogic = string.IsNullOrWhiteSpace(value) ? null : JsonConvert.DeserializeObject<ModelOutput>(value);
			} // set
		} // FuzzyLogicResponse

		[JsonProperty(PropertyName = "NN_response", NullValueHandling = NullValueHandling.Ignore)]
		public string NeuralNetworkResponse {
			get { return NeuralNetwork == null ? string.Empty : JsonConvert.SerializeObject(NeuralNetwork); }

			set {
				NeuralNetwork = string.IsNullOrWhiteSpace(value) ? null : JsonConvert.DeserializeObject<ModelOutput>(value);
			} // set
		} // NeuralNetworkResponse

		[JsonIgnore]
		public ModelOutput FuzzyLogic { get; set; }

		[JsonIgnore]
		public ModelOutput NeuralNetwork { get; set; }

		[JsonIgnore]
		public Bucket? Bucket { get; set; }

		[JsonProperty(PropertyName = "decision", NullValueHandling = NullValueHandling.Ignore)]
		public BucketDecision Decision {
			get { return this.decision; }
			set {
				if (value == null) {
					this.decision.Bucket = null;
					this.decision.Reason = null;
				} else {
					if (!ReferenceEquals(this.decision, value)) {
						this.decision.Bucket = value.Bucket;
						this.decision.Reason = value.Reason;
					} // if
				} // if
			} // set
		} // Decision

		private readonly BucketDecision decision;

		public class BucketDecision {
			public BucketDecision(InferenceOutput parent) {
				this.parent = parent;
			} // constructor

			[JsonProperty(PropertyName = "reason", NullValueHandling = NullValueHandling.Ignore)]
			public string Reason {
				get {
					if (string.IsNullOrWhiteSpace(this.reason))
						this.reason = string.Empty;

					return this.reason;
				} // get

				set { this.reason = string.IsNullOrWhiteSpace(value) ? string.Empty : value; }
			} // Reason

			[JsonProperty(PropertyName = "bucket", NullValueHandling = NullValueHandling.Ignore)]
			public string Bucket {
				get {
					if (string.IsNullOrWhiteSpace(this.bucket)) {
						this.bucket = string.Empty;
						this.parent.Bucket = null;
					} // if

					return this.bucket;
				} // get

				set {
					this.parent.Bucket = null;
					this.bucket = value;

					if (string.IsNullOrWhiteSpace(this.bucket))
						return;

					int b;

					if (!int.TryParse(value, out b)) {
						this.parent.Bucket = null;
						return;
					} // if

					Bucket[] buckets = (Bucket[])Enum.GetValues(typeof(Bucket));

					foreach (Bucket bkt in buckets) {
						if ((int)bkt == b) {
							this.parent.Bucket = bkt;
							return;
						} // if
					} // for each

					this.parent.Bucket = null;
				} // set
			} // Bucket

			private readonly InferenceOutput parent;

			private string reason;
			private string bucket;
		} // class BucketDecision
	} // class InferenceOutput
} // namespace
