using System;
using System.Collections.Generic;

namespace EzBobPersistence.ThirdParty.Amazon {
    using EzBobCommon;
    using EzBobModels.Amazon;
    using EzBobModels.Amazon.Helpers;

    public interface IAmazonOrdersQueries {
        IEnumerable<AmazonOrderItem> GetTopNOrderItems(int marketPlaceId, int topN = 10);
        Optional<DateTime> GetLastOrderDate(int marketPlaceId);
        bool SaveOrdersPayments(IEnumerable<AmazonOrderItemAndPayments> ordersPayments, int amazonOrderId);
        Optional<int> SaveOrder(AmazonOrder order);
        IAmazonCategoriesQueries Categories { get; set; }
        IAmazonOrderDetailsQueries OrderDetails { get; set; }
    }
}
