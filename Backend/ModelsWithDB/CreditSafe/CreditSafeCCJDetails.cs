using System;
using Ezbob.Utils;
using Ezbob.Utils.dbutils;

namespace Ezbob.Backend.ModelsWithDB.CreditSafe
{
    public class CreditSafeCCJDetails
    {

        [PK(true)]
        [NonTraversable]
        public long CreditSafeCCJDetailsID { get; set; }
        [FK("CreditSafeBaseData", "CreditSafeBaseDataID")]
        public long? CreditSafeBaseDataID { get; set; }
        [Length(10)]
        public string CaseNr { get; set; }
        public DateTime? CcjDate { get; set; }
        [Length(50)]
        public string Court { get; set; }
        public DateTime? CcjDatePaid { get; set; }
        [Length(10)]
        public string CcjStatus { get; set; }
        public int? CcjAmount { get; set; }


    }
}
