using Ezbob.Utils;
using Ezbob.Utils.dbutils;

namespace Ezbob.Backend.ModelsWithDB.CreditSafe
{
    public class CreditSafeBaseData_SecondarySicCodes
    {

        [PK(true)]
        [NonTraversable]
        public long CreditSafeBaseData_SecondarySicCodesID { get; set; }
        [FK("CreditSafeBaseData", "CreditSafeBaseDataID")]
        public long? CreditSafeBaseDataID { get; set; }
        [Length(10)]
        public string SicCode { get; set; }
        [Length(500)]
        public string SicDescription { get; set; }


    }
}
