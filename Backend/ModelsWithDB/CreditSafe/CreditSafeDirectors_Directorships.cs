using System;
using Ezbob.Utils;
using Ezbob.Utils.dbutils;

namespace Ezbob.Backend.ModelsWithDB.CreditSafe
{
    public class CreditSafeDirectors_Directorships
    {

        [PK(true)]
        [NonTraversable]
        public long CreditSafeDirectors_DirectorshipsID { get; set; }
        [FK("CreditSafeDirectors", "CreditSafeDirectorsID")]
        public long? CreditSafeDirectorsID { get; set; }
        [Length(100)]
        public string CompanyNumber { get; set; }
        [Length(100)]
        public string CompanyName { get; set; }
        [Length(100)]
        public string CompanyStatus { get; set; }
        [Length(100)]
        public string Function { get; set; }
        public DateTime? AppointedDate { get; set; }


    }
}
