namespace EzbobAPI.Extentions {
	using System.Net;
	using System.Web;

	public static class HttpExt {
		public static bool ContainsAuthorizationHeader(this HttpRequest request) {
			if (!string.IsNullOrWhiteSpace(request.Headers["Authorization"])) {
				if (request.Headers["Authorization"].ToLower()
					.StartsWith("bearer "))
					return true;
			}
			return false;
		}

		public static void SendUnauthorizedResponse(this HttpContext context) {
			context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
			context.Response.StatusDescription = "Access Denied";
			context.Response.Write("401 Access Denied");
			context.Response.End();
		}

		public static string ExtractSamlTokenFromAuthorizationHeader(this HttpRequest request) {
			string token;

			string authHeader = request.Headers["Authorization"];

			if (string.IsNullOrEmpty(authHeader))
				token = string.Empty;
			else {
				const string header = "bearer ";

				if (string.CompareOrdinal(authHeader, 0, header, 0, header.Length) == 0)
					token = authHeader.Remove(0, header.Length);
				else
					token = string.Empty;
			}
			return token;
		}
	}
}
