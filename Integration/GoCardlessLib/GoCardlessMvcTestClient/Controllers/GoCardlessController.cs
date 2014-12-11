using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using GoCardlessSdk;
using GoCardlessSdk.Api;
using GoCardlessSdk.Connect;
using GoCardlessSdk.WebHooks;

namespace GoCardlessMvcTestClient.Controllers {
	using System;
	using System.Collections.Specialized;
	using Models;

	public class GoCardlessController : Controller {
		[HttpPost]
		public ContentResult Index() {
			var requestContent = new StreamReader(Request.InputStream).ReadToEnd();
			var payload = WebHooksClient.ParseRequest(requestContent);
			// TODO: store request payload.

			// respond with status HTTP/1.1 200 OK within 5 seconds.
			// If the API server does not get a 200 OK response within this time,
			// it will retry up to 10 times at ever-increasing time intervals.
			// If you have time-consuming server-side processes that are triggered by a webhook,
			// e.g. email scripts, please consider processing them asynchronously.
			return Content("");
		}

		[HttpGet]
		public ActionResult ConfirmResource() {
			var requestContent = Request.QueryString;
			var resource = GoCardless.Connect.ConfirmResource(requestContent);

			// TODO: store request payload.
			TempData["payload"] = resource;

			return RedirectToAction("Success", "Home");
		}
		/*
        [HttpGet]
        public ActionResult CreateMerchantCallback(string code, string state)
        {
            var merchantResponse = GoCardless.Partner.ParseCreateMerchantResponse(
                "http://localhost:12345/GoCardless/CreateMerchantCallback", code);

            // TODO: store response
            TempData["payload"] = merchantResponse;
            
            // use ApiClient to make calls on behalf of merchant
            var merchantApiClient = new ApiClient(merchantResponse.AccessToken);

            return RedirectToAction("Success", "Home");
        }
		*/

		[HttpGet]
		public ActionResult PreAuthorizationCallback(string resource_id, string resource_type, string resource_uri, string signature, string state) {
			try {
				var data = new PreAuthorizationModel {
					resource_id = resource_id,
					resource_type = resource_type,
					resource_uri = resource_uri,
					signature = signature,
					state = state
				};
				var request = new NameValueCollection();
				request["resource_id"] = resource_id;
				request["resource_type"] = resource_type;
				request["resource_uri"] = resource_uri;
				request["signature"] = signature;
				request["state"] = state;
				var resource = GoCardless.Connect.ConfirmResource(request);
				data.ConfirmResource = resource;
				ViewData.Model = data;
				return View();
			} catch (ApiException ae) {
				return HandleException(ae);
			}
		}

		[HttpGet]
		public ActionResult PreAuthorizationCancelCallback(string state) {
			return View();
		}

		[NonAction]
		private ActionResult HandleException(ApiException exception) {
			if (TempData.ContainsKey("Exception")) {
				TempData["Exception"] = exception;
			}
			else {
				TempData.Add("Exception", exception);
			}

			return RedirectToAction("Error", "Home");
		}
		
	}
}
