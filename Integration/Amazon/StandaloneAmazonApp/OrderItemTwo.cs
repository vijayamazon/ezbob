using System;
using EZBob.DatabaseLib.DatabaseWrapper.Order;

namespace StandaloneAmazonApp
{
    public class OrderItemTwo
    {
        public override string ToString()
        {

            return string.Format("NumberOfItemsShipped: {0}, PurchaseDate: {1}, Total: {2}, OrderStatus: {3}, AmazonOrderId: {4}", NumberOfItemsShipped, PurchaseDate, Total, OrderStatus, AmazonOrderId);
        }

        public static OrderItemTwo FromOrderItem(AmazonOrderItem item)
        {
            return new OrderItemTwo()
            {
                AmazonOrderId = item.OrderId,
                Total = item.ItemPrice,
                PurchaseDate = item.PurchaseDate,
                NumberOfItemsShipped = item.QuantityPurchased
            };
        }

        public static OrderItemTwo FromOrderItem2(AmazonOrderItem2 item)
        {
            return new OrderItemTwo()
                {
                    AmazonOrderId = item.AmazonOrderId,
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

        public string AmazonOrderId { get; set; }
    }
}