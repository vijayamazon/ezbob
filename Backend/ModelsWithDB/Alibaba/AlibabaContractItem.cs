namespace Ezbob.Backend.ModelsWithDB.Alibaba
{
    using Ezbob.Utils;
    using Ezbob.Utils.dbutils;

    public class AlibabaContractItem {
        [PK(true)]
        [NonTraversable]
        public int? ItemId { get; set; }
        [FK("MP_ServiceLog", "Id")]
        public int? ContractId { get; set; }
        public long? OrderProdNumber { get; set; }
        [Length(100)]
        public string ProductName { get; set; }
        [Length(100)]
        public string ProductSpecs { get; set; }
        public int? ProductQuantity { get; set; }
        public int? ProductUnit { get; set; }
        public int? ProductUnitPrice { get; set; }
        public int? ProductTotalAmount { get; set; }
	}
}
