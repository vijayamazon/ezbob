using System;
using System.Web.Mvc;
using GoCardlessSdk;
using GoCardlessSdk.Connect;
using GoCardlessSdk.Partners;

namespace GoCardlessMvcTestClient.Controllers {
	public class HomeController : Controller {
		private readonly string _merchantId = ConfigManager.CurrentValues.Instance.GoCardlessMerchantId;

		[HttpGet]
		public ActionResult Index() {
			GoCardless.Environment = GoCardless.Environments.Sandbox;

			try {
				/*
				ViewBag.NewSubscriptionUrl = GoCardless.Connect.NewSubscriptionUrl(
					new SubscriptionRequest(_merchantId, 10m, 1, "month")
					{
						Name = "Test subscription",
						Description = "Testing a new monthly subscription",
						User = new UserRequest
						{
							BillingAddress1 = "Flat 1",
							BillingAddress2 = "100 Main St",
							BillingTown = "Townville",
							BillingCounty = "Countyshire",
							BillingPostcode = "N1 1AB",
							Email = "john.smith@example.com",
							FirstName = "John",
							LastName = "Smith",
						}
					});

				ViewBag.NewBillUrl = GoCardless.Connect.NewBillUrl(
					new BillRequest(_merchantId, 10m)
					{
						Name = "Test bill",
						Description = "Testing a new bill",
						User = new UserRequest
						{
							BillingAddress1 = "Flat 1",
							BillingAddress2 = "100 Main St",
							BillingTown = "Townville",
							BillingCounty = "Countyshire",
							BillingPostcode = "N1 1AB",
							Email = "john.smith@example.com",
							FirstName = "John",
							LastName = "Smith",
						}
					});
				*/
				ViewBag.NewPreAuthorizationUrl = GoCardless.Connect.NewPreAuthorizationUrl(
					new PreAuthorizationRequest(_merchantId, 1000, 1, "month") {
						ExpiresAt = DateTimeOffset.UtcNow.AddYears(1),
						Name = "Test preauth",
						Description = "Testing a new preauthorization",
						IntervalCount = 12,
						CalendarIntervals = true,
						User = new UserRequest {
							BillingAddress1 = "Flat 1",
							BillingAddress2 = "100 Main St",
							BillingTown = "Townville",
							BillingCounty = "Countyshire",
							BillingPostcode = "N1 1AB",
							Email = "stasd@ezbob.com",
							FirstName = "John",
							LastName = "Smith",
						}
					}, "http://localhost:12345/GoCardless/PreAuthorizationCallback", "http://localhost:12345/GoCardless/PreAuthorizationCancelCallback",
					state: "todo CustomerId");
				/*
				ViewBag.CreateMerchantUrl = GoCardless.Partner.NewMerchantUrl(
					"http://localhost:12345/GoCardless/CreateMerchantCallback",
					new Merchant
					{
						Name = "Mike the Merchant",
						BillingAddress1 = "Flat 1",
						BillingAddress2 = "200 High St",
						BillingTown = "Townville",
						BillingCounty = "Countyshire",
						BillingPostcode = "N1 1AB",
						User = new User
						{
							FirstName = "Mike",
							LastName = "Merchant",
							Email = "mike.merchant@example.com",
						}
					},
					state: "test_state");
				 * */
			}
			catch (ApiException ae) {
				return HandleException(ae);
			}
			catch (Exception ex) {
				ViewBag.Error = ex.Message;
			}

			return View();
		}

		/*
		public ActionResult Merchant()
		{
			GoCardless.Environment = GoCardless.Environments.Sandbox;
			ViewData.Model = GoCardless.Api.GetMerchant(_merchantId);
			return View();
		}
		*/
		[HttpGet]
		public ActionResult Bills() {
			try {
				GoCardless.Environment = GoCardless.Environments.Sandbox;
				ViewData.Model = GoCardless.Api.GetMerchantBills(_merchantId);
				return View();
			}
			catch (ApiException ae) {
				return HandleException(ae);
			}
		}
		/*
		public ActionResult Subscriptions()
		{
			GoCardless.Environment = GoCardless.Environments.Sandbox;
			ViewData.Model = GoCardless.Api.GetMerchantSubscriptions(_merchantId);
			return View();
		}
		*/
		[HttpGet]
		public ActionResult PreAuthorizations() {
			try {
				GoCardless.Environment = GoCardless.Environments.Sandbox;
				ViewData.Model = GoCardless.Api.GetMerchantPreAuthorizations(_merchantId);
				return View();
			}
			catch (ApiException ae) {
				return HandleException(ae);
			}
		}
		/*
		public ActionResult Users()
		{
			GoCardless.Environment = GoCardless.Environments.Sandbox;
			ViewData.Model = GoCardless.Api.GetMerchantUsers(_merchantId);
			return View();
		}
		*/
		[HttpGet]
		public ActionResult Cancel() {
			return View();
		}

		[HttpGet]
		public ActionResult Success() {
			ViewData.Model = TempData["payload"];
			return View();
		}

		[HttpGet]
		public ActionResult Error() {
			return View();
		}

		[HttpGet]
		public ActionResult PostBillRequest() {
			return View();
		}

		[HttpPost]
		public ActionResult PostBill(int amount, string preAuthorizationId) {
			try {
				GoCardless.Environment = GoCardless.Environments.Sandbox;
				ViewData.Model = GoCardless.Api.PostBill(amount, preAuthorizationId, "ezbob", "loan id repayment", DateTime.UtcNow);
				return View();
			}
			catch (ApiException ae) {
				return HandleException(ae);
			}
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
