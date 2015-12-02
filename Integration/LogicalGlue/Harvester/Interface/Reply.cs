namespace Ezbob.Integration.LogicalGlue.Harvester.Interface {
	using System.Net;
	using Ezbob.Utils.Extensions;
	using Newtonsoft.Json;

	public class Reply : IConvertableToShortString {
		[JsonProperty(PropertyName = "status")]
		public HttpStatusCode Status { get; set; }

		[JsonProperty(PropertyName = "timeout", NullValueHandling = NullValueHandling.Ignore)]
		public TimeoutReasons? Timeout { get; set; }

		[JsonProperty(PropertyName = "error", NullValueHandling = NullValueHandling.Ignore)]
		public string Error { get; set; }

		[JsonProperty(PropertyName = "logicalglue", NullValueHandling = NullValueHandling.Ignore)]
		public InferenceOutput Inference { get; set; }

		[JsonProperty(PropertyName = "equifax", NullValueHandling = NullValueHandling.Ignore)]
		public string EquifaxData { get; set; }

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
				Timeout == null ? "no timeout" : "timeout " + Timeout.Value,
				string.IsNullOrWhiteSpace(Error) ? "no error" : "with error",
				(Inference == null ? "no" : "with") + " inference",
				(string.IsNullOrWhiteSpace(EquifaxData) ? "no" : "with") + " Equifax data",
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
	} // class Reply

	public static class ReplyExt {
		public static bool Exists(this Reply reply) {
			return reply != null;
		} // Exists

		public static bool HasInference(this Reply reply) {
			return reply.Exists() && (reply.Inference != null);
		} // HasInference

		public static bool HasEquifaxData(this Reply reply) {
			return reply.Exists() && !string.IsNullOrWhiteSpace(reply.EquifaxData);
		} // EquifaxData
	} // class ReplyExt
} // namespace
