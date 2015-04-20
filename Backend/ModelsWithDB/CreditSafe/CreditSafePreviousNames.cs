using System;
using Ezbob.Utils;
using Ezbob.Utils.dbutils;

namespace Ezbob.Backend.ModelsWithDB.CreditSafe
{
    public class CreditSafePreviousNames
    {

        [PK(true)]
        [NonTraversable]
        public long CreditSafePreviousNamesID { get; set; }
        [FK("CreditSafeBaseData", "CreditSafeBaseDataID")]
        public long? CreditSafeBaseDataID { get; set; }
        [Length(100)]
        public string Name { get; set; }
        public DateTime? Date { get; set; }


    }
}
