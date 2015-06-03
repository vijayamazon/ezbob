namespace Ezbob.Backend.ModelsWithDB.Alibaba
{
    using Ezbob.Utils;
    using Ezbob.Utils.dbutils;

    public class AlibabaBuyer {
        [PK(true)]
        [NonTraversable]
        public int Id { get; set; }
        public long AliId { get; set; }
        public decimal? Freeze { get; set; }
        [FK("Customer", "Id")]
        public int? CustomerId { get; set; }
        [FK("AlibabaContract", "ContractId")]
        public long? ContractId { get; set; }
        [Length(100)]
        public string BussinessName { get; set; }
        [Length(100)]
        public string street1 { get; set; }
        [Length(100)]
        public string street2 { get; set; }
        [Length(100)]
        public string City { get; set; }
        [Length(100)]
        public string State { get; set; }
        [Length(100)]
        public string Zip { get; set; }
        [Length(100)]
        public string Country { get; set; }
        [Length(100)]
        public string AuthRepFname { get; set; }
        [Length(100)]
        public string AuthRepLname { get; set; }
        [Length(100)]
        public string Phone { get; set; }
        [Length(100)]
        public string Fax { get; set; }
        [Length(100)]
        public string Email { get; set; }
        public int? OrderRequestCountLastYear { get; set; }
        public bool? ConfirmShippingDocAndAmount { get; set; }
        [Length(100)]
        public string FinancingType { get; set; }
        public bool? ConfirmReleaseFunds { get; set; }
	}
}
