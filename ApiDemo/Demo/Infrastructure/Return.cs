namespace Demo.Infrastructure {
	using System.Net;
	using System.Net.Http;
	using System.Web.Http;
	using Filters;

	internal static class Return {
		public static HttpResponseException Success(int nApiVersion, string sFormat = "", params object[] args) {
			string sMsg = string.IsNullOrWhiteSpace(sFormat) ? "Success." : string.Format(sFormat, args);
			return Status(nApiVersion, HttpStatusCode.OK, sMsg);
		} // Success

		public static HttpResponseException Error(int nApiVersion, string sFormat = "", params object[] args) {
			string sMsg = string.IsNullOrWhiteSpace(sFormat) ? "Internal server error." : string.Format(sFormat, args);
			return Status(nApiVersion, HttpStatusCode.InternalServerError, sMsg);
		} // Error

		public static HttpResponseException NotFound(int nApiVersion, string sFormat = "", params object[] args) {
			string sMsg = string.IsNullOrWhiteSpace(sFormat) ? "Not found." : string.Format(sFormat, args);
			return Status(nApiVersion, HttpStatusCode.NotFound, sMsg);
		} // NotFound

		public static HttpResponseException Status(int nApiVersion, HttpStatusCode nCode, string sFormat = "", params object[] args) {
			string sMsg = string.IsNullOrWhiteSpace(sFormat) ? nCode.ToString() : string.Format(sFormat, args);

			var oResponse = new HttpResponseMessage {
				StatusCode = nCode,
				ReasonPhrase = sMsg,
			};

			HandleActionExecutedAttribute.FillResponse(nApiVersion, oResponse);

			return new HttpResponseException(oResponse);
		} // Error
	} // class Return
} // namespace
