using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using EZBob.DatabaseLib.Model.Fraud;
using EZBob.DatabaseLib.Repository;
using EzBob.Web.Areas.Underwriter.Models;
using EzBob.Web.Code;
using EzBob.Web.Infrastructure.csrf;
using NHibernate.Linq;
using Scorto.Web;
using StructureMap;

namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
    public class FraudDetectionLogController : Controller
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly FraudDetectionRepository _fraudDetectionLog;


        //https://localhost:44300/Underwriter/FraudDetectionLog/Index/2093?_=1366894550231

        public FraudDetectionLogController(ICustomerRepository customerRepository, FraudDetectionRepository fraudDetectionLog)
        {
            _customerRepository = customerRepository;
            _fraudDetectionLog = fraudDetectionLog;
        }

        [Ajax]
        [HttpGet]
        [Transactional]
        [ValidateJsonAntiForgeryToken]
        public JsonNetResult Index(int id)
        {
            var fraudDetectionHistory = _fraudDetectionLog.GetByCustomerId(id);
            var models = new List<FraudDetectionLogModel>();

            models.AddRange(fraudDetectionHistory.Select(val => new FraudDetectionLogModel
            {
                Id = val.Id,
                CompareField = val.CompareField,
                CurrentField = val.CurrentField,
                Value = val.Value,
                Concurrence = (val.ExternalUser == null ? "" : "External User (" + val.ExternalUser.Id.ToString() + ")") +
                            (val.InternalCustomer == null ? "" : "<p>Internal User (" + val.InternalCustomer.Id.ToString() + ") </p><p>" + val.InternalCustomer.PersonalInfo.Fullname + "</p>")
            }));

            models = new List<FraudDetectionLogModel>(models.OrderByDescending(x => x.Id));
            return this.JsonNet(models);
        }

    }
}
