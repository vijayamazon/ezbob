using Ezbob.Utils;
using Ezbob.Utils.dbutils;

namespace Ezbob.Backend.ModelsWithDB.CreditSafe
{
    public class CreditSafeTradingAddresses
    {

        [PK(true)]
        [NonTraversable]
        public long CreditSafeTradingAddressesID { get; set; }
        [FK("CreditSafeBaseData", "CreditSafeBaseDataID")]
        public long? CreditSafeBaseDataID { get; set; }
        [Length(100)]
        public string Address1 { get; set; }
        [Length(100)]
        public string Address2 { get; set; }
        [Length(100)]
        public string Address3 { get; set; }
        [Length(100)]
        public string Address4 { get; set; }
        [Length(10)]
        public string PostCode { get; set; }
        [Length(20)]
        public string Telephone { get; set; }
        public bool? TpsRegistered { get; set; }


    }
}
