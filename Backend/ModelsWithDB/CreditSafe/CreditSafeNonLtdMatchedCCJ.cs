using System;
using Ezbob.Utils;
using Ezbob.Utils.dbutils;

namespace Ezbob.Backend.ModelsWithDB.CreditSafe
{
    public class CreditSafeNonLtdMatchedCCJ
    {

        [PK(true)]
        [NonTraversable]
        public long CreditSafeNonLtdMatchedCCJID { get; set; }
        [FK("CreditSafeNonLtdBaseData", "CreditSafeNonLtdBaseDataID")]
        public long? CreditSafeNonLtdBaseDataID { get; set; }
        [Length(10)]
        public string CaseNr { get; set; }
        public DateTime? CcjDate { get; set; }
        public DateTime? CcjDatePaid { get; set; }
        [Length(50)]
        public string Court { get; set; }
        [Length(10)]
        public string CcjStatus { get; set; }
        public int? CcjAmount { get; set; }
        [Length(100)]
        public string Against { get; set; }
        [Length(100)]
        public string Address { get; set; }


    }
}
