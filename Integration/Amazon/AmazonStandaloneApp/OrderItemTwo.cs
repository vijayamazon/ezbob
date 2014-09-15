using System;
using EZBob.DatabaseLib.DatabaseWrapper.Order;

namespace AmazonStandaloneApp
{
    public class OrderItemTwo
    {
        public override string ToString()
        {

            return string.Format("NumberOfItemsShipped: {0}, PurchaseDate: {1}, Total: {2}, OrderStatus: {3}, OrderId: {4}", NumberOfItemsShipped, PurchaseDate, Total, OrderStatus, OrderId);
        }

      public static OrderItemTwo FromOrderItem(AmazonOrderItem item)
        {
            return new OrderItemTwo()
                {
                    OrderId = item.OrderId,
                    OrderStatus = item.OrderStatus,
                    Total = item.OrderTotal.Value,
                    PurchaseDate = item.PurchaseDate,
                    NumberOfItemsShipped = item.NumberOfItemsShipped
                };
        }

        public int? NumberOfItemsShipped { get; set; }

        public DateTime? PurchaseDate { get; set; }

        public double Total { get; set; }

        public AmazonOrdersList2ItemStatusType OrderStatus { get; set; }

        public string OrderId { get; set; }
    }
}