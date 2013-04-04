using EzBob.CommonLib;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper;
using EZBob.DatabaseLib.DatabaseWrapper.Order;
using EZBob.DatabaseLib.Model.Database;
using System;
using System.Collections.Generic;

namespace EKM
{
    public class EkmRetriveDataHelper : MarketplaceRetrieveDataHelperBase<EKMDatabaseFunctionType>
    {
        public EkmRetriveDataHelper(DatabaseDataHelper helper, DatabaseMarketplaceBase<EKMDatabaseFunctionType> marketplace)
            : base(helper, marketplace)
        {
            
        }

        protected override void InternalUpdateInfo(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
                                                   MP_CustomerMarketplaceUpdatingHistory historyRecord)
        {
           // var securityInfo = RetrieveCustomerSecurityInfo<EkmSecurityInfo>(databaseCustomerMarketPlace);
            
            //retreive data from ekm api
            //calculate agregated data
            //store orders

            

            UpdateClientOrdersInfo(databaseCustomerMarketPlace, /*securityInfo*/null, ActionAccessType.Full, historyRecord);

            
        }

        private void UpdateClientOrdersInfo(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, EkmSecurityInfo securityInfo, ActionAccessType actionAccessType, MP_CustomerMarketplaceUpdatingHistory historyRecord)
        {
            EKM.API.GetOrdersResponse res = new API.GetOrdersResponse();
            res.Body = new API.GetOrdersResponseBody();
            var order1 = new API.Order() {
                OrderDateISO="2013-04-03", 
                OrderDate = "2013-04-03",
                FirstName = "Stas", 
                LastName = "Du", 
                OrderNumber = "1", 
                OrderStatus = "Success",
                TotalCost = 12.4,
                CustomerID=2,
                CompanyName="dsf",
                EmailAddress="sdgf",
                OrderStatusColour = "fsdfs"
                
            };
            var order2 = new API.Order()
            {
                OrderDateISO = "2013-04-02",
                OrderDate = "2013-04-02",
                FirstName = "Yuly",
                LastName = "Sa",
                OrderNumber = "2",
                OrderStatus = "Success",
                TotalCost = 15.5,
                CustomerID = 2,
                CompanyName = "dsf",
                EmailAddress = "sdgf",
                OrderStatusColour = "fsdfs"
            };
            var orders = new[] { order1, order2};
            res.Body.GetOrdersResult = new API.OrdersObject() { Orders = orders, TotalOrders = 2 };
            var Iwant = new List<EkmOrderItem>();
            foreach(EKM.API.Order order in  res.Body.GetOrdersResult.Orders)
            {
                Iwant.Add(new EkmOrderItem()
                {
                    EkmOrderId= order.OrderID,
                    OrderNumber = order.OrderNumber,
                    CustomerID = order.CustomerID,
                    CompanyName = order.CompanyName,
                    FirstName = order.FirstName,
                    LastName = order.LastName,
                    EmailAddress = order.EmailAddress,
                    TotalCost = order.TotalCost,
                    OrderDate = order.OrderDate,
                    OrderStatus = order.OrderStatus,
                    OrderDateIso = order.OrderDateISO,
                    OrderStatusColour = order.OrderStatusColour,
                });
            }
            
            var elapsedTimeInfo = new ElapsedTimeInfo();
            EkmOrdersList list = new EkmOrdersList(DateTime.UtcNow,Iwant);
            
             
            

            ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
                                    ElapsedDataMemberType.StoreDataToDatabase,
                                    () => Helper.StoreEkmOrdersData(databaseCustomerMarketPlace, list, historyRecord));
            
        }

        protected override void AddAnalysisValues(IDatabaseCustomerMarketPlace marketPlace, AnalysisDataInfo data)
        {
            //store agregated

        }

        public override IMarketPlaceSecurityInfo RetrieveCustomerSecurityInfo(int customerMarketPlaceId)
        {
            return RetrieveCustomerSecurityInfo<EkmSecurityInfo>(GetDatabaseCustomerMarketPlace(customerMarketPlaceId));
        }
    }
}