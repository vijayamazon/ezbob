namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview {
    using System.Linq;
    using System.Web.Mvc;
    using EZBob.DatabaseLib.Model.Database.UserManagement;
    using EZBob.DatabaseLib.Repository;
    using Models;
    using System;
    using System.Collections.Generic;
    using Aspose.Words.Lists;
    using Ezbob.Backend.Models;
    using Ezbob.Utils.Extensions;
    using EzBob.Web.Areas.Underwriter.Models.Fraud;
    using EZBob.DatabaseLib.Model.Fraud;
    using Infrastructure.Attributes;
    using Newtonsoft.Json;
    using ServiceClientProxy;

    public class FraudDetectionLogController : Controller {
        private readonly FraudDetectionRepository _fraudDetectionLog;
        private readonly ServiceClient m_oServiceClient;
        private readonly IUsersRepository _usersRepository;
        private readonly FraudIovationRepository iovationRepository;
        public FraudDetectionLogController(FraudDetectionRepository fraudDetectionLog, IUsersRepository usersRepository, FraudIovationRepository iovationRepository) {
            _fraudDetectionLog = fraudDetectionLog;
            m_oServiceClient = new ServiceClient();
            _usersRepository = usersRepository;
            this.iovationRepository = iovationRepository;
        }

        [Ajax]
        [HttpGet]
        public JsonResult Index(int id) {
            DateTime? lastCheckDate;
            string customerRefNum;
            var rows = _fraudDetectionLog
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
        }

        [Ajax]
        [HttpPost]
        public JsonResult Recheck(int customerId) {
            var user = _usersRepository.Get(customerId);
            m_oServiceClient.Instance.FraudChecker(user.Id, FraudMode.FullCheck);
            return Json(new { success = true });
        }

        [Ajax]
        [HttpGet]
        public JsonResult IovationDetails(int id) {
            var iovation = this.iovationRepository.Get(id);
            if (iovation == null) {
                throw new Exception("Not found");
            }

            IovationDetailsModel model = new IovationDetailsModel {
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
                } catch (Exception ex) {
                    //log
                }
            }

            return Json(model, JsonRequestBehavior.AllowGet);
        }
    }
}