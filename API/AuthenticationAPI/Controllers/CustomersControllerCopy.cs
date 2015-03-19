namespace Ezbob.API.AuthenticationAPI.Controllers {
	using System;
	using System.Diagnostics;
	using System.Linq;
	using System.Security.Claims;
	using System.Threading.Tasks;
	using System.Web.Http;
	using System.Web.Http.Description;
	using System.Web.Http.Results;
	using Ezbob.API.AuthenticationAPI.Models;
	using Ezbob.Backend.Models.ExternalAPI;
	using Ezbob.Utils.Extensions;
	using Newtonsoft.Json;
	using ServiceClientProxy;

	[RequireHttps] 
	[RoutePrefix("api/customers")]
	public class CustomersControllerCopy : ApiController {

		// partherAppAlibaba | XDroz4HhR1EI2zsvd
		[Route("availableCredit", Name = "GetAbailableCreditPost")]
		[HttpPost]
		[ResponseType(typeof(AlibabaDto))]
		[ValidateModelState]
		//[Authorize(Roles = "PartnerAlibaba")]  [Authorize(Users = "partherAppAlibaba")] [Authorize(Roles="user")]
		[Authorize]
		public async Task<IHttpActionResult> GetAlibabaCustomerAvailableCredit([FromBody]AlibabaDto data) {

			if (!AlibabaPartnerCheck()) {
				return Unauthorized();
			}

			/*var claims = ClaimsPrincipal.Current.Claims;
			string username = "";

			foreach (var c in claims) {
				if (c.Type == "user_name") {
					username = c.Value;
					Trace.Write(string.Format("\n\n\t\t User uname: {0}", c.Value));
				}
			}

			if( username != "partherAppAlibaba" ) {
				return Unauthorized();
			}*/

			try {

				ServiceClient client = new ServiceClient();
				var result = client.Instance.CustomerAvaliableCredit(data.aId, data.aliMemberId).Result;

//{"currency":"GBP","aliMemberId":12345,"aId":18234,"loanId":null,"unusedCreditAmount":null,"unusedCreditAmount_USD":null,"lastUpdate":null,"lineStatus":""}
//{"availableCredit":{"currency":"GBP","aliMemberId":12345,"aId":18234,"loanId":null,"unusedCreditAmount":null,"unusedCreditAmount_USD":null,"lastUpdate":null,"lineStatus":""},"errCode":0,"errMsg":"NO_ERROR","requestId":"000001","responseId":"1000001","aliMemberId":12345,"aId":18234,"loanId":0}

				var response = new AlibabaDto() {
					requestId = data.requestId,
					responseId = data.responseId,
					aId = data.aId,
					aliMemberId = data.aliMemberId,
					availableCredit = result,
					
				};

				////  TODO - put real value
				int originID = 3;

				//// save to db
				var datatosave = new ApiCallData() {
					Request = JsonConvert.SerializeObject(data, Formatting.None),
					RequestId = data.requestId,
					Response = JsonConvert.SerializeObject(response, Formatting.None),
					CustomerID = data.aId,
					OriginID = originID, // put alibaba origin
					StatusCode = "200", //put dynamic
					ErrorCode = response.errCode.DescriptionAttr(),
					ErrorMessage = response.errMsg,
					Comments = "test save API"
				};

				var saveservice = client.Instance.SaveApiCall(datatosave, originID);

				return Ok(response);
			
			} catch (Exception e) {
				//Logger.SafeILog("InternalError during GetAlibabaCustomerAvailableCredit API");
				return InternalServerError(e);
			}
		}

		// https://api.ezbob.com/Alibaba/Service/qualify/{aId}/{aliMemberId}
		[Route("qualify", Name = "RequalifyCustomerPost")]
		[HttpPost]
		//[ResponseType(typeof(AlibabaDto))]
		[ValidateModelState]
		[Authorize]
		public async Task<IHttpActionResult> RequalifyCustomer([FromBody]AlibabaDto data) {

			var claims = ClaimsPrincipal.Current.Claims;
			string username = "";

			foreach (var c in claims) {
				if (c.Type == "user_name") {
					username = c.Value;
					Trace.Write(string.Format("\n\n\t\t User uname: {0}", c.Value));
				}
			}

			if (username != "partherAppAlibaba") {
				return Unauthorized();
			}

			try {

				ServiceClient client = new ServiceClient();
				var result = client.Instance.CustomerAvaliableCredit(data.aId, data.aliMemberId).Result;

				//{"currency":"GBP","aliMemberId":12345,"aId":18234,"loanId":null,"unusedCreditAmount":null,"unusedCreditAmount_USD":null,"lastUpdate":null,"lineStatus":""}
				//{"availableCredit":{"currency":"GBP","aliMemberId":12345,"aId":18234,"loanId":null,"unusedCreditAmount":null,"unusedCreditAmount_USD":null,"lastUpdate":null,"lineStatus":""},"errCode":0,"errMsg":"NO_ERROR","requestId":"000001","responseId":"1000001","aliMemberId":12345,"aId":18234,"loanId":0}

				var response = new AlibabaDto() {
					requestId = data.requestId,
					responseId = data.responseId,
					aId = data.aId,
					aliMemberId = data.aliMemberId,
					availableCredit = result,

				};

				////  TODO - put real value
				int originID = 3;

				//// save to db
				var datatosave = new ApiCallData() {
					Request = JsonConvert.SerializeObject(data, Formatting.None),
					RequestId = data.requestId,
					Response = JsonConvert.SerializeObject(response, Formatting.None),
					CustomerID = data.aId,
					OriginID = originID, // put alibaba origin
					StatusCode = "200", //put dynamic
					ErrorCode = response.errCode.DescriptionAttr(),
					ErrorMessage = response.errMsg,
					Comments = "test save API"
				};

				var saveservice = client.Instance.SaveApiCall(datatosave, originID);

				return Ok(response);

			} catch (Exception e) {
				Console.WriteLine(e);
				return InternalServerError(e);
				//	throw;
			}

			//return Ok(data);
		}

		private bool AlibabaPartnerCheck() {

			var claims = ClaimsPrincipal.Current.Claims;
			string username = "";

			foreach (var c in claims) {
				if (c.Type == "user_name") {
					username = c.Value;
					Trace.Write(string.Format("\n\n\t\t User uname: {0}", c.Value));
				}
			}

			if (username != "partherAppAlibaba") {
				//return Unauthorized();
				return false;
			}

			return true;
		}


		[Route("testuser", Name = "testUser")]
		[HttpGet]
		public async Task<IHttpActionResult> TestUser() {

			var currentPrincipal = ClaimsPrincipal.Current;

			Trace.Write(string.Format("\n\nTestUser currentPrincipal: {0}",
				JsonConvert.SerializeObject(currentPrincipal, Formatting.None, new JsonSerializerSettings {
					Formatting = Formatting.None,
					ReferenceLoopHandling = ReferenceLoopHandling.Ignore
				})));

			return Ok("currentPrincipal:" +
				JsonConvert.SerializeObject(currentPrincipal, Formatting.None, new JsonSerializerSettings {
					Formatting = Formatting.None,
					ReferenceLoopHandling = ReferenceLoopHandling.Ignore
				}));
		}

		[AllowAnonymous]
		[Route("test/{data}", Name = "testGet")]
		[HttpGet]
		public async Task<IHttpActionResult> Testtttt(string data) {
			Console.WriteLine("Hello " + data);
			return Ok("Hello " + data);
		}

	}
}