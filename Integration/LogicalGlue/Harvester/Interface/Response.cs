namespace Ezbob.Integration.LogicalGlue.Harvester.Interface {
	using Ezbob.Utils.Lingvo;
	using Newtonsoft.Json;

	public class Response<T> : IConvertableToShortString where T: class, IConvertableToShortString {
		public Response() {} // constructor

		public Response(string rawReply) {
			RawReply = rawReply;
			Parsed = JsonConvert.DeserializeObject<T>(RawReply);
		} // constructor

		public string RawReply { get; set; }
		public T Parsed { get; set; }

		public string ToShortString() {
			string raw = string.IsNullOrWhiteSpace(RawReply)
				? "Empty raw reply"
				: "Raw reply of " + Grammar.Number(RawReply.Length, "character");

			return string.Format(
				"{0}, {1}.",
				raw,
				Parsed == null ? "not parsed" : "parsed to " + Parsed.ToShortString()
			);
		} // ToShortString
	} // class Response
} // namespace
