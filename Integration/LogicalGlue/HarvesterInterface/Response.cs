namespace Ezbob.Integration.LogicalGlue.HarvesterInterface {
	using Newtonsoft.Json;

	public class Response<T> where T: class {
		public Response() {} // constructor

		public Response(string rawReply) {
			RawReply = rawReply;
			Parsed = JsonConvert.DeserializeObject<T>(RawReply);
		} // constructor

		public string RawReply { get; set; }
		public T Parsed { get; set; }
	} // class Response
} // namespace
