namespace EzBobModels.EBay {
    using System.Collections.Generic;

    public class EbayOrderInfo {
        public EBayOrderItem OrderItem { get; set; }
        public EbayUserAddressData ShippingAddress { get; set; }
        public IEnumerable<EbayTransaction> Transactions { get; set; }
        public IEnumerable<EbayExternalTransaction> ExternalTransactions { get; set; }
    }
}
