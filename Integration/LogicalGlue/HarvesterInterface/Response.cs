namespace Ezbob.Integration.LogicalGlue.HarvesterInterface {
	using Newtonsoft.Json;

	public class Response {
		public Response() {} // constructor

		public Response(string rawReply) {
			RawReply = rawReply;
			Reply = JsonConvert.DeserializeObject<Reply>(RawReply);
		} // constructor

		public string RawReply { get; set; }
		public Reply Reply { get; set; }
	} // class Response
} // namespace
