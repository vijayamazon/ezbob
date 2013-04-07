using EKM.API;
using EzBob.CommonLib;
using EzBob.CommonLib.Security;
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
            EkmSecurityInfo securityInfo = (EkmSecurityInfo)this.RetrieveCustomerSecurityInfo(databaseCustomerMarketPlace.Id);

            //store orders
            UpdateClientOrdersInfo(databaseCustomerMarketPlace, securityInfo, ActionAccessType.Full, historyRecord);

            //calculate agregated data
            //TODO

        }

        private void UpdateClientOrdersInfo(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, EkmSecurityInfo securityInfo, ActionAccessType actionAccessType, MP_CustomerMarketplaceUpdatingHistory historyRecord)
        {
            //retreive data from ekm api
           var ordersList = EkmConnector.GetOrders(securityInfo.Name, securityInfo.Password);

            var Iwant = new List<EkmOrderItem>();
            foreach (EKM.API.Order order in ordersList)
            {
                Iwant.Add(new EkmOrderItem()
                {
                    EkmOrderId = order.OrderID,
                    OrderNumber = order.OrderNumber,
                    CustomerID = order.CustomerID,
                    CompanyName = order.CompanyName,
                    FirstName = order.FirstName,
                    LastName = order.LastName,
                    EmailAddress = order.EmailAddress,
                    TotalCost = order.TotalCost,
                    OrderDate = DateTime.Parse(order.OrderDate),
                    OrderStatus = order.OrderStatus,
                    OrderDateIso = DateTime.Parse(order.OrderDateISO),
                    OrderStatusColour = order.OrderStatusColour,
                });
            }

            var elapsedTimeInfo = new ElapsedTimeInfo();
            EkmOrdersList list = new EkmOrdersList(DateTime.UtcNow, Iwant);
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
            EkmSecurityInfo ekmSecurityInfo = new EkmSecurityInfo();
            IDatabaseCustomerMarketPlace customerMarketPlace = GetDatabaseCustomerMarketPlace(customerMarketPlaceId);
            ekmSecurityInfo.Password = Encryptor.Decrypt(customerMarketPlace.SecurityData);
            ekmSecurityInfo.Name = customerMarketPlace.DisplayName;
            ekmSecurityInfo.MarketplaceId = customerMarketPlace.Id;
            return ekmSecurityInfo;
        }
    }
}