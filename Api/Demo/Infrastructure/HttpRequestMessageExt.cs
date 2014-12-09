namespace Demo.Infrastructure {
	using System.Net.Http;
	using System.ServiceModel.Channels;
	using System.Web;

	internal static class HttpRequestMessageExt {
		public static void SetUserName(this HttpRequestMessage oRequest, string sUserName) {
			if (oRequest == null)
				return;

			oRequest.Properties[Const.Data.UserName] = sUserName ?? string.Empty;
		} // SetUserName

		public static string GetUserName(this HttpRequestMessage oRequest) {
			if (oRequest == null)
				return string.Empty;

			if (oRequest.Properties.ContainsKey(Const.Data.UserName))
				return (oRequest.Properties[Const.Data.UserName] ?? string.Empty).ToString();

			return string.Empty;
		} // GetUserName

		public static string GetRemoteIp(this HttpRequestMessage request) {
			if (request == null)
				return "UNKNOWN REMOTE HOST (request is null)";

			if (request.Properties.ContainsKey("MS_HttpContext"))
				return ((HttpContextWrapper)request.Properties["MS_HttpContext"]).Request.UserHostAddress;

			if (request.Properties.ContainsKey(RemoteEndpointMessageProperty.Name))
				return ((RemoteEndpointMessageProperty)request.Properties[RemoteEndpointMessageProperty.Name]).Address;

			return "UNKNOWN REMOTE HOST";
		} // GetRemoteIp

	} // class HttpRequestMessageExt
} // namespace
