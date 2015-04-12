using System;
using Ezbob.Utils;
using Ezbob.Utils.dbutils;

namespace Ezbob.Backend.ModelsWithDB.CreditSafe
{
    class CreditSafeNonLtdRatings
    {

        [PK(true)]
        [NonTraversable]
        public long CreditSafeNonLtdRatingsID { get; set; }
        [FK("CreditSafeNonLtdBaseData", "CreditSafeNonLtdBaseDataID")]
        public long? CreditSafeNonLtdBaseDataID { get; set; }
        public DateTime? date { get; set; }
        public int? score { get; set; }
        [Length(100)]
        public string description { get; set; }


    }
}
