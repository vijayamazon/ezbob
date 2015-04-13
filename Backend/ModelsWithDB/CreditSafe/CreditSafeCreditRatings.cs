using System;
using Ezbob.Utils;
using Ezbob.Utils.dbutils;

namespace Ezbob.Backend.ModelsWithDB.CreditSafe
{
    public class CreditSafeCreditRatings
    {

        [PK(true)]
        [NonTraversable]
        public long CreditSafeCreditRatingsID { get; set; }
        [FK("CreditSafeBaseData", "CreditSafeBaseDataID")]
        public long? CreditSafeBaseDataID { get; set; }
        public DateTime? Date { get; set; }
        public int? Score { get; set; }
        [Length(500)]
        public string Description { get; set; }


    }
}
