using System.Collections.Generic;
using System.Linq;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using EZBob.DatabaseLib.Repository;

namespace EzBob.Web.Areas.Underwriter.Models
{
    public class ApiCheckLogBuilder
    {
        private readonly PostcodeServiceLogRepository _postcodeServiceLog;
        private readonly CustomerMarketPlaceUpdatingHistoryRepository _mpCustomerMarketPlaceUpdatingHistory;
        private readonly ServiceLogRepository _serviceLogRepository;
        private readonly PacnetPaypointServiceLogRepository _pacnetPaypointServiceLogRepository;

        public ApiCheckLogBuilder(PostcodeServiceLogRepository postcodeServiceLog, CustomerMarketPlaceUpdatingHistoryRepository mpCustomerMarketPlaceUpdatingHistory, ServiceLogRepository serviceLogRepository, PacnetPaypointServiceLogRepository pacnetPaypointServiceLogRepository)
        {
            _postcodeServiceLog = postcodeServiceLog;
            _mpCustomerMarketPlaceUpdatingHistory = mpCustomerMarketPlaceUpdatingHistory;
            _serviceLogRepository = serviceLogRepository;
            _pacnetPaypointServiceLogRepository = pacnetPaypointServiceLogRepository;
        }

        public List<ApiChecksLogModel> Create(EZBob.DatabaseLib.Model.Database.Customer customer)
        {
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
            var eseriasLog = _serviceLogRepository.GetByCustomer(customer);
            var pacnetLog = _pacnetPaypointServiceLogRepository.GetByCustomerId(customer.Id);

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
                Status = string.IsNullOrEmpty(val.history.Error) ? "Successful" : "Failed",
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

            return models;
        }
    }
}