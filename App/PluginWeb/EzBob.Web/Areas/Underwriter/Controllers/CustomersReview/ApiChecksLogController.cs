﻿using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using EZBob.DatabaseLib.Repository;
using EzBob.Web.Areas.Underwriter.Models;
using EzBob.Web.Infrastructure.csrf;
using Scorto.Web;
using StructureMap;

namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
    public class ApiChecksLogController : Controller
    {
        private readonly PostcodeServiceLogRepository _postcodeServiceLog;
        private readonly CustomerMarketPlaceUpdatingHistoryRepository _mpCustomerMarketPlaceUpdatingHistory;
        private readonly ICustomerRepository _customerRepository;
        private readonly ServiceLogRepository _eseriasLogRepository;
        private readonly PacnetPaypointServiceLogRepository _pacnetLogRepository;

        public ApiChecksLogController(PostcodeServiceLogRepository postcodeServiceLog, 
            CustomerMarketPlaceUpdatingHistoryRepository mpCustomerMarketPlaceUpdatingHistory, ICustomerRepository customerRepository)
        {
            _postcodeServiceLog = postcodeServiceLog;
            _mpCustomerMarketPlaceUpdatingHistory = mpCustomerMarketPlaceUpdatingHistory;
            _customerRepository = customerRepository;
            _eseriasLogRepository = ObjectFactory.GetInstance<ServiceLogRepository>();
            _pacnetLogRepository = ObjectFactory.GetInstance<PacnetPaypointServiceLogRepository>();
        }

        [Ajax]
        [HttpGet]
        [Transactional]
        [ValidateJsonAntiForgeryToken]
        public JsonNetResult Index(int id)
        {
            var customer = _customerRepository.Get(id);
            var models = new List<ApiChecksLogModel>();

            var postCode = _postcodeServiceLog.GetByCustomer(customer);
            var customerMarketPlaceHistory = _mpCustomerMarketPlaceUpdatingHistory
                                                .GetByCustomer(customer)
                                                .Select(c => new
                                                    {
                                                        history = c, 
                                                        marketplaceType = c.CustomerMarketPlace.Marketplace.Name, 
                                                        displayName = c.CustomerMarketPlace.DisplayName
                                                    });
            var eseriasLog = _eseriasLogRepository.GetBuCustomer(customer);
            var pacnetLog = _pacnetLogRepository.GetByCustomer(customer);

            models.AddRange(postCode.Select(val => new ApiChecksLogModel
                {
                    ApiType = "Postcode",
                    DateTime = val.InsertDate,
                    ErrorMessage = val.ErrorMessage,
                    Status = val.Status
                }));

            models.AddRange(customerMarketPlaceHistory.Select(val => new ApiChecksLogModel
            {
                ApiType = val.marketplaceType,
                DateTime = val.history.UpdatingStart,
                ErrorMessage = val.history.Error,
                Status = val.history.Error == null ? "Successful" : "Failed",
                Marketplace = val.displayName
            }));

            models.AddRange(eseriasLog.Select(val => new ApiChecksLogModel
            {
                ApiType = val.ServiceType,
                DateTime = val.InsertDate,
                ErrorMessage = "",
                Status = "Successful"
            }));

            models.AddRange(pacnetLog.Select(val => new ApiChecksLogModel
            {
                ApiType = val.RequestType,
                DateTime = val.InsertDate,
                ErrorMessage = val.ErrorMessage,
                Status = val.Status
            }));

            models = new List<ApiChecksLogModel>(models.OrderByDescending(x => x.DateTime));

            return this.JsonNet(models);
        }

    }
}
