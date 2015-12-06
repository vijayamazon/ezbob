namespace Ezbob.Integration.LogicalGlue.Harvester.Interface {
	using System;
	using System.Net;
	using Ezbob.Integration.LogicalGlue.Exceptions.Harvester;
	using Ezbob.Utils.Lingvo;
	using Newtonsoft.Json;

	public class Response<T> : IConvertableToShortString where T: class, IConvertableToShortString {
		public Response(HttpStatusCode status, string rawReply) {
			ParsingException = null;

			Status = status;
			RawReply = rawReply;

			if (string.IsNullOrWhiteSpace(RawReply)) {
				ParsingException = new HarvesterWarning(null, "Empty response received.");
				Parsed = null;
			} else {
				try {
					Parsed = JsonConvert.DeserializeObject<T>(RawReply);
				} catch (Exception e) {
					ParsingException = e;
					Parsed = null;
				} // try
			} // if
		} // constructor

		public HttpStatusCode Status { get; private set; }
		public string RawReply { get; private set; }
		public T Parsed { get; private set; }
		public Exception ParsingException { get; private set; }

		public string ToShortString() {
			string raw = string.IsNullOrWhiteSpace(RawReply)
				? "Empty raw reply"
				: "Raw reply of " + Grammar.Number(RawReply.Length, "character");

			string exception = ParsingException == null
				? string.Empty
				: string.Format(" (with exception '{0}': '{1}')", ParsingException.GetType(), ParsingException.Message);

			return string.Format(
				"{0}{1}, {2}.",
				raw,
				exception,
				Parsed == null ? "not parsed" : "parsed to " + Parsed.ToShortString()
			);
		} // ToShortString
	} // class Response
} // namespace
