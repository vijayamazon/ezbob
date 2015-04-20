using System;
using Ezbob.Utils;
using Ezbob.Utils.dbutils;

namespace Ezbob.Backend.ModelsWithDB.CreditSafe
{
    public class CreditSafeEventHistory
    {

        [PK(true)]
        [NonTraversable]
        public long CreditSafeEventHistoryID { get; set; }
        [FK("CreditSafeBaseData", "CreditSafeBaseDataID")]
        public long? CreditSafeBaseDataID { get; set; }
        public DateTime? Date { get; set; }
        [Length(500)]
        public string Text { get; set; }


    }
}
