using Ezbob.Utils;
using Ezbob.Utils.dbutils;

namespace Ezbob.Backend.ModelsWithDB.CreditSafe
{
    public class CreditSafeMortgages_PersonEntitled
    {

        [PK(true)]
        [NonTraversable]
        public long CreditSafeMortgages_PersonEntitledID { get; set; }
        [FK("CreditSafeMortgages", "CreditSafeMortgagesID")]
        public long? CreditSafeMortgagesID { get; set; }
        [Length(100)]
        public string Name { get; set; }


    }
}
