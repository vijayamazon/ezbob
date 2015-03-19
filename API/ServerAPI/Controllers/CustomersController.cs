namespace Ezbob.API.ServerAPI.Controllers {
	using System;
	using System.Diagnostics;
	using System.Net.Http;
	using System.Security.Claims;
	using System.Threading.Tasks;
	using System.Web.Http;
	using System.Web.Http.Description;
	using Ezbob.API.ServerAPI.Attributes;
	using Ezbob.API.ServerAPI.Models;
	using Ezbob.Utils;
	using Microsoft.Owin.Security;
	using Newtonsoft.Json;

	[RequireHttps] 
	[RoutePrefix("api/customers")]
	public class CustomersController : ApiController {

		// partherAppAlibaba | XDroz4HhR1EI2zsvd
		[Route("availableCredit", Name = "GetAbailableCreditPost")]
		[HttpPost]
		[ResponseType(typeof(AlibabaDto))]
		[ValidateModelState]
	//	[Authorize(Roles = "PartnerAlibaba")]
		// [Authorize(Users = "partherAppAlibaba")]
		//	[Authorize(Roles="user")]
		//	[Authorize]
		//[ClaimsAuthorize]
		public async Task<IHttpActionResult> GetAlibabaCustomerAvailableCredit([FromBody]AlibabaDto data) {

		//	Console.WriteLine("data: {1}, {2}, {3}, {0}", data.aId, data.aliMemberId, data.requestId, data.responseId);
			return Ok(data);
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