using System;
using System.Collections.Generic;
using Ezbob.Utils;
using Ezbob.Utils.dbutils;

namespace Ezbob.Backend.ModelsWithDB.CreditSafe
{
    public class CreditSafeMortgages
    {
        public CreditSafeMortgages()
        {
            PersonEntitled = new List<CreditSafeMortgages_PersonEntitled>();
        }
        [PK(true)]
        [NonTraversable]
        public long CreditSafeMortgagesID { get; set; }
        [FK("CreditSafeBaseData", "CreditSafeBaseDataID")]
        public long? CreditSafeBaseDataID { get; set; }
        [Length(50)]
        public string MortgageType { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? RegisterDate { get; set; }
        public DateTime? SatisfiedDate { get; set; }
        [Length(20)]
        public string Status { get; set; }
        public int? AmountSecured { get; set; }
		[Length(LengthType.MAX)]
        public string Details { get; set; }
        [NonTraversable]
        public List<CreditSafeMortgages_PersonEntitled> PersonEntitled { get; set; }

    }
}
