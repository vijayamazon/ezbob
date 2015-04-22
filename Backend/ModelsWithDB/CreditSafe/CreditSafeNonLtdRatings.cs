using System;
using Ezbob.Utils;
using Ezbob.Utils.dbutils;

namespace Ezbob.Backend.ModelsWithDB.CreditSafe
{
    public class CreditSafeNonLtdRatings
    {

        [PK(true)]
        [NonTraversable]
        public long CreditSafeNonLtdRatingsID { get; set; }
        [FK("CreditSafeNonLtdBaseData", "CreditSafeNonLtdBaseDataID")]
        public long? CreditSafeNonLtdBaseDataID { get; set; }
        public DateTime? Date { get; set; }
        public int? Score { get; set; }
        [Length(100)]
        public string Description { get; set; }


    }
}
