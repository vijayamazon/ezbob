namespace Ezbob.API.AuthenticationAPI.Controllers {
	using System;
	using System.Diagnostics;
	using System.Security.Claims;
	using System.ServiceModel.Channels;
	using System.Text;
	using System.Threading.Tasks;
	using System.Web.Http;
	using System.Web.Http.Description;
	using Ezbob.API.AuthenticationAPI.Models;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Models.ExternalAPI;
	using Ezbob.Utils.Extensions;
	using Newtonsoft.Json;
	using ServiceClientProxy;

	[RequireHttps] 
	[RoutePrefix("api/customers")]
	public class CustomersController : ApiController {

		// partherAppAlibaba | XDroz4HhR1EI2zsvd
		[Route("availableCredit", Name = "GetAbailableCreditPost")]
		[HttpPost]
		[ResponseType(typeof(AlibabaDto))]
		[ValidateModelState]
		[Authorize]
		public async Task<IHttpActionResult> GetAlibabaCustomerAvailableCredit([FromBody]AlibabaDto data) {

			if (!IsAlibabaPartner()) {
				this.SaveApiLog(data, new AlibabaDto(){ errCode = AlibabaErrorCode.Unauthorized, errMsg = AlibabaErrorCode.Unauthorized.DescriptionAttr()}, "401");
				return Unauthorized();
			}

			try {

				ServiceClient client = new ServiceClient();
				var result = client.Instance.CustomerAvaliableCredit(data.aId, data.aliMemberId).Result;

				var response = new AlibabaDto() {
					requestId = data.requestId,
					responseId = data.responseId,
					aId = data.aId,
					aliMemberId = data.aliMemberId,
					availableCredit = result,
				};

				// customerID and aliMemberID doesn't match each other in in system DB
				if (result.aId == null && result.aliMemberId == null) {

					response.errCode = AlibabaErrorCode.SYSTEM_CUSTOMER_ID_ALI_MEMBER_ID_MISMATCH;
					response.errMsg = AlibabaErrorCode.SYSTEM_CUSTOMER_ID_ALI_MEMBER_ID_MISMATCH.DescriptionAttr();

					this.SaveApiLog(data, response, "400");

					return BadRequest(response.errMsg);
				}

				// customerID not found in system DB
				if (result.aId == null) {

					response.errCode = AlibabaErrorCode.SYSTEM_CUSTOMER_ID_NOT_FOUND;
					response.errMsg = AlibabaErrorCode.SYSTEM_CUSTOMER_ID_NOT_FOUND.DescriptionAttr();

					this.SaveApiLog(data, response, "400");

					return BadRequest(response.errMsg);
				}

				// ali memberID not found in system DB
				if (result.aliMemberId == null) {

					response.errCode = AlibabaErrorCode.SYSTEM_ALI_MEMBER_ID_NOT_FOUND;
					response.errMsg = AlibabaErrorCode.SYSTEM_ALI_MEMBER_ID_NOT_FOUND.DescriptionAttr();

					this.SaveApiLog(data, response, "400");

					return BadRequest(response.errMsg);
				}

				if (result.creditLine == null) {
					response.errCode = AlibabaErrorCode.SYSTEM_NO_VALID_CREDITLINE_FOR_CUSTOMER;
					response.errMsg = AlibabaErrorCode.SYSTEM_NO_VALID_CREDITLINE_FOR_CUSTOMER.DescriptionAttr();
				}

				this.SaveApiLog(data, response, "200");

				return Ok(response);
			
				// ReSharper disable once CatchAllClause
			} catch (Exception e) {
				//Logger.SafeILog("InternalError during GetAlibabaCustomerAvailableCredit API");
				return InternalServerError(e);
			}
		}

		

		/// <summary>
		/// save to DB
		/// </summary>
		/// <param name="request"></param>
		/// <param name="response"></param>
		/// <param name="statusCode"></param>
		/// <param name="comments"></param>
		private void SaveApiLog(AlibabaDto request, AlibabaDto response, string statusCode = null, string comments = "" ) {

			//Trace.WriteLine(string.Format("SYSTEM_ALI_MEMBER_ID_NOT_FOUND: {0}", JsonConvert.SerializeObject(base.ActionContext.Request.Headers, Helper.JsonReferenceLoopHandling()))); 

			string url = base.ActionContext.Request.RequestUri.ToString();
			StringBuilder sb = new StringBuilder(JsonConvert.SerializeObject(request, Helper.JsonReferenceLoopHandling())).Append("; HEADERS: ").Append(JsonConvert.SerializeObject(base.ActionContext.Request.Headers, Helper.JsonReferenceLoopHandling()));

			try {
				var datatosave = new ApiCallData() {
					Request = sb.ToString(), 
					RequestId = request.requestId,
					Response = JsonConvert.SerializeObject(response, Helper.JsonReferenceLoopHandling()),
					CustomerID = request.aId,
					StatusCode = statusCode, 
					ErrorCode = response.errCode.DescriptionAttr(),
					ErrorMessage = response.errMsg,
					Source = ExternalAPISource.Alibaba.DescriptionAttr(),
					Comments = comments ,// "test save API"
					Url = url
				};

				ServiceClient client = new ServiceClient();
				client.Instance.SaveApiCall(datatosave);

			} catch {
				// TODO log exception
				throw;
			}
		}

		// https://api.ezbob.com/Alibaba/Service/qualify/{aId}/{aliMemberId}
		[Route("qualify", Name = "RequalifyCustomerPost")]
		[HttpPost]
		[ResponseType(typeof(AlibabaDto))]
		[ValidateModelState]
		[Authorize]
		public async Task<IHttpActionResult> RequalifyCustomer([FromBody]AlibabaDto data) {

			if (!IsAlibabaPartner()) {
				this.SaveApiLog(data, new AlibabaDto() { errCode = AlibabaErrorCode.Unauthorized, errMsg = AlibabaErrorCode.Unauthorized.DescriptionAttr() }, "401");
				return Unauthorized();
			}

			try {
				ServiceClient client = new ServiceClient();
				client.Instance.RequalifyCustomer(data.aId, data.aliMemberId);

				var response = new AlibabaDto() {
					requestId = data.requestId,
					responseId = data.responseId,
					aId = data.aId,
					aliMemberId = data.aliMemberId,
					errCode = AlibabaErrorCode.REQUALIFY_STARTED,
					errMsg = AlibabaErrorCode.REQUALIFY_STARTED.DescriptionAttr()
				};

				this.SaveApiLog(data, response, "200");

				return Ok(response);

				// ReSharper disable once CatchAllClause
			} catch (Exception e) {
				//Logger.SafeILog("InternalError during GetAlibabaCustomerAvailableCredit API");
				return InternalServerError(e);
			}
		}

		private bool IsAlibabaPartner() {
			var claims = ClaimsPrincipal.Current.Claims;
			string username = "";
			foreach (var c in claims) {
				if (c.Type == "user_name") {
					username = c.Value;
				}
			}
			if (username != "partherAppAlibaba") 
				return false;
			return true;
		}


		[Route("testuser", Name = "testUser")]
		[HttpGet]
		public async Task<IHttpActionResult> TestUser() {
			var currentPrincipal = ClaimsPrincipal.Current;
			return Ok("currentPrincipal:" + JsonConvert.SerializeObject(currentPrincipal, Helper.JsonReferenceLoopHandling()));
		}

		[AllowAnonymous]
		[Route("test/{data}", Name = "testGet")]
		[HttpGet]
		public async Task<IHttpActionResult> Testtttt(string data) {
			return Ok("Hello " + data);
		}
			

	}
}