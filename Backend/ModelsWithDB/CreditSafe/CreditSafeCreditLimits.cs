using System;
using Ezbob.Utils;
using Ezbob.Utils.dbutils;

namespace Ezbob.Backend.ModelsWithDB.CreditSafe
{
    public class CreditSafeCreditLimits
    {

        [PK(true)]
        [NonTraversable]
        public long CreditSafeCreditLimitsID { get; set; }
        [FK("CreditSafeBaseData", "CreditSafeBaseDataID")]
        public long? CreditSafeBaseDataID { get; set; }
        public int? Limit { get; set; }
        public DateTime? Date { get; set; }


    }
}
