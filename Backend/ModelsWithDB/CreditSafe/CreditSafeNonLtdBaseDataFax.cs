using Ezbob.Utils;
using Ezbob.Utils.dbutils;

namespace Ezbob.Backend.ModelsWithDB.CreditSafe
{
    public class CreditSafeNonLtdBaseDataFax
    {

        [PK(true)]
        [NonTraversable]
        public long CreditSafeNonLtdBaseDataFaxID { get; set; }
        [FK("CreditSafeNonLtdBaseData", "CreditSafeNonLtdBaseDataID")]
        public long? CreditSafeNonLtdBaseDataID { get; set; }
        [Length(20)]
        public string Fax { get; set; }
        public bool? FpsRegistered { get; set; }
        public bool? Main { get; set; }


    }
}
