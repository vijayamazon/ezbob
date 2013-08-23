﻿using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EZBob.DatabaseLib.Repository;
using EzBob.Web.Areas.Underwriter.Models;
using Scorto.Web;

namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
    public class FraudDetectionLogController : Controller
    {
        private readonly FraudDetectionRepository _fraudDetectionLog;

        public FraudDetectionLogController(FraudDetectionRepository fraudDetectionLog)
        {
            _fraudDetectionLog = fraudDetectionLog;
        }

        [Ajax]
        [HttpGet]
        public JsonNetResult Index(int id)
        {
            var models = new List<FraudDetectionLogModel>(_fraudDetectionLog.GetLastDetections(id).Select(x => new FraudDetectionLogModel(x)));
            models = new List<FraudDetectionLogModel>(models.OrderByDescending(x => x.Id));
            return this.JsonNet(models);
        }
    }
}