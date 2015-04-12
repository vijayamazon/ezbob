using System;
using Ezbob.Utils;
using Ezbob.Utils.dbutils;

namespace Ezbob.Backend.ModelsWithDB.CreditSafe
{
    class CreditSafeNonLtdLimits
    {

        [PK(true)]
        [NonTraversable]
        public long CreditSafeNonLtdLimitsID { get; set; }
        [FK("CreditSafeNonLtdBaseData", "CreditSafeNonLtdBaseDataID")]
        public long? CreditSafeNonLtdBaseDataID { get; set; }
        public int? Limit { get; set; }
        public DateTime? Date { get; set; }


    }
}
