using Ezbob.Utils;
using Ezbob.Utils.dbutils;

namespace Ezbob.Backend.ModelsWithDB.CreditSafe
{
    public class CreditSafeIndustries
    {

        [PK(true)]
        [NonTraversable]
        public long CreditSafeIndustriesID { get; set; }
        [FK("CreditSafeBaseData", "CreditSafeBaseDataID")]
        public long? CreditSafeBaseDataID { get; set; }
        [Length(500)]
        public string Name { get; set; }


    }
}
