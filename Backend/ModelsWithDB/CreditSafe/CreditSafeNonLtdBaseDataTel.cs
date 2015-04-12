using Ezbob.Utils;
using Ezbob.Utils.dbutils;

namespace Ezbob.Backend.ModelsWithDB.CreditSafe
{
    class CreditSafeNonLtdBaseDataTel
    {

        [PK(true)]
        [NonTraversable]
        public long CreditSafeNonLtdBaseDataTelID { get; set; }
        [FK("CreditSafeNonLtdBaseData", "CreditSafeNonLtdBaseDataID")]
        public long? CreditSafeNonLtdBaseDataID { get; set; }
        [Length(20)]
        public string Telephone { get; set; }
        public bool? TpsRegistered { get; set; }
        public bool? Main { get; set; }


    }
}
