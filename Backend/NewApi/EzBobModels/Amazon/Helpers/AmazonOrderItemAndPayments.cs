using System.Collections.Generic;

namespace EzBobModels.Amazon.Helpers
{
    public class AmazonOrderItemAndPayments
    {
        public AmazonOrderItem OrderItem { get; set; }
        public IList<AmazonOrderItemPayment> Payments { get; set; }  
    }
}
