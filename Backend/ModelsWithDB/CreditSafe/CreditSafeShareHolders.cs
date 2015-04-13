using Ezbob.Utils;
using Ezbob.Utils.dbutils;

namespace Ezbob.Backend.ModelsWithDB.CreditSafe
{
    public class CreditSafeShareHolders
    {

        [PK(true)]
        [NonTraversable]
        public long CreditSafeShareHoldersID { get; set; }
        [FK("CreditSafeBaseData", "CreditSafeBaseDataID")]
        public long? CreditSafeBaseDataID { get; set; }
        [Length(100)]
        public string Name { get; set; }
        [Length(250)]
        public string Shares { get; set; }
        [Length(10)]
        public string Currency { get; set; }


    }
}
