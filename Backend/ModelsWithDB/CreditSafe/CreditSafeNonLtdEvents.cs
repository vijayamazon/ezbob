using System;
using Ezbob.Utils;
using Ezbob.Utils.dbutils;

namespace Ezbob.Backend.ModelsWithDB.CreditSafe
{
    public class CreditSafeNonLtdEvents
    {

        [PK(true)]
        [NonTraversable]
        public long CreditSafeNonLtdEventsID { get; set; }
        [FK("CreditSafeNonLtdBaseData", "CreditSafeNonLtdBaseDataID")]
        public long? CreditSafeNonLtdBaseDataID { get; set; }
        public DateTime? Date { get; set; }
        [Length(250)]
        public string Text { get; set; }


    }
}
