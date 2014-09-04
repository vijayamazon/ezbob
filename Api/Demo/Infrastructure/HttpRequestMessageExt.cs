namespace Demo.Infrastructure {
	using System.Net.Http;

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
	} // class HttpRequestMessageExt
} // namespace
