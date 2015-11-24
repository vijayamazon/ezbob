namespace Ezbob.API.AuthenticationAPI {
	using System;
	using System.Diagnostics;
	using System.Net.Http.Headers;
	using System.Security.Cryptography;
	using System.Text;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Models.ExternalAPI;
	using Ezbob.Database;
	using Ezbob.Utils.Extensions;
	using Newtonsoft.Json;
	using ServiceClientProxy;

	public static class Helper {

		public static string CACHE_KEY_SEPARATOR = "-";

		public static string GetHash(string input) {
			HashAlgorithm hashAlgorithm = new SHA256CryptoServiceProvider();

			byte[] byteValue = Encoding.UTF8.GetBytes(input);

			byte[] byteHash = hashAlgorithm.ComputeHash(byteValue);

			return Convert.ToBase64String(byteHash);
		}

		public static JsonSerializerSettings JsonReferenceLoopHandling() {

			return new JsonSerializerSettings { Formatting = Formatting.None, ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
		}

		/// <summary>
		/// save to DB
		/// </summary>
		public static void SaveApiLog<Req, Res>(Req request, Res response, string reqId, string custRefNum, string statusCode = null, string comments = "", string errorCode = "", string errorMsg = "", string url = "", HttpRequestHeaders headers = null) {
			StringBuilder req = new StringBuilder(JsonConvert.SerializeObject(request, Helper.JsonReferenceLoopHandling())).Append("; HEADERS: ").Append(JsonConvert.SerializeObject(headers, Helper.JsonReferenceLoopHandling()));

			try {
				var datatosave = new ApiCallData() {
					Request = req.ToString(),
					RequestId = reqId,
					Response = JsonConvert.SerializeObject(response, Helper.JsonReferenceLoopHandling()),
					CustomerRefNum = custRefNum,
					StatusCode = statusCode,
					ErrorCode = errorCode,
					ErrorMessage = errorMsg,
					Source = ExternalAPISource.Alibaba.DescriptionAttr(),
					Comments = comments,
					Url = url
				};

				ServiceClient client = new ServiceClient();
				client.Instance.SaveApiCall(datatosave);

			} catch (Exception logex) {
				Trace.TraceError(DateTime.UtcNow + ": " + logex);
			}
		} //SaveApiLog
	}
}