﻿namespace Ezbob.API.AuthenticationAPI.Controllers {
	using System;
	using System.Diagnostics;
	using System.Net.Http.Headers;
	using System.Security.Claims;
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
	using WebApi.OutputCache.V2;

	[RequireHttps] 
	[RoutePrefix("api/customers")]
	public class CustomersController : ApiController {

		private readonly MediaTypeHeaderValue _responseMediaType = new MediaTypeHeaderValue("application/json");
	
		[Route("availableCredit", Name = "GetAbailableCreditPost")]
		[HttpPost]
		[ResponseType(typeof(AlibabaDto))]
		[ValidateModelState]
		[Authorize(Roles = "PartnerAlibaba")]
		[CacheOutput(ClientTimeSpan = 60, ServerTimeSpan = 60, ExcludeQueryStringFromCacheKey = true)] // client cache length in seconds
		public async Task<IHttpActionResult> GetAlibabaCustomerAvailableCredit([FromBody]AlibabaDto data) {

			var cache = Configuration.CacheOutputConfiguration().GetCacheOutputProvider(Request);
			string cachKey = CacheKeyCustomerAliMember(data);
			var cachedResponse = (AlibabaDto)cache.Get(cachKey);

			if (cachedResponse != null && cachedResponse.aId == data.aId && cachedResponse.aliMemberId == data.aliMemberId) {
				SaveApiLog(data, cachedResponse, "200", "from cache");
				return Ok(cachedResponse);
			}

			try {
				ServiceClient client = new ServiceClient();
				var result = client.Instance.CustomerAvaliableCredit(data.aId, data.aliMemberId).Result;

				string url = ActionContext.Request.RequestUri.ToString();

				var response = new AlibabaDto() {
					requestId = data.requestId,
					responseId = data.responseId,
					aId = data.aId,
					aliMemberId = data.aliMemberId,
					availableCredit = result,
					url = url
				};

				// customerID and aliMemberID doesn't match each other in in system DB
				if (result.aId == null && result.aliMemberId == null) {

					response.errCode = AlibabaErrorCode.SYSTEM_CUSTOMER_ID_ALI_MEMBER_ID_MISMATCH;
					response.errMsg = AlibabaErrorCode.SYSTEM_CUSTOMER_ID_ALI_MEMBER_ID_MISMATCH.DescriptionAttr();
					SaveApiLog(data, response, "400");
					return BadRequest(response.errMsg);
				}

				// customerID not found in system DB
				if (result.aId == null) {

					response.errCode = AlibabaErrorCode.SYSTEM_CUSTOMER_ID_NOT_FOUND;
					response.errMsg = AlibabaErrorCode.SYSTEM_CUSTOMER_ID_NOT_FOUND.DescriptionAttr();
					SaveApiLog(data, response, "400");
					return BadRequest(response.errMsg);
				}

				// ali memberID not found in system DB
				if (result.aliMemberId == null) {

					response.errCode = AlibabaErrorCode.SYSTEM_ALI_MEMBER_ID_NOT_FOUND;
					response.errMsg = AlibabaErrorCode.SYSTEM_ALI_MEMBER_ID_NOT_FOUND.DescriptionAttr();
					SaveApiLog(data, response, "400");
					return BadRequest(response.errMsg);
				}

				if (result.creditLine == null) {
					response.errCode = AlibabaErrorCode.SYSTEM_NO_VALID_CREDITLINE_FOR_CUSTOMER;
					response.errMsg = AlibabaErrorCode.SYSTEM_NO_VALID_CREDITLINE_FOR_CUSTOMER.DescriptionAttr();
				}

				SaveApiLog(data, response, "200");

				// set cache for 1 minute
				cache.Add(cachKey, response, DateTime.Now.AddSeconds(60), null);

				return Ok(response);

			} catch (Exception e) {
				Trace.TraceError(DateTime.UtcNow + ": " + e);
				return InternalServerError();
			}
		}//GetAlibabaCustomerAvailableCredit

		

		/// <summary>
		/// save to DB
		/// </summary>
		/// <param name="request"></param>
		/// <param name="response"></param>
		/// <param name="statusCode"></param>
		/// <param name="comments"></param>
		private void SaveApiLog(AlibabaDto request, AlibabaDto response, string statusCode = null, string comments = "" ) {
	
			StringBuilder req = new StringBuilder(JsonConvert.SerializeObject(request, Helper.JsonReferenceLoopHandling())).Append("; HEADERS: ").Append(JsonConvert.SerializeObject(ActionContext.Request.Headers, Helper.JsonReferenceLoopHandling()));

			try {
				var datatosave = new ApiCallData() {
					Request = req.ToString(), 
					RequestId = request.requestId,
					Response = JsonConvert.SerializeObject(response, Helper.JsonReferenceLoopHandling()),
					CustomerID = request.aId,
					StatusCode = statusCode, 
					ErrorCode = response.errCode.DescriptionAttr(),
					ErrorMessage = response.errMsg,
					Source = ExternalAPISource.Alibaba.DescriptionAttr(),
					Comments = comments ,  
					Url = response.url
				};

				ServiceClient client = new ServiceClient();
				client.Instance.SaveApiCall(datatosave);

			} catch (Exception logex) {
				Trace.TraceError(DateTime.UtcNow + ": " + logex);
			}
		} //SaveApiLog

		// but not https://api.ezbob.com/Alibaba/Service/qualify/{aId}/{aliMemberId}
		[Route("qualify", Name = "RequalifyCustomerPost")]
		[HttpPost]
		[ResponseType(typeof(AlibabaDto))]
		[ValidateModelState]
		[Authorize(Roles = "PartnerAlibaba")]
		//[CacheOutput(ClientTimeSpan = 7200, ServerTimeSpan = 7200, ExcludeQueryStringFromCacheKey = true)]  // 60sec*60min*2hours
		public async Task<IHttpActionResult> RequalifyCustomer([FromBody]AlibabaDto data) {

			var cache = Configuration.CacheOutputConfiguration().GetCacheOutputProvider(Request);
			string cachKey = CacheKeyCustomerAliMember(data);
			var cachedResponse = (AlibabaDto)cache.Get(cachKey);

			if (cachedResponse != null && cachedResponse.aId == data.aId && cachedResponse.aliMemberId == data.aliMemberId) {
				SaveApiLog(data, cachedResponse, "200", "from cache");
				return Ok(cachedResponse);
			}

			try {
				ServiceClient client = new ServiceClient();
				client.Instance.RequalifyCustomer(data.aId, data.aliMemberId);
				string url = ActionContext.Request.RequestUri.ToString();

				var response = new AlibabaDto() {
					requestId = data.requestId,
					responseId = data.responseId,
					aId = data.aId,
					aliMemberId = data.aliMemberId,
					errCode = AlibabaErrorCode.REQUALIFY_STARTED,
					errMsg = AlibabaErrorCode.REQUALIFY_STARTED.DescriptionAttr(),
					url = url
				};

				SaveApiLog(data, response, "200");

				// set cache for 1 minute
				cache.Add(cachKey, response, DateTime.Now.AddHours(2), null);

				return Ok(response);

			} catch (Exception e) {
				Trace.TraceError(DateTime.UtcNow + ": " + e);
				return InternalServerError();
			}
		}//RequalifyCustomer


		[Route("testuser", Name = "testUser")]
		[HttpGet]
		public async Task<IHttpActionResult> TestUser() {
			var currentPrincipal = ClaimsPrincipal.Current;
			return Ok("currentPrincipal:" + JsonConvert.SerializeObject(currentPrincipal, Helper.JsonReferenceLoopHandling()));
		}//TestUser

		[AllowAnonymous]
		[Route("test/{data}", Name = "testGet")]
		[HttpGet]
		public async Task<IHttpActionResult> Testtttt(string data) {
			return Ok("Hello " + data);
		}//Testtttt

		private string CacheKeyCustomerAliMember(AlibabaDto data) {
			StringBuilder sb = new StringBuilder(ActionContext.ControllerContext.ControllerDescriptor.ControllerName).Append(Helper.CACHE_KEY_SEPARATOR).Append(ActionContext.ActionDescriptor.ActionName).Append(Helper.CACHE_KEY_SEPARATOR);
			sb.Append("aId").Append(data.aId).Append(Helper.CACHE_KEY_SEPARATOR);
			sb.Append("aliMemberId").Append(data.aliMemberId).Append(Helper.CACHE_KEY_SEPARATOR);
			sb.Append(":").Append(this._responseMediaType);
			return sb.ToString();
		} //CacheKeyCustomerAliMember
	}
}