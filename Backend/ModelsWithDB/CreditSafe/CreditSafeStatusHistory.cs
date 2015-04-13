using System;
using Ezbob.Utils;
using Ezbob.Utils.dbutils;

namespace Ezbob.Backend.ModelsWithDB.CreditSafe
{
    public class CreditSafeStatusHistory
    {

        [PK(true)]
        [NonTraversable]
        public long CreditSafeStatusHistoryID { get; set; }
        [FK("CreditSafeBaseData", "CreditSafeBaseDataID")]
        public long? CreditSafeBaseDataID { get; set; }
        public DateTime? date { get; set; }
        [Length(500)]
        public string text { get; set; }


    }
}
