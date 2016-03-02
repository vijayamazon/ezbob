namespace Ezbob.Integration.LogicalGlue.Harvester.Interface {
	using System.Collections.Generic;
	using System.Net;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Ezbob.Utils.Extensions;
	using Newtonsoft.Json;

	public class Reply : IConvertableToShortString {
		public Reply() {
			this.parsedModels = new SortedDictionary<ModelNames, ModelOutput>();
		} // constructor

		[JsonProperty(PropertyName = "status")]
		public HttpStatusCode Status { get; set; }

		[JsonProperty(PropertyName = "timeout", NullValueHandling = NullValueHandling.Ignore)]
		public string Timeout { get; set; }

		[JsonProperty(PropertyName = "error", NullValueHandling = NullValueHandling.Ignore)]
		public string Error { get; set; }

		[JsonProperty(PropertyName = "logicalglue", NullValueHandling = NullValueHandling.Ignore)]
		public InferenceOutput Inference { get; set; }

		[JsonProperty(PropertyName = "equifax", NullValueHandling = NullValueHandling.Ignore)]
		public EquifaxData Equifax { get; set; }

		[JsonProperty(PropertyName = "etl", NullValueHandling = NullValueHandling.Ignore)]
		public Etl Etl { get; set; }

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString() {
			return this.ToLogStr();
		} // ToString

		public string ToShortString() {
			return string.Format(
				"Status '{0}', {1}, {2}, {3}, {4}, {5}.",
				Status,
				string.IsNullOrEmpty(Timeout) ? "no timeout" : "timeout " + Timeout,
				string.IsNullOrWhiteSpace(Error) ? "no error" : "with error",
				(Inference == null ? "no" : "with") + " inference",
				(this.HasEquifaxData() ? "with" : "no") + " Equifax data",
				(Etl == null ? "no" : "with") + " ETL"
			);
		} // ToShortString

		/// <summary>
		/// Determines whether the specified <see cref="T:System.Object"/>
		/// is equal to the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// true if the specified object  is equal to the current object; otherwise, false.
		/// </returns>
		/// <param name="obj">The object to compare with the current object. </param>
		public override bool Equals(object obj) {
			Reply r = obj as Reply;

			if (r == null)
				return false;

			return
				Status == r.Status;
		} // Equals

		/// <summary>
		/// Serves as a hash function for a particular type. 
		/// </summary>
		/// <returns>
		/// A hash code for the current <see cref="T:System.Object"/>.
		/// </returns>
		public override int GetHashCode() {
			return string.Join("_", Status).GetHashCode();
		} // GetHashCode

		public string Reason { get { return this.HasDecision() ? Inference.Decision.Reason : null; } }
		public string Outcome { get { return this.HasDecision() ? Inference.Decision.Outcome : null; } }

		public ModelOutput GetParsedModel(ModelNames name) {
			if (this.parsedModels.ContainsKey(name))
				return this.parsedModels[name];

			this.parsedModels[name] = this.HasModels()
				? JsonConvert.DeserializeObject<ModelOutput>(Inference.Decision.Models[name])
				: null;

			return this.parsedModels[name];
		} // GetParsedModel

		private readonly SortedDictionary<ModelNames, ModelOutput> parsedModels;
	} // class Reply

	public static class ReplyExt {
		public static bool Exists(this Reply reply) {
			return reply != null;
		} // Exists

		public static bool HasInference(this Reply reply) {
			return reply.Exists() && (reply.Inference != null);
		} // HasInference

		public static bool HasDecision(this Reply reply) {
			return reply.HasInference() && (reply.Inference.Decision != null);
		} // HasDecision

		public static bool HasModels(this Reply reply) {
			return reply.HasDecision() && (reply.Inference.Decision.Models != null);
		} // HasModels

		public static bool HasBucket(this Reply reply) {
			return reply.HasDecision() && (reply.Inference.Decision.Bucket != null);
		} // HasBucket

		public static bool HasEquifaxData(this Reply reply) {
			return reply.Exists() && (reply.Equifax != null) && !string.IsNullOrWhiteSpace(reply.Equifax.RawResponse);
		} // EquifaxData

		public static bool HasEtl(this Reply reply) {
			return reply.Exists() && (reply.Etl != null);
		} // HasEtl
	} // class ReplyExt
} // namespace
