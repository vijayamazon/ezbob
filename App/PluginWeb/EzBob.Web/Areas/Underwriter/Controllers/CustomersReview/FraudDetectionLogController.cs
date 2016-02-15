namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview {
	using System.Linq;
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Repository;
	using Models;
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.Models;
	using Ezbob.Utils.Extensions;
	using EzBob.Web.Areas.Underwriter.Models.Fraud;
	using EzBob.Web.Infrastructure;
	using EzBob.Web.Infrastructure.Attributes;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Model.Fraud;
	using Newtonsoft.Json;
	using ServiceClientProxy;

	public class FraudDetectionLogController : Controller {
		private readonly FraudDetectionRepository fraudDetectionLog;
		private readonly ServiceClient serviceClient;
		private readonly FraudIovationRepository iovationRepository;
		private readonly ICustomerRepository customerRepository;
		public FraudDetectionLogController(
			FraudDetectionRepository fraudDetectionLog,
			FraudIovationRepository iovationRepository, 
			ICustomerRepository customerRepository) {

			this.fraudDetectionLog = fraudDetectionLog;
			this.serviceClient = new ServiceClient();
			this.iovationRepository = iovationRepository;
			this.customerRepository = customerRepository;
		}//ctor

		[Ajax]
		[HttpGet]
		public JsonResult Index(int id) {
			DateTime? lastCheckDate;
			string customerRefNum;
			var rows = this.fraudDetectionLog
				.GetLastDetections(id, out lastCheckDate, out customerRefNum)
				.Select(x => new FraudDetectionLogRowModel(x))
				.OrderByDescending(x => x.Id)
				.ToList();

			var model = new FraudDetectionLogModel {
				FraudDetectionLogRows = rows,
				LastCheckDate = lastCheckDate,
				CustomerRefNumber = customerRefNum
			};
			return Json(model, JsonRequestBehavior.AllowGet);
		}//Index

		[Ajax]
		[HttpPost]
		[Permission(Name = "RecheckFraud")]
		public JsonResult Recheck(int customerId) {
			var user = this.customerRepository.Get(customerId);
			this.serviceClient.Instance.FraudChecker(user.Id, FraudMode.FullCheck);
			return Json(new { success = true });
		}//Recheck

		[Ajax]
		[HttpGet]
		public JsonResult IovationDetails(int id) {
			var iovation = this.iovationRepository.Get(id);
			if (iovation == null) {
				throw new Exception("Not found");
			}//if

			var model = new IovationDetailsModel {
				Id = iovation.FraudIovationID,
				Created = iovation.Created,
				Origin = iovation.Origin,
				Result = iovation.Result.DescriptionAttr(),
				Reason = iovation.Reason,
				TrackingNumber = iovation.TrackingNumber,
			};
			if (!string.IsNullOrEmpty(iovation.Details)) {
				try {
					model.Details = JsonConvert.DeserializeObject<List<IovationDetailModel>>(iovation.Details);
					model.DetailsNamesToDescription(model.Details);
				} catch (Exception) {
					//log
				}//try
			}//if

			return Json(model, JsonRequestBehavior.AllowGet);
		}//IovationDetails
	}//class FraudDetectionLogController
}//ns