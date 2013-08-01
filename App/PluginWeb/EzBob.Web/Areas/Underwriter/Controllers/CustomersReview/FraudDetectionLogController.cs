using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EZBob.DatabaseLib.Model.Fraud;
using EZBob.DatabaseLib.Repository;
using EzBob.Web.Areas.Underwriter.Models;
using EzBob.Web.Infrastructure.csrf;
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
        [Transactional]
        [ValidateJsonAntiForgeryToken]
        public JsonNetResult Index(int id)
        {
            IEnumerable<FraudDetection> fraudDetectionHistory = _fraudDetectionLog.GetByCustomerId(id);
            var models = new List<FraudDetectionLogModel>();

            models.AddRange(fraudDetectionHistory.Select(val => new FraudDetectionLogModel
                {
                    Id = val.Id,
                    CompareField = val.CompareField,
                    CurrentField = val.CurrentField,
                    Value = val.Value,
                    Concurrence = ConcurrencePrepare(val),
                    Type = val.ExternalUser != null ? "External" : "Internal"
                }));

            models = new List<FraudDetectionLogModel>(models.OrderByDescending(x => x.Id));
            return this.JsonNet(models);
        }

        private static string ConcurrencePrepare(FraudDetection val)
        {
            if (val.ExternalUser != null)
            {
                return string.Format("{0} {1} (id={2})", 
                    val.ExternalUser.FirstName, 
                    val.ExternalUser.LastName,
                    val.ExternalUser.Id);
            }

            var fullname = val.InternalCustomer.PersonalInfo!=null ? val.InternalCustomer.PersonalInfo.Fullname : "-";
            return string.Format("{0} (id={1})",
                    fullname,
                    val.InternalCustomer.Id);
        }
    }
}