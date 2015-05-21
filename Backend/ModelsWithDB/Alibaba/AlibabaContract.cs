namespace Ezbob.Backend.ModelsWithDB.Alibaba
{
    using System;
    using System.Collections.Generic;
    using Ezbob.Utils;
    using Ezbob.Utils.dbutils;

    public class AlibabaContract {
        public AlibabaContract() {
            Items = new List<AlibabaContractItem>();
        }
        [PK(true)]
        [NonTraversable]
		public long ContractId { get; set; }
        [Length(100)]
		public string RequestId { get; set; }
		public long? LoanId { get; set; }
        [Length(100)]
        public string OrderNumber { get; set; }
        [Length(100)]
        public string ShippingMark { get; set; }
        public int? TotalOrderAmount { get; set; }
        public int? DeviationQuantityAllowed { get; set; }
        [Length(100)]
        public string OrderAddtlDetails { get; set; }
        [Length(100)]
        public string ShippingTerms { get; set; }
        public DateTime? ShippingDate { get; set; }
        [Length(100)]
        public string LoadingPort { get; set; }
        [Length(100)]
        public string DestinationPort { get; set; }
        public int? TACoveredAmount { get; set; }
        public int? OrderDeposit { get; set; }
        public int? OrderBalance { get; set; }
        [Length(100)]
        public string OrderCurrency { get; set; }
        public byte?[] CommercialInvoice { get; set; }
        public byte?[] BillOfLading { get; set; }
        public byte?[] PackingList { get; set; }
        public byte?[] Other { get; set; }

        [NonTraversable]
        public List<AlibabaContractItem> Items { get; set; }
	}
}
